using ElectronicSignatureService.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Document = ElectronicSignatureService.Entities.Document;
using Signature = ElectronicSignatureService.Entities.Signature;

namespace ElectronicSignatureService.Controllers
{
    public class SignatureController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            AppInit.OnActionExecuting(this, context);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(string id)
        {
            Signature signature = Database.Signatures.Find(id)!;
            Document document = Database.Documents.Find(signature.DocumentID)!;
            ViewData["document"] = document;
            return View(signature);
        }

        public IActionResult Create(string documentId, int signatureSlot)
        {
            Document document = Database.Documents.Find(documentId)!;
            ViewData["slot"] = signatureSlot;

            HttpContext.Session.SetString("signatureDocumentId", documentId);
            HttpContext.Session.SetString("signatureSlot", signatureSlot.ToString());

            return View(document);
        }

        public IActionResult SelectMethod()
        {
            return PartialView();
        }

        public IActionResult LoginMethod()
        {
            return PartialView();
        }
        public IActionResult ConfirmLogin(string email, string password)
        {
            Account? account = Database.Accounts.Where(string.Format("EMail='{0}'", email)).FirstOrDefault();

            if (account == null || password.ToSHA256() != account.PasswordHash)
                return StatusCode(StatusCodes.Status401Unauthorized);

            HttpContext.Session.SetString("signatureMethod", "login");
            HttpContext.Session.SetString("signatureEmail", email);
            HttpContext.Session.SetString("signatureName", account.Name);
            HttpContext.Session.SetString("signatureCode", account.ID);

            return HandSign();
        }

        public IActionResult EMailMethod()
        {
            return PartialView();
        }
        public IActionResult ConfirmEMail(string email, string name)
        {
            string confirmationCode = "1234567890"; // TODO: random value
            
            HttpContext.Session.SetString("signatureMethod", "email");
            HttpContext.Session.SetString("signatureEmail", email);
            HttpContext.Session.SetString("signatureName", name);
            HttpContext.Session.SetString("signatureCode", confirmationCode);

            // TODO: send email with code...

            return PartialView();
        }
        public IActionResult SMSMethod()
        {
            return PartialView();
        }
        public IActionResult ExecSMSMethod()
        {
            return PartialView();
        }

        public IActionResult CertificateMethod()
        {
            return PartialView();
        }
        public IActionResult CompleteWithCertificate(string password, IFormFile certfile)
        {
            string documentID = HttpContext.Session.GetString("signatureDocumentId")!;
            int slot = int.Parse(HttpContext.Session.GetString("signatureSlot")!);

            string name = HttpContext.Session.GetString("signatureName")!;

            Document document = Database.Documents.Find(documentID)!;

            // certificate

            byte[] data = new byte[certfile.Length];
            Stream stream = certfile.OpenReadStream();
            int bytesRead = 0;
            while (bytesRead < data.Length)
                bytesRead += stream.Read(data, bytesRead, data.Length - bytesRead);

            X509Certificate2 cert = new X509Certificate2(data, password);

            // TODO: wieder einfügen:
            //if(!cert.Verify())
            //    return RedirectToAction("Create", "Signature", new
            //    {
            //        documentId = documentID,
            //        signatureSlot = slot,
            //        alert_error = "Certificate verification failed. Note that self-signed certificates are not allowed."
            //    });

            data = cert.Export(X509ContentType.Cert);


            ECDsa? key_ecdsa = cert.GetECDsaPrivateKey();
            DSA? key_dsa = cert.GetDSAPrivateKey();
            RSA? key_rsa = cert.GetRSAPrivateKey();

            byte[] dataToSign = Encoding.UTF8.GetBytes(document.HashCode);
            byte[] signatureData = new byte[0];

            if (key_rsa != null)
            {
                signatureData = key_rsa.SignData(dataToSign, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
            }
            else if (key_ecdsa != null)
            {
                signatureData = key_ecdsa.SignData(dataToSign, HashAlgorithmName.SHA512);
            }
            else if (key_dsa != null)
            {
                signatureData = key_dsa.SignData(dataToSign, HashAlgorithmName.SHA512);
            }
            else
                return RedirectToAction("Create", "Signature", new
                {
                    documentId = documentID,
                    signatureSlot = slot,
                    alert_error = "Certificate private key algorithm not supported."
                });

            Signature signature = new Signature();
            signature.DocumentID = documentID;
            signature.DocumentSlot = slot;
            signature.TimeStamp = DateTime.UtcNow;

            signature.Name = name;
            signature.IPAddress = HttpContext.Connection.RemoteIpAddress!.ToString();

            signature.SignatureData = Convert.ToBase64String(signatureData);
            signature.SignatureMethod = SignatureMethod.Certificate;

            signature.VerificationMethod = VerificationMethod.Certificate;
            signature.VerificationData = Convert.ToBase64String(data);

            signature.CalculateHash();

            DateTime timeStamp;
            signature.BlockchainMappingID = Blockchain.RegisterSignature(signature, out timeStamp);

            Database.Signatures.Add(signature);

            return RedirectToAction("Details", "Signature", new { id = signature.ID, alert_success = "Signature process complete." });
        }

        public IActionResult WalletMethod()
        {
            string documentID = HttpContext.Session.GetString("signatureDocumentId")!;
            Document document = Database.Documents.Find(documentID)!;

            CertWallet.SignatureRequestJson request = new CertWallet.SignatureRequestJson();
            request.Message = "Identify by certificate";
            request.EndpointUrl = Url.Action("SignatureResponse", "Certwallet", new { }, Request.Scheme)!;
            request.SessionId = HttpContext.Session.Id;
            request.DataToSign = Encoding.UTF8.GetBytes(document.HashCode);

            ViewData["signatureRequestUrl"] = CertWallet.CertWallet.SerializeToUrl(request);

            return PartialView();
        }
        public IActionResult CompleteWithWallet()
        {
            CertWallet.SignatureResponseJson signatureResponse = Controllers.CertwalletController.FetchResponse(this.HttpContext);
            if(signatureResponse == null)
                return RedirectToAction("Index", "Home", new { alert_error = "Failed interfacing certificates wallet." });

            string documentID = HttpContext.Session.GetString("signatureDocumentId")!;
            int slot = int.Parse(HttpContext.Session.GetString("signatureSlot")!);
            Document document = Database.Documents.Find(documentID)!;

            X509Certificate2 cert = new X509Certificate2(signatureResponse.Certificate!);

            // TODO: wieder einfügen:
            //if (!cert.Verify())
            //    return RedirectToAction("Create", "Signature", new
            //    {
            //        documentId = documentID,
            //        signatureSlot = slot,
            //        alert_error = "Certificate verification failed."
            //    });

            ECDsa? key_ecdsa = cert.GetECDsaPublicKey();
            DSA? key_dsa = cert.GetDSAPublicKey();
            RSA? key_rsa = cert.GetRSAPublicKey();

            byte[] dataToSign = Encoding.UTF8.GetBytes(document.HashCode);
            bool isValid = false;

            if (key_rsa != null)
            {
                isValid = key_rsa.VerifyData(dataToSign, signatureResponse.Signature!, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
            }
            else if (key_ecdsa != null)
            {
                isValid = key_ecdsa.VerifyData(dataToSign, signatureResponse.Signature!, HashAlgorithmName.SHA512);
            }
            else if (key_dsa != null)
            {
                isValid = key_dsa.VerifyData(dataToSign, signatureResponse.Signature!, HashAlgorithmName.SHA512);
            }
            
            if(!isValid)
                return RedirectToAction("Create", "Signature", new
                {
                    documentId = documentID,
                    signatureSlot = slot,
                    alert_error = "Signature invalid or algorithm not supported."
                });


            string name = HttpContext.Session.GetString("signatureName")!;

            Signature signature = new Signature();
            signature.DocumentID = documentID;
            signature.DocumentSlot = slot;
            signature.TimeStamp = DateTime.UtcNow;

            signature.Name = name;
            signature.IPAddress = HttpContext.Connection.RemoteIpAddress!.ToString();

            signature.SignatureData = Convert.ToBase64String(signatureResponse.Signature!);
            signature.SignatureMethod = SignatureMethod.Certificate;

            signature.VerificationMethod = VerificationMethod.Certificate;
            signature.VerificationData = Convert.ToBase64String(signatureResponse.Certificate!);

            signature.CalculateHash();

            DateTime timeStamp;
            signature.BlockchainMappingID = Blockchain.RegisterSignature(signature, out timeStamp);

            Database.Signatures.Add(signature);

            return RedirectToAction("Details", "Signature", new { id = signature.ID, alert_success = "Signature process complete." });
        }

        public IActionResult ConfirmCode(string confirmationcode)
        {
            string code = HttpContext.Session.GetString("signatureCode")!;

            if(confirmationcode != code)
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            return HandSign();
        }

        public IActionResult HandSign()
        {
            return PartialView("HandSign");
        }

        public IActionResult Complete(string handsign)
        {
            string documentID = HttpContext.Session.GetString("signatureDocumentId")!;
            int slot = int.Parse( HttpContext.Session.GetString("signatureSlot")!);
            string method = HttpContext.Session.GetString("signatureMethod")!;
            string? email = HttpContext.Session.GetString("signatureEmail");
            string? sms = HttpContext.Session.GetString("signatureSMS");
            string name = HttpContext.Session.GetString("signatureName")!;

            Signature signature = new Signature();
            signature.DocumentID = documentID;
            signature.DocumentSlot = slot;
            signature.TimeStamp = DateTime.UtcNow;

            signature.Name = name;
            signature.IPAddress = HttpContext.Connection.RemoteIpAddress!.ToString();

            signature.SignatureData = handsign;
            signature.SignatureMethod = SignatureMethod.Handwritten;

            if (method == "email")
            {
                signature.VerificationMethod = VerificationMethod.EMail;
                signature.VerificationData = email!;
            }
            else if (method == "sms")
            {
                signature.VerificationMethod = VerificationMethod.SMS;
                signature.VerificationData = sms!;
            }
            else if (method == "login")
            {
                signature.VerificationMethod = VerificationMethod.Login;
                signature.VerificationData = email!;
            }

            signature.CalculateHash();

            DateTime timeStamp;
            signature.BlockchainMappingID = Blockchain.RegisterSignature(signature, out timeStamp);
            //signature.TimeStamp = timeStamp;

            Database.Signatures.Add(signature);

            return RedirectToAction("Details", "Signature", new { id = signature.ID, alert_success = "Signature process complete." });
        }

        public  IActionResult DownloadJson(string id)
        {
            Signature signature = Database.Signatures.Find(id)!;

            return File(signature.ToJsonBytes(), "application/json", signature.ID + ".json");
        }

        public  IActionResult DownloadCertificate(string id)
        {
            Signature signature = Database.Signatures.Find(id)!;
            byte[] certfileData = Convert.FromBase64String(signature.VerificationData);

            return File(certfileData, "application/octet-stream", signature.ID + ".crt");
        }

        [HttpGet]
        public IActionResult Validate()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Validate(string id, string documentID, string documentSlot, string timeStamp, string signatureData, string verificationData)
        {
            DateTime timeStampObj = DateTime.Parse(timeStamp, CultureInfo.InvariantCulture);

            string hash = string.Format("{0};{1};{2};{3};{4}",
                documentID.Trim(),
                documentSlot.Trim().Trim('#'),
                timeStampObj.ToString("s", CultureInfo.InvariantCulture),
                signatureData.Trim(),
                verificationData.Trim()).ToSHA256_Base64();

            string originalTimeStamp;
            string originalHash = Blockchain.GetSignatureHash(id, out originalTimeStamp);
            bool isValid = hash == originalHash;

            ViewData["id"] = id;
            ViewData["documentID"] = documentID;
            ViewData["documentSlot"] = documentSlot;
            ViewData["timeStamp"] = timeStamp;
            ViewData["signatureData"] = signatureData;
            ViewData["verificationData"] = verificationData;
            ViewData["hash"] = hash;
            ViewData["originalHash"] = originalHash;
            ViewData["originalTimeStamp"] = originalTimeStamp;
            ViewData["isValid"] = isValid;

            return View("ValidationResult");
        }
    }
}

using ElectronicSignatureService.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Net.Codecrete.QrCodeGenerator;
using System.Text;
using Document = ElectronicSignatureService.Entities.Document;
using Signature = ElectronicSignatureService.Entities.Signature;

namespace ElectronicSignatureService.Controllers
{
    public class DocumentController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            AppInit.OnActionExecuting(this, context);
        }

        [LoginRequired]
        public IActionResult Index()
        {
            string sql = string.Format("SELECT * FROM [Document] WHERE AccountID='{0}' ORDER BY [TimeStamp] DESC LIMIT 10", HttpContext.GetAccountID());
            List<Document> documents = Database.QueryObjectsSQL<Document>(sql);
            return View(documents);
        }

        [LoginRequired]
        public IActionResult List()
        {
            List<Document> documents = Database.Documents.Where(string.Format("AccountID='{0}'", HttpContext.GetAccountID()));
            return View(documents);
        }

        public IActionResult Details(string id)
        {
            Document document = Database.Documents.Find(id)!;
            if(string.IsNullOrEmpty(document.HashCode))
                return RedirectToAction("ObtainQR", "Document", new { id = id });
            return View(document);
        }
        public IActionResult Signature(string id, int slot = 1)
        {
            Signature? signature = Database.Signatures.Where(string.Format("DocumentID='{0}' AND DocumentSlot={1}", id, slot)).FirstOrDefault();

            if (signature == null)
            {
                return RedirectToAction("Create", "Signature", new { documentId = id, signatureSlot = slot });
            }
            else
            {
                return RedirectToAction("Details", "Signature", new { id = signature.ID });
            }
        }

        [LoginRequired]
        public IActionResult Create()
        {
            return View();
        }

        [LoginRequired]
        public IActionResult RegisterDocument(string name, int signatures)
        {
            Document document = new Document();
            document.BlockchainMappingID = Blockchain.RegisterDocument(document);
            document.Name = name;
            document.SignaturePlaceholderCount = signatures;
            document.AccountID = HttpContext.GetAccountID()!;

            Database.Documents.Add(document);

            return RedirectToAction("ObtainQR", "Document", new { id = document.ID });
        }

        [LoginRequired]
        public IActionResult ObtainQR(string id)
        {
            Document document = Database.Documents.Find(id)!;

            List<string> urls = new List<string>();
            for (int i = 1; i <= document.SignaturePlaceholderCount; i++)
            {
                string url = Url.Action("Signature", "Document", new { id = document.ID, slot = i }, Request.Scheme)!;
                urls.Add(url);
            }

            ViewData["urls"] = urls;

            return View(document);
        }

        [LoginRequired]
        public IActionResult RegisterDocumentHash(string id, string filename, string hash)
        {
            Document document = Database.Documents.Find(id)!;
            DateTime timeStamp;
            if (!Blockchain.RegisterDocumentHash(document.BlockchainMappingID, hash, out timeStamp))
                return this.StatusCode(StatusCodes.Status400BadRequest);
            
            document.Filename = filename;
            document.HashCode = hash;
            document.TimeStamp = timeStamp;

            Database.Documents.InsertOrUpdate(document);

            return Json(document);
        }

    }
}

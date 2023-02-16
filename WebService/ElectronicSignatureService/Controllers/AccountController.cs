using ElectronicSignatureService.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Document = ElectronicSignatureService.Entities.Document;
using Signature = ElectronicSignatureService.Entities.Signature;

namespace ElectronicSignatureService.Controllers
{
    public class AccountController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            ViewData["alert_success"] = Request.Query["alert_success"].FirstOrDefault();
            ViewData["alert_error"] = Request.Query["alert_error"].FirstOrDefault();
            ViewData["alert_info"] = Request.Query["alert_info"].FirstOrDefault();
        }

        public IActionResult Index()
        {
            return RedirectToAction("Register", "Account");
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(string email, string password, string name)
        {
            string confirmationCode = "1234567890"; // TODO: random value

            HttpContext.Session.SetString("registrationEmail", email);
            HttpContext.Session.SetString("registrationName", name);
            HttpContext.Session.SetString("registrationPasswordHash", password.ToSHA256());
            HttpContext.Session.SetString("registrationCode", confirmationCode);

            return RedirectToAction("ConfirmEmail", "Account");
        }
        [HttpGet]
        public IActionResult ConfirmEmail()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ConfirmEmail(string confirmationcode)
        {
            string confirmationCode = HttpContext.Session.GetString("registrationCode")!;

            if (confirmationCode != confirmationcode)
                return RedirectToAction("ConfirmEmail", "Account", new { alert_error = "Invalid confirmation code." });

            Account account = new Account();
            account.EMail = HttpContext.Session.GetString("registrationEmail")!;
            account.Name = HttpContext.Session.GetString("registrationName")!;
            account.PasswordHash = HttpContext.Session.GetString("registrationPasswordHash")!;

            Database.Insert(account);

            HttpContext.Login(account.ID);

            return RedirectToAction("Index", "Home");
        }



        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            Account? account = Database.Accounts.Where(string.Format("EMail='{0}'", email)).FirstOrDefault();

            if (account == null || password.ToSHA256() != account.PasswordHash)
                return RedirectToAction("Login", "Account", new { alert_error = "Invalid email or password." });

            HttpContext.Login(account.ID);

            return RedirectToAction("Index", "Document", new { alert_success = string.Format("Hello {0}.", account.Name) });
        }


        public IActionResult Logout()
        {
            HttpContext.Logout();
            return RedirectToAction("Index", "Home", new { alert_success = "Good bye." });
        }
    }
}

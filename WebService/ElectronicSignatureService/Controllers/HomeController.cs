using ElectronicSignatureService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Net.Codecrete.QrCodeGenerator;
using System;
using System.Diagnostics;
using System.Text;
using Document = ElectronicSignatureService.Entities.Document;
using Signature = ElectronicSignatureService.Entities.Signature;

namespace ElectronicSignatureService.Controllers
{
    public class HomeController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            ViewData["alert_success"] = Request.Query["alert_success"].FirstOrDefault();
            ViewData["alert_error"] = Request.Query["alert_error"].FirstOrDefault();
            ViewData["alert_info"] = Request.Query["alert_info"].FirstOrDefault();
        }

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public IActionResult QR(string target)
        {
            byte[] data = Encoding.UTF8.GetBytes(QrCode.EncodeText(target, QrCode.Ecc.Low).ToSvgString(4, "#c3124c", "#ffffff"));
            return File(data, "image/svg+xml", "qr.svg");
        }

        public IActionResult Test()
        {
            return View();
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Globalization;

namespace ElectronicSignatureService
{
    public static class AppInit
    {
        public static string AppDataPath { get; private set; } = string.Empty;
        public static string ProductName { get; private set; } = "LIONS.sign";

        public static void Init(IWebHostEnvironment environment)
        {
            AppDataPath = Path.Combine(environment.ContentRootPath, "App_Data");
            if (!Directory.Exists(AppDataPath))
                Directory.CreateDirectory(AppDataPath);

            AppConfig.Init();
            Database.Init();
            Blockchain.Init();
        }


        public static void OnActionExecuting(Controller controller, ActionExecutingContext context)
        {
            // handle optional alert parameters
            controller.ViewData["alert_success"] = controller.Request.Query["alert_success"].FirstOrDefault();
            controller.ViewData["alert_error"] = controller.Request.Query["alert_error"].FirstOrDefault();
            controller.ViewData["alert_info"] = controller.Request.Query["alert_info"].FirstOrDefault();

            // set culture from session / browser settings

            CultureInfo culture = CultureInfo.InvariantCulture;
            string? sessionCulture = controller.HttpContext.Session.GetString("CultureInfo");

            if (!string.IsNullOrEmpty(sessionCulture))
            {
                culture = new CultureInfo(sessionCulture);
            }
            else
            {
                string? lang = controller.Request.Headers["Accept-Language"];
                if (!string.IsNullOrEmpty(lang))
                {
                    var firstLang = lang.Split(',').FirstOrDefault();
                    if (!string.IsNullOrEmpty(firstLang))
                        culture = new CultureInfo(firstLang);
                }


                controller.HttpContext.Session.SetString("CultureInfo", culture.TwoLetterISOLanguageName);
            }

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }
    }
}

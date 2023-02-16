using ElectronicSignatureService.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ElectronicSignatureService
{
    public static class Extensions
    {
        public static bool IsLoggedIn(this HttpContext context)
        {
            return !string.IsNullOrEmpty(context.Session.GetString("sessionUser"));
        }
        public static void Login(this HttpContext context, string userID)
        {
            context.Session.SetString("sessionUser", userID);
        }
        public static void Logout(this HttpContext context)
        {
            context.Session.Remove("sessionUser");
        }
        public static Account? GetAccount(this HttpContext context)
        {
            string? id = context.Session.GetString("sessionUser");
            if(string.IsNullOrEmpty(id))
                return null;

            return Database.Accounts.Find(id);
        }
        public static string? GetAccountID(this HttpContext context)
        {
            return context.Session.GetString("sessionUser");
        }
    }

    public class LoginRequiredAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string? account = filterContext.HttpContext.GetAccountID();
            if (account == null)
                filterContext.Result = new RedirectResult("/Account/Login");
        }
    }
}

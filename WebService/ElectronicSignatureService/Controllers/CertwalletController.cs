using ElectronicSignatureService.CertWallet;
using Microsoft.AspNetCore.Mvc;
using Document = ElectronicSignatureService.Entities.Document;
using Signature = ElectronicSignatureService.Entities.Signature;

namespace ElectronicSignatureService.Controllers
{
    public class CertwalletController : Controller
    {
        private static Dictionary<string, SignatureResponseJson> _responses = new Dictionary<string, SignatureResponseJson>();
        public static SignatureResponseJson FetchResponse(HttpContext context)
        {
            string sessionId = context.Session.Id;
            lock (_responses)
            {
                SignatureResponseJson result = _responses[sessionId];
                _responses.Remove(sessionId);
                return result;
            }
        }

        public IActionResult State()
        {
            string sessionId = HttpContext.Session.Id;
            bool result = false;
            lock(_responses)
            {
                result = _responses.ContainsKey(sessionId);
            }
            return Json(new { received = result });
        }

        [HttpPost]
        public IActionResult SignatureResponse([FromBody] SignatureResponseJson response)
        {
            if(response != null && !string.IsNullOrEmpty(response.SessionId))
            {
                lock (_responses)
                {
                    _responses.Add(response.SessionId, response);
                }
            }
            return Json(new { received = true });
        }
    }
}

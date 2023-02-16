using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CertificatesWallet
{
    public static class JsonObjects
    {
        public static SignatureRequestJson DeserializeRequest(string url)
        {
            // certwallet://xmeroifhmixerfhrfchefrirmhxiefhxneri
            string urlContent = url.Split(new char[] { '/', ':' }, StringSplitOptions.RemoveEmptyEntries).Last();

            string json = Encoding.UTF8.GetString(Convert.FromBase64String(Uri.UnescapeDataString(urlContent)));

            return JsonSerializer.Deserialize<SignatureRequestJson>(json, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
        }

        public static string SerializeResponse(SignatureResponseJson response)
        {
            return JsonSerializer.Serialize<SignatureResponseJson>(response, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
        }
    }


    public class SignatureRequestJson
    {
        public string Message { get; set; }
        public string SessionId { get; set; }
        public byte[] DataToSign { get; set; }
        public string EndpointUrl { get; set; }
    }

    public class SignatureResponseJson
    {
        public string SessionId { get; set; }
        public byte[] Certificate { get; set; }
        public byte[] Signature { get; set; }
    }
}

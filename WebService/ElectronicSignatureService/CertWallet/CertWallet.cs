using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace ElectronicSignatureService.CertWallet
{
    public static class CertWallet
    {
        public static string SerializeToUrl(SignatureRequestJson request)
        {
            string json = JsonSerializer.Serialize<SignatureRequestJson>(request, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
            string urlContent = Uri.EscapeDataString(Convert.ToBase64String(Encoding.UTF8.GetBytes(json)));

            return "certwallet://" + urlContent;
        }

        public static SignatureResponseJson DeserializeResponse(string json)
        {
            return JsonSerializer.Deserialize<SignatureResponseJson>(json, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            })!;
        }

    }

    public class SignatureRequestJson
    {
        public string Message { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public byte[]? DataToSign { get; set; } = null;
        public string EndpointUrl { get; set; } = string.Empty;
    }

    public class SignatureResponseJson
    {
        public string SessionId { get; set; } = string.Empty;
        public byte[]? Certificate { get; set; } = null;
        public byte[]? Signature { get; set; } = null;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace CertificatesWallet
{
    public class CJWSHeaderInfo
    {
        /// <summary>
        /// The type of the document (should always be 'jwd').
        /// </summary>
        [JsonPropertyName("typ")]
        public string Type { get; set; } = "cjws1";

        /// <summary>
        /// The content type of the document (json for serialized objects as payload, binary for anything else).
        /// </summary>
        [JsonPropertyName("cty")]
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// The display name of the document.
        /// </summary>
        [JsonPropertyName("dsp")]
        public string DisplayText { get; set; } = string.Empty;


        public static byte[] DecodeUrlBase64(string data)
        {
            data = data.Replace('_', '/').Replace('-', '+');
            int trail = 4 - data.Length % 4;
            if (trail > 0 && trail < 4)
                data += new string('=', trail);
            return Convert.FromBase64String(data.Replace('_', '/').Replace('-', '+'));
        }

        /// <summary>
        /// Deserializes a URL-encoded base64 string to a CJWS2Header object.
        /// </summary>
        public static CJWSHeaderInfo FromString(string headerString)
        {
            byte[] data = DecodeUrlBase64(headerString);
            string json = Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<CJWSHeaderInfo>(json, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
            })!;
        }
    }
}

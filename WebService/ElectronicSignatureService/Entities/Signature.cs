using System.Globalization;
using System.Text.Json;

namespace ElectronicSignatureService.Entities
{
    public class Signature : DatabaseEntity
    {
        [DatabaseProperty]
        public string BlockchainMappingID { get; set; } = string.Empty;
        [DatabaseProperty]
        public string AccountID { get; set; } = string.Empty;
        [DatabaseProperty]
        public string DocumentID { get; set; } = string.Empty;
        [DatabaseProperty]
        public int DocumentSlot { get; set; } = 0;
        [DatabaseProperty]
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        [DatabaseProperty]
        public string Name { get; set; } = string.Empty;
        [DatabaseProperty]
        public string IPAddress { get; set; } = string.Empty;

        [DatabaseProperty]
        public string SignatureData { get; set; } = string.Empty;
        [DatabaseProperty]
        public SignatureMethod SignatureMethod { get; set; }

        [DatabaseProperty]
        public string VerificationData { get; set; } = string.Empty;
        [DatabaseProperty]
        public VerificationMethod VerificationMethod { get; set; }

        [DatabaseProperty]
        public string HashCode { get; set; } = string.Empty;

        public void CalculateHash()
        {
            this.HashCode = string.Format("{0};{1};{2};{3};{4}",
                this.DocumentID,
                this.DocumentSlot,
                this.TimeStamp.ToString("s", CultureInfo.InvariantCulture), 
                this.SignatureData, 
                this.VerificationData).ToSHA256();
        }

        public string ToJsonString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
        }
        public byte[] ToJsonBytes()
        {
            return System.Text.Encoding.UTF8.GetBytes(this.ToJsonString());
        }

        public static Signature FromJson(byte[] data)
        {
            return FromJson(System.Text.Encoding.UTF8.GetString(data));
        }
        public static Signature FromJson(string json)
        {
            return JsonSerializer.Deserialize<Signature>(json, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            })!;
        }
    }

    public enum VerificationMethod
    {
        None,
        EMail,
        SMS,
        Login,
        Certificate,
        VerifiableCredential
    }
    public enum SignatureMethod
    {
        Handwritten,
        Certificate
    }

}

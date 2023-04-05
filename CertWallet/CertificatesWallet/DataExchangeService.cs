using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CertificatesWallet
{
    public class DataExchangeService
    {
        public string BaseURL { get; set; }
        public string ApiKey { get; set; }

        private static JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            
        };

        public DataExchangeService(string baseUrl, string apiKey)
        {
            this.BaseURL = baseUrl;
            this.ApiKey = apiKey;
        }

        public DataExchangeResult RegisterMessage()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                return httpClient.GetAsync(string.Format("{0}/Register?key={1}", this.BaseURL, this.ApiKey))
                    .Result.Content.ReadFromJsonAsync<DataExchangeResult>(serializerOptions).Result!;
            }
        }

        public DataExchangeResult UploadMessage(byte[] bytes)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                ByteArrayContent content = new ByteArrayContent(bytes);

                return httpClient.PostAsync(string.Format("{0}/Upload?key={1}", this.BaseURL, this.ApiKey), content)
                    .Result.Content.ReadFromJsonAsync<DataExchangeResult>(serializerOptions).Result!;
            }
        }

        public static DataExchangeResult UploadMessage(string uploadUrl, byte[] bytes)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                ByteArrayContent content = new ByteArrayContent(bytes);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                return httpClient.PostAsync(uploadUrl, content)
                    .Result.Content.ReadFromJsonAsync<DataExchangeResult>(serializerOptions).Result!;
            }
        }
        public static async Task<DataExchangeResult> UploadMessageAsync(string uploadUrl, byte[] bytes)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                ByteArrayContent content = new ByteArrayContent(bytes);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                HttpResponseMessage response = await httpClient.PostAsync(uploadUrl, content);
                DataExchangeResult result = await response.Content.ReadFromJsonAsync<DataExchangeResult>();
                return result;
            }
        }

        public static byte[] RetrieveMessage(string retrieveUrl)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                return httpClient.GetAsync(retrieveUrl)
                    .Result.Content.ReadAsByteArrayAsync().Result;
            }
        }
    }

    public class DataExchangeResult
    {
        public string RetrieveUrl { get; set; } = string.Empty;
        public string UploadUrl { get; set; } = string.Empty;
    }
}

using ElectronicSignatureService.Entities;
using Nethereum.Web3;
using Account = Nethereum.Web3.Accounts.Account;
using Signature = ElectronicSignatureService.Entities.Signature;
using Document = ElectronicSignatureService.Entities.Document;
using System.Globalization;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System.Numerics;
using System.Security.Policy;
using Org.BouncyCastle.Asn1.Tsp;

namespace ElectronicSignatureService
{
    public class Blockchain
    {
        // TODO: set false!
        public static bool IsMockedUp {  get; set; } = false;


        [Function("createDocument", "uint64")]
        public class CreateDocumentFunction : FunctionMessage
        {
        }

        [Function("updateDocument", "uint")]
        public class UpdateDocumentFunction : FunctionMessage
        {
            [Parameter("uint64", "_id", 1)]
            public ulong ID { get; set; }
            [Parameter("string", "_hashCode", 2)]
            public string HashCode { get; set; } = string.Empty;
        }

        [Function("requestDocument", typeof(HashOutputDTO))]
        public class RequestDocumentFunction : FunctionMessage
        {
            [Parameter("uint64", "_id", 1)]
            public ulong ID { get; set; }
        }

        [Function("createSignature", typeof(CreateSignatureOutputDTO))]
        public class CreateSignatureFunction : FunctionMessage
        {
            [Parameter("string", "_hashCode", 1)]
            public string HashCode { get; set; } = string.Empty;
        }

        [Function("requestSignature", typeof(HashOutputDTO))]
        public class RequestSignatureFunction : FunctionMessage
        {
            [Parameter("uint64", "_id", 1)]
            public ulong ID { get; set; }
        }

        [FunctionOutput]
        public class CreateSignatureOutputDTO : IFunctionOutputDTO
        {
            [Parameter("uint64", "id", 1)]
            public ulong ID { get; set; }
            [Parameter("uint", "timestamp", 2)]
            public long TimeStamp { get; set; }
        }

        [FunctionOutput]
        public class HashOutputDTO : IFunctionOutputDTO
        {
            [Parameter("string", "hashCode", 1)]
            public string HashCode { get; set; } = string.Empty;
            [Parameter("uint", "timestamp", 2)]
            public long TimeStamp { get; set; }
        }

        [Event("DocumentCreated")]
        public class DocumentCreatedEventDTO : IEventDTO
        {
            [Parameter("uint64", "_id", 1)]
            public ulong ID { get; set; }
        }
        [Event("DocumentUpdated")]
        public class DocumentUpdatedEventDTO : IEventDTO
        {
            [Parameter("uint64", "_id", 1)]
            public ulong ID { get; set; }

            [Parameter("uint", "_timestamp", 2)]
            public uint TimeStamp { get; set; }
        }
        [Event("SignatureCreated")]
        public class SignatureCreatedEventDTO : IEventDTO
        {
            [Parameter("uint64", "_id", 1)]
            public ulong ID { get; set; }

            [Parameter("uint", "_timestamp", 2)]
            public uint TimeStamp { get; set; }
        }


        private static Account? account = null;
        private static Web3? web3 = null;
        private static string contractAddress = string.Empty;

        public static void Init()
        {
            if (IsMockedUp)
                return;
            //var url = "http://testchain.nethereum.com:8545";
            //var privateKey = "0x7580e7fb49df1c861f0050fae31c2224c6aba908e116b8da44ee8cd927b990b0";

            try
            {
                var url = AppConfig.Current.EthereumUrl;
                var privateKey = AppConfig.Current.EthereumPrivateKey;
                contractAddress = AppConfig.Current.EthereumContractAddress;
                account = new Account(privateKey);
                web3 = new Web3(account, url);

                // TODO: check if still required...
                web3.TransactionManager.UseLegacyAsDefault = true;

            }
            catch (Exception ex)
            {
                throw new Exception("Filed to initalize Nethereum.", ex);
            }
        }

        public static string RegisterDocument(Document document)
        {
            if (IsMockedUp)
                return (DateTime.Now - DateTime.Now.Date).Ticks.ToString();
            if (web3 == null)
                throw new Exception("Ethereum Interface not initialized.");

            // nethereum

            CreateDocumentFunction input = new CreateDocumentFunction()
            {
            };
            //var handler = web3.Eth.GetContractQueryHandler<CreateDocumentFunction>();
            //BigInteger result = handler.QueryAsync<BigInteger>(contractAddress, input).Result;


            var handler = web3.Eth.GetContractTransactionHandler<CreateDocumentFunction>();
            var transactionReceipt = handler.SendRequestAndWaitForReceiptAsync(contractAddress, input).Result;
            DocumentCreatedEventDTO result = transactionReceipt.DecodeAllEvents<DocumentCreatedEventDTO>().First().Event;

            return result.ID.ToString();
        }
        public static bool RegisterDocumentHash(string id, string hash, out DateTime timeStamp)
        {
            timeStamp = DateTime.Now;
            if (IsMockedUp)
                return true;
            if (web3 == null)
                throw new Exception("Ethereum Interface not initialized.");

            // nethereum

            UpdateDocumentFunction input = new UpdateDocumentFunction()
            {
                ID = ulong.Parse(id),
                HashCode = hash
            };
            //var handler = web3.Eth.GetContractQueryHandler<UpdateDocumentFunction>();
            //uint timestampsec = handler.QueryAsync<uint>(contractAddress, input).Result;

            var handler = web3.Eth.GetContractTransactionHandler<UpdateDocumentFunction>();
            var transactionReceipt = handler.SendRequestAndWaitForReceiptAsync(contractAddress, input).Result;
            DocumentUpdatedEventDTO result = transactionReceipt.DecodeAllEvents<DocumentUpdatedEventDTO>().First().Event;


            timeStamp = new DateTime(1970, 1, 1).AddSeconds(result.TimeStamp);

            return result.TimeStamp > 0;
        }

        public static string RegisterSignature(Signature signature, out DateTime timeStamp)
        {
            timeStamp = DateTime.Now;
            if (IsMockedUp)
                return (DateTime.Now - DateTime.Now.Date).Ticks.ToString();
            if (web3 == null)
                throw new Exception("Ethereum Interface not initialized.");

            // nethereum

            CreateSignatureFunction input = new CreateSignatureFunction()
            {
                HashCode = signature.HashCode
            };
            //var handler = web3.Eth.GetContractQueryHandler<CreateSignatureFunction>();
            //CreateSignatureOutputDTO result = handler.QueryAsync<CreateSignatureOutputDTO>(contractAddress, input).Result;

            var handler = web3.Eth.GetContractTransactionHandler<CreateSignatureFunction>();
            var transactionReceipt = handler.SendRequestAndWaitForReceiptAsync(contractAddress, input).Result;
            SignatureCreatedEventDTO result = transactionReceipt.DecodeAllEvents<SignatureCreatedEventDTO>().First().Event;

            timeStamp = new DateTime(1970, 1, 1).AddSeconds(result.TimeStamp);

            return result.ID.ToString();
        }
        public static string GetDocumentHash(string id, out string timeStamp)
        {
            if (IsMockedUp)
            {
                Document document = Database.Documents.Find(id)!;
                timeStamp = document.TimeStamp.ToString("s", CultureInfo.InvariantCulture);
                return document.HashCode;
            }
            if (web3 == null)
                throw new Exception("Ethereum Interface not initialized.");

            // nethereum

            RequestDocumentFunction input = new RequestDocumentFunction()
            {
                ID = ulong.Parse(id)
            };
            var handler = web3.Eth.GetContractQueryHandler<RequestDocumentFunction>();
            HashOutputDTO result = handler.QueryAsync<HashOutputDTO>(contractAddress, input).Result;

            timeStamp = new DateTime(1970, 1, 1).AddSeconds(result.TimeStamp).ToString("s", CultureInfo.InvariantCulture);
            
            return result.HashCode;
        }
        public static string GetSignatureHash(string id, out string timeStamp)
        {
            if (IsMockedUp)
            {
                Signature signature = Database.Signatures.Find(id)!;
                timeStamp = signature.TimeStamp.ToString("s", CultureInfo.InvariantCulture);
                return signature.HashCode;
            }
            if (web3 == null)
                throw new Exception("Ethereum Interface not initialized.");

            // nethereum

            RequestSignatureFunction input = new RequestSignatureFunction()
            {
                ID = ulong.Parse(id)
            };
            var handler = web3.Eth.GetContractQueryHandler<RequestSignatureFunction>();
            HashOutputDTO result = handler.QueryAsync<HashOutputDTO>(contractAddress, input).Result;

            timeStamp = new DateTime(1970, 1, 1).AddSeconds(result.TimeStamp).ToString("s", CultureInfo.InvariantCulture);

            return result.HashCode;
        }
    }

}

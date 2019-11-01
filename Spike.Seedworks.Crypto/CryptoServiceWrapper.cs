
using System.IO;
using CryptographyService.Contracts;
using Spike.Seedworks.CryptoService.Properties;
using Spike.CryptographyService.Contracts;
using Spike.CryptographyService.Models.DigitalSigning;
using Spike.CryptographyService.Simulator;
using Newtonsoft.Json;

namespace Spike.Seedworks.CryptoService
{
    public class CryptoServiceWrapper<T> 
    {
        private readonly ICryptographyService cryptoService;
        private readonly ICryptographyService simulatedService;

        public ICryptographyService GetCurrentCryptoService => Settings.Default.UseSimulator ? simulatedService : cryptoService;


        public CryptoServiceWrapper(ICryptographyService service, ICryptographyService sim)
        {
            cryptoService = service;
            simulatedService = new CryptographySimulatedService();

        }

        public string Encrypt(T content, string salt)
        {
           var serializedContent = JsonConvert.SerializeObject(content);

           return cryptoService.Encrypt(serializedContent, salt);
        }

        public T Decrypt(string encryptedContent, string salt)
        {
            var decryptedContent = cryptoService.Decrypt(encryptedContent, salt);
            var content = JsonConvert.DeserializeObject<T>(decryptedContent);

            return content;
        }

        public SigningOutput SignContent(T request)
        {
            var serializedRequest = JsonConvert.SerializeObject(request);
            var signResponse = cryptoService.SignContent(serializedRequest);

            return signResponse;
        }

        public SigningEnvelope<T> DecryptSignature(string signedContent, string signatory)
        {
            var decryptedSingature = cryptoService.DecryptSignature(signedContent, signatory);
            var deserializedContent = JsonConvert.DeserializeObject<T>(decryptedSingature?.Body?.Content);

            var body = new ContentBody<T>();

            if (decryptedSingature?.Body != null)
            {
                body.Content = deserializedContent;
                body.Version = decryptedSingature.Body.Version;
                body.CreateDateTime = decryptedSingature.Body.CreateDateTime;
                body.EmailAddress = decryptedSingature.Body.EmailAddress;
                body.IpAddress = decryptedSingature.Body.IpAddress;
                body.Signatory = decryptedSingature.Body.Signatory;
            }

            var mappedSignature = new SigningEnvelope<T>
            {
                Header = decryptedSingature?.Header,
                Body = body
            };

            return mappedSignature;
        }

        public string HashContent(T content)
        {
            var serializedContent = JsonConvert.SerializeObject(content);

            return cryptoService.HashContent(serializedContent);
        }

        public string HashContent(FileStream file)
        {
            return cryptoService.HashContent(file);
        }

        public VerificationResult<T> VerifySignature(DigitalSignature<T> signature)
        {
            var serializedOriginalContent = JsonConvert.SerializeObject(signature.OriginalContent);

            var mappedSigniture = new DigitalSignature<string>
            {
                SignatoryReference = signature.SignatoryReference,
                SignedContent = signature.SignedContent,
                OriginalContent = serializedOriginalContent
            };

            var serverVerification = cryptoService.VerifySignature(mappedSigniture);

            var expectedContent = JsonConvert.DeserializeObject<T>(serverVerification.ExpectedContent);
            var signedContent = JsonConvert.DeserializeObject<T>(serverVerification.SignedContent);

            return new VerificationResult<T>
            {
                CreatedDateTime = serverVerification.CreatedDateTime,
                IpAddress = serverVerification.IpAddress,
                SignatoryEmailAddress = serverVerification.SignatoryEmailAddress,
                SignatoryMatchedToSignature = serverVerification.SignatoryMatchedToSignature,
                SignedContentMatchesToSignature = serverVerification.SignedContentMatchesToSignature,
                SignedContent = signedContent,
                ExpectedContent = expectedContent
            };
        }
    }
}

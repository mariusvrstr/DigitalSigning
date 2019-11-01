using System;
using System.IO;
using CryptographyService.Contracts;
using Spike.CryptographyService.Models.DigitalSigning;
using Newtonsoft.Json;

namespace Spike.CryptographyService.Simulator
{
    public class CryptographySimulatedService : ICryptographyService
    {
        private const string SimAppKey = "SimulatedAppKey";
        private const string SimUserSymKey = "SimulatedUserSymmetricKey";
        private const string Signatory = "Bob123";
        private const string SignatoryEmail = "Bob@123.com";
        private const string SignatoryIpAddress = "192.167.23.4";
        private const string HashValue = "192.167.23.4";

        private string WrapWithSimulatorEnvolope(string content, string key)
        {
            return $"[{key}]: {content}";
        }

        private string Encrypt(string serializedObject, string salt, string key)
        {
            return WrapWithSimulatorEnvolope(serializedObject, key);
        }
        
        public string Encrypt(string serializedObject, string salt)
        {
            return Encrypt(serializedObject, salt, SimAppKey);
        }

        public string Decrypt(string encryptedContent, string salt)
        {
            const string delimtingValue = "]: ";
            var positionOfKey = encryptedContent.IndexOf(delimtingValue, StringComparison.Ordinal);
            

            if (positionOfKey == -1)
            {
                throw new Exception($"No key specified in the encrypted content.");
            }

            var keyName = encryptedContent.Substring(1, positionOfKey - 1);

            if (keyName != SimAppKey && keyName != SimUserSymKey)
            {
                throw new Exception($"Unable to decrypt encrypted content in Simulator Mode for key [{keyName}]");
            }

            var contentStart = positionOfKey + delimtingValue.Length;
            var serializedContent = encryptedContent.Substring(contentStart, encryptedContent.Length - (contentStart));

            return serializedContent;
        }

        public SigningOutput SignContent(string serializedRequest)
        {
            var salt = Guid.NewGuid().ToString();
            var envolope = SigningEnvelope<string>.Create(serializedRequest, Signatory, SignatoryEmail, SignatoryIpAddress);
            envolope.AddEncryptedHashForBody(HashValue);

            var serializedSignedContent = JsonConvert.SerializeObject(envolope);

            return SigningOutput.Create(serializedSignedContent, Signatory);
        }

        public SigningEnvelope<string> DecryptSignature(string signedContent, string signatory)
        {
            return  JsonConvert.DeserializeObject<SigningEnvelope<string>>(signedContent);
        }

        public string HashContent(string content)
        {
            return HashValue;
        }

        public string HashContent(FileStream file)
        {
            return HashValue;
        }

        public VerificationResult<string> VerifySignature(DigitalSignature<string> signature)
        {
            var verificationResults = new VerificationResult<string>();

            var envelope = DecryptSignature(signature.SignedContent, signature.SignatoryReference);

            if (envelope == null)
            {
                verificationResults.SignatoryMatchedToSignature = false;
                return verificationResults;
            }

            var hashCurrentBody = HashValue;
            var signitureMatch = hashCurrentBody == envelope.Header.EncryptedBodyHashSignature;

            verificationResults.IpAddress = envelope.Body?.IpAddress;
            verificationResults.SignatoryEmailAddress = envelope.Body?.EmailAddress;
            verificationResults.SignatoryMatchedToSignature = true;
            verificationResults.SignedContentMatchesToSignature = signitureMatch;

            verificationResults.ExpectedContent = signature.OriginalContent;
            verificationResults.SignedContent = envelope?.Body?.Content;

            return verificationResults;
        }
    }
}

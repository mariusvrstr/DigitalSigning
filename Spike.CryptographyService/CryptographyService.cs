using System.IO;
using CryptographyService.Contracts;
using Spike.CryptographyService.Cryptography;
using Spike.CryptographyService.Models.DigitalSigning;

namespace Spike.CryptographyService
{
    public class CryptographyService : ICryptographyService
    {
        public EncryptionManager EncryptionManager { get; set; }
        public HashManager HashManager { get; set; }

        public CryptographyService(EncryptionManager encryptionManager, HashManager hashManager)
        {
            this.EncryptionManager = encryptionManager;
            this.HashManager = hashManager;
        }

        public string Encrypt(string serializedObject, string salt)
        {
            return EncryptionManager.Encrypt(serializedObject, salt);
        }

        public string Decrypt(string encryptedContent, string salt)
        {
            return EncryptionManager.Decrypt(encryptedContent, salt);
        }

        public SigningOutput SignContent(string serializedRequest)
        {
            return EncryptionManager.SignContent(serializedRequest);
        }

        public SigningEnvelope<string> DecryptSignature(string signedContent, string signatory)
        {
            return EncryptionManager.DecryptSignedContent<string>(signedContent, signatory);
        }

        public string HashContent(string content)
        {
            return HashManager.HashContent(content);
        }

        public string HashContent(FileStream file)
        {
            return HashManager.HashContent(file);
        }

        public VerificationResult<string> VerifySignature(DigitalSignature<string> signature)
        {
            return EncryptionManager.VerifySignature(signature);
        }
    }
}

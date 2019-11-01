
using System.IO;
using Spike.CryptographyService.Models.DigitalSigning;

namespace CryptographyService.Contracts
{
    public interface ICryptographyService
    {
        string Encrypt(string serializedObject, string salt);

        string Decrypt(string encryptedContent, string salt);

        SigningOutput SignContent(string serializedRequest);

        SigningEnvelope<string> DecryptSignature(string signedContent, string signatory);

        string HashContent(string content);

        string HashContent(FileStream file);

        VerificationResult<string> VerifySignature(DigitalSignature<string> signature);
    }
}

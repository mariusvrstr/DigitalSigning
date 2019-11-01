
namespace Spike.CryptographyService.Models.Keys
{
    public class UserKeys
    {
        public string EncryptedPrivateKey { get; set; }
        public string PublicKey { get; set; }
        public string EncryptedSymetricKey { get; set; }

        public static UserKeys Create(string encryptedPrivateKey, string publicKey, string encryptedSymetricKey)
        {
            var keyPair = new UserKeys
            {
                EncryptedPrivateKey = encryptedPrivateKey,
                PublicKey = publicKey,
                EncryptedSymetricKey = encryptedSymetricKey
                
            };

            return keyPair;
        }
    }
}

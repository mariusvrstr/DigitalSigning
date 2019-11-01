
using Spike.CryptographyService.DAL;
using Spike.CryptographyService.Models.Keys;
using Spike.CryptographyService.Repository;

namespace Spike.CryptographyService.Cryptography
{
    public class KeyStore
    {
        public KeyStoreRepository KeyStoreRepo { get; }
        private const int RsaKeySize = 2048;
        private const string EncodingMode = "hex";
        
        public KeyStore(KeyStoreRepository repo)
        {
            KeyStoreRepo = repo;
        }

        /// <summary>
        /// Gets the private key for user. THIS MAY ONLY BE USED IN CONTEXT OF THE CURRENT USER!!!
        /// </summary>
        /// <param name="userReference">The user reference.</param>
        /// <returns>Private user key</returns>
        public string GetPrivateKeyForUser(string userReference)
        {
            var encryptedPrivateKey = GetUserKeyPair(userReference).EncryptedPrivateKey;
            var privateKey = EncryptionManager.Decrypt(encryptedPrivateKey, string.Empty);
        
            return privateKey;
        }

        public string GetSymmetricKeyForUser(string userReference)
        {
            var userKeys = GetUserKeyPair(userReference);

            var rsaDecryptor = new Chilkat.Rsa {EncodingMode = EncodingMode};

            rsaDecryptor.ImportPublicKey(userKeys.PublicKey);
            var symmetricKey = rsaDecryptor.DecryptStringENC(userKeys.EncryptedSymetricKey, false);

            return symmetricKey;
        }
        
        public string GetPublicKeyForUser(string userReference)
        {
            return GetUserKeyPair(userReference).PublicKey;
        }

        private UserKeys GenerateNewKeyPair()
        {
            var rsa = new Chilkat.Rsa();

            rsa.GenerateKey(RsaKeySize);

            var publicKey = rsa.ExportPublicKey();
            var privateKey = rsa.ExportPrivateKey();

            var symetricKey = EncryptionManager.GenerateSymmetricKey();

            var rsaEncryptor = new Chilkat.Rsa {EncodingMode = EncodingMode};
            rsaEncryptor.ImportPrivateKey(privateKey);
            
            // Encrypted with private so that public key can get access to symmetric key
            var encryptedSymetricKey = rsaEncryptor.EncryptStringENC(symetricKey, true);

            // Encrypted with application key so that the application can manage access to private keys
            var encryptedPrivateKey = EncryptionManager.Encrypt(privateKey, string.Empty);

            return UserKeys.Create(encryptedPrivateKey, publicKey, encryptedSymetricKey);
        }

        public UserKeys GetUserKeyPair(string userReference)
        {
            var userKeys = KeyStoreRepo.GetKeyPairByUserReference(userReference);

            if (userKeys != null)
            {
                return  UserKeys.Create(userKeys.PrivateKeyEncrypted, userKeys.PublicKey, userKeys.SymmetricKeyEncrypted);
            }

            // TODO: Ensure this is locked for race conditions
            var keyCombo = GenerateNewKeyPair();
            
            KeyStoreRepo.Add(new UserKeyEntity(userReference, keyCombo.EncryptedPrivateKey, keyCombo.PublicKey, keyCombo.EncryptedSymetricKey));

            KeyStoreRepo.Save();

            return keyCombo;
        }
    }
}

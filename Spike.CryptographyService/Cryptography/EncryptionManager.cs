using System;
using System.Security.Cryptography;
using Spike.CryptographyService.Models.DigitalSigning;
using Newtonsoft.Json;

namespace Spike.CryptographyService.Cryptography
{
    public class EncryptionManager
    {
        private const string EncryptionAlgorithm = "";
        private const string AesKeyCipher = "cbc";
        private const int AesKeyLength = 256;
        private const string EncodingMode = "hex";

        private KeyStore KeyStoreAdapter { get; }
        private string _signatoryCode;

        private static void RegisterChilKat()
        {
            var glob = new Chilkat.Global();
            var success = glob.UnlockBundle("Anything for 30-day trial");
        }

        private static string ResolveUserCode()
        {
            //TODO: Replace with Identity Service call that resolves both authenticated user codes and security tokens to emails
            return "Bob112233";
        }

        private static string ResolveUserEmail()
        {
            //TODO: Replace with Identity Service call that resolves both authenticated user codes and security tokens to emails
            return "bob@somthing.com";
        }

        private string ResolveUserIpAddress()
        {
            //TODO: Retrieve this from the user context
            return "192.168.3.44";
        }

        private static Chilkat.Crypt2 CreateAesEncryptionUtility()
        {
            return new Chilkat.Crypt2
            {
                CryptAlgorithm = EncryptionAlgorithm,
                CipherMode = AesKeyCipher,
                KeyLength = AesKeyLength,
                PaddingScheme = 0,
                EncodingMode = EncodingMode
            };
        }

        public string SignatoryCode
        {
            get
            {
                return _signatoryCode = _signatoryCode ?? ResolveUserCode();
            }
        }

        private string _signatoryEmail;

        public string SignatoryEmail
        {
            get
            {
                return _signatoryEmail = _signatoryEmail ?? ResolveUserEmail();
            }
        }

        private string _signatoryIpAddress;

        public string SignatoryIpAddress
        {
            get
            {
                return _signatoryIpAddress = _signatoryIpAddress ?? ResolveUserIpAddress();
            }
        }

        private string SignatoryReference => !string.IsNullOrWhiteSpace(SignatoryCode) ? SignatoryCode : SignatoryEmail;

        private static string Encrypt(string content, string key, string salt)
        {
            var crypt = CreateAesEncryptionUtility();

            crypt.SetEncodedIV(salt, EncodingMode);
            crypt.SetEncodedKey(key, EncodingMode);

            var encryptedContent = crypt.EncryptStringENC(content);

            return encryptedContent;
        }

        private static string Decrypt(string encryptedConent, string key, string salt)
        {
            var crypt = CreateAesEncryptionUtility();

            crypt.SetEncodedIV(salt, EncodingMode);
            crypt.SetEncodedKey(key, EncodingMode);

            var decryptedContent = crypt.DecryptStringENC(encryptedConent);

            return decryptedContent;
        }

        public static string GenerateSymmetricKey()
        {
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                var tokenData = new byte[32]; // Needed for an AES 256 bit encryption
                rng.GetBytes(tokenData);

                return Convert.ToBase64String(tokenData);
            }
        }

        public EncryptionManager(KeyStore keystoreManager)
        {
            KeyStoreAdapter = keystoreManager;
            RegisterChilKat();
        }
        
        public static string Encrypt(string serializedContent, string salt)
        {
            var keyUsed = ApplicationKeyManager.CurrentKeyName;
            var applicationKey = ApplicationKeyManager.Instance.CurrentApplicationKey;
            var encryptedContent = Encrypt(serializedContent, applicationKey, salt);

            return $"[{keyUsed}]: {encryptedContent}";
        }

        public static string Decrypt(string encryptedContent, string salt)
        {
            const string delimtingValue = "]: ";
            var positionOfKey = encryptedContent.IndexOf(delimtingValue, StringComparison.Ordinal);
            string encryptedData;
            string applicationKey;
            
            // If the encryption contains a key reference decrypt specific
            if (positionOfKey > -1)
            {
                var keyName = encryptedContent.Substring(1, positionOfKey - 1);
                var encryptionStart = positionOfKey + delimtingValue.Length;
                encryptedData = encryptedContent.Substring(encryptionStart, encryptedContent.Length - (encryptionStart));
                applicationKey = ApplicationKeyManager.Instance.ResolveApplicationKey(keyName);
            }
            else
            {
                encryptedData = encryptedContent;
                applicationKey = ApplicationKeyManager.Instance.CurrentApplicationKey;
            }

            var decryptedContentSerialized = Decrypt(encryptedData, applicationKey, salt);

            return decryptedContentSerialized;
        }
        
        public SigningOutput SignContent(string serializedRequest)
        {
            var hashManager = new HashManager();
            var privateKey = KeyStoreAdapter.GetPrivateKeyForUser(SignatoryReference);

            var envolope = SigningEnvelope<string>.Create(serializedRequest, SignatoryReference, SignatoryEmail, SignatoryIpAddress);
            var hashSigningContent = hashManager.HashContent(envolope.Body);

            var rsaEncryptor = new Chilkat.Rsa {EncodingMode = EncodingMode};
            rsaEncryptor.ImportPrivateKey(privateKey);

            var encryptedSignContentHash = rsaEncryptor.EncryptStringENC(hashSigningContent, true);
            envolope.AddEncryptedHashForBody(encryptedSignContentHash);
            
            var symmetricKey = KeyStoreAdapter.GetSymmetricKeyForUser(SignatoryReference);

            var serializedSignedContent = JsonConvert.SerializeObject(envolope);
            var encryptedSignedString = Encrypt(serializedSignedContent, symmetricKey);

            return SigningOutput.Create(encryptedSignedString, SignatoryReference);
        }

        public SigningEnvelope<T> DecryptSignedContent<T>(string encryptedSignedContent, string signatory)
        {
            var symmetricKey = KeyStoreAdapter.GetSymmetricKeyForUser(signatory);

            var decryptedEnvolope = Decrypt(encryptedSignedContent, symmetricKey);

            var decryptedContent = JsonConvert.DeserializeObject<SigningEnvelope<T>>(decryptedEnvolope);

            return decryptedContent;
        }

        public VerificationResult<string> VerifySignature(DigitalSignature<string> signature)
        {
            var verificationResults = new VerificationResult<string>();
            var hashManager = new HashManager();

            var envelope = DecryptSignedContent<string>(signature.SignedContent, signature.SignatoryReference);

            if (envelope == null)
            {
                verificationResults.SignatoryMatchedToSignature = false;
                return verificationResults;
            }

            var hashCurrentBody = hashManager.HashContent(envelope.Body);
            var publicKey = KeyStoreAdapter.GetPublicKeyForUser(signature.SignatoryReference);

            // Decrypt hash with public key to ensure there is no tampering and content is still the same
            var rsaDecryptor = new Chilkat.Rsa {EncodingMode = EncodingMode};

            rsaDecryptor.ImportPublicKey(publicKey);
            var decryptedoriginalHash = rsaDecryptor.DecryptStringENC(envelope.Header.EncryptedBodyHashSignature, false);

            var signitureMatch = hashCurrentBody == decryptedoriginalHash;

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

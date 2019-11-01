
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Spike.CryptographyService.Properties;

namespace Spike.CryptographyService.Cryptography
{
    public class ApplicationKeyManager
    {
        private static ApplicationKeyManager _instance;

        public static ApplicationKeyManager Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                var newInstance = new ApplicationKeyManager();
                
                return _instance = newInstance;
            }
        }
        
        private const string GreenKeyName = "GreenKey";
        private const string BlueKeyName = "BlueKey";
        public const string CurrentKeyName = GreenKeyName;

        public Dictionary<string, string> ApplicationKeys { get; set; }
        private string CertificatePrivateKey { get; set; }

        public ApplicationKeyManager()
        {
            LoadCryptoServiceKeys();
            ApplicationKeys = LoadApplicationKeys();
        }

        private void LoadCryptoServiceKeys()
        {
            X509Certificate2 cryptoServiceCert = null;

            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);

            store.Open(OpenFlags.ReadOnly);

            foreach (var certificate in store.Certificates)
            {
                if (certificate.FriendlyName == Settings.Default.CryptoServiceCertFriendlyName)
                {
                    cryptoServiceCert = certificate;
                }
            }

            if (cryptoServiceCert == null)
            {
                throw new Exception($"Unable to load the CryptoService certificate [{Settings.Default.CryptoServiceCertFriendlyName}] for user [{System.Security.Principal.WindowsIdentity.GetCurrent().Name}] ensure the certificate have been added to the trusted root of this users personal certificate store.");
            }

            CertificatePrivateKey = cryptoServiceCert.PrivateKey?.ToXmlString(true);
        }

        private string DecryptWithApplicationCertPrivateKey(string encryptedkey)
        {
            var rsaDecryptor = new Chilkat.Rsa { EncodingMode = "hex" };
            rsaDecryptor.ImportPrivateKey(CertificatePrivateKey);

            var applicationKey = rsaDecryptor.DecryptStringENC(encryptedkey, true);

            return applicationKey;
        }

        private Dictionary<string, string> LoadApplicationKeys()
        {
            var keys = new Dictionary<string, string>();

            var encryptedGreenKey = Settings.Default.EncryptedGreenApplicationKey;
            var greenKey = DecryptWithApplicationCertPrivateKey(encryptedGreenKey);
            keys.Add(GreenKeyName, greenKey);

            var encryptedBlueKey = Settings.Default.EncryptedBlueApplicationKey;
            var blueKey = encryptedBlueKey;
            keys.Add(BlueKeyName, blueKey);

            return keys;
        }

        public string CurrentApplicationKey
        {
            get
            {
                var currentEncryptionKey = ApplicationKeys?.SingleOrDefault(a => a.Key == CurrentKeyName);

                if (currentEncryptionKey == null)
                {
                    throw new Exception($"Could not retrieve the current application key [{CurrentKeyName}]");
                }

                return currentEncryptionKey.Value.Value;
            }
        }

        public string ResolveApplicationKey(string keyName)
        {
            var currentEncryptionKey = ApplicationKeys?.SingleOrDefault(a => a.Key == keyName);

            if (currentEncryptionKey == null)
            {
                throw new Exception($"Could not retrieve the requested application key [{keyName}]");
            }

            return currentEncryptionKey.Value.Value;
        }
    }
}

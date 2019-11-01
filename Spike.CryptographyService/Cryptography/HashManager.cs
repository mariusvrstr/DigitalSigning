using System;
using System.IO;
using Newtonsoft.Json;

namespace Spike.CryptographyService.Cryptography
{
    public class HashManager
    {
        private void RegisterChilKat()
        {
            var glob = new Chilkat.Global();
            var success = glob.UnlockBundle("Anything for 30-day trial");
        }

        public HashManager()
        {
            RegisterChilKat();
        }

        public string HashContent<T>(T content)
        {
            var crypt = new Chilkat.Crypt2 {HashAlgorithm = "sha256"};
            var serializedContent = JsonConvert.SerializeObject(content);

            var hash = crypt.HashStringENC(serializedContent);

            return hash;
        }

        public string HashContent(string content)
        {
            var crypt = new Chilkat.Crypt2 {HashAlgorithm = "sha256"};
            var hash = crypt.HashStringENC(content);

            return hash;
        }

        public string HashContent(FileStream file)
        {
            throw new NotImplementedException();
        }
    }
}

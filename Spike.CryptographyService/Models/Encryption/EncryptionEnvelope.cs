
namespace Spike.CryptographyService.Models.Encryption
{
    public class EncryptionEnvelope<T>
    {
        private const int CurrentVersion = 1;

        public T Content { get; set; }
        
        public string Salt { get; set; }

        public int Version { get; set; }

        public EncryptionEnvelope<T> Create(T content, string salt)
        {
            return new EncryptionEnvelope<T>
            {
                Content = content,
                Salt = salt,
                Version = CurrentVersion
            };
        }
    }
}

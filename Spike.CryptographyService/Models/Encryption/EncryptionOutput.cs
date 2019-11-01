
namespace Spike.CryptographyService.Models.Encryption
{
    public class EncryptionOutput
    {
        public string EncryptedContent { get; set; }

        public string KeyUsed { get; set; }

        public string Salt { get; set; }
    }
}

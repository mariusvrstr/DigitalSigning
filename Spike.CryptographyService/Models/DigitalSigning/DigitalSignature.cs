
namespace Spike.CryptographyService.Models.DigitalSigning
{
    public class DigitalSignature<T>
    {
        public string SignedContent { get; set; }

        public string SignatoryReference { get; set; }

        public T OriginalContent { get; set; }
    }
}

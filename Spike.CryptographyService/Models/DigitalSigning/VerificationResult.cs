using System;

namespace Spike.CryptographyService.Models.DigitalSigning
{
    public class VerificationResult<T>
    {
        public T ExpectedContent { get; set; }
        public T SignedContent { get; set; }
        public string SignatoryEmailAddress { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedDateTime { get; set; }
        
        public bool SignatoryMatchedToSignature { get; set; }
        public bool SignedContentMatchesToSignature { get; set; }
    }
}

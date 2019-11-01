using System;

namespace Spike.CryptographyService.Models.DigitalSigning
{
    public class ContentBody<T>
    {
        public T Content { get; set; }

        public string IpAddress { get; set; }

        public string ApproximateLocation { get; set; }

        public string Signatory { get; set; }

        public string EmailAddress { get; set; }

        public DateTime CreateDateTime { get; set; }

        public int Version { get; set; }
    }
}

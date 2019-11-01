using System;

namespace Spike.CryptographyService.Models.DigitalSigning
{
    public class SigningEnvelope<T>
    {
        private const int CurrentVersion = 1;

        public ContentHeader Header { get; set; }

        public ContentBody<T> Body { get; set; }

        public void AddEncryptedHashForBody(string hash)
        {
            Header.EncryptedBodyHashSignature = hash;
        }
        
        public static SigningEnvelope<T> Create(T content, string ipAddress, string signatory, string emailAddress)
        {
            var envolope = new SigningEnvelope<T>
            {
                Header = new ContentHeader(),
                Body = new ContentBody<T>
                {
                    Content = content,
                    IpAddress = ipAddress,

                    Signatory = signatory,
                    EmailAddress = emailAddress,
                    CreateDateTime = DateTime.Now,
                    Version = CurrentVersion
                }
            };

            return envolope;
        }
    }
}


namespace Spike.CryptographyService.Models.DigitalSigning
{
    public class SigningOutput
    {
        public string Signature { get; set; }

        public string SignatoryReference { get; set; }

        public static SigningOutput Create(string signature, string signatoryReference)
        {
            var response = new SigningOutput()
            {
                Signature = signature,
                SignatoryReference = signatoryReference
            };

            return response;
        }
    }
}

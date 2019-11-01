using System;
using Spike.CryptographyService.Repository;

namespace Spike.CryptographyService.DAL
{
    public class UserKeyEntity : IEntityBase
    {
        public UserKeyEntity()
        {
        }

        public UserKeyEntity(string userReference, string privateKeyEncrypted, string publicKey, string symmetricKeyEncrypted)
        {
            this.Id = new Guid();

            this.UserReference = userReference;
            this.PrivateKeyEncrypted = privateKeyEncrypted;
            this.PublicKey = publicKey;
            this.SymmetricKeyEncrypted = symmetricKeyEncrypted;
        }

        public Guid Id { get; set; }

        public string UserReference { get; set; }

        public string PrivateKeyEncrypted { get; set; }

        public string SymmetricKeyEncrypted { get; set; }

        public string PublicKey { get; set; }
    }
}

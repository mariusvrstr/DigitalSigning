using System.Data.Entity.ModelConfiguration;

namespace Spike.CryptographyService.DAL.Configurations
{
    public class UserKeyEntityConfiguration : EntityTypeConfiguration<UserKeyEntity>
    {
        public UserKeyEntityConfiguration()
        {
            this.HasKey(m => m.Id);
            this.Property(m => m.UserReference).IsRequired().HasMaxLength(255);
            this.Property(m => m.PublicKey).IsRequired();
            this.Property(m => m.PrivateKeyEncrypted).IsRequired();
            this.Property(m => m.SymmetricKeyEncrypted).IsRequired();
        }
    }
}

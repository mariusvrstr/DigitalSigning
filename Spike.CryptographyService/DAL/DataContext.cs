using System.Data.Entity;
using Spike.CryptographyService.DAL.Configurations;

namespace Spike.CryptographyService.DAL
{
    public class DataContext : DbContext
    {
        public DataContext() : base("name=SpikeCryptography") // Connection String Name
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<DataContext>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new UserKeyEntityConfiguration());
        }

        public DbSet<UserKeyEntity> UserKeys { get; set; }
    }
}

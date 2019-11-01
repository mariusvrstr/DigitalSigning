
using System.Linq;
using Spike.CryptographyService.DAL;

namespace Spike.CryptographyService.Repository
{
    public class KeyStoreRepository : RepositoryBase<UserKeyEntity>
    {
        public KeyStoreRepository(DataContext dataContext) : base(dataContext)
        {
        }

        public UserKeyEntity GetKeyPairByUserReference(string userReferece)
        {
            return this.Context.UserKeys.SingleOrDefault(e => (e.UserReference == userReferece));
        }
    }
}

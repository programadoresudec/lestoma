using lestoma.DatabaseOffline.ModelsOffline;
using lestoma.DatabaseOffline.Repositories.IRepository;

namespace lestoma.DatabaseOffline.Repositories.Repository
{
    public class UpaRepository : GenericRepository<UpaOffline>, IUpaRepository
    {
        public UpaRepository(DatabaseOffline db)
            : base(db)
        {
        }
    }
}
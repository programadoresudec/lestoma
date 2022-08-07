using lestoma.DatabaseOffline.ModelsOffline;
using lestoma.DatabaseOffline.Repositories.IRepository;

namespace lestoma.DatabaseOffline.Repositories.Repository
{
    public class LaboratorioRepository : GenericRepository<LaboratorioOffline>, ILaboratorioRepository
    {
        public LaboratorioRepository(DatabaseOffline db)
            : base(db)
        {
        }
    }
}
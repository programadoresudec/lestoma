using lestoma.DatabaseOffline.ModelsOffline;
using lestoma.DatabaseOffline.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.Repositories.Repository
{
    public class ModuloRepository : GenericRepository<ModuloOffline>, IModuloRepository
    {
        public ModuloRepository(DatabaseOffline db)
            : base(db)
        {
        }

    }
}
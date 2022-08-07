using lestoma.DatabaseOffline.ModelsOffline;
using lestoma.DatabaseOffline.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.Repositories.Repository
{
    public class ComponenteRepository : GenericRepository<ComponenteOffline>, IComponenteRepository
    {
        public ComponenteRepository(DatabaseOffline db)
            : base(db)
        {
        }
    }
}
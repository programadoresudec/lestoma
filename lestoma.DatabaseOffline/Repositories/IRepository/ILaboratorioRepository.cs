using lestoma.DatabaseOffline.ModelsOffline;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.Repositories.IRepository
{
    public interface ILaboratorioRepository : IGenericRepository<LaboratorioOffline>
    {
    }
}

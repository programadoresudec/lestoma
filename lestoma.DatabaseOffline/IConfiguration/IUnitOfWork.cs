using lestoma.DatabaseOffline.Repositories.IRepository;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.IConfiguration
{
    public interface IUnitOfWork
    {
        ILaboratorioRepository Laboratorio { get; }
        IComponenteRepository Componentes { get; }
        Task<bool> CompleteAsync();
        Task EnsureDeletedBD();
    }
}

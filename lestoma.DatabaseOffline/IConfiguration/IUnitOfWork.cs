using lestoma.DatabaseOffline.Repositories.IRepository;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.IConfiguration
{
    public interface IUnitOfWork
    {
        public string DBPath { get; set; }
        ILaboratorioRepository Laboratorio { get; }
        IComponenteRepository Componentes { get; }
        Task<bool> CompleteAsync();
    }
}

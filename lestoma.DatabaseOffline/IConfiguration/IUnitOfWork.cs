using lestoma.DatabaseOffline.Repositories.IRepository;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.IConfiguration
{
    public interface IUnitOfWork
    {
        public string DBPath { get; set; }
        IActividadRepository Actividades { get; }
        IModuloRepository Modulos { get; }
        IUpaRepository Upas { get; }
        ILaboratorioRepository Laboratorio { get; }
        IComponenteRepository Componentes { get; }
        Task<bool> CompleteAsync();
    }
}

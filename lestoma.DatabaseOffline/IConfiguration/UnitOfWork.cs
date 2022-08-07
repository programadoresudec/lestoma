using lestoma.DatabaseOffline.Repositories.IRepository;
using lestoma.DatabaseOffline.Repositories.Repository;
using System;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.IConfiguration
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly DatabaseOffline _contextOffline;

        public UnitOfWork()
        {
            _contextOffline = new DatabaseOffline(DBPath);
            ActividadRepository actividadContext = new ActividadRepository(_contextOffline);
            ComponenteRepository componenteContext = new ComponenteRepository(_contextOffline);
            UpaRepository upaContext = new UpaRepository(_contextOffline);
            ModuloRepository moduloContext = new ModuloRepository(_contextOffline);
            LaboratorioRepository laboratorioContext = new LaboratorioRepository(_contextOffline);

            Actividades = actividadContext;
            Modulos = moduloContext;
            Upas = upaContext;
            Laboratorio = laboratorioContext;
            Componentes = componenteContext;
        }

        public string DBPath { get; set; }

        public IActividadRepository Actividades { get; set; }

        public IModuloRepository Modulos { get; set; }

        public IUpaRepository Upas { get; set; }

        public ILaboratorioRepository Laboratorio { get; set; }

        public IComponenteRepository Componentes { get; set; }

        public async Task<bool> CompleteAsync()
        {
            await _contextOffline.SaveChangesAsync();
            return true;
        }


        public void Dispose()
        {
            _contextOffline.DisposeAsync();
        }
    }
}

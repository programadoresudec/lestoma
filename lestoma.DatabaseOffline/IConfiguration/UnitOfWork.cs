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
            ComponenteRepository componenteContext = new ComponenteRepository(_contextOffline);
            LaboratorioRepository laboratorioContext = new LaboratorioRepository(_contextOffline);
            Laboratorio = laboratorioContext;
            Componentes = componenteContext;
        }
        public string DBPath { get; set; }

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

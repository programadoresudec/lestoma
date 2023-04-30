using lestoma.CommonUtils.Helpers;
using lestoma.DatabaseOffline.Repositories.IRepository;
using lestoma.DatabaseOffline.Repositories.Repository;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.IConfiguration
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly DatabaseOffline _contextOffline;

        public UnitOfWork(string DBPath)
        {
            _contextOffline = new DatabaseOffline(DBPath);
            ComponenteRepository componenteContext = new ComponenteRepository(_contextOffline);
            LaboratorioRepository laboratorioContext = new LaboratorioRepository(_contextOffline);
            Laboratorio = laboratorioContext;
            Componentes = componenteContext;
        }
        public ILaboratorioRepository Laboratorio { get; set; }
        public IComponenteRepository Componentes { get; set; }
        public async Task<bool> CompleteAsync()
        {
            return await _contextOffline.SaveChangesAsync() > 0;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
          
        }
        protected virtual void Dispose(bool disposing)
        {
            _contextOffline.DisposeAsync();
        }
        public async Task EnsureDeletedBD()
        {
            if (_contextOffline.Database.GetService<IRelationalDatabaseCreator>().Exists())
            {
                // la base de datos ha sido eliminada 
                await _contextOffline.Database.EnsureDeletedAsync();
                LestomaLog.Normal("la base de datos ha sido eliminada");
            }
        }
    }
}

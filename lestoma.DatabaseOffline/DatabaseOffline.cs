using lestoma.DatabaseOffline.ModelsOffline;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline
{
    public class DatabaseOffline : DbContext
    {
        private readonly string _databasePath;
        public DatabaseOffline() { }

        #region Constructor conexion sqlite
        public DatabaseOffline(string dbPath)
        {
            _databasePath = dbPath;
            if (!string.IsNullOrWhiteSpace(dbPath))
            {
                //Database.EnsureDeleted();
                if (!Database.GetService<IRelationalDatabaseCreator>().Exists())
                {
                    // la base de datos ha sido creada
                    Database.EnsureCreated();
                }
            }
            // aplicar que cuando se desloguee elimine la bd local de sqlite

        }
        #endregion

        #region instancia de sqlite
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!string.IsNullOrEmpty(_databasePath))
            {
                optionsBuilder.UseSqlite(string.Format("Filename={0}", _databasePath));
            }
        }
        #endregion

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }

        #region DBSET tablas 

        public DbSet<LaboratorioOffline> TablaLaboratorio { get; set; }
        public DbSet<ComponenteOffline> TablaComponentes { get; set; }
        #endregion
    }
}

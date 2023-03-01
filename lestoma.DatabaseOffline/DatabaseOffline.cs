using lestoma.DatabaseOffline.ModelsOffline;
using Microsoft.EntityFrameworkCore;
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
            // aplicar que cuando se desloguee elimine la bd local de sqlite
            //Database.EnsureDeleted();
            Database.EnsureCreated();
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

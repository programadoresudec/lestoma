using lestoma.Entidades.Models;
using Microsoft.EntityFrameworkCore;

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
            Database.EnsureDeleted();
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



        #region DBSET tablas 

        public DbSet<EActividad> TablaActividades { get; set; }
        public DbSet<EUpaActividad> TablaUpasConActividades { get; set; }
        public DbSet<EUpa> TablaUpas { get; set; }


        #endregion
    }
}

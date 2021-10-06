using lestoma.Entidades.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.Repository
{
    public class ActividadRepository : GenericRepository<EActividad>
    {
        private readonly DatabaseOffline _db;

        public ActividadRepository(DatabaseOffline db)
            : base(db)
        {
            _db = db;
        }
        public async Task<bool> ExisteActividad(string nombre)
        {
            return await _db.TablaActividades.AnyAsync(x => x.Nombre.ToLower().Equals(nombre.ToLower()));
        }
    }
}
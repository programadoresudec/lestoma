using lestoma.CommonUtils.DTOs;
using lestoma.DatabaseOffline.ModelsOffline;
using lestoma.DatabaseOffline.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.Repositories.Repository
{
    public class ActividadRepository : GenericRepository<ActividadOffline>, IActividadRepository
    {
        public ActividadRepository(DatabaseOffline db)
            : base(db)
        {
        }

        public async Task<List<ActividadDTO>> GetActividades()
        {
            var query = await GetAll();
            var listado = _mapper.Map<List<ActividadDTO>>(query.ToList());
            return listado;
        }

        public async void MergeDataOnline(List<ActividadDTO> listado)
        {
            var actividades = _mapper.Map<List<ActividadOffline>>(listado);
            await Merge(actividades);
        }

        public async Task<bool> ExisteActividad(string nombre)
        {
            return await _dbSet.AnyAsync(x => x.Nombre.ToLower().Equals(nombre.ToLower()));
        }
    }
}
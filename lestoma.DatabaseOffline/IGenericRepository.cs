using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline
{
    public interface IGenericRepository<T> where T : class
    {
        Task Create(T entidad);
        Task<T> GetById(object id);
        Task Update(T entidad);
        Task Delete(T entidad);
        void Merge(List<T> ListadoEntidad);
        Task<IEnumerable<T>> GetAll();
    }
}

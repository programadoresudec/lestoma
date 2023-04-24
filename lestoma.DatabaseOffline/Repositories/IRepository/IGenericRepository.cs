using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.Repositories.IRepository
{
    public interface IGenericRepository<T> where T : class
    {
        Task Create(T entidad);
        Task<T> GetById(object id);
        Task<IEnumerable<T>> GetAll();
        Task<bool> ExistData();
        Task<int> CountData();
    }
}

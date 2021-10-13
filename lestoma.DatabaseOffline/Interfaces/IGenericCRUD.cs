using lestoma.CommonUtils.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.Interfaces
{
    public interface IGenericCRUD<T> where T : class
    {
        public string DbPath { get; set; }
        Task<Response> CrearAsync(T entidad);
        Task<Response> GetByIdAsync(object id);
        Task<Response> ActualizarAsync(T entidad);
        Task EliminarAsync(object id);
        Task<List<T>> GetAll();
        Task MergeEntity(List<T> entidad);
    }
}

using lestoma.CommonUtils.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lestoma.DatabaseOffline.Interfaces
{
    public interface IGenericCRUD<T, TID> where T : class
    {
        public string DbPath { get; set; }
        Task<Response> CrearAsync(T entidad);
        Task<Response> GetByIdAsync(TID id);
        Task<Response> ActualizarAsync(T entidad);
        Task EliminarAsync(TID id);
    }
}

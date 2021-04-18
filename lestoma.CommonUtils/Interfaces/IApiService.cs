using lestoma.CommonUtils.Responses;
using System.Threading.Tasks;

namespace lestoma.CommonUtils.Interfaces
{
    public interface IApiService
    {
        Task<Response> GetListAsync<T>(string urlBase, string controller);
        Task<Response> PostAsync<T>(string urlBase, string controller, T model);
    }
}

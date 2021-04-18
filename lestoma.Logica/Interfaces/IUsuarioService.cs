using lestoma.CommonUtils.Entities;
using lestoma.CommonUtils.Responses;
using System.Threading.Tasks;

namespace lestoma.Logica.Interfaces
{
    public interface IUsuarioService
    {
        Task<Response> Login(LoginRequest login);
        Task<Response> Register(EUsuario usuario);
        Task<Response> ChangePassword(EUsuario usuario);
        Task<Response> RecoverPassword(EUsuario usuario);

    }
}

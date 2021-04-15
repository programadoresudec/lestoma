using lestoma.Common.Responses;
using lestoma.Data;
using lestoma.Data.DAO;
using System;
using System.Threading.Tasks;

namespace lestoma.Logica
{
    public class LCuenta
    {
        private readonly Response _respuesta = new Response();
        public async Task<Response> Login(ResponseLogin login, Mapeo db)
        {
            var user = await new DAOUsuario().Logeo(login, db);
            if (user == null)
            {
                _respuesta.Mensaje = "El nombre de la cuenta, correo  y/o la contraseña que has introducido son incorrectos.";
            }
            else
            {
                _respuesta.Mensaje = "Ha iniciado satisfactoriamente al aplicativo.";
                _respuesta.Data = user;
                _respuesta.IsExito = true;
            }
            return _respuesta;
        }
    }
}

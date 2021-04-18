using lestoma.CommonUtils.Entities;
using lestoma.CommonUtils.Responses;
using lestoma.Data;
using lestoma.Data.DAO;
using lestoma.Logica.Interfaces;
using System;
using System.Threading.Tasks;

namespace lestoma.Logica.LogicaService
{
    public class LSUsuario : IUsuarioService
    {
        private readonly Response _respuesta = new Response();
        private readonly Mapeo _db;

        public LSUsuario(Mapeo db)
        {
            _db = db;
        }

        public async Task<Response> Login(LoginRequest login)
        {
            var user = await new DAOUsuario().Logeo(login, _db);
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
        public async Task<Response> Register(EUsuario usuario)
        {
            throw new NotImplementedException();
        }

        public async Task<Response> ChangePassword(EUsuario usuario)
        {
            throw new NotImplementedException();
        }



        public async Task<Response> RecoverPassword(EUsuario usuario)
        {
            throw new NotImplementedException();
        }
    }
}

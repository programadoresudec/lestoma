using lestoma.CommonUtils.Entities;
using lestoma.CommonUtils.Enums;
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

        private IGenericRepository<EUsuario> _usuarioRepository;

        public LSUsuario(IGenericRepository<EUsuario> usuarioRepository, Mapeo db)
        {
            this._db = db;
            this._usuarioRepository = usuarioRepository;
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
            bool existe = await new DAOUsuario().ExisteCorreo(usuario.Email, _db);
            if (existe)
            {
                _respuesta.Mensaje = "El correo ya esta en uso.";
            }
            else
            {
                usuario.RolId = (int)TipoRol.Auxiliar;
                usuario.EstadoId = (int)TipoEstadoUsuario.CheckCuenta;
                await _usuarioRepository.Create(usuario);
                _respuesta.Mensaje = "Se ha registrado satisfactoriamente.";
                _respuesta.IsExito = true;
            }
            return _respuesta;
        }

        public async Task<Response> ChangePassword(EUsuario usuario)
        {


            usuario.EstadoId = (int)TipoEstadoUsuario.Activado;
            await _usuarioRepository.Create(usuario);

            return _respuesta;
        }



        public async Task<Response> RecoverPassword(EUsuario usuario)
        {
            bool existe = await new DAOUsuario().ExisteCorreo(usuario.Email, _db);
            if (!existe)
            {
                _respuesta.Mensaje = "El correo no existe.";
            }
            return _respuesta;
        }

        public async Task<Response> lista()
        {
           
            _respuesta.Data = await _usuarioRepository.GetAll();
            return  _respuesta;
        }
    }
}

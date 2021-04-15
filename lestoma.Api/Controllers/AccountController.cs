using lestoma.Common.Entities;
using lestoma.Common.Responses;
using lestoma.Data;
using lestoma.Logica;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lestoma.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseController
    {
        private readonly Mapeo _context;
        public AccountController(Mapeo context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Logeo(ResponseLogin logeo)
        {
            if (ModelState.IsValid)
            {
                Respuesta = await new LCuenta().Login(logeo, _context);
                if (Respuesta.Data == null)
                {
                    return Unauthorized(Respuesta);
                }
                UsuarioToken usuario = new UsuarioToken
                {
                    Rol = ((EUsuario)Respuesta.Data).Rol.NombreRol,
                    Token = "sdwq"
                };
                Respuesta.Data = usuario;
                return Created(string.Empty, Respuesta);
            }
            return Unauthorized(Respuesta);
        }
    }
}

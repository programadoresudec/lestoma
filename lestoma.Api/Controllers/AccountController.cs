using lestoma.CommonUtils.Entities;
using lestoma.CommonUtils.Responses;
using lestoma.Data;
using lestoma.Logica;
using lestoma.Logica.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace lestoma.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseController
    {
        private readonly Mapeo _context;
        private readonly IUsuarioService _usuarioService;

        public AccountController(Mapeo context, IUsuarioService usuarioService)
        {
            _context = context;
            _usuarioService = usuarioService;
        }

        [HttpGet("Usuarios")]
        public async Task<IActionResult> Lista()
        {
            List<EUsuario> usuarios = await _context.TablaUsuarios.ToListAsync();
           return Ok(usuarios);
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Logeo(LoginRequest logeo)
        {
            if (ModelState.IsValid)
            {
                Respuesta = await _usuarioService.Login(logeo);

                if (Respuesta.Data == null)
                {
                    return Unauthorized(Respuesta);
                }
                TokenRequest usuario = new TokenRequest
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

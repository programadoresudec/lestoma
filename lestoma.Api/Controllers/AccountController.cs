using lestoma.CommonUtils;
using lestoma.CommonUtils.Entities;
using lestoma.CommonUtils.Responses;
using lestoma.Data;
using lestoma.Logica;
using lestoma.Logica.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace lestoma.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseController
    {
        private readonly AppSettings _appSettings;
        private readonly Mapeo _context;
        private readonly IUsuarioService _usuarioService;

        public AccountController(Mapeo context, IUsuarioService usuarioService, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _usuarioService = usuarioService;
            _appSettings = appSettings.Value;
        }

        [Authorize]
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
                    Token = GetToken((EUsuario)Respuesta.Data)
                };
                Respuesta.Data = usuario;
                return Created(string.Empty, Respuesta);
            }
            return Unauthorized(Respuesta);
        }

        private string GetToken(EUsuario user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var llave = Encoding.ASCII.GetBytes(_appSettings.Secreto);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity
                (
                    new Claim[]
                    {
                        new Claim(ClaimTypes.Role, user.Rol.NombreRol),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email)
                    }
                ),
                Audience = _appSettings.Audience,
                Issuer = _appSettings.Issuer,
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(llave), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}


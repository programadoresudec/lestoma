using AutoMapper;
using lestoma.CommonUtils;
using lestoma.CommonUtils.Entities;
using lestoma.CommonUtils.Responses;
using lestoma.Logica.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
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
        private readonly IUsuarioService _usuarioService;

        public AccountController(IUsuarioService usuarioService,
            IOptions<AppSettings> appSettings, IMapper mapper)
            : base(mapper)
        {
            _usuarioService = usuarioService;
            _appSettings = appSettings.Value;
        }

        [Authorize]
        [HttpGet("Usuarios")]
        public async Task<IActionResult> Lista()
        {

            return Ok();
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Logeo(LoginRequest logeo)
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
            return Ok(Respuesta);
        }

        [HttpPost("Registro")]
        public async Task<IActionResult> Registrarse(UsuarioRequest usuario)
        {
            var entidad = Mapear<UsuarioRequest, EUsuario>(usuario);
            Respuesta = await _usuarioService.Register(entidad);
            if (!Respuesta.IsExito)
            {
                return Conflict(Respuesta);
            }
            Respuesta.Data = usuario;
            return Created(string.Empty, Respuesta);
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


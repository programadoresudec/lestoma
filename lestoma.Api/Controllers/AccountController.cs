﻿using AutoMapper;
using lestoma.Api.Helpers;
using lestoma.CommonUtils.Entities;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Requests;
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
        private readonly IMailHelper _mailHelper;
        public AccountController(IUsuarioService usuarioService,
            IOptions<AppSettings> appSettings, IMapper mapper, IMailHelper mailHelper)
            : base(mapper)
        {
            _mailHelper = mailHelper; 
            _usuarioService = usuarioService;
            _appSettings = appSettings.Value;
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("Usuarios")]
        public async Task<IActionResult> Lista()
        {
            Respuesta = await _usuarioService.lista();
            return Ok(Respuesta);
        }
        #region logeo
        [HttpPost("Login")]
        public async Task<IActionResult> Logeo(LoginRequest logeo)
        {
            Respuesta = await _usuarioService.Login(logeo);
            if (Respuesta.Data == null)
            {
                return Unauthorized(Respuesta);
            }
            TokenResponse usuario = new()
            {
                Rol = ((EUsuario)Respuesta.Data).Rol.NombreRol,
                Token = GetToken((EUsuario)Respuesta.Data),
                User = new UserApp
                {
                    Nombre = ((EUsuario)Respuesta.Data).Nombre,
                    Apellido = ((EUsuario)Respuesta.Data).Apellido
                }
            };
            Respuesta.Data = usuario;
            return Ok(Respuesta);
        }
        #endregion

        #region registrarse
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
        #endregion

        #region olvido su contraseña
        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest email)
        {
            Respuesta = await _usuarioService.ForgotPassword(email);
            if (!Respuesta.IsExito && Respuesta.Data == null)
            {
                return Conflict(Respuesta);
            }
            var from = ((EUsuario)Respuesta.Data).Email;
            var codigo = ((EUsuario)Respuesta.Data).CodigoRecuperacion;

            await _mailHelper.SendCorreo(from, codigo, "Recuperación contraseña");
            return Created(string.Empty, Respuesta);
        }
        #endregion
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


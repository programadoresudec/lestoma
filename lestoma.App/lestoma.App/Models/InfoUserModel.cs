using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using System;

namespace lestoma.App.Models
{
    public class InfoUserModel
    {
        public InfoUserDTO InfoUser { get; set; } = new InfoUserDTO();
        public string ColorEstado => GetColor(InfoUser.Estado);

        private string GetColor(EstadoDTO estado)
        {
            string color = string.Empty;
            if (estado != null)
            {
                switch (estado.Id)
                {
                    case (int)TipoEstadoUsuario.CheckCuenta:
                        color = "#17a2b8";
                        break;
                    case (int)TipoEstadoUsuario.Activado:
                        color = "#79C000";
                        break;
                    case (int)TipoEstadoUsuario.Inactivo:
                        color = "#FBE122";
                        break;
                    case (int)TipoEstadoUsuario.Bloqueado:
                        color = "#FF4A4A";
                        break;
                }
            }
            return color;
        }
    }
}

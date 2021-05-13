using System;
using System.Collections.Generic;
using System.Text;

namespace lestoma.CommonUtils.Responses
{
    public class TokenRequest
    {
        public string Rol { get; set; }
        public string Token { get; set; }
        public UserApp User { get; set; }
    }

    public class UserApp
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string FullName => $"{Nombre} {Apellido}";
    }
}

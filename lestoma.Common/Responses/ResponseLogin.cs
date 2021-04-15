using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lestoma.Common.Responses
{
    public class ResponseLogin
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string Clave { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace lestoma.CommonUtils.Responses
{
    public class RequestLogin
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string Clave { get; set; }
    }
}

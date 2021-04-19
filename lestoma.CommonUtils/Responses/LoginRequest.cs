using System.ComponentModel.DataAnnotations;

namespace lestoma.CommonUtils.Responses
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Clave { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace lestoma.CommonUtils.Requests
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Clave { get; set; }
    }

    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class RecoverPasswordRequest
    {
        [Required]
        public string Codigo { get; set; }

        [Required]
        public string Password { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace HomeNodeAPI.DTOs.Auth
{
    public class LoginUserDTO
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}

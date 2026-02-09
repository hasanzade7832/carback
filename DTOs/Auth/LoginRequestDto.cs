using System.ComponentModel.DataAnnotations;

namespace CarAds.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required]
        public string Phone { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}

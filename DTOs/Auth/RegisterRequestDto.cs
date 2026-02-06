using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using CarAds.Attributes;   // ✅ بسیار مهم

namespace CarAds.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Phone { get; set; } = null!;

        public string? Email { get; set; }

        [Required]
        [MaxFileSize(2 * 1024 * 1024)] // 2MB
        [AllowedExtensions(new[] { ".jpg", ".jpeg", ".png", ".webp" })]
        public IFormFile LicenseImage { get; set; } = null!;
    }
}

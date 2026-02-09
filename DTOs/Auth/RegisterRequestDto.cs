using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using CarAds.Attributes;

namespace CarAds.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        public string Username { get; set; } = null!;

        // ✅ اجباری
        [Required]
        public string Phone { get; set; } = null!;

        // ✅ اجباری
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        // ✅ اجباری
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;

        [Required]
        [MaxFileSize(2 * 1024 * 1024)] // 2MB
        [AllowedExtensions(new[] { ".jpg", ".jpeg", ".png", ".webp" })]
        public IFormFile LicenseImage { get; set; } = null!;
    }
}

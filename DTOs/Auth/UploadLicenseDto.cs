using System.ComponentModel.DataAnnotations;

namespace CarAds.DTOs.Auth
{
    public class UploadLicenseDto
    {
        [Required]
        public IFormFile LicenseImage { get; set; } = null!;
    }
}

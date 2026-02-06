using CarAds.Enums;

namespace CarAds.Models
{
    public class License
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string ImageUrl { get; set; } = null!;

        public LicenseStatus Status { get; set; } = LicenseStatus.Pending;

        // ✅ استفاده شده در LicenseController
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ✅ Navigation
        public User User { get; set; } = null!;
    }
}

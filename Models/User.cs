using CarAds.Enums;

namespace CarAds.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;
        public string Phone { get; set; } = null!;

        // ✅ استفاده شده در AuthController
        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.User;

        // ✅ استفاده شده در Register
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ✅ Navigation
        public License? License { get; set; }
        public ICollection<CarAd> CarAds { get; set; } = new List<CarAd>();
    }
}

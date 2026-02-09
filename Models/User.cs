using CarAds.Enums;

namespace CarAds.Models
{
    public class User
    {
        public int Id { get; set; }

        // ✅ NEW
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        public string Username { get; set; } = null!;
        public string Phone { get; set; } = null!;

        // ✅ اجباری
        public string Email { get; set; } = null!;

        // ✅ برای لاگین
        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.User;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ✅ Navigation
        public License? License { get; set; }
        public ICollection<CarAd> CarAds { get; set; } = new List<CarAd>();
    }
}

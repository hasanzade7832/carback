using CarAds.Enums;

namespace CarAds.Models
{
    public class CarAd
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }

        public CarAdStatus Status { get; set; } = CarAdStatus.Pending;

        // ✅ تاریخ‌ها
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }

        // ✅ Navigation
        public User User { get; set; } = null!;

        public int? ApprovedByAdminId { get; set; }
        public User? ApprovedByAdmin { get; set; }

    }
}

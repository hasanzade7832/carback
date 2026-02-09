using CarAds.Enums;

namespace CarAds.Models
{
    public class CarAd
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        // ===== قبلی‌ها (حذف نمی‌شوند) =====
        // Title = نام خودرو
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        // Price = قیمت بر اساس "میلیون تومان" (مثل 12 یا 120.5)
        public decimal Price { get; set; }

        // ===== جدیدها مطابق فرم =====
        public CarAdType Type { get; set; } = CarAdType.UsedSale;

        // سال ساخت عددی (مثل 1401 یا 2018)
        public int Year { get; set; }

        public string Color { get; set; } = null!;

        // کارکرد (کیلومتر) - برای صفر هم می‌تونه 0 باشد
        public int MileageKm { get; set; }

        // مهلت بیمه (ماه)
        public int? InsuranceMonths { get; set; }

        public GearboxType Gearbox { get; set; } = GearboxType.None;

        // شماره شاسی
        public string ChassisNumber { get; set; } = null!;

        public CarAdStatus Status { get; set; } = CarAdStatus.Pending;

        // تاریخ‌ها
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;

        public int? ApprovedByAdminId { get; set; }
        public User? ApprovedByAdmin { get; set; }
    }
}

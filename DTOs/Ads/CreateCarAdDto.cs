using System.ComponentModel.DataAnnotations;
using CarAds.Enums;

namespace CarAds.DTOs.Ads
{
    public class CreateCarAdDto
    {
        [Required]
        public CarAdType Type { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!; // نام خودرو

        [Required]
        public int Year { get; set; }

        [Required]
        [MaxLength(50)]
        public string Color { get; set; } = null!;

        [Required]
        public int MileageKm { get; set; }

        public int? InsuranceMonths { get; set; }

        public GearboxType Gearbox { get; set; } = GearboxType.None;

        [Required]
        [MaxLength(50)]
        public string ChassisNumber { get; set; } = null!;

        // قیمت بر اساس میلیون تومان (مثل 12 یا 120.5)
        [Required]
        public decimal Price { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}

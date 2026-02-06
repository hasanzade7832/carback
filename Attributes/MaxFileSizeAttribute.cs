using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CarAds.Attributes
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly long _maxFileSize;

        public MaxFileSizeAttribute(long maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult(
                        $"حداکثر حجم فایل {_maxFileSize / 1024 / 1024}MB است"
                    );
                }
            }

            return ValidationResult.Success;
        }
    }
}

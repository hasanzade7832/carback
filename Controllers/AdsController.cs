using CarAds.Data;
using CarAds.DTOs.Ads;
using CarAds.Enums;
using CarAds.Models;
using CarAds.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CarAds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<CarAdHub> _hubContext;

        public AdsController(AppDbContext context, IHubContext<CarAdHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // =========================
        // لیست عمومی آگهی‌ها (فقط Approved به خاطر QueryFilter)
        // فیلتر بر اساس تب
        // =========================
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetApprovedAds([FromQuery] CarAdType? type = null)
        {
            var q = _context.CarAds.AsQueryable(); // QueryFilter اینجا فعاله

            if (type.HasValue)
                q = q.Where(x => x.Type == type.Value);

            var items = await q
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.Type,
                    x.Title,
                    x.Year,
                    x.Color,
                    x.MileageKm,
                    x.Price,
                    x.Gearbox,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        // =========================
        // جزئیات عمومی یک آگهی (Approved)
        // =========================
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetApprovedAd(int id)
        {
            var ad = await _context.CarAds
                .Where(x => x.Id == id) // QueryFilter => فقط Approved
                .Select(x => new
                {
                    x.Id,
                    x.Type,
                    x.Title,
                    x.Year,
                    x.Color,
                    x.MileageKm,
                    x.InsuranceMonths,
                    x.Gearbox,
                    x.ChassisNumber,
                    x.Price,
                    x.Description,
                    x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (ad == null)
                return NotFound("آگهی یافت نشد");

            return Ok(ad);
        }

        // =========================
        // ثبت آگهی جدید (کاربر لاگین)
        // خروجی: Pending
        // + ✅ SignalR => CarAdPending فقط برای Admins
        // =========================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateCarAdDto dto)
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("توکن معتبر نیست");

            var userId = int.Parse(userIdStr);

            var ad = new CarAd
            {
                UserId = userId,
                Type = dto.Type,
                Title = dto.Title,
                Year = dto.Year,
                Color = dto.Color,
                MileageKm = dto.MileageKm,
                InsuranceMonths = dto.InsuranceMonths,
                Gearbox = dto.Gearbox,
                ChassisNumber = dto.ChassisNumber,
                Price = dto.Price,
                Description = dto.Description ?? string.Empty,
                Status = CarAdStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.CarAds.Add(ad);
            await _context.SaveChangesAsync();

            // ✅ لحظه‌ای ارسال به ادمین‌ها: آگهی جدید Pending
            await _hubContext.Clients.Group("Admins").SendAsync(
                "CarAdPending",
                new
                {
                    ad.Id,
                    ad.Type,
                    ad.Title,
                    ad.Year,
                    ad.Color,
                    ad.MileageKm,
                    ad.Price,
                    ad.Gearbox,
                    ad.CreatedAt,
                    ad.UserId
                }
            );

            return Ok(new
            {
                message = "آگهی ثبت شد و در انتظار بررسی است.",
                adId = ad.Id,
                status = ad.Status.ToString()
            });
        }

        // =========================
        // آگهی‌های خودم (همه وضعیت‌ها)
        // =========================
        [HttpGet("mine")]
        [Authorize]
        public async Task<IActionResult> GetMine()
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("توکن معتبر نیست");

            var userId = int.Parse(userIdStr);

            var ads = await _context.CarAds
                .IgnoreQueryFilters() // تا Pending/Rejected هم بیاد
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.Type,
                    x.Title,
                    x.Year,
                    x.Color,
                    x.MileageKm,
                    x.Price,
                    x.Status,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(ads);
        }

        // =========================
        // ویرایش آگهی (فقط صاحب آگهی و فقط وقتی Pending باشد)
        // =========================
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCarAdDto dto)
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("توکن معتبر نیست");

            var userId = int.Parse(userIdStr);

            var ad = await _context.CarAds
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (ad == null)
                return NotFound("آگهی یافت نشد");

            if (ad.UserId != userId)
                return Forbid();

            if (ad.Status != CarAdStatus.Pending)
                return BadRequest("فقط آگهی‌های در انتظار بررسی قابل ویرایش هستند");

            ad.Type = dto.Type;
            ad.Title = dto.Title;
            ad.Year = dto.Year;
            ad.Color = dto.Color;
            ad.MileageKm = dto.MileageKm;
            ad.InsuranceMonths = dto.InsuranceMonths;
            ad.Gearbox = dto.Gearbox;
            ad.ChassisNumber = dto.ChassisNumber;
            ad.Price = dto.Price;
            ad.Description = dto.Description ?? string.Empty;

            await _context.SaveChangesAsync();

            return Ok("آگهی ویرایش شد");
        }

        // =========================
        // حذف آگهی (فقط صاحب آگهی و فقط Pending)
        // =========================
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("توکن معتبر نیست");

            var userId = int.Parse(userIdStr);

            var ad = await _context.CarAds
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (ad == null)
                return NotFound("آگهی یافت نشد");

            if (ad.UserId != userId)
                return Forbid();

            if (ad.Status != CarAdStatus.Pending)
                return BadRequest("فقط آگهی‌های در انتظار بررسی قابل حذف هستند");

            _context.CarAds.Remove(ad);
            await _context.SaveChangesAsync();

            return Ok("آگهی حذف شد");
        }
    }
}

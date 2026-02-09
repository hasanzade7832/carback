using CarAds.Data;
using CarAds.Enums;
using CarAds.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CarAds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class CarAdsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<CarAdHub> _hubContext;

        public CarAdsController(
            AppDbContext context,
            IHubContext<CarAdHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // =====================================
        // لیست آگهی‌های در انتظار بررسی
        // (IgnoreQueryFilters خیلی مهم)
        // =====================================
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingAds()
        {
            var ads = await _context.CarAds
                .IgnoreQueryFilters()
                .Where(x => x.Status == CarAdStatus.Pending)
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
                    x.ChassisNumber,
                    x.CreatedAt,
                    x.UserId
                })
                .ToListAsync();

            return Ok(ads);
        }

        // =====================================
        // تأیید آگهی
        // + ✅ Realtime برای:
        //   - صفحه اصلی (All) => CarAdApproved
        //   - پنل ادمین‌ها (Admins) => CarAdApprovedForAdmins
        //   - پنل صاحب آگهی (User:{id}) => CarAdStatusChanged
        // =====================================
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveAd(int id)
        {
            var ad = await _context.CarAds
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (ad == null)
                return NotFound("آگهی یافت نشد");

            if (ad.Status != CarAdStatus.Pending)
                return BadRequest("این آگهی قبلاً بررسی شده است");

            // ✅ Admin Id از JWT
            var adminId = int.Parse(User.FindFirst("userId")!.Value);

            ad.Status = CarAdStatus.Approved;
            ad.ApprovedAt = DateTime.UtcNow;
            ad.ApprovedByAdminId = adminId;

            await _context.SaveChangesAsync();

            // 1) ✅ برای صفحه اصلی (عمومی) - همه ببینن
            await _hubContext.Clients.All.SendAsync(
                "CarAdApproved",
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
                    ad.CreatedAt
                }
            );

            // 2) ✅ برای پنل ادمین‌ها - بدون رفرش از لیست Pending حذف/آپدیت کنن
            await _hubContext.Clients.Group("Admins").SendAsync(
                "CarAdApprovedForAdmins",
                new
                {
                    ad.Id,
                    status = ad.Status.ToString(),
                    ad.ApprovedAt,
                    ad.ApprovedByAdminId
                }
            );

            // 3) ✅ برای صاحب آگهی - وضعیت آگهی در پنل کاربر لحظه‌ای آپدیت شود
            await _hubContext.Clients.Group($"User:{ad.UserId}").SendAsync(
                "CarAdStatusChanged",
                new
                {
                    adId = ad.Id,
                    status = ad.Status.ToString(),
                    approvedAt = ad.ApprovedAt,
                    rejectedAt = ad.RejectedAt
                }
            );

            return Ok("آگهی با موفقیت تأیید شد");
        }

        // =====================================
        // رد آگهی
        // + ✅ Realtime برای:
        //   - پنل ادمین‌ها (Admins) => CarAdRejectedForAdmins
        //   - پنل صاحب آگهی (User:{id}) => CarAdStatusChanged
        // =====================================
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectAd(int id)
        {
            var ad = await _context.CarAds
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (ad == null)
                return NotFound("آگهی یافت نشد");

            if (ad.Status != CarAdStatus.Pending)
                return BadRequest("این آگهی قبلاً بررسی شده است");

            ad.Status = CarAdStatus.Rejected;
            ad.RejectedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // 1) ✅ برای پنل ادمین‌ها - از لیست pending حذف/آپدیت
            await _hubContext.Clients.Group("Admins").SendAsync(
                "CarAdRejectedForAdmins",
                new
                {
                    ad.Id,
                    status = ad.Status.ToString(),
                    ad.RejectedAt
                }
            );

            // 2) ✅ برای صاحب آگهی - وضعیت آگهی لحظه‌ای آپدیت
            await _hubContext.Clients.Group($"User:{ad.UserId}").SendAsync(
                "CarAdStatusChanged",
                new
                {
                    adId = ad.Id,
                    status = ad.Status.ToString(),
                    approvedAt = ad.ApprovedAt,
                    rejectedAt = ad.RejectedAt
                }
            );

            return Ok("آگهی رد شد");
        }
    }
}

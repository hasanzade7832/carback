using CarAds.Data;
using CarAds.Enums;
using CarAds.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
                    x.Title,
                    x.Price,
                    x.CreatedAt,
                    x.UserId
                })
                .ToListAsync();

            return Ok(ads);
        }

        // =====================================
        // تأیید آگهی
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
            var adminId = int.Parse(
                User.FindFirst("userId")!.Value
            );

            ad.Status = CarAdStatus.Approved;
            ad.ApprovedAt = DateTime.UtcNow;
            ad.ApprovedByAdminId = adminId;

            await _context.SaveChangesAsync();

            // ✅ SignalR فقط برای آگهی تأییدشده
            await _hubContext.Clients.All.SendAsync(
                "CarAdApproved",
                new
                {
                    ad.Id,
                    ad.Title,
                    ad.Price,
                    ad.CreatedAt
                }
            );

            return Ok("آگهی با موفقیت تأیید شد");
        }

        // =====================================
        // رد آگهی
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

            return Ok("آگهی رد شد");
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarAds.Data;
using CarAds.Enums;

namespace CarAds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class LicenseController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public LicenseController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ✅ لیست مجوزهای Pending
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingLicenses()
        {
            var licenses = await _context.Licenses
                .Include(x => x.User)
                .Where(x => x.Status == LicenseStatus.Pending)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    Username = x.User.Username,
                    x.User.Phone,
                    x.ImageUrl,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(licenses);
        }

        // ✅ لیست مجوزهای Approved
        [HttpGet("approved")]
        public async Task<IActionResult> GetApprovedLicenses()
        {
            var licenses = await _context.Licenses
                .Include(x => x.User)
                .Where(x => x.Status == LicenseStatus.Approved)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    Username = x.User.Username,
                    x.User.Phone,
                    x.ImageUrl,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(licenses);
        }

        // ✅ دانلود واقعی تصویر مجوز (Attachment)
        [HttpGet("{id:int}/download")]
        public async Task<IActionResult> DownloadLicenseImage(int id)
        {
            var lic = await _context.Licenses
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (lic == null) return NotFound("مجوز یافت نشد");

            // ImageUrl مثل: /uploads/licenses/xxx.webp
            var relative = lic.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_env.WebRootPath, relative);

            if (!System.IO.File.Exists(fullPath))
                return NotFound("فایل تصویر وجود ندارد");

            var ext = Path.GetExtension(fullPath).ToLowerInvariant();
            var contentType = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            var safeUser = lic.User?.Username ?? "user";
            var fileName = $"license_{id}_{safeUser}{ext}";

            return PhysicalFile(fullPath, contentType, fileName);
        }

        // ✅ تأیید مجوز
        [HttpPost("{id:int}/approve")]
        public async Task<IActionResult> ApproveLicense(int id)
        {
            var license = await _context.Licenses.FindAsync(id);
            if (license == null) return NotFound();

            license.Status = LicenseStatus.Approved;
            await _context.SaveChangesAsync();

            return Ok("مجوز تأیید شد");
        }

        // ✅ رد مجوز
        [HttpPost("{id:int}/reject")]
        public async Task<IActionResult> RejectLicense(int id)
        {
            var license = await _context.Licenses.FindAsync(id);
            if (license == null) return NotFound();

            license.Status = LicenseStatus.Rejected;
            await _context.SaveChangesAsync();

            return Ok("مجوز رد شد");
        }
    }
}

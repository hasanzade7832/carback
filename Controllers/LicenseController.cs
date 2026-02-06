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

        public LicenseController(AppDbContext context)
        {
            _context = context;
        }

        // لیست مجوزهای Pending
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingLicenses()
        {
            var licenses = await _context.Licenses
                .Include(x => x.User)
                .Where(x => x.Status == LicenseStatus.Pending)
                .Select(x => new
                {
                    x.Id,
                    x.User.Username,
                    x.User.Phone,
                    x.ImageUrl,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(licenses);
        }

        // تأیید مجوز
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveLicense(int id)
        {
            var license = await _context.Licenses.FindAsync(id);
            if (license == null)
                return NotFound();

            license.Status = LicenseStatus.Approved;
            await _context.SaveChangesAsync();

            return Ok("مجوز تأیید شد");
        }

        // رد مجوز
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectLicense(int id)
        {
            var license = await _context.Licenses.FindAsync(id);
            if (license == null)
                return NotFound();

            license.Status = LicenseStatus.Rejected;
            await _context.SaveChangesAsync();

            return Ok("مجوز رد شد");
        }
    }
}

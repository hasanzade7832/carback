using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CarAds.Data;
using CarAds.DTOs.Auth;
using CarAds.Enums;
using CarAds.Models;

namespace CarAds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public AuthController(
            AppDbContext context,
            IWebHostEnvironment env,
            IConfiguration configuration)
        {
            _context = context;
            _env = env;
            _configuration = configuration;
        }

        // =========================
        // Register
        // =========================
        [HttpPost("register")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Register([FromForm] RegisterRequestDto dto)
        {
            if (_context.Users.Any(x => x.Phone == dto.Phone))
                return BadRequest("شماره تلفن تکراری است");

            var allowedMimeTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!allowedMimeTypes.Contains(dto.LicenseImage.ContentType))
                return BadRequest("نوع فایل تصویر معتبر نیست");

            var uploadsRoot = Path.Combine(
                _env.WebRootPath,
                "uploads",
                "licenses"
            );

            if (!Directory.Exists(uploadsRoot))
                Directory.CreateDirectory(uploadsRoot);

            var extension = Path.GetExtension(dto.LicenseImage.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadsRoot, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await dto.LicenseImage.CopyToAsync(stream);
            }

            var user = new User
            {
                Username = dto.Username,
                Phone = dto.Phone,
                Email = dto.Email,
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var license = new License
            {
                UserId = user.Id,
                ImageUrl = $"/uploads/licenses/{fileName}",
                Status = LicenseStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Licenses.Add(license);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "ثبت‌نام انجام شد. مجوز در انتظار بررسی است.",
                userId = user.Id,
                licenseStatus = license.Status.ToString()
            });
        }

        // =========================
        // Login + JWT
        // =========================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var user = await _context.Users
                .Include(x => x.License)
                .FirstOrDefaultAsync(x => x.Phone == dto.Phone);

            if (user == null)
                return Unauthorized("کاربر یافت نشد");

            if (user.License == null)
                return Unauthorized("مجوزی برای این کاربر ثبت نشده");

            if (user.License.Status != LicenseStatus.Approved)
                return Unauthorized("مجوز هنوز تأیید نشده است");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("userId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("licenseStatus", user.License.Status.ToString())
            };

            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(jwtSettings["ExpireMinutes"]!)
                ),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                expiresIn = jwtSettings["ExpireMinutes"],
                role = user.Role.ToString(),
                licenseStatus = user.License.Status.ToString()
            });
        }
    }
}

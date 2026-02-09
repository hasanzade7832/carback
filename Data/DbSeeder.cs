using CarAds.Enums;
using CarAds.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarAds.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

            // ✅ مطمئن شو دیتابیس و مایگریشن‌ها اعمال شده
            await context.Database.MigrateAsync();

            var superAdminPhone = "09926559671";
            var superAdminPassword = "Admin123!";

            // اگر قبلاً وجود داشت، کاری نکن
            var user = await context.Users
                .Include(x => x.License)
                .FirstOrDefaultAsync(x => x.Phone == superAdminPhone);

            if (user != null)
            {
                // اگر نقش سوپرادمین نبود، اصلاح کن
                if (user.Role != UserRole.SuperAdmin)
                {
                    user.Role = UserRole.SuperAdmin;
                }

                // اگر لایسنس نداشت یا Approved نبود، درستش کن تا لاگین کار کنه
                if (user.License == null)
                {
                    user.License = new License
                    {
                        UserId = user.Id,
                        ImageUrl = "/uploads/licenses/seed-superadmin.webp",
                        Status = LicenseStatus.Approved,
                        CreatedAt = DateTime.UtcNow
                    };
                }
                else if (user.License.Status != LicenseStatus.Approved)
                {
                    user.License.Status = LicenseStatus.Approved;
                }

                await context.SaveChangesAsync();
                return;
            }

            // ✅ ساخت سوپرادمین جدید
            var newUser = new User
            {
                FirstName = "Super",
                LastName = "Admin",
                Username = "superadmin",
                Phone = superAdminPhone,
                Email = "superadmin@carads.local",
                Role = UserRole.SuperAdmin,
                CreatedAt = DateTime.UtcNow
            };

            newUser.PasswordHash = hasher.HashPassword(newUser, superAdminPassword);

            context.Users.Add(newUser);
            await context.SaveChangesAsync();

            // ✅ ساخت License تایید شده تا Login اجازه بده
            var license = new License
            {
                UserId = newUser.Id,
                ImageUrl = "/uploads/licenses/seed-superadmin.webp",
                Status = LicenseStatus.Approved,
                CreatedAt = DateTime.UtcNow
            };

            context.Licenses.Add(license);
            await context.SaveChangesAsync();
        }
    }
}

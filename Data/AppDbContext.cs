using CarAds.Enums;
using CarAds.Models;
using Microsoft.EntityFrameworkCore;

namespace CarAds.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<License> Licenses => Set<License>();
        public DbSet<CarAd> CarAds => Set<CarAd>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // User
            // =========================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.FirstName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(x => x.LastName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(x => x.Username)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(x => x.Phone)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(x => x.Email)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(x => x.PasswordHash)
                      .IsRequired();

                entity.Property(x => x.CreatedAt)
                      .IsRequired();

                entity.HasIndex(x => x.Username).IsUnique();
                entity.HasIndex(x => x.Phone).IsUnique();
                entity.HasIndex(x => x.Email).IsUnique();
            });

            // =========================
            // License (1 ↔ 1 User)
            // =========================
            modelBuilder.Entity<License>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ImageUrl)
                      .IsRequired();

                entity.Property(x => x.Status)
                      .IsRequired();

                entity.Property(x => x.CreatedAt)
                      .IsRequired();

                entity.HasOne(x => x.User)
                      .WithOne(x => x.License)
                      .HasForeignKey<License>(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // =========================
            // CarAd
            // =========================
            modelBuilder.Entity<CarAd>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(x => x.Description)
                      .IsRequired();

                // قیمت بر اساس میلیون تومان
                entity.Property(x => x.Price)
                      .HasColumnType("decimal(18,2)");

                entity.Property(x => x.Type)
                      .IsRequired();

                entity.Property(x => x.Year)
                      .IsRequired();

                entity.Property(x => x.Color)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(x => x.MileageKm)
                      .IsRequired();

                entity.Property(x => x.InsuranceMonths);

                entity.Property(x => x.Gearbox)
                      .IsRequired();

                entity.Property(x => x.ChassisNumber)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(x => x.Status)
                      .IsRequired();

                entity.Property(x => x.CreatedAt)
                      .IsRequired();

                entity.Property(x => x.ApprovedAt);
                entity.Property(x => x.RejectedAt);

                // User ↔ CarAd (1 → many)
                entity.HasOne(x => x.User)
                      .WithMany(u => u.CarAds)
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Admin Approver (Audit)
                entity.HasOne(x => x.ApprovedByAdmin)
                      .WithMany()
                      .HasForeignKey(x => x.ApprovedByAdminId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ✅ Global Query Filter: فقط آگهی‌های Approved
            modelBuilder.Entity<CarAd>()
                .HasQueryFilter(x => x.Status == CarAdStatus.Approved);
        }
    }
}

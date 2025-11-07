using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using GuideMe.ViewModels;
using GuideMe.Models;

namespace GuideMe.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<Guide> Guides { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Booking> Booking { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ContactAccess> ContactAccess { get; set; }
        public DbSet<UserOTP> UserOTPs { get; set; }





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // علاقة Trip ↔ Visitor
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Visitor)
                .WithMany(v => v.Trips)
                .HasForeignKey(t => t.VisitorId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة Trip ↔ Booking
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Trip)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة Booking ↔ Visitor
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Visitor)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VisitorId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة Booking ↔ Guide
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Guide)
                .WithMany(g => g.Bookings)
                .HasForeignKey(b => b.GuideId)
                .OnDelete(DeleteBehavior.Restrict);

            //  Review ↔ Visitor
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Visitor)
                .WithMany(v => v.Reviews)
                .HasForeignKey(r => r.VisitorId)
                .OnDelete(DeleteBehavior.Restrict);

            //  Review ↔ Trip
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Trip)
                .WithMany(t => t.Reviews)
                .HasForeignKey(r => r.TripId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        public DbSet<GuideMe.Models.ForgetPasswordVM> ForgetPasswordVM { get; set; } = default!;
        public DbSet<GuideMe.ViewModels.ResetPasswordVM> ResetPasswordVM { get; set; } = default!;
        public DbSet<GuideMe.Models.ChangePasswordVM> ChangePasswordVM { get; set; } = default!;

 





    }
}

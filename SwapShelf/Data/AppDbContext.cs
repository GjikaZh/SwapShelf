using Microsoft.EntityFrameworkCore;
using SwapShelf.Models;

namespace SwapShelf.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Listing> Listings { get; set; }
        public DbSet<WantedBook> WantedBooks { get; set; }
        public DbSet<SwapRequest> SwapRequests { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // SwapRequest — two FK to User, need explicit config to avoid cascade conflict
            modelBuilder.Entity<SwapRequest>()
                .HasOne(s => s.Initiator)
                .WithMany(u => u.InitiatedSwaps)
                .HasForeignKey(s => s.InitiatorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SwapRequest>()
                .HasOne(s => s.Receiver)
                .WithMany(u => u.ReceivedSwaps)
                .HasForeignKey(s => s.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // SwapRequest — two FK to Listing
            modelBuilder.Entity<SwapRequest>()
                .HasOne(s => s.InitiatorListing)
                .WithMany(l => l.InitiatedSwapRequests)
                .HasForeignKey(s => s.InitiatorListingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SwapRequest>()
                .HasOne(s => s.ReceiverListing)
                .WithMany(l => l.ReceivedSwapRequests)
                .HasForeignKey(s => s.ReceiverListingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review — two FK to User
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Reviewer)
                .WithMany(u => u.ReviewsGiven)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Reviewee)
                .WithMany(u => u.ReviewsReceived)
                .HasForeignKey(r => r.RevieweeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint — one review per direction per swap
            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.SwapRequestId, r.ReviewerId })
                .IsUnique();

            // Unique constraint — one wanted entry per user per book
            modelBuilder.Entity<WantedBook>()
                .HasIndex(w => new { w.UserId, w.BookId })
                .IsUnique();
        }
    }
}
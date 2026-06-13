using Microsoft.EntityFrameworkCore;
using SwapShelf.Data;
using SwapShelf.Models;
using SwapShelf.Repositories;

namespace SwapShelf.Tests.Repositories
{
    public class ReviewRepositoryTests
    {
        private static AppDbContext CreateContext() =>
            new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        /// <summary>
        /// Seeds the minimum entities needed for a SwapRequest and returns
        /// (user1, user2, swap). Caller is responsible for SaveChangesAsync.
        /// </summary>
        private static async Task<(User user1, User user2, SwapRequest swap)> SeedSwapAsync(AppDbContext ctx)
        {
            var user1 = new User { Id = 1, FullName = "Alice", Email = "alice@e.com", PasswordHash = "hash" };
            var user2 = new User { Id = 2, FullName = "Bob",   Email = "bob@e.com",   PasswordHash = "hash" };
            var book1 = new Book { Id = 1, Title = "Book A", Author = "Author A", Genre = "Fiction" };
            var book2 = new Book { Id = 2, Title = "Book B", Author = "Author B", Genre = "Fantasy" };

            await ctx.Users.AddRangeAsync(user1, user2);
            await ctx.Books.AddRangeAsync(book1, book2);

            var listing1 = new Listing { Id = 1, UserId = user1.Id, BookId = book1.Id, User = user1, Book = book1, Condition = ListingCondition.Good, Location = "Skopje" };
            var listing2 = new Listing { Id = 2, UserId = user2.Id, BookId = book2.Id, User = user2, Book = book2, Condition = ListingCondition.Good, Location = "Bitola" };
            await ctx.Listings.AddRangeAsync(listing1, listing2);

            var swap = new SwapRequest
            {
                Id = 1,
                InitiatorId = user1.Id,
                ReceiverId  = user2.Id,
                InitiatorListingId = listing1.Id,
                ReceiverListingId  = listing2.Id,
                Initiator = user1,
                Receiver  = user2,
                InitiatorListing = listing1,
                ReceiverListing  = listing2,
                Status = SwapStatus.Completed
            };
            await ctx.SwapRequests.AddAsync(swap);
            await ctx.SaveChangesAsync();

            return (user1, user2, swap);
        }

        // ── GetByRevieweeIdAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task GetByRevieweeIdAsync_ReturnsCorrectReviews()
        {
            await using var ctx = CreateContext();
            var (user1, user2, swap) = await SeedSwapAsync(ctx);

            // user1 reviews user2 (reviewee=2), user2 reviews user1 (reviewee=1)
            // Seed a third user to write a second review for user1 without violating the unique index
            var user3 = new User { Id = 3, FullName = "Carol", Email = "carol@e.com", PasswordHash = "hash" };
            await ctx.Users.AddAsync(user3);

            var book3    = new Book    { Id = 3, Title = "Book C", Author = "Author C", Genre = "Mystery" };
            await ctx.Books.AddAsync(book3);

            var listing3 = new Listing { Id = 3, UserId = user3.Id, BookId = book3.Id, User = user3, Book = book3, Condition = ListingCondition.Fair, Location = "Ohrid" };
            await ctx.Listings.AddAsync(listing3);

            var swap2 = new SwapRequest
            {
                Id = 2,
                InitiatorId = user3.Id,
                ReceiverId  = user1.Id,
                InitiatorListingId = listing3.Id,
                ReceiverListingId  = 1,
                Initiator = user3,
                Receiver  = user1,
                InitiatorListing = listing3,
                ReceiverListing  = ctx.Listings.Find(1)!,
                Status = SwapStatus.Completed
            };
            await ctx.SwapRequests.AddAsync(swap2);
            await ctx.SaveChangesAsync();

            // ReviewerId must differ per SwapRequest (unique index on SwapRequestId+ReviewerId)
            await ctx.Reviews.AddRangeAsync(
                new Review { Id = 1, SwapRequestId = swap.Id,  ReviewerId = user2.Id, RevieweeId = user1.Id, Rating = 5, Comment = "Great", Reviewer = user2, Reviewee = user1, SwapRequest = swap },
                new Review { Id = 2, SwapRequestId = swap2.Id, ReviewerId = user3.Id, RevieweeId = user1.Id, Rating = 4, Comment = "Good",  Reviewer = user3, Reviewee = user1, SwapRequest = swap2 },
                new Review { Id = 3, SwapRequestId = swap.Id,  ReviewerId = user1.Id, RevieweeId = user2.Id, Rating = 3, Comment = "Ok",    Reviewer = user1, Reviewee = user2, SwapRequest = swap });
            await ctx.SaveChangesAsync();

            var repo = new ReviewRepository(ctx);
            var result = await repo.GetByRevieweeIdAsync(user1.Id);

            Assert.Equal(2, result.Count());
            Assert.All(result, r => Assert.Equal(user1.Id, r.RevieweeId));
        }

        [Fact]
        public async Task GetByRevieweeIdAsync_IncludesReviewer()
        {
            await using var ctx = CreateContext();
            var (user1, user2, swap) = await SeedSwapAsync(ctx);

            await ctx.Reviews.AddAsync(new Review
            {
                Id = 1,
                SwapRequestId = swap.Id,
                ReviewerId    = user1.Id,
                RevieweeId    = user2.Id,
                Rating        = 5,
                Comment       = "Excellent",
                Reviewer      = user1,
                Reviewee      = user2,
                SwapRequest   = swap
            });
            await ctx.SaveChangesAsync();

            var repo = new ReviewRepository(ctx);
            var result = (await repo.GetByRevieweeIdAsync(user2.Id)).ToList();

            Assert.Single(result);
            Assert.NotNull(result[0].Reviewer);
        }

        // ── ExistsAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task ExistsAsync_ExistingReview_ReturnsTrue()
        {
            await using var ctx = CreateContext();
            var (user1, user2, swap) = await SeedSwapAsync(ctx);

            await ctx.Reviews.AddAsync(new Review
            {
                Id = 1,
                SwapRequestId = swap.Id,
                ReviewerId    = user1.Id,
                RevieweeId    = user2.Id,
                Rating        = 4,
                Comment       = "Good swap",
                Reviewer      = user1,
                Reviewee      = user2,
                SwapRequest   = swap
            });
            await ctx.SaveChangesAsync();

            var repo = new ReviewRepository(ctx);
            var result = await repo.ExistsAsync(swap.Id, user1.Id);

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_NonExistingReview_ReturnsFalse()
        {
            await using var ctx = CreateContext();
            var repo = new ReviewRepository(ctx);

            var result = await repo.ExistsAsync(99, 99);

            Assert.False(result);
        }

        // ── CreateAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_PersistsReview()
        {
            await using var ctx = CreateContext();
            var (user1, user2, swap) = await SeedSwapAsync(ctx);

            var repo = new ReviewRepository(ctx);
            var created = await repo.CreateAsync(new Review
            {
                SwapRequestId = swap.Id,
                ReviewerId    = user1.Id,
                RevieweeId    = user2.Id,
                Rating        = 5,
                Comment       = "Fantastic!"
            });

            var exists = await repo.ExistsAsync(swap.Id, user1.Id);
            Assert.True(exists);
            Assert.True(created.Id > 0);
        }
    }
}

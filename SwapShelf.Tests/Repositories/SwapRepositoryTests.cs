using Microsoft.EntityFrameworkCore;
using SwapShelf.Data;
using SwapShelf.Models;
using SwapShelf.Repositories;

namespace SwapShelf.Tests.Repositories
{
    public class SwapRepositoryTests
    {
        private static AppDbContext CreateContext() =>
            new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        /// <summary>
        /// Seeds two users, two books, two listings, and returns them so tests
        /// can build SwapRequests. Calls SaveChangesAsync internally.
        /// </summary>
        private static async Task<(User user1, User user2, Listing listing1, Listing listing2)> SeedBaseEntitiesAsync(AppDbContext ctx)
        {
            var user1 = new User { Id = 1, FullName = "Alice", Email = "alice@e.com", PasswordHash = "hash" };
            var user2 = new User { Id = 2, FullName = "Bob",   Email = "bob@e.com",   PasswordHash = "hash" };
            var book1 = new Book { Id = 1, Title = "Book A", Author = "Author A", Genre = "Fiction" };
            var book2 = new Book { Id = 2, Title = "Book B", Author = "Author B", Genre = "Fantasy" };

            await ctx.Users.AddRangeAsync(user1, user2);
            await ctx.Books.AddRangeAsync(book1, book2);

            var listing1 = new Listing
            {
                Id = 1, UserId = user1.Id, BookId = book1.Id,
                User = user1, Book = book1,
                Condition = ListingCondition.Good, Location = "Skopje",
                Status = ListingStatus.Available
            };
            var listing2 = new Listing
            {
                Id = 2, UserId = user2.Id, BookId = book2.Id,
                User = user2, Book = book2,
                Condition = ListingCondition.Fair, Location = "Bitola",
                Status = ListingStatus.Available
            };
            await ctx.Listings.AddRangeAsync(listing1, listing2);
            await ctx.SaveChangesAsync();

            return (user1, user2, listing1, listing2);
        }

        private static SwapRequest MakeSwap(int id, User initiator, User receiver, Listing initiatorListing, Listing receiverListing, SwapStatus status = SwapStatus.Pending) =>
            new()
            {
                Id = id,
                InitiatorId        = initiator.Id,
                ReceiverId         = receiver.Id,
                InitiatorListingId = initiatorListing.Id,
                ReceiverListingId  = receiverListing.Id,
                Initiator          = initiator,
                Receiver           = receiver,
                InitiatorListing   = initiatorListing,
                ReceiverListing    = receiverListing,
                Status             = status
            };

        // ── GetByUserIdAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetByUserIdAsync_ReturnsSwapsWhereUserIsInitiator()
        {
            await using var ctx = CreateContext();
            var (user1, user2, listing1, listing2) = await SeedBaseEntitiesAsync(ctx);

            await ctx.SwapRequests.AddAsync(MakeSwap(1, user1, user2, listing1, listing2));
            await ctx.SaveChangesAsync();

            var repo = new SwapRepository(ctx);
            var result = await repo.GetByUserIdAsync(user1.Id);

            Assert.Single(result);
            Assert.Equal(user1.Id, result.First().InitiatorId);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsSwapsWhereUserIsReceiver()
        {
            await using var ctx = CreateContext();
            var (user1, user2, listing1, listing2) = await SeedBaseEntitiesAsync(ctx);

            await ctx.SwapRequests.AddAsync(MakeSwap(1, user1, user2, listing1, listing2));
            await ctx.SaveChangesAsync();

            var repo = new SwapRepository(ctx);
            var result = await repo.GetByUserIdAsync(user2.Id);

            Assert.Single(result);
            Assert.Equal(user2.Id, result.First().ReceiverId);
        }

        // ── GetByIdAsync ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsSwap()
        {
            await using var ctx = CreateContext();
            var (user1, user2, listing1, listing2) = await SeedBaseEntitiesAsync(ctx);

            var swap = MakeSwap(1, user1, user2, listing1, listing2);
            await ctx.SwapRequests.AddAsync(swap);
            await ctx.SaveChangesAsync();

            var repo = new SwapRepository(ctx);
            var result = await repo.GetByIdAsync(swap.Id);

            Assert.NotNull(result);
            Assert.Equal(swap.Id, result.Id);
            Assert.NotNull(result.Initiator);
            Assert.NotNull(result.Receiver);
            Assert.NotNull(result.InitiatorListing);
            Assert.NotNull(result.ReceiverListing);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            await using var ctx = CreateContext();
            var repo = new SwapRepository(ctx);

            var result = await repo.GetByIdAsync(999);

            Assert.Null(result);
        }

        // ── HasActiveSwapForListingAsync ─────────────────────────────────────────

        [Fact]
        public async Task HasActiveSwapForListingAsync_PendingSwap_ReturnsTrue()
        {
            await using var ctx = CreateContext();
            var (user1, user2, listing1, listing2) = await SeedBaseEntitiesAsync(ctx);

            await ctx.SwapRequests.AddAsync(MakeSwap(1, user1, user2, listing1, listing2, SwapStatus.Pending));
            await ctx.SaveChangesAsync();

            var repo = new SwapRepository(ctx);
            var result = await repo.HasActiveSwapForListingAsync(listing1.Id);

            Assert.True(result);
        }

        [Fact]
        public async Task HasActiveSwapForListingAsync_AcceptedSwap_ReturnsTrue()
        {
            await using var ctx = CreateContext();
            var (user1, user2, listing1, listing2) = await SeedBaseEntitiesAsync(ctx);

            await ctx.SwapRequests.AddAsync(MakeSwap(1, user1, user2, listing1, listing2, SwapStatus.Accepted));
            await ctx.SaveChangesAsync();

            var repo = new SwapRepository(ctx);
            var result = await repo.HasActiveSwapForListingAsync(listing1.Id);

            Assert.True(result);
        }

        [Fact]
        public async Task HasActiveSwapForListingAsync_CompletedSwap_ReturnsFalse()
        {
            await using var ctx = CreateContext();
            var (user1, user2, listing1, listing2) = await SeedBaseEntitiesAsync(ctx);

            await ctx.SwapRequests.AddAsync(MakeSwap(1, user1, user2, listing1, listing2, SwapStatus.Completed));
            await ctx.SaveChangesAsync();

            var repo = new SwapRepository(ctx);
            var result = await repo.HasActiveSwapForListingAsync(listing1.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task HasActiveSwapForListingAsync_NoSwap_ReturnsFalse()
        {
            await using var ctx = CreateContext();
            var repo = new SwapRepository(ctx);

            var result = await repo.HasActiveSwapForListingAsync(999);

            Assert.False(result);
        }

        // ── CreateAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_PersistsSwap()
        {
            await using var ctx = CreateContext();
            var (user1, user2, listing1, listing2) = await SeedBaseEntitiesAsync(ctx);

            var repo = new SwapRepository(ctx);
            var created = await repo.CreateAsync(new SwapRequest
            {
                InitiatorId        = user1.Id,
                ReceiverId         = user2.Id,
                InitiatorListingId = listing1.Id,
                ReceiverListingId  = listing2.Id,
                Status             = SwapStatus.Pending
            });

            var fetched = await repo.GetByIdAsync(created.Id);
            Assert.NotNull(fetched);
            Assert.Equal(SwapStatus.Pending, fetched.Status);
        }

        // ── UpdateAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_PersistsStatusChange()
        {
            await using var ctx = CreateContext();
            var (user1, user2, listing1, listing2) = await SeedBaseEntitiesAsync(ctx);

            var swap = MakeSwap(1, user1, user2, listing1, listing2, SwapStatus.Pending);
            await ctx.SwapRequests.AddAsync(swap);
            await ctx.SaveChangesAsync();

            var repo = new SwapRepository(ctx);
            swap.Status = SwapStatus.Accepted;
            await repo.UpdateAsync(swap);

            var fetched = await repo.GetByIdAsync(swap.Id);
            Assert.NotNull(fetched);
            Assert.Equal(SwapStatus.Accepted, fetched.Status);
        }
    }
}

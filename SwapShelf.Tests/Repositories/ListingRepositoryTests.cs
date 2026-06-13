using Microsoft.EntityFrameworkCore;
using SwapShelf.Data;
using SwapShelf.Models;
using SwapShelf.Repositories;

namespace SwapShelf.Tests.Repositories
{
    public class ListingRepositoryTests
    {
        private static AppDbContext CreateContext() =>
            new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        private static User MakeUser(int id, string email) =>
            new() { Id = id, FullName = $"User{id}", Email = email, PasswordHash = "hash" };

        private static Book MakeBook(int id, string title = "Book", string author = "Author", string genre = "Fiction") =>
            new() { Id = id, Title = title, Author = author, Genre = genre };

        private static Listing MakeListing(int id, Book book, User user,
            ListingStatus status = ListingStatus.Available,
            ListingCondition condition = ListingCondition.Good,
            string location = "Skopje") =>
            new()
            {
                Id = id,
                BookId = book.Id,
                UserId = user.Id,
                Book = book,
                User = user,
                Status = status,
                Condition = condition,
                Location = location
            };

        // ── GetAllAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_NoFilters_OnlyReturnsAvailableListings()
        {
            await using var ctx = CreateContext();
            var user = MakeUser(1, "u1@e.com");
            var book = MakeBook(1);
            await ctx.Users.AddAsync(user);
            await ctx.Books.AddAsync(book);
            await ctx.Listings.AddRangeAsync(
                MakeListing(1, book, user, ListingStatus.Available),
                MakeListing(2, book, user, ListingStatus.Available),
                MakeListing(3, book, user, ListingStatus.Locked));
            await ctx.SaveChangesAsync();

            var repo = new ListingRepository(ctx);
            var result = await repo.GetAllAsync(null, null, null, null);

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllAsync_FilterByGenre_ReturnsMatching()
        {
            await using var ctx = CreateContext();
            var user = MakeUser(1, "u1@e.com");
            var mysteryBook1 = MakeBook(1, "Murder A", "Author A", "Mystery");
            var mysteryBook2 = MakeBook(2, "Murder B", "Author B", "Mystery");
            var fictionBook  = MakeBook(3, "Story C",  "Author C", "Fiction");
            await ctx.Users.AddAsync(user);
            await ctx.Books.AddRangeAsync(mysteryBook1, mysteryBook2, fictionBook);
            await ctx.Listings.AddRangeAsync(
                MakeListing(1, mysteryBook1, user),
                MakeListing(2, mysteryBook2, user),
                MakeListing(3, fictionBook,  user));
            await ctx.SaveChangesAsync();

            var repo = new ListingRepository(ctx);
            var result = await repo.GetAllAsync(genre: "mystery", null, null, null);

            Assert.Equal(2, result.Count());
            Assert.All(result, l => Assert.Equal("Mystery", l.Book.Genre));
        }

        [Fact]
        public async Task GetAllAsync_FilterByCondition_ReturnsMatching()
        {
            await using var ctx = CreateContext();
            var user = MakeUser(1, "u1@e.com");
            var book = MakeBook(1);
            await ctx.Users.AddAsync(user);
            await ctx.Books.AddAsync(book);
            await ctx.Listings.AddRangeAsync(
                MakeListing(1, book, user, condition: ListingCondition.Good),
                MakeListing(2, book, user, condition: ListingCondition.Fair),
                MakeListing(3, book, user, condition: ListingCondition.Poor));
            await ctx.SaveChangesAsync();

            var repo = new ListingRepository(ctx);
            var result = await repo.GetAllAsync(null, condition: "Good", null, null);

            Assert.Single(result);
            Assert.Equal(ListingCondition.Good, result.First().Condition);
        }

        [Fact]
        public async Task GetAllAsync_FilterByAuthor_ReturnsMatching()
        {
            await using var ctx = CreateContext();
            var user = MakeUser(1, "u1@e.com");
            var tolkienBook  = MakeBook(1, "LOTR",      "Tolkien",  "Fantasy");
            var rowlingBook  = MakeBook(2, "Harry P",   "Rowling",  "Fantasy");
            await ctx.Users.AddAsync(user);
            await ctx.Books.AddRangeAsync(tolkienBook, rowlingBook);
            await ctx.Listings.AddRangeAsync(
                MakeListing(1, tolkienBook, user),
                MakeListing(2, rowlingBook, user));
            await ctx.SaveChangesAsync();

            var repo = new ListingRepository(ctx);
            var result = await repo.GetAllAsync(null, null, null, author: "tolkien");

            Assert.Single(result);
            Assert.Contains("Tolkien", result.First().Book.Author);
        }

        // ── GetByIdAsync ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_ReturnsListingWithNavProps()
        {
            await using var ctx = CreateContext();
            var user = MakeUser(1, "u1@e.com");
            var book = MakeBook(1, "Dune", "Herbert", "Sci-Fi");
            await ctx.Users.AddAsync(user);
            await ctx.Books.AddAsync(book);
            var listing = MakeListing(1, book, user);
            await ctx.Listings.AddAsync(listing);
            await ctx.SaveChangesAsync();

            var repo = new ListingRepository(ctx);
            var result = await repo.GetByIdAsync(listing.Id);

            Assert.NotNull(result);
            Assert.NotNull(result.Book);
            Assert.NotNull(result.User);
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ReturnsNull()
        {
            await using var ctx = CreateContext();
            var repo = new ListingRepository(ctx);

            var result = await repo.GetByIdAsync(999);

            Assert.Null(result);
        }

        // ── GetByUserIdAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetByUserIdAsync_ReturnsOnlyUserListings()
        {
            await using var ctx = CreateContext();
            var user1 = MakeUser(1, "u1@e.com");
            var user2 = MakeUser(2, "u2@e.com");
            var book  = MakeBook(1);
            await ctx.Users.AddRangeAsync(user1, user2);
            await ctx.Books.AddAsync(book);
            await ctx.Listings.AddRangeAsync(
                MakeListing(1, book, user1),
                MakeListing(2, book, user1),
                MakeListing(3, book, user2));
            await ctx.SaveChangesAsync();

            var repo = new ListingRepository(ctx);
            var result = await repo.GetByUserIdAsync(user1.Id);

            Assert.Equal(2, result.Count());
            Assert.All(result, l => Assert.Equal(user1.Id, l.UserId));
        }

        // ── GetAvailableByBookAsync ──────────────────────────────────────────────

        [Fact]
        public async Task GetAvailableByBookAsync_ExcludesSpecifiedUser()
        {
            await using var ctx = CreateContext();
            var user1 = MakeUser(1, "u1@e.com");
            var user2 = MakeUser(2, "u2@e.com");
            var book  = MakeBook(1);
            await ctx.Users.AddRangeAsync(user1, user2);
            await ctx.Books.AddAsync(book);
            await ctx.Listings.AddRangeAsync(
                MakeListing(1, book, user1, ListingStatus.Available),
                MakeListing(2, book, user2, ListingStatus.Available));
            await ctx.SaveChangesAsync();

            var repo = new ListingRepository(ctx);
            var result = await repo.GetAvailableByBookAsync(book.Id, excludeUserId: user1.Id);

            Assert.Single(result);
            Assert.Equal(user2.Id, result.First().UserId);
        }

        [Fact]
        public async Task GetAvailableByBookAsync_ExcludesNonAvailable()
        {
            await using var ctx = CreateContext();
            var user1 = MakeUser(1, "u1@e.com");
            var user2 = MakeUser(2, "u2@e.com");
            var book  = MakeBook(1);
            await ctx.Users.AddRangeAsync(user1, user2);
            await ctx.Books.AddAsync(book);
            // user2 has a Locked listing — should not be returned
            await ctx.Listings.AddAsync(MakeListing(1, book, user2, ListingStatus.Locked));
            await ctx.SaveChangesAsync();

            var repo = new ListingRepository(ctx);
            var result = await repo.GetAvailableByBookAsync(book.Id, excludeUserId: user1.Id);

            Assert.Empty(result);
        }

        // ── GetAvailableByUserAsync ──────────────────────────────────────────────

        [Fact]
        public async Task GetAvailableByUserAsync_ReturnsOnlyAvailableForUser()
        {
            await using var ctx = CreateContext();
            var user = MakeUser(1, "u1@e.com");
            var book = MakeBook(1);
            await ctx.Users.AddAsync(user);
            await ctx.Books.AddAsync(book);
            await ctx.Listings.AddRangeAsync(
                MakeListing(1, book, user, ListingStatus.Available),
                MakeListing(2, book, user, ListingStatus.Locked));
            await ctx.SaveChangesAsync();

            var repo = new ListingRepository(ctx);
            var result = await repo.GetAvailableByUserAsync(user.Id);

            Assert.Single(result);
            Assert.Equal(ListingStatus.Available, result.First().Status);
        }

        // ── CreateAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_PersistsListing()
        {
            await using var ctx = CreateContext();
            var user = MakeUser(1, "u1@e.com");
            var book = MakeBook(1, "Foundation", "Asimov", "Sci-Fi");
            await ctx.Users.AddAsync(user);
            await ctx.Books.AddAsync(book);
            await ctx.SaveChangesAsync();

            var repo = new ListingRepository(ctx);
            var listing = new Listing
            {
                BookId = book.Id,
                UserId = user.Id,
                Condition = ListingCondition.Good,
                Location = "Skopje"
            };
            var created = await repo.CreateAsync(listing);

            var fetched = await repo.GetByIdAsync(created.Id);
            Assert.NotNull(fetched);
            Assert.Equal("Skopje", fetched.Location);
        }

        // ── UpdateAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_PersistsStatusChange()
        {
            await using var ctx = CreateContext();
            var user = MakeUser(1, "u1@e.com");
            var book = MakeBook(1);
            await ctx.Users.AddAsync(user);
            await ctx.Books.AddAsync(book);
            var listing = MakeListing(1, book, user, ListingStatus.Available);
            await ctx.Listings.AddAsync(listing);
            await ctx.SaveChangesAsync();

            var repo = new ListingRepository(ctx);
            listing.Status = ListingStatus.Locked;
            await repo.UpdateAsync(listing);

            var fetched = await repo.GetByIdAsync(listing.Id);
            Assert.NotNull(fetched);
            Assert.Equal(ListingStatus.Locked, fetched.Status);
        }

        // ── DeleteAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_RemovesListing()
        {
            await using var ctx = CreateContext();
            var user = MakeUser(1, "u1@e.com");
            var book = MakeBook(1);
            await ctx.Users.AddAsync(user);
            await ctx.Books.AddAsync(book);
            var listing = MakeListing(1, book, user);
            await ctx.Listings.AddAsync(listing);
            await ctx.SaveChangesAsync();

            var repo = new ListingRepository(ctx);
            await repo.DeleteAsync(listing.Id);

            var fetched = await repo.GetByIdAsync(listing.Id);
            Assert.Null(fetched);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingId_DoesNotThrow()
        {
            await using var ctx = CreateContext();
            var repo = new ListingRepository(ctx);

            var ex = await Record.ExceptionAsync(() => repo.DeleteAsync(999));

            Assert.Null(ex);
        }
    }
}

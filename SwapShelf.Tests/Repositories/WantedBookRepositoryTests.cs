using Microsoft.EntityFrameworkCore;
using SwapShelf.Data;
using SwapShelf.Models;
using SwapShelf.Repositories;

namespace SwapShelf.Tests.Repositories
{
    public class WantedBookRepositoryTests
    {
        private static AppDbContext CreateContext() =>
            new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        private static User MakeUser(int id, string email) =>
            new() { Id = id, FullName = $"User{id}", Email = email, PasswordHash = "hash" };

        private static Book MakeBook(int id, string title = "Book") =>
            new() { Id = id, Title = title, Author = "Author", Genre = "Fiction" };

        private static WantedBook MakeWantedBook(int id, User user, Book book) =>
            new() { Id = id, UserId = user.Id, BookId = book.Id, User = user, Book = book };

        // ── GetByUserIdAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetByUserIdAsync_ReturnsOnlyUserEntries()
        {
            await using var ctx = CreateContext();
            var user1 = MakeUser(1, "u1@e.com");
            var user2 = MakeUser(2, "u2@e.com");
            var book1 = MakeBook(1, "Book 1");
            var book2 = MakeBook(2, "Book 2");
            var book3 = MakeBook(3, "Book 3");
            await ctx.Users.AddRangeAsync(user1, user2);
            await ctx.Books.AddRangeAsync(book1, book2, book3);
            await ctx.WantedBooks.AddRangeAsync(
                MakeWantedBook(1, user1, book1),
                MakeWantedBook(2, user1, book2),
                MakeWantedBook(3, user2, book3));
            await ctx.SaveChangesAsync();

            var repo = new WantedBookRepository(ctx);
            var result = await repo.GetByUserIdAsync(user1.Id);

            Assert.Equal(2, result.Count());
            Assert.All(result, w => Assert.Equal(user1.Id, w.UserId));
        }

        // ── GetByIdAsync ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_IncludesBookAndUser()
        {
            await using var ctx = CreateContext();
            var user = MakeUser(1, "u1@e.com");
            var book = MakeBook(1, "Dune");
            await ctx.Users.AddAsync(user);
            await ctx.Books.AddAsync(book);
            var wanted = MakeWantedBook(1, user, book);
            await ctx.WantedBooks.AddAsync(wanted);
            await ctx.SaveChangesAsync();

            var repo = new WantedBookRepository(ctx);
            var result = await repo.GetByIdAsync(wanted.Id);

            Assert.NotNull(result);
            Assert.NotNull(result.Book);
            Assert.NotNull(result.User);
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ReturnsNull()
        {
            await using var ctx = CreateContext();
            var repo = new WantedBookRepository(ctx);

            var result = await repo.GetByIdAsync(999);

            Assert.Null(result);
        }

        // ── ExistsAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task ExistsAsync_ExistingEntry_ReturnsTrue()
        {
            await using var ctx = CreateContext();
            var user = MakeUser(1, "u1@e.com");
            var book = MakeBook(1);
            await ctx.Users.AddAsync(user);
            await ctx.Books.AddAsync(book);
            await ctx.WantedBooks.AddAsync(MakeWantedBook(1, user, book));
            await ctx.SaveChangesAsync();

            var repo = new WantedBookRepository(ctx);
            var result = await repo.ExistsAsync(user.Id, book.Id);

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_NonExistingEntry_ReturnsFalse()
        {
            await using var ctx = CreateContext();
            var repo = new WantedBookRepository(ctx);

            var result = await repo.ExistsAsync(99, 99);

            Assert.False(result);
        }

        // ── CreateAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_PersistsWantedBook()
        {
            await using var ctx = CreateContext();
            var user = MakeUser(1, "u1@e.com");
            var book = MakeBook(1, "Foundation");
            await ctx.Users.AddAsync(user);
            await ctx.Books.AddAsync(book);
            await ctx.SaveChangesAsync();

            var repo = new WantedBookRepository(ctx);
            var created = await repo.CreateAsync(new WantedBook { UserId = user.Id, BookId = book.Id });

            var exists = await repo.ExistsAsync(user.Id, book.Id);
            Assert.True(exists);
            Assert.True(created.Id > 0);
        }

        // ── DeleteAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_RemovesEntry()
        {
            await using var ctx = CreateContext();
            var user = MakeUser(1, "u1@e.com");
            var book = MakeBook(1);
            await ctx.Users.AddAsync(user);
            await ctx.Books.AddAsync(book);
            var wanted = MakeWantedBook(1, user, book);
            await ctx.WantedBooks.AddAsync(wanted);
            await ctx.SaveChangesAsync();

            var repo = new WantedBookRepository(ctx);
            await repo.DeleteAsync(wanted.Id);

            var result = await repo.GetByIdAsync(wanted.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingId_DoesNotThrow()
        {
            await using var ctx = CreateContext();
            var repo = new WantedBookRepository(ctx);

            var ex = await Record.ExceptionAsync(() => repo.DeleteAsync(999));

            Assert.Null(ex);
        }

        // ── DeleteByUserAndBookAsync ─────────────────────────────────────────────

        [Fact]
        public async Task DeleteByUserAndBookAsync_RemovesCorrectEntry()
        {
            await using var ctx = CreateContext();
            var user  = MakeUser(1, "u1@e.com");
            var book1 = MakeBook(1, "Book 1");
            var book2 = MakeBook(2, "Book 2");
            await ctx.Users.AddAsync(user);
            await ctx.Books.AddRangeAsync(book1, book2);
            await ctx.WantedBooks.AddRangeAsync(
                MakeWantedBook(1, user, book1),
                MakeWantedBook(2, user, book2));
            await ctx.SaveChangesAsync();

            var repo = new WantedBookRepository(ctx);
            await repo.DeleteByUserAndBookAsync(user.Id, book1.Id);

            var remaining = await repo.GetByUserIdAsync(user.Id);
            Assert.Single(remaining);
            Assert.Equal(book2.Id, remaining.First().BookId);
        }

        [Fact]
        public async Task DeleteByUserAndBookAsync_NoMatchingEntry_DoesNotThrow()
        {
            await using var ctx = CreateContext();
            var repo = new WantedBookRepository(ctx);

            var ex = await Record.ExceptionAsync(() => repo.DeleteByUserAndBookAsync(99, 99));

            Assert.Null(ex);
        }
    }
}

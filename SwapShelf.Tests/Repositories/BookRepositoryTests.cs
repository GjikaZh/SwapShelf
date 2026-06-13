using Microsoft.EntityFrameworkCore;
using SwapShelf.Data;
using SwapShelf.Models;
using SwapShelf.Repositories;

namespace SwapShelf.Tests.Repositories
{
    public class BookRepositoryTests
    {
        private static AppDbContext CreateContext() =>
            new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        // ── GetAllAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_ReturnsAllBooks()
        {
            await using var ctx = CreateContext();
            await ctx.Books.AddRangeAsync(
                new Book { Title = "Book A", Author = "Author A", Genre = "Fiction" },
                new Book { Title = "Book B", Author = "Author B", Genre = "Mystery" },
                new Book { Title = "Book C", Author = "Author C", Genre = "Sci-Fi" });
            await ctx.SaveChangesAsync();

            var repo = new BookRepository(ctx);
            var result = await repo.GetAllAsync();

            Assert.Equal(3, result.Count());
        }

        // ── GetByIdAsync ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsBook()
        {
            await using var ctx = CreateContext();
            var book = new Book { Title = "Dune", Author = "Frank Herbert", Genre = "Sci-Fi" };
            await ctx.Books.AddAsync(book);
            await ctx.SaveChangesAsync();

            var repo = new BookRepository(ctx);
            var result = await repo.GetByIdAsync(book.Id);

            Assert.NotNull(result);
            Assert.Equal("Dune", result.Title);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            await using var ctx = CreateContext();
            var repo = new BookRepository(ctx);

            var result = await repo.GetByIdAsync(999);

            Assert.Null(result);
        }

        // ── GetByTitleAndAuthorAsync ─────────────────────────────────────────────

        [Fact]
        public async Task GetByTitleAndAuthorAsync_CaseInsensitive_ReturnsBook()
        {
            await using var ctx = CreateContext();
            await ctx.Books.AddAsync(new Book
            {
                Title = "Lord of the Rings",
                Author = "Tolkien",
                Genre = "Fantasy"
            });
            await ctx.SaveChangesAsync();

            var repo = new BookRepository(ctx);
            var result = await repo.GetByTitleAndAuthorAsync("lord of the rings", "tolkien");

            Assert.NotNull(result);
            Assert.Equal("Lord of the Rings", result.Title);
        }

        [Fact]
        public async Task GetByTitleAndAuthorAsync_NotFound_ReturnsNull()
        {
            await using var ctx = CreateContext();
            var repo = new BookRepository(ctx);

            var result = await repo.GetByTitleAndAuthorAsync("Nonexistent", "Nobody");

            Assert.Null(result);
        }

        // ── CreateAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_PersistsBook()
        {
            await using var ctx = CreateContext();
            var repo = new BookRepository(ctx);

            var created = await repo.CreateAsync(new Book
            {
                Title = "1984",
                Author = "George Orwell",
                Genre = "Dystopia"
            });

            var fetched = await repo.GetByIdAsync(created.Id);
            Assert.NotNull(fetched);
            Assert.Equal("1984", fetched.Title);
        }
    }
}

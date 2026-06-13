using Microsoft.EntityFrameworkCore;
using SwapShelf.Data;
using SwapShelf.Models;
using SwapShelf.Repositories;

namespace SwapShelf.Tests.Repositories
{
    public class UserRepositoryTests
    {
        private static AppDbContext CreateContext() =>
            new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        private static User MakeUser(string email = "alice@example.com", string name = "Alice") =>
            new() { FullName = name, Email = email, PasswordHash = "hash" };

        // ── GetByIdAsync ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_ExistingUser_ReturnsUser()
        {
            await using var ctx = CreateContext();
            var user = MakeUser();
            await ctx.Users.AddAsync(user);
            await ctx.SaveChangesAsync();

            var repo = new UserRepository(ctx);
            var result = await repo.GetByIdAsync(user.Id);

            Assert.NotNull(result);
            Assert.Equal("Alice", result.FullName);
        }

        [Fact]
        public async Task GetByIdAsync_NonExisting_ReturnsNull()
        {
            await using var ctx = CreateContext();
            var repo = new UserRepository(ctx);

            var result = await repo.GetByIdAsync(999);

            Assert.Null(result);
        }

        // ── GetByEmailAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
        {
            await using var ctx = CreateContext();
            await ctx.Users.AddAsync(MakeUser("bob@example.com", "Bob"));
            await ctx.SaveChangesAsync();

            var repo = new UserRepository(ctx);
            var result = await repo.GetByEmailAsync("bob@example.com");

            Assert.NotNull(result);
            Assert.Equal("Bob", result.FullName);
        }

        [Fact]
        public async Task GetByEmailAsync_NonExisting_ReturnsNull()
        {
            await using var ctx = CreateContext();
            var repo = new UserRepository(ctx);

            var result = await repo.GetByEmailAsync("nobody@example.com");

            Assert.Null(result);
        }

        // ── GetAllAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_ReturnsAllUsers()
        {
            await using var ctx = CreateContext();
            await ctx.Users.AddRangeAsync(
                MakeUser("u1@example.com", "User1"),
                MakeUser("u2@example.com", "User2"),
                MakeUser("u3@example.com", "User3"));
            await ctx.SaveChangesAsync();

            var repo = new UserRepository(ctx);
            var result = await repo.GetAllAsync();

            Assert.Equal(3, result.Count());
        }

        // ── ExistsAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task ExistsAsync_ExistingEmail_ReturnsTrue()
        {
            await using var ctx = CreateContext();
            await ctx.Users.AddAsync(MakeUser("exists@example.com"));
            await ctx.SaveChangesAsync();

            var repo = new UserRepository(ctx);
            var result = await repo.ExistsAsync("exists@example.com");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_NonExistingEmail_ReturnsFalse()
        {
            await using var ctx = CreateContext();
            var repo = new UserRepository(ctx);

            var result = await repo.ExistsAsync("ghost@example.com");

            Assert.False(result);
        }

        // ── CreateAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_PersistsUser()
        {
            await using var ctx = CreateContext();
            var repo = new UserRepository(ctx);

            var created = await repo.CreateAsync(MakeUser("new@example.com", "New User"));

            var fetched = await repo.GetByIdAsync(created.Id);
            Assert.NotNull(fetched);
            Assert.Equal("new@example.com", fetched.Email);
        }

        // ── UpdateAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_PersistsChanges()
        {
            await using var ctx = CreateContext();
            var user = new User { FullName = "Charlie", Email = "charlie@example.com", PasswordHash = "hash", TrustScore = 0 };
            await ctx.Users.AddAsync(user);
            await ctx.SaveChangesAsync();

            var repo = new UserRepository(ctx);
            user.TrustScore = 4.5;
            await repo.UpdateAsync(user);

            var fetched = await repo.GetByIdAsync(user.Id);
            Assert.NotNull(fetched);
            Assert.Equal(4.5, fetched.TrustScore);
        }
    }
}

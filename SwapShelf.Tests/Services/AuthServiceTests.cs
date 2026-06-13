using Microsoft.Extensions.Configuration;
using NSubstitute;
using SwapShelf.DTOs;
using SwapShelf.Models;
using SwapShelf.Repositories;
using SwapShelf.Services.Implementations;
using SwapShelf.Tests.Helpers;

namespace SwapShelf.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            // Wire up a minimal IConfiguration that satisfies GenerateJwtToken
            var jwtSection = Substitute.For<IConfigurationSection>();
            jwtSection["Key"].Returns("TestKeyThatIsAtLeast32CharactersLongForHmac!");
            jwtSection["Issuer"].Returns("SwapShelfTest");
            jwtSection["Audience"].Returns("SwapShelfTestUsers");

            var config = Substitute.For<IConfiguration>();
            config.GetSection("JwtSettings").Returns(jwtSection);

            _sut = new AuthService(_userRepo, config);
        }

        // ── RegisterAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task RegisterAsync_EmailAlreadyRegistered_ThrowsInvalidOperationException()
        {
            _userRepo.ExistsAsync("taken@test.com").Returns(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.RegisterAsync(new RegisterRequest
                {
                    Email = "taken@test.com",
                    FullName = "Someone",
                    Password = "pass123"
                }));
        }

        [Fact]
        public async Task RegisterAsync_NewEmail_ReturnsTokenAndCorrectEmail()
        {
            var user = TestBuilders.User(1, "Alice", "alice@test.com");
            _userRepo.ExistsAsync("alice@test.com").Returns(false);
            _userRepo.CreateAsync(Arg.Any<User>()).Returns(user);

            var result = await _sut.RegisterAsync(new RegisterRequest
            {
                Email = "alice@test.com",
                FullName = "Alice",
                Password = "pass123"
            });

            Assert.NotEmpty(result.Token);
            Assert.Equal("alice@test.com", result.Email);
            Assert.Equal("Alice", result.FullName);
        }

        [Fact]
        public async Task RegisterAsync_NewEmail_PasswordIsHashedNotStored()
        {
            var capturedUser = default(User);
            _userRepo.ExistsAsync(Arg.Any<string>()).Returns(false);
            _userRepo.CreateAsync(Arg.Any<User>()).Returns(ci =>
            {
                capturedUser = ci.Arg<User>();
                return Task.FromResult(ci.Arg<User>());
            });

            await _sut.RegisterAsync(new RegisterRequest
            {
                Email = "bob@test.com",
                FullName = "Bob",
                Password = "plaintext"
            });

            Assert.NotNull(capturedUser);
            Assert.NotEqual("plaintext", capturedUser!.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify("plaintext", capturedUser.PasswordHash));
        }

        // ── LoginAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task LoginAsync_UserNotFound_ThrowsUnauthorizedAccessException()
        {
            _userRepo.GetByEmailAsync("ghost@test.com").Returns((User?)null);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _sut.LoginAsync(new LoginRequest { Email = "ghost@test.com", Password = "any" }));
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
        {
            var user = TestBuilders.User(1, email: "user@test.com");
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
            _userRepo.GetByEmailAsync("user@test.com").Returns(user);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _sut.LoginAsync(new LoginRequest { Email = "user@test.com", Password = "wrongpassword" }));
        }

        [Fact]
        public async Task LoginAsync_BannedUser_ThrowsUnauthorizedAccessException()
        {
            var user = TestBuilders.User(1, email: "banned@test.com");
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass123");
            user.IsBanned = true;
            _userRepo.GetByEmailAsync("banned@test.com").Returns(user);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _sut.LoginAsync(new LoginRequest { Email = "banned@test.com", Password = "pass123" }));
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsNonEmptyToken()
        {
            var user = TestBuilders.User(1, "Valid User", "valid@test.com");
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass123");
            _userRepo.GetByEmailAsync("valid@test.com").Returns(user);

            var result = await _sut.LoginAsync(new LoginRequest { Email = "valid@test.com", Password = "pass123" });

            Assert.NotEmpty(result.Token);
            Assert.Equal("valid@test.com", result.Email);
            Assert.Equal("Valid User", result.FullName);
            Assert.Equal("User", result.Role);
        }
    }
}

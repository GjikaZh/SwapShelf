using SwapShelf.DTOs;
using SwapShelf.Models;
using SwapShelf.Repositories;
using SwapShelf.Services.Implementations;
using SwapShelf.Tests.Helpers;

namespace SwapShelf.Tests.Services
{
    public class AdminServiceTests
    {
        private readonly IUserRepository    _userRepo    = Substitute.For<IUserRepository>();
        private readonly IListingRepository _listingRepo = Substitute.For<IListingRepository>();
        private readonly AdminService _sut;

        public AdminServiceTests()
        {
            _sut = new AdminService(_userRepo, _listingRepo);
        }

        // ── GetAllUsersAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task GetAllUsersAsync_ReturnsMappedResponses()
        {
            _userRepo.GetAllAsync()
                .Returns(new List<User>
                {
                    TestBuilders.User(1, "Alice", "alice@test.com"),
                    TestBuilders.User(2, "Bob",   "bob@test.com")
                });

            var result = (await _sut.GetAllUsersAsync()).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("Alice", result[0].FullName);
            Assert.Equal("alice@test.com", result[0].Email);
        }

        [Fact]
        public async Task GetAllUsersAsync_EmptyDb_ReturnsEmpty()
        {
            _userRepo.GetAllAsync().Returns(new List<User>());

            var result = await _sut.GetAllUsersAsync();

            Assert.Empty(result);
        }

        // ── BanUserAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task BanUserAsync_ValidUser_SetsBannedTrue()
        {
            var user = TestBuilders.User(1);
            _userRepo.GetByIdAsync(1).Returns(user);
            _userRepo.UpdateAsync(Arg.Any<User>()).Returns(ci => Task.FromResult(ci.Arg<User>()));

            await _sut.BanUserAsync(1);

            await _userRepo.Received(1).UpdateAsync(Arg.Is<User>(u => u.IsBanned == true));
        }

        [Fact]
        public async Task BanUserAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            _userRepo.GetByIdAsync(99).Returns((User?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.BanUserAsync(99));
        }

        [Fact]
        public async Task BanUserAsync_AdminUser_ThrowsInvalidOperationException()
        {
            var admin = TestBuilders.User(2);
            admin.Role = UserRole.Admin;
            _userRepo.GetByIdAsync(2).Returns(admin);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.BanUserAsync(2));
        }

        // ── UnbanUserAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task UnbanUserAsync_ValidUser_SetsBannedFalse()
        {
            var user = TestBuilders.User(1);
            user.IsBanned = true;
            _userRepo.GetByIdAsync(1).Returns(user);
            _userRepo.UpdateAsync(Arg.Any<User>()).Returns(ci => Task.FromResult(ci.Arg<User>()));

            await _sut.UnbanUserAsync(1);

            await _userRepo.Received(1).UpdateAsync(Arg.Is<User>(u => u.IsBanned == false));
        }

        [Fact]
        public async Task UnbanUserAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            _userRepo.GetByIdAsync(99).Returns((User?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UnbanUserAsync(99));
        }

        // ── GetAllListingsAsync ───────────────────────────────────────────────

        [Fact]
        public async Task GetAllListingsAsync_ReturnsMappedListingsOfAllStatuses()
        {
            var listings = new List<Listing>
            {
                TestBuilders.Listing(1, userId: 1, status: ListingStatus.Available),
                TestBuilders.Listing(2, userId: 2, status: ListingStatus.Locked),
                TestBuilders.Listing(3, userId: 1, status: ListingStatus.Swapped)
            };
            _listingRepo.GetAllForAdminAsync().Returns(listings);

            var result = (await _sut.GetAllListingsAsync()).ToList();

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllListingsAsync_EmptyDb_ReturnsEmpty()
        {
            _listingRepo.GetAllForAdminAsync().Returns(new List<Listing>());

            var result = await _sut.GetAllListingsAsync();

            Assert.Empty(result);
        }

        // ── DeleteListingAsync ────────────────────────────────────────────────

        [Fact]
        public async Task DeleteListingAsync_ValidListing_CallsDelete()
        {
            var listing = TestBuilders.Listing(1, status: ListingStatus.Available);
            _listingRepo.GetByIdAsync(1).Returns(listing);
            _listingRepo.DeleteAsync(1).Returns(Task.CompletedTask);

            await _sut.DeleteListingAsync(1);

            await _listingRepo.Received(1).DeleteAsync(1);
        }

        [Fact]
        public async Task DeleteListingAsync_NotFound_ThrowsKeyNotFoundException()
        {
            _listingRepo.GetByIdAsync(99).Returns((Listing?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteListingAsync(99));
        }

        [Fact]
        public async Task DeleteListingAsync_LockedListing_ThrowsInvalidOperationException()
        {
            var listing = TestBuilders.Listing(1, status: ListingStatus.Locked);
            _listingRepo.GetByIdAsync(1).Returns(listing);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteListingAsync(1));
        }

        [Fact]
        public async Task DeleteListingAsync_SwappedListing_CallsDelete()
        {
            // Admin CAN delete Swapped listings (only Locked is blocked)
            var listing = TestBuilders.Listing(1, status: ListingStatus.Swapped);
            _listingRepo.GetByIdAsync(1).Returns(listing);
            _listingRepo.DeleteAsync(1).Returns(Task.CompletedTask);

            await _sut.DeleteListingAsync(1);

            await _listingRepo.Received(1).DeleteAsync(1);
        }
    }
}

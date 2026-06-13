using NSubstitute;
using SwapShelf.DTOs;
using SwapShelf.Models;
using SwapShelf.Repositories;
using SwapShelf.Services.Implementations;
using SwapShelf.Tests.Helpers;

namespace SwapShelf.Tests.Services
{
    public class ListingServiceTests
    {
        private readonly IListingRepository _listingRepo = Substitute.For<IListingRepository>();
        private readonly IBookRepository _bookRepo = Substitute.For<IBookRepository>();
        private readonly ListingService _sut;

        public ListingServiceTests()
        {
            _sut = new ListingService(_listingRepo, _bookRepo);
        }

        // ── GetByUserAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task GetByUserAsync_ReturnsOnlyCurrentUsersListings()
        {
            var listings = new List<Listing>
            {
                TestBuilders.Listing(1, userId: 5),
                TestBuilders.Listing(2, userId: 5)
            };
            _listingRepo.GetByUserIdAsync(5).Returns(listings);

            var result = await _sut.GetByUserAsync(5);

            Assert.Equal(2, result.Count());
            Assert.All(result, r => Assert.Equal(5, r.UserId));
        }

        // ── CreateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_ValidRequest_ReturnsListingWithCorrectBook()
        {
            var book    = TestBuilders.Book(10);
            var listing = TestBuilders.Listing(1, userId: 3, bookId: 10);
            var request = new ListingRequest { BookId = 10, Condition = ListingCondition.Good, Location = "Skopje" };

            _bookRepo.GetByIdAsync(10).Returns(book);
            _listingRepo.CreateAsync(Arg.Any<Listing>()).Returns(listing);
            _listingRepo.GetByIdAsync(listing.Id).Returns(listing);

            var result = await _sut.CreateAsync(userId: 3, request);

            Assert.Equal(10, result.Book.Id);
            Assert.Equal(ListingStatus.Available, result.Status);
        }

        [Fact]
        public async Task CreateAsync_BookNotFound_ThrowsKeyNotFoundException()
        {
            _bookRepo.GetByIdAsync(99).Returns((Book?)null);
            var request = new ListingRequest { BookId = 99, Location = "Skopje" };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CreateAsync(1, request));
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_ValidOwner_UpdatesListing()
        {
            var listing = TestBuilders.Listing(1, userId: 5, status: ListingStatus.Available);
            _listingRepo.GetByIdAsync(1).Returns(listing);
            _listingRepo.UpdateAsync(Arg.Any<Listing>()).Returns(listing);
            var request = new ListingRequest { BookId = 1, Condition = ListingCondition.Fair, Location = "Bitola" };

            var result = await _sut.UpdateAsync(userId: 5, listingId: 1, request);

            Assert.Equal(ListingCondition.Fair, result.Condition);
        }

        [Fact]
        public async Task UpdateAsync_UserDoesNotOwnListing_ThrowsUnauthorizedAccessException()
        {
            var listing = TestBuilders.Listing(1, userId: 10);
            _listingRepo.GetByIdAsync(1).Returns(listing);
            var request = new ListingRequest { BookId = 1, Location = "Skopje" };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _sut.UpdateAsync(userId: 99, listingId: 1, request));
        }

        [Fact]
        public async Task UpdateAsync_LockedListing_ThrowsInvalidOperationException()
        {
            var listing = TestBuilders.Listing(1, userId: 5, status: ListingStatus.Locked);
            _listingRepo.GetByIdAsync(1).Returns(listing);
            var request = new ListingRequest { BookId = 1, Location = "Skopje" };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.UpdateAsync(userId: 5, listingId: 1, request));
        }

        [Fact]
        public async Task UpdateAsync_SwappedListing_ThrowsInvalidOperationException()
        {
            var listing = TestBuilders.Listing(1, userId: 5, status: ListingStatus.Swapped);
            _listingRepo.GetByIdAsync(1).Returns(listing);
            var request = new ListingRequest { BookId = 1, Location = "Skopje" };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.UpdateAsync(userId: 5, listingId: 1, request));
        }

        // ── DeleteAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_ValidOwner_CallsRepositoryDelete()
        {
            var listing = TestBuilders.Listing(1, userId: 5, status: ListingStatus.Available);
            _listingRepo.GetByIdAsync(1).Returns(listing);
            _listingRepo.DeleteAsync(1).Returns(Task.CompletedTask);

            await _sut.DeleteAsync(userId: 5, listingId: 1);

            await _listingRepo.Received(1).DeleteAsync(1);
        }

        [Fact]
        public async Task DeleteAsync_UserDoesNotOwnListing_ThrowsUnauthorizedAccessException()
        {
            var listing = TestBuilders.Listing(1, userId: 10);
            _listingRepo.GetByIdAsync(1).Returns(listing);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _sut.DeleteAsync(userId: 99, listingId: 1));
        }

        [Fact]
        public async Task DeleteAsync_LockedListing_ThrowsInvalidOperationException()
        {
            var listing = TestBuilders.Listing(1, userId: 5, status: ListingStatus.Locked);
            _listingRepo.GetByIdAsync(1).Returns(listing);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.DeleteAsync(userId: 5, listingId: 1));
        }

        [Fact]
        public async Task DeleteAsync_SwappedListing_ThrowsInvalidOperationException()
        {
            var listing = TestBuilders.Listing(1, userId: 5, status: ListingStatus.Swapped);
            _listingRepo.GetByIdAsync(1).Returns(listing);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.DeleteAsync(userId: 5, listingId: 1));
        }
    }
}

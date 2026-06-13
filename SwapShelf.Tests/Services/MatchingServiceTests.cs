using NSubstitute;
using SwapShelf.Models;
using SwapShelf.Repositories;
using SwapShelf.Services.Implementations;
using SwapShelf.Tests.Helpers;

namespace SwapShelf.Tests.Services
{
    public class MatchingServiceTests
    {
        private readonly IListingRepository    _listingRepo    = Substitute.For<IListingRepository>();
        private readonly IWantedBookRepository _wantedBookRepo = Substitute.For<IWantedBookRepository>();
        private readonly MatchingService _sut;

        public MatchingServiceTests()
        {
            _sut = new MatchingService(_listingRepo, _wantedBookRepo);
        }

        // ── Edge cases ────────────────────────────────────────────────────────

        [Fact]
        public async Task GetMatchesAsync_UserHasNoWantedBooks_ReturnsEmpty()
        {
            _listingRepo.GetAvailableByUserAsync(1)
                        .Returns(new List<Listing>());
            _wantedBookRepo.GetByUserIdAsync(1)
                           .Returns(new List<WantedBook>());

            var result = await _sut.GetMatchesAsync(userId: 1);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMatchesAsync_NobodyHasWantedBook_ReturnsEmpty()
        {
            var myListing = TestBuilders.Listing(1, userId: 1, bookId: 10);
            var myWanted  = TestBuilders.WantedBook(1, userId: 1, bookId: 20);

            _listingRepo.GetAvailableByUserAsync(1)
                        .Returns(new List<Listing> { myListing });
            _wantedBookRepo.GetByUserIdAsync(1)
                           .Returns(new List<WantedBook> { myWanted });
            // No one has a listing for book 20
            _listingRepo.GetAvailableByBookAsync(20, 1)
                        .Returns(new List<Listing>());

            var result = await _sut.GetMatchesAsync(userId: 1);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMatchesAsync_OtherUserDoesNotWantMyBooks_ReturnsEmpty()
        {
            // I (user 1) have book A, want book B
            // They (user 2) have book B but want book C (not mine)
            var bookA = TestBuilders.Book(10, "Book A");
            var bookB = TestBuilders.Book(20, "Book B");

            var myListing   = TestBuilders.Listing(1, userId: 1, bookId: 10); myListing.Book   = bookA;
            var theirListing = TestBuilders.Listing(2, userId: 2, bookId: 20); theirListing.Book = bookB;

            var myWanted    = TestBuilders.WantedBook(1, userId: 1, bookId: 20); // want book B
            var theirWanted = TestBuilders.WantedBook(2, userId: 2, bookId: 99); // want book C, not A

            _listingRepo.GetAvailableByUserAsync(1)
                        .Returns(new List<Listing> { myListing });
            _wantedBookRepo.GetByUserIdAsync(1)
                           .Returns(new List<WantedBook> { myWanted });
            _listingRepo.GetAvailableByBookAsync(20, 1)
                        .Returns(new List<Listing> { theirListing });
            _wantedBookRepo.GetByUserIdAsync(2)
                           .Returns(new List<WantedBook> { theirWanted });

            var result = await _sut.GetMatchesAsync(userId: 1);

            Assert.Empty(result);
        }

        // ── Happy path ────────────────────────────────────────────────────────

        [Fact]
        public async Task GetMatchesAsync_MutualInterest_ReturnsSingleMatch()
        {
            // I (user 1) have book A (10), want book B (20)
            // They (user 2) have book B (20), want book A (10)  ← perfect mutual match
            var bookA = TestBuilders.Book(10, "Book A");
            var bookB = TestBuilders.Book(20, "Book B");

            var myListing    = TestBuilders.Listing(1, userId: 1, bookId: 10); myListing.Book    = bookA;
            var theirListing = TestBuilders.Listing(2, userId: 2, bookId: 20); theirListing.Book  = bookB;
            theirListing.User = TestBuilders.User(2, "Other User");

            var myWanted    = TestBuilders.WantedBook(1, userId: 1, bookId: 20); // I want book B
            var theirWanted = TestBuilders.WantedBook(2, userId: 2, bookId: 10); // They want book A

            _listingRepo.GetAvailableByUserAsync(1)
                        .Returns(new List<Listing> { myListing });
            _wantedBookRepo.GetByUserIdAsync(1)
                           .Returns(new List<WantedBook> { myWanted });
            _listingRepo.GetAvailableByBookAsync(20, 1)
                        .Returns(new List<Listing> { theirListing });
            _wantedBookRepo.GetByUserIdAsync(2)
                           .Returns(new List<WantedBook> { theirWanted });

            var result = (await _sut.GetMatchesAsync(userId: 1)).ToList();

            Assert.Single(result);
            Assert.Equal(2, result[0].TheirUserId);
            Assert.Equal(theirListing.Id, result[0].TheirListing.Id);
            Assert.Single(result[0].MyMatchingListings);
            Assert.Equal(myListing.Id, result[0].MyMatchingListings[0].Id);
        }

        [Fact]
        public async Task GetMatchesAsync_SameUserAppearsMultipleTimes_DeduplicatedByListing()
        {
            // I want two books, user 2 has both.
            // User 2 wants my one book.
            // Should produce two separate match entries (one per their listing).
            var bookA = TestBuilders.Book(10, "Book A");
            var bookB = TestBuilders.Book(20, "Book B");
            var bookC = TestBuilders.Book(30, "Book C");

            var myListing     = TestBuilders.Listing(1, userId: 1, bookId: 10); myListing.Book     = bookA;
            var theirListing1 = TestBuilders.Listing(2, userId: 2, bookId: 20); theirListing1.Book  = bookB;
            var theirListing2 = TestBuilders.Listing(3, userId: 2, bookId: 30); theirListing2.Book  = bookC;
            theirListing1.User = TestBuilders.User(2, "User 2");
            theirListing2.User = TestBuilders.User(2, "User 2");

            var myWanted1  = TestBuilders.WantedBook(1, userId: 1, bookId: 20);
            var myWanted2  = TestBuilders.WantedBook(2, userId: 1, bookId: 30);
            var theirWanted = TestBuilders.WantedBook(3, userId: 2, bookId: 10); // they want book A

            _listingRepo.GetAvailableByUserAsync(1)
                        .Returns(new List<Listing> { myListing });
            _wantedBookRepo.GetByUserIdAsync(1)
                           .Returns(new List<WantedBook> { myWanted1, myWanted2 });
            _listingRepo.GetAvailableByBookAsync(20, 1)
                        .Returns(new List<Listing> { theirListing1 });
            _listingRepo.GetAvailableByBookAsync(30, 1)
                        .Returns(new List<Listing> { theirListing2 });
            _wantedBookRepo.GetByUserIdAsync(2)
                           .Returns(new List<WantedBook> { theirWanted });

            var result = (await _sut.GetMatchesAsync(userId: 1)).ToList();

            // Two distinct listing matches (deduplicated per listing, not per user)
            Assert.Equal(2, result.Count);
            Assert.All(result, m => Assert.Equal(2, m.TheirUserId));
        }
    }
}

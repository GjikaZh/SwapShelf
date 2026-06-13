using NSubstitute;
using SwapShelf.DTOs;
using SwapShelf.Models;
using SwapShelf.Repositories;
using SwapShelf.Services.Implementations;
using SwapShelf.Tests.Helpers;

namespace SwapShelf.Tests.Services
{
    public class WantedBookServiceTests
    {
        private readonly IWantedBookRepository _wantedRepo = Substitute.For<IWantedBookRepository>();
        private readonly IBookRepository _bookRepo = Substitute.For<IBookRepository>();
        private readonly WantedBookService _sut;

        public WantedBookServiceTests()
        {
            _sut = new WantedBookService(_wantedRepo, _bookRepo);
        }

        // ── GetByUserAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task GetByUserAsync_ReturnsOnlyCurrentUsersWantedBooks()
        {
            var wanted = new List<WantedBook>
            {
                TestBuilders.WantedBook(1, userId: 7, bookId: 1),
                TestBuilders.WantedBook(2, userId: 7, bookId: 2)
            };
            _wantedRepo.GetByUserIdAsync(7).Returns(wanted);

            var result = await _sut.GetByUserAsync(7);

            Assert.Equal(2, result.Count());
        }

        // ── AddAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task AddAsync_BookNotFound_ThrowsKeyNotFoundException()
        {
            _bookRepo.GetByIdAsync(99).Returns((Book?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _sut.AddAsync(1, new WantedBookRequest { BookId = 99 }));
        }

        [Fact]
        public async Task AddAsync_BookAlreadyOnWantedList_ThrowsInvalidOperationException()
        {
            _bookRepo.GetByIdAsync(5).Returns(TestBuilders.Book(5));
            _wantedRepo.ExistsAsync(1, 5).Returns(true);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.AddAsync(1, new WantedBookRequest { BookId = 5 }));
        }

        [Fact]
        public async Task AddAsync_ValidRequest_CreatesWantedBookScopedToUser()
        {
            var book   = TestBuilders.Book(5);
            var wanted = TestBuilders.WantedBook(1, userId: 1, bookId: 5);

            _bookRepo.GetByIdAsync(5).Returns(book);
            _wantedRepo.ExistsAsync(1, 5).Returns(false);
            _wantedRepo.CreateAsync(Arg.Any<WantedBook>()).Returns(wanted);
            _wantedRepo.GetByIdAsync(wanted.Id).Returns(wanted);

            var result = await _sut.AddAsync(userId: 1, new WantedBookRequest { BookId = 5 });

            Assert.Equal(5, result.Book.Id);
            await _wantedRepo.Received(1).CreateAsync(Arg.Is<WantedBook>(w => w.UserId == 1 && w.BookId == 5));
        }

        // ── RemoveAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task RemoveAsync_WantedBookNotFound_ThrowsKeyNotFoundException()
        {
            _wantedRepo.GetByIdAsync(99).Returns((WantedBook?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _sut.RemoveAsync(userId: 1, wantedBookId: 99));
        }

        [Fact]
        public async Task RemoveAsync_UserDoesNotOwnEntry_ThrowsUnauthorizedAccessException()
        {
            var wanted = TestBuilders.WantedBook(1, userId: 10); // owned by user 10
            _wantedRepo.GetByIdAsync(1).Returns(wanted);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _sut.RemoveAsync(userId: 99, wantedBookId: 1));
        }

        [Fact]
        public async Task RemoveAsync_ValidOwner_CallsRepositoryDelete()
        {
            var wanted = TestBuilders.WantedBook(1, userId: 5);
            _wantedRepo.GetByIdAsync(1).Returns(wanted);
            _wantedRepo.DeleteAsync(1).Returns(Task.CompletedTask);

            await _sut.RemoveAsync(userId: 5, wantedBookId: 1);

            await _wantedRepo.Received(1).DeleteAsync(1);
        }
    }
}

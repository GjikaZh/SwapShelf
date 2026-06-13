using SwapShelf.DTOs;
using SwapShelf.Models;
using SwapShelf.Repositories;
using SwapShelf.Services.Implementations;
using SwapShelf.Tests.Helpers;

namespace SwapShelf.Tests.Services
{
    public class BookServiceTests
    {
        private readonly IBookRepository _bookRepo = Substitute.For<IBookRepository>();
        private readonly BookService _sut;

        public BookServiceTests()
        {
            _sut = new BookService(_bookRepo);
        }

        // ── GetAllAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_ReturnsMappedResponses()
        {
            _bookRepo.GetAllAsync()
                .Returns(new List<Book>
                {
                    TestBuilders.Book(1, "Book A", "Author A"),
                    TestBuilders.Book(2, "Book B", "Author B")
                });

            var result = (await _sut.GetAllAsync()).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("Book A", result[0].Title);
            Assert.Equal("Book B", result[1].Title);
        }

        [Fact]
        public async Task GetAllAsync_EmptyCatalog_ReturnsEmpty()
        {
            _bookRepo.GetAllAsync().Returns(new List<Book>());

            var result = await _sut.GetAllAsync();

            Assert.Empty(result);
        }

        // ── GetByIdAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_ExistingBook_ReturnsResponse()
        {
            _bookRepo.GetByIdAsync(5)
                .Returns(TestBuilders.Book(5, "Found Book", "Found Author"));

            var result = await _sut.GetByIdAsync(5);

            Assert.Equal(5, result.Id);
            Assert.Equal("Found Book", result.Title);
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ThrowsKeyNotFoundException()
        {
            _bookRepo.GetByIdAsync(99).Returns((Book?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(99));
        }

        // ── CreateAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_NewBook_ReturnsCreatedResponse()
        {
            var request = new BookRequest { Title = "New Book", Author = "New Author", Genre = "Fiction" };
            var created  = TestBuilders.Book(1, "New Book", "New Author");

            _bookRepo.GetByTitleAndAuthorAsync("New Book", "New Author")
                .Returns((Book?)null);
            _bookRepo.CreateAsync(Arg.Any<Book>())
                .Returns(created);

            var result = await _sut.CreateAsync(request);

            Assert.Equal(1, result.Id);
            Assert.Equal("New Book", result.Title);
        }

        [Fact]
        public async Task CreateAsync_DuplicateTitleAndAuthor_ThrowsInvalidOperationException()
        {
            var request = new BookRequest { Title = "Existing", Author = "Author", Genre = "Fiction" };
            _bookRepo.GetByTitleAndAuthorAsync("Existing", "Author")
                .Returns(TestBuilders.Book(1, "Existing", "Author"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_PersistsAllFields()
        {
            var request = new BookRequest
            {
                Title  = "Test Title",
                Author = "Test Author",
                Genre  = "Mystery",
                ISBN   = "978-3-16-148410-0"
            };

            _bookRepo.GetByTitleAndAuthorAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns((Book?)null);
            _bookRepo.CreateAsync(Arg.Any<Book>())
                .Returns(new Book
                {
                    Id     = 1,
                    Title  = "Test Title",
                    Author = "Test Author",
                    Genre  = "Mystery",
                    ISBN   = "978-3-16-148410-0"
                });

            var result = await _sut.CreateAsync(request);

            Assert.Equal("Mystery", result.Genre);
            Assert.Equal("978-3-16-148410-0", result.ISBN);
        }

        [Fact]
        public async Task CreateAsync_NullIsbn_IsAllowed()
        {
            var request = new BookRequest { Title = "No ISBN", Author = "Author", Genre = "Fiction", ISBN = null };
            var created  = new Book { Id = 1, Title = "No ISBN", Author = "Author", Genre = "Fiction", ISBN = null };

            _bookRepo.GetByTitleAndAuthorAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns((Book?)null);
            _bookRepo.CreateAsync(Arg.Any<Book>()).Returns(created);

            var result = await _sut.CreateAsync(request);

            Assert.Null(result.ISBN);
        }
    }
}

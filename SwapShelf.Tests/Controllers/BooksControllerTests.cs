using Microsoft.AspNetCore.Mvc;
using SwapShelf.Controllers;
using SwapShelf.DTOs;
using SwapShelf.Services.Interfaces;
using SwapShelf.Tests.Helpers;

namespace SwapShelf.Tests.Controllers
{
    public class BooksControllerTests
    {
        private readonly IBookService _bookServiceMock = Substitute.For<IBookService>();
        private readonly BooksController _controller;

        public BooksControllerTests()
        {
            _controller = new BooksController(_bookServiceMock);
        }

        // ── GetAll ────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAll_ReturnsOkWithBooks()
        {
            _bookServiceMock
                .GetAllAsync()
                .Returns(new List<BookResponse>
                {
                    new() { Id = 1, Title = "Book One", Author = "Author A", Genre = "Fiction" },
                    new() { Id = 2, Title = "Book Two", Author = "Author B", Genre = "Non-Fiction" }
                });

            var result = await _controller.GetAll() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task GetAll_EmptyCatalog_ReturnsOkWithEmptyList()
        {
            _bookServiceMock
                .GetAllAsync()
                .Returns(new List<BookResponse>());

            var result = await _controller.GetAll() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        // ── GetById ───────────────────────────────────────────────────────────

        [Fact]
        public async Task GetById_ExistingId_ReturnsOk()
        {
            _bookServiceMock
                .GetByIdAsync(1)
                .Returns(new BookResponse { Id = 1, Title = "Book One", Author = "Author A", Genre = "Fiction" });

            var result = await _controller.GetById(1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task GetById_NonExistingId_Returns404()
        {
            _bookServiceMock
                .GetByIdAsync(99)
                .Throws(new KeyNotFoundException("Book 99 not found."));

            var result = await _controller.GetById(99) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        // ── Create ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Create_NewBook_Returns201()
        {
            var request = new BookRequest { Title = "New Book", Author = "Author A", Genre = "Fiction" };
            _bookServiceMock
                .CreateAsync(request)
                .Returns(new BookResponse { Id = 1, Title = "New Book", Author = "Author A", Genre = "Fiction" });

            ControllerTestHelper.SetUser(_controller, userId: 1);

            var result = await _controller.Create(request) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
        }

        [Fact]
        public async Task Create_DuplicateBook_Returns400()
        {
            var request = new BookRequest { Title = "Existing Book", Author = "Author A", Genre = "Fiction" };
            _bookServiceMock
                .CreateAsync(request)
                .Throws(new InvalidOperationException("This book already exists in the catalog."));

            ControllerTestHelper.SetUser(_controller, userId: 1);

            var result = await _controller.Create(request) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using SwapShelf.Controllers;
using SwapShelf.DTOs;
using SwapShelf.Services.Interfaces;
using SwapShelf.Tests.Helpers;

namespace SwapShelf.Tests.Controllers
{
    public class WantedBooksControllerTests
    {
        private readonly Mock<IWantedBookService> _mockWantedBookService;
        private readonly WantedBooksController _controller;

        private static readonly WantedBookResponse _stubWanted = new WantedBookResponse
        {
            Id = 1,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };

        private static readonly WantedBookRequest _stubRequest = new WantedBookRequest
        {
            BookId = 10
        };

        public WantedBooksControllerTests()
        {
            _mockWantedBookService = new Mock<IWantedBookService>();
            _controller = new WantedBooksController(_mockWantedBookService.Object);
            ControllerTestHelper.SetUser(_controller, 1);
        }

        [Fact]
        public async Task GetMyWanted_ReturnsOk()
        {
            var wanted = new List<WantedBookResponse> { _stubWanted };
            _mockWantedBookService.Setup(s => s.GetByUserAsync(1)).ReturnsAsync(wanted);

            var result = await _controller.GetMyWanted() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task Add_ValidRequest_Returns201()
        {
            _mockWantedBookService.Setup(s => s.AddAsync(1, _stubRequest)).ReturnsAsync(_stubWanted);

            var result = await _controller.Add(_stubRequest) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
        }

        [Fact]
        public async Task Add_BookNotFound_Returns404()
        {
            _mockWantedBookService.Setup(s => s.AddAsync(1, _stubRequest))
                .ThrowsAsync(new KeyNotFoundException("Book not found"));

            var result = await _controller.Add(_stubRequest) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task Add_AlreadyOnList_Returns400()
        {
            _mockWantedBookService.Setup(s => s.AddAsync(1, _stubRequest))
                .ThrowsAsync(new InvalidOperationException("Book is already on wanted list"));

            var result = await _controller.Add(_stubRequest) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task Remove_Valid_Returns204()
        {
            _mockWantedBookService.Setup(s => s.RemoveAsync(1, 1)).Returns(Task.CompletedTask);

            var result = await _controller.Remove(1) as NoContentResult;

            Assert.NotNull(result);
            Assert.Equal(204, result.StatusCode);
        }

        [Fact]
        public async Task Remove_NotFound_Returns404()
        {
            _mockWantedBookService.Setup(s => s.RemoveAsync(1, 99))
                .ThrowsAsync(new KeyNotFoundException("Wanted book not found"));

            var result = await _controller.Remove(99) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task Remove_NotOwner_ReturnsForbid()
        {
            _mockWantedBookService.Setup(s => s.RemoveAsync(1, 1))
                .ThrowsAsync(new UnauthorizedAccessException());

            var result = await _controller.Remove(1) as ForbidResult;

            Assert.NotNull(result);
        }
    }
}

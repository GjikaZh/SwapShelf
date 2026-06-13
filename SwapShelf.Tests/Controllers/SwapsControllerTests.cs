using Microsoft.AspNetCore.Mvc;
using SwapShelf.Controllers;
using SwapShelf.DTOs;
using SwapShelf.Services.Interfaces;
using SwapShelf.Tests.Helpers;

namespace SwapShelf.Tests.Controllers
{
    public class SwapsControllerTests
    {
        private readonly Mock<ISwapService> _mockSwapService;
        private readonly SwapsController _controller;

        private static readonly SwapRequestResponse _stubSwap = new SwapRequestResponse
        {
            Id = 1,
            InitiatorId = 1,
            ReceiverId = 2
        };

        private static readonly SwapRequestCreate _stubCreate = new SwapRequestCreate
        {
            InitiatorListingId = 1,
            ReceiverListingId = 2
        };

        public SwapsControllerTests()
        {
            _mockSwapService = new Mock<ISwapService>();
            _controller = new SwapsController(_mockSwapService.Object);
            ControllerTestHelper.SetUser(_controller, 1);
        }

        [Fact]
        public async Task GetMySwaps_ReturnsOkWithSwaps()
        {
            var swaps = new List<SwapRequestResponse> { _stubSwap };
            _mockSwapService.Setup(s => s.GetByUserAsync(1)).ReturnsAsync(swaps);

            var result = await _controller.GetMySwaps() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task GetById_Found_ReturnsOk()
        {
            _mockSwapService.Setup(s => s.GetByIdAsync(1, 1)).ReturnsAsync(_stubSwap);

            var result = await _controller.GetById(1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task GetById_NotFound_Returns404()
        {
            _mockSwapService.Setup(s => s.GetByIdAsync(99, 1))
                .ThrowsAsync(new KeyNotFoundException("Swap not found"));

            var result = await _controller.GetById(99) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetById_Unauthorized_ReturnsForbid()
        {
            _mockSwapService.Setup(s => s.GetByIdAsync(1, 1))
                .ThrowsAsync(new UnauthorizedAccessException());

            var result = await _controller.GetById(1) as ForbidResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Create_ValidRequest_Returns201()
        {
            _mockSwapService.Setup(s => s.CreateAsync(1, _stubCreate)).ReturnsAsync(_stubSwap);

            var result = await _controller.Create(_stubCreate) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
        }

        [Fact]
        public async Task Create_ListingNotFound_Returns404()
        {
            _mockSwapService.Setup(s => s.CreateAsync(1, _stubCreate))
                .ThrowsAsync(new KeyNotFoundException("Listing not found"));

            var result = await _controller.Create(_stubCreate) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task Create_NotOwner_ReturnsForbid()
        {
            _mockSwapService.Setup(s => s.CreateAsync(1, _stubCreate))
                .ThrowsAsync(new UnauthorizedAccessException());

            var result = await _controller.Create(_stubCreate) as ForbidResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Create_InvalidRequest_Returns400()
        {
            _mockSwapService.Setup(s => s.CreateAsync(1, _stubCreate))
                .ThrowsAsync(new InvalidOperationException("Invalid request"));

            var result = await _controller.Create(_stubCreate) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task Accept_Valid_ReturnsOk()
        {
            _mockSwapService.Setup(s => s.AcceptAsync(1, 1)).ReturnsAsync(_stubSwap);

            var result = await _controller.Accept(1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task Accept_NotFound_Returns404()
        {
            _mockSwapService.Setup(s => s.AcceptAsync(99, 1))
                .ThrowsAsync(new KeyNotFoundException("Swap not found"));

            var result = await _controller.Accept(99) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task Accept_NotReceiver_ReturnsForbid()
        {
            _mockSwapService.Setup(s => s.AcceptAsync(1, 1))
                .ThrowsAsync(new UnauthorizedAccessException());

            var result = await _controller.Accept(1) as ForbidResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Accept_NotPending_Returns400()
        {
            _mockSwapService.Setup(s => s.AcceptAsync(1, 1))
                .ThrowsAsync(new InvalidOperationException("Swap is not in Pending state"));

            var result = await _controller.Accept(1) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task Reject_Valid_ReturnsOk()
        {
            _mockSwapService.Setup(s => s.RejectAsync(1, 1)).ReturnsAsync(_stubSwap);

            var result = await _controller.Reject(1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task MarkInTransit_Valid_ReturnsOk()
        {
            _mockSwapService.Setup(s => s.MarkInTransitAsync(1, 1)).ReturnsAsync(_stubSwap);

            var result = await _controller.MarkInTransit(1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task MarkInTransit_NotAccepted_Returns400()
        {
            _mockSwapService.Setup(s => s.MarkInTransitAsync(1, 1))
                .ThrowsAsync(new InvalidOperationException("Swap is not in Accepted state"));

            var result = await _controller.MarkInTransit(1) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task Complete_Valid_ReturnsOk()
        {
            _mockSwapService.Setup(s => s.CompleteAsync(1, 1)).ReturnsAsync(_stubSwap);

            var result = await _controller.Complete(1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task Complete_NotInTransit_Returns400()
        {
            _mockSwapService.Setup(s => s.CompleteAsync(1, 1))
                .ThrowsAsync(new InvalidOperationException("Swap is not in InTransit state"));

            var result = await _controller.Complete(1) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task Cancel_Valid_ReturnsOk()
        {
            _mockSwapService.Setup(s => s.CancelAsync(1, 1)).ReturnsAsync(_stubSwap);

            var result = await _controller.Cancel(1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task Cancel_AlreadyCompleted_Returns400()
        {
            _mockSwapService.Setup(s => s.CancelAsync(1, 1))
                .ThrowsAsync(new InvalidOperationException("Cannot cancel a completed swap"));

            var result = await _controller.Cancel(1) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }
    }
}

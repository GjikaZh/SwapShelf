using Microsoft.AspNetCore.Mvc;
using SwapShelf.Controllers;
using SwapShelf.DTOs;
using SwapShelf.Services.Interfaces;
using SwapShelf.Tests.Helpers;

namespace SwapShelf.Tests.Controllers
{
    public class ReviewsControllerTests
    {
        private readonly IReviewService _mockReviewService;
        private readonly ReviewsController _controller;

        private static readonly ReviewResponse _stubReview = new ReviewResponse
        {
            Id = 1,
            SwapRequestId = 5,
            ReviewerId = 1,
            ReviewerName = "Alice",
            RevieweeId = 2,
            RevieweeName = "Bob",
            Rating = 5,
            Comment = "Great swap!",
            CreatedAt = DateTime.UtcNow
        };

        private static readonly ReviewRequest _stubRequest = new ReviewRequest
        {
            SwapRequestId = 5,
            RevieweeId = 2,
            Rating = 5,
            Comment = "Great swap!"
        };

        public ReviewsControllerTests()
        {
            _mockReviewService = Substitute.For<IReviewService>();
            _controller = new ReviewsController(_mockReviewService);
        }

        [Fact]
        public async Task GetByUser_ReturnsOk()
        {
            var reviews = new List<ReviewResponse> { _stubReview };
            _mockReviewService.GetByUserAsync(2).Returns(reviews);

            var result = await _controller.GetByUser(2) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task Create_ValidRequest_Returns201()
        {
            ControllerTestHelper.SetUser(_controller, 1);
            _mockReviewService.CreateAsync(1, _stubRequest).Returns(_stubReview);

            var result = await _controller.Create(_stubRequest) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
        }

        [Fact]
        public async Task Create_SwapNotFound_Returns404()
        {
            ControllerTestHelper.SetUser(_controller, 1);
            _mockReviewService.CreateAsync(1, _stubRequest)
                .Throws(new KeyNotFoundException("Swap not found"));

            var result = await _controller.Create(_stubRequest) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task Create_NotParticipant_ReturnsForbid()
        {
            ControllerTestHelper.SetUser(_controller, 1);
            _mockReviewService.CreateAsync(1, _stubRequest)
                .Throws(new UnauthorizedAccessException());

            var result = await _controller.Create(_stubRequest) as ForbidResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Create_InvalidRating_Returns400()
        {
            ControllerTestHelper.SetUser(_controller, 1);
            _mockReviewService.CreateAsync(1, _stubRequest)
                .Throws(new InvalidOperationException("Rating must be between 1 and 5"));

            var result = await _controller.Create(_stubRequest) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }
    }
}

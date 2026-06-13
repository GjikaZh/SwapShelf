using NSubstitute;
using SwapShelf.DTOs;
using SwapShelf.Models;
using SwapShelf.Repositories;
using SwapShelf.Services.Implementations;
using SwapShelf.Tests.Helpers;

namespace SwapShelf.Tests.Services
{
    public class ReviewServiceTests
    {
        private readonly IReviewRepository _reviewRepo = Substitute.For<IReviewRepository>();
        private readonly ISwapRepository   _swapRepo   = Substitute.For<ISwapRepository>();
        private readonly IUserRepository   _userRepo   = Substitute.For<IUserRepository>();
        private readonly ReviewService _sut;

        public ReviewServiceTests()
        {
            _sut = new ReviewService(_reviewRepo, _swapRepo, _userRepo);
        }

        /// Convenience — a completed swap between user 1 (initiator) and user 2 (receiver).
        private SwapRequest CompletedSwap() =>
            TestBuilders.SwapRequest(1, initiatorId: 1, receiverId: 2, status: SwapStatus.Completed);

        // ── Rating validation ─────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_RatingZero_ThrowsInvalidOperationException()
        {
            var request = new ReviewRequest { SwapRequestId = 1, RevieweeId = 2, Rating = 0, Comment = "x" };
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(reviewerId: 1, request));
        }

        [Fact]
        public async Task CreateAsync_RatingSix_ThrowsInvalidOperationException()
        {
            var request = new ReviewRequest { SwapRequestId = 1, RevieweeId = 2, Rating = 6, Comment = "x" };
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(reviewerId: 1, request));
        }

        // ── Swap validation ───────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_SwapNotFound_ThrowsKeyNotFoundException()
        {
            _swapRepo.GetByIdAsync(99).Returns((SwapRequest?)null);
            var request = new ReviewRequest { SwapRequestId = 99, RevieweeId = 2, Rating = 4, Comment = "x" };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CreateAsync(reviewerId: 1, request));
        }

        [Fact]
        public async Task CreateAsync_SwapNotCompleted_ThrowsInvalidOperationException()
        {
            var swap = TestBuilders.SwapRequest(1, initiatorId: 1, receiverId: 2, status: SwapStatus.InTransit);
            _swapRepo.GetByIdAsync(1).Returns(swap);
            var request = new ReviewRequest { SwapRequestId = 1, RevieweeId = 2, Rating = 4, Comment = "x" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(reviewerId: 1, request));
        }

        // ── Participant validation ────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_ReviewerNotInSwap_ThrowsUnauthorizedAccessException()
        {
            _swapRepo.GetByIdAsync(1).Returns(CompletedSwap());
            var request = new ReviewRequest { SwapRequestId = 1, RevieweeId = 2, Rating = 4, Comment = "x" };

            // User 99 is not part of this swap
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.CreateAsync(reviewerId: 99, request));
        }

        [Fact]
        public async Task CreateAsync_RevieweeNotInSwap_ThrowsInvalidOperationException()
        {
            _swapRepo.GetByIdAsync(1).Returns(CompletedSwap());
            // Reviewee 99 was not part of the swap
            var request = new ReviewRequest { SwapRequestId = 1, RevieweeId = 99, Rating = 4, Comment = "x" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(reviewerId: 1, request));
        }

        [Fact]
        public async Task CreateAsync_ReviewingYourself_ThrowsInvalidOperationException()
        {
            _swapRepo.GetByIdAsync(1).Returns(CompletedSwap());
            // Reviewer and reviewee are the same person
            var request = new ReviewRequest { SwapRequestId = 1, RevieweeId = 1, Rating = 5, Comment = "x" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(reviewerId: 1, request));
        }

        [Fact]
        public async Task CreateAsync_AlreadyReviewedThisSwap_ThrowsInvalidOperationException()
        {
            _swapRepo.GetByIdAsync(1).Returns(CompletedSwap());
            _reviewRepo.ExistsAsync(1, 1).Returns(true);
            var request = new ReviewRequest { SwapRequestId = 1, RevieweeId = 2, Rating = 5, Comment = "x" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(reviewerId: 1, request));
        }

        // ── Happy path ────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_ValidRequest_CreatesReviewAndRecalculatesTrustScore()
        {
            var reviewee = TestBuilders.User(2);
            var review   = TestBuilders.Review(1, swapId: 1, reviewerId: 1, revieweeId: 2, rating: 4);

            _swapRepo.GetByIdAsync(1).Returns(CompletedSwap());
            _reviewRepo.ExistsAsync(1, 1).Returns(false);
            _reviewRepo.CreateAsync(Arg.Any<Review>()).Returns(review);
            // Both GetByRevieweeIdAsync calls return the same list (including the new review)
            _reviewRepo.GetByRevieweeIdAsync(2)
                       .Returns(new List<Review> { review });
            _userRepo.GetByIdAsync(2).Returns(reviewee);
            _userRepo.UpdateAsync(Arg.Any<User>()).Returns(ci => Task.FromResult(ci.Arg<User>()));

            var request = new ReviewRequest { SwapRequestId = 1, RevieweeId = 2, Rating = 4, Comment = "Great swap!" };
            await _sut.CreateAsync(reviewerId: 1, request);

            // Trust score should equal the average of the returned reviews (4.0)
            await _userRepo.Received(1).UpdateAsync(Arg.Is<User>(u => u.Id == 2 && u.TrustScore == 4.0));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public async Task CreateAsync_BoundaryRatings_DoNotThrow(int rating)
        {
            var reviewee = TestBuilders.User(2);
            var review   = TestBuilders.Review(1, swapId: 1, reviewerId: 1, revieweeId: 2, rating: rating);

            _swapRepo.GetByIdAsync(1).Returns(CompletedSwap());
            _reviewRepo.ExistsAsync(1, 1).Returns(false);
            _reviewRepo.CreateAsync(Arg.Any<Review>()).Returns(review);
            _reviewRepo.GetByRevieweeIdAsync(2)
                       .Returns(new List<Review> { review });
            _userRepo.GetByIdAsync(2).Returns(reviewee);
            _userRepo.UpdateAsync(Arg.Any<User>()).Returns(ci => Task.FromResult(ci.Arg<User>()));

            var request = new ReviewRequest { SwapRequestId = 1, RevieweeId = 2, Rating = rating, Comment = "ok" };

            // Should not throw
            await _sut.CreateAsync(reviewerId: 1, request);
        }
    }
}

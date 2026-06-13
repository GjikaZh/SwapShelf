using SwapShelf.DTOs;
using SwapShelf.Models;
using SwapShelf.Repositories;
using SwapShelf.Services.Interfaces;

namespace SwapShelf.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ISwapRepository _swapRepository;
        private readonly IUserRepository _userRepository;

        public ReviewService(IReviewRepository reviewRepository, ISwapRepository swapRepository, IUserRepository userRepository)
        {
            _reviewRepository = reviewRepository;
            _swapRepository = swapRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<ReviewResponse>> GetByUserAsync(int userId)
        {
            var reviews = await _reviewRepository.GetByRevieweeIdAsync(userId);
            return reviews.Select(MapToResponse);
        }

        public async Task<ReviewResponse> CreateAsync(int reviewerId, ReviewRequest request)
        {
            if (request.Rating < 1 || request.Rating > 5)
                throw new InvalidOperationException("Rating must be between 1 and 5.");

            var swap = await _swapRepository.GetByIdAsync(request.SwapRequestId)
                ?? throw new KeyNotFoundException("Swap not found.");

            if (swap.Status != SwapStatus.Completed)
                throw new InvalidOperationException("You can only review after a swap is completed.");

            if (swap.InitiatorId != reviewerId && swap.ReceiverId != reviewerId)
                throw new UnauthorizedAccessException("You were not part of this swap.");

            if (request.RevieweeId != swap.InitiatorId && request.RevieweeId != swap.ReceiverId)
                throw new InvalidOperationException("Reviewee was not part of this swap.");

            if (request.RevieweeId == reviewerId)
                throw new InvalidOperationException("You cannot review yourself.");

            var alreadyReviewed = await _reviewRepository.ExistsAsync(request.SwapRequestId, reviewerId);
            if (alreadyReviewed)
                throw new InvalidOperationException("You have already reviewed this swap.");

            var review = new Review
            {
                SwapRequestId = request.SwapRequestId,
                ReviewerId = reviewerId,
                RevieweeId = request.RevieweeId,
                Rating = request.Rating,
                Comment = request.Comment
            };

            await _reviewRepository.CreateAsync(review);

            // Recalculate trust score for the reviewee
            var allReviews = await _reviewRepository.GetByRevieweeIdAsync(request.RevieweeId);
            var reviewee = await _userRepository.GetByIdAsync(request.RevieweeId);
            if (reviewee != null)
            {
                reviewee.TrustScore = allReviews.Average(r => r.Rating);
                await _userRepository.UpdateAsync(reviewee);
            }

            var full = await _reviewRepository.GetByRevieweeIdAsync(request.RevieweeId);
            var created = full.OrderByDescending(r => r.CreatedAt).First();
            return MapToResponse(created);
        }

        private static ReviewResponse MapToResponse(Review r) => new()
        {
            Id = r.Id,
            SwapRequestId = r.SwapRequestId,
            ReviewerId = r.ReviewerId,
            ReviewerName = r.Reviewer?.FullName ?? string.Empty,
            RevieweeId = r.RevieweeId,
            RevieweeName = r.Reviewee?.FullName ?? string.Empty,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        };
    }
}
using SwapShelf.DTOs;

namespace SwapShelf.Services.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewResponse>> GetByUserAsync(int userId);
        Task<ReviewResponse> CreateAsync(int reviewerId, ReviewRequest request);
    }
}
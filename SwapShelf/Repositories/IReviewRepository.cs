using SwapShelf.Models;

namespace SwapShelf.Repositories
{
    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetByRevieweeIdAsync(int revieweeId);
        Task<bool> ExistsAsync(int swapRequestId, int reviewerId);
        Task<Review> CreateAsync(Review review);
    }
}
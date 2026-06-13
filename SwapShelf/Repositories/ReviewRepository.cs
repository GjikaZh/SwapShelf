using Microsoft.EntityFrameworkCore;
using SwapShelf.Data;
using SwapShelf.Models;

namespace SwapShelf.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Review>> GetByRevieweeIdAsync(int revieweeId)
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .Include(r => r.SwapRequest)
                .Where(r => r.RevieweeId == revieweeId)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int swapRequestId, int reviewerId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.SwapRequestId == swapRequestId && r.ReviewerId == reviewerId);
        }

        public async Task<Review> CreateAsync(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }
    }
}
using Microsoft.EntityFrameworkCore;
using SwapShelf.Data;
using SwapShelf.Models;

namespace SwapShelf.Repositories
{
    public class SwapRepository : ISwapRepository
    {
        private readonly AppDbContext _context;

        public SwapRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SwapRequest>> GetByUserIdAsync(int userId)
        {
            return await _context.SwapRequests
                .Include(s => s.Initiator)
                .Include(s => s.Receiver)
                .Include(s => s.InitiatorListing).ThenInclude(l => l.Book)
                .Include(s => s.ReceiverListing).ThenInclude(l => l.Book)
                .Include(s => s.Reviews)
                .Where(s => s.InitiatorId == userId || s.ReceiverId == userId)
                .ToListAsync();
        }

        public async Task<SwapRequest?> GetByIdAsync(int id)
        {
            return await _context.SwapRequests
                .Include(s => s.Initiator)
                .Include(s => s.Receiver)
                .Include(s => s.InitiatorListing).ThenInclude(l => l.Book)
                .Include(s => s.ReceiverListing).ThenInclude(l => l.Book)
                .Include(s => s.Reviews)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> HasActiveSwapForListingAsync(int listingId)
        {
            return await _context.SwapRequests
                .AnyAsync(s =>
                    (s.InitiatorListingId == listingId || s.ReceiverListingId == listingId) &&
                    (s.Status == SwapStatus.Pending || s.Status == SwapStatus.Accepted));
        }

        public async Task<SwapRequest> CreateAsync(SwapRequest swapRequest)
        {
            _context.SwapRequests.Add(swapRequest);
            await _context.SaveChangesAsync();
            return swapRequest;
        }

        public async Task<SwapRequest> UpdateAsync(SwapRequest swapRequest)
        {
            _context.SwapRequests.Update(swapRequest);
            await _context.SaveChangesAsync();
            return swapRequest;
        }
    }
}
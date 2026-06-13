using SwapShelf.Models;

namespace SwapShelf.Repositories
{
    public interface ISwapRepository
    {
        Task<IEnumerable<SwapRequest>> GetByUserIdAsync(int userId);
        Task<SwapRequest?> GetByIdAsync(int id);
        Task<bool> HasActiveSwapForListingAsync(int listingId);
        Task<SwapRequest> CreateAsync(SwapRequest swapRequest);
        Task<SwapRequest> UpdateAsync(SwapRequest swapRequest);
    }
}

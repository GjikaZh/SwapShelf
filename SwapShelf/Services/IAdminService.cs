using SwapShelf.DTOs;

namespace SwapShelf.Services.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<AdminUserResponse>> GetAllUsersAsync();
        Task BanUserAsync(int userId);
        Task UnbanUserAsync(int userId);
        Task<IEnumerable<ListingResponse>> GetAllListingsAsync();
        Task DeleteListingAsync(int listingId);
    }
}

using SwapShelf.DTOs;

namespace SwapShelf.Services.Interfaces
{
    public interface IListingService
    {
        Task<IEnumerable<ListingResponse>> GetAllAsync(string? genre, string? condition, string? location, string? author);
        Task<IEnumerable<ListingResponse>> GetByUserAsync(int userId);
        Task<ListingResponse> GetByIdAsync(int id);
        Task<ListingResponse> CreateAsync(int userId, ListingRequest request);
        Task<ListingResponse> UpdateAsync(int userId, int listingId, ListingRequest request);
        Task DeleteAsync(int userId, int listingId);
    }
}
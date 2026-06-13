using SwapShelf.Models;

namespace SwapShelf.Repositories
{
    public interface IListingRepository
    {
        Task<IEnumerable<Listing>> GetAllAsync(string? genre, string? condition, string? location, string? author);
        Task<IEnumerable<Listing>> GetAllForAdminAsync();
        Task<Listing?> GetByIdAsync(int id);
        Task<IEnumerable<Listing>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Listing>> GetAvailableByBookAsync(int bookId, int excludeUserId);
        Task<IEnumerable<Listing>> GetAvailableByUserAsync(int userId);
        Task<Listing> CreateAsync(Listing listing);
        Task<Listing> UpdateAsync(Listing listing);
        Task DeleteAsync(int id);
    }
}
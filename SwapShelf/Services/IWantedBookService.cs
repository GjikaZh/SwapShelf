using SwapShelf.DTOs;

namespace SwapShelf.Services.Interfaces
{
    public interface IWantedBookService
    {
        Task<IEnumerable<WantedBookResponse>> GetByUserAsync(int userId);
        Task<WantedBookResponse> AddAsync(int userId, WantedBookRequest request);
        Task RemoveAsync(int userId, int wantedBookId);
    }
}
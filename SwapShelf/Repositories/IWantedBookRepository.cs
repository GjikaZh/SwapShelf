using SwapShelf.Models;

namespace SwapShelf.Repositories
{
    public interface IWantedBookRepository
    {
        Task<IEnumerable<WantedBook>> GetByUserIdAsync(int userId);
        Task<WantedBook?> GetByIdAsync(int id);
        Task<bool> ExistsAsync(int userId, int bookId);
        Task<WantedBook> CreateAsync(WantedBook wantedBook);
        Task DeleteAsync(int id);
        Task DeleteByUserAndBookAsync(int userId, int bookId);
    }
}
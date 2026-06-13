using SwapShelf.DTOs;

namespace SwapShelf.Services.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookResponse>> GetAllAsync();
        Task<BookResponse> GetByIdAsync(int id);
        Task<BookResponse> CreateAsync(BookRequest request);
    }
}

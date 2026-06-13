using SwapShelf.Models;

namespace SwapShelf.Repositories
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync();
        Task<Book?> GetByIdAsync(int id);
        Task<Book?> GetByTitleAndAuthorAsync(string title, string author);
        Task<Book> CreateAsync(Book book);
    }
}
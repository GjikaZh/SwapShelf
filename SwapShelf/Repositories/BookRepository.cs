using Microsoft.EntityFrameworkCore;
using SwapShelf.Data;
using SwapShelf.Models;

namespace SwapShelf.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly AppDbContext _context;

        public BookRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _context.Books.ToListAsync();
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books.FindAsync(id);
        }

        public async Task<Book?> GetByTitleAndAuthorAsync(string title, string author)
        {
            return await _context.Books
                .FirstOrDefaultAsync(b => b.Title.ToLower() == title.ToLower()
                                       && b.Author.ToLower() == author.ToLower());
        }

        public async Task<Book> CreateAsync(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return book;
        }
    }
}
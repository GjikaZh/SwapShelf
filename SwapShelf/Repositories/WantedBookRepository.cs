using Microsoft.EntityFrameworkCore;
using SwapShelf.Data;
using SwapShelf.Models;

namespace SwapShelf.Repositories
{
    public class WantedBookRepository : IWantedBookRepository
    {
        private readonly AppDbContext _context;

        public WantedBookRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WantedBook>> GetByUserIdAsync(int userId)
        {
            return await _context.WantedBooks
                .Include(w => w.Book)
                .Include(w => w.User)
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }

        public async Task<WantedBook?> GetByIdAsync(int id)
        {
            return await _context.WantedBooks
                .Include(w => w.Book)
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<bool> ExistsAsync(int userId, int bookId)
        {
            return await _context.WantedBooks
                .AnyAsync(w => w.UserId == userId && w.BookId == bookId);
        }

        public async Task<WantedBook> CreateAsync(WantedBook wantedBook)
        {
            _context.WantedBooks.Add(wantedBook);
            await _context.SaveChangesAsync();
            return wantedBook;
        }

        public async Task DeleteAsync(int id)
        {
            var wantedBook = await _context.WantedBooks.FindAsync(id);
            if (wantedBook != null)
            {
                _context.WantedBooks.Remove(wantedBook);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByUserAndBookAsync(int userId, int bookId)
        {
            var wantedBook = await _context.WantedBooks
                .FirstOrDefaultAsync(w => w.UserId == userId && w.BookId == bookId);
            if (wantedBook != null)
            {
                _context.WantedBooks.Remove(wantedBook);
                await _context.SaveChangesAsync();
            }
        }
    }
}
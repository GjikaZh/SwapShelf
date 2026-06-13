using Microsoft.EntityFrameworkCore;
using SwapShelf.Data;
using SwapShelf.Models;

namespace SwapShelf.Repositories
{
    public class ListingRepository : IListingRepository
    {
        private readonly AppDbContext _context;

        public ListingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Listing>> GetAllAsync(string? genre, string? condition, string? location, string? author)
        {
            var query = _context.Listings
                .Include(l => l.Book)
                .Include(l => l.User)
                .Where(l => l.Status == ListingStatus.Available)
                .AsQueryable();

            if (!string.IsNullOrEmpty(genre))
                query = query.Where(l => l.Book.Genre.ToLower() == genre.ToLower());

            if (!string.IsNullOrEmpty(condition) && Enum.TryParse<ListingCondition>(condition, true, out var parsedCondition))
                query = query.Where(l => l.Condition == parsedCondition);

            if (!string.IsNullOrEmpty(location))
                query = query.Where(l => l.Location.ToLower().Contains(location.ToLower()));

            if (!string.IsNullOrEmpty(author))
                query = query.Where(l => l.Book.Author.ToLower().Contains(author.ToLower()));

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Listing>> GetAllForAdminAsync()
        {
            return await _context.Listings
                .Include(l => l.Book)
                .Include(l => l.User)
                .ToListAsync();
        }

        public async Task<Listing?> GetByIdAsync(int id)
        {
            return await _context.Listings
                .Include(l => l.Book)
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Listing>> GetByUserIdAsync(int userId)
        {
            return await _context.Listings
                .Include(l => l.Book)
                .Include(l => l.User)
                .Where(l => l.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Listing>> GetAvailableByBookAsync(int bookId, int excludeUserId)
        {
            return await _context.Listings
                .Include(l => l.Book)
                .Include(l => l.User)
                .Where(l => l.BookId == bookId
                    && l.UserId != excludeUserId
                    && l.Status == ListingStatus.Available)
                .ToListAsync();
        }

        public async Task<IEnumerable<Listing>> GetAvailableByUserAsync(int userId)
        {
            return await _context.Listings
                .Include(l => l.Book)
                .Include(l => l.User)
                .Where(l => l.UserId == userId
                    && l.Status == ListingStatus.Available)
                .ToListAsync();
        }

        public async Task<Listing> CreateAsync(Listing listing)
        {
            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();
            return listing;
        }

        public async Task<Listing> UpdateAsync(Listing listing)
        {
            _context.Listings.Update(listing);
            await _context.SaveChangesAsync();
            return listing;
        }

        public async Task DeleteAsync(int id)
        {
            var listing = await _context.Listings.FindAsync(id);
            if (listing != null)
            {
                _context.Listings.Remove(listing);
                await _context.SaveChangesAsync();
            }
        }
    }
}
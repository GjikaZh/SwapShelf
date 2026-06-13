using SwapShelf.DTOs;
using SwapShelf.Models;
using SwapShelf.Repositories;
using SwapShelf.Services.Interfaces;

namespace SwapShelf.Services.Implementations
{
    public class WantedBookService : IWantedBookService
    {
        private readonly IWantedBookRepository _wantedBookRepository;
        private readonly IBookRepository _bookRepository;

        public WantedBookService(IWantedBookRepository wantedBookRepository, IBookRepository bookRepository)
        {
            _wantedBookRepository = wantedBookRepository;
            _bookRepository = bookRepository;
        }

        public async Task<IEnumerable<WantedBookResponse>> GetByUserAsync(int userId)
        {
            var wanted = await _wantedBookRepository.GetByUserIdAsync(userId);
            return wanted.Select(MapToResponse);
        }

        public async Task<WantedBookResponse> AddAsync(int userId, WantedBookRequest request)
        {
            var book = await _bookRepository.GetByIdAsync(request.BookId)
                ?? throw new KeyNotFoundException($"Book {request.BookId} not found.");

            var alreadyExists = await _wantedBookRepository.ExistsAsync(userId, request.BookId);
            if (alreadyExists)
                throw new InvalidOperationException("This book is already on your wanted list.");

            var wanted = new WantedBook
            {
                UserId = userId,
                BookId = request.BookId
            };

            var created = await _wantedBookRepository.CreateAsync(wanted);
            var full = await _wantedBookRepository.GetByIdAsync(created.Id);
            return MapToResponse(full!);
        }

        public async Task RemoveAsync(int userId, int wantedBookId)
        {
            var wanted = await _wantedBookRepository.GetByIdAsync(wantedBookId)
                ?? throw new KeyNotFoundException($"Wanted book {wantedBookId} not found.");

            if (wanted.UserId != userId)
                throw new UnauthorizedAccessException("You can only remove your own wanted books.");

            await _wantedBookRepository.DeleteAsync(wantedBookId);
        }

        private static WantedBookResponse MapToResponse(WantedBook w) => new()
        {
            Id = w.Id,
            UserId = w.UserId,
            Book = new BookResponse
            {
                Id = w.Book.Id,
                Title = w.Book.Title,
                Author = w.Book.Author,
                Genre = w.Book.Genre,
                ISBN = w.Book.ISBN
            },
            CreatedAt = w.CreatedAt
        };
    }
}
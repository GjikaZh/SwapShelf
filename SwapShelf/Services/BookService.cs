using SwapShelf.DTOs;
using SwapShelf.Models;
using SwapShelf.Repositories;
using SwapShelf.Services.Interfaces;

namespace SwapShelf.Services.Implementations
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;

        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<IEnumerable<BookResponse>> GetAllAsync()
        {
            var books = await _bookRepository.GetAllAsync();
            return books.Select(MapToResponse);
        }

        public async Task<BookResponse> GetByIdAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Book {id} not found.");
            return MapToResponse(book);
        }

        public async Task<BookResponse> CreateAsync(BookRequest request)
        {
            var existing = await _bookRepository.GetByTitleAndAuthorAsync(request.Title, request.Author);
            if (existing != null)
                throw new InvalidOperationException("This book already exists in the catalog.");

            var book = new Book
            {
                Title  = request.Title,
                Author = request.Author,
                Genre  = request.Genre,
                ISBN   = request.ISBN
            };

            var created = await _bookRepository.CreateAsync(book);
            return MapToResponse(created);
        }

        private static BookResponse MapToResponse(Book b) => new()
        {
            Id     = b.Id,
            Title  = b.Title,
            Author = b.Author,
            Genre  = b.Genre,
            ISBN   = b.ISBN
        };
    }
}

using SwapShelf.DTOs;
using SwapShelf.Models;
using SwapShelf.Repositories;
using SwapShelf.Services.Interfaces;

namespace SwapShelf.Services.Implementations
{
    public class ListingService : IListingService
    {
        private readonly IListingRepository _listingRepository;
        private readonly IBookRepository _bookRepository;

        public ListingService(IListingRepository listingRepository, IBookRepository bookRepository)
        {
            _listingRepository = listingRepository;
            _bookRepository = bookRepository;
        }

        public async Task<IEnumerable<ListingResponse>> GetAllAsync(string? genre, string? condition, string? location, string? author)
        {
            var listings = await _listingRepository.GetAllAsync(genre, condition, location, author);
            return listings.Select(MapToResponse);
        }

        public async Task<IEnumerable<ListingResponse>> GetByUserAsync(int userId)
        {
            var listings = await _listingRepository.GetByUserIdAsync(userId);
            return listings.Select(MapToResponse);
        }

        public async Task<ListingResponse> GetByIdAsync(int id)
        {
            var listing = await _listingRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Listing {id} not found.");
            return MapToResponse(listing);
        }

        public async Task<ListingResponse> CreateAsync(int userId, ListingRequest request)
        {
            var book = await _bookRepository.GetByIdAsync(request.BookId)
                ?? throw new KeyNotFoundException($"Book {request.BookId} not found.");

            var listing = new Listing
            {
                UserId = userId,
                BookId = request.BookId,
                Condition = request.Condition,
                Location = request.Location,
                Status = ListingStatus.Available
            };

            var created = await _listingRepository.CreateAsync(listing);
            var full = await _listingRepository.GetByIdAsync(created.Id);
            return MapToResponse(full!);
        }

        public async Task<ListingResponse> UpdateAsync(int userId, int listingId, ListingRequest request)
        {
            var listing = await _listingRepository.GetByIdAsync(listingId)
                ?? throw new KeyNotFoundException($"Listing {listingId} not found.");

            if (listing.UserId != userId)
                throw new UnauthorizedAccessException("You can only edit your own listings.");

            if (listing.Status == ListingStatus.Locked)
                throw new InvalidOperationException("Cannot edit a listing that is part of an active swap.");

            if (listing.Status == ListingStatus.Swapped)
                throw new InvalidOperationException("Cannot edit a listing that has already been swapped.");

            listing.Condition = request.Condition;
            listing.Location = request.Location;

            var updated = await _listingRepository.UpdateAsync(listing);
            return MapToResponse(updated);
        }

        public async Task DeleteAsync(int userId, int listingId)
        {
            var listing = await _listingRepository.GetByIdAsync(listingId)
                ?? throw new KeyNotFoundException($"Listing {listingId} not found.");

            if (listing.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own listings.");

            if (listing.Status == ListingStatus.Locked)
                throw new InvalidOperationException("Cannot delete a listing that is part of an active swap.");

            if (listing.Status == ListingStatus.Swapped)
                throw new InvalidOperationException("Cannot delete a listing that has already been swapped.");

            await _listingRepository.DeleteAsync(listingId);
        }

        public static ListingResponse MapToResponse(Listing listing) => new()
        {
            Id = listing.Id,
            UserId = listing.UserId,
            UserFullName = listing.User?.FullName ?? string.Empty,
            Book = new BookResponse
            {
                Id = listing.Book.Id,
                Title = listing.Book.Title,
                Author = listing.Book.Author,
                Genre = listing.Book.Genre,
                ISBN = listing.Book.ISBN
            },
            Condition = listing.Condition,
            Status = listing.Status,
            Location = listing.Location,
            CreatedAt = listing.CreatedAt
        };
    }
}
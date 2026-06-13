using SwapShelf.DTOs;
using SwapShelf.Models;
using SwapShelf.Repositories;
using SwapShelf.Services.Implementations;
using SwapShelf.Services.Interfaces;

namespace SwapShelf.Services.Implementations
{
    public class SwapService : ISwapService
    {
        private readonly ISwapRepository _swapRepository;
        private readonly IListingRepository _listingRepository;
        private readonly IWantedBookRepository _wantedBookRepository;

        public SwapService(ISwapRepository swapRepository, IListingRepository listingRepository, IWantedBookRepository wantedBookRepository)
        {
            _swapRepository = swapRepository;
            _listingRepository = listingRepository;
            _wantedBookRepository = wantedBookRepository;
        }

        public async Task<IEnumerable<SwapRequestResponse>> GetByUserAsync(int userId)
        {
            var swaps = await _swapRepository.GetByUserIdAsync(userId);
            return swaps.Select(MapToResponse);
        }

        public async Task<SwapRequestResponse> GetByIdAsync(int swapId, int userId)
        {
            var swap = await GetAndValidateAccessAsync(swapId, userId);
            return MapToResponse(swap);
        }

        public async Task<SwapRequestResponse> CreateAsync(int initiatorId, SwapRequestCreate request)
        {
            if (request.InitiatorListingId == request.ReceiverListingId)
                throw new InvalidOperationException("Cannot swap a listing with itself.");

            var initiatorListing = await _listingRepository.GetByIdAsync(request.InitiatorListingId)
                ?? throw new KeyNotFoundException("Initiator listing not found.");

            var receiverListing = await _listingRepository.GetByIdAsync(request.ReceiverListingId)
                ?? throw new KeyNotFoundException("Receiver listing not found.");

            if (initiatorListing.UserId != initiatorId)
                throw new UnauthorizedAccessException("You can only offer your own listings.");

            if (receiverListing.UserId == initiatorId)
                throw new InvalidOperationException("Cannot swap with yourself.");

            if (initiatorListing.Status != ListingStatus.Available)
                throw new InvalidOperationException("Your listing is not available for swapping.");

            if (receiverListing.Status != ListingStatus.Available)
                throw new InvalidOperationException("The requested listing is not available for swapping.");

            var hasActive = await _swapRepository.HasActiveSwapForListingAsync(request.InitiatorListingId);
            if (hasActive)
                throw new InvalidOperationException("Your listing already has an active swap request.");

            var swap = new SwapRequest
            {
                InitiatorId = initiatorId,
                ReceiverId = receiverListing.UserId,
                InitiatorListingId = request.InitiatorListingId,
                ReceiverListingId = request.ReceiverListingId,
                Status = SwapStatus.Pending
            };

            // Lock both listings
            initiatorListing.Status = ListingStatus.Locked;
            receiverListing.Status = ListingStatus.Locked;
            await _listingRepository.UpdateAsync(initiatorListing);
            await _listingRepository.UpdateAsync(receiverListing);

            var created = await _swapRepository.CreateAsync(swap);
            var full = await _swapRepository.GetByIdAsync(created.Id);
            return MapToResponse(full!);
        }

        public async Task<SwapRequestResponse> AcceptAsync(int swapId, int userId)
        {
            var swap = await GetAndValidateAccessAsync(swapId, userId);

            if (swap.ReceiverId != userId)
                throw new UnauthorizedAccessException("Only the receiver can accept a swap.");

            if (swap.Status != SwapStatus.Pending)
                throw new InvalidOperationException("Only pending swaps can be accepted.");

            swap.Status = SwapStatus.Accepted;
            swap.UpdatedAt = DateTime.UtcNow;
            var updated = await _swapRepository.UpdateAsync(swap);
            return MapToResponse(updated);
        }

        public async Task<SwapRequestResponse> RejectAsync(int swapId, int userId)
        {
            var swap = await GetAndValidateAccessAsync(swapId, userId);

            if (swap.ReceiverId != userId)
                throw new UnauthorizedAccessException("Only the receiver can reject a swap.");

            if (swap.Status != SwapStatus.Pending)
                throw new InvalidOperationException("Only pending swaps can be rejected.");

            swap.Status = SwapStatus.Rejected;
            swap.UpdatedAt = DateTime.UtcNow;

            // Unlock both listings
            await UnlockListingsAsync(swap);

            var updated = await _swapRepository.UpdateAsync(swap);
            return MapToResponse(updated);
        }

        public async Task<SwapRequestResponse> MarkInTransitAsync(int swapId, int userId)
        {
            var swap = await GetAndValidateAccessAsync(swapId, userId);

            if (swap.Status != SwapStatus.Accepted)
                throw new InvalidOperationException("Only accepted swaps can be marked as in transit.");

            swap.Status = SwapStatus.InTransit;
            swap.UpdatedAt = DateTime.UtcNow;
            var updated = await _swapRepository.UpdateAsync(swap);
            return MapToResponse(updated);
        }

        public async Task<SwapRequestResponse> CompleteAsync(int swapId, int userId)
        {
            var swap = await GetAndValidateAccessAsync(swapId, userId);

            if (swap.Status != SwapStatus.InTransit)
                throw new InvalidOperationException("Only in-transit swaps can be completed.");

            swap.Status = SwapStatus.Completed;
            swap.UpdatedAt = DateTime.UtcNow;

            // Mark both listings as swapped permanently
            var initiatorListing = await _listingRepository.GetByIdAsync(swap.InitiatorListingId);
            var receiverListing = await _listingRepository.GetByIdAsync(swap.ReceiverListingId);

            if (initiatorListing != null) { initiatorListing.Status = ListingStatus.Swapped; await _listingRepository.UpdateAsync(initiatorListing); }
            if (receiverListing != null) { receiverListing.Status = ListingStatus.Swapped; await _listingRepository.UpdateAsync(receiverListing); }

            // Auto-remove the received book from each user's wanted list.
            // Initiator receives the receiver's book, receiver receives the initiator's book.
            if (receiverListing != null)
                await _wantedBookRepository.DeleteByUserAndBookAsync(swap.InitiatorId, receiverListing.BookId);
            if (initiatorListing != null)
                await _wantedBookRepository.DeleteByUserAndBookAsync(swap.ReceiverId, initiatorListing.BookId);

            var updated = await _swapRepository.UpdateAsync(swap);
            return MapToResponse(updated);
        }

        public async Task<SwapRequestResponse> CancelAsync(int swapId, int userId)
        {
            var swap = await GetAndValidateAccessAsync(swapId, userId);

            if (swap.Status == SwapStatus.Completed)
                throw new InvalidOperationException("Cannot cancel a completed swap.");

            if (swap.Status == SwapStatus.Rejected)
                throw new InvalidOperationException("Cannot cancel a rejected swap.");

            swap.Status = SwapStatus.Cancelled;
            swap.UpdatedAt = DateTime.UtcNow;

            // Unlock both listings so they become available again
            await UnlockListingsAsync(swap);

            var updated = await _swapRepository.UpdateAsync(swap);
            return MapToResponse(updated);
        }

        private async Task<SwapRequest> GetAndValidateAccessAsync(int swapId, int userId)
        {
            var swap = await _swapRepository.GetByIdAsync(swapId)
                ?? throw new KeyNotFoundException($"Swap {swapId} not found.");

            if (swap.InitiatorId != userId && swap.ReceiverId != userId)
                throw new UnauthorizedAccessException("You do not have access to this swap.");

            return swap;
        }

        private async Task UnlockListingsAsync(SwapRequest swap)
        {
            var initiatorListing = await _listingRepository.GetByIdAsync(swap.InitiatorListingId);
            var receiverListing = await _listingRepository.GetByIdAsync(swap.ReceiverListingId);

            if (initiatorListing != null) { initiatorListing.Status = ListingStatus.Available; await _listingRepository.UpdateAsync(initiatorListing); }
            if (receiverListing != null) { receiverListing.Status = ListingStatus.Available; await _listingRepository.UpdateAsync(receiverListing); }
        }

        public static SwapRequestResponse MapToResponse(SwapRequest s) => new()
        {
            Id = s.Id,
            InitiatorId = s.InitiatorId,
            InitiatorName = s.Initiator?.FullName ?? string.Empty,
            ReceiverId = s.ReceiverId,
            ReceiverName = s.Receiver?.FullName ?? string.Empty,
            InitiatorListing = ListingService.MapToResponse(s.InitiatorListing),
            ReceiverListing = ListingService.MapToResponse(s.ReceiverListing),
            Status = s.Status,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            ReviewerIds = s.Reviews.Select(r => r.ReviewerId).ToList()
        };
    }
}
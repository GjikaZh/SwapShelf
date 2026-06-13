using SwapShelf.DTOs;
using SwapShelf.Models;
using SwapShelf.Repositories;
using SwapShelf.Services.Implementations;
using SwapShelf.Services.Interfaces;

namespace SwapShelf.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository    _userRepository;
        private readonly IListingRepository _listingRepository;

        public AdminService(IUserRepository userRepository, IListingRepository listingRepository)
        {
            _userRepository    = userRepository;
            _listingRepository = listingRepository;
        }

        public async Task<IEnumerable<AdminUserResponse>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(u => new AdminUserResponse
            {
                Id         = u.Id,
                FullName   = u.FullName,
                Email      = u.Email,
                Role       = u.Role.ToString(),
                TrustScore = u.TrustScore,
                IsBanned   = u.IsBanned,
                CreatedAt  = u.CreatedAt
            });
        }

        public async Task BanUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException($"User {userId} not found.");

            if (user.Role == UserRole.Admin)
                throw new InvalidOperationException("Cannot ban an admin.");

            user.IsBanned = true;
            await _userRepository.UpdateAsync(user);
        }

        public async Task UnbanUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException($"User {userId} not found.");

            user.IsBanned = false;
            await _userRepository.UpdateAsync(user);
        }

        public async Task<IEnumerable<ListingResponse>> GetAllListingsAsync()
        {
            // Use admin-specific query so ALL statuses (Available, Locked, Swapped) are returned
            var listings = await _listingRepository.GetAllForAdminAsync();
            return listings.Select(ListingService.MapToResponse);
        }

        public async Task DeleteListingAsync(int listingId)
        {
            var listing = await _listingRepository.GetByIdAsync(listingId)
                ?? throw new KeyNotFoundException($"Listing {listingId} not found.");

            if (listing.Status == ListingStatus.Locked)
                throw new InvalidOperationException("Cannot delete a listing that is part of an active swap.");

            await _listingRepository.DeleteAsync(listingId);
        }
    }
}

using SwapShelf.DTOs;
using SwapShelf.Repositories;
using SwapShelf.Services.Interfaces;

namespace SwapShelf.Services.Implementations
{
    public class MatchingService : IMatchingService
    {
        private readonly IListingRepository _listingRepository;
        private readonly IWantedBookRepository _wantedBookRepository;

        public MatchingService(IListingRepository listingRepository, IWantedBookRepository wantedBookRepository)
        {
            _listingRepository = listingRepository;
            _wantedBookRepository = wantedBookRepository;
        }

        public async Task<IEnumerable<MatchResponse>> GetMatchesAsync(int userId)
        {
            // What I have available
            var myListings = await _listingRepository.GetAvailableByUserAsync(userId);

            // What I want
            var myWanted = await _wantedBookRepository.GetByUserIdAsync(userId);

            var matches = new List<MatchResponse>();

            foreach (var wanted in myWanted)
            {
                // Find other users' available listings for this book
                var theirListings = await _listingRepository.GetAvailableByBookAsync(wanted.BookId, excludeUserId: userId);

                foreach (var theirListing in theirListings)
                {
                    // Check if they want any book I have
                    var theirWanted = await _wantedBookRepository.GetByUserIdAsync(theirListing.UserId);

                    var myMatchingListings = myListings
                        .Where(ml => theirWanted.Any(tw => tw.BookId == ml.BookId))
                        .ToList();

                    if (myMatchingListings.Any())
                    {
                        // Avoid duplicate matches for the same user
                        var alreadyAdded = matches.Any(m => m.TheirUserId == theirListing.UserId
                                                         && m.TheirListing.Id == theirListing.Id);
                        if (!alreadyAdded)
                        {
                            matches.Add(new MatchResponse
                            {
                                TheirUserId = theirListing.UserId,
                                TheirUserName = theirListing.User?.FullName ?? string.Empty,
                                TheirTrustScore = theirListing.User?.TrustScore ?? 0,
                                TheirListing = ListingService.MapToResponse(theirListing),
                                MyMatchingListings = myMatchingListings.Select(ListingService.MapToResponse).ToList()
                            });
                        }
                    }
                }
            }

            return matches;
        }
    }
}
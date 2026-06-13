using SwapShelf.DTOs;

namespace SwapShelf.Services.Interfaces
{
    public interface IMatchingService
    {
        Task<IEnumerable<MatchResponse>> GetMatchesAsync(int userId);
    }
}
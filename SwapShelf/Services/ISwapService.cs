using SwapShelf.DTOs;

namespace SwapShelf.Services.Interfaces
{
    public interface ISwapService
    {
        Task<IEnumerable<SwapRequestResponse>> GetByUserAsync(int userId);
        Task<SwapRequestResponse> GetByIdAsync(int swapId, int userId);
        Task<SwapRequestResponse> CreateAsync(int initiatorId, SwapRequestCreate request);
        Task<SwapRequestResponse> AcceptAsync(int swapId, int userId);
        Task<SwapRequestResponse> RejectAsync(int swapId, int userId);
        Task<SwapRequestResponse> MarkInTransitAsync(int swapId, int userId);
        Task<SwapRequestResponse> CompleteAsync(int swapId, int userId);
        Task<SwapRequestResponse> CancelAsync(int swapId, int userId);
    }
}
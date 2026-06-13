using AutoMapper.Internal;

namespace SwapShelf.Models
{
    public enum ListingCondition
    {
        New,
        Good,
        Fair,
        Poor
    }

    public enum ListingStatus
    {
        Available,
        Locked,
        Swapped
    }

    public class Listing
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public ListingCondition Condition { get; set; }
        public ListingStatus Status { get; set; } = ListingStatus.Available;
        public string Location { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; } = null!;
        public Book Book { get; set; } = null!;
        public ICollection<SwapRequest> InitiatedSwapRequests { get; set; } = new List<SwapRequest>();
        public ICollection<SwapRequest> ReceivedSwapRequests { get; set; } = new List<SwapRequest>();
    }
}
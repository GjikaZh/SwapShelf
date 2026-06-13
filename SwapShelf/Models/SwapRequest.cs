namespace SwapShelf.Models
{
    public enum SwapStatus
    {
        Pending,
        Accepted,
        Rejected,
        InTransit,
        Completed,
        Cancelled
    }

    public class SwapRequest
    {
        public int Id { get; set; }
        public int InitiatorId { get; set; }
        public int ReceiverId { get; set; }
        public int InitiatorListingId { get; set; }
        public int ReceiverListingId { get; set; }
        public SwapStatus Status { get; set; } = SwapStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User Initiator { get; set; } = null!;
        public User Receiver { get; set; } = null!;
        public Listing InitiatorListing { get; set; } = null!;
        public Listing ReceiverListing { get; set; } = null!;
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
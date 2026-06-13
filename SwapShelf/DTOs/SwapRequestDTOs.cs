using System.ComponentModel.DataAnnotations;
using SwapShelf.Models;

namespace SwapShelf.DTOs
{
    public class SwapRequestCreate
    {
        [Range(1, int.MaxValue, ErrorMessage = "A valid Initiator Listing ID is required.")]
        public int InitiatorListingId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A valid Receiver Listing ID is required.")]
        public int ReceiverListingId { get; set; }
    }

    public class SwapRequestResponse
    {
        public int Id { get; set; }
        public int InitiatorId { get; set; }
        public string InitiatorName { get; set; } = string.Empty;
        public int ReceiverId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public ListingResponse InitiatorListing { get; set; } = null!;
        public ListingResponse ReceiverListing { get; set; } = null!;
        public SwapStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        /// <summary>IDs of users who have already submitted a review for this swap.</summary>
        public List<int> ReviewerIds { get; set; } = new();
    }
}
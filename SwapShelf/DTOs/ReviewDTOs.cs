using System.ComponentModel.DataAnnotations;

namespace SwapShelf.DTOs
{
    public class ReviewRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "A valid Swap Request ID is required.")]
        public int SwapRequestId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A valid Reviewee ID is required.")]
        public int RevieweeId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Comment is required.")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Comment must be between 1 and 1000 characters.")]
        public string Comment { get; set; } = string.Empty;
    }

    public class ReviewResponse
    {
        public int Id { get; set; }
        public int SwapRequestId { get; set; }
        public int ReviewerId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public int RevieweeId { get; set; }
        public string RevieweeName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
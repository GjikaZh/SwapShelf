using System.ComponentModel.DataAnnotations;
using SwapShelf.Models;

namespace SwapShelf.DTOs
{
    public class ListingRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "A valid Book ID is required.")]
        public int BookId { get; set; }

        [EnumDataType(typeof(ListingCondition), ErrorMessage = "Invalid listing condition.")]
        public ListingCondition Condition { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Location must be between 2 and 200 characters.")]
        public string Location { get; set; } = string.Empty;
    }

    public class ListingResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public BookResponse Book { get; set; } = null!;
        public ListingCondition Condition { get; set; }
        public ListingStatus Status { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
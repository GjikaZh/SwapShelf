using System.ComponentModel.DataAnnotations;

namespace SwapShelf.DTOs
{
    public class WantedBookRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "A valid Book ID is required.")]
        public int BookId { get; set; }
    }

    public class WantedBookResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public BookResponse Book { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
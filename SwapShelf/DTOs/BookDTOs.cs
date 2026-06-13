using System.ComponentModel.DataAnnotations;

namespace SwapShelf.DTOs
{
    public class BookRequest
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author is required.")]
        [StringLength(150, MinimumLength = 1, ErrorMessage = "Author must be between 1 and 150 characters.")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Genre is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Genre must be between 1 and 100 characters.")]
        public string Genre { get; set; } = string.Empty;

        [StringLength(17, ErrorMessage = "ISBN must not exceed 17 characters.")]
        [RegularExpression(@"^(?:\d{9}[\dX]|\d{13})$",
            ErrorMessage = "ISBN must be a valid 10- or 13-digit ISBN (digits only, no dashes; X allowed as last digit for ISBN-10).")]
        public string? ISBN { get; set; }
    }

    public class BookResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string? ISBN { get; set; }
    }
}
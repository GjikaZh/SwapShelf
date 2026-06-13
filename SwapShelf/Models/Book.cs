using System.Reflection;

namespace SwapShelf.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string? ISBN { get; set; }

        // Navigation properties
        public ICollection<Listing> Listings { get; set; } = new List<Listing>();
        public ICollection<WantedBook> WantedBooks { get; set; } = new List<WantedBook>();
    }
}
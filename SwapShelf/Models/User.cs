namespace SwapShelf.Models
{
    public enum UserRole
    {
        User,
        Admin
    }

    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
        public double TrustScore { get; set; } = 0.0;
        public bool IsBanned { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Listing> Listings { get; set; } = new List<Listing>();
        public ICollection<WantedBook> WantedBooks { get; set; } = new List<WantedBook>();
        public ICollection<SwapRequest> InitiatedSwaps { get; set; } = new List<SwapRequest>();
        public ICollection<SwapRequest> ReceivedSwaps { get; set; } = new List<SwapRequest>();
        public ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
        public ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
    }
}
using SwapShelf.Models;

namespace SwapShelf.Tests.Helpers
{
    /// <summary>
    /// Factory helpers that produce fully populated domain objects for use in unit tests.
    /// Navigation properties are always wired up so MapToResponse never throws.
    /// </summary>
    internal static class TestBuilders
    {
        internal static Book Book(int id = 1, string title = "Test Book",
            string author = "Test Author", string genre = "Fiction") => new()
        {
            Id = id,
            Title = title,
            Author = author,
            Genre = genre
        };

        internal static User User(int id = 1, string name = "Test User",
            string email = "test@test.com") => new()
        {
            Id = id,
            FullName = name,
            Email = email,
            Role = UserRole.User,
            TrustScore = 0.0,
            IsBanned = false
        };

        internal static Listing Listing(
            int id = 1,
            int userId = 1,
            int bookId = 1,
            ListingStatus status = ListingStatus.Available,
            ListingCondition condition = ListingCondition.Good,
            string location = "Test City")
        {
            var book = Book(bookId, $"Book {bookId}", $"Author {bookId}");
            var user = User(userId, $"User {userId}", $"user{userId}@test.com");
            return new Listing
            {
                Id = id,
                UserId = userId,
                BookId = bookId,
                Status = status,
                Condition = condition,
                Location = location,
                Book = book,
                User = user
            };
        }

        internal static WantedBook WantedBook(int id = 1, int userId = 1, int bookId = 1) => new()
        {
            Id = id,
            UserId = userId,
            BookId = bookId,
            Book = Book(bookId, $"Book {bookId}", $"Author {bookId}"),
            User = User(userId, $"User {userId}", $"user{userId}@test.com")
        };

        /// <summary>
        /// Produces a SwapRequest with all navigation properties set so that
        /// SwapService.MapToResponse can be called without NullReferenceExceptions.
        /// </summary>
        internal static SwapRequest SwapRequest(
            int id = 1,
            int initiatorId = 1,
            int receiverId = 2,
            int initiatorListingId = 1,
            int receiverListingId = 2,
            SwapStatus status = SwapStatus.Pending)
        {
            var initiatorListing = Listing(initiatorListingId, initiatorId, bookId: initiatorListingId);
            var receiverListing  = Listing(receiverListingId,  receiverId,  bookId: receiverListingId);
            return new SwapRequest
            {
                Id = id,
                InitiatorId = initiatorId,
                ReceiverId = receiverId,
                InitiatorListingId = initiatorListingId,
                ReceiverListingId = receiverListingId,
                Status = status,
                Initiator = User(initiatorId, $"User {initiatorId}", $"user{initiatorId}@test.com"),
                Receiver  = User(receiverId,  $"User {receiverId}",  $"user{receiverId}@test.com"),
                InitiatorListing = initiatorListing,
                ReceiverListing  = receiverListing,
                Reviews = new List<Review>()
            };
        }

        internal static Review Review(
            int id = 1,
            int swapId = 1,
            int reviewerId = 1,
            int revieweeId = 2,
            int rating = 5) => new()
        {
            Id = id,
            SwapRequestId = swapId,
            ReviewerId = reviewerId,
            RevieweeId = revieweeId,
            Rating = rating,
            Comment = "Test comment",
            CreatedAt = DateTime.UtcNow,
            Reviewer = User(reviewerId, $"Reviewer {reviewerId}"),
            Reviewee = User(revieweeId, $"Reviewee {revieweeId}")
        };
    }
}

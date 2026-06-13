namespace SwapShelf.DTOs
{
    public class MatchResponse
    {
        public int TheirUserId { get; set; }
        public string TheirUserName { get; set; } = string.Empty;
        public double TheirTrustScore { get; set; }
        public ListingResponse TheirListing { get; set; } = null!;
        public List<ListingResponse> MyMatchingListings { get; set; } = new();
    }
}
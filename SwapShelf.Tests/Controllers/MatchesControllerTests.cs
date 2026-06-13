using Microsoft.AspNetCore.Mvc;
using SwapShelf.Controllers;
using SwapShelf.DTOs;
using SwapShelf.Services.Interfaces;
using SwapShelf.Tests.Helpers;

namespace SwapShelf.Tests.Controllers
{
    public class MatchesControllerTests
    {
        private readonly IMatchingService _mockMatchingService;
        private readonly MatchesController _controller;

        public MatchesControllerTests()
        {
            _mockMatchingService = Substitute.For<IMatchingService>();
            _controller = new MatchesController(_mockMatchingService);
            ControllerTestHelper.SetUser(_controller, 1);
        }

        [Fact]
        public async Task GetMatches_ReturnsOkWithMatches()
        {
            var matches = new List<MatchResponse>
            {
                new MatchResponse { TheirUserId = 2, TheirUserName = "Bob", TheirTrustScore = 4.5 },
                new MatchResponse { TheirUserId = 3, TheirUserName = "Carol", TheirTrustScore = 3.8 }
            };
            _mockMatchingService.GetMatchesAsync(1).Returns(matches);

            var result = await _controller.GetMatches() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var returnedMatches = result.Value as IEnumerable<MatchResponse>;
            Assert.NotNull(returnedMatches);
            Assert.Equal(2, returnedMatches.Count());
        }

        [Fact]
        public async Task GetMatches_NoMatches_ReturnsEmptyOk()
        {
            var emptyMatches = new List<MatchResponse>();
            _mockMatchingService.GetMatchesAsync(1).Returns(emptyMatches);

            var result = await _controller.GetMatches() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var returnedMatches = result.Value as IEnumerable<MatchResponse>;
            Assert.NotNull(returnedMatches);
            Assert.Empty(returnedMatches);
        }
    }
}

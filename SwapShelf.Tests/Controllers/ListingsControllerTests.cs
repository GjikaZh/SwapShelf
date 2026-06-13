using Microsoft.AspNetCore.Mvc;
using SwapShelf.Controllers;
using SwapShelf.DTOs;
using SwapShelf.Models;
using SwapShelf.Services.Interfaces;
using SwapShelf.Tests.Helpers;

namespace SwapShelf.Tests.Controllers
{
    public class ListingsControllerTests
    {
        private readonly IListingService _listingServiceMock = Substitute.For<IListingService>();
        private readonly ListingsController _controller;

        public ListingsControllerTests()
        {
            _controller = new ListingsController(_listingServiceMock);
        }

        [Fact]
        public async Task GetMine_ReturnsOk()
        {
            var listings = new List<ListingResponse>
            {
                new ListingResponse { Id = 1, UserId = 1 }
            };
            _listingServiceMock
                .GetByUserAsync(1)
                .Returns(listings);

            ControllerTestHelper.SetUser(_controller, userId: 1);

            var result = await _controller.GetMine() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            var listings = new List<ListingResponse>
            {
                new ListingResponse { Id = 1, UserId = 1 }
            };
            _listingServiceMock
                .GetAllAsync(null, null, null, null)
                .Returns(listings);

            var result = await _controller.GetAll(null, null, null, null) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task GetById_Found_ReturnsOk()
        {
            var listing = new ListingResponse { Id = 1, UserId = 1 };
            _listingServiceMock
                .GetByIdAsync(1)
                .Returns(listing);

            var result = await _controller.GetById(1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task GetById_NotFound_Returns404()
        {
            _listingServiceMock
                .GetByIdAsync(99)
                .Throws(new KeyNotFoundException("Listing 99 not found."));

            var result = await _controller.GetById(99) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task Create_Valid_Returns201()
        {
            var request = new ListingRequest { BookId = 1, Condition = ListingCondition.Good, Location = "NYC" };
            var created = new ListingResponse { Id = 1, UserId = 1 };

            _listingServiceMock
                .CreateAsync(1, request)
                .Returns(created);

            ControllerTestHelper.SetUser(_controller, userId: 1);

            var result = await _controller.Create(request) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
        }

        [Fact]
        public async Task Create_BookNotFound_Returns404()
        {
            var request = new ListingRequest { BookId = 99, Condition = ListingCondition.Good, Location = "NYC" };

            _listingServiceMock
                .CreateAsync(1, request)
                .Throws(new KeyNotFoundException("Book 99 not found."));

            ControllerTestHelper.SetUser(_controller, userId: 1);

            var result = await _controller.Create(request) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task Update_Valid_ReturnsOk()
        {
            var request = new ListingRequest { BookId = 1, Condition = ListingCondition.Good, Location = "NYC" };
            var updated = new ListingResponse { Id = 1, UserId = 1 };

            _listingServiceMock
                .UpdateAsync(1, 1, request)
                .Returns(updated);

            ControllerTestHelper.SetUser(_controller, userId: 1);

            var result = await _controller.Update(1, request) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task Update_NotOwner_ReturnsForbid()
        {
            var request = new ListingRequest { BookId = 1, Condition = ListingCondition.Good, Location = "NYC" };

            _listingServiceMock
                .UpdateAsync(2, 1, request)
                .Throws(new UnauthorizedAccessException("Not the owner."));

            ControllerTestHelper.SetUser(_controller, userId: 2);

            var result = await _controller.Update(1, request) as ForbidResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Update_LockedListing_Returns400()
        {
            var request = new ListingRequest { BookId = 1, Condition = ListingCondition.Good, Location = "NYC" };

            _listingServiceMock
                .UpdateAsync(1, 1, request)
                .Throws(new InvalidOperationException("Listing is locked."));

            ControllerTestHelper.SetUser(_controller, userId: 1);

            var result = await _controller.Update(1, request) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task Delete_Valid_Returns204()
        {
            _listingServiceMock
                .DeleteAsync(1, 1)
                .Returns(Task.CompletedTask);

            ControllerTestHelper.SetUser(_controller, userId: 1);

            var result = await _controller.Delete(1) as NoContentResult;

            Assert.NotNull(result);
            Assert.Equal(204, result.StatusCode);
        }

        [Fact]
        public async Task Delete_NotFound_Returns404()
        {
            _listingServiceMock
                .DeleteAsync(1, 99)
                .Throws(new KeyNotFoundException("Listing 99 not found."));

            ControllerTestHelper.SetUser(_controller, userId: 1);

            var result = await _controller.Delete(99) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task Delete_NotOwner_ReturnsForbid()
        {
            _listingServiceMock
                .DeleteAsync(2, 1)
                .Throws(new UnauthorizedAccessException("Not the owner."));

            ControllerTestHelper.SetUser(_controller, userId: 2);

            var result = await _controller.Delete(1) as ForbidResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Delete_LockedListing_Returns400()
        {
            _listingServiceMock
                .DeleteAsync(1, 1)
                .Throws(new InvalidOperationException("Listing is locked."));

            ControllerTestHelper.SetUser(_controller, userId: 1);

            var result = await _controller.Delete(1) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }
    }
}

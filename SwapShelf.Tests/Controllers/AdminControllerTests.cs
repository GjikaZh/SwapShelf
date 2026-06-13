using Microsoft.AspNetCore.Mvc;
using SwapShelf.Controllers;
using SwapShelf.DTOs;
using SwapShelf.Services.Interfaces;
using SwapShelf.Tests.Helpers;

namespace SwapShelf.Tests.Controllers
{
    public class AdminControllerTests
    {
        private readonly IAdminService _adminServiceMock = Substitute.For<IAdminService>();
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            _controller = new AdminController(_adminServiceMock);
            ControllerTestHelper.SetUser(_controller, userId: 999, role: "Admin");
        }

        // ── GetAllUsers ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllUsers_ReturnsOk()
        {
            _adminServiceMock
                .GetAllUsersAsync()
                .Returns(new List<AdminUserResponse>
                {
                    new() { Id = 1, FullName = "Alice", Email = "alice@test.com" }
                });

            var result = await _controller.GetAllUsers() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        // ── BanUser ───────────────────────────────────────────────────────────

        [Fact]
        public async Task BanUser_ValidUser_Returns200()
        {
            _adminServiceMock
                .BanUserAsync(1)
                .Returns(Task.CompletedTask);

            var result = await _controller.BanUser(1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task BanUser_UserNotFound_Returns404()
        {
            _adminServiceMock
                .BanUserAsync(99)
                .Throws(new KeyNotFoundException("User 99 not found."));

            var result = await _controller.BanUser(99) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task BanUser_AdminUser_Returns400()
        {
            _adminServiceMock
                .BanUserAsync(2)
                .Throws(new InvalidOperationException("Cannot ban an admin."));

            var result = await _controller.BanUser(2) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        // ── UnbanUser ─────────────────────────────────────────────────────────

        [Fact]
        public async Task UnbanUser_ValidUser_Returns200()
        {
            _adminServiceMock
                .UnbanUserAsync(1)
                .Returns(Task.CompletedTask);

            var result = await _controller.UnbanUser(1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task UnbanUser_NotFound_Returns404()
        {
            _adminServiceMock
                .UnbanUserAsync(99)
                .Throws(new KeyNotFoundException("User 99 not found."));

            var result = await _controller.UnbanUser(99) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        // ── GetAllListings ────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllListings_ReturnsOk()
        {
            _adminServiceMock
                .GetAllListingsAsync()
                .Returns(new List<ListingResponse>
                {
                    new() { Id = 1, UserId = 1 }
                });

            var result = await _controller.GetAllListings() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        // ── DeleteListing ─────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteListing_Valid_Returns200()
        {
            _adminServiceMock
                .DeleteListingAsync(1)
                .Returns(Task.CompletedTask);

            var result = await _controller.DeleteListing(1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task DeleteListing_NotFound_Returns404()
        {
            _adminServiceMock
                .DeleteListingAsync(99)
                .Throws(new KeyNotFoundException("Listing 99 not found."));

            var result = await _controller.DeleteListing(99) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task DeleteListing_Locked_Returns400()
        {
            _adminServiceMock
                .DeleteListingAsync(1)
                .Throws(new InvalidOperationException("Cannot delete a listing that is part of an active swap."));

            var result = await _controller.DeleteListing(1) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }
    }
}

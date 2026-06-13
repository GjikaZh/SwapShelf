using Microsoft.AspNetCore.Mvc;
using SwapShelf.Controllers;
using SwapShelf.DTOs;
using SwapShelf.Services.Interfaces;
using SwapShelf.Tests.Helpers;

namespace SwapShelf.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock = new();
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _controller = new AuthController(_authServiceMock.Object);
        }

        [Fact]
        public async Task Register_ValidRequest_Returns200WithToken()
        {
            var authResponse = new AuthResponse
            {
                Token = "tok",
                Email = "a@b.com",
                FullName = "A",
                Role = "User"
            };
            _authServiceMock
                .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
                .ReturnsAsync(authResponse);

            var result = await _controller.Register(new RegisterRequest
            {
                FullName = "A",
                Email = "a@b.com",
                Password = "pass123"
            }) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns400()
        {
            _authServiceMock
                .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
                .ThrowsAsync(new InvalidOperationException("Email taken"));

            var result = await _controller.Register(new RegisterRequest
            {
                FullName = "A",
                Email = "a@b.com",
                Password = "pass123"
            }) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task Login_ValidCredentials_Returns200WithToken()
        {
            var authResponse = new AuthResponse
            {
                Token = "tok",
                Email = "a@b.com",
                FullName = "A",
                Role = "User"
            };
            _authServiceMock
                .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(authResponse);

            var result = await _controller.Login(new LoginRequest
            {
                Email = "a@b.com",
                Password = "pass123"
            }) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task Login_WrongCredentials_Returns401()
        {
            _authServiceMock
                .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

            var result = await _controller.Login(new LoginRequest
            {
                Email = "a@b.com",
                Password = "wrongpass"
            }) as UnauthorizedObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }
    }
}

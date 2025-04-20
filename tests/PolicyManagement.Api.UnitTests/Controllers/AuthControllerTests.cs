//Generated using AI
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolicyManagement.Application.Contracts.Identity;
using PolicyManagement.Domain.Entities.DefaultDb.Identity;
using PolicyManagementApp.Api.Controllers;
using Xunit;

namespace PolicyManagement.Api.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AuthController(_mockAuthService.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkObjectResult()
    {
        // Arrange
        var authRequest = new AuthenticationRequest
        {
            Username = "testuser",
            Password = "testpassword"
        };

        var authResponse = new AuthenticationResponse
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            JwtToken = "test-jwt-token",
            TokenExpires = DateTime.UtcNow.AddHours(1)
        };

        _mockAuthService
            .Setup(s => s.AuthenticateAsync(authRequest))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Login(authRequest);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(authResponse);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorizedResult()
    {
        // Arrange
        var authRequest = new AuthenticationRequest
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        _mockAuthService
            .Setup(s => s.AuthenticateAsync(authRequest))
            .ThrowsAsync(new Exception("Invalid credentials"));

        // Act
        var result = await _controller.Login(authRequest);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        // Just verify the result contains the expected message somewhere
        var objectResult = (UnauthorizedObjectResult)result.Result;
        objectResult.Value.ToString().Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task Login_WithInvalidModel_ReturnsBadRequestResult()
    {
        // Arrange
        var authRequest = new AuthenticationRequest(); // Invalid model
        _controller.ModelState.AddModelError("Username", "Required");

        // Act
        var result = await _controller.Login(authRequest);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }
} 
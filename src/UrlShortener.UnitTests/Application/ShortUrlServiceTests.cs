using FluentAssertions;
using FluentAssertions.LanguageExt;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UrlShortener.Application;
using UrlShortener.Domain.Contracts;
using UrlShortener.Domain.Models;
using UrlShortener.Domain.OperationErrors;

namespace UrlShortener.UnitTests.Application;

public class ShortUrlServiceTests
{
    private readonly Mock<ISystemClock> _systemClockMock;
    private readonly Mock<IRepository> _repositoryMock;
    private readonly Mock<IUrlShortenerGenerator> _generatorMock;
    private readonly ShortUrlService _systemUnderTest;

    public ShortUrlServiceTests()
    {
        _systemClockMock = new Mock<ISystemClock>();
        _repositoryMock = new Mock<IRepository>();
        _generatorMock = new Mock<IUrlShortenerGenerator>();
        _systemUnderTest = new ShortUrlService(
            _systemClockMock.Object, 
            NullLogger<ShortUrlService>.Instance, 
            _repositoryMock.Object, 
            _generatorMock.Object);
    }
    
    [Fact]
    public async Task GetById_EntityNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var id = "nonexistentId";
        _repositoryMock.Setup(r => r.GetById(It.IsAny<string>()))
            .ReturnsAsync((ShortUrl)null);

        // Act
        var result = await _systemUnderTest.GetById(id);

        // Assert
        result.Should().BeLeft(l => l.Should().Be(RetrieveError.NotFound));
    }

    [Fact]
    public async Task GetById_EntityInvalid_ShouldReturnGone()
    {
        // Arrange
        var id = "invalidId";
        var entity = new ShortUrl { Id = id, Url = "foo.bar", CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10), TtlMinutes = 5 };
        _repositoryMock.Setup(r => r.GetById(id))
            .ReturnsAsync(entity);
        _systemClockMock.SetupGet(s => s.UtcNow).Returns(DateTimeOffset.UtcNow);

        // Act
        var result = await _systemUnderTest.GetById(id);

        // Assert
        result.Should().BeLeft(l => l.Should().Be(RetrieveError.Gone));
    }

    [Fact]
    public async Task GetById_ValidEntity_ShouldReturnUrl()
    {
        // Arrange
        var id = "validId";
        var url = "https://example.com";
        var entity = new ShortUrl { Id = id, Url = url, CreatedAt = DateTimeOffset.UtcNow };
        _repositoryMock.Setup(r => r.GetById(It.IsAny<string>()))
            .ReturnsAsync(entity);
        _systemClockMock.SetupGet(s => s.UtcNow).Returns(DateTimeOffset.UtcNow);

        // Act
        var result = await _systemUnderTest.GetById(id);

        // Assert
        result.Should().BeRight(r => r.Should().Be(url));
    }
    
    [Fact]
    public async Task Create_WithId_ShouldReturnCreateResponse_WhenRequestIsValid()
    {
        // Arrange
        var id = "test";
        var url = "https://example.com";
        var request = new CreateRequest(url, 60);
        _repositoryMock.Setup(r => r.GetById(It.IsAny<string>())).ReturnsAsync((ShortUrl)null);
        _systemClockMock.Setup(c => c.UtcNow).Returns(DateTimeOffset.UtcNow);

        // Act
        var result = await _systemUnderTest.Create(id, request);

        // Assert
        result.Should().BeRight(r => r.Url.Should().Be(url));
        _repositoryMock.Verify(r => r.Create(It.IsAny<ShortUrl>()), Times.Once);
    }
    
    [Fact]
    public async Task Create_WithId_ShouldReturnError_WhenUrlIsInvalid()
    {
        // Arrange
        var id = "test";
        var request = new CreateRequest("invalid-url", 60);
    
        // Act
        var result = await _systemUnderTest.Create(id, request);

        // Assert
        result.Should().BeLeft(l => l.Should().Be(CreateError.BadUrl));
    }
    
    [Fact]
    public async Task Create_WithId_ShouldReturnError_WhenIdAlreadyExists()
    {
        // Arrange
        var id = "test";
        var request = new CreateRequest("https://example.com", 60);
        var existingEntity = new ShortUrl { Id = id, Url = "https://example.com", TtlMinutes = 60, CreatedAt = DateTimeOffset.UtcNow };

        _repositoryMock.Setup(r => r.GetById(It.IsAny<string>())).ReturnsAsync(existingEntity);

        // Act
        var result = await _systemUnderTest.Create(id, request);

        // Assert
        result.Should().BeLeft(l => l.Should().Be(CreateError.AlreadyExists));
    }
    
    [Fact]
    public async Task Create_ShouldReturnCreateResponse_WhenRequestIsValid()
    {
        // Arrange
        var id = "generatedTestId";
        var url = "https://example.com";
        var request = new CreateRequest(url, 60);
        _generatorMock.Setup(g => g.Generate(It.IsAny<string>())).Returns(id);
        _repositoryMock.Setup(r => r.GetById(It.IsAny<string>())).ReturnsAsync((ShortUrl)null);
        _systemClockMock.Setup(c => c.UtcNow).Returns(DateTimeOffset.UtcNow);

        // Act
        var result = await _systemUnderTest.Create(request);

        // Assert
        result.Should().BeRight(r => r.Url.Should().Be(url));
        _repositoryMock.Verify(r => r.Create(It.IsAny<ShortUrl>()), Times.Once);
    }
    
    [Fact]
    public async Task Create_ShouldReturnError_WhenUrlIsInvalid()
    {
        // Arrange
        var request = new CreateRequest("invalid-url", 60);
    
        // Act
        var result = await _systemUnderTest.Create(request);

        // Assert
        result.Should().BeLeft(l => l.Should().Be(CreateError.BadUrl));
    }
    
    [Fact]
    public async Task Create_ShouldReturnError_WhenGeneratedIdAlreadyExists()
    {
        // Arrange
        var request = new CreateRequest("https://example.com", 60);
        var id = "generatedTestId";
        var existingEntity = new ShortUrl { Id = id, Url = "https://example.com", TtlMinutes = 60, CreatedAt = DateTimeOffset.UtcNow };

        _generatorMock.Setup(g => g.Generate(It.IsAny<string>())).Returns(id);
        _repositoryMock.Setup(r => r.GetById(It.IsAny<string>())).ReturnsAsync(existingEntity);

        // Act
        var result = await _systemUnderTest.Create(request);

        // Assert
        result.Should().BeLeft(l => l.Should().Be(CreateError.AlreadyExists));
    }
    
    [Fact]
    public async Task DeleteById_ShouldReturnUnit_WhenEntityExists()
    {
        // Arrange
        var id = "test";
        var entity = new ShortUrl { Id = id, Url = "https://example.com" };
        _repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync(entity);

        // Act
        var result = await _systemUnderTest.DeleteById(id);

        // Assert
        result.Should().BeRight();
        _repositoryMock.Verify(r => r.Delete(entity), Times.Once);
    }
    
    [Fact]
    public async Task DeleteById_ShouldReturnNotFoundError_WhenEntityDoesNotExist()
    {
        // Arrange
        var id = "test";
        _repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync((ShortUrl)null);

        // Act
        var result = await _systemUnderTest.DeleteById(id);

        // Assert
        result.Should().BeLeft(l => l.Should().Be(DeleteError.NotFound));
        _repositoryMock.Verify(r => r.Delete(It.IsAny<ShortUrl>()), Times.Never);
    }
}
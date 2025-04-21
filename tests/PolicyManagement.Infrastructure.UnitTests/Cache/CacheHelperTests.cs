using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using PolicyManagement.Infrastructure.Cache;

namespace PolicyManagement.Infrastructure.UnitTests.Cache;

public class CacheHelperTests
{
    private readonly Mock<IMemoryCache> _memoryCacheMock;
    private readonly Mock<IOutputCacheStore> _outputCacheStoreMock;
    private readonly CacheHelper _cacheHelper;

    public CacheHelperTests()
    {
        _memoryCacheMock = new Mock<IMemoryCache>();
        _outputCacheStoreMock = new Mock<IOutputCacheStore>();
        _cacheHelper = new CacheHelper(_memoryCacheMock.Object, _outputCacheStoreMock.Object);
    }

    [Fact]
    public void TryGetValue_WhenKeyExists_ShouldReturnTrueAndValue()
    {
        // Arrange
        var cacheKey = "test-key";
        var expectedValue = "test-value";
        
        object retrievedValue = expectedValue;
        _memoryCacheMock.Setup(m => m.TryGetValue(cacheKey, out retrievedValue)).Returns(true);

        // Act
        bool result = _cacheHelper.TryGetValue(cacheKey, out string actualValue);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void TryGetValue_WhenKeyDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var cacheKey = "non-existent-key";
        
        object retrievedValue = null;
        _memoryCacheMock.Setup(m => m.TryGetValue(cacheKey, out retrievedValue)).Returns(false);

        // Act
        bool result = _cacheHelper.TryGetValue(cacheKey, out string actualValue);

        // Assert
        Assert.False(result);
        Assert.Null(actualValue);
    }

    [Fact]
    public void Set_WithDefaultExpiration_ShouldSetValueWithDefaultSlidingExpiration()
    {
        // Arrange
        var cacheKey = "test-key";
        var value = "test-value";
        
        var memoryCacheEntryMock = new Mock<ICacheEntry>();
        _memoryCacheMock.Setup(m => m.CreateEntry(cacheKey)).Returns(memoryCacheEntryMock.Object);

        // Act
        _cacheHelper.Set(cacheKey, value);

        // Assert
        _memoryCacheMock.Verify(m => m.CreateEntry(cacheKey), Times.Once);
        memoryCacheEntryMock.VerifySet(
            e => e.Value = value,
            Times.Once);
        memoryCacheEntryMock.VerifySet(
            e => e.SlidingExpiration = It.Is<TimeSpan>(t => t.TotalMinutes == 15),
            Times.Once);
    }

    [Fact]
    public void Set_WithCustomExpiration_ShouldSetValueWithSpecifiedExpiration()
    {
        // Arrange
        var cacheKey = "test-key";
        var value = "test-value";
        var customExpiration = TimeSpan.FromMinutes(30);
        
        var memoryCacheEntryMock = new Mock<ICacheEntry>();
        _memoryCacheMock.Setup(m => m.CreateEntry(cacheKey)).Returns(memoryCacheEntryMock.Object);

        // Act
        _cacheHelper.Set(cacheKey, value, customExpiration);

        // Assert
        _memoryCacheMock.Verify(m => m.CreateEntry(cacheKey), Times.Once);
        memoryCacheEntryMock.VerifySet(
            e => e.Value = value,
            Times.Once);
        memoryCacheEntryMock.VerifySet(
            e => e.SlidingExpiration = customExpiration,
            Times.Once);
    }

    [Fact]
    public void InvalidateCache_ShouldRemoveAllCachedKeys()
    {
        // Arrange
        var cacheKey1 = "key1";
        var cacheKey2 = "key2";
        var value = "test-value";
        
        var memoryCacheEntryMock1 = new Mock<ICacheEntry>();
        var memoryCacheEntryMock2 = new Mock<ICacheEntry>();
        _memoryCacheMock.Setup(m => m.CreateEntry(cacheKey1)).Returns(memoryCacheEntryMock1.Object);
        _memoryCacheMock.Setup(m => m.CreateEntry(cacheKey2)).Returns(memoryCacheEntryMock2.Object);

        // Act
        _cacheHelper.Set(cacheKey1, value);
        _cacheHelper.Set(cacheKey2, value);
        _cacheHelper.InvalidateCache();

        // Assert
        _memoryCacheMock.Verify(m => m.Remove(cacheKey1), Times.Once);
        _memoryCacheMock.Verify(m => m.Remove(cacheKey2), Times.Once);
    }
} 
using SensorApi.Utility;

namespace SensorApi.Tests.Utility;

public class AgainstNullOrEmptyWithNullOrWhitespaceValuesTests
{

    [Fact]
    public void AgainstNullOrEmptyWithNullOrWhitespaceValues_WithValidStrings_DoesNotThrow()
    {
        var strings = new List<string> { "value1", "value2", "value3" };

        var exception = Record.Exception(() => Guard.AgainstNullOrEmptyWithNullOrWhitespaceValues(strings, nameof(strings)));
        Assert.Null(exception);
    }

    [Fact]
    public void AgainstNullOrEmptyWithNullOrWhitespaceValues_WithNullCollection_ThrowsArgumentException()
    {
        List<string>? strings = null;

        var exception = Assert.Throws<ArgumentException>(() =>
            Guard.AgainstNullOrEmptyWithNullOrWhitespaceValues(strings!, "strings"));
        Assert.Contains("strings", exception.Message);
        Assert.Contains("null or empty", exception.Message);
    }

    [Fact]
    public void AgainstNullOrEmptyWithNullOrWhitespaceValues_WithEmptyCollection_ThrowsArgumentException()
    {
        var strings = new List<string>();

        var exception = Assert.Throws<ArgumentException>(() =>
            Guard.AgainstNullOrEmptyWithNullOrWhitespaceValues(strings, "strings"));
        Assert.Contains("strings", exception.Message);
        Assert.Contains("null or empty", exception.Message);
    }

    [Fact]
    public void AgainstNullOrEmptyWithNullOrWhitespaceValues_WithNullString_ThrowsArgumentException()
    {
        var strings = new List<string?> { "value1", null, "value2" };

        var exception = Assert.Throws<ArgumentException>(() =>
            Guard.AgainstNullOrEmptyWithNullOrWhitespaceValues(strings!, "strings"));
        Assert.Contains("strings", exception.Message);
        Assert.Contains("null or whitespace", exception.Message);
    }

    [Fact]
    public void AgainstNullOrEmptyWithNullOrWhitespaceValues_WithEmptyString_ThrowsArgumentException()
    {
        var strings = new List<string> { "value1", "", "value2" };

        var exception = Assert.Throws<ArgumentException>(() =>
            Guard.AgainstNullOrEmptyWithNullOrWhitespaceValues(strings, "strings"));
        Assert.Contains("strings", exception.Message);
        Assert.Contains("null or whitespace", exception.Message);
    }

    [Fact]
    public void AgainstNullOrEmptyWithNullOrWhitespaceValues_WithWhitespaceString_ThrowsArgumentException()
    {
        var strings = new List<string> { "value1", "   ", "value2" };

        var exception = Assert.Throws<ArgumentException>(() =>
            Guard.AgainstNullOrEmptyWithNullOrWhitespaceValues(strings, "strings"));
        Assert.Contains("strings", exception.Message);
        Assert.Contains("null or whitespace", exception.Message);
    }

    [Fact]
    public void AgainstNullOrEmptyWithNullOrWhitespaceValues_WithTabString_ThrowsArgumentException()
    {
        var strings = new List<string> { "value1", "\t", "value2" };

        var exception = Assert.Throws<ArgumentException>(() =>
            Guard.AgainstNullOrEmptyWithNullOrWhitespaceValues(strings, "strings"));
        Assert.Contains("strings", exception.Message);
        Assert.Contains("null or whitespace", exception.Message);
    }

    [Fact]
    public void AgainstNull_WithComplexObject_ReturnsObject()
    {
        var obj = new { Name = "Test", Value = 42 };

        var result = Guard.AgainstNull(obj, nameof(obj));

        Assert.Equal(obj, result);
    }
}

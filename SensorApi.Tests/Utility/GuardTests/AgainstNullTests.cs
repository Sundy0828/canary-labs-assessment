using SensorApi.Utility;

namespace SensorApi.Tests.Utility;

public class AgainstNullTests
{
    [Fact]
    public void AgainstNull_WithNonNullValue_ReturnsValue()
    {
        var value = "test";

        var result = Guard.AgainstNull(value, nameof(value));

        Assert.Equal(value, result);
    }

    [Fact]
    public void AgainstNull_WithNullValue_ThrowsArgumentNullException()
    {
        string? value = null;

        var exception = Assert.Throws<ArgumentNullException>(() => Guard.AgainstNull(value!, "testParam"));
        Assert.Contains("testParam", exception.Message);
    }

    [Fact]
    public void AgainstNull_WithNullValueAndNoName_ThrowsWithEmptyName()
    {
        string? value = null;

        Assert.Throws<ArgumentNullException>(() => Guard.AgainstNull(value!));
    }
}

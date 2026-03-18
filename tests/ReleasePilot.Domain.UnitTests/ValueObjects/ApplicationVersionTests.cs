using ReleasePilot.Domain.Exceptions;
using ReleasePilot.Domain.ValueObjects;

namespace ReleasePilot.Domain.UnitTests.ValueObjects;

public class ApplicationVersionTests
{
    [Fact]
    public void Create_WithValidValue_ReturnsApplicationVersion()
    {
        var version = ApplicationVersion.Create("1.0.0");

        Assert.Equal("1.0.0", version.Value);
    }

    [Fact]
    public void Create_WithArbitraryString_ReturnsApplicationVersion()
    {
        var version = ApplicationVersion.Create("v2.3.1-beta");

        Assert.Equal("v2.3.1-beta", version.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ThrowsDomainException(string? value)
    {
        Assert.Throws<DomainException>(() => ApplicationVersion.Create(value!));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var version = ApplicationVersion.Create("3.2.1");

        Assert.Equal("3.2.1", version.ToString());
    }

    [Fact]
    public void Equality_TwoInstancesWithSameValue_AreEqual()
    {
        var version1 = ApplicationVersion.Create("1.0.0");
        var version2 = ApplicationVersion.Create("1.0.0");

        Assert.Equal(version1, version2);
    }

    [Fact]
    public void Equality_TwoInstancesWithDifferentValues_AreNotEqual()
    {
        var version1 = ApplicationVersion.Create("1.0.0");
        var version2 = ApplicationVersion.Create("2.0.0");

        Assert.NotEqual(version1, version2);
    }
}
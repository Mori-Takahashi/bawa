using __APP_NAME__.Kit;
using Xunit;

namespace __APP_NAME__.Kit.Tests;

public class ItemTitleTests
{
    [Theory]
    [InlineData("  Buy   milk ", "Buy milk")]
    [InlineData("\tCall\nMom", "Call Mom")]
    [InlineData("", "")]
    public void Clean_collapses_whitespace(string raw, string expected)
        => Assert.Equal(expected, ItemTitle.Clean(raw));
}

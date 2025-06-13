namespace AideMemoire.Tests;

public class ApplicationTests {
    [Fact]
    public async Task RunAsync_WithEmptyArgs_ReturnsZero() {
        var app = new Application();
        var args = Array.Empty<string>();

        var result = await app.RunAsync(args);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task RunAsync_WithVersionCommand_ReturnsZero() {
        var app = new Application();
        var args = new[] { "version" };

        var result = await app.RunAsync(args);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task RunAsync_WithHelpOption_ReturnsZero() {
        var app = new Application();
        var args = new[] { "--help" };

        var result = await app.RunAsync(args);

        Assert.Equal(0, result);
    }
}

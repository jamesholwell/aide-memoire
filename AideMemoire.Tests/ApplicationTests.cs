namespace AideMemoire.Tests;

public class ApplicationTests {
    [Fact]
    public async Task RunAsync_WithEmptyArgs_DoesNotThrow() {
        var app = new Application();
        var args = Array.Empty<string>();

        await app.RunAsync(args);
    }

    [Fact]
    public async Task RunAsync_WithVersionCommand_DoesNotThrow() {
        var app = new Application();
        var args = new[] { "version" };

        await app.RunAsync(args);
    }
}

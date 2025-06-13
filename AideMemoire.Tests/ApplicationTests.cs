using System.CommandLine;
using System.CommandLine.IO;

namespace AideMemoire.Tests;

public class ApplicationTests {
    private IConsole console = new TestConsole();

    private Application CreateApplication() {
        return new Application(console);
    }

    [Fact]
    public async Task RunAsync_WithEmptyArgs_ReturnsZero() {
        Application app = CreateApplication();

        var args = Array.Empty<string>();
        var result = await app.RunAsync(args);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task RunAsync_WithHelpOption_ReturnsZero() {
        Application app = CreateApplication();

        var args = new[] { "--help" };
        var result = await app.RunAsync(args);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task RunAsync_WithAboutCommand_OutputsCorrectInformation() {
        Application app = CreateApplication();

        var args = new[] { "about" };
        await app.RunAsync(args);

        var output = console.Out.ToString();
        Assert.Contains("aide-mémoire:", output);
        Assert.Contains("v0.1", output);
    }
}

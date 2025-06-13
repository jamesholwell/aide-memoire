using System.CommandLine;

namespace AideMemoire;

public class Application {
    public async Task<int> RunAsync(string[] args) {
        var rootCommand = new RootCommand("aide-mémoire");

        // add about command
        var aboutCommand = new Command("about", "Show information about the application");
        aboutCommand.SetHandler(ShowAboutInformation);
        rootCommand.AddCommand(aboutCommand);

        // default handler (for when no command is specified)
        rootCommand.SetHandler(ShowAboutInformation);

        return await rootCommand.InvokeAsync(args);
    }

    private static void ShowAboutInformation() {
        Console.WriteLine("aide-mémoire: n. a thing, especially a book or document, that helps you to remember something");
        Console.WriteLine("v0.1");
        Console.WriteLine(string.Empty);
    }
}

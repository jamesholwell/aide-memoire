using System.CommandLine;

namespace AideMemoire.Commands;

public class AboutCommand : IApplicationCommand {
    public void RegisterCommand(RootCommand root) {
        var aboutCommand = new Command("about", "Show information about the application");
        aboutCommand.SetHandler(ctx => ShowAboutInformation(ctx.Console));

        root.AddCommand(aboutCommand);
    }

    internal static void ShowAboutInformation(IConsole console) {
        console.WriteLine("aide-mémoire: n. a thing, especially a book or document, that helps you to remember something");
        console.WriteLine("v0.1");
        console.WriteLine(string.Empty);
    }
}

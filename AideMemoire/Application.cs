using System.CommandLine;
using System.CommandLine.IO;
using AideMemoire.Commands;

namespace AideMemoire;

public class Application(IConsole console) {
    private readonly IConsole console = console;

    public Application() : this(new SystemConsole()) { }

    public async Task<int> RunAsync(string[] args) {
        var rootCommand = new RootCommand("aide-mémoire");

        // Use reflection to discover and add all command handlers
        DiscoverCommands(rootCommand);

        // default handler (for when no command is specified)
        rootCommand.SetHandler(DefaultHandler);

        return await rootCommand.InvokeAsync(args, console);
    }

    private void DiscoverCommands(RootCommand rootCommand) {
        var commandTypes = typeof(AboutCommand).Assembly
            .GetTypes()
            .Where(type =>
                !type.IsInterface
                && !type.IsAbstract
                && typeof(IApplicationCommand).IsAssignableFrom(type));

        foreach (var commandType in commandTypes) {
            if (Activator.CreateInstance(commandType) is not IApplicationCommand command) {
                throw new InvalidOperationException($"Could not create instance of {commandType.Name}");
            }

            command.RegisterCommand(rootCommand);
        }
    }

    private void DefaultHandler() {
        AboutCommand.ShowAboutInformation(console);
    }
}

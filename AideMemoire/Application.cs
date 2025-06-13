using System.CommandLine;
using System.CommandLine.IO;
using AideMemoire.Handlers;

namespace AideMemoire;

public class Application(IConsole console) {
    private readonly IConsole console = console;

    public Application() : this(new SystemConsole()) { }

    public async Task<int> RunAsync(string[] args) {
        var rootCommand = new RootCommand("aide-mémoire");

        // Use reflection to discover and add all command handlers
        DiscoverHandlers(rootCommand);

        // default handler (for when no command is specified)
        rootCommand.SetHandler(DefaultHandler);

        return await rootCommand.InvokeAsync(args, console);
    }

    private void DiscoverHandlers(RootCommand rootCommand) {
        var handlerTypes = typeof(AboutHandler).Assembly
            .GetTypes()
            .Where(type =>
                !type.IsInterface
                && !type.IsAbstract
                && typeof(IApplicationHandler).IsAssignableFrom(type));

        foreach (var handlerType in handlerTypes) {
            if (Activator.CreateInstance(handlerType) is not IApplicationHandler handler) {
                throw new InvalidOperationException($"Could not create instance of {handlerType.Name}");
            }

            handler.RegisterCommand(rootCommand);
        }
    }

    private void DefaultHandler() {
        AboutHandler.ShowAboutInformation(console);
    }
}

namespace AideMemoire;

public class Application {
    public async Task RunAsync(string[] args) {
        if (args.Length == 0) {
            ShowVersion();
            return;
        }

        var command = args[0].ToLowerInvariant();
        
        switch (command) {
            case "version":
            case "--version":
            case "-v":
                ShowVersion();
                break;
            
            default:
                Console.WriteLine($"Unknown command: {command}");
                break;
        }
    }

    private static void ShowVersion() {
        Console.WriteLine("aide-mémoire: n. a thing, especially a book or document, that helps you to remember something");
        Console.WriteLine("v0.1");
        Console.WriteLine(string.Empty);
    }
}

using System.CommandLine;

namespace AideMemoire;

public interface IApplicationHandler {
    /// <summary>
    ///     Registers the handler with the root
    /// </summary>
    void RegisterCommand(RootCommand root);
}

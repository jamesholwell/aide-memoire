using System.CommandLine;

namespace AideMemoire;

public interface IApplicationCommand {
    /// <summary>
    ///     Registers the command with the root
    /// </summary>
    void RegisterCommand(RootCommand root);
}

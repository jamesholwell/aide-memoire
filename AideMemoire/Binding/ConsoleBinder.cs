using System.CommandLine;
using System.CommandLine.Binding;

public class ConsoleBinder : BinderBase<IConsole> {
    protected override IConsole GetBoundValue(BindingContext bindingContext) => bindingContext.Console;
}

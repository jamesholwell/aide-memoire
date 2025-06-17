using System.CommandLine.Binding;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AideMemoire.Binding;

public class MediatorBinder : BinderBase<IMediator> {
    protected override IMediator GetBoundValue(BindingContext bindingContext) {
        return Program.Host.Services.GetRequiredService<IMediator>();
    }
}

using System.CommandLine.Binding;
using AideMemoire.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

public class MemoryRepositoryBinder : BinderBase<IMemoryRepository> {
    protected override IMemoryRepository GetBoundValue(BindingContext bindingContext) =>
        Program.Host.Services.GetRequiredService<IMemoryRepository>();
}

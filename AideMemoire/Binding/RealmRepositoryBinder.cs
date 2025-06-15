using System.CommandLine.Binding;
using AideMemoire.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

public class RealmRepositoryBinder : BinderBase<IRealmRepository> {
    protected override IRealmRepository GetBoundValue(BindingContext bindingContext) =>
        Program.Host.Services.GetRequiredService<IRealmRepository>();
}

using System.CommandLine.Binding;
using Microsoft.Extensions.DependencyInjection;

public class HttpClientFactoryBinder : BinderBase<IHttpClientFactory> {
    protected override IHttpClientFactory GetBoundValue(BindingContext bindingContext) =>
        Program.Host.Services.GetRequiredService<IHttpClientFactory>();
}

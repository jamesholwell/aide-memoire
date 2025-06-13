using AideMemoire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();

// register services
builder.Services
    .AddHttpClient()
    .ConfigureHttpClientDefaults(builder => {
        builder.ConfigureHttpClient(client =>{
            client.DefaultRequestHeaders.UserAgent.ParseAdd("aide-memoire/0.1");
        });
    });

builder.Services.AddSingleton<Application>();

// execute
host = builder.Build();
await host.Services.GetRequiredService<Application>().RunAsync(args);

public partial class Program {
    private static IHost? host;

    internal static IHost Host { get => host!; }
}
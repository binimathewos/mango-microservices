using Mango.Service.EmailApi.Messaging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Hosting.Internal;

namespace Mango.Service.EmailApi.Extensions;

public static class ApplicationBuilderExtensions
{
    private static IAzureServiceBusConsumer? AzureServiceBusConsumer { get; set; }
    public static IApplicationBuilder UseAzureServiceBusConsumer(this IApplicationBuilder app)
    {
        AzureServiceBusConsumer = app.ApplicationServices.GetService<IAzureServiceBusConsumer>();
        IHostApplicationLifetime? applicationLifetime = app.ApplicationServices.GetService<IHostApplicationLifetime>();

        applicationLifetime?.ApplicationStarted.Register(OnStart);
        applicationLifetime?.ApplicationStopped.Register(OnStop);

        return app;
    }

    private static void OnStop()
    {
        AzureServiceBusConsumer?.Stop();
    }

    private static void OnStart()
    {
        AzureServiceBusConsumer?.Start();
    }
}
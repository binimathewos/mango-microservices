using Mango.Service.EmailApi.Messaging;

namespace Mango.Service.EmailApi.Extensions;

public static class ApplicationBuilderExtensions
{
    private static IRabbitMqConsumer? RabbitMqConsumer { get; set; }

    public static IApplicationBuilder UseRabbitMqConsumer(this IApplicationBuilder app)
    {
        RabbitMqConsumer = app.ApplicationServices.GetService<IRabbitMqConsumer>();
        IHostApplicationLifetime? applicationLifetime = app.ApplicationServices.GetService<IHostApplicationLifetime>();

        applicationLifetime?.ApplicationStarted.Register(OnStart);
        applicationLifetime?.ApplicationStopped.Register(OnStop);

        return app;
    }

    private static void OnStop()
    {
        RabbitMqConsumer?.Stop().GetAwaiter().GetResult();
    }

    private static void OnStart()
    {
        RabbitMqConsumer?.Start().GetAwaiter().GetResult();
    }
}

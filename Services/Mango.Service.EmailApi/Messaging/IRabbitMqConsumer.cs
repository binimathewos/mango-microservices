namespace Mango.Service.EmailApi.Messaging;

public interface IRabbitMqConsumer
{
    Task Start();
    Task Stop();
}

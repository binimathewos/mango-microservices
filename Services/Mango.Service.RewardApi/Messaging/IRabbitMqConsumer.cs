namespace Mango.Service.RewardApi.Messaging;

public interface IRabbitMqConsumer
{
    Task Start();
    Task Stop();
}

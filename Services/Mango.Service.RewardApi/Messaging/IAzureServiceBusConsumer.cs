namespace Mango.Service.RewardApi.Messaging;

public interface IAzureServiceBusConsumer
{
    Task Start();
    Task Stop();
}
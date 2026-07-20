namespace Mango.Service.EmailApi.Messaging;

public interface IAzureServiceBusConsumer
{
    Task Start();
    Task Stop();
}
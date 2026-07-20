using System.Text;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace Mango.MessageBus;

public class MessageBus : IMessageBus
{
    private string _connectionString = "Endpoint=sb://<your-namespace>.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=<your-shared-access-key>";
    public async Task PublishMessage(object message, string topic_queue_Name)
    {
        await using var client = new ServiceBusClient(_connectionString);

        ServiceBusSender sender = client.CreateSender(topic_queue_Name);

        var jsonMessage = JsonConvert.SerializeObject(message);

        ServiceBusMessage finalMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage))
        {
            CorrelationId = Guid.NewGuid().ToString()
        };

        await sender.SendMessageAsync(finalMessage);
        await client.DisposeAsync();
    }
}

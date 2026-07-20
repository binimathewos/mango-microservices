using System.Text;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace Mango.MessageBus;

public class MessageBus : IMessageBus
{
    private readonly string? _connectionString;

    public MessageBus(string? connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task PublishMessage(object message, string topic_queue_Name)
    {
        // Service Bus not configured (missing or still a placeholder) — skip publishing
        // so a missing messaging setup doesn't break the calling request.
        if (string.IsNullOrWhiteSpace(_connectionString) || _connectionString.Contains("<your-namespace>"))
        {
            return;
        }

        await using var client = new ServiceBusClient(_connectionString);

        ServiceBusSender sender = client.CreateSender(topic_queue_Name);

        var jsonMessage = JsonConvert.SerializeObject(message);

        ServiceBusMessage finalMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage))
        {
            CorrelationId = Guid.NewGuid().ToString()
        };

        await sender.SendMessageAsync(finalMessage);
    }
}

using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Mango.MessageBus;

public class MessageBus : IMessageBus
{
    private readonly string? _hostName;
    private readonly string? _userName;
    private readonly string? _password;

    public MessageBus(string? hostName, string? userName, string? password)
    {
        _hostName = hostName;
        _userName = userName;
        _password = password;
    }

    public async Task PublishMessage(object message, string topic_queue_Name)
    {
        // RabbitMQ not configured (missing or still a placeholder) — skip publishing
        // so a missing messaging setup doesn't break the calling request.
        if (string.IsNullOrWhiteSpace(_hostName) || _hostName.Contains("<your-"))
        {
            return;
        }

        var factory = new ConnectionFactory
        {
            HostName = _hostName,
            UserName = string.IsNullOrWhiteSpace(_userName) ? "guest" : _userName,
            Password = string.IsNullOrWhiteSpace(_password) ? "guest" : _password
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        // Publish to a fanout exchange named after the topic/queue. Each consumer declares
        // its own queue and binds it to this exchange, so single- and multi-consumer flows
        // (e.g. the order-created topic consumed by both Email and Reward) work uniformly.
        await channel.ExchangeDeclareAsync(topic_queue_Name, ExchangeType.Fanout, durable: false);

        var jsonMessage = JsonConvert.SerializeObject(message);
        var body = Encoding.UTF8.GetBytes(jsonMessage);

        await channel.BasicPublishAsync(exchange: topic_queue_Name, routingKey: string.Empty, body: body);
    }
}

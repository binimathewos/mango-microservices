using System.Text;
using Mango.Service.RewardApi.Models.Dto;
using Mango.Service.RewardApi.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Mango.Service.RewardApi.Messaging;

public class RabbitMqConsumer : IRabbitMqConsumer
{
    private readonly RewardsService _rewardsService;
    private readonly string? _hostName;
    private readonly string? _userName;
    private readonly string? _password;
    private readonly string _orderCreatedExchange;
    private readonly string _rewardsQueue;

    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqConsumer(IConfiguration configuration, RewardsService rewardsService)
    {
        _rewardsService = rewardsService;

        _hostName = configuration.GetValue<string>("RabbitMq:HostName");
        _userName = configuration.GetValue<string>("RabbitMq:UserName");
        _password = configuration.GetValue<string>("RabbitMq:Password");

        _orderCreatedExchange = configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic")!;
        _rewardsQueue = configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedSubscription")!;
    }

    public async Task Start()
    {
        // RabbitMQ not configured (missing or still a placeholder) — skip wiring up the
        // consumer so a missing messaging setup doesn't crash app startup.
        if (string.IsNullOrWhiteSpace(_hostName) || _hostName.Contains("<your-"))
        {
            return;
        }

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = string.IsNullOrWhiteSpace(_userName) ? "guest" : _userName,
                Password = string.IsNullOrWhiteSpace(_password) ? "guest" : _password
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Bind a dedicated queue to the order-created fanout exchange so Reward gets its
            // own copy of every order-created message (Email binds its own queue too).
            await _channel.ExchangeDeclareAsync(_orderCreatedExchange, ExchangeType.Fanout, durable: false);
            // Durable queue: RabbitMQ 4.x deprecated transient non-exclusive queues.
            await _channel.QueueDeclareAsync(_rewardsQueue, durable: true, exclusive: false, autoDelete: false);
            await _channel.QueueBindAsync(_rewardsQueue, _orderCreatedExchange, routingKey: string.Empty);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += OnOrderRewardsReceived;
            await _channel.BasicConsumeAsync(_rewardsQueue, autoAck: false, consumer);
        }
        catch (Exception ex)
        {
            // Non-fatal: a missing/unreachable broker shouldn't take the whole service down.
            Console.WriteLine($"RabbitMQ consumer failed to start: {ex}");
        }
    }

    public async Task Stop()
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
            _channel = null;
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    private async Task OnOrderRewardsReceived(object? sender, BasicDeliverEventArgs args)
    {
        var body = Encoding.UTF8.GetString(args.Body.Span);

        RewardsDto rewardsDto = JsonConvert.DeserializeObject<RewardsDto>(body)!;

        await _rewardsService.UpdateRewards(rewardsDto);

        if (_channel is not null)
        {
            await _channel.BasicAckAsync(args.DeliveryTag, multiple: false);
        }
    }
}

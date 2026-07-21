using System.Text;
using Mango.Service.EmailApi.Models.Dto;
using Mango.Service.EmailApi.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Mango.Service.EmailApi.Messaging;

public class RabbitMqConsumer : IRabbitMqConsumer
{
    private readonly EmailService _emailService;
    private readonly string? _hostName;
    private readonly string? _userName;
    private readonly string? _password;

    private readonly string _emailCartQueue;
    private readonly string _registerUserQueue;
    private readonly string _orderCreatedExchange;
    private readonly string _orderCreatedQueue;

    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqConsumer(IConfiguration configuration, EmailService emailService)
    {
        _emailService = emailService;

        _hostName = configuration.GetValue<string>("RabbitMq:HostName");
        _userName = configuration.GetValue<string>("RabbitMq:UserName");
        _password = configuration.GetValue<string>("RabbitMq:Password");

        _emailCartQueue = configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue")!;
        _registerUserQueue = configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue")!;
        _orderCreatedExchange = configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic")!;
        _orderCreatedQueue = configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedSubscription")!;
    }

    public async Task Start()
    {
        // RabbitMQ not configured (missing or still a placeholder) — skip wiring up the
        // consumers so a missing messaging setup doesn't crash app startup.
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

            // Every producer publishes to a fanout exchange named after the queue/topic, so
            // each consumer declares its own queue and binds it to that exchange.
            await BindFanoutQueue(_emailCartQueue, _emailCartQueue);
            await BindFanoutQueue(_registerUserQueue, _registerUserQueue);
            await BindFanoutQueue(_orderCreatedExchange, _orderCreatedQueue);

            await Consume(_emailCartQueue, OnEmailCartReceived);
            await Consume(_registerUserQueue, OnRegisterUserReceived);
            await Consume(_orderCreatedQueue, OnOrderCreatedReceived);
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

    private async Task BindFanoutQueue(string exchange, string queue)
    {
        await _channel!.ExchangeDeclareAsync(exchange, ExchangeType.Fanout, durable: false);
        // Durable queue: RabbitMQ 4.x deprecated transient non-exclusive queues.
        await _channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync(queue, exchange, routingKey: string.Empty);
    }

    private async Task Consume(string queue, AsyncEventHandler<BasicDeliverEventArgs> handler)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel!);
        consumer.ReceivedAsync += handler;
        await _channel!.BasicConsumeAsync(queue, autoAck: false, consumer);
    }

    private async Task OnEmailCartReceived(object? sender, BasicDeliverEventArgs args)
    {
        var body = Encoding.UTF8.GetString(args.Body.Span);
        CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(body)!;

        await _emailService.EmailCartAndLog(cartDto);
        await _channel!.BasicAckAsync(args.DeliveryTag, multiple: false);
    }

    private async Task OnRegisterUserReceived(object? sender, BasicDeliverEventArgs args)
    {
        var body = Encoding.UTF8.GetString(args.Body.Span);
        string emailAddress = JsonConvert.DeserializeObject<string>(body)!;

        await _emailService.RegisterUserEmailAndLog(emailAddress);
        await _channel!.BasicAckAsync(args.DeliveryTag, multiple: false);
    }

    private async Task OnOrderCreatedReceived(object? sender, BasicDeliverEventArgs args)
    {
        var body = Encoding.UTF8.GetString(args.Body.Span);
        RewardsDto rewardsDto = JsonConvert.DeserializeObject<RewardsDto>(body)!;

        await _emailService.LogOrderPlaced(rewardsDto);
        await _channel!.BasicAckAsync(args.DeliveryTag, multiple: false);
    }
}

using System.Text;
using Azure.Messaging.ServiceBus;
using Mango.Service.RewardApi.Models.Dto;
using Mango.Service.RewardApi.Services;
using Newtonsoft.Json;

namespace Mango.Service.RewardApi.Messaging;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
    private readonly IConfiguration _configuration;
    private readonly RewardsService _rewardsService;
    private readonly ILogger<AzureServiceBusConsumer> _logger;
    private readonly ServiceBusProcessor? _rewardsProcessor;


    public AzureServiceBusConsumer(IConfiguration configuration, RewardsService rewardsService, ILogger<AzureServiceBusConsumer> logger)
    {
        _configuration = configuration;
        _rewardsService = rewardsService;
        _logger = logger;

        string? serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");

        // Service Bus not configured (missing or still a placeholder) — skip wiring up
        // the processor so a missing messaging setup doesn't crash app startup.
        if (string.IsNullOrWhiteSpace(serviceBusConnectionString) || serviceBusConnectionString.Contains("<your-namespace>"))
        {
            return;
        }

        var client = new ServiceBusClient(serviceBusConnectionString);

        string orderCreatedTopic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic")!;
        string orderCreatedSubscription = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedSubscription")!;

        _rewardsProcessor = client.CreateProcessor(orderCreatedTopic, orderCreatedSubscription);
    }

    public async Task Start()
    {
        if (_rewardsProcessor is null)
        {
            return;
        }

        _rewardsProcessor.ProcessMessageAsync += OnOrderRewardsReceived;
        _rewardsProcessor.ProcessErrorAsync += OnOrderRewardsError;
        await _rewardsProcessor.StartProcessingAsync();
    }

    public async Task Stop()
    {
        if (_rewardsProcessor is null)
        {
            return;
        }

        await _rewardsProcessor.StopProcessingAsync();
        await _rewardsProcessor.DisposeAsync();
    }

    private Task OnOrderRewardsError(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus error on order-created rewards subscription (source: {ErrorSource})", args.ErrorSource);
        return Task.CompletedTask;
    }

    private async Task OnOrderRewardsReceived(ProcessMessageEventArgs args)
    {
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body);

        try
        {
            RewardsDto rewardsDto = JsonConvert.DeserializeObject<RewardsDto>(body)!;

            await _rewardsService.UpdateRewards(rewardsDto);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to process order-created rewards message {MessageId}", message.MessageId);
            throw;
        }
    }
}

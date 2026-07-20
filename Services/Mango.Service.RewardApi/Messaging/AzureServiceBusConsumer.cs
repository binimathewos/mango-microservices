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
    private ServiceBusProcessor _rewardsProcessor;


    public AzureServiceBusConsumer(IConfiguration configuration, RewardsService rewardsService)
    {
        _configuration = configuration;
        string serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString")!;


        _rewardsService = rewardsService;

        var client = new ServiceBusClient(serviceBusConnectionString);

        string orderCreatedTopic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic")!;
        string orderCreatedSubscription = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedSubscription")!;

        _rewardsProcessor = client.CreateProcessor(orderCreatedTopic, orderCreatedSubscription);
    }

    public async Task Start()
    {
        _rewardsProcessor.ProcessMessageAsync += OnOrderRewardsReceived;
        _rewardsProcessor.ProcessErrorAsync += OnOrderRewardsError;
        await _rewardsProcessor.StartProcessingAsync();
    }

    public async Task Stop()
    {
        await _rewardsProcessor.StopProcessingAsync();
        await _rewardsProcessor.DisposeAsync();
    }

    private Task OnOrderRewardsError(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
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
        catch (System.Exception)
        {
            throw;
        }
    }
}
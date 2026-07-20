using System.Text;
using Azure.Messaging.ServiceBus;
using Mango.Service.EmailApi.Models.Dto;
using Mango.Service.EmailApi.Services;
using Newtonsoft.Json;

namespace Mango.Service.EmailApi.Messaging;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
    private readonly IConfiguration _configuration;
    private readonly EmailService _emailService;
    private ServiceBusProcessor _emailCartProcessor;
    private ServiceBusProcessor _registerUserProcessor;
    private ServiceBusProcessor _orderProcessor;

    public AzureServiceBusConsumer(IConfiguration configuration, EmailService emailService)
    {
        _configuration = configuration;
        string serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString")!;


        _emailService = emailService;

        var client = new ServiceBusClient(serviceBusConnectionString);

        string emailCartQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue")!;
        _emailCartProcessor = client.CreateProcessor(emailCartQueue);

        string registerUserQueue = _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue")!;
        _registerUserProcessor = client.CreateProcessor(registerUserQueue);

        string orderCreatedTopic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic")!;
        string orderCreatedSubscriprion = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedSubscription")!;
        _orderProcessor = client.CreateProcessor(orderCreatedTopic, orderCreatedSubscriprion);
    }

    public async Task Start()
    {
        _emailCartProcessor.ProcessMessageAsync += OnEmailCartReceived;
        _emailCartProcessor.ProcessErrorAsync += OnEmailCartError;
        await _emailCartProcessor.StartProcessingAsync();

        _registerUserProcessor.ProcessMessageAsync += OnRegisterUserReceived;
        _registerUserProcessor.ProcessErrorAsync += OnRegisterUserError;
        await _registerUserProcessor.StartProcessingAsync();

        _orderProcessor.ProcessMessageAsync += OnOrderCreatedReceived;
        _orderProcessor.ProcessErrorAsync += OnOrderCreatedError;
        await _orderProcessor.StartProcessingAsync();
    }

    public async Task Stop()
    {
        await _emailCartProcessor.StopProcessingAsync();
        await _emailCartProcessor.DisposeAsync();

        await _registerUserProcessor.StopProcessingAsync();
        await _registerUserProcessor.DisposeAsync();

        await _orderProcessor.StopProcessingAsync();
        await _orderProcessor.DisposeAsync();
    }

    private Task OnEmailCartError(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }

    private async Task OnEmailCartReceived(ProcessMessageEventArgs args)
    {
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body);

        try
        {
            CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(body)!;

            await _emailService.EmailCartAndLog(cartDto);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    private Task OnRegisterUserError(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }

    private async Task OnRegisterUserReceived(ProcessMessageEventArgs args)
    {
        try
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            string emailAddress = JsonConvert.DeserializeObject<string>(body)!;

            await _emailService.RegisterUserEmailAndLog(emailAddress);
            await args.CompleteMessageAsync(args.Message);

        }
        catch (System.Exception)
        {
            throw;
        }
    }

    private Task OnOrderCreatedError(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }

    private async Task OnOrderCreatedReceived(ProcessMessageEventArgs args)
    {
        try
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            RewardsDto rewardsDto = JsonConvert.DeserializeObject<RewardsDto>(body)!;

            await _emailService.LogOrderPlaced(rewardsDto);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (System.Exception)
        {
            throw;
        }
    }
}
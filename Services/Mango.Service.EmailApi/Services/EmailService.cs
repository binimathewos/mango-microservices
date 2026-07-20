using System.Text;
using Mango.Service.EmailApi.Data;
using Mango.Service.EmailApi.Models;
using Mango.Service.EmailApi.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Mango.Service.EmailApi.Services;

public class EmailService : IEmailService
{
    private DbContextOptions<AppDbContext> _dbOptions;
    public EmailService(DbContextOptions<AppDbContext> dbOptions)
    {
        _dbOptions = dbOptions;
    }

    public async Task EmailCartAndLog(CartDto cartDto)
    {
        StringBuilder message = new StringBuilder();

        message.AppendLine("<br/>Cart Email Requested");
        message.AppendLine("<br/>Total: " + cartDto?.CartHeader?.CartTotal);
        message.Append("<ul>");
        foreach (var item in cartDto.CartDetails)
        {
            message.Append("<li>");
            message.Append(item?.Product?.Name + " x " + item?.Count);
            message.Append("</li>");
        }
        message.Append("</ul>");

        await LogAndEmail(message.ToString(), cartDto.CartHeader.UserId);
    }

    public async Task LogOrderPlaced(RewardsDto rewardsDto)
    {
        string message = "New Order Placed Successfully. <br /> Order ID: " + rewardsDto.OrderId;
        await LogAndEmail(message, "admin@xyz.com");
    }

    public async Task RegisterUserEmailAndLog(string emailAddress)
    {
        string message = "User Registered Successfully. <br /> Email: " + emailAddress;
        await LogAndEmail(message, "admin@xyz.com");
    }

    private async Task<bool> LogAndEmail(string message, string email)
    {
        try
        {
            EmailLogger emailLog = new EmailLogger
            {
                Email = email,
                EmailSent = DateTime.UtcNow,
                Message = message
            };

            await using var _dbContext = new AppDbContext(_dbOptions);
            await _dbContext.EmailLoggers.AddAsync(emailLog);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex?.Message);
            return false;
        }
    }
}
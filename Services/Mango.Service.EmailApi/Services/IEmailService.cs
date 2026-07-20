using Mango.Service.EmailApi.Models.Dto;

namespace Mango.Service.EmailApi.Services;

public interface IEmailService
{
    Task EmailCartAndLog(CartDto cartDto);
    Task RegisterUserEmailAndLog(string emailAddress);
    Task LogOrderPlaced(RewardsDto rewardsDto);
}
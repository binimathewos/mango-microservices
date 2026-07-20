using System.Text;
using Mango.Service.RewardApi.Data;
using Mango.Service.RewardApi.Models;
using Mango.Service.RewardApi.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Mango.Service.RewardApi.Services;

public class RewardsService : IRewardsService
{
    private DbContextOptions<AppDbContext> _dbOptions;
    public RewardsService(DbContextOptions<AppDbContext> dbOptions)
    {
        _dbOptions = dbOptions;
    }

    public async Task<bool> UpdateRewards(RewardsDto rewardsDto)
    {
        try
        {
            Rewards rewards = new Rewards
            {
                OrderId = rewardsDto.OrderId,
                RewardsActivity = rewardsDto.RewardsActivity,
                UserId = rewardsDto.UserId,
                RewardsDate = DateTime.UtcNow
            };

            await using var _dbContext = new AppDbContext(_dbOptions);
            await _dbContext.Rewards.AddAsync(rewards);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

}
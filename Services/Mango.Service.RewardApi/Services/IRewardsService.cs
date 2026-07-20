using Mango.Service.RewardApi.Models.Dto;

namespace Mango.Service.RewardApi.Services;

public interface IRewardsService
{
    Task<bool> UpdateRewards(RewardsDto rewardsDto);
}
using System.ComponentModel.DataAnnotations;

namespace Mango.Service.RewardApi.Models;

public class Rewards
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string UserId { get; set; } = string.Empty;
    [Required]
    public DateTime RewardsDate { get; set; }
    [Required]
    public int RewardsActivity { get; set; }
    [Required]
    public int OrderId { get; set; }
}
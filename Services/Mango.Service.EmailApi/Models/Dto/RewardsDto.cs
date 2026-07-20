namespace Mango.Service.EmailApi.Models.Dto;

public class RewardsDto
{
    public string UserId { get; set; } = string.Empty;
    public int RewardsActivity { get; set; }
    public int OrderId { get; set; }
}
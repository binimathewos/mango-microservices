using System.ComponentModel.DataAnnotations;

namespace Mango.Service.CouponApi.Models;

public class Coupon
{
    [Key]
    public int CouponId { get; set; }
    [Required]
    public string CouponCode { get; set; } = string.Empty;
    [Required]
    public double DiscountAmount { get; set; }
    public double MinimumAmount { get; set; }
}

namespace Mango.Service.CouponApi.Models.Dto
{
    public class CouponDto
    {
        public int CouponId { get; set; }
        public string CouponCode { get; set; } = string.Empty;
        public double DiscountAmount { get; set; }
        public double MinimumAmount { get; set; }
    }
}
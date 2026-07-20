namespace Mango.Web
{
    public class SD
    {

        public static string ProductAPIBase { get; set; } = string.Empty;
        public static string CouponAPIBase { get; set; } = string.Empty;
        public static string AuthAPIBase { get; set; } = string.Empty;
        public static string ShoppingCartAPIBase { get; set; } = string.Empty;
        public static string OrderAPIBase { get; set; } = string.Empty;

        public static string RoleAdmin = "ADMIN";
        public static string RoleCustomer = "CUSTOMER";

        public static string TokenCookie = "JWTTOKEN";

        public static string Appetizer = "Appetizer";
        public static string Dessert = "Dessert";
        public static string Entree = "Entree";


        public const string Status_Pending = "Pending";
        public const string Status_Approved = "Approved";
        public const string Status_ReadyForPickup = "ReadyForPickup";
        public const string Status_Completed = "Completed";
        public const string Status_Refunded = "Refunded";
        public const string Status_Cancelled = "Cancelled";

        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        }

        public enum ContentType
        {
            Json,
            MultipartFormData
        }
    }
}
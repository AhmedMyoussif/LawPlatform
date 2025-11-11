namespace LawPlatform.Entities.DTO.Payment
{
    public class TamaraCheckoutRequest
    {
        public MoneyAmount total_amount { get; set; }
        public MoneyAmount shipping_amount { get; set; }
        public MoneyAmount tax_amount { get; set; }
        public string order_reference_id { get; set; }
        public string order_number { get; set; }
        public Discount discount { get; set; }
        public List<OrderItem> items { get; set; }
        public Consumer consumer { get; set; }
        public string country_code { get; set; }
        public string description { get; set; }
        public MerchantUrl merchant_url { get; set; }
        public string payment_type { get; set; }
        public int? instalments { get; set; }
        public Address billing_address { get; set; }
        public Address shipping_address { get; set; }
        public string platform { get; set; }
        public bool is_mobile { get; set; }
        public string locale { get; set; }
    }

    public class MoneyAmount
    {
        public decimal amount { get; set; }
        public string currency { get; set; }
    }

    public class Discount
    {
        public MoneyAmount amount { get; set; }
        public string name { get; set; }
    }

    public class Consumer
    {
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string phone_number { get; set; }
    }

    public class MerchantUrl
    {
        public string cancel { get; set; }
        public string failure { get; set; }
        public string success { get; set; }
        public string notification { get; set; }
    }

    public class Address
    {
        public string city { get; set; }
        public string country_code { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string line1 { get; set; }
        public string line2 { get; set; }
        public string phone_number { get; set; }
        public string region { get; set; }
    }

    public class OrderItem
    {
        public string name { get; set; }
        public string type { get; set; }
        public string reference_id { get; set; }
        public string sku { get; set; }
        public int quantity { get; set; }
        public MoneyAmount discount_amount { get; set; }
        public MoneyAmount tax_amount { get; set; }
        public MoneyAmount unit_price { get; set; }
        public MoneyAmount total_amount { get; set; }
    }

    // Keep old classes for backwards compatibility
    public class CustomerInfo
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string phone_number { get; set; }
        public string email { get; set; }
    }

    public class ShippingAddress
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string line1 { get; set; }
        public string city { get; set; }
        public string country_code { get; set; }
        public string phone_number { get; set; }
    }
}

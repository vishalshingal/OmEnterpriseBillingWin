namespace OmEnterpriseBillingWin.Models
{
    public class SaleOrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public SaleOrder? Order { get; set; }
        public int ItemId { get; set; }
        public Item? Item { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}

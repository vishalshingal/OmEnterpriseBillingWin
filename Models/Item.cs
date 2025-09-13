namespace OmEnterpriseBillingWin.Models
{
    public class Item
    {
        public int ItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal SalePrice { get; set; }
        public int StockQty { get; set; }
        public int MinStockLevel { get; set; }

    }
}

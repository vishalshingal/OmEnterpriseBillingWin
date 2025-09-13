using System;

namespace OmEnterpriseBillingWin.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public Item? Item { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
        public string Seller { get; set; } = string.Empty;
        public int StakeholderId { get; set; }
        public Stakeholder? Stakeholder { get; set; }
    }
}

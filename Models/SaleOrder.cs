using System;
using System.Collections.Generic;

namespace OmEnterpriseBillingWin.Models
{
    public class SaleOrder
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int StakeholderId { get; set; }
        public Stakeholder? Stakeholder { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string Status { get; set; } = "Completed";
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<SaleOrderItem> OrderItems { get; set; } = new List<SaleOrderItem>();
    }
}

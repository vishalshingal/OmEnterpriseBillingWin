namespace OmEnterpriseBillingWin.Models
{
    public enum StakeholderType
    {
        Buyer,
        Seller
    }

    public class Stakeholder
    {
        public int StakeholderId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public StakeholderType Type { get; set; }
        public decimal DiscountPercentage { get; set; }
    }
}

using OmEnterpriseBillingWin.Models;
using OmEnterpriseBillingWin.Services;

namespace OmEnterpriseBillingWin
{
    public partial class Dashboard : Form
    {
        private readonly PurchaseService _purchaseService;
        private readonly SaleService _saleService;
        private readonly ItemService _itemService;

        public Dashboard()
        {
            InitializeComponent();
            _purchaseService = new PurchaseService();
            _saleService = new SaleService();
            _itemService = new ItemService();
        }

        private async void Dashboard_Load(object sender, EventArgs e)
        {
            await RefreshDashboard();
        }

    private async Task RefreshDashboard()
    {
        try
        {
            // Load summary data
            var thisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var nextMonth = thisMonth.AddMonths(1);

            var monthlyPurchases = await _purchaseService.GetTotalPurchaseAmount(thisMonth, nextMonth);
            var monthlySales = await _saleService.GetTotalSaleAmount(thisMonth, nextMonth);
            var items = await _itemService.GetAllItemsAsync();
            var totalStock = items.Sum(i => i.StockQty);
            var balance = monthlySales - monthlyPurchases;

            // Update summary cards
            lblPurchases.Text = $"₹ {monthlyPurchases:N2}";
            lblSales.Text = $"₹ {monthlySales:N2}";
            lblStock.Text = totalStock.ToString();
            lblBalance.Text = $"₹ {balance:N2}";

            // Load recent transactions
            var purchases = await _purchaseService.GetAllPurchasesAsync();
            var sales = await _saleService.GetAllSalesAsync();

            var recentTransactions = purchases.Select(p => new
            {
                Type = "Purchase",
                Date = p.Date,
                Item = p.Item?.Name,
                Quantity = p.Quantity,
                Amount = p.Price * p.Quantity,
                Person = p.Seller
            })
            .Concat(sales.Select(s => new
            {
                Type = "Sale",
                Date = s.Date,
                Item = s.Item?.Name,
                Quantity = s.Quantity,
                Amount = s.Price * s.Quantity,
                Person = s.Buyer
            }))
            .OrderByDescending(t => t.Date)
            .Take(10)
            .ToList();

            dgvRecent.DataSource = recentTransactions;
            dgvRecent.Columns["Type"].Width = 100;
            dgvRecent.Columns["Date"].Width = 150;
            dgvRecent.Columns["Amount"].DefaultCellStyle.Format = "N2";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error refreshing dashboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

        private void btnBuy_Click(object sender, EventArgs e)
        {
            var purchaseForm = new PurchaseForm();
            purchaseForm.FormClosed += async (s, args) => await RefreshDashboard();
            purchaseForm.ShowDialog();
        }

        private void btnSell_Click(object sender, EventArgs e)
        {
            var saleForm = new SaleForm();
            saleForm.FormClosed += async (s, args) => await RefreshDashboard();
            saleForm.ShowDialog();
        }

        private void btnManageItems_Click(object sender, EventArgs e)
        {
            var itemsForm = new ItemsForm();
            itemsForm.FormClosed += async (s, args) => await RefreshDashboard();
            itemsForm.ShowDialog();
        }

        private void btnViewStock_Click(object sender, EventArgs e)
        {
            var stockForm = new StockForm();
            stockForm.FormClosed += async (s, args) => await RefreshDashboard();
            stockForm.ShowDialog();
        }

        private void btnManageStakeholders_Click(object sender, EventArgs e)
        {
            var stakeholdersForm = new StakeholdersForm();
            stakeholdersForm.ShowDialog();
        }
    }
}

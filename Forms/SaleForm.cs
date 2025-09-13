using System;
using System.Windows.Forms;
using OmEnterpriseBillingWin.Models;
using OmEnterpriseBillingWin.Services;

namespace OmEnterpriseBillingWin
{
    public partial class SaleForm : Form
    {
        private readonly SaleService _saleService;
        private readonly ItemService _itemService;
        private readonly StakeholderService _stakeholderService;

        public SaleForm()
        {
            InitializeComponent();
            _saleService = new SaleService();
            _itemService = new ItemService();
            _stakeholderService = new StakeholderService();
            LoadItems();
            LoadBuyers();
        }

        private async void LoadItems()
        {
            try
            {
                var items = await _itemService.GetAllItemsAsync();
                cboItems.DataSource = items;
                cboItems.DisplayMember = "Name";
                cboItems.ValueMember = "ItemId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading items: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadBuyers()
        {
            try
            {
                var buyers = await _stakeholderService.GetStakeholdersByTypeAsync(StakeholderType.Buyer);
                cboBuyers.DataSource = buyers;
                cboBuyers.DisplayMember = "Name";
                cboBuyers.ValueMember = "StakeholderId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading buyers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void cboItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboItems.SelectedItem is Item selectedItem)
            {
                txtPrice.Text = selectedItem.SalePrice.ToString("N2");
                lblStock.Text = $"Available Stock: {selectedItem.StockQty}";
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (cboItems.SelectedItem == null)
            {
                MessageBox.Show("Please select an item", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid price", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var item = (Item)cboItems.SelectedItem;
            if (quantity > item.StockQty)
            {
                MessageBox.Show("Insufficient stock available", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var sale = new Sale
            {
                ItemId = item.ItemId,
                Quantity = quantity,
                Price = price,
                Date = dateTimePicker.Value,
                StakeholderId = (int)cboBuyers.SelectedValue
            };

            try
            {
                await _saleService.AddSaleAsync(sale);
                MessageBox.Show("Sale recorded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error recording sale: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

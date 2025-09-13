using System;
using System.Windows.Forms;
using OmEnterpriseBillingWin.Models;
using OmEnterpriseBillingWin.Services;

namespace OmEnterpriseBillingWin
{
    public partial class PurchaseForm : Form
    {
        private readonly PurchaseService _purchaseService;
        private readonly ItemService _itemService;
        private readonly StakeholderService _stakeholderService;

        public PurchaseForm()
        {
            InitializeComponent();
            _purchaseService = new PurchaseService();
            _itemService = new ItemService();
            _stakeholderService = new StakeholderService();
            LoadItems();
            LoadSellers();
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

        private async void LoadSellers()
        {
            try
            {
                var sellers = await _stakeholderService.GetStakeholdersByTypeAsync(StakeholderType.Seller);
                cboSellers.DataSource = sellers;
                cboSellers.DisplayMember = "Name";
                cboSellers.ValueMember = "StakeholderId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sellers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            var purchase = new Purchase
            {
                ItemId = item.ItemId,
                Quantity = quantity,
                Price = price,
                Date = dateTimePicker.Value,
                StakeholderId = (int)cboSellers.SelectedValue
            };

            try
            {
                await _purchaseService.AddPurchaseAsync(purchase);
                MessageBox.Show("Purchase recorded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error recording purchase: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

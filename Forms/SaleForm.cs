using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
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
        private List<SaleLineItem> _saleItems;
        private List<Item> _availableItems;

        public SaleForm()
        {
            InitializeComponent();
            _saleService = new SaleService();
            _itemService = new ItemService();
            _stakeholderService = new StakeholderService();
            _saleItems = new List<SaleLineItem>();
            LoadData();
            SetupGrid();
        }

        private async void LoadData()
        {
            try
            {
                // Load buyers
                var buyers = await _stakeholderService.GetStakeholdersByTypeAsync(StakeholderType.Buyer);
                cboBuyers.DataSource = buyers;
                cboBuyers.DisplayMember = "Name";
                cboBuyers.ValueMember = "StakeholderId";

                // Load items
                _availableItems = (await _itemService.GetAllItemsAsync()).ToList();
                cboItems.DataSource = _availableItems;
                cboItems.DisplayMember = "Name";
                cboItems.ValueMember = "ItemId";

                RefreshGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupGrid()
        {
            dgvSaleItems.AutoGenerateColumns = false;
            dgvSaleItems.Columns.Clear();

            // Item Name Column
            var itemNameColumn = new DataGridViewTextBoxColumn
            {
                Name = "ItemName",
                HeaderText = "Product Name",
                DataPropertyName = "ItemName",
                Width = 200,
                ReadOnly = true
            };

            // Quantity Column
            var quantityColumn = new DataGridViewTextBoxColumn
            {
                Name = "Quantity",
                HeaderText = "Quantity",
                DataPropertyName = "Quantity",
                Width = 80
            };

            // Unit Price Column
            var unitPriceColumn = new DataGridViewTextBoxColumn
            {
                Name = "UnitPrice",
                HeaderText = "Unit Price",
                DataPropertyName = "UnitPrice",
                Width = 100,
                DefaultCellStyle = { Format = "C2" }
            };

            // Total Column
            var totalColumn = new DataGridViewTextBoxColumn
            {
                Name = "Total",
                HeaderText = "Total",
                DataPropertyName = "Total",
                Width = 100,
                ReadOnly = true,
                DefaultCellStyle = { Format = "C2" }
            };

            // Remove Button Column
            var removeColumn = new DataGridViewButtonColumn
            {
                Name = "Remove",
                HeaderText = "Action",
                Text = "Remove",
                UseColumnTextForButtonValue = true,
                Width = 80
            };

            dgvSaleItems.Columns.AddRange(itemNameColumn, quantityColumn, unitPriceColumn, totalColumn, removeColumn);

            // Event handlers
            dgvSaleItems.CellValueChanged += DgvSaleItems_CellValueChanged;
            dgvSaleItems.CellClick += DgvSaleItems_CellClick;
        }

        private void RefreshGrid()
        {
            dgvSaleItems.DataSource = null;
            dgvSaleItems.DataSource = new BindingList<SaleLineItem>(_saleItems);
            CalculateTotals();
        }

        private void CalculateTotals()
        {
            decimal subtotal = _saleItems.Sum(item => item.Total);
            
            // Get buyer's discount percentage if selected
            decimal discountPercentage = 0;
            if (cboBuyers.SelectedItem is Stakeholder buyer)
            {
                discountPercentage = buyer.DiscountPercentage;
            }
            
            decimal discount = subtotal * (discountPercentage / 100m);
            decimal total = subtotal - discount;

            lblSubtotal.Text = subtotal.ToString("C2");
            lblDiscount.Text = discount.ToString("C2");
            lblTotal.Text = total.ToString("C2");
        }

        private void cboItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboItems.SelectedItem is Item selectedItem)
            {
                txtPrice.Text = selectedItem.SalePrice.ToString("F2");
                lblStock.Text = $"Available Stock: {selectedItem.StockQty}";
            }
        }

        private void btnAddItem_Click(object sender, EventArgs e)
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

            var selectedItem = (Item)cboItems.SelectedItem;

            // Check if item already exists in the list
            var existingItem = _saleItems.FirstOrDefault(si => si.ItemId == selectedItem.ItemId);
            if (existingItem != null)
            {
                // Update quantity
                int newQuantity = existingItem.Quantity + quantity;
                if (newQuantity > selectedItem.StockQty)
                {
                    MessageBox.Show($"Total quantity ({newQuantity}) exceeds available stock ({selectedItem.StockQty})",
                        "Stock Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                existingItem.Quantity = newQuantity;
                existingItem.Total = existingItem.Quantity * existingItem.UnitPrice;
            }
            else
            {
                // Check stock availability
                if (quantity > selectedItem.StockQty)
                {
                    MessageBox.Show($"Insufficient stock. Available: {selectedItem.StockQty}",
                        "Stock Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Add new item
                var saleLineItem = new SaleLineItem
                {
                    ItemId = selectedItem.ItemId,
                    ItemName = selectedItem.Name,
                    Quantity = quantity,
                    UnitPrice = price,
                    Total = quantity * price
                };
                _saleItems.Add(saleLineItem);
            }

            // Clear input fields
            txtQuantity.Clear();
            txtPrice.Clear();
            cboItems.SelectedIndex = -1;
            lblStock.Text = "Available Stock: 0";

            RefreshGrid();
        }

        private void DgvSaleItems_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && (e.ColumnIndex == dgvSaleItems.Columns["Quantity"].Index ||
                                   e.ColumnIndex == dgvSaleItems.Columns["UnitPrice"].Index))
            {
                var item = _saleItems[e.RowIndex];
                var selectedItem = _availableItems.FirstOrDefault(i => i.ItemId == item.ItemId);

                if (selectedItem != null && item.Quantity > selectedItem.StockQty)
                {
                    MessageBox.Show($"Quantity exceeds available stock ({selectedItem.StockQty})",
                        "Stock Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    item.Quantity = selectedItem.StockQty;
                }

                item.Total = item.Quantity * item.UnitPrice;
                RefreshGrid();
            }
        }

        private void DgvSaleItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvSaleItems.Columns["Remove"].Index)
            {
                _saleItems.RemoveAt(e.RowIndex);
                RefreshGrid();
            }
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear all items?", "Confirm Clear",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _saleItems.Clear();
                RefreshGrid();
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (cboBuyers.SelectedValue == null)
            {
                MessageBox.Show("Please select a buyer", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_saleItems.Any())
            {
                MessageBox.Show("Please add at least one item to the sale", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Create sales for each line item
                foreach (var lineItem in _saleItems)
                {
                    var sale = new Sale
                    {
                        ItemId = lineItem.ItemId,
                        Quantity = lineItem.Quantity,
                        Price = lineItem.UnitPrice,
                        Date = dateTimePicker.Value,
                        StakeholderId = (int)cboBuyers.SelectedValue
                    };

                    await _saleService.AddSaleAsync(sale);
                }

                MessageBox.Show($"Sale recorded successfully!\nTotal Amount: {_saleItems.Sum(i => i.Total):C2}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

    // Helper class for sale line items
    public class SaleLineItem
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}

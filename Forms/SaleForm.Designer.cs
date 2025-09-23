using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private bool _isReadOnlyMode;
        private SaleOrder? _viewingOrder;

        public SaleForm(SaleOrder? orderToView = null)
        {
            InitializeComponent();
            _saleService = new SaleService();
            _itemService = new ItemService();
            _stakeholderService = new StakeholderService();
            _saleItems = new List<SaleLineItem>();
            _viewingOrder = orderToView;
            _isReadOnlyMode = orderToView != null;

            LoadData();
            SetupGrid();

            if (_isReadOnlyMode)
            {
                SetupReadOnlyMode();
            }
        }

        private void SetupReadOnlyMode()
        {
            if (_viewingOrder == null) return;

            // Set form title
            this.Text = $"View Order - {_viewingOrder.OrderNumber}";

            // Make all input controls readonly
            cboBuyers.Enabled = false;
            dateTimePicker.Enabled = false;
            cboItems.Enabled = false;
            txtQuantity.ReadOnly = true;
            txtPrice.ReadOnly = true;
            btnAddItem.Enabled = false;
            btnClearAll.Enabled = false;
            btnSave.Visible = false;
            dgvSaleItems.ReadOnly = true;

            // Load order data
            LoadOrderData();

            // Change cancel button text
            btnCancel.Text = "Close";
        }

        private void LoadOrderData()
        {
            if (_viewingOrder == null) return;

            // Set customer
            cboBuyers.SelectedValue = _viewingOrder.StakeholderId;
            dateTimePicker.Value = _viewingOrder.OrderDate;

            // Load order items
            _saleItems.Clear();
            foreach (var orderItem in _viewingOrder.OrderItems)
            {
                var saleLineItem = new SaleLineItem
                {
                    ItemId = orderItem.ItemId,
                    ItemName = orderItem.Item?.Name ?? "Unknown Item",
                    Quantity = orderItem.Quantity,
                    UnitPrice = orderItem.UnitPrice,
                    Total = orderItem.Total
                };
                _saleItems.Add(saleLineItem);
            }

            RefreshGrid();
        }

        // ... (rest of the existing SaleForm methods remain the same)

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (_isReadOnlyMode) return;

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
                // Create sale order
                var saleOrder = new SaleOrder
                {
                    StakeholderId = (int)cboBuyers.SelectedValue,
                    OrderDate = dateTimePicker.Value,
                    Subtotal = _saleItems.Sum(i => i.Total),
                    TaxAmount = _saleItems.Sum(i => i.Total) * 0.1m,
                    GrandTotal = _saleItems.Sum(i => i.Total) * 1.1m,
                    Status = "Completed",
                    Notes = string.Empty
                };

                // Add order items
                foreach (var lineItem in _saleItems)
                {
                    saleOrder.OrderItems.Add(new SaleOrderItem
                    {
                        ItemId = lineItem.ItemId,
                        Quantity = lineItem.Quantity,
                        UnitPrice = lineItem.UnitPrice,
                        Total = lineItem.Total
                    });
                }

                var orderId = await _saleService.CreateSaleOrderAsync(saleOrder);

                MessageBox.Show($"Order saved successfully!\nOrder Number: {saleOrder.OrderNumber}\nTotal Amount: {saleOrder.GrandTotal:C2}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving order: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

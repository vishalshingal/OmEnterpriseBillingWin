using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OmEnterpriseBillingWin.Models;
using OmEnterpriseBillingWin.Services;

namespace OmEnterpriseBillingWin
{
    public class SaleForm : Form
    {
        // Services
        private readonly SaleService _saleService;
        private readonly ItemService _itemService;
        private readonly StakeholderService _stakeholderService;

        // Controls
        private GroupBox groupBoxCustomer;
        private Label label5;
        private ComboBox cboBuyers;
        private Label label4;
        private DateTimePicker dateTimePicker;

        private GroupBox groupBoxProduct;
        private Button btnAddItem;
        private Label lblStock;
        private Label label3;
        private TextBox txtPrice;
        private Label label2;
        private TextBox txtQuantity;
        private Label label1;
        private ComboBox cboItems;

        private GroupBox groupBoxItems;
        private Button btnClearAll;
        private DataGridView dgvSaleItems;

        private GroupBox groupBoxTotals;
        private Label lblGrandTotal;
        private Label lblTax;
        private Label lblSubtotal;

        private Button btnSave;
        private Button btnCancel;

        // Data
        private List<SaleLineItem> _saleItems;
        private List<Item> _availableItems;

        public SaleForm()
        {
            _saleService = new SaleService();
            _itemService = new ItemService();
            _stakeholderService = new StakeholderService();
            _saleItems = new List<SaleLineItem>();

            InitializeForm();
            SetupEvents();
            LoadData();
        }

        private void InitializeForm()
        {
            // Form properties
            this.Text = "Multi-Product Sale";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            CreateControls();
            LayoutControls();
        }

        private void CreateControls()
        {
            // Customer Group
            groupBoxCustomer = new GroupBox { Text = "Customer Information", Size = new Size(760, 80), Location = new Point(12, 12) };
            label5 = new Label { Text = "Customer:", Location = new Point(15, 25), Size = new Size(59, 15) };
            cboBuyers = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(80, 22), Size = new Size(300, 23) };
            label4 = new Label { Text = "Date:", Location = new Point(400, 25), Size = new Size(34, 15) };
            dateTimePicker = new DateTimePicker { Location = new Point(440, 22), Size = new Size(200, 23), Value = DateTime.Now };

            // Product Group
            groupBoxProduct = new GroupBox { Text = "Add Products", Size = new Size(760, 100), Location = new Point(12, 98) };
            label1 = new Label { Text = "Product:", Location = new Point(15, 25), Size = new Size(49, 15) };
            cboItems = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(80, 22), Size = new Size(200, 23) };
            lblStock = new Label { Text = "Available Stock: 0", Location = new Point(80, 48), Size = new Size(200, 15), ForeColor = Color.Blue };
            label2 = new Label { Text = "Quantity:", Location = new Point(300, 25), Size = new Size(56, 15) };
            txtQuantity = new TextBox { Location = new Point(360, 22), Size = new Size(80, 23) };
            label3 = new Label { Text = "Price:", Location = new Point(450, 25), Size = new Size(36, 15) };
            txtPrice = new TextBox { Location = new Point(490, 22), Size = new Size(100, 23) };
            btnAddItem = new Button { Text = "Add Item", Location = new Point(650, 35), Size = new Size(90, 35), BackColor = Color.LightGreen };

            // Items Group
            groupBoxItems = new GroupBox { Text = "Sale Items", Size = new Size(760, 250), Location = new Point(12, 204) };
            btnClearAll = new Button { Text = "Clear All", Location = new Point(650, 20), Size = new Size(90, 30), BackColor = Color.LightCoral };
            dgvSaleItems = new DataGridView
            {
                Location = new Point(15, 55),
                Size = new Size(725, 180),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // Totals Group
            groupBoxTotals = new GroupBox { Text = "Totals", Size = new Size(400, 80), Location = new Point(12, 460) };
            lblSubtotal = new Label { Text = "Subtotal: $0.00", Location = new Point(15, 21), Size = new Size(150, 15) };
            lblTax = new Label { Text = "Tax (10%): $0.00", Location = new Point(15, 38), Size = new Size(150, 15) };
            lblGrandTotal = new Label
            {
                Text = "Grand Total: $0.00",
                Location = new Point(15, 55),
                Size = new Size(150, 15),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold)
            };

            // Action Buttons
            btnSave = new Button
            {
                Text = "Save Sale",
                Location = new Point(580, 480),
                Size = new Size(90, 35),
                BackColor = Color.LightBlue,
                Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold)
            };
            btnCancel = new Button { Text = "Cancel", Location = new Point(682, 480), Size = new Size(90, 35) };
        }

        private void LayoutControls()
        {
            // Add controls to groups
            groupBoxCustomer.Controls.AddRange(new Control[] { label5, cboBuyers, label4, dateTimePicker });
            groupBoxProduct.Controls.AddRange(new Control[] { label1, cboItems, lblStock, label2, txtQuantity, label3, txtPrice, btnAddItem });
            groupBoxItems.Controls.AddRange(new Control[] { btnClearAll, dgvSaleItems });
            groupBoxTotals.Controls.AddRange(new Control[] { lblSubtotal, lblTax, lblGrandTotal });

            // Add groups to form
            this.Controls.AddRange(new Control[]
            {
                groupBoxCustomer,
                groupBoxProduct,
                groupBoxItems,
                groupBoxTotals,
                btnSave,
                btnCancel
            });
        }

        private void SetupEvents()
        {
            cboItems.SelectedIndexChanged += CboItems_SelectedIndexChanged;
            btnAddItem.Click += BtnAddItem_Click;
            btnClearAll.Click += BtnClearAll_Click;
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;
        }

        private async void LoadData()
        {
            try
            {
                // Load buyers
                var buyers = await _stakeholderService.GetStakeholdersByTypeAsync(StakeholderType.Buyer);
                cboBuyers.DataSource = buyers.ToList();
                cboBuyers.DisplayMember = "Name";
                cboBuyers.ValueMember = "StakeholderId";

                // Load items
                _availableItems = (await _itemService.GetAllItemsAsync()).ToList();
                cboItems.DataSource = _availableItems;
                cboItems.DisplayMember = "Name";
                cboItems.ValueMember = "ItemId";

                SetupGrid();
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
            dgvSaleItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ItemName",
                HeaderText = "Product Name",
                DataPropertyName = "ItemName",
                Width = 200,
                ReadOnly = true
            });

            // Quantity Column
            dgvSaleItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Quantity",
                HeaderText = "Quantity",
                DataPropertyName = "Quantity",
                Width = 80
            });

            // Unit Price Column
            dgvSaleItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "UnitPrice",
                HeaderText = "Unit Price",
                DataPropertyName = "UnitPrice",
                Width = 100,
                DefaultCellStyle = { Format = "C2" }
            });

            // Total Column
            dgvSaleItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Total",
                HeaderText = "Total",
                DataPropertyName = "Total",
                Width = 100,
                ReadOnly = true,
                DefaultCellStyle = { Format = "C2" }
            });

            // Remove Button Column
            dgvSaleItems.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Remove",
                HeaderText = "Action",
                Text = "Remove",
                UseColumnTextForButtonValue = true,
                Width = 80
            });

            dgvSaleItems.CellClick += DgvSaleItems_CellClick;
            dgvSaleItems.CellValueChanged += DgvSaleItems_CellValueChanged;
        }

        private void RefreshGrid()
        {
            dgvSaleItems.DataSource = null;
            dgvSaleItems.DataSource = _saleItems.ToList();
            CalculateTotals();
        }

        private void CalculateTotals()
        {
            decimal subtotal = _saleItems.Sum(item => item.Total);
            decimal tax = subtotal * 0.1m; // 10% tax rate
            decimal grandTotal = subtotal + tax;

            lblSubtotal.Text = $"Subtotal: {subtotal:C2}";
            lblTax.Text = $"Tax (10%): {tax:C2}";
            lblGrandTotal.Text = $"Grand Total: {grandTotal:C2}";
        }

        private void CboItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboItems.SelectedItem is Item selectedItem)
            {
                txtPrice.Text = selectedItem.SalePrice.ToString("F2");
                lblStock.Text = $"Available Stock: {selectedItem.StockQty}";
            }
        }

        private void BtnAddItem_Click(object sender, EventArgs e)
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

        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear all items?", "Confirm Clear",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _saleItems.Clear();
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

        private async void BtnSave_Click(object sender, EventArgs e)
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

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    // Helper class for sale line items
    public class SaleLineItem
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}

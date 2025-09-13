using System;
using System.Windows.Forms;
using OmEnterpriseBillingWin.Models;
using OmEnterpriseBillingWin.Services;

namespace OmEnterpriseBillingWin
{
    public partial class ItemsForm : Form
    {
        private readonly ItemService _itemService;
        private Item _selectedItem;

        public ItemsForm()
        {
            InitializeComponent();
            _itemService = new ItemService();
            LoadItems();
            SetEditMode(false);
        }

        private async void LoadItems()
        {
            try
            {
                var items = await _itemService.GetAllItemsAsync();
                dgvItems.DataSource = items;
                FormatGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading items: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatGrid()
        {
            dgvItems.Columns["ItemId"].Visible = false;
            dgvItems.Columns["Name"].HeaderText = "Item Name";
            dgvItems.Columns["SalePrice"].HeaderText = "Sale Price";
            dgvItems.Columns["StockQty"].HeaderText = "Current Stock";
            dgvItems.Columns["MinStockLevel"].HeaderText = "Min Stock Level";
            
            dgvItems.Columns["SalePrice"].DefaultCellStyle.Format = "N2";
            
            foreach (DataGridViewColumn col in dgvItems.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private void SetEditMode(bool editing)
        {
            pnlEdit.Enabled = editing;
            dgvItems.Enabled = !editing;
            btnNew.Enabled = !editing;
            btnEdit.Enabled = !editing && dgvItems.SelectedRows.Count > 0;
            btnDelete.Enabled = !editing && dgvItems.SelectedRows.Count > 0;
            btnSave.Enabled = editing;
            btnCancel.Enabled = editing;
        }

        private void ClearFields()
        {
            txtName.Text = "";
            txtDefaultPrice.Text = "0.00";
            txtReorderLevel.Text = "0";
            _selectedItem = null;
        }

        private void dgvItems_SelectionChanged(object sender, EventArgs e)
        {
            btnEdit.Enabled = dgvItems.SelectedRows.Count > 0;
            btnDelete.Enabled = dgvItems.SelectedRows.Count > 0;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearFields();
            SetEditMode(true);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvItems.SelectedRows.Count > 0)
            {
                _selectedItem = (Item)dgvItems.SelectedRows[0].DataBoundItem;
                txtName.Text = _selectedItem.Name;
                txtDefaultPrice.Text = _selectedItem.SalePrice.ToString("N2");
                txtReorderLevel.Text = _selectedItem.MinStockLevel.ToString();
                SetEditMode(true);
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvItems.SelectedRows.Count > 0)
            {
                var item = (Item)dgvItems.SelectedRows[0].DataBoundItem;
                if (MessageBox.Show($"Are you sure you want to delete {item.Name}?", "Confirm Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        await _itemService.DeleteItemAsync(item.ItemId);
                        LoadItems();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter item name", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtDefaultPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Please enter a valid sale price", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtReorderLevel.Text, out int minStock) || minStock < 0)
            {
                MessageBox.Show("Please enter a valid minimum stock level", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (_selectedItem == null)
                {
                    var item = new Item
                    {
                        Name = txtName.Text.Trim(),
                        SalePrice = price,
                        StockQty = 0,
                        MinStockLevel = minStock
                    };
                    await _itemService.AddItemAsync(item);
                }
                else
                {
                    _selectedItem.Name = txtName.Text.Trim();
                    _selectedItem.SalePrice = price;
                    _selectedItem.MinStockLevel = minStock;
                    await _itemService.UpdateItemAsync(_selectedItem);
                }

                LoadItems();
                SetEditMode(false);
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            SetEditMode(false);
            ClearFields();
        }
    }
}

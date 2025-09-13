using System;
using System.Drawing;
using System.Windows.Forms;
using OmEnterpriseBillingWin.Models;
using OmEnterpriseBillingWin.Services;

namespace OmEnterpriseBillingWin
{
    public partial class StockForm : Form
    {
        private readonly ItemService _itemService;

        public StockForm()
        {
            InitializeComponent();
            _itemService = new ItemService();
            LoadStock();
        }

        private async void LoadStock()
        {
            try
            {
                var items = await _itemService.GetAllItemsAsync();
                dgvStock.DataSource = items;
                FormatGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading stock: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatGrid()
        {
            dgvStock.Columns["ItemID"].Visible = false;
            dgvStock.Columns["Name"].HeaderText = "Item Name";
            dgvStock.Columns["SalePrice"].HeaderText = "Sale Price";
            dgvStock.Columns["StockQty"].HeaderText = "Current Stock";
            dgvStock.Columns["MinStockLevel"].HeaderText = "Min Stock Level";
            
            dgvStock.Columns["SalePrice"].DefaultCellStyle.Format = "N2";            // Add color formatting
            dgvStock.CellFormatting += DgvStock_CellFormatting;

            foreach (DataGridViewColumn col in dgvStock.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private void DgvStock_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var item = (Item)dgvStock.Rows[e.RowIndex].DataBoundItem;
                
                // Color entire row based on stock level
                if (item.StockQty <= 0)
                {
                    dgvStock.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightPink;
                    dgvStock.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                }
                else if (item.StockQty <= item.MinStockLevel)
                {
                    dgvStock.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                    dgvStock.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DarkGoldenrod;
                }
                else
                {
                    dgvStock.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                    dgvStock.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DarkGreen;
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadStock();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (dgvStock.DataSource is System.Collections.Generic.List<Item> items)
            {
                dgvStock.DataSource = items.FindAll(i => 
                    i.Name.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}

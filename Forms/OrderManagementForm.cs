using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using OmEnterpriseBillingWin.Models;
using OmEnterpriseBillingWin.Services;

namespace OmEnterpriseBillingWin
{
    public partial class OrderManagementForm : Form
    {
        private readonly StakeholderService _stakeholderService;
        private readonly SaleService _saleService;
        private List<Stakeholder> _stakeholders;
        private List<SaleOrder> _orders;

        public OrderManagementForm()
        {
            InitializeComponent();
            _stakeholderService = new StakeholderService();
            _saleService = new SaleService();
            _stakeholders = new List<Stakeholder>();
            _orders = new List<SaleOrder>();
            LoadStakeholders();
            SetupGrids();
        }

        private async void LoadStakeholders()
        {
            try
            {
                _stakeholders = (await _stakeholderService.GetStakeholdersByTypeAsync(StakeholderType.Buyer)).ToList();

                // Add "All Customers" option
                var allCustomers = new Stakeholder
                {
                    StakeholderId = -1,
                    Name = "-- All Customers --",
                    Type = StakeholderType.Buyer
                };
                _stakeholders.Insert(0, allCustomers);

                cboStakeholders.DataSource = _stakeholders;
                cboStakeholders.DisplayMember = "Name";
                cboStakeholders.ValueMember = "StakeholderId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupGrids()
        {
            // Setup Orders Grid
            dgvOrders.AutoGenerateColumns = false;
            dgvOrders.Columns.Clear();

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OrderNumber",
                HeaderText = "Order #",
                DataPropertyName = "OrderNumber",
                Width = 120
            });

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OrderDate",
                HeaderText = "Date",
                DataPropertyName = "OrderDate",
                Width = 100,
                DefaultCellStyle = { Format = "dd/MM/yyyy" }
            });

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CustomerName",
                HeaderText = "Customer",
                DataPropertyName = "Stakeholder.Name",
                Width = 200
            });

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "GrandTotal",
                HeaderText = "Total Amount",
                DataPropertyName = "GrandTotal",
                Width = 120,
                DefaultCellStyle = { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Status",
                DataPropertyName = "Status",
                Width = 100
            });

            dgvOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOrders.MultiSelect = false;
            dgvOrders.SelectionChanged += DgvOrders_SelectionChanged;
            dgvOrders.DoubleClick += DgvOrders_DoubleClick;
        }

        private async void cboStakeholders_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboStakeholders.SelectedValue == null) return;

            var stakeholderId = (int)cboStakeholders.SelectedValue;

            try
            {
                if (stakeholderId == -1)
                {
                    // Load all orders (you'll need to implement this method)
                    _orders = new List<SaleOrder>(); // For now, empty list
                    lblOrderCount.Text = "Total Orders: 0";
                }
                else
                {
                    _orders = await _saleService.GetSaleOrdersByStakeholderAsync(stakeholderId);
                    lblOrderCount.Text = $"Total Orders: {_orders.Count}";
                }

                dgvOrders.DataSource = null;
                dgvOrders.DataSource = _orders;

                // Update summary
                if (_orders.Any())
                {
                    var totalAmount = _orders.Sum(o => o.GrandTotal);
                    lblTotalAmount.Text = $"Total Amount: {totalAmount:C2}";
                }
                else
                {
                    lblTotalAmount.Text = "Total Amount: $0.00";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvOrders_SelectionChanged(object sender, EventArgs e)
        {
            btnViewOrder.Enabled = dgvOrders.SelectedRows.Count > 0;
        }

        private void DgvOrders_DoubleClick(object sender, EventArgs e)
        {
            ViewSelectedOrder();
        }

        private async void btnViewOrder_Click(object sender, EventArgs e)
        {
            ViewSelectedOrder();
        }

        private async void ViewSelectedOrder()
        {
            if (dgvOrders.SelectedRows.Count == 0) return;

            var selectedOrder = (SaleOrder)dgvOrders.SelectedRows[0].DataBoundItem;

            try
            {
                // Load full order details
                var fullOrder = await _saleService.GetSaleOrderByIdAsync(selectedOrder.OrderId);
                if (fullOrder != null)
                {
                    var viewForm = new SaleForm(fullOrder);
                    viewForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading order details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (cboStakeholders.SelectedValue != null)
            {
                cboStakeholders_SelectedIndexChanged(cboStakeholders, EventArgs.Empty);
            }
        }
    }
}

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

            // Initialize services and data
            _stakeholderService = new StakeholderService();
            _saleService = new SaleService();
            _stakeholders = new List<Stakeholder>();
            _orders = new List<SaleOrder>();

            // Wire up event handlers AFTER InitializeComponent
            this.Load += OrderManagementForm_Load;
            cboStakeholders.SelectedIndexChanged += cboStakeholders_SelectedIndexChanged;
            btnRefresh.Click += btnRefresh_Click;
            btnViewOrder.Click += btnViewOrder_Click;
            btnClose.Click += btnClose_Click;
            dgvOrders.SelectionChanged += DgvOrders_SelectionChanged;
            dgvOrders.DoubleClick += DgvOrders_DoubleClick;
        }

        private async void OrderManagementForm_Load(object sender, EventArgs e)
        {
            await LoadStakeholders();
            SetupGrids();
        }

        private async System.Threading.Tasks.Task LoadStakeholders()
        {
            try
            {
                _stakeholders = (await _stakeholderService.GetStakeholdersByTypeAsync(StakeholderType.Buyer)).ToList();

                // Add "All Customers" option
                var allCustomers = new Stakeholder
                {
                    StakeholderId = -1,
                    Name = "-- Select Customer --",
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
        }

        private async void cboStakeholders_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboStakeholders.SelectedValue == null || (int)cboStakeholders.SelectedValue == -1)
            {
                dgvOrders.DataSource = null;
                lblOrderCount.Text = "Total Orders: 0";
                lblTotalAmount.Text = "Total Amount: $0.00";
                return;
            }

            var stakeholderId = (int)cboStakeholders.SelectedValue;

            try
            {
                _orders = await _saleService.GetSaleOrdersByStakeholderAsync(stakeholderId);
                lblOrderCount.Text = $"Total Orders: {_orders.Count}";

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

        private void btnViewOrder_Click(object sender, EventArgs e)
        {
            ViewSelectedOrder();
        }

        private async void ViewSelectedOrder()
        {
            if (dgvOrders.SelectedRows.Count == 0) return;

            try
            {
                var selectedOrder = (SaleOrder)dgvOrders.SelectedRows[0].DataBoundItem;

                // For now, just show a message - implement full view later
                MessageBox.Show($"Viewing Order: {selectedOrder.OrderNumber}\nTotal: {selectedOrder.GrandTotal:C2}",
                    "Order Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing order: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

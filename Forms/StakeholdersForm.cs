using System;
using System.Windows.Forms;
using OmEnterpriseBillingWin.Models;
using OmEnterpriseBillingWin.Services;

namespace OmEnterpriseBillingWin
{
    public partial class StakeholdersForm : Form
    {
        private readonly StakeholderService _stakeholderService;
        private Stakeholder? _selectedStakeholder;

        public StakeholdersForm()
        {
            InitializeComponent();
            _stakeholderService = new StakeholderService();

            cboType.Items.AddRange(new object[] { "Buyer", "Seller" });
            cboType.SelectedIndex = 0;
            cboType.SelectedIndexChanged += cboType_SelectedIndexChanged;

            LoadStakeholders();
            SetEditMode(false);
        }

        private async void LoadStakeholders()
        {
            try
            {
                var stakeholders = await _stakeholderService.GetAllStakeholdersAsync();
                dgvStakeholders.DataSource = stakeholders;
                FormatGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading stakeholders: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatGrid()
        {
            dgvStakeholders.Columns["StakeholderId"].Visible = false;
            dgvStakeholders.Columns["Name"].HeaderText = "Name";
            dgvStakeholders.Columns["ContactNumber"].HeaderText = "Contact Number";
            dgvStakeholders.Columns["Address"].HeaderText = "Address";
            dgvStakeholders.Columns["Type"].HeaderText = "Type";
            dgvStakeholders.Columns["DiscountPercentage"].HeaderText = "Discount (%)";
            
            dgvStakeholders.Columns["DiscountPercentage"].DefaultCellStyle.Format = "N2";
            
            foreach (DataGridViewColumn col in dgvStakeholders.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private void SetEditMode(bool editing)
        {
            pnlEdit.Enabled = editing;
            dgvStakeholders.Enabled = !editing;
            btnNew.Enabled = !editing;
            btnEdit.Enabled = !editing && dgvStakeholders.SelectedRows.Count > 0;
            btnDelete.Enabled = !editing && dgvStakeholders.SelectedRows.Count > 0;
            btnSave.Enabled = editing;
            btnCancel.Enabled = editing;
            
            // Hide discount fields for Seller type
            bool isSellerType = cboType.SelectedIndex == 1; // Index 1 is Seller
            txtDiscount.Visible = !isSellerType;
            label5.Visible = !isSellerType;
            if (isSellerType)
            {
                txtDiscount.Text = "0.00";
            }
        }

        private void ClearFields()
        {
            txtName.Text = "";
            txtContact.Text = "";
            txtAddress.Text = "";
            cboType.SelectedIndex = 0;
            txtDiscount.Text = "0.00";
            _selectedStakeholder = null;
        }

        private void dgvStakeholders_SelectionChanged(object sender, EventArgs e)
        {
            btnEdit.Enabled = dgvStakeholders.SelectedRows.Count > 0;
            btnDelete.Enabled = dgvStakeholders.SelectedRows.Count > 0;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearFields();
            SetEditMode(true);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvStakeholders.SelectedRows.Count > 0)
            {
                _selectedStakeholder = (Stakeholder)dgvStakeholders.SelectedRows[0].DataBoundItem;
                txtName.Text = _selectedStakeholder.Name;
                txtContact.Text = _selectedStakeholder.ContactNumber;
                txtAddress.Text = _selectedStakeholder.Address;
                cboType.SelectedIndex = (int)_selectedStakeholder.Type;
                txtDiscount.Text = _selectedStakeholder.DiscountPercentage.ToString("N2");
                SetEditMode(true);
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvStakeholders.SelectedRows.Count > 0)
            {
                var stakeholder = (Stakeholder)dgvStakeholders.SelectedRows[0].DataBoundItem;
                if (MessageBox.Show($"Are you sure you want to delete {stakeholder.Name}?", "Confirm Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        await _stakeholderService.DeleteStakeholderAsync(stakeholder.StakeholderId);
                        LoadStakeholders();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting stakeholder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter stakeholder name", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtDiscount.Text, out decimal discount) || discount < 0 || discount > 100)
            {
                MessageBox.Show("Please enter a valid discount percentage (0-100)", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var stakeholder = _selectedStakeholder ?? new Stakeholder();
                stakeholder.Name = txtName.Text.Trim();
                stakeholder.ContactNumber = txtContact.Text.Trim();
                stakeholder.Address = txtAddress.Text.Trim();
                stakeholder.Type = (StakeholderType)cboType.SelectedIndex;
                stakeholder.DiscountPercentage = discount;

                if (_selectedStakeholder == null)
                {
                    await _stakeholderService.AddStakeholderAsync(stakeholder);
                }
                else
                {
                    await _stakeholderService.UpdateStakeholderAsync(stakeholder);
                }

                LoadStakeholders();
                SetEditMode(false);
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving stakeholder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            SetEditMode(false);
            ClearFields();
        }

        private void cboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isSellerType = cboType.SelectedIndex == 1; // Index 1 is Seller
            txtDiscount.Visible = !isSellerType;
            label5.Visible = !isSellerType;
            if (isSellerType)
            {
                txtDiscount.Text = "0.00";
            }
        }
    }
}

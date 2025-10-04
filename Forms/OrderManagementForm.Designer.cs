namespace OmEnterpriseBillingWin
{
    partial class OrderManagementForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.groupBoxFilters = new System.Windows.Forms.GroupBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cboStakeholders = new System.Windows.Forms.ComboBox();
            this.groupBoxOrders = new System.Windows.Forms.GroupBox();
            this.dgvOrders = new System.Windows.Forms.DataGridView();
            this.groupBoxSummary = new System.Windows.Forms.GroupBox();
            this.lblTotalAmount = new System.Windows.Forms.Label();
            this.lblOrderCount = new System.Windows.Forms.Label();
            this.btnViewOrder = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.groupBoxFilters.SuspendLayout();
            this.groupBoxOrders.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOrders)).BeginInit();
            this.groupBoxSummary.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxFilters
            // 
            this.groupBoxFilters.Controls.Add(this.btnRefresh);
            this.groupBoxFilters.Controls.Add(this.label1);
            this.groupBoxFilters.Controls.Add(this.cboStakeholders);
            this.groupBoxFilters.Location = new System.Drawing.Point(12, 12);
            this.groupBoxFilters.Name = "groupBoxFilters";
            this.groupBoxFilters.Size = new System.Drawing.Size(900, 60);
            this.groupBoxFilters.TabIndex = 0;
            this.groupBoxFilters.TabStop = false;
            this.groupBoxFilters.Text = "Filter Options";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(450, 22);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 25);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Customer:";
            // 
            // cboStakeholders
            // 
            this.cboStakeholders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStakeholders.FormattingEnabled = true;
            this.cboStakeholders.Location = new System.Drawing.Point(83, 22);
            this.cboStakeholders.Name = "cboStakeholders";
            this.cboStakeholders.Size = new System.Drawing.Size(350, 23);
            this.cboStakeholders.TabIndex = 1;
            // 
            // groupBoxOrders
            // 
            this.groupBoxOrders.Controls.Add(this.dgvOrders);
            this.groupBoxOrders.Location = new System.Drawing.Point(12, 78);
            this.groupBoxOrders.Name = "groupBoxOrders";
            this.groupBoxOrders.Size = new System.Drawing.Size(900, 400);
            this.groupBoxOrders.TabIndex = 1;
            this.groupBoxOrders.TabStop = false;
            this.groupBoxOrders.Text = "Orders";
            // 
            // dgvOrders
            // 
            this.dgvOrders.AllowUserToAddRows = false;
            this.dgvOrders.AllowUserToDeleteRows = false;
            this.dgvOrders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvOrders.Location = new System.Drawing.Point(15, 22);
            this.dgvOrders.Name = "dgvOrders";
            this.dgvOrders.ReadOnly = true;
            this.dgvOrders.Size = new System.Drawing.Size(870, 365);
            this.dgvOrders.TabIndex = 0;
            // 
            // groupBoxSummary
            // 
            this.groupBoxSummary.Controls.Add(this.lblTotalAmount);
            this.groupBoxSummary.Controls.Add(this.lblOrderCount);
            this.groupBoxSummary.Location = new System.Drawing.Point(12, 484);
            this.groupBoxSummary.Name = "groupBoxSummary";
            this.groupBoxSummary.Size = new System.Drawing.Size(500, 60);
            this.groupBoxSummary.TabIndex = 2;
            this.groupBoxSummary.TabStop = false;
            this.groupBoxSummary.Text = "Summary";
            // 
            // lblTotalAmount
            // 
            this.lblTotalAmount.AutoSize = true;
            this.lblTotalAmount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalAmount.Location = new System.Drawing.Point(250, 25);
            this.lblTotalAmount.Name = "lblTotalAmount";
            this.lblTotalAmount.Size = new System.Drawing.Size(113, 15);
            this.lblTotalAmount.TabIndex = 1;
            this.lblTotalAmount.Text = "Total Amount: $0.00";
            // 
            // lblOrderCount
            // 
            this.lblOrderCount.AutoSize = true;
            this.lblOrderCount.Location = new System.Drawing.Point(15, 25);
            this.lblOrderCount.Name = "lblOrderCount";
            this.lblOrderCount.Size = new System.Drawing.Size(84, 15);
            this.lblOrderCount.TabIndex = 0;
            this.lblOrderCount.Text = "Total Orders: 0";
            // 
            // btnViewOrder
            // 
            this.btnViewOrder.BackColor = System.Drawing.Color.LightBlue;
            this.btnViewOrder.Enabled = false;
            this.btnViewOrder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnViewOrder.Location = new System.Drawing.Point(720, 500);
            this.btnViewOrder.Name = "btnViewOrder";
            this.btnViewOrder.Size = new System.Drawing.Size(100, 30);
            this.btnViewOrder.TabIndex = 3;
            this.btnViewOrder.Text = "View Order";
            this.btnViewOrder.UseVisualStyleBackColor = false;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(830, 500);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(80, 30);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // OrderManagementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(924, 556);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnViewOrder);
            this.Controls.Add(this.groupBoxSummary);
            this.Controls.Add(this.groupBoxOrders);
            this.Controls.Add(this.groupBoxFilters);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OrderManagementForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Order Management";
            this.groupBoxFilters.ResumeLayout(false);
            this.groupBoxFilters.PerformLayout();
            this.groupBoxOrders.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvOrders)).EndInit();
            this.groupBoxSummary.ResumeLayout(false);
            this.groupBoxSummary.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox groupBoxFilters;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboStakeholders;
        private System.Windows.Forms.GroupBox groupBoxOrders;
        private System.Windows.Forms.DataGridView dgvOrders;
        private System.Windows.Forms.GroupBox groupBoxSummary;
        private System.Windows.Forms.Label lblTotalAmount;
        private System.Windows.Forms.Label lblOrderCount;
        private System.Windows.Forms.Button btnViewOrder;
        private System.Windows.Forms.Button btnClose;
    }
}

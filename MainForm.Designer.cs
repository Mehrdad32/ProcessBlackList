namespace ProcessBlacklist
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.grbProcessList = new System.Windows.Forms.GroupBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.dgvProcessList = new System.Windows.Forms.DataGridView();
            this.grbBlackList = new System.Windows.Forms.GroupBox();
            this.cmbMode = new System.Windows.Forms.ComboBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.dgvBlackList = new System.Windows.Forms.DataGridView();
            this.Column5 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cmsProcessRightClick = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.refreshProcessListNowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.killThisProcessNowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.grbOptions = new System.Windows.Forms.GroupBox();
            this.txtCPU = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbSortTable = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkShowMessage = new System.Windows.Forms.CheckBox();
            this.chkShowThisAppOnTop = new System.Windows.Forms.CheckBox();
            this.btnTray = new System.Windows.Forms.Button();
            this.chkWarnToClose = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.nudRefreshRate = new System.Windows.Forms.NumericUpDown();
            this.chkAskBeforeKill = new System.Windows.Forms.CheckBox();
            this.chkOperation = new System.Windows.Forms.CheckBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.cmsBlackListRightClick = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeThisRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableDisableThisRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeThisRowModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearKilledCounterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearLastKilledTimeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.label7 = new System.Windows.Forms.Label();
            this.grbProcessList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProcessList)).BeginInit();
            this.grbBlackList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBlackList)).BeginInit();
            this.cmsProcessRightClick.SuspendLayout();
            this.grbOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRefreshRate)).BeginInit();
            this.cmsBlackListRightClick.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // grbProcessList
            // 
            this.grbProcessList.Controls.Add(this.txtSearch);
            this.grbProcessList.Controls.Add(this.dgvProcessList);
            this.grbProcessList.Location = new System.Drawing.Point(13, 112);
            this.grbProcessList.Name = "grbProcessList";
            this.grbProcessList.Size = new System.Drawing.Size(409, 372);
            this.grbProcessList.TabIndex = 0;
            this.grbProcessList.TabStop = false;
            this.grbProcessList.Text = "Process List";
            // 
            // txtSearch
            // 
            this.txtSearch.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
            this.txtSearch.Location = new System.Drawing.Point(6, 344);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(397, 22);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.Text = "Qucik search...";
            this.txtSearch.TextChanged += new System.EventHandler(this.TxtSearchProcess_TextChanged);
            this.txtSearch.Enter += new System.EventHandler(this.TxtSearchProcess_Enter);
            this.txtSearch.Leave += new System.EventHandler(this.TxtSearchProcess_Leave);
            // 
            // dgvProcessList
            // 
            this.dgvProcessList.AllowUserToAddRows = false;
            this.dgvProcessList.AllowUserToDeleteRows = false;
            this.dgvProcessList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProcessList.Location = new System.Drawing.Point(7, 21);
            this.dgvProcessList.Name = "dgvProcessList";
            this.dgvProcessList.ReadOnly = true;
            this.dgvProcessList.Size = new System.Drawing.Size(396, 316);
            this.dgvProcessList.TabIndex = 0;
            this.dgvProcessList.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DgvProcessList_CellMouseDown);
            // 
            // grbBlackList
            // 
            this.grbBlackList.Controls.Add(this.cmbMode);
            this.grbBlackList.Controls.Add(this.btnAdd);
            this.grbBlackList.Controls.Add(this.txtInput);
            this.grbBlackList.Controls.Add(this.dgvBlackList);
            this.grbBlackList.Location = new System.Drawing.Point(428, 112);
            this.grbBlackList.Name = "grbBlackList";
            this.grbBlackList.Size = new System.Drawing.Size(606, 372);
            this.grbBlackList.TabIndex = 1;
            this.grbBlackList.TabStop = false;
            this.grbBlackList.Text = "Blacklist";
            // 
            // cmbMode
            // 
            this.cmbMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMode.Enabled = false;
            this.cmbMode.FormattingEnabled = true;
            this.cmbMode.Items.AddRange(new object[] {
            "Just this process",
            "Contain this text"});
            this.cmbMode.Location = new System.Drawing.Point(298, 344);
            this.cmbMode.Name = "cmbMode";
            this.cmbMode.Size = new System.Drawing.Size(197, 22);
            this.cmbMode.TabIndex = 3;
            this.cmbMode.SelectedIndexChanged += new System.EventHandler(this.CmbMode_SelectedIndexChanged);
            // 
            // btnAdd
            // 
            this.btnAdd.Enabled = false;
            this.btnAdd.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
            this.btnAdd.Location = new System.Drawing.Point(501, 343);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(99, 23);
            this.btnAdd.TabIndex = 2;
            this.btnAdd.Text = "Add to blacklist";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
            // 
            // txtInput
            // 
            this.txtInput.Location = new System.Drawing.Point(6, 344);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(286, 22);
            this.txtInput.TabIndex = 1;
            this.txtInput.TextChanged += new System.EventHandler(this.TxtInput_TextChanged);
            // 
            // dgvBlackList
            // 
            this.dgvBlackList.AllowUserToAddRows = false;
            this.dgvBlackList.AllowUserToDeleteRows = false;
            this.dgvBlackList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBlackList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column5,
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4});
            this.dgvBlackList.Location = new System.Drawing.Point(6, 22);
            this.dgvBlackList.Name = "dgvBlackList";
            this.dgvBlackList.ReadOnly = true;
            this.dgvBlackList.Size = new System.Drawing.Size(594, 316);
            this.dgvBlackList.TabIndex = 0;
            this.dgvBlackList.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DgvBlackList_CellMouseDown);
            this.dgvBlackList.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.DgvBlackList_RowsAdded);
            this.dgvBlackList.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.DgvBlackList_RowsRemoved);
            // 
            // Column5
            // 
            this.Column5.HeaderText = "Enable";
            this.Column5.Name = "Column5";
            this.Column5.ReadOnly = true;
            this.Column5.Width = 50;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Text to kill";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 140;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "Mode";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "Killed count";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "Last killed time";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.Width = 160;
            // 
            // cmsProcessRightClick
            // 
            this.cmsProcessRightClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshProcessListNowToolStripMenuItem,
            this.killThisProcessNowToolStripMenuItem,
            this.toolStripMenuItem1,
            this.toolStripMenuItem2,
            this.toolStripMenuItem3,
            this.toolStripMenuItem4});
            this.cmsProcessRightClick.Name = "contextMenuStrip1";
            this.cmsProcessRightClick.Size = new System.Drawing.Size(244, 136);
            this.cmsProcessRightClick.Text = "Copy process details";
            // 
            // refreshProcessListNowToolStripMenuItem
            // 
            this.refreshProcessListNowToolStripMenuItem.Name = "refreshProcessListNowToolStripMenuItem";
            this.refreshProcessListNowToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.refreshProcessListNowToolStripMenuItem.Text = "Refresh process list";
            this.refreshProcessListNowToolStripMenuItem.Click += new System.EventHandler(this.RefreshProcessListNowToolStripMenuItem_Click);
            // 
            // killThisProcessNowToolStripMenuItem
            // 
            this.killThisProcessNowToolStripMenuItem.Name = "killThisProcessNowToolStripMenuItem";
            this.killThisProcessNowToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.killThisProcessNowToolStripMenuItem.Text = "Kill now this process";
            this.killThisProcessNowToolStripMenuItem.Click += new System.EventHandler(this.KillThisProcessNowToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(243, 22);
            this.toolStripMenuItem1.Text = "Copy process details";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.CopyProcessName_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(243, 22);
            this.toolStripMenuItem2.Text = "Search process in Google";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.SearchProcessOnGoogle_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(243, 22);
            this.toolStripMenuItem3.Text = "Add name to blacklist inputbox";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.AddNameToInput_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(243, 22);
            this.toolStripMenuItem4.Text = "Add to blacklist just this process";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.ToolStripMenuItem4_Click);
            // 
            // grbOptions
            // 
            this.grbOptions.Controls.Add(this.txtCPU);
            this.grbOptions.Controls.Add(this.label3);
            this.grbOptions.Controls.Add(this.cmbSortTable);
            this.grbOptions.Controls.Add(this.label2);
            this.grbOptions.Controls.Add(this.chkShowMessage);
            this.grbOptions.Controls.Add(this.chkShowThisAppOnTop);
            this.grbOptions.Controls.Add(this.btnTray);
            this.grbOptions.Controls.Add(this.chkWarnToClose);
            this.grbOptions.Controls.Add(this.label1);
            this.grbOptions.Controls.Add(this.nudRefreshRate);
            this.grbOptions.Controls.Add(this.chkAskBeforeKill);
            this.grbOptions.Controls.Add(this.chkOperation);
            this.grbOptions.Location = new System.Drawing.Point(13, 490);
            this.grbOptions.Name = "grbOptions";
            this.grbOptions.Size = new System.Drawing.Size(1021, 89);
            this.grbOptions.TabIndex = 3;
            this.grbOptions.TabStop = false;
            this.grbOptions.Text = "Blacklist Options";
            // 
            // txtCPU
            // 
            this.txtCPU.ForeColor = System.Drawing.Color.DimGray;
            this.txtCPU.Location = new System.Drawing.Point(943, 19);
            this.txtCPU.Name = "txtCPU";
            this.txtCPU.ReadOnly = true;
            this.txtCPU.Size = new System.Drawing.Size(60, 22);
            this.txtCPU.TabIndex = 11;
            this.txtCPU.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label3.Location = new System.Drawing.Point(781, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(156, 14);
            this.label3.TabIndex = 10;
            this.label3.Text = "--> CPU usage by this rate:";
            // 
            // cmbSortTable
            // 
            this.cmbSortTable.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSortTable.FormattingEnabled = true;
            this.cmbSortTable.Items.AddRange(new object[] {
            "None",
            "Name",
            "ID",
            "Responding"});
            this.cmbSortTable.Location = new System.Drawing.Point(669, 43);
            this.cmbSortTable.Name = "cmbSortTable";
            this.cmbSortTable.Size = new System.Drawing.Size(106, 22);
            this.cmbSortTable.TabIndex = 9;
            this.cmbSortTable.SelectedIndexChanged += new System.EventHandler(this.CmbSortTable_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(568, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 14);
            this.label2.TabIndex = 8;
            this.label2.Text = "Process list sort:";
            // 
            // chkShowMessage
            // 
            this.chkShowMessage.AutoSize = true;
            this.chkShowMessage.Checked = true;
            this.chkShowMessage.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowMessage.Location = new System.Drawing.Point(6, 69);
            this.chkShowMessage.Name = "chkShowMessage";
            this.chkShowMessage.Size = new System.Drawing.Size(391, 18);
            this.chkShowMessage.TabIndex = 7;
            this.chkShowMessage.Text = "Show message after killed application [works just in kill now mode]";
            this.chkShowMessage.UseVisualStyleBackColor = true;
            // 
            // chkShowThisAppOnTop
            // 
            this.chkShowThisAppOnTop.AutoSize = true;
            this.chkShowThisAppOnTop.Checked = true;
            this.chkShowThisAppOnTop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowThisAppOnTop.Location = new System.Drawing.Point(571, 69);
            this.chkShowThisAppOnTop.Name = "chkShowThisAppOnTop";
            this.chkShowThisAppOnTop.Size = new System.Drawing.Size(182, 18);
            this.chkShowThisAppOnTop.TabIndex = 6;
            this.chkShowThisAppOnTop.Text = "Show this application on top";
            this.chkShowThisAppOnTop.UseVisualStyleBackColor = true;
            this.chkShowThisAppOnTop.CheckedChanged += new System.EventHandler(this.ChkOnTop_CheckedChanged);
            // 
            // btnTray
            // 
            this.btnTray.Location = new System.Drawing.Point(916, 60);
            this.btnTray.Name = "btnTray";
            this.btnTray.Size = new System.Drawing.Size(99, 23);
            this.btnTray.TabIndex = 5;
            this.btnTray.Text = "Minimize App";
            this.btnTray.UseVisualStyleBackColor = true;
            this.btnTray.Click += new System.EventHandler(this.BtnTray_Click);
            // 
            // chkWarnToClose
            // 
            this.chkWarnToClose.AutoSize = true;
            this.chkWarnToClose.Enabled = false;
            this.chkWarnToClose.Location = new System.Drawing.Point(145, 21);
            this.chkWarnToClose.Name = "chkWarnToClose";
            this.chkWarnToClose.Size = new System.Drawing.Size(258, 18);
            this.chkWarnToClose.TabIndex = 4;
            this.chkWarnToClose.Text = "Warn in not responding process to close it";
            this.chkWarnToClose.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(569, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(148, 14);
            this.label1.TabIndex = 3;
            this.label1.Text = "Check process rate (ms): ";
            // 
            // nudRefreshRate
            // 
            this.nudRefreshRate.Location = new System.Drawing.Point(723, 17);
            this.nudRefreshRate.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudRefreshRate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudRefreshRate.Name = "nudRefreshRate";
            this.nudRefreshRate.Size = new System.Drawing.Size(52, 22);
            this.nudRefreshRate.TabIndex = 2;
            this.nudRefreshRate.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudRefreshRate.ValueChanged += new System.EventHandler(this.NupRefreshRate_ValueChanged);
            // 
            // chkAskBeforeKill
            // 
            this.chkAskBeforeKill.AutoSize = true;
            this.chkAskBeforeKill.Checked = true;
            this.chkAskBeforeKill.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAskBeforeKill.Location = new System.Drawing.Point(6, 45);
            this.chkAskBeforeKill.Name = "chkAskBeforeKill";
            this.chkAskBeforeKill.Size = new System.Drawing.Size(279, 18);
            this.chkAskBeforeKill.TabIndex = 1;
            this.chkAskBeforeKill.Text = "Ask before killing [works just in kill now mode]";
            this.chkAskBeforeKill.UseVisualStyleBackColor = true;
            // 
            // chkOperation
            // 
            this.chkOperation.AutoSize = true;
            this.chkOperation.Location = new System.Drawing.Point(6, 21);
            this.chkOperation.Name = "chkOperation";
            this.chkOperation.Size = new System.Drawing.Size(118, 18);
            this.chkOperation.TabIndex = 0;
            this.chkOperation.Text = "Enable operation";
            this.chkOperation.UseVisualStyleBackColor = true;
            this.chkOperation.CheckedChanged += new System.EventHandler(this.ChkOperation_CheckedChanged);
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // cmsBlackListRightClick
            // 
            this.cmsBlackListRightClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeThisRowToolStripMenuItem,
            this.enableDisableThisRowToolStripMenuItem,
            this.changeThisRowModeToolStripMenuItem,
            this.clearKilledCounterToolStripMenuItem,
            this.clearLastKilledTimeToolStripMenuItem});
            this.cmsBlackListRightClick.Name = "contextMenuStrip2";
            this.cmsBlackListRightClick.Size = new System.Drawing.Size(204, 114);
            // 
            // removeThisRowToolStripMenuItem
            // 
            this.removeThisRowToolStripMenuItem.Name = "removeThisRowToolStripMenuItem";
            this.removeThisRowToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.removeThisRowToolStripMenuItem.Text = "Remove this row";
            this.removeThisRowToolStripMenuItem.Click += new System.EventHandler(this.RemoveThisRowToolStripMenuItem_Click);
            // 
            // enableDisableThisRowToolStripMenuItem
            // 
            this.enableDisableThisRowToolStripMenuItem.Name = "enableDisableThisRowToolStripMenuItem";
            this.enableDisableThisRowToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.enableDisableThisRowToolStripMenuItem.Text = "Enable / Disable this row";
            this.enableDisableThisRowToolStripMenuItem.Click += new System.EventHandler(this.EnableDisableThisRowToolStripMenuItem_Click);
            // 
            // changeThisRowModeToolStripMenuItem
            // 
            this.changeThisRowModeToolStripMenuItem.Name = "changeThisRowModeToolStripMenuItem";
            this.changeThisRowModeToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.changeThisRowModeToolStripMenuItem.Text = "Change mode";
            this.changeThisRowModeToolStripMenuItem.Click += new System.EventHandler(this.ChangeThisRowModeToolStripMenuItem_Click);
            // 
            // clearKilledCounterToolStripMenuItem
            // 
            this.clearKilledCounterToolStripMenuItem.Name = "clearKilledCounterToolStripMenuItem";
            this.clearKilledCounterToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.clearKilledCounterToolStripMenuItem.Text = "Clear killed counter";
            this.clearKilledCounterToolStripMenuItem.Click += new System.EventHandler(this.ClearKilledCounterToolStripMenuItem_Click);
            // 
            // clearLastKilledTimeToolStripMenuItem
            // 
            this.clearLastKilledTimeToolStripMenuItem.Name = "clearLastKilledTimeToolStripMenuItem";
            this.clearLastKilledTimeToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.clearLastKilledTimeToolStripMenuItem.Text = "Clear last killed time";
            this.clearLastKilledTimeToolStripMenuItem.Click += new System.EventHandler(this.ClearLastKilledTimeToolStripMenuItem_Click);
            // 
            // timer2
            // 
            this.timer2.Enabled = true;
            this.timer2.Interval = 500;
            this.timer2.Tick += new System.EventHandler(this.Timer2_Tick);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::ProcessBlacklist.Properties.Resources.Process_Warning_icon;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(125, 94);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
            this.label4.Location = new System.Drawing.Point(143, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(197, 27);
            this.label4.TabIndex = 6;
            this.label4.Text = "Process Blacklist";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
            this.label5.Location = new System.Drawing.Point(336, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "v1.0 (2019-12-23)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(143, 39);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(537, 14);
            this.label6.TabIndex = 8;
            this.label6.Text = "If you want to put one or more process in blacklist and when they run automaticly" +
    " will be killed. ";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(143, 53);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(402, 14);
            this.linkLabel1.TabIndex = 9;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "For more information about this application see this page in my website.";
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(355, 92);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(112, 14);
            this.linkLabel2.TabIndex = 10;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "www.Mehrdad32.ir";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(143, 92);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(206, 14);
            this.label7.TabIndex = 11;
            this.label7.Text = "Copyright © 2020 By Mehrdad32 on";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1046, 588);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.linkLabel2);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.grbOptions);
            this.Controls.Add(this.grbBlackList);
            this.Controls.Add(this.grbProcessList);
            this.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Process Blacklist v1.0";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.grbProcessList.ResumeLayout(false);
            this.grbProcessList.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProcessList)).EndInit();
            this.grbBlackList.ResumeLayout(false);
            this.grbBlackList.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBlackList)).EndInit();
            this.cmsProcessRightClick.ResumeLayout(false);
            this.grbOptions.ResumeLayout(false);
            this.grbOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRefreshRate)).EndInit();
            this.cmsBlackListRightClick.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grbProcessList;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.DataGridView dgvProcessList;
        private System.Windows.Forms.GroupBox grbBlackList;
        private System.Windows.Forms.ComboBox cmbMode;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.DataGridView dgvBlackList;
        private System.Windows.Forms.ContextMenuStrip cmsProcessRightClick;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.GroupBox grbOptions;
        private System.Windows.Forms.Button btnTray;
        private System.Windows.Forms.CheckBox chkWarnToClose;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nudRefreshRate;
        private System.Windows.Forms.CheckBox chkAskBeforeKill;
        private System.Windows.Forms.CheckBox chkOperation;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripMenuItem refreshProcessListNowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem killThisProcessNowToolStripMenuItem;
        private System.Windows.Forms.CheckBox chkShowThisAppOnTop;
        private System.Windows.Forms.CheckBox chkShowMessage;
        private System.Windows.Forms.ComboBox cmbSortTable;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ContextMenuStrip cmsBlackListRightClick;
        private System.Windows.Forms.ToolStripMenuItem removeThisRowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeThisRowModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearKilledCounterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearLastKilledTimeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableDisableThisRowToolStripMenuItem;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column5;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.TextBox txtCPU;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.Label label7;
    }
}


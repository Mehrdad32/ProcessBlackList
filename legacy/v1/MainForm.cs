//ProcessBlackList By Mehrdad32, 2020 [www.Mehrdad32.ir]

using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Security.Principal;
using System.Windows.Forms;

namespace ProcessBlacklist
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private Process[] _ps;
        private int _rowIndex = -1;
        private int _cellIndex = -1;
        private readonly PerformanceCounter _myAppCpu = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName, true);

        public bool IsUserAdministrator()
        {
            //bool value to hold our return value
            bool isAdmin;

            var user = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(user);

            try
            {
                //get the currently logged in user
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                isAdmin = false;
            }

            return isAdmin;
        }

        private void OnRefreshGrid(object sender, EventArgs e)
        {
            if(dgvProcessList.Rows.Count >0)
            {
                if (dgvProcessList.CurrentRow != null) _rowIndex = dgvProcessList.CurrentRow.Index;
                _cellIndex = dgvProcessList.CurrentCell.ColumnIndex;
            }

            var source = new BindingSource();
            var table = new DataTable("Process List");

            _ps = Process.GetProcesses();

            table.Columns.Add("Name");
            table.Columns.Add("ID");
            table.Columns.Add("Responding");

            for (var i = 0; i < _ps.Length; ++i)
            {
                table.Rows.Add(new object[] { _ps[i].ProcessName + ".exe", _ps[i].Id, _ps[i].Responding });
            }

            table.AcceptChanges();
            source.DataSource = table;
            dgvProcessList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            var scroll = dgvProcessList.FirstDisplayedScrollingRowIndex;
            dgvProcessList.DataSource = source;

            if (scroll != -1)
                dgvProcessList.FirstDisplayedScrollingRowIndex = scroll;

            if(_rowIndex != -1 && _cellIndex != -1)
            {
                dgvProcessList.CurrentCell = dgvProcessList.Rows[_rowIndex].Cells[_cellIndex];
            }

            if(cmbSortTable.SelectedIndex != 0)
            {
                dgvProcessList.Sort(dgvProcessList.Columns[cmbSortTable.SelectedIndex-1], ListSortDirection.Ascending);
            }

            if (_ps.Length != 0)
                grbProcessList.Text = "Process List (" + _ps.Length + ")";
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (IsUserAdministrator())
            {
                Text += " (Admin mode is ON)";
            }
            else
            {
                try
                {
                    var processModule = Process.GetCurrentProcess().MainModule;
                    if (processModule == null) return;
                    var exeName = processModule.FileName;
                    var startInfo = new ProcessStartInfo(exeName) {Verb = "runas"};
                    Process.Start(startInfo);

                    //Application.Current.Shutdown();
                    return;
                }
                catch (SystemException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            cmbSortTable.SelectedIndex = 3;
            OnRefreshGrid(null, null);
        }

        private void TxtSearchProcess_TextChanged(object sender, EventArgs e)
        {
            if (txtSearch.Text == "") return;

            txtSearch.Font = new Font(txtSearch.Font, FontStyle.Bold);

            for (var i = 0; i < dgvProcessList.Rows.Count; i++)
            {
                if (dgvProcessList.Rows[i].Cells[0].Value.ToString().ToLower().Contains(txtSearch.Text.ToLower()))
                {
                    dgvProcessList.CurrentCell = dgvProcessList.Rows[i].Cells[0];
                }
            }
        }

        private void TxtSearchProcess_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "Quick search...")
            {
                txtSearch.Text = "";
                txtSearch.Font = new Font(txtSearch.Font, FontStyle.Bold);
            }
        }

        private void TxtSearchProcess_Leave(object sender, EventArgs e)
        {
            if (txtSearch.Text == "")
            {
                txtSearch.Text = "Quick search...";
                txtSearch.Font = new Font(txtSearch.Font, FontStyle.Italic);
                dgvProcessList.Refresh();
            }
        }

        private void DgvProcessList_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Ignore if a column or row header is clicked
            if (e.RowIndex == -1 || e.ColumnIndex == -1) return;
            if (e.Button != MouseButtons.Right) return;

            var clickedCell = (sender as DataGridView)?.Rows[e.RowIndex].Cells[e.ColumnIndex];

            // Here you can do whatever you want with the cell
            this.dgvProcessList.CurrentCell = clickedCell;  // Select the clicked cell, for instance

            // Get mouse position relative to the vehicles grid
            var relativeMousePosition = dgvProcessList.PointToClient(Cursor.Position);

            // Show the context menu
            this.cmsProcessRightClick.Show(dgvProcessList, relativeMousePosition);
        }

        private void CopyProcessName_Click(object sender, EventArgs e)
        {
            if (dgvProcessList.CurrentRow != null)
                Clipboard.SetText("Process Name:" + dgvProcessList.CurrentRow.Cells[0].Value +
                                  ", Process ID:" + dgvProcessList.CurrentRow.Cells[1].Value);
        }

        private void SearchProcessOnGoogle_Click(object sender, EventArgs e)
        {
            if (dgvProcessList.CurrentRow != null)
                Process.Start("https://www.google.com/search?q=windows+process+" +
                                                 dgvProcessList.CurrentRow.Cells[0].Value);
        }

        private void AddNameToInput_Click(object sender, EventArgs e)
        {
            if (dgvProcessList.CurrentRow != null) txtInput.Text = dgvProcessList.CurrentRow.Cells[0].Value.ToString();
        }

        private void RefreshProcessListNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnRefreshGrid(null, null);
        }

        private void KillThisProcessNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dofProcess = dgvProcessList.CurrentRow?.Cells[1].Value.ToString();

            if (dgvProcessList.CurrentRow == null) return;

            foreach(var p in _ps)
            {
                if (p.Id.ToString() != dofProcess) continue;

                if (chkAskBeforeKill.Checked)
                {
                    if (MessageBox.Show("Process [" + p.ProcessName + "] goes to kill?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        p.Kill();
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    p.Kill();
                }

                if(chkShowMessage.Checked)
                {
                    MessageBox.Show("Process [" + p.ProcessName + "] has been killed.", "Alter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                OnRefreshGrid(null,null);
            }
        }

        private void ChkOnTop_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = chkShowThisAppOnTop.Checked;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if(txtInput.Text != "" && cmbMode.SelectedItem != null)
            {
                txtInput.Text = txtInput.Text.Replace(".exe", "");
                dgvBlackList.Rows.Add(true, txtInput.Text, cmbMode.Text, 0, 0);
                txtSearch.Text = "";
            }
            else
            {
                MessageBox.Show("Fill input options to add new item to blacklist.","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        private void TxtInput_TextChanged(object sender, EventArgs e)
        {
            if(txtInput.Text == "")
            {
                btnAdd.Enabled = cmbMode.Enabled = false;
            }
            else
            {
                btnAdd.Enabled = cmbMode.Enabled = true;
                cmbMode.SelectedIndex = 0;
            }
        }

        private void CmbSortTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSortTable.SelectedIndex != 0 && dgvProcessList.DataSource != null)
            {
                dgvProcessList.Sort(dgvProcessList.Columns[cmbSortTable.SelectedIndex - 1], ListSortDirection.Ascending);
            }
        }

        private void ToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if (dgvProcessList.CurrentRow != null)
            {
                string name = dgvProcessList.CurrentRow.Cells[0].Value.ToString();
                dgvBlackList.Rows.Add(true, name, "Just this process", 0, 0);
            }
        }

        private void DgvBlackList_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Ignore if a column or row header is clicked
            if (e.RowIndex == -1 || e.ColumnIndex == -1) return;
            if (e.Button != MouseButtons.Right) return;
            var clickedCell = (sender as DataGridView)?.Rows[e.RowIndex].Cells[e.ColumnIndex];

            // Here you can do whatever you want with the cell
            dgvBlackList.CurrentCell = clickedCell;  // Select the clicked cell, for instance

            // Get mouse position relative to the vehicles grid
            var relativeMousePosition = dgvBlackList.PointToClient(Cursor.Position);

            // Show the context menu
            cmsBlackListRightClick.Show(dgvBlackList, relativeMousePosition);
        }

        private void RemoveThisRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvBlackList.CurrentRow != null)
                dgvBlackList.Rows.Remove(dgvBlackList.Rows[dgvBlackList.CurrentRow.Index]);
        }

        private void ChangeThisRowModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvBlackList.CurrentRow != null && dgvBlackList.Rows[dgvBlackList.CurrentRow.Index].Cells[2].Value.ToString() == "Just this process")
            {
                dgvBlackList.Rows[dgvBlackList.CurrentRow.Index].Cells[2].Value = "Contain this text";
            }
            else
            {
                if (dgvBlackList.CurrentRow != null)
                    dgvBlackList.Rows[dgvBlackList.CurrentRow.Index].Cells[2].Value = "Just this process";
            }
        }

        private void ClearKilledCounterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvBlackList.CurrentRow != null) dgvBlackList.Rows[dgvBlackList.CurrentRow.Index].Cells[3].Value = "0";
        }

        private void ClearLastKilledTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvBlackList.CurrentRow != null) dgvBlackList.Rows[dgvBlackList.CurrentRow.Index].Cells[4].Value = "0";
        }

        private void ChkOperation_CheckedChanged(object sender, EventArgs e)
        {
            if(chkOperation.Checked)
            {
                if(dgvBlackList.Rows.Count < 1)
                {
                    MessageBox.Show("Before start monitoring you must add new item to blacklist. Operation to start has been canceled.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    chkOperation.Checked = false;
                    return;
                }
                timer1.Start();
                this.Text += " [Monitoring]";
                chkWarnToClose.Enabled = true;
            }
            else
            {
                timer1.Stop();
                chkWarnToClose.Enabled = false;
                this.Text = this.Text.Replace(" [Monitoring]", "");
            }
        }

        private void NupRefreshRate_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = (int)nudRefreshRate.Value;
        }
        
        public float GetCPU()
        {
            return _myAppCpu.NextValue();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (dgvBlackList.Rows.Count > 0)
            {
                try
                {
                    OnRefreshGrid(null, null);

                    for (var i = 0; i < dgvBlackList.Rows.Count; i++)
                    {
                        if (!(bool) dgvBlackList.Rows[i].Cells[0].Value) continue;

                        var nameOfProcess = dgvBlackList.Rows[i].Cells[1].Value.ToString();
                        var countOfKilled = int.Parse(dgvBlackList.Rows[i].Cells[3].Value.ToString());

                        if (dgvBlackList.Rows[i].Cells[2].Value.ToString() == "Just this process")
                        {
                            foreach (var p in _ps)
                            {
                                if (p.ProcessName != nameOfProcess) continue;
                                p.Kill();
                                dgvBlackList.Rows[i].Cells[3].Value = countOfKilled + 1;
                                dgvBlackList.Rows[i].Cells[4].Value = DateTime.Now;
                                OnRefreshGrid(null, null);
                            }
                        }
                        else
                        {
                            foreach (var p in _ps)
                            {
                                if (!p.ProcessName.ToLower().Contains(nameOfProcess.ToLower())) continue;

                                p.Kill();

                                dgvBlackList.Rows[i].Cells[3].Value = countOfKilled + 1;
                                dgvBlackList.Rows[i].Cells[4].Value = DateTime.Now;

                                OnRefreshGrid(null, null);
                            }
                        }
                    }
                }
                catch (SystemException ex)
                {
                    MessageBox.Show("Error on killing process, Cause:" + ex.Message, "Not killed");
                    timer1.Stop();
                    chkOperation.Checked = false;
                }
            }
            else
            {
                MessageBox.Show("You haven't any item in black list to kill. Operation was stop.", "Not killed");
                timer1.Stop();
                chkOperation.Checked = false;
            }
        }

        private void EnableDisableThisRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(dgvBlackList.CurrentRow != null && (bool)dgvBlackList.CurrentRow.Cells[0].Value)
            {
                dgvBlackList.CurrentRow.Cells[0].Value = false;
            }
            else
            {
                if (dgvBlackList.CurrentRow != null) dgvBlackList.CurrentRow.Cells[0].Value = true;
            }
        }

        private void CmbMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cmbMode.SelectedIndex == 1)
            {
                MessageBox.Show("Be careful to select this option! If you add text [pad] to blacklist by this option all process that contains [pad] in their names, like [phonepad],[gamepad],[notepad] will be killed!","Warning",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            txtCPU.Text = ((int)GetCPU()) + "%";
        }

        private void BtnTray_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void DgvBlackList_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            grbBlackList.Text = "Blacklist (" + dgvBlackList.Rows.Count + ")";
        }

        private void DgvBlackList_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            grbBlackList.Text = "Blacklist (" + dgvBlackList.Rows.Count + ")";
        }
    }
}

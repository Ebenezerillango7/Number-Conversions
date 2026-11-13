using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NumberConversions.Core;
using Paratext.PluginInterfaces;

namespace NumberConversions.Plugin.UI
{
    public class ConversionMainWindow : Form
    {
        private readonly IPluginHost _host;
        private readonly IParatextChildState? _childState;
        private readonly IPluginObject _pluginObj;
        private readonly ParatextScriptureService _service;

        private ComboBox _cboProjects = null!;
        private ComboBox _cboBooks = null!;
        private ComboBox _cboFilter = null!;
        private Button _btnScan = null!;
        private DataGridView _grid = null!;
        private ProgressBar _progressBar = null!;
        private Label _lblStatus = null!;
        private Button _btnSelectAll = null!;
        private Button _btnUnselectAll = null!;
        private Button _btnFlagNotes = null!;
        private Button _btnApply = null!;
        private Button _btnUninstall = null!;

        private readonly List<ConversionProposal> _currentProposals = new List<ConversionProposal>();

        public ConversionMainWindow(IPluginHost host, IParatextChildState? childState, IPluginObject pluginObj)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _childState = childState;
            _pluginObj = pluginObj ?? throw new ArgumentNullException(nameof(pluginObj));
            _service = new ParatextScriptureService(_host);

            InitializeComponent();
            LoadProjects();
        }

        private void InitializeComponent()
        {
            Text = "Tamil Scripture Number Conversions - Paratext 9.5";
            Size = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5f);

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 55, Padding = new Padding(10) };
            var lblProj = new Label { Text = "Project:", AutoSize = true, Location = new Point(10, 18) };
            _cboProjects = new ComboBox { Location = new Point(65, 15), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboProjects.SelectedIndexChanged += (s, e) => LoadBooks();

            var lblBook = new Label { Text = "Book:", AutoSize = true, Location = new Point(235, 18) };
            _cboBooks = new ComboBox { Location = new Point(275, 15), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblFilter = new Label { Text = "View:", AutoSize = true, Location = new Point(425, 18) };
            _cboFilter = new ComboBox { Location = new Point(465, 15), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboFilter.Items.AddRange(new object[] { "All Candidates", "Normal Conversions", "Flagged / Suspicious Only" });
            _cboFilter.SelectedIndex = 0;
            _cboFilter.SelectedIndexChanged += (s, e) => RefreshGrid();

            _btnScan = new Button { Text = "Scan Numbers", Location = new Point(640, 13), Width = 130, Height = 28 };
            _btnScan.Click += BtnScan_Click;

            _btnUninstall = new Button
            {
                Text = "Uninstall Plugin...",
                Location = new Point(930, 13),
                Width = 140,
                Height = 28,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                ForeColor = Color.FromArgb(160, 40, 40),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnUninstall.FlatAppearance.BorderColor = Color.FromArgb(220, 180, 180);
            _btnUninstall.Click += BtnUninstall_Click;

            topPanel.Controls.AddRange(new Control[] { lblProj, _cboProjects, lblBook, _cboBooks, lblFilter, _cboFilter, _btnScan, _btnUninstall });

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var chkCol = new DataGridViewCheckBoxColumn { HeaderText = "Apply", Width = 50, Name = "Apply" };
            var statusCol = new DataGridViewTextBoxColumn { HeaderText = "Status", Width = 90, ReadOnly = true, Name = "Status" };
            var refCol = new DataGridViewTextBoxColumn { HeaderText = "Verse", Width = 85, ReadOnly = true, Name = "Ref" };
            var ruleCol = new DataGridViewTextBoxColumn { HeaderText = "Type", Width = 130, ReadOnly = true, Name = "Rule" };
            var origCol = new DataGridViewTextBoxColumn { HeaderText = "Original", Width = 130, ReadOnly = true, Name = "Orig" };
            var repCol = new DataGridViewTextBoxColumn { HeaderText = "Proposed Replacement", Width = 150, ReadOnly = true, Name = "Rep" };
            var warnCol = new DataGridViewTextBoxColumn { HeaderText = "Warning / Bug Context", Width = 200, ReadOnly = true, Name = "Warn" };

            _grid.Columns.AddRange(chkCol, statusCol, refCol, ruleCol, origCol, repCol, warnCol);

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 75, Padding = new Padding(10) };
            _progressBar = new ProgressBar { Location = new Point(10, 10), Width = 300, Height = 20 };
            _lblStatus = new Label { Text = "Ready", Location = new Point(10, 38), AutoSize = true };

            _btnSelectAll = new Button { Text = "Select Normal", Location = new Point(480, 20), Width = 110, Height = 32 };
            _btnSelectAll.Click += (s, e) => SetSelection(true, onlyNormal: true);

            _btnUnselectAll = new Button { Text = "Unselect All", Location = new Point(600, 20), Width = 100, Height = 32 };
            _btnUnselectAll.Click += (s, e) => SetSelection(false, onlyNormal: false);

            _btnFlagNotes = new Button
            {
                Text = "Flag as Paratext Notes",
                Location = new Point(710, 18),
                Width = 180,
                Height = 36,
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.0f, FontStyle.Bold)
            };
            _btnFlagNotes.Click += BtnFlagNotes_Click;

            _btnApply = new Button
            {
                Text = "Apply Selected",
                Location = new Point(900, 18),
                Width = 160,
                Height = 36,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            _btnApply.Click += BtnApply_Click;

            bottomPanel.Controls.AddRange(new Control[] { _progressBar, _lblStatus, _btnSelectAll, _btnUnselectAll, _btnFlagNotes, _btnApply });

            Controls.Add(_grid);
            Controls.Add(topPanel);
            Controls.Add(bottomPanel);
        }

        private void LoadProjects()
        {
            _cboProjects.Items.Clear();
            var projects = _host.GetAllProjects();
            foreach (var proj in projects)
            {
                _cboProjects.Items.Add(proj);
            }

            if (_childState?.Project != null && _cboProjects.Items.Contains(_childState.Project))
            {
                _cboProjects.SelectedItem = _childState.Project;
            }
            else if (_cboProjects.Items.Count > 0)
            {
                _cboProjects.SelectedIndex = 0;
            }
        }

        private void LoadBooks()
        {
            _cboBooks.Items.Clear();
            if (_cboProjects.SelectedItem is not IProject proj)
                return;

            foreach (IBookInfo bookInfo in proj.AvailableBooks)
            {
                _cboBooks.Items.Add(new BookItem(bookInfo.Number, $"{bookInfo.Code} ({bookInfo.Number})"));
            }

            if (_cboBooks.Items.Count > 0)
                _cboBooks.SelectedIndex = 0;
        }

        private void BtnScan_Click(object? sender, EventArgs e)
        {
            if (_cboProjects.SelectedItem is not IProject proj || _cboBooks.SelectedItem is not BookItem book)
                return;

            _currentProposals.Clear();
            _btnScan.Enabled = false;
            _lblStatus.Text = "Scanning chapters...";

            try
            {
                _currentProposals.AddRange(_service.ScanBook(proj, book.BookNum, (chap, total) =>
                {
                    _progressBar.Maximum = total;
                    _progressBar.Value = chap;
                    _lblStatus.Text = $"Scanning chapter {chap} of {total}...";
                    Application.DoEvents();
                }));

                RefreshGrid();
                int flaggedCount = _currentProposals.Count(p => p.IsSuspicious);
                _lblStatus.Text = $"Scan complete: Found {_currentProposals.Count} items ({flaggedCount} flagged/suspicious for manual review).";
            }
            finally
            {
                _btnScan.Enabled = true;
                _progressBar.Value = 0;
            }
        }

        private void RefreshGrid()
        {
            _grid.Rows.Clear();
            string filter = _cboFilter.SelectedItem?.ToString() ?? "All Candidates";

            IEnumerable<ConversionProposal> list = _currentProposals;
            if (filter == "Normal Conversions")
                list = list.Where(p => !p.IsSuspicious);
            else if (filter == "Flagged / Suspicious Only")
                list = list.Where(p => p.IsSuspicious);

            foreach (var p in list)
            {
                string statusText = p.IsSuspicious ? "⚠️ Flagged" : "✓ Normal";
                string ruleName = p.RuleType.ToString();

                int rowIdx = _grid.Rows.Add(p.IsAccepted, statusText, p.Reference, ruleName, p.MatchedToken, p.Replacement, p.WarningReason);
                var row = _grid.Rows[rowIdx];
                row.Tag = p;

                if (p.IsSuspicious)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 225); // Light amber
                    row.Cells["Status"].Style.ForeColor = Color.DarkOrange;
                    row.Cells["Status"].Style.Font = new Font("Segoe UI", 9.0f, FontStyle.Bold);
                }
            }
        }

        private void SetSelection(bool isSelected, bool onlyNormal)
        {
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.Tag is ConversionProposal p)
                {
                    if (onlyNormal && p.IsSuspicious)
                    {
                        // Suspicious items remain unselected by default
                        row.Cells["Apply"].Value = false;
                        p.IsAccepted = false;
                    }
                    else
                    {
                        row.Cells["Apply"].Value = isSelected;
                        p.IsAccepted = isSelected;
                    }
                }
            }
        }

        private void BtnFlagNotes_Click(object? sender, EventArgs e)
        {
            SyncGridSelections();

            var flagged = _currentProposals.Where(p => p.IsSuspicious).ToList();
            if (flagged.Count == 0)
            {
                MessageBox.Show("No suspicious or flagged items found to create notes for.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"This will create {flagged.Count} Paratext Project Notes anchored to their respective verses for manual translator review.\n\nProceed?",
                "Create Paratext Notes",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            if (_cboProjects.SelectedItem is not IProject proj || _cboBooks.SelectedItem is not BookItem book)
                return;

            bool ok = _service.FlagItemsAsParatextNotes(proj, _pluginObj, book.BookNum, flagged, out string msg);
            MessageBox.Show(msg, ok ? "Notes Created" : "Error", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }

        private void BtnApply_Click(object? sender, EventArgs e)
        {
            SyncGridSelections();

            var approved = _currentProposals.Where(p => p.IsAccepted).ToList();
            if (approved.Count == 0)
            {
                MessageBox.Show("Please select at least one conversion to apply.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Warn if user manually approved any suspicious items
            int suspiciousApproved = approved.Count(p => p.IsSuspicious);
            string warnNotice = suspiciousApproved > 0
                ? $"\n\nNOTE: You have checked {suspiciousApproved} FLAGGED/SUSPICIOUS items. These will be converted as shown."
                : "";

            var confirm = MessageBox.Show(
                $"Are you sure you want to apply {approved.Count} approved number conversions to this book?{warnNotice}\n\nA Paratext history point will be created automatically.",
                "Confirm Scripture Revision",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            if (_cboProjects.SelectedItem is not IProject proj || _cboBooks.SelectedItem is not BookItem book)
                return;

            bool success = _service.ApplyApprovedConversions(proj, _pluginObj, book.BookNum, approved, out string msg);
            if (success)
            {
                MessageBox.Show(msg, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                BtnScan_Click(sender, e);
            }
            else
            {
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUninstall_Click(object? sender, EventArgs e)
        {
            PluginUninstaller.InitiateUninstall(() => this.Close());
        }

        private void SyncGridSelections()
        {
            _grid.EndEdit();

            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.Tag is ConversionProposal p)
                {
                    p.IsAccepted = Convert.ToBoolean(row.Cells["Apply"].Value);
                }
            }
        }

        private class BookItem
        {
            public int BookNum { get; }
            public string DisplayName { get; }

            public BookItem(int bookNum, string displayName)
            {
                BookNum = bookNum;
                DisplayName = displayName;
            }

            public override string ToString() => DisplayName;
        }
    }
}

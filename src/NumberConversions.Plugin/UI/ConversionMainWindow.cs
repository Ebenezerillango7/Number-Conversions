using System;
using System.Collections.Generic;
using System.Drawing;
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
        private Button _btnScan = null!;
        private DataGridView _grid = null!;
        private ProgressBar _progressBar = null!;
        private Label _lblStatus = null!;
        private Button _btnSelectAll = null!;
        private Button _btnUnselectAll = null!;
        private Button _btnApply = null!;

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
            Size = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5f);

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 55, Padding = new Padding(10) };
            var lblProj = new Label { Text = "Project:", AutoSize = true, Location = new Point(10, 18) };
            _cboProjects = new ComboBox { Location = new Point(70, 15), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboProjects.SelectedIndexChanged += (s, e) => LoadBooks();

            var lblBook = new Label { Text = "Book:", AutoSize = true, Location = new Point(270, 18) };
            _cboBooks = new ComboBox { Location = new Point(315, 15), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };

            _btnScan = new Button { Text = "Scan Numbers", Location = new Point(480, 13), Width = 130, Height = 28 };
            _btnScan.Click += BtnScan_Click;

            topPanel.Controls.AddRange(new Control[] { lblProj, _cboProjects, lblBook, _cboBooks, _btnScan });

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
            var refCol = new DataGridViewTextBoxColumn { HeaderText = "Verse", Width = 90, ReadOnly = true, Name = "Ref" };
            var ruleCol = new DataGridViewTextBoxColumn { HeaderText = "Rule", Width = 140, ReadOnly = true, Name = "Rule" };
            var origCol = new DataGridViewTextBoxColumn { HeaderText = "Original", Width = 120, ReadOnly = true, Name = "Orig" };
            var repCol = new DataGridViewTextBoxColumn { HeaderText = "Replacement", Width = 140, ReadOnly = true, Name = "Rep" };
            var prevCol = new DataGridViewTextBoxColumn { HeaderText = "Proposed Verse Preview", ReadOnly = true, Name = "Prev" };

            _grid.Columns.AddRange(chkCol, refCol, ruleCol, origCol, repCol, prevCol);

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 70, Padding = new Padding(10) };
            _progressBar = new ProgressBar { Location = new Point(10, 10), Width = 300, Height = 20 };
            _lblStatus = new Label { Text = "Ready", Location = new Point(10, 35), AutoSize = true };

            _btnSelectAll = new Button { Text = "Select All", Location = new Point(530, 20), Width = 100, Height = 30 };
            _btnSelectAll.Click += (s, e) => SetAllSelection(true);

            _btnUnselectAll = new Button { Text = "Unselect All", Location = new Point(640, 20), Width = 100, Height = 30 };
            _btnUnselectAll.Click += (s, e) => SetAllSelection(false);

            _btnApply = new Button
            {
                Text = "Apply Selected",
                Location = new Point(760, 18),
                Width = 180,
                Height = 34,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            _btnApply.Click += BtnApply_Click;

            bottomPanel.Controls.AddRange(new Control[] { _progressBar, _lblStatus, _btnSelectAll, _btnUnselectAll, _btnApply });

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

            _grid.Rows.Clear();
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

                foreach (var p in _currentProposals)
                {
                    string ruleName = p.RuleType == ConversionRuleType.DigitToText ? "0-199 (Digit -> Word)" : "200+ (Word -> Digit)";
                    _grid.Rows.Add(p.IsAccepted, p.Reference, ruleName, p.MatchedToken, p.Replacement, p.ProposedText);
                }

                _lblStatus.Text = $"Scan complete: Found {_currentProposals.Count} number conversion candidates.";
            }
            finally
            {
                _btnScan.Enabled = true;
                _progressBar.Value = 0;
            }
        }

        private void SetAllSelection(bool isSelected)
        {
            foreach (DataGridViewRow row in _grid.Rows)
            {
                row.Cells["Apply"].Value = isSelected;
            }
            foreach (var p in _currentProposals)
            {
                p.IsAccepted = isSelected;
            }
        }

        private void BtnApply_Click(object? sender, EventArgs e)
        {
            // Sync checkbox states from grid
            for (int i = 0; i < _grid.Rows.Count && i < _currentProposals.Count; i++)
            {
                bool apply = Convert.ToBoolean(_grid.Rows[i].Cells["Apply"].Value);
                _currentProposals[i].IsAccepted = apply;
            }

            var approved = _currentProposals.Where(p => p.IsAccepted).ToList();
            if (approved.Count == 0)
            {
                MessageBox.Show("Please select at least one conversion to apply.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Are you sure you want to apply {approved.Count} approved number conversions to this book?\n\nA Paratext history point will be created automatically.",
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

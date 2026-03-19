using SupportTicketDesktop.Models;
using SupportTicketDesktop.Services;
using SupportTicketDesktop.UI;

namespace SupportTicketDesktop.Forms;

public class TicketListForm : Form
{
    private readonly MainForm _parent;
    private DataGridView _grid = null!;
    private FlatButton _btnRefresh = null!;
    private Label _lblStatus = null!;
    private TextBox _txtSearch = null!;
    private List<Ticket> _allTickets = new();

    public TicketListForm(MainForm parent)
    {
        _parent = parent;
        InitializeComponent();
        _ = LoadTicketsAsync();
    }

    private void InitializeComponent()
    {
        BackColor = Theme.Background;

        // Header
        var lblTitle = new Label
        {
            Text = AppSession.IsAdmin ? "All Support Tickets" : "My Tickets",
            Location = new Point(30, 24),
            Size = new Size(400, 36),
            Font = Theme.FontTitle,
            ForeColor = Theme.TextPrimary
        };
        Controls.Add(lblTitle);

        var lblSub = new Label
        {
            Text = AppSession.IsAdmin
                        ? "Manage and track all customer tickets"
                        : "View and track your submitted tickets",
            Location = new Point(24, 62),
            Size = new Size(400, 20),
            Font = Theme.FontSmall,
            ForeColor = Theme.TextSecondary
        };
        Controls.Add(lblSub);

        _txtSearch = new DarkTextBox
        {
            Location = new Point(450, 30),
            Size = new Size(220, 28),
            PlaceholderText = "Search tickets..."
        };
        _txtSearch.TextChanged += (_, _) => FilterGrid();
        Controls.Add(_txtSearch);

        _btnRefresh = new FlatButton
        {
            Text = "Refresh",
            Location = new Point(686, 28),
            Size = new Size(100, 30),
            Font = Theme.FontSmall
        };
        _btnRefresh.Click += async (_, _) => await LoadTicketsAsync();
        Controls.Add(_btnRefresh);

        if (!AppSession.IsAdmin)
        {
            var btnNew = new FlatButton
            {
                Text = "+ New Ticket",
                Location = new Point(796, 28),
                Size = new Size(120, 30),
                AccentColor = Theme.Success,
                Font = Theme.FontSmall
            };
            btnNew.Click += (_, _) => _parent.LoadNewTicket();
            Controls.Add(btnNew);
        }

        // Grid
        _grid = new DataGridView
        {
            Location = new Point(30, 100),
            Anchor = AnchorStyles.Top | AnchorStyles.Left |
                                    AnchorStyles.Right | AnchorStyles.Bottom,
            Size = new Size(900, 500),
            BackgroundColor = Theme.Surface,
            ForeColor = Theme.TextPrimary,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = Theme.BorderColor,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = Theme.FontBody,
            ColumnHeadersHeight = 40,
            Cursor = Cursors.Hand
        };
        _grid.RowTemplate.Height = 44;

        _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Theme.SurfaceLight,
            ForeColor = Theme.TextSecondary,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            SelectionBackColor = Theme.SurfaceLight,
            SelectionForeColor = Theme.TextSecondary,
            Padding = new Padding(8, 0, 0, 0)
        };
        _grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Theme.Surface,
            ForeColor = Theme.TextPrimary,
            SelectionBackColor = Color.FromArgb(30, Theme.Primary),
            SelectionForeColor = Theme.TextPrimary,
            Padding = new Padding(8, 0, 0, 0)
        };
        _grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Theme.SurfaceLight,
            ForeColor = Theme.TextPrimary,
            SelectionBackColor = Color.FromArgb(30, Theme.Primary),
            SelectionForeColor = Theme.TextPrimary,
            Padding = new Padding(8, 0, 0, 0)
        };

        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "TicketNumber", HeaderText = "Ticket #", FillWeight = 15 },
            new DataGridViewTextBoxColumn { Name = "Subject", HeaderText = "Subject", FillWeight = 30 },
            new DataGridViewTextBoxColumn { Name = "Priority", HeaderText = "Priority", FillWeight = 11 },
            new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", FillWeight = 13 },
            new DataGridViewTextBoxColumn { Name = "CreatedDate", HeaderText = "Created", FillWeight = 15 },
            new DataGridViewTextBoxColumn { Name = "AssignedTo", HeaderText = "Assigned To", FillWeight = 13 }
        );

        if (AppSession.IsAdmin)
        {
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CreatedBy",
                HeaderText = "Created By",
                FillWeight = 13
            });
        }

        _grid.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "View",
            HeaderText = "",
            Text = "View Details",
            UseColumnTextForButtonValue = true,
            FillWeight = 12,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Theme.Primary,
                ForeColor = Theme.Background,
                Font = Theme.FontSmall,
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });

        _grid.CellClick += Grid_CellClick;
        _grid.CellFormatting += Grid_CellFormatting;
        Controls.Add(_grid);

        _lblStatus = new Label
        {
            Text = "Loading tickets...",
            Location = new Point(24, 300),
            Size = new Size(400, 30),
            ForeColor = Theme.TextSecondary,
            Font = Theme.FontBody
        };
        Controls.Add(_lblStatus);
    }

    private async Task LoadTicketsAsync()
    {
        _btnRefresh.Enabled = false;
        _lblStatus.Text = "Loading...";
        _lblStatus.Visible = true;
        _grid.Rows.Clear();

        var result = await ApiClient.GetTickets();
        if (result.Success && result.Data != null)
        {
            _allTickets = result.Data;
            PopulateGrid(_allTickets);
            _lblStatus.Visible = _allTickets.Count == 0;
            _lblStatus.Text = "No tickets found.";
        }
        else
        {
            _lblStatus.ForeColor = Theme.Danger;
            _lblStatus.Text = $"Error: {result.Message}";
        }
        _btnRefresh.Enabled = true;
    }

    private void FilterGrid()
    {
        var q = _txtSearch.Text.Trim().ToLower();
        var filtered = string.IsNullOrEmpty(q)
            ? _allTickets
            : _allTickets.Where(t =>
                t.TicketNumber.ToLower().Contains(q) ||
                t.Subject.ToLower().Contains(q) ||
                t.Status.ToLower().Contains(q)).ToList();
        PopulateGrid(filtered);
    }

    private void PopulateGrid(List<Ticket> tickets)
    {
        _grid.Rows.Clear();
        foreach (var t in tickets)
        {
            var values = new List<object>
            {
                t.TicketNumber,
                t.Subject.Length > 55 ? t.Subject[..55] + "…" : t.Subject,
                t.Priority,
                t.Status == "InProgress" ? "In Progress" : t.Status,
                t.CreatedAt.ToLocalTime().ToString("dd MMM yyyy"),
                t.AssignedToName ?? "—"
            };

            if (AppSession.IsAdmin)
                values.Add(t.CreatedByName ?? "");

            var rowIndex = _grid.Rows.Add(values.ToArray());
            _grid.Rows[rowIndex].Tag = t.Id;
        }
    }

    private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0) return;
        var colName = _grid.Columns[e.ColumnIndex].Name;
        var val = e.Value?.ToString() ?? "";

        if (colName == "Priority")
        {
            e.CellStyle.ForeColor = Theme.PriorityColor(val);
            e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        }
        else if (colName == "Status")
        {
            var raw = val == "In Progress" ? "InProgress" : val;
            e.CellStyle.ForeColor = Theme.StatusColor(raw);
            e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        }
    }

    private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        var id = (int)_grid.Rows[e.RowIndex].Tag!;
        _parent.LoadTicketDetail(id);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (_grid != null)
            _grid.Size = new Size(Width - 60, Height - 120);
    }
}
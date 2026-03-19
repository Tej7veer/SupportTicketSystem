using SupportTicketDesktop.Models;
using SupportTicketDesktop.Services;
using SupportTicketDesktop.UI;

namespace SupportTicketDesktop.Forms;

public class TicketDetailForm : Form
{
    private readonly MainForm _parent;
    private readonly int _ticketId;
    private Ticket? _ticket;
    private DarkComboBox _cmbAssign = null!;
    private DarkComboBox _cmbStatus = null!;
    private TextBox _txtRemark = null!;
    private TextBox _txtComment = null!;
    private CheckBox _chkInternal = null!;
    private FlatButton _btnAssign = null!;
    private FlatButton _btnStatus = null!;
    private FlatButton _btnComment = null!;
    private Label _lblLoading = null!;
    private List<User> _admins = new();
    private Panel _scrollPanel = null!;

    public TicketDetailForm(MainForm parent, int ticketId)
    {
        _parent = parent;
        _ticketId = ticketId;
        InitializeComponent();
        _ = LoadDetailAsync();
    }

    private void InitializeComponent()
    {
        BackColor = Theme.Background;
        AutoScroll = true;

        var btnBack = new Button
        {
            Text = "← Back",
            Location = new Point(24, 20),
            Size = new Size(100, 32),
            Font = Theme.FontSmall,
            FlatStyle = FlatStyle.Flat,
            BackColor = Theme.SurfaceLight,
            ForeColor = Theme.TextPrimary,
            Cursor = Cursors.Hand
        };
        btnBack.FlatAppearance.BorderColor = Theme.BorderColor;
        btnBack.FlatAppearance.BorderSize = 1;
        btnBack.FlatAppearance.MouseOverBackColor = Theme.BorderColor;
        btnBack.FlatAppearance.MouseDownBackColor = Theme.TextMuted;
        btnBack.Click += (_, _) => _parent.LoadTicketList();
        Controls.Add(btnBack);

        Controls.Add(new Label
        {
            Text = "Ticket Details",
            Location = new Point(140, 18),
            Size = new Size(400, 34),
            Font = Theme.FontTitle,
            ForeColor = Theme.TextPrimary
        });

        _lblLoading = new Label
        {
            Text = "Loading ticket details...",
            Location = new Point(24, 80),
            Size = new Size(600, 30),
            ForeColor = Theme.TextSecondary,
            Font = Theme.FontBody
        };
        Controls.Add(_lblLoading);

        _scrollPanel = new Panel
        {
            Location = new Point(0, 70),
            AutoScroll = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Left |
                         AnchorStyles.Right | AnchorStyles.Bottom
        };
        Controls.Add(_scrollPanel);
    }

    private async Task LoadDetailAsync()
    {
        // Clear previous content
        _scrollPanel.Controls.Clear();

        var result = await ApiClient.GetTicketDetail(_ticketId);
        if (!result.Success || result.Data == null)
        {
            _lblLoading.ForeColor = Theme.Danger;
            _lblLoading.Text = $"Error: {result.Message}";
            return;
        }

        _ticket = result.Data.Ticket;

        if (AppSession.IsAdmin)
        {
            var adminsResult = await ApiClient.GetAdmins();
            _admins = adminsResult.Data ?? new List<User>();
        }

        _lblLoading.Visible = false;
        BuildDetailUI(result.Data);
    }

    private void BuildDetailUI(TicketDetailResponse data)
    {
        var t = data.Ticket;
        int y = 10;
        int cw = Width - 80;

        // ── Info Card ──────────────────────────────────────────────
        var infoCard = new CardPanel
        {
            Location = new Point(24, y),
            Size = new Size(cw, 200)
        };
        _scrollPanel.Controls.Add(infoCard);

        infoCard.Controls.Add(new Label
        {
            Text = t.TicketNumber,
            Location = new Point(20, 14),
            AutoSize = true,
            Font = new Font("Consolas", 10f),
            ForeColor = Theme.Primary
        });

        infoCard.Controls.Add(new Label
        {
            Text = t.Subject,
            Location = new Point(20, 34),
            Size = new Size(cw - 40, 28),
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = Theme.TextPrimary
        });

        // Badges
        var priBadge = new BadgeLabel
        {
            Text = t.Priority,
            BadgeColor = Theme.PriorityColor(t.Priority),
            Location = new Point(20, 70),
            Width = 80
        };
        infoCard.Controls.Add(priBadge);

        var stText = t.Status == "InProgress" ? "In Progress" : t.Status;
        var stBadge = new BadgeLabel
        {
            Text = stText,
            BadgeColor = Theme.StatusColor(t.Status),
            Location = new Point(110, 70),
            Width = 100
        };
        infoCard.Controls.Add(stBadge);

        // Meta
        int mx = 20;
        AddMeta(infoCard, "Created By", t.CreatedByName ?? "—", ref mx, 104);
        AddMeta(infoCard, "Assigned To", t.AssignedToName ?? "Unassigned", ref mx, 104);
        AddMeta(infoCard, "Created",
            t.CreatedAt.ToLocalTime().ToString("dd MMM yyyy, HH:mm"), ref mx, 104);

        // Description
        infoCard.Controls.Add(new Label
        {
            Text = "DESCRIPTION",
            Location = new Point(20, 148),
            AutoSize = true,
            Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            ForeColor = Theme.TextMuted
        });
        infoCard.Controls.Add(new TextBox
        {
            Location = new Point(20, 164),
            Size = new Size(cw - 40, 26),
            ReadOnly = true,
            BackColor = Theme.SurfaceLight,
            ForeColor = Theme.TextSecondary,
            BorderStyle = BorderStyle.None,
            Font = Theme.FontBody,
            Text = t.Description
        });

        y += 216;

        // ── Admin Actions ──────────────────────────────────────────
        if (AppSession.IsAdmin && t.Status != "Closed")
        {
            var adminCard = new CardPanel
            {
                Location = new Point(24, y),
                Size = new Size(cw, 170)
            };
            _scrollPanel.Controls.Add(adminCard);

            adminCard.Controls.Add(new Label
            {
                Text = "⚙  Admin Actions",
                Location = new Point(20, 14),
                AutoSize = true,
                Font = Theme.FontHeading,
                ForeColor = Theme.Warning
            });

            // Assign
            adminCard.Controls.Add(new Label
            {
                Text = "ASSIGN TO",
                Location = new Point(20, 48),
                AutoSize = true,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Theme.TextMuted
            });
            _cmbAssign = new DarkComboBox
            {
                Location = new Point(20, 66),
                Size = new Size(200, 30)
            };
            foreach (var admin in _admins)
                _cmbAssign.Items.Add($"{admin.FullName} ({admin.Username})");
            if (t.AssignedToUserId.HasValue)
            {
                var idx = _admins.FindIndex(a => a.Id == t.AssignedToUserId.Value);
                if (idx >= 0) _cmbAssign.SelectedIndex = idx;
            }
            adminCard.Controls.Add(_cmbAssign);

            _btnAssign = new FlatButton
            {
                Text = "Assign",
                Location = new Point(232, 66),
                Size = new Size(90, 30),
                AccentColor = Theme.Warning,
                Font = Theme.FontSmall
            };
            _btnAssign.Click += BtnAssign_Click;
            adminCard.Controls.Add(_btnAssign);

            // Status
            adminCard.Controls.Add(new Label
            {
                Text = "CHANGE STATUS",
                Location = new Point(360, 48),
                AutoSize = true,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Theme.TextMuted
            });
            _cmbStatus = new DarkComboBox
            {
                Location = new Point(360, 66),
                Size = new Size(160, 30)
            };
            if (t.Status == "Open") _cmbStatus.Items.Add("InProgress");
            if (t.Status == "InProgress") _cmbStatus.Items.Add("Closed");
            if (_cmbStatus.Items.Count > 0) _cmbStatus.SelectedIndex = 0;
            adminCard.Controls.Add(_cmbStatus);

            _btnStatus = new FlatButton
            {
                Text = "Update",
                Location = new Point(532, 66),
                Size = new Size(90, 30),
                AccentColor = Theme.Info,
                Font = Theme.FontSmall
            };
            _btnStatus.Click += BtnUpdateStatus_Click;
            adminCard.Controls.Add(_btnStatus);

            // Remarks
            adminCard.Controls.Add(new Label
            {
                Text = "REMARKS (optional)",
                Location = new Point(20, 108),
                AutoSize = true,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Theme.TextMuted
            });
            _txtRemark = new TextBox
            {
                Location = new Point(20, 124),
                Size = new Size(580, 28),
                BackColor = Theme.SurfaceLight,
                ForeColor = Theme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = Theme.FontBody,
                PlaceholderText = "Optional remarks..."
            };
            adminCard.Controls.Add(_txtRemark);

            y += 186;
        }

        // ── Add Comment ────────────────────────────────────────────
        if (t.Status != "Closed")
        {
            var commentCard = new CardPanel
            {
                Location = new Point(24, y),
                Size = new Size(cw, 150)
            };
            _scrollPanel.Controls.Add(commentCard);

            commentCard.Controls.Add(new Label
            {
                Text = "💬  Add Comment",
                Location = new Point(20, 14),
                AutoSize = true,
                Font = Theme.FontHeading,
                ForeColor = Theme.TextPrimary
            });

            _txtComment = new TextBox
            {
                Location = new Point(20, 44),
                Size = new Size(580, 60),
                Multiline = true,
                BackColor = Theme.SurfaceLight,
                ForeColor = Theme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = Theme.FontBody,
                PlaceholderText = "Write your comment here..."
            };
            commentCard.Controls.Add(_txtComment);

            if (AppSession.IsAdmin)
            {
                _chkInternal = new CheckBox
                {
                    Text = "Internal (admin only)",
                    Location = new Point(616, 44),
                    AutoSize = true,
                    ForeColor = Theme.TextSecondary,
                    Font = Theme.FontSmall
                };
                commentCard.Controls.Add(_chkInternal);
            }

            _btnComment = new FlatButton
            {
                Text = "Post Comment",
                Location = new Point(20, 112),
                Size = new Size(130, 30),
                Font = Theme.FontSmall
            };
            _btnComment.Click += BtnComment_Click;
            commentCard.Controls.Add(_btnComment);

            y += 166;
        }

        // ── Activity Timeline ──────────────────────────────────────
        var allEntries = new List<(DateTime dt, string by, string text, Color color)>();

        foreach (var h in data.History)
        {
            var txt = h.OldStatus != null
                ? $"Status: {h.OldStatus} → {h.NewStatus}"
                : "Ticket created";
            if (!string.IsNullOrEmpty(h.Remarks))
                txt += $" — {h.Remarks}";
            allEntries.Add((h.ChangedAt, h.ChangedByName ?? "System", txt, Theme.Info));
        }
        foreach (var c in data.Comments)
        {
            var prefix = c.IsInternal ? "[Internal] " : "";
            allEntries.Add((c.CreatedAt, c.CreatedByName ?? "",
                prefix + c.CommentText,
                c.IsInternal ? Theme.Warning : Theme.TextPrimary));
        }

        allEntries = allEntries.OrderByDescending(e => e.dt).ToList();

        int timelineHeight = Math.Max(120, allEntries.Count * 70 + 60);
        var timelineCard = new CardPanel
        {
            Location = new Point(24, y),
            Size = new Size(cw, timelineHeight)
        };
        _scrollPanel.Controls.Add(timelineCard);

        timelineCard.Controls.Add(new Label
        {
            Text = "📋  Activity Timeline",
            Location = new Point(20, 14),
            AutoSize = true,
            Font = Theme.FontHeading,
            ForeColor = Theme.TextPrimary
        });

        if (allEntries.Count == 0)
        {
            timelineCard.Controls.Add(new Label
            {
                Text = "No activity recorded yet.",
                Location = new Point(20, 46),
                AutoSize = true,
                ForeColor = Theme.TextMuted,
                Font = Theme.FontBody
            });
        }
        else
        {
            int ey = 46;
            foreach (var entry in allEntries)
            {
                var dot = new Panel
                {
                    Location = new Point(20, ey + 8),
                    Size = new Size(10, 10),
                    BackColor = entry.color
                };
                timelineCard.Controls.Add(dot);

                timelineCard.Controls.Add(new Label
                {
                    Text = entry.text,
                    Location = new Point(40, ey),
                    Size = new Size(cw - 220, 32),
                    Font = Theme.FontBody,
                    ForeColor = entry.color
                });
                timelineCard.Controls.Add(new Label
                {
                    Text = $"by {entry.by}",
                    Location = new Point(40, ey + 34),
                    AutoSize = true,
                    Font = Theme.FontSmall,
                    ForeColor = Theme.TextMuted
                });
                timelineCard.Controls.Add(new Label
                {
                    Text = entry.dt.ToLocalTime().ToString("dd MMM yyyy, HH:mm"),
                    Location = new Point(cw - 200, ey),
                    Size = new Size(180, 20),
                    Font = Theme.FontSmall,
                    ForeColor = Theme.TextSecondary,
                    TextAlign = ContentAlignment.TopRight
                });
                ey += 68;
            }
        }

        y += timelineHeight + 16;
        _scrollPanel.Size = new Size(Width - 40, Height - 70);
    }

    private static void AddMeta(Control parent, string key, string value, ref int x, int y)
    {
        parent.Controls.Add(new Label
        {
            Text = key,
            Location = new Point(x, y),
            AutoSize = true,
            Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            ForeColor = Theme.TextMuted
        });
        parent.Controls.Add(new Label
        {
            Text = value,
            Location = new Point(x, y + 16),
            AutoSize = true,
            Font = Theme.FontBody,
            ForeColor = Theme.TextPrimary
        });
        x += 180;
    }

    private async void BtnAssign_Click(object? sender, EventArgs e)
    {
        if (_cmbAssign.SelectedIndex < 0)
        { MessageBox.Show("Please select an admin.", "Warning"); return; }

        var admin = _admins[_cmbAssign.SelectedIndex];
        _btnAssign.Enabled = false;
        var result = await ApiClient.AssignTicket(_ticketId, admin.Id);
        if (result.Success)
        {
            MessageBox.Show($"Assigned to {admin.FullName}.", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            _ = LoadDetailAsync();
        }
        else
        {
            MessageBox.Show(result.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _btnAssign.Enabled = true;
        }
    }

    private async void BtnUpdateStatus_Click(object? sender, EventArgs e)
    {
        if (_cmbStatus.SelectedIndex < 0)
        { MessageBox.Show("Please select a status.", "Warning"); return; }

        var newStatus = _cmbStatus.SelectedItem!.ToString()!;
        var remark = _txtRemark?.Text.Trim();
        _btnStatus.Enabled = false;
        var result = await ApiClient.UpdateStatus(_ticketId, newStatus, remark);
        if (result.Success)
        {
            MessageBox.Show($"Status updated to {newStatus}.", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            _ = LoadDetailAsync();
        }
        else
        {
            MessageBox.Show(result.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _btnStatus.Enabled = true;
        }
    }

    private async void BtnComment_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtComment.Text))
        { MessageBox.Show("Comment cannot be empty.", "Warning"); return; }

        var isInternal = AppSession.IsAdmin && (_chkInternal?.Checked ?? false);
        _btnComment.Enabled = false;
        var result = await ApiClient.AddComment(
            _ticketId, _txtComment.Text.Trim(), isInternal);

        if (result.Success)
        {
            _txtComment.Clear();
            MessageBox.Show("Comment posted.", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            _ = LoadDetailAsync();
        }
        else
        {
            MessageBox.Show(result.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _btnComment.Enabled = true;
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (_scrollPanel != null)
            _scrollPanel.Size = new Size(Width - 40, Height - 70);
    }
}

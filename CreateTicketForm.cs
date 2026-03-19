using SupportTicketDesktop.Services;
using SupportTicketDesktop.UI;

namespace SupportTicketDesktop.Forms;

public class CreateTicketForm : Form
{
    private readonly MainForm _parent;
    private TextBox _txtSubject = null!;
    private TextBox _txtDescription = null!;
    private DarkComboBox _cmbPriority = null!;
    private FlatButton _btnSubmit = null!;
    private Label _lblStatus = null!;

    public CreateTicketForm(MainForm parent)
    {
        _parent = parent;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        BackColor = Theme.Background;
        int cx = 30, y = 30;

        Controls.Add(new Label
        {
            Text = "Create New Ticket",
            Location = new Point(cx, y),
            Size = new Size(500, 36),
            Font = Theme.FontTitle,
            ForeColor = Theme.TextPrimary
        });
        y += 42;

        Controls.Add(new Label
        {
            Text = "Describe your issue and we'll get back to you soon.",
            Location = new Point(cx, y),
            Size = new Size(500, 20),
            Font = Theme.FontSmall,
            ForeColor = Theme.TextSecondary
        });
        y += 40;

        var card = new CardPanel
        {
            Location = new Point(cx, y),
            Size = new Size(860, 420)
        };
        Controls.Add(card);

        int fy = 20;

        AddLabel(card, "SUBJECT *", new Point(20, fy));
        fy += 22;
        _txtSubject = new DarkTextBox
        {
            Location = new Point(20, fy),
            Size = new Size(660, 34),
            PlaceholderText = "Brief description of your issue"
        };
        card.Controls.Add(_txtSubject);
        fy += 50;

        AddLabel(card, "PRIORITY *", new Point(20, fy));
        fy += 22;
        _cmbPriority = new DarkComboBox
        {
            Location = new Point(20, fy),
            Size = new Size(200, 32)
        };
        _cmbPriority.Items.AddRange(new object[] { "Low", "Medium", "High" });
        _cmbPriority.SelectedIndex = 1;
        card.Controls.Add(_cmbPriority);
        fy += 50;

        // Priority hints
        card.Controls.Add(new Label
        {
            Text = "● Low: Minor issues, general questions",
            Location = new Point(20, fy),
            AutoSize = true,
            Font = Theme.FontSmall,
            ForeColor = Theme.Success
        });
        fy += 18;
        card.Controls.Add(new Label
        {
            Text = "● Medium: Moderate impact, workaround exists",
            Location = new Point(20, fy),
            AutoSize = true,
            Font = Theme.FontSmall,
            ForeColor = Theme.Warning
        });
        fy += 18;
        card.Controls.Add(new Label
        {
            Text = "● High: Critical issue, business impact",
            Location = new Point(20, fy),
            AutoSize = true,
            Font = Theme.FontSmall,
            ForeColor = Theme.Danger
        });
        fy += 30;

        AddLabel(card, "DESCRIPTION *", new Point(20, fy));
        fy += 22;
        _txtDescription = new TextBox
        {
            Location = new Point(20, fy),
            Size = new Size(660, 110),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Theme.SurfaceLight,
            ForeColor = Theme.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle,
            Font = Theme.FontBody,
            PlaceholderText = "Describe your issue in detail..."
        };
        card.Controls.Add(_txtDescription);
        fy += 124;

        _btnSubmit = new FlatButton
        {
            Text = "Submit Ticket",
            Location = new Point(20, fy),
            Size = new Size(150, 40)
        };
        _btnSubmit.Click += BtnSubmit_Click;
        card.Controls.Add(_btnSubmit);

        var btnCancel = new FlatButton
        {
            Text = "Cancel",
            Location = new Point(182, fy),
            Size = new Size(100, 40),
            IsOutline = true,
            AccentColor = Theme.TextSecondary
        };
        btnCancel.Click += (_, _) => _parent.LoadTicketList();
        card.Controls.Add(btnCancel);

        _lblStatus = new Label
        {
            Location = new Point(20, fy + 46),
            Size = new Size(660, 24),
            Font = Theme.FontSmall,
            ForeColor = Theme.Danger
        };
        card.Controls.Add(_lblStatus);
    }

    private static void AddLabel(Control parent, string text, Point loc)
    {
        parent.Controls.Add(new Label
        {
            Text = text,
            Location = loc,
            AutoSize = true,
            Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            ForeColor = Theme.TextMuted
        });
    }

    private async void BtnSubmit_Click(object? sender, EventArgs e)
    {
        _lblStatus.Text = "";

        if (string.IsNullOrWhiteSpace(_txtSubject.Text))
        { _lblStatus.Text = "✗ Subject is required."; return; }

        if (string.IsNullOrWhiteSpace(_txtDescription.Text))
        { _lblStatus.Text = "✗ Description is required."; return; }

        _btnSubmit.Enabled = false;
        _btnSubmit.Text = "Submitting...";

        var result = await ApiClient.CreateTicket(
            _txtSubject.Text.Trim(),
            _txtDescription.Text.Trim(),
            _cmbPriority.SelectedItem?.ToString() ?? "Medium");

        if (result.Success && result.Data != null)
        {
            MessageBox.Show(
                $"Ticket created!\n\nTicket #: {result.Data.TicketNumber}",
                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _parent.LoadTicketList();
        }
        else
        {
            _lblStatus.Text = $"✗ {result.Message}";
            _btnSubmit.Enabled = true;
            _btnSubmit.Text = "Submit Ticket";
        }
    }
}
using SupportTicketDesktop.Models;
using SupportTicketDesktop.Services;
using SupportTicketDesktop.UI;

namespace SupportTicketDesktop.Forms;

public class LoginForm : Form
{
    private TextBox _txtUsername = null!;
    private TextBox _txtPassword = null!;
    private FlatButton _btnLogin = null!;
    private Label _lblStatus = null!;
    private bool _loading;

    public LoginForm()
    {
        InitializeComponent();
        Theme.ApplyToForm(this);
    }

    private void InitializeComponent()
    {
        Text = "Support Ticket System — Login";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        WindowState = FormWindowState.Maximized;

        // ── Full background panel ─────────────────────────────────
        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Background
        };
        Controls.Add(mainPanel);

        // ── Accent line (add FIRST — docks to top first) ──────────
        var accentLine = new Panel
        {
            Dock = DockStyle.Top,
            Height = 4,
            BackColor = Theme.Primary
        };
        mainPanel.Controls.Add(accentLine);

        // ── Logo panel (add SECOND — sits above accent) ───────────
        var logoPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 120,
            BackColor = Theme.Surface
        };
        mainPanel.Controls.Add(logoPanel);

        // App name — docked to bottom of logo panel
        var lblAppName = new Label
        {
            Text = "SUPPORT DESK",
            Dock = DockStyle.Bottom,
            Height = 45,
            Font = new Font("Segoe UI", 15f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Theme.TextPrimary,
            BackColor = Theme.Surface
        };
        logoPanel.Controls.Add(lblAppName);

        // Icon — fills remaining space, centered
        var lblIcon = new Label
        {
            Text = "🎫",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Emoji", 42f),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Theme.Primary,
            BackColor = Theme.Surface
        };
        logoPanel.Controls.Add(lblIcon);

        // ── Footer hint — docked to bottom ───────────────────────
        var hint = new Label
        {
            Text = "admin / admin123   |   alice / user123",
            Dock = DockStyle.Bottom,
            Height = 30,
            Font = Theme.FontSmall,
            ForeColor = Theme.TextMuted,
            TextAlign = ContentAlignment.MiddleCenter
        };
        mainPanel.Controls.Add(hint);

        // ── Login card — centered on resize ──────────────────────
        var card = new CardPanel
        {
            Size = new Size(440, 370)
        };
        mainPanel.Controls.Add(card);

        // Center card on load and resize
        this.Load += (_, _) =>
        {
            card.Location = new Point(
                (mainPanel.ClientSize.Width - card.Width) / 2,
                130 + (mainPanel.ClientSize.Height - 130 - card.Height) / 2
            );
        };
        this.Resize += (_, _) =>
        {
            card.Location = new Point(
                (mainPanel.ClientSize.Width - card.Width) / 2,
                130 + (mainPanel.ClientSize.Height - 130 - card.Height) / 2
            );
        };

        int y = 24;

        // Sign In title
        card.Controls.Add(new Label
        {
            Text = "Sign In",
            Location = new Point(0, y),
            Size = new Size(440, 36),
            Font = Theme.FontHeading,
            ForeColor = Theme.TextPrimary,
            TextAlign = ContentAlignment.MiddleCenter
        });
        y += 48;

        // Username
        AddFieldLabel(card, "USERNAME", ref y);
        _txtUsername = new DarkTextBox
        {
            Location = new Point(20, y),
            Size = new Size(400, 36),
            PlaceholderText = "Enter your username",
            Font = Theme.FontBody
        };
        card.Controls.Add(_txtUsername);
        y += 50;

        // Password
        AddFieldLabel(card, "PASSWORD", ref y);
        _txtPassword = new DarkTextBox
        {
            Location = new Point(20, y),
            Size = new Size(400, 36),
            UseSystemPasswordChar = true,
            PlaceholderText = "Enter your password",
            Font = Theme.FontBody
        };
        card.Controls.Add(_txtPassword);
        y += 54;

        // Sign In button
        _btnLogin = new FlatButton
        {
            Text = "Sign In",
            Location = new Point(20, y),
            Size = new Size(400, 48),
            Font = new Font("Segoe UI", 12f, FontStyle.Bold)
        };
        _btnLogin.Click += BtnLogin_Click;
        card.Controls.Add(_btnLogin);
        y += 58;

        // Status label
        _lblStatus = new Label
        {
            Location = new Point(20, y),
            Size = new Size(400, 40),
            ForeColor = Theme.Danger,
            Font = Theme.FontSmall,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = ""
        };
        card.Controls.Add(_lblStatus);

        AcceptButton = _btnLogin;
    }

    private static void AddFieldLabel(Control parent, string text, ref int y)
    {
        parent.Controls.Add(new Label
        {
            Text = text,
            Location = new Point(20, y),
            Size = new Size(380, 18),
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            ForeColor = Theme.TextMuted
        });
        y += 20;
    }

    private async void BtnLogin_Click(object? sender, EventArgs e)
    {
        if (_loading) return;

        var username = _txtUsername.Text.Trim();
        var password = _txtPassword.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            _lblStatus.ForeColor = Theme.Warning;
            _lblStatus.Text = "⚠ Please enter username and password.";
            return;
        }

        _loading = true;
        _btnLogin.Enabled = false;
        _btnLogin.Text = "Signing in...";
        _lblStatus.Text = "";

        try
        {
            var result = await ApiClient.Login(username, password);
            if (result.Success && result.Data != null)
            {
                var data = result.Data;
                AppSession.Token = data.Token;
                AppSession.UserId = data.UserId;
                AppSession.Username = data.Username;
                AppSession.FullName = data.FullName;
                AppSession.Role = data.Role;
                ApiClient.SetToken(data.Token);

                var mainForm = new MainForm();
                mainForm.Show();
                Hide();
            }
            else
            {
                _lblStatus.ForeColor = Theme.Danger;
                _lblStatus.Text = $"✗ {result.Message}";
            }
        }
        finally
        {
            _loading = false;
            _btnLogin.Enabled = true;
            _btnLogin.Text = "Sign In";
        }
    }
}
using SupportTicketDesktop.Models;
using SupportTicketDesktop.UI;

namespace SupportTicketDesktop.Forms;

public class MainForm : Form
{
    private Panel _sidebar = null!;
    private Panel _contentArea = null!;
    private NavButton _btnTickets = null!;
    private NavButton? _btnNew = null;
    private NavButton _btnLogout = null!;
    private Form? _currentChild;

    public MainForm()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint, true);
        InitializeComponent();
        Theme.ApplyToForm(this);
        LoadTicketList();
    }

    private void InitializeComponent()
    {
        Text = $"Support Desk — {AppSession.FullName}";
        Size = new Size(1280, 800);
        MinimumSize = new Size(1100, 700);
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;

        // 1. Content area (Fill — added first)
        _contentArea = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Background
        };
        SetDoubleBuffer(_contentArea);
        Controls.Add(_contentArea);

        // 2. Border line (Left)
        Controls.Add(new Panel
        {
            Dock = DockStyle.Left,
            Width = 1,
            BackColor = Theme.BorderColor
        });

        // 3. Sidebar (Left — added last so renders leftmost)
        _sidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 220,
            BackColor = Theme.Surface
        };
        SetDoubleBuffer(_sidebar);
        Controls.Add(_sidebar);

        // Brand
        _sidebar.Controls.Add(new Label
        {
            Text = "🎫  Support Desk",
            Location = new Point(0, 0),
            Size = new Size(220, 70),
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = Theme.Primary,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Theme.Surface
        });

        // Top separator
        _sidebar.Controls.Add(new Panel
        {
            Location = new Point(0, 70),
            Size = new Size(220, 1),
            BackColor = Theme.BorderColor
        });

        // Nav buttons
        int navY = 80;
        _btnTickets = CreateNav("📋  All Tickets", navY);
        navY += 46;

        if (!AppSession.IsAdmin)
        {
            _btnNew = CreateNav("➕  New Ticket", navY);
            navY += 46;
        }

        // Middle separator
        _sidebar.Controls.Add(new Panel
        {
            Location = new Point(0, navY + 8),
            Size = new Size(220, 1),
            BackColor = Theme.BorderColor
        });

        // Sign out button
        _btnLogout = CreateNav("⎋  Sign Out", navY + 18);
        _btnLogout.ForeColor = Theme.Danger;

        // User info panel at bottom
        var userPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 72,
            BackColor = Theme.SurfaceLight
        };
        _sidebar.Controls.Add(userPanel);

        userPanel.Controls.Add(new Panel
        {
            Dock = DockStyle.Top,
            Height = 1,
            BackColor = Theme.BorderColor
        });

        userPanel.Controls.Add(new Label
        {
            Text = AppSession.FullName.Length > 0
                        ? AppSession.FullName[0].ToString().ToUpper() : "?",
            Location = new Point(12, 16),
            Size = new Size(40, 40),
            Font = new Font("Segoe UI", 15f, FontStyle.Bold),
            ForeColor = Theme.Background,
            BackColor = Theme.Primary,
            TextAlign = ContentAlignment.MiddleCenter
        });

        userPanel.Controls.Add(new Label
        {
            Text = AppSession.FullName,
            Location = new Point(60, 18),
            Size = new Size(152, 20),
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            BackColor = Theme.SurfaceLight
        });

        userPanel.Controls.Add(new Label
        {
            Text = AppSession.Role,
            Location = new Point(60, 40),
            Size = new Size(100, 16),
            Font = Theme.FontSmall,
            ForeColor = AppSession.IsAdmin ? Theme.Warning : Theme.Info,
            BackColor = Theme.SurfaceLight
        });

        // Events
        _btnTickets.Click += (_, _) => LoadTicketList();
        if (_btnNew != null)
            _btnNew.Click += (_, _) => LoadNewTicket();
        _btnLogout.Click += BtnLogout_Click;
    }

    // Helper to enable double buffering on any control
    private static void SetDoubleBuffer(Control ctrl)
    {
        typeof(Control).GetProperty("DoubleBuffered",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance)!
            .SetValue(ctrl, true);
    }

    private NavButton CreateNav(string text, int y)
    {
        var btn = new NavButton
        {
            Text = text,
            Location = new Point(0, y),
            Size = new Size(220, 44)
        };
        _sidebar.Controls.Add(btn);
        return btn;
    }

    private void SetActiveNav(NavButton active)
    {
        _btnTickets.IsSelected = (active == _btnTickets);
        _btnTickets.Invalidate();
        if (_btnNew != null)
        {
            _btnNew.IsSelected = (active == _btnNew);
            _btnNew.Invalidate();
        }
    }

    public void LoadTicketList()
    {
        SetActiveNav(_btnTickets);
        LoadContent(new TicketListForm(this));
    }

    public void LoadNewTicket()
    {
        if (_btnNew != null) SetActiveNav(_btnNew);
        LoadContent(new CreateTicketForm(this));
    }

    public void LoadTicketDetail(int ticketId)
    {
        SetActiveNav(_btnTickets);
        LoadContent(new TicketDetailForm(this, ticketId));
    }

    private void LoadContent(Form childForm)
    {
        _contentArea.SuspendLayout();

        if (_currentChild != null)
        {
            _currentChild.Hide();
            _contentArea.Controls.Remove(_currentChild);
            _currentChild.Dispose();
            _currentChild = null;
        }

        _contentArea.Controls.Clear();

        _currentChild = childForm;
        childForm.TopLevel = false;
        childForm.FormBorderStyle = FormBorderStyle.None;
        childForm.Dock = DockStyle.Fill;
        childForm.BackColor = Theme.Background;

        SetDoubleBuffer(childForm);

        _contentArea.Controls.Add(childForm);
        _contentArea.ResumeLayout(true);
        childForm.Show();

        _contentArea.Invalidate(true);
        _contentArea.Update();
        _sidebar.Invalidate(true);
        _sidebar.Update();
    }

    private void BtnLogout_Click(object? sender, EventArgs e)
    {
        if (MessageBox.Show("Are you sure you want to sign out?", "Sign Out",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            AppSession.Clear();
            new LoginForm().Show();
            Close();
        }
    }
}
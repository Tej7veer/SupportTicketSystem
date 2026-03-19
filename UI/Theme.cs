namespace SupportTicketDesktop.UI;

public static class Theme
{
    public static readonly Color Background = Color.FromArgb(18, 22, 30);
    public static readonly Color Surface = Color.FromArgb(26, 32, 44);
    public static readonly Color SurfaceLight = Color.FromArgb(36, 44, 60);
    public static readonly Color BorderColor = Color.FromArgb(50, 62, 82);
    public static readonly Color Primary = Color.FromArgb(0, 188, 212);
    public static readonly Color PrimaryDark = Color.FromArgb(0, 151, 167);
    public static readonly Color PrimaryLight = Color.FromArgb(77, 208, 225);
    public static readonly Color TextPrimary = Color.FromArgb(236, 239, 244);
    public static readonly Color TextSecondary = Color.FromArgb(140, 158, 180);
    public static readonly Color TextMuted = Color.FromArgb(80, 100, 130);
    public static readonly Color Success = Color.FromArgb(38, 166, 91);
    public static readonly Color Warning = Color.FromArgb(255, 171, 0);
    public static readonly Color Danger = Color.FromArgb(239, 68, 68);
    public static readonly Color Info = Color.FromArgb(99, 179, 237);

    public static Color PriorityColor(string priority) => priority switch
    {
        "High" => Danger,
        "Medium" => Warning,
        "Low" => Success,
        _ => TextSecondary
    };

    public static Color StatusColor(string status) => status switch
    {
        "Open" => Info,
        "InProgress" => Warning,
        "Closed" => TextMuted,
        _ => TextSecondary
    };

    public static readonly Font FontTitle = new("Segoe UI", 18f, FontStyle.Bold);
    public static readonly Font FontHeading = new("Segoe UI", 13f, FontStyle.Bold);
    public static readonly Font FontBody = new("Segoe UI", 10f);
    public static readonly Font FontSmall = new("Segoe UI", 8.5f);
    public static readonly Font FontBadge = new("Segoe UI", 8f, FontStyle.Bold);

    public const int CornerRadius = 8;
    public const int Padding = 16;

    public static void ApplyToForm(Form form)
    {
        form.BackColor = Background;
        form.ForeColor = TextPrimary;
        form.Font = FontBody;
    }
}

public class FlatButton : Button
{
    public Color AccentColor { get; set; } = Theme.Primary;
    public bool IsOutline { get; set; } = false;
    private bool _hovered, _pressed;

    public FlatButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Cursor = Cursors.Hand;
        Font = Theme.FontBody;
        Size = new Size(120, 38);
        UpdateColors();
    }

    private void UpdateColors()
    {
        if (IsOutline)
        {
            BackColor = Color.Transparent;
            ForeColor = AccentColor;
            FlatAppearance.BorderSize = 1;
            FlatAppearance.BorderColor = AccentColor;
        }
        else
        {
            BackColor = _pressed ? Theme.PrimaryDark
                      : _hovered ? Theme.PrimaryLight
                      : AccentColor;
            ForeColor = Theme.Background;
            FlatAppearance.BorderSize = 0;
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    { _hovered = true; UpdateColors(); Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e)
    { _hovered = false; UpdateColors(); Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e)
    { _pressed = true; UpdateColors(); Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e)
    { _pressed = false; UpdateColors(); Invalidate(); base.OnMouseUp(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var brush = new SolidBrush(BackColor);
        using var path = RoundedRect(ClientRectangle, Theme.CornerRadius);
        g.FillPath(brush, path);

        if (IsOutline)
        {
            using var pen = new Pen(AccentColor, 1.5f);
            g.DrawPath(pen, path);
        }

        var textSize = g.MeasureString(Text, Font);
        using var textBrush = new SolidBrush(ForeColor);
        g.DrawString(Text, Font, textBrush,
            (Width - textSize.Width) / 2,
            (Height - textSize.Height) / 2);
    }

    private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
        path.AddArc(r.Right - radius * 2, r.Y, radius * 2, radius * 2, 270, 90);
        path.AddArc(r.Right - radius * 2, r.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(r.X, r.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseFigure();
        return path;
    }
}

public class DarkTextBox : TextBox
{
    public DarkTextBox()
    {
        BackColor = Theme.SurfaceLight;
        ForeColor = Theme.TextPrimary;
        BorderStyle = BorderStyle.FixedSingle;
        Font = Theme.FontBody;
    }
}

public class DarkComboBox : ComboBox
{
    public DarkComboBox()
    {
        BackColor = Theme.SurfaceLight;
        ForeColor = Theme.TextPrimary;
        FlatStyle = FlatStyle.Flat;
        Font = Theme.FontBody;
        DrawMode = DrawMode.OwnerDrawFixed;
        DropDownStyle = ComboBoxStyle.DropDownList;
        ItemHeight = 24;
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0) return;
        var bg = e.State.HasFlag(DrawItemState.Selected) ? Theme.Primary : Theme.SurfaceLight;
        var fg = e.State.HasFlag(DrawItemState.Selected) ? Theme.Background : Theme.TextPrimary;
        using var brush = new SolidBrush(bg);
        e.Graphics.FillRectangle(brush, e.Bounds);
        using var textBrush = new SolidBrush(fg);
        e.Graphics.DrawString(Items[e.Index]?.ToString(), Font, textBrush,
            e.Bounds.X + 4, e.Bounds.Y + 3);
    }
}

public class CardPanel : Panel
{
    public CardPanel()
    {
        BackColor = Theme.Surface;
        Padding = new Padding(Theme.Padding);

        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint, true);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var pen = new Pen(Theme.BorderColor, 1f);
        using var path = RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), Theme.CornerRadius);
        g.DrawPath(pen, path);
    }

    private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
        path.AddArc(r.Right - radius * 2, r.Y, radius * 2, radius * 2, 270, 90);
        path.AddArc(r.Right - radius * 2, r.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(r.X, r.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseFigure();
        return path;
    }
}

public class BadgeLabel : Label
{
    private Color _badgeColor = Theme.Info;
    public Color BadgeColor
    {
        get => _badgeColor;
        set { _badgeColor = value; Invalidate(); }
    }

    public BadgeLabel()
    {
        AutoSize = false;
        Height = 22;
        Font = Theme.FontBadge;
        TextAlign = ContentAlignment.MiddleCenter;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var bg = new SolidBrush(Color.FromArgb(40, _badgeColor));
        using var path = RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 10);
        g.FillPath(bg, path);
        using var border = new Pen(Color.FromArgb(120, _badgeColor), 1f);
        g.DrawPath(border, path);
        using var textBrush = new SolidBrush(_badgeColor);
        var sf = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        g.DrawString(Text, Font, textBrush,
            new RectangleF(0, 0, Width, Height), sf);
    }

    private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
        path.AddArc(r.Right - radius * 2, r.Y, radius * 2, radius * 2, 270, 90);
        path.AddArc(r.Right - radius * 2, r.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(r.X, r.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseFigure();
        return path;
    }
}

public class NavButton : Button
{
    public bool IsSelected { get; set; }
    private bool _hovered;

    public NavButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseOverBackColor = Color.Transparent;
        FlatAppearance.MouseDownBackColor = Color.Transparent;
        TextAlign = ContentAlignment.MiddleLeft;
        Padding = new Padding(16, 0, 0, 0);
        Cursor = Cursors.Hand;
        Font = Theme.FontBody;
        Height = 44;
        ForeColor = Theme.TextSecondary;
        BackColor = Color.Transparent;

        // Enable double buffering
        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint, true);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _hovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hovered = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var rec = new Rectangle(0, 0, Width, Height);

        // Clear background first — this fixes the ghost
        using var clearBrush = new SolidBrush(Theme.Surface);
        g.FillRectangle(clearBrush, rec);

        // Hover / selected background
        if (IsSelected)
        {
            using var selBrush = new SolidBrush(Color.FromArgb(30, Theme.Primary));
            g.FillRectangle(selBrush, rec);

            // Left accent bar
            using var accentBrush = new SolidBrush(Theme.Primary);
            g.FillRectangle(accentBrush, new Rectangle(0, 0, 3, Height));
        }
        else if (_hovered)
        {
            using var hovBrush = new SolidBrush(Color.FromArgb(15, Theme.Primary));
            g.FillRectangle(hovBrush, rec);
        }

        // Text color
        var textColor = IsSelected ? Theme.Primary
                      : _hovered ? Theme.TextPrimary
                      : Theme.TextSecondary;

        using var textBrush = new SolidBrush(textColor);
        g.DrawString(Text, Font, textBrush, Padding.Left, (Height - Font.Height) / 2f);
    }
}
using System.Drawing.Drawing2D;

namespace SimuCity.Ui;

/// <summary>
/// A button with rounded corners. Windows Forms buttons are rectangular
/// by default, so we clip the control to a rounded rectangle using its
/// Region property. This is a simple, reliable trick that needs no
/// external libraries.
/// </summary>
public class RoundedButton : Button
{
    /// <summary>How round the corners are, in pixels.</summary>
    public int CornerRadius { get; set; } = 14;

    public RoundedButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;      // hide the default border
        BackColor = Color.FromArgb(242, 163, 60); // warm amber accent
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        Cursor = Cursors.Hand;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        using GraphicsPath path = CreateRoundedPath(ClientRectangle, CornerRadius);
        Region = new Region(path);           // clip everything to the rounded shape
    }

    /// <summary>Builds a rounded-rectangle path used by both controls below.</summary>
    internal static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
    {
        int diameter = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}

/// <summary>
/// A panel that looks like a white "card" with rounded corners.
/// Used for the dashboard statistics and the location buildings.
/// </summary>
public class CardPanel : Panel
{
    public int CornerRadius { get; set; } = 16;

    public CardPanel()
    {
        BackColor = Color.White;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        using GraphicsPath path = RoundedButton.CreateRoundedPath(ClientRectangle, CornerRadius);
        Region = new Region(path);
    }
}

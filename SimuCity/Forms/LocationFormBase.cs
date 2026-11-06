using SimuCity.Ui;

namespace SimuCity.Forms;

/// <summary>
/// Shared base class for the five location windows (University, Café,
/// Library, Market, Bank). It only provides the common look: a navy
/// header with the location name and a "Back to Town" button that
/// closes the window, so the user returns to the town map.
/// Each location form adds its own controls to the Body panel.
/// </summary>
public abstract class LocationFormBase : Form
{
    private static readonly Color Navy = Color.FromArgb(31, 58, 95);
    private static readonly Color Background = Color.FromArgb(238, 242, 246);

    protected LocationFormBase(string locationTitle)
    {
        Text = "SIMUCITY - " + locationTitle;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(820, 560);
        MinimumSize = new Size(760, 500);
        BackColor = Background;
        Font = new Font("Segoe UI", 9F);
        ShowIcon = false;

        // The Fill-docked body is added FIRST so that the Top-docked
        // header (added later in BuildHeader) docks correctly above it.
        Body = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            BackColor = Background,
        };
        Controls.Add(Body);
    }

    /// <summary>Main content area of the location window.</summary>
    protected Panel Body { get; }

    /// <summary>Creates the standard header and adds it above the body.</summary>
    protected void BuildHeader(string emoji, string title, string subtitle)
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Navy };

        var titleLabel = new Label
        {
            Text = emoji + "  " + title,
            Font = new Font("Segoe UI", 17F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(18, 10),
        };
        var subtitleLabel = new Label
        {
            Text = subtitle,
            Font = new Font("Segoe UI", 9F, FontStyle.Italic),
            ForeColor = Color.FromArgb(190, 208, 230),
            AutoSize = true,
            Location = new Point(20, 44),
        };
        var backButton = new RoundedButton
        {
            Text = "← Back to Town",
            Dock = DockStyle.Right,
            Width = 150,
            BackColor = Color.FromArgb(242, 163, 60),
        };
        backButton.Click += (_, _) => Close();   // closing returns to the town map

        header.Controls.Add(titleLabel);
        header.Controls.Add(subtitleLabel);
        header.Controls.Add(backButton);
        Controls.Add(header);
    }

    /// <summary>Small helper: a white rounded card used inside locations.</summary>
    protected static CardPanel NewCard(int height)
    {
        return new CardPanel { Dock = DockStyle.Top, Height = height, Padding = new Padding(12) };
    }

    /// <summary>Helper for a small caption label above an input.</summary>
    protected static Label NewLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            ForeColor = Color.FromArgb(60, 66, 76),
        };
    }
}

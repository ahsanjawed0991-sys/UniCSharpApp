using System.Drawing.Drawing2D;
using SimuCity.Data;
using SimuCity.Ui;

namespace SimuCity.Forms;

/// <summary>
/// The main town screen of SIMUCITY.
/// It shows the welcome header, a dashboard with live statistics,
/// an illustrated 2D map with five clickable location cards,
/// and a footer. Every location opens as a dialog; when it is
/// closed the user is back on the town map.
/// </summary>
public class MainForm : Form
{
    // Consistent color palette used across the whole application.
    private static readonly Color Navy      = Color.FromArgb(31, 58, 95);
    private static readonly Color Background = Color.FromArgb(238, 242, 246);
    private static readonly Color MutedText  = Color.FromArgb(110, 118, 129);

    // Registry of all town locations. The dashboard "Locations" count is
    // derived from this list, so it always matches the real UI.
    private readonly (string Name, string Emoji, string Blurb, Func<Form> Factory)[] _locations =
    {
        ("University", "🎓", "Track subjects & assignments", () => new UniversityForm()),
        ("Café",       "☕", "Order food & get a receipt",  () => new CafeForm()),
        ("Library",    "📚", "Your personal book collection", () => new LibraryForm()),
        ("Market",     "🛒", "Shopping list with quantities", () => new MarketForm()),
        ("Bank",       "🏦", "Wallet, income & expenses",   () => new BankForm()),
    };

    private readonly Panel _statsPanel = new();
    private readonly MapPanel _mapPanel = new();
    private readonly List<(Label Title, Label Value)> _statLabels = new();

    public MainForm()
    {
        Text = "SIMUCITY - My Virtual Town";
        ClientSize = new Size(1024, 680);
        MinimumSize = new Size(1024, 680);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Background;
        Font = new Font("Segoe UI", 9F);

        BuildDashboard();
        BuildMap();

        // Fill-docked map must be added FIRST so the top/bottom panels
        // dock correctly around it.
        Controls.Add(_mapPanel);
        Controls.Add(_statsPanel);
        Controls.Add(CreateHeader());
        Controls.Add(CreateFooter());

        _mapPanel.Resize += (_, _) => LayoutLocationCards();
        _statsPanel.Resize += (_, _) => LayoutStatCards();

        RefreshDashboard();
        LayoutStatCards();
        LayoutLocationCards();
    }

    // ---------------------------------------------------------------
    // Header: SIMUCITY title + subtitle
    // ---------------------------------------------------------------

    private Panel CreateHeader()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 96, BackColor = Navy };

        var title = new Label
        {
            Text = "🏙️  SIMUCITY",
            Font = new Font("Segoe UI", 26F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(24, 12),
        };
        var subtitle = new Label
        {
            Text = "Your Town, Your Everyday World",
            Font = new Font("Segoe UI", 12F, FontStyle.Italic),
            ForeColor = Color.FromArgb(190, 208, 230),
            AutoSize = true,
            Location = new Point(28, 60),
        };
        header.Controls.Add(title);
        header.Controls.Add(subtitle);
        return header;
    }


    // ---------------------------------------------------------------
    // Dashboard: statistic cards calculated from the real data
    // ---------------------------------------------------------------

    private void BuildDashboard()
    {
        _statsPanel.Dock = DockStyle.Top;
        _statsPanel.Height = 118;
        _statsPanel.BackColor = Background;

        string[] titles = { "Locations", "Pending Assignments", "Library Books", "Items to Buy", "Wallet Balance" };
        foreach (string t in titles)
        {
            var card = new CardPanel { Size = new Size(178, 88) };
            var titleLbl = new Label
            {
                Text = t,
                Dock = DockStyle.Top,
                Height = 26,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = MutedText,
            };
            var valueLbl = new Label
            {
                Text = "-",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Navy,
            };
            card.Controls.Add(valueLbl);   // fill first, then top
            card.Controls.Add(titleLbl);
            _statsPanel.Controls.Add(card);
            _statLabels.Add((titleLbl, valueLbl));
        }
    }

    private void LayoutStatCards()
    {
        int cardWidth = 178, gap = 14;
        int total = _statLabels.Count * cardWidth + (_statLabels.Count - 1) * gap;
        int x = Math.Max(8, (_statsPanel.ClientSize.Width - total) / 2);
        int y = ( _statsPanel.ClientSize.Height - 88 ) / 2;
        foreach (var (title, value) in _statLabels)
        {
            var card = (CardPanel)title.Parent!;
            card.Bounds = new Rectangle(x, y, cardWidth, 88);
            x += cardWidth + gap;
        }
    }

    /// <summary>Recalculates every dashboard number from the actual data.</summary>
    private void RefreshDashboard()
    {
        var values = new string[]
        {
            _locations.Length.ToString(),
            TownDataStore.PendingAssignments.ToString(),
            TownDataStore.BookCount.ToString(),
            TownDataStore.ItemsToBuy.ToString(),
            "Rs " + TownDataStore.Balance.ToString("N2"),
        };
        for (int i = 0; i < _statLabels.Count; i++)
            _statLabels[i].Value.Text = values[i];
    }

    // ---------------------------------------------------------------
    // Town map with the five location "buildings"
    // ---------------------------------------------------------------

    private readonly List<CardPanel> _locationCards = new();

    private void BuildMap()
    {
        _mapPanel.Dock = DockStyle.Fill;
        foreach (var loc in _locations)
        {
            var card = CreateLocationCard(loc);
            _locationCards.Add(card);
            _mapPanel.Controls.Add(card);
        }
    }

    private CardPanel CreateLocationCard((string Name, string Emoji, string Blurb, Func<Form> Factory) loc)
    {
        var card = new CardPanel { Size = new Size(196, 148), Cursor = Cursors.Hand };

        var emoji = new Label
        {
            Text = loc.Emoji,
            Dock = DockStyle.Top,
            Height = 62,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI Emoji", 26F),
            Cursor = Cursors.Hand,
        };
        var name = new Label
        {
            Text = loc.Name,
            Dock = DockStyle.Top,
            Height = 28,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Navy,
            Cursor = Cursors.Hand,
        };
        var blurb = new Label
        {
            Text = loc.Blurb + "\n(Click to visit)",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopCenter,
            Font = new Font("Segoe UI", 8.25F),
            ForeColor = MutedText,
            Cursor = Cursors.Hand,
        };

        card.Controls.Add(blurb);  // fill first
        card.Controls.Add(name);
        card.Controls.Add(emoji);

        // Clicking the card OR any label inside it opens the location.
        void Open(object? sender, EventArgs e) => Visit(loc.Factory());
        card.Click += Open;
        emoji.Click += Open;
        name.Click += Open;
        blurb.Click += Open;
        return card;
    }

    /// <summary>Positions the location cards on the map relative to its size.</summary>
    private void LayoutLocationCards()
    {
        if (_locationCards.Count != _locations.Length) return;
        var w = _mapPanel.ClientSize.Width;
        var h = _mapPanel.ClientSize.Height;
        Point[] spots =
        {
            new((int)(w * 0.06), (int)(h * 0.10)),   // University
            new((int)(w * 0.39), (int)(h * 0.06)),   // Café
            new((int)(w * 0.72), (int)(h * 0.10)),   // Bank
            new((int)(w * 0.20), (int)(h * 0.55)),   // Library
            new((int)(w * 0.58), (int)(h * 0.58)),   // Market
        };
        for (int i = 0; i < _locationCards.Count; i++)
            _locationCards[i].Location = spots[i];
    }

    /// <summary>Opens a location window; when it closes, refresh the dashboard.</summary>
    private void Visit(Form locationForm)
    {
        locationForm.ShowDialog(this);
        RefreshDashboard();
    }

    // ---------------------------------------------------------------
    // Footer
    // ---------------------------------------------------------------

    private Panel CreateFooter()
    {
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 46, BackColor = Color.White };

        var status = new Label
        {
            Text = "💾 Data is saved automatically to: " + TownDataStore.DataFolderPath,
            Dock = DockStyle.Left,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(16, 0, 0, 0),
            ForeColor = MutedText,
            Font = new Font("Segoe UI", 8.5F),
        };
        var exit = new RoundedButton
        {
            Text = "Exit Town",
            Dock = DockStyle.Right,
            Width = 120,
            BackColor = Color.FromArgb(192, 80, 77),
        };
        exit.Click += (_, _) => Close();

        footer.Controls.Add(status);
        footer.Controls.Add(exit);
        return footer;
    }


    // Refresh statistics whenever the town window becomes active again.
    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        RefreshDashboard();
    }

    /// <summary>
    /// The map itself: a green town area with two crossing roads and a pond,
    /// drawn with simple GDI+ shapes (no image files needed, so the app works
    /// even if assets are missing).
    /// </summary>
    private class MapPanel : Panel
    {
        public MapPanel()
        {
            BackColor = Color.FromArgb(211, 232, 199); // light "grass" green
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            int midX = Width / 2;
            int midY = Height / 2;

            // Two crossing roads.
            using (var roadBrush = new SolidBrush(Color.FromArgb(176, 178, 184)))
            {
                g.FillRectangle(roadBrush, 0, midY - 24, Width, 48);
                g.FillRectangle(roadBrush, midX - 24, 0, 48, Height);
            }
            // Dashed white center lines on the roads.
            using (var dashPen = new Pen(Color.White, 2) { DashStyle = DashStyle.Dash })
            {
                g.DrawLine(dashPen, 0, midY, Width, midY);
                g.DrawLine(dashPen, midX, 0, midX, Height);
            }
            // A small pond in the top-right corner.
            using (var pondBrush = new SolidBrush(Color.FromArgb(168, 214, 235)))
                g.FillEllipse(pondBrush, Width - 120, 24, 88, 56);
        }
    }
}

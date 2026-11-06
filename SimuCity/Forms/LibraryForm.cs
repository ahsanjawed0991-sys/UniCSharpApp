using SimuCity.Data;
using SimuCity.Models;
using SimuCity.Ui;

namespace SimuCity.Forms;

/// <summary>
/// LIBRARY location: the user can add books (title + author), browse the
/// whole collection, search by title or author, and remove entries.
/// </summary>
public class LibraryForm : LocationFormBase
{
    private readonly TextBox _titleBox = new();
    private readonly TextBox _authorBox = new();
    private readonly TextBox _searchBox = new();
    private readonly ListView _list = new();
    private readonly Label _emptyLabel = new();

    public LibraryForm() : base("Library")
    {
        BuildHeader("📚", "Library", "Keep your personal book collection - add, search and remove books.");
        BuildUi();
        RefreshList();
    }

    private void BuildUi()
    {
        // --- Top card: add a book + search ---
        var topCard = NewCard(112);

        var titleLbl = NewLabel("Book title:");
        titleLbl.Location = new Point(12, 14);
        _titleBox.Bounds = new Rectangle(90, 11, 220, 26);

        var authorLbl = NewLabel("Author:");
        authorLbl.Location = new Point(330, 14);
        _authorBox.Bounds = new Rectangle(390, 11, 220, 26);

        var addButton = new RoundedButton
        {
            Text = "➕ Add Book",
            Bounds = new Rectangle(600, 8, 150, 34),
        };
        addButton.Click += AddButton_Click;

        var searchLbl = NewLabel("🔍 Search:");
        searchLbl.Location = new Point(12, 62);
        _searchBox.Bounds = new Rectangle(90, 59, 300, 26);
        _searchBox.TextChanged += (_, _) => RefreshList();   // live search

        topCard.Controls.Add(_searchBox);
        topCard.Controls.Add(searchLbl);
        topCard.Controls.Add(addButton);
        topCard.Controls.Add(_authorBox);
        topCard.Controls.Add(authorLbl);
        topCard.Controls.Add(_titleBox);
        topCard.Controls.Add(titleLbl);

        // --- List card ---
        var listCard = new CardPanel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        _list.View = View.Details;
        _list.FullRowSelect = true;
        _list.GridLines = true;
        _list.Dock = DockStyle.Fill;
        _list.Columns.Add("Title", 300);
        _list.Columns.Add("Author", 240);

        _emptyLabel.Dock = DockStyle.Fill;
        _emptyLabel.TextAlign = ContentAlignment.MiddleCenter;
        _emptyLabel.ForeColor = Color.FromArgb(140, 148, 158);
        _emptyLabel.Font = new Font("Segoe UI", 10F, FontStyle.Italic);
        _emptyLabel.Visible = false;

        listCard.Controls.Add(_emptyLabel);
        listCard.Controls.Add(_list);

        // --- Bottom bar ---
        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 56 };
        var removeButton = new RoundedButton
        {
            Text = "🗑 Remove Selected",
            Dock = DockStyle.Left,
            Width = 190,
            BackColor = Color.FromArgb(192, 80, 77),
        };
        removeButton.Click += RemoveButton_Click;
        bottom.Controls.Add(removeButton);

        Body.Controls.Add(listCard);
        Body.Controls.Add(bottom);
        Body.Controls.Add(topCard);
    }

    private void AddButton_Click(object? sender, EventArgs e)
    {
        string title = _titleBox.Text.Trim();
        string author = _authorBox.Text.Trim();

        // --- Input validation ---
        if (title.Length == 0 || author.Length == 0)
        {
            MessageBox.Show("Please enter BOTH the book title and the author.",
                "SimuCity Library", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        TownDataStore.Data.Books.Add(new Book { Title = title, Author = author });
        TownDataStore.Save();

        _titleBox.Clear();
        _authorBox.Clear();
        RefreshList();

        MessageBox.Show($"📚 '{title}' by {author} added to your library!",
            "SimuCity Library", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void RemoveButton_Click(object? sender, EventArgs e)
    {
        if (_list.SelectedItems.Count == 0)
        {
            MessageBox.Show("Please select a book to remove.", "SimuCity Library",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var book = _list.SelectedItems[0].Tag as Book;
        if (book == null) return;

        TownDataStore.Data.Books.Remove(book);
        TownDataStore.Save();
        RefreshList();

        MessageBox.Show($"'{book.Title}' was removed from the library.",
            "SimuCity Library", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>Rebuilds the list, applying the search filter if one is typed.</summary>
    private void RefreshList()
    {
        string filter = _searchBox.Text.Trim().ToLower();

        _list.Items.Clear();
        foreach (var book in TownDataStore.Data.Books)
        {
            // Search matches title OR author (case-insensitive).
            bool matches = filter.Length == 0
                || book.Title.ToLower().Contains(filter)
                || book.Author.ToLower().Contains(filter);
            if (!matches) continue;

            var row = new ListViewItem(book.Title);
            row.SubItems.Add(book.Author);
            row.Tag = book;
            _list.Items.Add(row);
        }

        bool hasAnyBooks = TownDataStore.Data.Books.Count > 0;
        _emptyLabel.Visible = _list.Items.Count == 0;
        _emptyLabel.Text = hasAnyBooks
            ? "🔍 No books match your search."
            : "📭 Your library is empty.\nAdd your first book above!";
    }
}

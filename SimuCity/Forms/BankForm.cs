using SimuCity.Data;
using SimuCity.Models;
using SimuCity.Ui;

namespace SimuCity.Forms;

/// <summary>
/// BANK location: a simple personal wallet. The user adds income and
/// expenses, sees the current balance, and can review the transaction
/// history. The balance is always calculated from the real transactions.
/// </summary>
public class BankForm : LocationFormBase
{
    private readonly ComboBox _typeBox = new();
    private readonly TextBox _descBox = new();
    private readonly NumericUpDown _amountBox = new();
    private readonly Label _balanceLabel = new();
    private readonly ListView _list = new();
    private readonly Label _emptyLabel = new();

    public BankForm() : base("Bank")
    {
        BuildHeader("🏦", "Bank", "Your personal wallet - add income and expenses and watch the balance.");
        BuildUi();
        RefreshAll();
    }

    private void BuildUi()
    {
        // --- Top card: add a transaction ---
        var topCard = NewCard(118);

        var typeLbl = NewLabel("Type:");
        typeLbl.Location = new Point(12, 14);
        _typeBox.Bounds = new Rectangle(90, 11, 130, 26);
        _typeBox.DropDownStyle = ComboBoxStyle.DropDownList;  // fixed choices only
        _typeBox.Items.Add("Income");
        _typeBox.Items.Add("Expense");
        _typeBox.SelectedIndex = 0;

        var descLbl = NewLabel("Description:");
        descLbl.Location = new Point(240, 14);
        _descBox.Bounds = new Rectangle(335, 11, 220, 26);

        var amountLbl = NewLabel("Amount (Rs):");
        amountLbl.Location = new Point(560, 14);
        _amountBox.Bounds = new Rectangle(650, 11, 110, 26);
        _amountBox.DecimalPlaces = 2;
        _amountBox.Minimum = 1;              // validation: no zero/negative money
        _amountBox.Maximum = 100000000;
        _amountBox.Increment = 100;
        _amountBox.ThousandsSeparator = true;
        _amountBox.Value = 100;

        var addButton = new RoundedButton
        {
            Text = "➕ Add Transaction",
            Bounds = new Rectangle(90, 56, 180, 34),
        };
        addButton.Click += AddButton_Click;

        var hint = NewLabel("Tip: the balance is always recalculated from your real transactions.");
        hint.Location = new Point(290, 64);
        hint.ForeColor = Color.FromArgb(140, 148, 158);
        topCard.Controls.Add(hint);

        topCard.Controls.Add(addButton);
        topCard.Controls.Add(_amountBox);
        topCard.Controls.Add(amountLbl);
        topCard.Controls.Add(_descBox);
        topCard.Controls.Add(descLbl);
        topCard.Controls.Add(_typeBox);
        topCard.Controls.Add(typeLbl);

        // --- Balance card ---
        var balanceCard = new CardPanel { Dock = DockStyle.Top, Height = 74 };
        var balanceTitle = NewLabel("💰 CURRENT BALANCE");
        balanceTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        balanceTitle.ForeColor = Color.FromArgb(110, 118, 129);
        balanceTitle.Dock = DockStyle.Top;
        balanceTitle.Height = 24;
        balanceTitle.TextAlign = ContentAlignment.MiddleCenter;
        _balanceLabel.Dock = DockStyle.Fill;
        _balanceLabel.TextAlign = ContentAlignment.MiddleCenter;
        _balanceLabel.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
        balanceCard.Controls.Add(_balanceLabel);
        balanceCard.Controls.Add(balanceTitle);

        // --- Transaction history list ---
        var listCard = new CardPanel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        var historyTitle = NewLabel("📜 TRANSACTION HISTORY");
        historyTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        historyTitle.Dock = DockStyle.Top;
        historyTitle.Height = 26;

        _list.View = View.Details;
        _list.FullRowSelect = true;
        _list.GridLines = true;
        _list.Dock = DockStyle.Fill;
        _list.Columns.Add("Date", 130);
        _list.Columns.Add("Description", 260);
        _list.Columns.Add("Type", 100);
        _list.Columns.Add("Amount", 150);

        _emptyLabel.Dock = DockStyle.Fill;
        _emptyLabel.TextAlign = ContentAlignment.MiddleCenter;
        _emptyLabel.ForeColor = Color.FromArgb(140, 148, 158);
        _emptyLabel.Font = new Font("Segoe UI", 10F, FontStyle.Italic);
        _emptyLabel.Text = "📭 No transactions yet.\nAdd your first income or expense above!";
        _emptyLabel.Visible = false;

        listCard.Controls.Add(_emptyLabel);
        listCard.Controls.Add(_list);
        listCard.Controls.Add(historyTitle);

        Body.Controls.Add(listCard);
        Body.Controls.Add(balanceCard);
        Body.Controls.Add(topCard);
    }

    private void AddButton_Click(object? sender, EventArgs e)
    {
        string description = _descBox.Text.Trim();
        string type = _typeBox.SelectedItem?.ToString() ?? "Income";

        // Validation: description should not be empty; amount must be positive.
        if (description.Length == 0)
        {
            MessageBox.Show("Please write a short description (e.g. 'Pocket money').",
                "SimuCity Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        decimal amount = _amountBox.Value;
        if (amount <= 0)
        {
            MessageBox.Show("Amount must be greater than zero.",
                "SimuCity Bank", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        TownDataStore.Data.Transactions.Add(new Transaction
        {
            Date = DateTime.Now,
            Description = description,
            Type = type,
            Amount = amount,
        });
        TownDataStore.Save();

        _descBox.Clear();
        RefreshAll();

        MessageBox.Show($"🏦 {type} of Rs {amount:N2} added.\nNew balance: Rs {TownDataStore.Balance:N2}",
            "SimuCity Bank", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>Refreshes the balance label and the history list.</summary>
    private void RefreshAll()
    {
        decimal balance = TownDataStore.Balance;   // calculated from real data
        _balanceLabel.Text = "Rs " + balance.ToString("N2");
        _balanceLabel.ForeColor = balance >= 0
            ? Color.FromArgb(62, 155, 107)
            : Color.FromArgb(192, 80, 77);

        _list.Items.Clear();
        // Show newest transactions first.
        foreach (var t in TownDataStore.Data.Transactions.OrderByDescending(t => t.Date))
        {
            var row = new ListViewItem(t.Date.ToString("g"));
            row.SubItems.Add(t.Description);
            row.SubItems.Add(t.Type);
            string sign = t.Type == "Income" ? "+" : "-";
            row.SubItems.Add($"{sign} Rs {t.Amount:N2}");
            _list.Items.Add(row);
        }
        _emptyLabel.Visible = _list.Items.Count == 0;
    }
}

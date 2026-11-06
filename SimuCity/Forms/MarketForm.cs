using SimuCity.Data;
using SimuCity.Models;
using SimuCity.Ui;

namespace SimuCity.Forms;

/// <summary>
/// MARKET location: a shopping list where the user adds products with
/// quantities, marks items as purchased, and removes them.
/// </summary>
public class MarketForm : LocationFormBase
{
    private readonly TextBox _productBox = new();
    private readonly NumericUpDown _qtyBox = new();
    private readonly ListView _list = new();
    private readonly Label _emptyLabel = new();

    public MarketForm() : base("Market")
    {
        BuildHeader("🛒", "Market", "Build your shopping list, then tick items off as you buy them.");
        BuildUi();
        RefreshList();
    }

    private void BuildUi()
    {
        // --- Top card: add product ---
        var topCard = NewCard(76);

        var productLbl = NewLabel("Product:");
        productLbl.Location = new Point(12, 14);
        _productBox.Bounds = new Rectangle(90, 11, 240, 26);

        var qtyLbl = NewLabel("Quantity:");
        qtyLbl.Location = new Point(350, 14);
        _qtyBox.Bounds = new Rectangle(425, 11, 90, 26);
        _qtyBox.Minimum = 1;          // validation: quantity must be >= 1
        _qtyBox.Maximum = 999;
        _qtyBox.Value = 1;

        var addButton = new RoundedButton
        {
            Text = "➕ Add Item",
            Bounds = new Rectangle(540, 8, 150, 34),
        };
        addButton.Click += AddButton_Click;

        topCard.Controls.Add(addButton);
        topCard.Controls.Add(_qtyBox);
        topCard.Controls.Add(qtyLbl);
        topCard.Controls.Add(_productBox);
        topCard.Controls.Add(productLbl);

        // --- List card ---
        var listCard = new CardPanel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        _list.View = View.Details;
        _list.FullRowSelect = true;
        _list.GridLines = true;
        _list.Dock = DockStyle.Fill;
        _list.Columns.Add("Product", 300);
        _list.Columns.Add("Quantity", 100);
        _list.Columns.Add("Status", 160);

        _emptyLabel.Dock = DockStyle.Fill;
        _emptyLabel.TextAlign = ContentAlignment.MiddleCenter;
        _emptyLabel.ForeColor = Color.FromArgb(140, 148, 158);
        _emptyLabel.Font = new Font("Segoe UI", 10F, FontStyle.Italic);
        _emptyLabel.Text = "🛒 Your shopping list is empty.\nAdd your first product above!";
        _emptyLabel.Visible = false;

        listCard.Controls.Add(_emptyLabel);
        listCard.Controls.Add(_list);

        // --- Bottom bar ---
        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 56 };
        var purchasedButton = new RoundedButton
        {
            Text = "✔ Mark Purchased",
            Dock = DockStyle.Left,
            Width = 190,
            BackColor = Color.FromArgb(62, 155, 107),
        };
        purchasedButton.Click += (_, _) => SetPurchased(true);
        var notPurchasedButton = new RoundedButton
        {
            Text = "↩ Mark Not Purchased",
            Dock = DockStyle.Left,
            Width = 210,
            BackColor = Color.FromArgb(110, 118, 129),
        };
        notPurchasedButton.Click += (_, _) => SetPurchased(false);
        var removeButton = new RoundedButton
        {
            Text = "🗑 Remove",
            Dock = DockStyle.Left,
            Width = 130,
            BackColor = Color.FromArgb(192, 80, 77),
        };
        removeButton.Click += RemoveButton_Click;

        // Last-added left-docked control docks first (leftmost).
        bottom.Controls.Add(removeButton);
        bottom.Controls.Add(notPurchasedButton);
        bottom.Controls.Add(purchasedButton);

        Body.Controls.Add(listCard);
        Body.Controls.Add(bottom);
        Body.Controls.Add(topCard);
    }

    private void AddButton_Click(object? sender, EventArgs e)
    {
        string product = _productBox.Text.Trim();
        if (product.Length == 0)
        {
            MessageBox.Show("Please type a product name first.",
                "SimuCity Market", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_qtyBox.Value < 1)   // extra safety check on the quantity
        {
            MessageBox.Show("Quantity must be at least 1.",
                "SimuCity Market", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        TownDataStore.Data.ShoppingItems.Add(
            new ShoppingItem { Product = product, Quantity = (int)_qtyBox.Value, IsPurchased = false });
        TownDataStore.Save();

        _productBox.Clear();
        RefreshList();

        MessageBox.Show($"🛒 {product} (x{(int)_qtyBox.Value}) added to your shopping list!",
            "SimuCity Market", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>Marks the selected shopping item as purchased / not purchased.</summary>
    private void SetPurchased(bool purchased)
    {
        if (_list.SelectedItems.Count == 0)
        {
            MessageBox.Show("Please select an item in the list first.",
                "SimuCity Market", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var item = _list.SelectedItems[0].Tag as ShoppingItem;
        if (item == null) return;

        item.IsPurchased = purchased;
        TownDataStore.Save();
        RefreshList();
    }

    private void RemoveButton_Click(object? sender, EventArgs e)
    {
        if (_list.SelectedItems.Count == 0)
        {
            MessageBox.Show("Please select an item to remove.",
                "SimuCity Market", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var item = _list.SelectedItems[0].Tag as ShoppingItem;
        if (item == null) return;

        TownDataStore.Data.ShoppingItems.Remove(item);
        TownDataStore.Save();
        RefreshList();

        MessageBox.Show($"'{item.Product}' was removed from the list.",
            "SimuCity Market", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>Rebuilds the list from stored data, with an empty state.</summary>
    private void RefreshList()
    {
        _list.Items.Clear();
        foreach (var item in TownDataStore.Data.ShoppingItems)
        {
            var row = new ListViewItem(item.Product);
            row.SubItems.Add("x" + item.Quantity);
            row.SubItems.Add(item.IsPurchased ? "✔ Purchased" : "🛒 To buy");
            row.Tag = item;
            _list.Items.Add(row);
        }
        _emptyLabel.Visible = _list.Items.Count == 0;
    }
}

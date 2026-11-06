using SimuCity.Ui;

namespace SimuCity.Forms;

/// <summary>
/// CAFÉ location: shows a small menu, lets the user build an order,
/// calculates the total bill, and generates a simple text receipt.
/// The current order only lives for this visit (it is not saved).
/// </summary>
public class CafeForm : LocationFormBase
{
    /// <summary>One line on the café menu.</summary>
    private class MenuEntry
    {
        public string Name = string.Empty;
        public decimal Price;
        public override string ToString() => $"{Name}  -  Rs {Price:N0}";
    }

    /// <summary>One line in the current order.</summary>
    private class OrderLine
    {
        public string Name = string.Empty;
        public int Quantity;
        public decimal UnitPrice;
        public decimal LineTotal => Quantity * UnitPrice;
    }

    // The fixed café menu (name + price in Rs).
    private static readonly MenuEntry[] Menu =
    {
        new() { Name = "Cappuccino",   Price = 350m },
        new() { Name = "Espresso",     Price = 250m },
        new() { Name = "Cold Coffee",  Price = 300m },
        new() { Name = "Tea",          Price = 150m },
        new() { Name = "Sandwich",     Price = 400m },
        new() { Name = "Cake Slice",   Price = 280m },
        new() { Name = "Samosa",       Price = 120m },
        new() { Name = "Orange Juice", Price = 200m },
    };

    private readonly ListBox _menuList = new();
    private readonly NumericUpDown _qtyBox = new();
    private readonly ListView _orderList = new();
    private readonly Label _totalLabel = new();
    private readonly TextBox _receiptBox = new();
    private readonly List<OrderLine> _order = new();

    public CafeForm() : base("Café")
    {
        BuildHeader("☕", "Café", "Pick items from the menu, build your order and print a receipt.");
        BuildUi();
        RefreshOrder();
    }

    private void BuildUi()
    {
        // ----- Left: menu -----
        var menuCard = new CardPanel { Dock = DockStyle.Left, Width = 270, Padding = new Padding(12) };

        var menuTitle = NewLabel("📋 MENU");
        menuTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        menuTitle.Dock = DockStyle.Top;
        menuTitle.Height = 28;

        _menuList.Dock = DockStyle.Fill;
        _menuList.Font = new Font("Segoe UI", 10F);
        foreach (var entry in Menu)
            _menuList.Items.Add(entry);
        _menuList.SelectedIndex = 0;

        var qtyRow = new Panel { Dock = DockStyle.Bottom, Height = 86 };
        var qtyLbl = NewLabel("Quantity:");
        qtyLbl.Location = new Point(0, 8);
        _qtyBox.Bounds = new Rectangle(80, 5, 80, 26);
        _qtyBox.Minimum = 1;          // validation: quantity must be >= 1
        _qtyBox.Maximum = 20;
        _qtyBox.Value = 1;
        var addButton = new RoundedButton
        {
            Text = "➕ Add to Order",
            Bounds = new Rectangle(0, 44, 160, 34),
        };
        addButton.Click += AddToOrder_Click;
        qtyRow.Controls.Add(addButton);
        qtyRow.Controls.Add(_qtyBox);
        qtyRow.Controls.Add(qtyLbl);

        menuCard.Controls.Add(_menuList);   // fill
        menuCard.Controls.Add(qtyRow);      // bottom
        menuCard.Controls.Add(menuTitle);   // top

        // ----- Right: order + receipt -----
        var rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 0, 0, 0) };

        var receiptCard = new CardPanel { Dock = DockStyle.Bottom, Height = 170, Padding = new Padding(10) };
        var receiptTitle = NewLabel("🧾 RECEIPT");
        receiptTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        receiptTitle.Dock = DockStyle.Top;
        receiptTitle.Height = 22;
        _receiptBox.Dock = DockStyle.Fill;
        _receiptBox.Multiline = true;
        _receiptBox.ReadOnly = true;
        _receiptBox.ScrollBars = ScrollBars.Vertical;
        _receiptBox.Font = new Font("Consolas", 9F);
        _receiptBox.Text = "Your receipt will appear here...";
        receiptCard.Controls.Add(_receiptBox);
        receiptCard.Controls.Add(receiptTitle);

        var bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 52 };
        _totalLabel.Dock = DockStyle.Left;
        _totalLabel.Width = 260;
        _totalLabel.TextAlign = ContentAlignment.MiddleLeft;
        _totalLabel.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        _totalLabel.ForeColor = Color.FromArgb(31, 58, 95);
        var receiptButton = new RoundedButton
        {
            Text = "🧾 Receipt",
            Dock = DockStyle.Right,
            Width = 120,
            BackColor = Color.FromArgb(62, 155, 107),
        };
        receiptButton.Click += GenerateReceipt_Click;
        var clearButton = new RoundedButton
        {
            Text = "🗑 New Order",
            Dock = DockStyle.Right,
            Width = 120,
            BackColor = Color.FromArgb(192, 80, 77),
        };
        clearButton.Click += (_, _) => { _order.Clear(); RefreshOrder(); };
        bottomBar.Controls.Add(clearButton);      // right-most docks first
        bottomBar.Controls.Add(receiptButton);
        bottomBar.Controls.Add(_totalLabel);

        var orderCard = new CardPanel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        var orderTitle = NewLabel("🛒 YOUR ORDER");
        orderTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        orderTitle.Dock = DockStyle.Top;
        orderTitle.Height = 28;
        _orderList.View = View.Details;
        _orderList.FullRowSelect = true;
        _orderList.GridLines = true;
        _orderList.Dock = DockStyle.Fill;
        _orderList.Columns.Add("Item", 200);
        _orderList.Columns.Add("Qty", 60);
        _orderList.Columns.Add("Unit Price", 110);
        _orderList.Columns.Add("Line Total", 110);
        orderCard.Controls.Add(_orderList);
        orderCard.Controls.Add(orderTitle);

        rightPanel.Controls.Add(orderCard);       // fill
        rightPanel.Controls.Add(bottomBar);       // bottom
        rightPanel.Controls.Add(receiptCard);     // bottom (above bottomBar)

        Body.Controls.Add(rightPanel);            // fill
        Body.Controls.Add(menuCard);              // left
    }

    /// <summary>Adds the selected menu item (with quantity) to the order.</summary>
    private void AddToOrder_Click(object? sender, EventArgs e)
    {
        if (_menuList.SelectedItem is not MenuEntry entry)
        {
            MessageBox.Show("Please select a menu item first.", "SimuCity Café",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int qty = (int)_qtyBox.Value;   // NumericUpDown guarantees 1..20
        _order.Add(new OrderLine { Name = entry.Name, Quantity = qty, UnitPrice = entry.Price });
        RefreshOrder();
    }

    /// <summary>Rebuilds the order list and recalculates the total bill.</summary>
    private void RefreshOrder()
    {
        _orderList.Items.Clear();
        decimal total = 0m;
        foreach (var line in _order)
        {
            var row = new ListViewItem(line.Name);
            row.SubItems.Add(line.Quantity.ToString());
            row.SubItems.Add("Rs " + line.UnitPrice.ToString("N0"));
            row.SubItems.Add("Rs " + line.LineTotal.ToString("N0"));
            _orderList.Items.Add(row);
            total += line.LineTotal;    // simple loop + calculation
        }
        _totalLabel.Text = "TOTAL BILL:  Rs " + total.ToString("N2");
    }

    /// <summary>Builds a simple itemized receipt as text.</summary>
    private void GenerateReceipt_Click(object? sender, EventArgs e)
    {
        if (_order.Count == 0)
        {
            MessageBox.Show("Your order is empty - add something tasty first! ☕",
                "SimuCity Café", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        decimal total = 0m;
        var receipt = new System.Text.StringBuilder();
        receipt.AppendLine("********************************");
        receipt.AppendLine("        SIMUCITY  CAFÉ");
        receipt.AppendLine("********************************");
        receipt.AppendLine("Date: " + DateTime.Now.ToString("g"));
        receipt.AppendLine("--------------------------------");
        receipt.AppendLine($"{"Item",-18}{"Qty",4}{"Amount",10}");
        receipt.AppendLine("--------------------------------");
        foreach (var line in _order)
        {
            receipt.AppendLine($"{line.Name,-18}{line.Quantity,4}{"Rs " + line.LineTotal.ToString("N0"),10}");
            total += line.LineTotal;
        }
        receipt.AppendLine("--------------------------------");
        receipt.AppendLine($"{"TOTAL",-22}{"Rs " + total.ToString("N2"),10}");
        receipt.AppendLine("********************************");
        receipt.AppendLine("   Thank you! Visit again soon 😊");

        _receiptBox.Text = receipt.ToString();
        MessageBox.Show("Receipt generated! See the receipt panel below the order. 🧾",
            "SimuCity Café", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}

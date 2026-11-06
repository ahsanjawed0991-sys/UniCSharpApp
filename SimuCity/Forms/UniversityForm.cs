using SimuCity.Data;
using SimuCity.Models;
using SimuCity.Ui;

namespace SimuCity.Forms;

/// <summary>
/// UNIVERSITY location: the student can add subjects with assignments
/// and due dates, see them in a list, and mark them as completed.
/// </summary>
public class UniversityForm : LocationFormBase
{
    private readonly TextBox _subjectBox = new();
    private readonly TextBox _titleBox = new();
    private readonly DateTimePicker _duePicker = new();
    private readonly ListView _list = new();
    private readonly Label _emptyLabel = new();

    public UniversityForm() : base("University")
    {
        BuildHeader("🎓", "University", "Add your subjects and assignments, then mark them as completed.");
        BuildUi();
        RefreshList();
    }

    private void BuildUi()
    {
        // --- Input card (top) ---
        var inputCard = NewCard(118);

        var subjectLbl = NewLabel("Subject:");
        subjectLbl.Location = new Point(12, 14);
        _subjectBox.Bounds = new Rectangle(90, 11, 200, 26);

        var titleLbl = NewLabel("Assignment:");
        titleLbl.Location = new Point(310, 14);
        _titleBox.Bounds = new Rectangle(395, 11, 260, 26);

        var dueLbl = NewLabel("Due date:");
        dueLbl.Location = new Point(12, 56);
        _duePicker.Bounds = new Rectangle(90, 53, 200, 26);
        _duePicker.Format = DateTimePickerFormat.Short;

        var addButton = new RoundedButton
        {
            Text = "➕ Add Assignment",
            Bounds = new Rectangle(310, 50, 180, 34),
        };
        addButton.Click += AddButton_Click;

        var hint = NewLabel("Tip: pick a due date with the calendar dropdown.");
        hint.Location = new Point(500, 58);
        hint.ForeColor = Color.FromArgb(140, 148, 158);

        inputCard.Controls.Add(hint);
        inputCard.Controls.Add(addButton);
        inputCard.Controls.Add(_duePicker);
        inputCard.Controls.Add(dueLbl);
        inputCard.Controls.Add(_titleBox);
        inputCard.Controls.Add(titleLbl);
        inputCard.Controls.Add(_subjectBox);
        inputCard.Controls.Add(subjectLbl);

        // --- List card (fills the rest) ---
        var listCard = new CardPanel { Dock = DockStyle.Fill, Padding = new Padding(10) };

        _list.View = View.Details;
        _list.FullRowSelect = true;
        _list.GridLines = true;
        _list.Dock = DockStyle.Fill;
        _list.Columns.Add("Subject", 150);
        _list.Columns.Add("Assignment", 250);
        _list.Columns.Add("Due Date", 110);
        _list.Columns.Add("Status", 110);

        _emptyLabel.Text = "📭 No assignments yet.\nAdd your first assignment above!";
        _emptyLabel.Dock = DockStyle.Fill;
        _emptyLabel.TextAlign = ContentAlignment.MiddleCenter;
        _emptyLabel.ForeColor = Color.FromArgb(140, 148, 158);
        _emptyLabel.Font = new Font("Segoe UI", 10F, FontStyle.Italic);
        _emptyLabel.Visible = false;

        listCard.Controls.Add(_emptyLabel);   // added last -> stays on top when visible
        listCard.Controls.Add(_list);

        // --- Bottom action bar ---
        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = Color.Transparent };
        var doneButton = new RoundedButton
        {
            Text = "✔ Mark as Completed",
            Dock = DockStyle.Left,
            Width = 190,
            BackColor = Color.FromArgb(62, 155, 107),
        };
        doneButton.Click += (_, _) => SetCompleted(true);
        var pendingButton = new RoundedButton
        {
            Text = "↩ Mark as Pending",
            Dock = DockStyle.Left,
            Width = 180,
            BackColor = Color.FromArgb(110, 118, 129),
        };
        pendingButton.Click += (_, _) => SetCompleted(false);

        // Last-added left-docked control docks first (leftmost).
        bottom.Controls.Add(pendingButton);
        bottom.Controls.Add(doneButton);

        // Add Fill card first so Top/Bottom cards dock around it.
        Body.Controls.Add(listCard);
        Body.Controls.Add(bottom);
        Body.Controls.Add(inputCard);
    }

    /// <summary>Validates the inputs and adds a new assignment.</summary>
    private void AddButton_Click(object? sender, EventArgs e)
    {
        string subject = _subjectBox.Text.Trim();
        string title = _titleBox.Text.Trim();

        // --- Input validation: no empty entries allowed ---
        if (subject.Length == 0 || title.Length == 0)
        {
            MessageBox.Show(
                "Please fill in BOTH the subject and the assignment title.",
                "SimuCity - Missing info",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var assignment = new Assignment
        {
            Subject = subject,
            Title = title,
            DueDate = _duePicker.Value.Date,
            IsCompleted = false,
        };
        TownDataStore.Data.Assignments.Add(assignment);
        TownDataStore.Save();

        _subjectBox.Clear();
        _titleBox.Clear();
        RefreshList();

        MessageBox.Show(
            $"Assignment added!\n\n{subject} - {title}\nDue: {assignment.DueDate:d}",
            "SimuCity - Success",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    /// <summary>Marks the selected assignment as completed / pending.</summary>
    private void SetCompleted(bool completed)
    {
        if (_list.SelectedItems.Count == 0)
        {
            MessageBox.Show(
                "Please select an assignment in the list first.",
                "SimuCity - Nothing selected",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var assignment = _list.SelectedItems[0].Tag as Assignment;
        if (assignment == null) return;

        assignment.IsCompleted = completed;
        TownDataStore.Save();
        RefreshList();

        MessageBox.Show(
            completed
                ? $"✔ Great! '{assignment.Title}' marked as completed."
                : $"'{assignment.Title}' is back on the pending list.",
            "SimuCity",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    /// <summary>Rebuilds the list from the stored data and handles the empty state.</summary>
    private void RefreshList()
    {
        _list.Items.Clear();
        foreach (var a in TownDataStore.Data.Assignments)
        {
            var row = new ListViewItem(a.Subject);
            row.SubItems.Add(a.Title);
            row.SubItems.Add(a.DueDate.ToString("d"));
            row.SubItems.Add(a.IsCompleted ? "✔ Done" : "⏳ Pending");
            row.Tag = a;   // keep a reference to the real object
            _list.Items.Add(row);
        }
        _emptyLabel.Visible = _list.Items.Count == 0;
    }
}

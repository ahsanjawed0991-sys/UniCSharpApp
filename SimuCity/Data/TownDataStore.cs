using System.Text.Json;
using System.Windows.Forms;
using SimuCity.Models;

namespace SimuCity.Data;

/// <summary>
/// Handles ALL saving and loading of town data.
/// Data is stored as one JSON file inside the user's AppData folder
/// (e.g. C:\Users\&lt;name&gt;\AppData\Roaming\SimuCity\town-data.json),
/// so everything survives an application restart. System.Text.Json is
/// part of .NET 8, so no external packages are needed.
/// </summary>
public static class TownDataStore
{
    // Folder + file used for persistence (kept out of the source code folder).
    private static readonly string DataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SimuCity");

    private static readonly string DataFile = Path.Combine(DataFolder, "town-data.json");

    // The in-memory copy of everything the user has created.
    private static TownData _data = new();

    /// <summary>Read-only access to the loaded town data.</summary>
    public static TownData Data => _data;

    /// <summary>Folder where the JSON file lives (shown in the UI for clarity).</summary>
    public static string DataFolderPath => DataFolder;

    // ---------------------------------------------------------------
    // Load / Save
    // ---------------------------------------------------------------

    /// <summary>
    /// Loads data from disk. If the file does not exist yet (first run),
    /// a small set of sample records is created so the town is not empty.
    /// If the file is missing, empty or corrupted, the app starts with an
    /// empty town instead of crashing.
    /// </summary>
    public static void Load()
    {
        try
        {
            if (File.Exists(DataFile))
            {
                string json = File.ReadAllText(DataFile);
                _data = JsonSerializer.Deserialize<TownData>(json) ?? new TownData();
            }
            else
            {
                SeedSampleData();   // first run only - never overwrites real data
                Save();
            }
        }
        catch
        {
            // A corrupted/unreadable file must not crash the app.
            _data = new TownData();
        }
    }

    /// <summary>Writes the whole town data object to the JSON file.</summary>
    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(DataFolder); // safe if it already exists
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_data, options);
            File.WriteAllText(DataFile, json);
        }
        catch (Exception ex)
        {
            // Saving should never crash the demo - show a friendly message.
            MessageBox.Show(
                "Could not save data to disk.\n\n" + ex.Message,
                "SimuCity - Save problem",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    // ---------------------------------------------------------------
    // Statistics calculated from the REAL data (used by the dashboard)
    // ---------------------------------------------------------------

    /// <summary>Number of assignments that are not completed yet.</summary>
    public static int PendingAssignments => _data.Assignments.Count(a => !a.IsCompleted);

    /// <summary>Number of books currently in the library.</summary>
    public static int BookCount => _data.Books.Count;

    /// <summary>Number of shopping items still left to buy.</summary>
    public static int ItemsToBuy => _data.ShoppingItems.Count(i => !i.IsPurchased);

    /// <summary>Current wallet balance = income - expenses.</summary>
    public static decimal Balance =>
        _data.Transactions
            .Where(t => t.Type == "Income")
            .Sum(t => t.Amount)
        - _data.Transactions
            .Where(t => t.Type == "Expense")
            .Sum(t => t.Amount);

    // ---------------------------------------------------------------
    // Sample data for the very first run
    // ---------------------------------------------------------------

    private static void SeedSampleData()
    {
        _data = new TownData
        {
            Assignments = new List<Assignment>
            {
                new() { Subject = "Mathematics", Title = "Integration worksheet", DueDate = DateTime.Today.AddDays(3),  IsCompleted = false },
                new() { Subject = "English",     Title = "Essay: My Virtual Town", DueDate = DateTime.Today.AddDays(6), IsCompleted = false },
                new() { Subject = "C#",          Title = "WinForms lab report",   DueDate = DateTime.Today.AddDays(-1), IsCompleted = true  },
            },
            Books = new List<Book>
            {
                new() { Title = "A Tale of Two Cities",        Author = "Charles Dickens" },
                new() { Title = "The Hobbit",                  Author = "J.R.R. Tolkien" },
                new() { Title = "Introduction to Algorithms",  Author = "Cormen et al." },
            },
            ShoppingItems = new List<ShoppingItem>
            {
                new() { Product = "Milk",  Quantity = 2,  IsPurchased = false },
                new() { Product = "Bread", Quantity = 1,  IsPurchased = false },
                new() { Product = "Eggs",  Quantity = 12, IsPurchased = true  },
            },
            Transactions = new List<Transaction>
            {
                new() { Date = DateTime.Today.AddDays(-2), Description = "Pocket money", Type = "Income",  Amount = 5000m },
                new() { Date = DateTime.Today.AddDays(-1), Description = "Café snack",   Type = "Expense", Amount = 450m  },
                new() { Date = DateTime.Today.AddDays(-1), Description = "Textbook",     Type = "Expense", Amount = 1200m },
                new() { Date = DateTime.Today,            Description = "Tutoring",     Type = "Income",  Amount = 3000m },
            },
        };
    }
}

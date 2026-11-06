namespace SimuCity.Models;

// ---------------------------------------------------------------
// Simple "data classes" (models). Each class describes ONE record
// that the user creates inside a town location. They are plain
// classes with properties, which makes them easy to understand,
// easy to explain in a presentation, and easy to serialize to JSON.
// ---------------------------------------------------------------

/// <summary>A university assignment the student wants to track.</summary>
public class Assignment
{
    public string Subject { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
}

/// <summary>A book stored in the town library collection.</summary>
public class Book
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
}

/// <summary>One product on the market shopping list.</summary>
public class ShoppingItem
{
    public string Product { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public bool IsPurchased { get; set; }
}

/// <summary>A single wallet movement in the bank (income or expense).</summary>
public class Transaction
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    /// <summary>"Income" or "Expense".</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>Always a positive number; the sign comes from Type.</summary>
    public decimal Amount { get; set; }
}

/// <summary>
/// The root object that is written to / read from the JSON data file.
/// Grouping all lists in one object keeps the file format simple.
/// </summary>
public class TownData
{
    public List<Assignment> Assignments { get; set; } = new();
    public List<Book> Books { get; set; } = new();
    public List<ShoppingItem> ShoppingItems { get; set; } = new();
    public List<Transaction> Transactions { get; set; } = new();
}

# SIMUCITY – My Virtual Town 🎓☕📚🛒🏦

**UniCSharpApp** — a beginner-friendly **C# Windows Forms** desktop application built for a
university midterm project. SIMUCITY is a miniature interactive virtual town: a colorful 2D
town map with five clickable locations, and each location offers one small, useful,
fully working everyday feature.

> This is a student project, not a commercial product and not a complex city simulator.

---

## 🎯 Objectives

- Practice C# fundamentals: classes, objects, methods, events, lists, conditions, loops,
  calculations and file handling.
- Build a clean Windows Forms UI with a consistent color palette, rounded cards and
  intuitive navigation.
- Persist user data locally with **JSON files** (`System.Text.Json`) so everything
  survives an application restart.
- Keep the scope small, stable and demonstrable in a short presentation.

## ✨ Implemented features (5 locations)

| Location | What you can do |
|---|---|
| 🎓 **University** | Add subjects + assignments with due dates, view them in a list, mark them completed/pending |
| ☕ **Café** | Browse a menu, add items with quantities to an order, see the live total bill, generate an itemized receipt |
| 📚 **Library** | Add books (title + author), view the collection, live search by title/author, remove books |
| 🛒 **Market** | Create a shopping list, add products with quantities, mark items purchased, remove items |
| 🏦 **Bank** | Personal wallet: add income/expenses, see the current balance, view transaction history |

Additional details:

- **Main town screen** with the SIMUCITY title, the subtitle *"Your Town, Your Everyday World"*,
  an illustrated 2D map (roads + pond drawn in code — no image files required) and five
  clickable location cards.
- **Dashboard** with live statistics calculated from the real data: number of locations,
  pending assignments, books in the library, items still to buy, and the current wallet balance.
- Every location has a **← Back to Town** button (closing it returns to the map).
- **Input validation** (no empty entries, quantities ≥ 1, positive amounts) with friendly
  success/error message boxes instead of crashes.
- **Empty states** for every list ("No assignments yet…", etc.).
- **Persistence**: all data is saved automatically as JSON to
  `%AppData%\SimuCity\town-data.json` (i.e. `C:\Users\<you>\AppData\Roaming\SimuCity\`).
  A small sample town is created only on the very first run; existing data is never overwritten.

## 🛠 Technologies

- **C# / .NET 8** (`net8.0-windows`)
- **Windows Forms** (`UseWindowsForms`) — 2D desktop UI, GDI+ drawing for the map
- **System.Text.Json** for JSON save/load (built into .NET — no external packages)
- No databases, no cloud, no game engines, no third-party dependencies.

## 📁 Project structure

```
UniCSharpApp/
├── SimuCity.sln                  # Visual Studio solution
├── SimuCity/
│   ├── SimuCity.csproj           # .NET 8 Windows Forms project
│   ├── Program.cs                # App entry point (loads data, opens main window)
│   ├── Models.cs                 # Data classes: Assignment, Book, ShoppingItem, Transaction, TownData
│   ├── Data/
│   │   └── TownDataStore.cs      # JSON save/load + dashboard statistics + first-run sample data
│   ├── Ui/
│   │   └── RoundedControls.cs    # RoundedButton / CardPanel (rounded corners via Region)
│   └── Forms/
│       ├── MainForm.cs           # Town map, location cards, dashboard, header/footer
│       ├── LocationFormBase.cs   # Shared header + "Back to Town" for all locations
│       ├── UniversityForm.cs
│       ├── CafeForm.cs
│       ├── LibraryForm.cs
│       ├── MarketForm.cs
│       └── BankForm.cs
```

## ▶️ Setup & run instructions

### Option A — Visual Studio 2022 (recommended for the demo)
1. Install **Visual Studio 2022** with the **".NET desktop development"** workload
   (this includes the .NET 8 SDK).
2. Clone the repo: `git clone https://github.com/ahsanjawed0991-sys/UniCSharpApp.git`
3. Open **`SimuCity.sln`** in Visual Studio.
4. Press **F5** (or *Debug → Start Debugging*). The project targets `net8.0-windows`,
   so it builds and runs on Windows.

### Option B — .NET CLI (Windows)
```bash
git clone https://github.com/ahsanjawed0991-sys/UniCSharpApp.git
cd UniCSharpApp
dotnet build          # compiles the project
dotnet run --project SimuCity   # starts SIMUCITY
```

> Note: Windows Forms runs on **Windows**. The project file targets `net8.0-windows`
  so it builds correctly there; on Linux/macOS the Windows Desktop SDK is not
  available, so only the source can be inspected, not executed.

## 🧠 How the project works (short explanation)

1. `Program.Main` calls `TownDataStore.Load()` which reads `town-data.json` from the
   user's AppData folder (or creates sample data on first run), then opens `MainForm`.
2. `MainForm` draws the town map and five location cards. Clicking a card opens the
   matching location form with `ShowDialog`; when it closes, the dashboard refreshes.
3. Each location form edits the lists in `TownDataStore.Data` and calls
   `TownDataStore.Save()`, which serializes everything to JSON with `System.Text.Json`.
4. Dashboard numbers (pending assignments, book count, items to buy, balance) are
   computed with LINQ from the loaded data — never hardcoded.

## 🎤 How to demonstrate it to your teacher (5-minute script)

1. **Start** the app → show the town map, title and subtitle; point at the 5 location cards.
2. **Dashboard** → explain that the numbers come from real data (e.g. pending assignments = 2).
3. **University** → add an assignment (e.g. Physics, "Lab report", due date), then select it
   and click **Mark as Completed**; go back and watch the dashboard number drop.
4. **Café** → add 2 Cappuccinos and 1 Samosa, show the total, click **Generate Receipt**.
5. **Library** → add a book, type part of its name in **Search** to show filtering, remove it.
6. **Market** → add "Milk x2", mark it **Purchased**, watch the dashboard "Items to Buy" change.
7. **Bank** → add an income of Rs 1000, show the balance and history update.
8. **Close and reopen** the app → show that everything was loaded from the JSON file.
9. Mention where the data file lives (`%AppData%\SimuCity\town-data.json`).

## ⚠️ Known limitations

- Windows Forms requires **Windows** to run (by design of the framework).
- The café order is intentionally session-only (not saved); assignments, books,
  shopping list and wallet transactions are persisted.
- Graphics are simple code-drawn shapes/emoji — deliberately lightweight for a midterm.

## 👨‍💻 Author

University midterm project — **UniCSharpApp / SIMUCITY – My Virtual Town**.

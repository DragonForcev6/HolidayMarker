// Holiday Maker – meny och inmatning.
// Allt SQL ligger i Db.cs.

// Anslutningssträngen läses från connection.txt (finns inte i git, se connection.example.txt).
var connectionFile = Path.Combine(AppContext.BaseDirectory, "connection.txt");
if (!File.Exists(connectionFile))
{
    Console.WriteLine("Hittar inte connection.txt. Kopiera connection.example.txt till connection.txt och fyll i ditt lösenord.");
    return;
}
var db = new Db(File.ReadAllText(connectionFile).Trim());
db.TestConnection();
int? customerId = null; // vald kund

while (true)
{
    Console.WriteLine();
    Console.WriteLine($"=== Holiday Maker ===   Vald kund: {customerId?.ToString() ?? "(ingen)"}");
    Console.WriteLine("1. Registrera kund");
    Console.WriteLine("2. Välj kund");
    Console.WriteLine("3. Sök lediga rum");
    Console.WriteLine("4. Boka rum");
    Console.WriteLine("5. Visa kundens bokningar");
    Console.WriteLine("6. Ändra antal personer");
    Console.WriteLine("7. Avboka");
    Console.WriteLine("0. Avsluta");

    try
    {
        switch (Console.ReadLine())
        {
            case "1": /* TODO: fråga namn + kontakt -> db.AddCustomer */ break;
            case "2": /* TODO: db.PrintCustomers -> välj id -> customerId */ break;
            case "3": /* TODO: fråga ort, datum, antal, filter, sortering -> db.PrintAvailableRooms */ break;
            case "4": /* TODO: välj rum, datum, antal -> db.CreateBooking -> visa nr + totalpris */ break;
            case "5": /* TODO: db.PrintBookings(customerId) */ break;
            case "6": /* TODO: välj bokning, nytt antal -> db.UpdateGuests */ break;
            case "7": /* TODO: välj bokning -> db.DeleteBooking */ break;
            case "0": return;
            default: Console.WriteLine("Ogiltigt val."); break;
        }
    }
    catch (Exception ex)
    {
        // Gör att databasfel inte kraschar programmet.
        Console.WriteLine($"Fel: {ex.Message}");
    }
}

// ---- Hjälpmetoder för inmatning (fråga igen vid felaktig inmatning) ----

static int ReadInt(string prompt)
{
    // TODO: int.TryParse i en loop
    throw new NotImplementedException();
}

static DateOnly ReadDate(string prompt)
{
    // TODO: DateOnly.TryParse i en loop (format yyyy-MM-dd)
    throw new NotImplementedException();
}

// Holiday Maker – meny och inmatning.
// Allt SQL ligger i Db.cs.

using System.Globalization;

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
    Console.WriteLine($"=== Boundless Horizons ===   Vald kund: {customerId?.ToString() ?? "(ingen)"}");
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
            case "3":
                {
                    Console.Write("Stad:");
                    string city = Console.ReadLine() ?? "";
                    DateOnly checkIn = ReadDate("Inchecking (yyyy-MM-dd):");
                    DateOnly checkOut = ReadDate("Utcheckning (yyyy-MM-dd):");
                    int guests = ReadInt("Antal personer:");
                    bool needPool = ReadYesNo("Måste ha en pool (ja/nej):");
                    bool needResturant = ReadYesNo("Måste ha en resturang? (ja/nej):");
                    int? maxBeach = ReadOptionalInt("Max avstånd till stranden (m Enter = inget filter):");
                    int? maxCenter = ReadOptionalInt("Max avståndet till centrum (m, Enter = inget filter):");
                    bool sortByRating = ReadYesNo("Sortera på omdöme? (ja = omdöme, nej = pris):");

                    db.PrintAvailableRooms(city, checkIn, checkOut, guests, needPool, needRestaurant,
                        maxBeach, maxCenter, sortByRating);
                } break;
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
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();

        if (int.TryParse(input, out int value))
            return value;

        Console.WriteLine("Ogiltigt tal, försök igen.");
    }
}

static DateOnly ReadDate(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();

        if (DateOnly.TryParseExact(input, "yyyy-MM-dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
                return date;

        Console.WriteLine("Ogiltigt datum, använd formatet yyyy-MM-dd (t.ex 2026-11-11).");
    }
}

// Frågar användaren om ett valfritt heltal (0 eller högre).
// Retunerar null om användaren lämnar fältet tomt.
// Frågar igen till giltigt svar.
static int? ReadOptionalInt(string prompt)
{
   while (true) // Upprepar tills det retunerar giltigt svar.
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();
        
        // Retunerar null om användaren lämnar fältet tomt. 
        if (string.IsNullOrWhiteSpace(input))
            return null;
        
        // Godkvänner bar heltal som inte är negativt.
        if (int.TryParse(input, out int value) && value >= 0)
            return value; 
        
        Console.WriteLine("Ogiltigt tal, försök igen.");  
    }

static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine()?.Trim().ToLower();
            // 1. Om input är "ja" -> return true
                if (input == "ja")
                    return true;
            // 2. Om input är "nej" -> return false
                if (input == "nej")
                    return false;
            // 3. Annars: skriv "Ogiltigt svar, skriv ja eller nej
            Console.WriteLine("Ogiltigt svar skriv ja eller nej.");  
        }
    }
}

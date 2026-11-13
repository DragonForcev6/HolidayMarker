// Holiday Maker – meny och inmatning.
// All SQL ligger i Db.cs.

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

// Vald kund. int? betyder att den kan vara null (= ingen kund vald ännu).
// Menyval 4–7 kräver att en kund är vald.
int? customerId = null;

// Huvudloopen: visar menyn om och om igen tills operatören väljer 0.
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
    Console.WriteLine("8. Visa alla rum och bokade perioder");
    Console.WriteLine("0. Avsluta");

    try
    {
        switch (Console.ReadLine())
        {
            case "1": // Registrera kund
                {
                    string firstName = ReadText("Förnamn:");
                    string lastName = ReadText("Efternamn:");
                    string email = ReadText("E-post:");
                    string phone = ReadText("Telefonnummer:");

                    int newId = db.AddCustomer(firstName, lastName, email, phone);
                    Console.WriteLine($"Kund registrerad med id {newId}.");
                    customerId = newId; // den nya kunden blir vald direkt
                }
                break;

            case "2": // Välj kund: visa alla kunder och spara det id operatören väljer
                {
                    db.PrintCustomers();
                    int id = ReadInt("Välj kund-id: ");

                    if (db.CustomerExists(id))
                        customerId = id;
                    else
                        Console.WriteLine("Det finns ingen kund med det id:t.");
                }
                break;

            case "3": // Sök lediga rum: samla in alla sökval, sedan sköter SQL i Db.cs filtrering och sortering
                {
                    string city = ReadCity(db);
                    DateOnly checkIn = ReadDate("Incheckning (År-Månad-Dag): ");
                    DateOnly checkOut = ReadDate("Utcheckning (År-Månad-Dag): ");
                    int guests = ReadInt("Antal personer: ");
                    bool needPool = ReadYesNo("Måste hotellet ha en pool? (ja/nej): ");
                    bool needRestaurant = ReadYesNo("Måste hotellet ha en restaurang? (ja/nej): ");
                    bool sortByRating = ReadYesNo("Sortera på omdöme? (ja = omdöme, nej = pris): ");
                    // Avstånden är valfria: Enter ger null = inget filter.
                    int? maxBeach = ReadOptionalInt("Max avstånd till stranden (Skriv in antal meter eller tryck Enter för att gå vidare): ");
                    int? maxCenter = ReadOptionalInt("Max avstånd till centrum (Skriv in antal meter eller tryck Enter för att gå vidare): ");

                    Console.WriteLine();

                    db.PrintAvailableRooms(city, checkIn, checkOut, guests, needPool, needRestaurant,
                        maxBeach, maxCenter, sortByRating);
                }
                break;
            case "4": // Boka rum åt vald kund
                {
                    // break inuti if hoppar ur hela case 4 direkt.
                    if (customerId == null)
                    {
                        Console.WriteLine("Välj en kund först (val 2).");
                        break;
                    }

                    int roomId = ReadInt("Rum-id: ");
                    int guests = ReadInt("Antal personer: ");
                    DateOnly checkIn = ReadDate("Incheckning År-Månad-Dag: ");
                    DateOnly checkOut = ReadDate("Utcheckning År-Månad-Dag: ");

                    if (checkOut <= checkIn)
                    {
                        Console.WriteLine("Utcheckning måste ske efter incheckning.");
                        break;
                    }

                    // .Value plockar ut talet ur int? (säkert, vi har redan kollat att den inte är null).
                    // CreateBooking kastar ett undantag om rummet är fullt/för litet -> fångas av catch nedan.
                    int bookingId = db.CreateBooking(customerId.Value, roomId, checkIn, checkOut, guests);
                    Console.WriteLine($"Bokning klar med bokningsnummer: {bookingId}.");

                }
                break;
            case "5": // Visa vald kunds bokningar med totalpris
                {
                    if (customerId == null)
                    {
                        Console.WriteLine("Välj en kund först (val 2).");
                        break;
                    }

                    db.PrintBookings(customerId.Value);
                }
                break;
            case "6": // Ändra antal personer på en bokning (kapaciteten kontrolleras i SQL)
                {
                    if (customerId == null)
                    {
                        Console.WriteLine("Välj en kund först (val 2).");
                        break;
                    }

                    db.PrintBookings(customerId.Value);
                    int bookingId = ReadInt("Bokningsnummer:");
                    int guests = ReadInt("Antal gäster:");

                    if (guests < 1)
                    {
                        Console.WriteLine("Antalet gäster måste vara minst 1.");
                        break;
                    }

                    if (db.UpdateGuests(bookingId, customerId.Value, guests))
                        Console.WriteLine("Bokningen är uppdaterad.");
                    else
                        Console.WriteLine("Kunde inte ändra: kunden har ingen sådan bokning eller rummet rymmer inte så många.");
                }
                break;

            case "7": // Avboka: visa bokningarna, fråga om bekräftelse, ta bort
                {
                    if (customerId == null)
                    {
                        Console.WriteLine("Välj en kund först (val 2).");
                        break;
                    }

                    db.PrintBookings(customerId.Value);
                    int bookingId = ReadInt("Avboka bokningsnumret:");

                    if (!ReadYesNo("Är du säker på att du vill avboka? (ja/nej): "))
                    {
                        Console.WriteLine("Avbokningen är avbruten.");
                        break;
                    }

                    if (db.DeleteBooking(bookingId, customerId.Value))
                    {
                        Console.WriteLine("Bokningen är avbokad.");
                    }
                    else
                    {
                        Console.WriteLine("Kunden har ingen bokning med det numret.");
                    }
                }
                break;

            case "8": // Visa alla rum i en stad och när de är bokade (inga datum behövs)
                {
                    string city = ReadCity(db);
                    Console.WriteLine();
                    db.PrintRoomsWithBookings(city);
                }
                break;

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
// Alla följer samma mönster: while (true) -> fråga -> läs -> return om svaret är giltigt,
// annars skriv ett felmeddelande och loopa igen. Programmet kraschar alltså aldrig på felaktig inmatning.

// Frågar efter ett heltal. TryParse returnerar false i stället för att krascha om det inte är ett tal.
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

// Frågar efter ett datum i exakt formatet yyyy-MM-dd. InvariantCulture gör att det fungerar
// likadant oavsett vilka språkinställningar datorn har.
static DateOnly ReadDate(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();

        if (DateOnly.TryParseExact(input, "yyyy-MM-dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
            return date;

        Console.WriteLine("Ogiltigt datum, använd formatet yyyy-MM-dd (t.ex. 2026-11-11).");
    }
}

// Frågar användaren om ett valfritt heltal (0 eller högre).
// Returnerar null om användaren lämnar fältet tomt.
// Frågar igen tills svaret är giltigt.
static int? ReadOptionalInt(string prompt)
{
    while (true) // Upprepar tills metoden returnerar ett giltigt svar.
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();

        // Returnerar null om användaren lämnar fältet tomt.
        if (string.IsNullOrWhiteSpace(input))
            return null;

        // Godkänner bara heltal som inte är negativa.
        if (int.TryParse(input, out int value) && value >= 0)
            return value;

        Console.WriteLine("Ogiltigt tal, försök igen.");
    }
}

// Frågar ja/nej och returnerar true/false. Trim + ToLower gör att " JA " också godkänns.
static bool ReadYesNo(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine()?.Trim().ToLower();

        if (input == "ja")
            return true;

        if (input == "nej")
            return false;

        Console.WriteLine("Ogiltigt svar, skriv ja eller nej.");
    }
}

// Frågar efter en text. Frågar igen om användaren lämnar fältet tomt.
static string ReadText(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine()?.Trim();

        if (!string.IsNullOrWhiteSpace(input))
            return input;
        Console.WriteLine("Du måste skriva någonting.");
    }
}

// Visar städerna med nummer och frågar tills operatören väljer en siffra som finns.
// Returnerar namnet på staden. Används av menyval 3 och 8.
static string ReadCity(Db db)
{
    Console.WriteLine("Tillgängliga städer: ");
    List<string> cities = db.PrintCities();

    while (true)
    {
        int choice = ReadInt("Välj stad (siffra): ");

        if (choice >= 1 && choice <= cities.Count)
            return cities[choice - 1]; // listan börjar på 0 men menyn på 1

        Console.WriteLine($"Välj en siffra mellan 1 och {cities.Count}.");
    }
}



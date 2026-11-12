// Alla SQL-anrop. Använd ALLTID parametrar (@namn), aldrig strängsammanslagning.
using MySqlConnector;

public class Db(string connectionString)
{
    // Kollar att programmet når databasen. Skriver ut MySQL-versionen.
    public void TestConnection()
    {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();
        Console.WriteLine($"Ansluten till MySQL {conn.ServerVersion}, databas: {conn.Database}");
    }

    // Krav 1
    public int AddCustomer(string firstName, string lastName, string email, string phone)
    {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        string sql = @"INSERT INTO Kund (fornamn, efternamn, `e-post`, telefon)
                        VALUES (@fornamn, @efternamn, @epost, @telefon)";
    
    using var cmd = new MySqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@fornamn", firstName);
    cmd.Parameters.AddWithValue("@efternamn", lastName);
    cmd.Parameters.AddWithValue("@epost", email);
    cmd.Parameters.AddWithValue("@telefon", phone);
    
    cmd.ExecuteNonQuery(); // kör INSERT
        return (int)cmd.LastInsertedId; // id:t som MySql gav ny kund
    }

    public void PrintCustomers()
    {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        string sql ="SELECT id, fornamn, efternamn, `e-post`, telefon FROM Kund ORDER BY id";
        using var cmd = new MySqlCommand(sql, conn);
        using var reader =cmd.ExecuteReader();

        while (reader.Read())
        {
            int id = reader.GetInt32("id");
            string firstName = reader.GetString("fornamn");
            string lastName = reader.GetString("efternamn");
            string email = reader.GetString("e-post");
            string phone = reader.GetString("telefon");

            Console.WriteLine($"{id}: {firstName} {lastName}, {email}, {phone}");
        }
    }

    public void PrintCities()
    {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        string sql = "SELECT DISTINCT stad FROM Boende ORDER BY stad";
        using var cmd = new MySqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            Console.Write(reader.GetString("stad") + "  "); // skriver stad, ingen radbytning
        }
        Console.WriteLine(); // en radbrytning när alla städer är utskrivna.

    }

    // Krav 2 – SQL sköter filtrering och sortering.
    // Tips: utcheckningsdagen är ledig. Två perioder överlappar om
    //       befintlig.incheckning < ny.utcheckning OCH befintlig.utcheckning > ny.incheckning
    // Valfria filter: null = inget filter.
    public void PrintAvailableRooms(string city, DateOnly checkIn, DateOnly checkOut, int guests,
        bool needPool, bool needRestaurant, int? maxBeach, int? maxCenter, bool sortByRating)
    {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        // Sortering kan inte vara en @parameter, så välj mellan två fasta texter.
        string orderBy = sortByRating ? "b.betyg DESC, r.pris_per_natt ASC" : "r.pris_per_natt ASC";

        string sql = $@"SELECT r.id, b.hotell_namn, r.rums_nummer, r.max_personer, r.pris_per_natt,
                       b.betyg, b.strand_avstand, b.centrum_avstand, b.pool, b.resturang
                       FROM Rum r
                       JOIN Boende b ON b.boende_id = r.boende_id
                       WHERE b.stad = @city
                        AND r.max_personer >= @guests
                        AND (@needPool = 0 OR b.pool = 1)
                        AND (@needResturant = 0 OR b.resturang = 1)
                        AND (@maxBeach IS NULL OR b.strand_avstand <= @maxBeach)
                        AND (@maxCenter IS NULL OR b.centrum_avstand <= @maxCenter)
                        AND NOT EXISTS (
                             SELECT 1 FROM Bokningar bk
                             WHERE bk.rum_id = r.id
                               AND bk.incheckning < @checkOut
                               AND bk.utcheckning > @checkIn)
                    ORDER BY {orderBy}";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@city", city);
        cmd.Parameters.AddWithValue("@checkIn", checkIn);
        cmd.Parameters.AddWithValue("@checkOut", checkOut);
        cmd.Parameters.AddWithValue("@guests", guests);
        cmd.Parameters.AddWithValue("@needPool", needPool);
        cmd.Parameters.AddWithValue("@needResturant", needRestaurant);
        cmd.Parameters.AddWithValue("@maxBeach", (object?)maxBeach ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@maxCenter", (object?)maxCenter ?? DBNull.Value);

        using var reader = cmd.ExecuteReader();

        int count = 0;
        while (reader.Read())
        {
            count++;
            int roomId = reader.GetInt32("id");
            string hotel = reader.GetString("hotell_namn");
            string roomNumber = reader.GetString("rums_nummer"); // varchar i databasen, t.ex. "A1"
            int maxPersons = reader.GetInt32("max_personer");
            decimal price = reader.GetDecimal("pris_per_natt");
            int rating = reader.GetInt32("betyg");

            Console.WriteLine($"Rum_id: {roomId}, Hotell: {hotel}, Rum: {roomNumber}, Max personer: {maxPersons}, Pris per natt: {price} kr, Betyg: {rating}");
        }

        if (count == 0)
        {
            Console.WriteLine("Tyvärr hittades inga lediga rum.");
        }
    }
       

    // Krav 3 – kolla tillgänglighet + kapacitet igen innan INSERT. Returnerar bokningsnummer.
    public int CreateBooking(int customerId, int roomId, DateOnly checkIn, DateOnly checkOut, int guests)
        => throw new NotImplementedException();

    // Krav 4 – totalpris = antal nätter × pris per natt
    public void PrintBookings(int customerId)
        => throw new NotImplementedException();

    // Krav 5 – nya antalet måste rymmas i rummet
    public bool UpdateGuests(int bookingId, int guests)
        => throw new NotImplementedException();

    // Krav 6
    public bool DeleteBooking(int bookingId)
        => throw new NotImplementedException();
}



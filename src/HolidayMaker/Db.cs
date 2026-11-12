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

        string sql = "SELECT id, fornamn, efternamn, `e-post`, telefon FROM Kund ORDER BY id";
        using var cmd = new MySqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

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
    {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        // Kolla att rummet finns och att det rymmer det antal gäster som önskas.
        int maxPersons;
        decimal price;

        using (var cmd = new MySqlCommand("SELECT max_personer, pris_per_natt FROM Rum WHERE id = @roomId", conn))
        {
            cmd.Parameters.AddWithValue("@roomId", roomId);
            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                throw new InvalidOperationException("Rummet finns inte.");

            maxPersons = reader.GetInt32("max_personer");
            price = reader.GetDecimal("pris_per_natt");
        }

        if (guests > maxPersons)
            throw new InvalidOperationException($"Rummet rymmer max {maxPersons} personer.");

        // Kolla att rummet är ledigt under den önskade perioden.
        string checkAvailabilitySql = @"SELECT COUNT(*) FROM Bokningar
                                        WHERE rum_id = @roomId
                                          AND incheckning < @checkOut
                                          AND utcheckning > @checkIn";
        using var checkAvailabilityCmd = new MySqlCommand(checkAvailabilitySql, conn);
        checkAvailabilityCmd.Parameters.AddWithValue("@roomId", roomId);
        checkAvailabilityCmd.Parameters.AddWithValue("@checkIn", checkIn);
        checkAvailabilityCmd.Parameters.AddWithValue("@checkOut", checkOut);

        int overlappingBookings = Convert.ToInt32(checkAvailabilityCmd.ExecuteScalar());
        if (overlappingBookings > 0)
            throw new InvalidOperationException("Rummet är tyvärr inte ledigt under den önskade perioden.");

        // Skapa bokningen.
        string insertBookingSql = @"INSERT INTO Bokningar (kund_id, rum_id, antal_personer, incheckning, utcheckning)
                                    VALUES (@customerId, @roomId, @guests, @checkIn, @checkOut)";
        using var insertBookingCmd = new MySqlCommand(insertBookingSql, conn);
        insertBookingCmd.Parameters.AddWithValue("@customerId", customerId);
        insertBookingCmd.Parameters.AddWithValue("@roomId", roomId);
        insertBookingCmd.Parameters.AddWithValue("@guests", guests);
        insertBookingCmd.Parameters.AddWithValue("@checkIn", checkIn);
        insertBookingCmd.Parameters.AddWithValue("@checkOut", checkOut);

        insertBookingCmd.ExecuteNonQuery();

        int nights = checkOut.DayNumber - checkIn.DayNumber;
        Console.WriteLine($"{nights} nätter x {price} kr = {nights * price} kr");

        return (int)insertBookingCmd.LastInsertedId; // Returnerar bokningsnummer

    }


    public void PrintBookings(int customerId)
    {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        string sql = @"SELECT bk.boknings_id, b.hotell_namn, b.stad, r.rums_nummer,
                              bk.antal_personer, bk.incheckning, bk.utcheckning, r.pris_per_natt,
                              DATEDIFF(bk.utcheckning, bk.incheckning) AS natter,
                              DATEDIFF(bk.utcheckning, bk.incheckning) * r.pris_per_natt AS totalpris
                    FROM Bokningar bk
                    JOIN Rum r ON r.id = bk.rum_id
                    JOIN Boende b ON b.boende_id =r.boende_id
                    WHERE bk.kund_id = @customerId
                    ORDER BY bk.incheckning";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@customerId", customerId);
        using var reader = cmd.ExecuteReader();

            int count = 0;
            while (reader.Read())
            {
                count++;

                // 1. Läs ut varje kolumn från SELECT (namnen måste matcha SQL-frågan)
                int bookingId = reader.GetInt32("boknings_id");
                string hotel = reader.GetString("hotell_namn");
                string city = reader.GetString("stad");
                string room = reader.GetString("rums_nummer");
                int guests = reader.GetInt32("antal_personer");
                DateTime checkIn = reader.GetDateTime("incheckning");
                DateTime checkOut = reader.GetDateTime("utcheckning");
                decimal price = reader.GetDecimal("pris_per_natt");
                long nights = reader.GetInt64("natter");
                decimal total = reader.GetDecimal("totalpris");

                // 2. Skriv ut bokningen
                Console.WriteLine($"Bokning {bookingId}: {hotel} ({city}), rum {room}, {guests} pers, " +
                                  $"{checkIn:yyyy-MM-dd} - {checkOut:yyyy-MM-dd}, " +
                                  $"{nights} nätter x {price} kr = {total} kr");
            }

            if (count == 0)
            {
                Console.WriteLine("Kunden har inga bokningar.");
            }

        
    }

    // Krav 5 – nya antalet måste rymmas i rummet
    public bool UpdateGuests(int bookingId, int guests)
    {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        string sql = @"UPDATE Bokningar bk
                    JOIN Rum r ON r.id = bk.rum_id
                    SET bk.antal_personer = @guests
                    WHERE bk.boknings_id = @bookingId
                        AND r.max_personer >= @guests";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@guests", guests);
        cmd.Parameters.AddWithValue("@bookingId", bookingId);

        int rows = cmd.ExecuteNonQuery();
        return rows > 0;
    }

   
    public bool DeleteBooking(int bookingId)
    {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        string sql = @"DELETE FROM Bokningar WHERE boknings_id = @bookingId";
       
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@bookingId", bookingId);

        return cmd.ExecuteNonQuery() > 0;
    }
}



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
    public int AddCustomer(string name, string email, string phone)
        => throw new NotImplementedException();

    public void PrintCustomers()
        => throw new NotImplementedException();

    // Krav 2 – SQL sköter filtrering och sortering.
    // Tips: utcheckningsdagen är ledig. Två perioder överlappar om
    //       befintlig.incheckning < ny.utcheckning OCH befintlig.utcheckning > ny.incheckning
    // Valfria filter: null = inget filter.
    public void PrintAvailableRooms(string city, DateOnly checkIn, DateOnly checkOut, int guests,
        bool needPool, bool needRestaurant, int? maxBeach, int? maxCenter, bool sortByRating)
        => throw new NotImplementedException();

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

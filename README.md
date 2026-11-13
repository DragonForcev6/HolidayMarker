# Holiday Maker

Konsolapplikation för en fiktiv resebyrå (Boundless Horizons). Operatören kan registrera kunder,
söka lediga rum med filter, boka, visa bokningar med totalpris, ändra antal personer, avboka
och se alla rum i en stad med de perioder som redan är bokade.

Byggd i C# (.NET 10) mot en MySQL-databas.

## Filer
- `hotel.png` – E/R-diagram (Boende 1–N Rum 1–N Bokningar N–1 Kund)
- `sql/01_create_tables.sql` – skapar databasen `hotel` och tabellerna
- `sql/02_testdata.sql` – testdata: 5 boenden i 5 städer, 21 rum, 20 kunder, 25 bokningar i december 2026
- `src/HolidayMaker/` – C#-programmet (`Program.cs` = meny och inmatning, `Db.cs` = all SQL)
- `src/HolidayMaker/connection.example.txt` – mall för anslutningssträngen

## Kom igång
1. **Databas:** MySQL 8. Öppna och kör först `sql/01_create_tables.sql` och sedan
   `sql/02_testdata.sql` i DBeaver (Alt+X kör hela skriptet). Skriptet skapar databasen `hotel`.
2. **Anslutningssträng:** kopiera `src/HolidayMaker/connection.example.txt` till
   `src/HolidayMaker/connection.txt` och fyll i ditt eget användarnamn och lösenord.
   `connection.txt` finns i `.gitignore` så att lösenordet aldrig hamnar på GitHub.
3. **Kör programmet** från projektroten:
   ```
   dotnet run --project src/HolidayMaker
   ```
   Programmet skriver `Ansluten till MySQL ...` om anslutningen fungerar.

NuGet-paketet `MySqlConnector` hämtas automatiskt vid första bygget.

## Tester
Alla tester är gjorda mot testdatan i `02_testdata.sql`.

| Test | Resultat |
|------|----------|
| Ledigt rum visas, upptaget filtreras bort | ✅ Göteborg 1–5 dec visar rum A1 och B1; A2 och B2 är bokade och visas inte |
| Bokning samma dag som annan kund checkar ut | ✅ Rum 11 i Malmö bokat 1–5 dec, ny bokning 5–8 dec godkänns |
| Överlappande bokning nekas (även omslutande) | ✅ Samma period nekas, och 10–20 dec runt en bokning 11–18 dec nekas |
| För stort sällskap kan inte boka | ✅ 3 personer i ett enkelrum ger "Rummet rymmer max 1 personer" |
| Kombinerade filter + båda sorteringarna | ✅ Pool + restaurang + max 100 m till strand ger inga träffar i Malmö, max 200 m ger träffar. Båda sorteringarna körs utan fel, men eftersom varje stad bara har ett hotell i testdatan blir ordningen densamma (se Reflektion) |
| Ändring av antal personer följer kapacitet | ✅ Enkelrum: 1 person godkänns, 3 personer nekas |
| Avbokning frigör rummet | ✅ Efter avbokning visas rummet igen i sökningen för samma period |
| Kund-id som inte finns kan inte väljas | ✅ Kund-id 999 ger "Det finns ingen kund med det id:t" och ingen kund blir vald |
| Bara egna bokningar kan ändras och avbokas | ✅ Med kund 1 vald nekas ändring och avbokning av bokning 2 (som tillhör kund 2). Kund 1:s egen bokning 1 går att ändra och avboka |
| Stad väljs med en siffra | ✅ Städerna visas som "1. Göteborg  2. Malmö ...". Siffran för Malmö ger samma träffar som förut. 0, 9 och "abc" ger en ny fråga |
| Alla rum och bokade perioder (menyval 8) | ✅ Malmö visar bokningarna 12–18 dec på rum 14–17. Rum utan kommande bokningar visar "inga bokningar" |
| Data finns kvar efter omstart | ✅ Kund och bokningar som skapats i programmet finns kvar när programmet startas om |

Testfall som finns färdiga i testdatan:
- **Sökning utan träff:** Malmö 2026-12-12 till 2026-12-18 (alla rum är bokade).
- **Sökning med träff:** Malmö 2026-12-01 till 2026-12-05.

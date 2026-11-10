# Holiday Maker

Konsolapplikation för en fiktiv resebyrå.

## Filer
- `er-diagram.png` – E/R-diagram (TODO: lägg till)
- `sql/01_create_tables.sql` – skapar tabellerna
- `sql/02_testdata.sql` – testdata
- `src/HolidayMaker/` – C#-programmet (`Program.cs` = meny, `Db.cs` = SQL)

## Kom igång
1. TODO: vilken databas + kör `01_create_tables.sql` och `02_testdata.sql` i DBeaver
2. TODO: var anslutningssträngen anges
3. `cd src/HolidayMaker` och `dotnet run`

## Tester
| Test | Resultat |
|------|----------|
| Ledigt rum visas, upptaget filtreras bort | |
| Bokning samma dag som annan kund checkar ut | |
| Överlappande bokning nekas (även omslutande) | |
| För stort sällskap kan inte boka | |
| Kombinerade filter + båda sorteringarna | |
| Ändring av antal personer följer kapacitet | |
| Avbokning frigör rummet | |
| Data finns kvar efter omstart | |

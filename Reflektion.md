# Reflektion

## Datamodell

Databasen består av fyra tabeller: `Boende`, `Rum`, `Kund` och `Bokningar` (se `hotel.png`).

- Ett boende kan ha många rum. Därför har `Rum` en kolumn `boende_id` som pekar på `Boende`.
- En bokning kopplar ihop en kund med ett rum under en viss period, via `kund_id` och `rum_id`.
- Pris och hur många som får plats har jag lagt på rummet, eftersom rummen på samma hotell kan kosta olika och vara olika stora. Pool, restaurang, avstånd och betyg har jag lagt på boendet, eftersom de gäller hela hotellet.

### Varför har staden ingen egen tabell?
Jag funderade på att göra en egen tabell för orter, men valde att ha staden som en vanlig kolumn (`stad`) i `Boende`. Det är bara några få städer och de används bara när man söker. En egen tabell hade varit mer normaliserad och skyddat mot stavfel som "Goteborg", men då hade varje sökning behövt en JOIN till. För att ingen ska stava fel visar programmet vilka städer som finns med en siffra framför, och operatören väljer stad med siffran i stället för att skriva namnet.

### Regler som skyddar datan
- Alla tabeller har `PRIMARY KEY` med `AUTO_INCREMENT`, och tabellerna hänger ihop med `FOREIGN KEY`.
- `NOT NULL` på alla kolumner som måste fyllas i.
- `CHECK` ser till att betyget är 1–5, att avstånd och pris inte är negativa, att max antal personer är minst 1, att en bokning gäller minst 1 person och att utcheckningen är efter incheckningen.
- `UNIQUE (boende_id, rums_nummer)` gör att samma rumsnummer kan finnas på olika hotell, men inte två gånger på samma.

### Något jag fick ändra på vägen
Från början sparade jag avstånden som text (`varchar`). Det visade sig bli fel, eftersom text jämförs tecken för tecken. Då blir till exempel `'1000' <= '500'` sant, eftersom 1 kommer före 5, och avståndsfiltret gav fel svar. Så jag bytte till `int` i stället.

## Säkerhet

### SQL-injektion
All SQL i `Db.cs` använder parametrar (`@city`, `@guests` osv.). Det operatören skriver klistras alltså aldrig in direkt i SQL-texten, så det går inte att lura in egen SQL. Det enda som byggs ihop som en sträng är `ORDER BY`, men där väljer koden bara mellan två fasta texter som jag har skrivit själv.

### Lösenordet till databasen
Anslutningssträngen med lösenordet ligger i `connection.txt`. Den filen står i `.gitignore`, så den laddas aldrig upp till GitHub. I repot finns bara `connection.example.txt` som en mall att utgå från.

### Att datan blir rätt
Precis innan bokningen sparas kollar `CreateBooking` en gång till att rummet är ledigt och att det får plats så många personer. Det kan kännas dubbelt, eftersom sökningen redan har filtrerat, men operatören kan ju skriva in vilket rum-id som helst. Skulle något ändå slinka igenom finns databasens `CHECK` och `FOREIGN KEY` som en sista spärr.

### Bara den valda kundens bokningar
När man väljer kund kollar `CustomerExists` först att kund-id:t finns. Annars blir ingen kund vald. Från början gick det att välja vilket id som helst, och felet upptäcktes först vid bokningen när främmande nyckeln sa stopp.

`UpdateGuests` och `DeleteBooking` får nu också med kund-id:t och har `AND kund_id = @customerId` i SQL-frågan. Förut kunde man ändra eller avboka en annan kunds bokning om man skrev in rätt bokningsnummer. Nu matchar frågan ingen rad om bokningen tillhör någon annan, så `ExecuteNonQuery()` returnerar 0 och programmet säger att kunden inte har någon sådan bokning.

### Om något går fel
Varje menyval körs inuti en `try/catch`. Blir det ett databasfel visas ett meddelande i stället för att hela programmet kraschar. Skriver man in något ogiltigt frågar programmet igen tills svaret är okej.

## Resultat

Alla åtta menyval fungerar: registrera kund, välja kund, söka lediga rum med filter och sortering, boka, visa bokningar med totalpris, ändra antal personer, avboka och visa alla rum med bokade perioder. Alla tester i README-tabellen är godkända.

### Visa alla rum utan att ange datum
Menyval 8 lade jag till för att kunna se vilka rum som finns och när de är bokade, utan att först skriva in datum. Ett rum är ju bara ledigt eller upptaget under en viss period, så i stället visas varje rum med de perioder som redan är bokade.

`PrintRoomsWithBookings` använder `LEFT JOIN` mot `Bokningar`, så att rum utan bokningar också kommer med. Villkoret `bk.utcheckning > CURDATE()` står i `ON` och inte i `WHERE`, annars hade rum utan bokningar försvunnit. `GROUP_CONCAT` slår ihop alla bokade perioder för ett rum till en enda text.

### Det som inte blev helt som jag tänkt
- Sorteringen på betyg syns inte så tydligt med min testdata. Varje stad har bara ett hotell, och alla rum på samma hotell har samma betyg, så det finns inte så mycket att sortera.

## Möjliga förbättringar

- **Använda en transaktion i `CreateBooking`.** Om två operatörer skulle boka samma rum exakt samtidigt kan båda hinna klara kontrollen innan någon av dem har sparat. Med en transaktion och låsning skulle det inte kunna hända.
- **Fler hotell per stad i testdatan**, så att det syns ordentligt att sorteringen på betyg fungerar.
- **Enklare kolumnnamn**, utan bindestreck och å/ä/ö. Till exempel `epost` i stället för `e-post`, som måste skrivas med backticks varje gång.

## Användning av AI och annan hjälp

Under hela arbetet har jag använt Claude Code (en AI-assistent i VS Code) som en slags handledare. Jag har bett den visa var och hur saker ska göras, och sedan skrivit det mesta av koden själv. AI:n har hjälpt mig med:

- att koppla ihop programmet med MySQL (NuGet-paket, `connection.txt` och `.gitignore`) och att komma igång med git och GitHub,
- att förklara SQL-saker jag inte kunde sedan tidigare, som `JOIN`, `NOT EXISTS`, `DATEDIFF` och valfria filter med `IS NULL OR`,
- att hitta fel, till exempel stavfel i kolumnnamn, klamrar och semikolon som saknades och fel citattecken i `CHECK`-villkoren,
- att rätta `PrintAvailableRooms` när jag bad om det, och att skapa sex extra testbokningar,
- att visa var och hur jag kunde göra fyra av förbättringarna (kolla att kund-id finns, bara kunna ändra och avboka egna bokningar, `CHECK` på antal personer och välja stad med en siffra), som jag sedan skrev in själv,
- att föreslå och visa koden för menyval 8 (alla rum och bokade perioder), som jag också skrev in själv,
- att rätta stavfel i texterna och några fel i den nya koden (semikolon, en klammer som saknades och felstavade kolumnnamn) när jag bad om det,
- att skriva utkast till README, den här reflektionen och kommentarerna i koden.

GitHub Copilot föreslog också en del av koden i `CreateBooking` medan jag skrev.

Jag har kontrollerat resultatet genom att bygga projektet, köra programmet och testa varje menyval mot testdatan, och genom att titta i tabellerna i DBeaver. Jag har gått igenom koden så att jag kan förklara vad varje del gör.

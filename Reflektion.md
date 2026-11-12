# Reflektion

## Datamodell

Databasen består av fyra tabeller: `Boende`, `Rum`, `Kund` och `Bokningar` (se `hotel.png`).

- Ett boende kan ha många rum. Därför har `Rum` en kolumn `boende_id` som pekar på `Boende`.
- En bokning kopplar ihop en kund med ett rum under en viss period, via `kund_id` och `rum_id`.
- Pris och hur många som får plats har jag lagt på rummet, eftersom rummen på samma hotell kan kosta olika och vara olika stora. Pool, restaurang, avstånd och betyg har jag lagt på boendet, eftersom de gäller hela hotellet.

### Varför har staden ingen egen tabell?
Jag funderade på att göra en egen tabell för orter, men valde att ha staden som en vanlig kolumn (`stad`) i `Boende`. Det är bara några få städer och de används bara när man söker. En egen tabell hade varit mer normaliserad och skyddat mot stavfel som "Goteborg", men då hade varje sökning behövt en JOIN till. För att ingen ska stava fel visar programmet vilka städer som finns innan man söker.

### Regler som skyddar datan
- Alla tabeller har `PRIMARY KEY` med `AUTO_INCREMENT`, och tabellerna hänger ihop med `FOREIGN KEY`.
- `NOT NULL` på alla kolumner som måste fyllas i.
- `CHECK` ser till att betyget är 1–5, att avstånd och pris inte är negativa, att max antal personer är minst 1 och att utcheckningen är efter incheckningen.
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

### Om något går fel
Varje menyval körs inuti en `try/catch`. Blir det ett databasfel visas ett meddelande i stället för att hela programmet kraschar. Skriver man in något ogiltigt frågar programmet igen tills svaret är okej.

## Resultat

Alla sju menyval fungerar: registrera kund, välja kund, söka lediga rum med filter och sortering, boka, visa bokningar med totalpris, ändra antal personer och avboka. Alla tester i README-tabellen är godkända.

### Det som inte blev helt som jag tänkt
- Sorteringen på betyg syns inte så tydligt med min testdata. Varje stad har bara ett hotell, och alla rum på samma hotell har samma betyg, så det finns inte så mycket att sortera.
- Man kan välja ett kund-id som inte finns. Det upptäcks först när man försöker boka, då främmande nyckeln säger stopp.

## Möjliga förbättringar

- **Bara kunna ändra och avboka sina egna bokningar.** Som det är nu kan man ändra eller avboka en annan kunds bokning om man skriver in rätt bokningsnummer. Det skulle gå att lösa genom att skicka med kund-id och lägga till `AND kund_id = @customerId` i SQL-frågan.
- **Kolla att kund-id finns** redan när man väljer kund, i stället för att det upptäcks först vid bokningen.
- **Använda en transaktion i `CreateBooking`.** Om två operatörer skulle boka samma rum exakt samtidigt kan båda hinna klara kontrollen innan någon av dem har sparat. Med en transaktion och låsning skulle det inte kunna hända.
- **Fler hotell per stad i testdatan**, så att det syns ordentligt att sorteringen på betyg fungerar.
- **Välja stad med en siffra** i stället för att behöva skriva namnet.
- **`CHECK (antal_personer > 0)` i `Bokningar`.** Just nu kontrolleras det bara i programmet, inte i databasen.
- **Enklare kolumnnamn**, utan bindestreck och å/ä/ö. Till exempel `epost` i stället för `e-post`, som måste skrivas med backticks varje gång.

## Användning av AI och annan hjälp

Under hela arbetet har jag använt Claude Code (en AI-assistent i VS Code) som en slags handledare. Jag har bett den visa var och hur saker ska göras, och sedan skrivit det mesta av koden själv. AI:n har hjälpt mig med:

- att koppla ihop programmet med MySQL (NuGet-paket, `connection.txt` och `.gitignore`) och att komma igång med git och GitHub,
- att förklara SQL-saker jag inte kunde sedan tidigare, som `JOIN`, `NOT EXISTS`, `DATEDIFF` och valfria filter med `IS NULL OR`,
- att hitta fel, till exempel stavfel i kolumnnamn, klamrar och semikolon som saknades och fel citattecken i `CHECK`-villkoren,
- att rätta `PrintAvailableRooms` när jag bad om det, och att skapa sex extra testbokningar,
- att skriva utkast till README, den här reflektionen och kommentarerna i koden.

GitHub Copilot föreslog också en del av koden i `CreateBooking` medan jag skrev.

Jag har kontrollerat resultatet genom att bygga projektet, köra programmet och testa varje menyval mot testdatan, och genom att titta i tabellerna i DBeaver. Jag har gått igenom koden så att jag kan förklara vad varje del gör.

# Projekt architektúra röviden

Ez a projekt egy Blazor Server alkalmazás, amit Tauri desktop környezetben futtatnak.

## Fő építőkockák

- UI / felhasználói réteg: a Razor oldalak és komponensek a src/Pages és src/Shared mappában találhatók. Ezek a felületet építik fel, és a szolgáltatásokon keresztül érnek el adatokat.
- Szolgáltatások: a src/Services mappában lévő osztályok tartalmazzák az üzleti logikát. Például a tranzakciók, tárcák és kategóriák kezelése ide fut.
- Adatbázis réteg: az EF Core alapú WalleeDbContext felelős a SQLite kapcsolatért és az entitások leképezéséért.
- Modellok: a src/Models mappában található entitások, mint a Wallet, Category, Transaction és ExpectedTransaction.

## Mitől mitől függ?

- A felület a szolgáltatásokra épül, nem közvetlenül az adatbázisra.
- A szolgáltatások az EF Core DbContextFactory segítségével nyitnak adatbázis kapcsolatot.
- A tranzakciók, tárcák és kategóriák közötti kapcsolatok a modellok és a DbContext beállításai alapján működnek.
- A Tauri wrapper csak a desktop futtatókörnyezetet biztosítja; a valós alkalmazáslogika továbbra is a .NET/Blazor rétegben van.

## Fő technológiák

- .NET 8 + Blazor Server
- Entity Framework Core + SQLite
- Tailwind CSS a stílusokhoz
- Tauri a desktop alkalmazás csomagolásához

## Adatfolyam

1. A felhasználó egy oldalon vagy komponensen interakcióba lép.
2. A Razor komponens egy szolgáltatást hív.
3. A szolgáltatás adatbázis műveleteket végez az EF Core segítségével.
4. Az eredmény visszajut a felületre, ahol megjelenik.

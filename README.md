# Restaurant Reservation

ASP.NET Core MVC restaurant reservation and ordering administration project.

## Local setup

1. Open `RestaurantReservation.sln` in Visual Studio.
2. Make sure SQL Server Express is running.
3. Update `RestaurantReservation/appsettings.json` if your SQL Server instance is different.
4. Run database migrations if needed.
5. Start the project from Visual Studio.

Default local accounts seeded by the app:

- `manager@e.com` / `manager@e.com`
- `staff@e.com` / `staff@e.com`

The ordering API also expects MongoDB on `mongodb://localhost:27017`.

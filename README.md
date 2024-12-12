# Dog Grooming Management API

A .NET Core Web API for managing a dog grooming business, handling appointments, customer management, and authentication.

## Technology Stack

- **Framework**: .NET 8.0
- **Database**: SQL Server 2019
- **ORM**: Entity Framework Core 9.0
- **Authentication**: JWT Bearer tokens
- **API Documentation**: Swagger/OpenAPI
- **Password Hashing**: BCrypt.NET
- **Development Environment**: Visual Studio 2022/VS Code

## Architecture & Design Patterns

- **Architecture**: Clean Architecture
- **Design Patterns**:
  - Repository Pattern (via Entity Framework)
  - Dependency Injection
  - Service Layer Pattern
  - CQRS (for appointment management)

## Key Features

- JWT-based authentication
- Appointment scheduling system
- Business hours validation
- Conflict detection
- Available time slots calculation
- Customer management
- Swagger API documentation

## Prerequisites

1. .NET 8.0 SDK
2. SQL Server 2019 or later
3. Visual Studio 2022 or VS Code
4. SQL Server Management Studio (SSMS)

## Project Setup

1. Clone the repository:

```bash
git clone https://github.com/dennis1232/DogGroomingManageApi.git
cd DogGroomingAPI
```

2. Database Setup:
   - Install SQL Server and SSMS
   - Create a new database named `DogGroomingDB`
   - Update connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=DogGroomingDB;User Id=sa;Password=YourPassword123!;Encrypt=False;TrustServerCertificate=True;"
  }
}
```

3. Apply Database Migrations:

```bash
dotnet ef database update
```

4. Configure JWT:
   Create `appsettings.Development.json`:

```json
{
  "Jwt": {
    "Key": "YourSuperLongSecretKeyThatIsAtLeast32Characters"
  }
}
```

5. Install Dependencies:

```bash
dotnet restore
```

6. Run the Application:

```bash
dotnet run
```

## API Endpoints

The API will be available at:

- HTTP: http://localhost:5035
- HTTPS: https://localhost:7099

Swagger documentation:

- http://localhost:5035/swagger
- https://localhost:7099/swagger

## Project Structure

```
DogGroomingAPI/
├── Controllers/           # API endpoints
├── Models/               # Domain models and DTOs
├── Services/             # Business logic
├── Migrations/           # EF Core migrations
└── Properties/           # Launch settings
```

## Key Components

1. **AppointmentService**: Handles appointment logic

   - Scheduling
   - Conflict detection
   - Available time slots

2. **CustomerService**: Manages customer operations

   - Authentication
   - Registration

3. **Database Context**:
   - Appointments table
   - Customers table
   - Entity configurations

## Security Features

- JWT authentication
- Password hashing with BCrypt
- CORS policy configuration
- Model validation

## Development Notes

CORS Configuration:

- Configured for frontend at http://localhost:3000

## Environment Variables

Required in `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Jwt": {
    "Key": "YourSuperLongSecretKeyThatIsAtLeast32Characters"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Your_Connection_String_Here"
  }
}
```

## Testing

The API can be tested using:

- Swagger UI
- Postman
- Integrated test client

## Common Issues & Solutions

1. Database Connection:

   - Ensure SQL Server is running
   - Verify connection string
   - Check SQL Server authentication mode

2. JWT Token:

   - Ensure key length is sufficient
   - Verify token expiration
   - Check authorization headers

3. CORS:
   - Verify frontend URL in CORS policy
   - Check HTTP/HTTPS settings
   - Confirm allowed methods and headers

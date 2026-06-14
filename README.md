# SwapShelf

SwapShelf is a full-stack book swapping platform where users can list books they own, add books they want, find possible matches, request swaps, and review other users after completed exchanges.

## Live Demo

Frontend: https://swap-shelf-ruby.vercel.app
Backend API: https://swapshelf-api.onrender.com/api

> Note: The backend is hosted on Render, so it may take a few seconds to wake up if it has been inactive.

## Project Overview

The goal of SwapShelf is to make book exchanging easier by connecting users who have books available with users who want those books. The system supports user authentication, book listings, wanted books, swap requests, matching logic, reviews, and admin management.

## Features

### User Features

* Register and login with JWT authentication
* Browse available book listings
* Add books to the catalog
* Create personal book listings
* Add wanted books
* View matching swap opportunities
* Send swap requests
* Accept, reject, cancel, or complete swaps
* Review users after completed swaps
* View trust score based on received reviews

### Admin Features

* View all users
* Ban and unban users
* View listings
* Delete inappropriate listings

## Tech Stack

### Backend

* ASP.NET Core Web API
* Entity Framework Core
* PostgreSQL
* JWT Authentication
* Repository-Service-Controller architecture
* Docker deployment on Render

### Frontend

* React
* Vite
* Axios
* CSS
* Deployed on Vercel

### Testing

* xUnit
* Moq
* EF Core InMemory
* Coverlet for code coverage

## Architecture

The backend follows a layered structure:

```text
Controller → Service → Repository → Database
```

### Main Layers

```text
Controllers
DTOs
Models
Repositories
Services
Data / AppDbContext
Migrations
```

### Main Backend Modules

* Auth
* Books
* Listings
* Wanted Books
* Matches
* Swaps
* Reviews
* Admin

## API Endpoints

### Authentication

```http
POST /api/auth/register
POST /api/auth/login
```

### Books

```http
GET  /api/books
GET  /api/books/{id}
POST /api/books
```

### Listings

```http
GET    /api/listings
GET    /api/listings/{id}
GET    /api/listings/mine
POST   /api/listings
PUT    /api/listings/{id}
DELETE /api/listings/{id}
```

### Wanted Books

```http
GET    /api/wanted
POST   /api/wanted
DELETE /api/wanted/{id}
```

### Matches

```http
GET /api/matches
```

### Swaps

```http
GET /api/swaps
GET /api/swaps/{id}
POST /api/swaps
PUT /api/swaps/{id}/accept
PUT /api/swaps/{id}/reject
PUT /api/swaps/{id}/transit
PUT /api/swaps/{id}/complete
PUT /api/swaps/{id}/cancel
```

### Reviews

```http
GET  /api/reviews/user/{userId}
POST /api/reviews
```

### Admin

```http
GET    /api/admin/users
PUT    /api/admin/users/{id}/ban
PUT    /api/admin/users/{id}/unban
GET    /api/admin/listings
DELETE /api/admin/listings/{id}
```

## Database

The project uses PostgreSQL with Entity Framework Core migrations.

Main tables:

```text
Users
Books
Listings
WantedBooks
SwapRequests
Reviews
```

## Local Backend Setup

### 1. Clone the repository

```bash
git clone https://github.com/GjikaZh/SwapShelf.git
cd SwapShelf
```

### 2. Configure backend settings

Create an `appsettings.json` file inside the backend project folder.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=SwapShelfDB;Username=postgres;Password=your_password"
  },
  "JwtSettings": {
    "Key": "your-super-secret-key-with-at-least-32-characters",
    "Issuer": "SwapShelf",
    "Audience": "SwapShelfUsers"
  },
  "AllowedOrigin": "http://localhost:5173",
  "AllowedHosts": "*"
}
```

### 3. Apply database migrations

```bash
dotnet ef database update
```

Or from Visual Studio Package Manager Console:

```powershell
Update-Database
```

### 4. Run the backend

```bash
dotnet run
```

## Local Frontend Setup

Go to the frontend folder:

```bash
cd swapshelf-ui
```

Install dependencies:

```bash
npm install
```

Create a `.env` file:

```env
VITE_API_URL=http://localhost:5000/api
```

Run the frontend:

```bash
npm run dev
```

## Deployment

### Backend Deployment

The backend is deployed on Render using Docker.

Render environment variables used:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Render PostgreSQL connection string
JwtSettings__Key=secret key
JwtSettings__Issuer=SwapShelf
JwtSettings__Audience=SwapShelfUsers
AllowedOrigin=https://swap-shelf-ruby.vercel.app
```

### Frontend Deployment

The frontend is deployed on Vercel.

Vercel environment variable:

```text
VITE_API_URL=https://swapshelf-api.onrender.com/api
```

## Testing

Run all tests:

```bash
dotnet test
```

Run tests with coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Generate a readable coverage report:

```bash
reportgenerator -reports:"SwapShelf.Tests/TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

Then open:

```text
coveragereport/index.html
```

## Example Books to List

```text
The Metamorphosis - Franz Kafka
The Eyes of Darkness - Dean Koontz
Atomic Habits - James Clear
Clean Code - Robert C. Martin
The Alchemist - Paulo Coelho
```

## Project Status

The project is fully deployed and connected:

```text
React frontend → ASP.NET Core API → PostgreSQL database
```

Users can register, login, create listings, add wanted books, find matches, request swaps, and review other users.

## Live Website

The deployed frontend application is available here:

https://swap-shelf-ruby.vercel.app/

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

# Performance Testing with k6

This folder contains performance tests for the SwapShelf backend API. The tests were created using **k6**, a performance testing tool used to simulate multiple virtual users and measure how the system behaves under load.

## Test Environment

The tests were executed against the local backend API:

```text
https://localhost:7208/api
```

The backend was run locally through Visual Studio, and k6 was executed from the terminal. The frontend UI was not required for these tests because k6 tests the backend API directly.

Since the tests were executed locally, the results are not affected by Render free-tier cold starts, network delays, or deployment limitations.

## Tool Used

```text
k6
```

k6 was used to measure:

```text
response time
failed requests
successful checks
number of virtual users
request throughput
system stability under load
```

## Test Files

### 1. Books Load Test

File:

```text
books-load-test.js
```

Purpose:

This test checks the performance of the books endpoint under normal load.

Endpoint tested:

```http
GET /api/books
```

Configuration:

```text
10 virtual users
30 seconds
```

Result summary:

```text
300 requests
0 failed requests
100% checks passed
Average response time: 28.18ms
p95 response time: 41.2ms
```

Conclusion:

The books endpoint performed successfully under normal concurrent load.

---

### 2. Listings Load Test

File:

```text
listings-load-test.js
```

Purpose:

This test checks the performance of the listings endpoint, which is one of the main user-facing endpoints in the application.

Endpoint tested:

```http
GET /api/listings
```

Configuration:

```text
20 virtual users
30 seconds
```

Result summary:

```text
580 requests
0 failed requests
100% checks passed
Average response time: 38.91ms
p95 response time: 123.34ms
```

Conclusion:

The listings endpoint remained stable and responsive under medium concurrent load.

---

### 3. Browse Scenario Test

File:

```text
browse-scenario-test.js
```

Purpose:

This test simulates a realistic user browsing flow by calling multiple public endpoints.

Endpoints tested:

```http
GET /api/books
GET /api/listings
```

Configuration:

```text
15 virtual users
45 seconds
```

Result summary:

```text
678 HTTP requests
0 failed requests
100% checks passed
Average response time: 20.91ms
p95 response time: 51.63ms
```

Conclusion:

The public browsing flow remained stable and responsive under concurrent user activity.

---

### 4. Stress Test

File:

```text
stress-test.js
```

Purpose:

This test gradually increases the number of users to check how the system behaves under heavier load.

Endpoint tested:

```http
GET /api/listings
```

Configuration:

```text
10 virtual users
25 virtual users
50 virtual users
then ramp down to 0
```

Result summary:

```text
1696 requests
0 failed requests
100% checks passed
Average response time: 10.56ms
p95 response time: 22.29ms
```

Conclusion:

The tested endpoint remained stable under increased concurrent load up to 50 virtual users.

---

### 5. Authentication/Register Performance Test

File:

```text
auth-performance-test.js
```

Purpose:

This test checks the performance of a POST/write operation by registering multiple users with unique email addresses.

Endpoint tested:

```http
POST /api/auth/register
```

Configuration:

```text
5 virtual users
30 seconds
```

Result summary:

```text
130 registration requests
0 failed requests
100% checks passed
Average response time: 175.76ms
p95 response time: 178.1ms
```

Conclusion:

The registration endpoint successfully handled concurrent write requests within the expected response time.

---

### 6. Breakpoint Test

File:

```text
breakpoint-test.js
```

Purpose:

This test was created to find the approximate edge point where the system starts failing or becoming too slow.

Endpoint tested:

```http
GET /api/listings
```

Configuration:

```text
Gradually increased load up to 200 virtual users
```

Breaking condition:

```text
Failed HTTP requests
or response time above 2000ms
```

Result summary:

```text
11,829 requests
0 failed requests
100% checks passed
Average response time: 19.28ms
p95 response time: 49.34ms
Maximum response time: 190.93ms
```

Conclusion:

The system did not reach the breaking point during the test. Under the local testing environment, the tested endpoint remained stable up to 200 virtual users.

## How to Run the Tests

First, make sure the backend API is running locally:

```text
https://localhost:7208/swagger/index.html
```

Then open a terminal inside the `performance-tests` folder.

Run each test using:

```powershell
k6 run --insecure-skip-tls-verify .\books-load-test.js
```

```powershell
k6 run --insecure-skip-tls-verify .\listings-load-test.js
```

```powershell
k6 run --insecure-skip-tls-verify .\browse-scenario-test.js
```

```powershell
k6 run --insecure-skip-tls-verify .\stress-test.js
```

```powershell
k6 run --insecure-skip-tls-verify .\auth-performance-test.js
```

```powershell
k6 run --insecure-skip-tls-verify .\breakpoint-test.js
```

The `--insecure-skip-tls-verify` option is used because the tests run against a local HTTPS development certificate.

## Overall Conclusion

The performance testing results show that the SwapShelf backend API remained stable under the selected local test conditions. The tested endpoints handled normal load, medium load, realistic browsing flow, stress load, authentication requests, and breakpoint testing without failed requests.

The breakpoint test attempted to find the system limit by increasing the load up to 200 virtual users. No breaking point was reached, since all requests succeeded and response times stayed below the defined threshold. This indicates that the tested backend endpoints performed efficiently in the local environment.


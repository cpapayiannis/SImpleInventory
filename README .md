# 🛒 SimpleInventory

A sample **Inventory Management** system built with **ASP.NET Core 8, EF Core, MVC, and Web API**.

---

## ✨ Features

- **MVC UI** for Products & Categories  
  - Products: List, Search, Filter, Paging, Sorting, Create/Edit/Delete with validation  
  - Categories: List, Create, Delete (blocked if products exist)  
- **RESTful API** under `/api/*`  
  - Products: full CRUD, search/filter/paging  
  - Categories: list + create  
- **Authentication**  
  - **MVC UI**: Cookie login with a hardcoded demo user  
  - **API**: JWT Bearer tokens required  
- **Validation Rules**  
  - Unique SKU (3–32 chars)  
  - Name required  
  - Price ≥ 0  
  - Quantity ≥ 0  
  - Category required  
- **Constraints**  
  - Cannot delete a category if products exist (409 Conflict)  
- **Extras**  
  - AutoMapper DTO mapping  
  - Rate limiting on API write endpoints  
  - AJAX search in Products list (live filtering without reload)  

---

## 🚀 Getting Started

### 1. Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- PowerShell (or Bash)
- (Optional) [SQLite tools](https://www.sqlite.org/download.html) to inspect the database.

---

### 2. Database Setup
Apply migrations and create the database:

```powershell
dotnet ef database update --project src/SimpleInventory.DataAccess
```

By default, a SQLite file `SimpleInventory.db` is created in the `AppData` folder of the UI project.

---

### 3. Authentication

#### MVC UI (cookie login)
Use the hardcoded demo account:

- **Email:** `admin@example.com`  
- **Password:** `Password!123`

After login, you can access Create/Edit/Delete pages for Products and Categories.

#### API (JWT)
To create or update data via API, you must supply a **JWT token** in the `Authorization` header.

Generate one by running the included test:

```powershell
dotnet test -l "console;verbosity=detailed" --filter "FullyQualifiedName~JwtTests.GenerateToken_ShouldReturn_ValidJwt"
```

Look for a line like:
```
Generated JWT: eyJhbGciOiJIUzI1NiIsInR5cCI6...
```

Copy the token and include it in API requests:
```
Authorization: Bearer {your_token_here}
```

---

### 4. Run the Apps

#### Run UI (MVC)
```powershell
dotnet run --project src/SimpleInventory
```
Access at: [https://localhost:44329](https://localhost:44329) (IIS Express default).

#### Run API
```powershell
dotnet run --project src/SimpleInventory.Api
```
Swagger available at: [https://localhost:7158/swagger](https://localhost:7158/swagger)

---

### 5. Example API Requests

#### GET products (search + paging)
```http
GET https://localhost:7158/api/products?q=monitor&page=1&pageSize=10
```

#### POST new product
```http
POST https://localhost:7158/api/products
Authorization: Bearer {your_token}
Content-Type: application/json

{
  "sku": "MON-001",
  "name": "27-inch Monitor",
  "price": 199.99,
  "quantity": 5,
  "categoryId": 1,
  "updatedAt": "2025-01-01T00:00:00Z"
}
```

#### GET categories
```http
GET https://localhost:7158/api/categories
```

#### POST new category
```http
POST https://localhost:7158/api/categories
Authorization: Bearer {your_token}
Content-Type: application/json

{
  "name": "Monitors"
}
```

---

## 🧪 Tests

Run all tests:

```powershell
dotnet test
```

Unit tests include:
- Product creation validation (duplicate SKU, negative price)
- Filtering/search logic (name, SKU, category, paging, sorting)
- Category deletion rule (blocked if products exist)

---

## 📂 Project Structure

```
src/
  SimpleInventory/           # MVC UI (cookie login, razor views)
  SimpleInventory.Api/       # Web API (JWT, Swagger, rate limiting)
  SimpleInventory.DataAccess # EF Core DbContext + Repository
  SimpleInventory.Common/    # Domain models & shared classes
tests/
  SimpleInventory.Tests/     # MSTest unit tests
```

---

## 📝 License
MIT (for demo/educational use)

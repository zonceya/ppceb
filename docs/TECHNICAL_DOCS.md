# PPECB ASSESSMENT - TECHNICAL DOCUMENTATION

## ARCHITECTURE OVERVIEW

This project follows **N-Tier Architecture** with 5 distinct layers:

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **Presentation Layer** | PPECB (MVC) | User interface, Razor Views |
| **API Layer** | PPECB.API | RESTful endpoints, JWT authentication |
| **Service Layer** | PPECB.Services | Business logic, validation, Excel |
| **Data Access Layer** | PPECB.Data | Entity Framework Core, migrations |
| **Domain Layer** | PPECB.Domain | Core entities, interfaces |

## SOLID PRINCIPLES APPLIED

| Principle | Implementation |
|-----------|----------------|
| **Single Responsibility** | Each service handles one concern |
| **Open/Closed** | Interfaces allow extension |
| **Liskov Substitution** | DTOs replace entities in API |
| **Interface Segregation** | Small, focused interfaces |
| **Dependency Inversion** | Depends on abstractions |

## DESIGN PATTERNS USED

| Pattern | Location | Purpose |
|---------|----------|---------|
| **Repository** | Entity Framework Core | Data access abstraction |
| **Dependency Injection** | Program.cs | Loose coupling |
| **DTO Pattern** | API/DTOs | Prevent circular references |
| **Factory** | ProductCodeGenerator | Auto-generate codes |
| **Strategy** | CategoryCodeValidator | Validation logic |

## AUTHENTICATION FLOW

### MVC (Cookie-based)
1. User submits login → Identity validates → Cookie created → Subsequent requests include cookie

### API (JWT Token)
1. User POSTs to /api/auth/login → JWT token generated → Client includes token in Authorization header

## DATABASE TABLES

- **AspNetUsers** - User accounts
- **Categories** - Product categories (user-specific)
- **Products** - Products linked to categories

## AUDIT FIELDS

All user-created tables include:
- CreatedBy, CreatedDate, UpdatedBy, UpdatedDate

## SECURITY MEASURES

| Area | Implementation |
|------|----------------|
| Authentication | Cookie (MVC) + JWT (API) |
| Authorization | [Authorize] + UserId filtering |
| Data Isolation | All queries filter by UserId |

## PERFORMANCE OPTIMIZATIONS

| Optimization | Benefit |
|--------------|---------|
| AsNoTracking() | 30-50% faster reads |
| Pagination | 10 items per page |
| Database indexes | Faster queries |

## EXCEL IMPORT/EXPORT

- Library: EPPlus 6.2.10
- Features: Import, Export, Template download

## API ENDPOINTS

| Method | Endpoint | Auth |
|--------|----------|------|
| POST | /api/auth/register | None |
| POST | /api/auth/login | None |
| GET | /api/Categories | JWT |
| GET | /api/Products | JWT |
| POST | /api/Products | JWT |
| PUT | /api/Products/{id} | JWT |
| DELETE | /api/Products/{id} | JWT |

## TECHNOLOGY STACK

| Component | Technology | Version |
|-----------|------------|---------|
| Framework | .NET | 8.0 (LTS) |
| ORM | Entity Framework Core | 8.0 |
| Database | SQL Server LocalDB | 2022 |
| API Auth | JWT Bearer | 8.0 |
| Excel | EPPlus | 6.2.10 |
| Frontend | Bootstrap | 5.3 |
| API Docs | Swashbuckle | 6.5.0 |

## DATA VALIDATION RULES

| Field | Rule |
|-------|------|
| Category Code | 3 letters + 3 numbers (ABC123) |
| Product Code | Auto-generated (yyyyMM-###) |
| Price | Positive decimal |
| Email | Unique, valid format |

## CONCLUSION

The PPECB Assessment successfully implements all required features:
- User Authentication (MVC Cookie + API JWT)
- Category and Product Management
- Excel Import/Export
- Image Upload
- N-Tier Architecture
- SOLID Principles
- Comprehensive Documentation

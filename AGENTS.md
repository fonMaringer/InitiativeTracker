# AGENTS.md

## 1. Tech Stack & Project Environment

- **Runtime:** .NET 10.0 (`net10.0`)
- **Language:** C# 14 (LangVersion: `latest`)
- **Framework:** ASP.NET Core Blazor Server with interactive server render mode (`AddInteractiveServerComponents()`, `AddInteractiveServerRenderMode()`)
- **UI Library:** Blazor.Bootstrap 3.5.0
- **Data Access:** EF Core 10.0.9 with SQLite provider (`Microsoft.EntityFrameworkCore.Sqlite` 10.0.9)
- **Logging:** Serilog (Console + File sinks) via `Serilog.Extensions.Logging` 10.0.0 and `Serilog.Settings.Configuration` 10.0.0
- **JS Interop:** `Microsoft.JSInterop` 10.0.9
- **Dependency Injection:** Built-in MS.DI (`Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.8)
- **Testing:** NUnit 4.5.1, FluentAssertions 8.0.0, NSubstitute 5.3.0, `coverlet.collector` 8.0.0, EF Core InMemory 10.0.9
- **Solution file:** `InitiativeTracker.slnx`

### Architecture Pattern
**Layered architecture (single-project monolith)** within `InitiativeTracker/`:
- `Domain/Entities/` — EF Core entity classes and domain models (`InitiativeEntity`, `MiniatureEntity`, `ItemEntity`, `SpellEntity`, `InitiativeListItem`)
- `Domain/Enums/` — enumerations (`CreatureSize`, `ItemRarity`, `SpellClass`, `Source`)
- `Application/` — business-logic service classes with interfaces, DTOs, and HTML print generators
- `Infrastructure/Database/` — EF Core DbContext and entity configurations
- `Infrastructure/Extensions/` — DI registration extensions using C# 14 extension everything syntax
- `Infrastructure/Options/` — typed configuration options classes
- `Integration/RestClients/TtgClub/` — external HTTP API clients against ttg.club bestiary API
- `Components/Pages/`, `Components/Layout/` — Blazor UI pages and layouts

---

## 2. Core Repository Commands

```bash
dotnet restore
dotnet build
dotnet run --project InitiativeTracker/InitiativeTracker.csproj
dotnet test
```

To run tests with coverage:
```bash
dotnet test /p:CollectCoverage=true
```

To apply EF Core migrations (when needed):
```bash
dotnet ef database update --project InitiativeTracker/InitiativeTracker.csproj --startup-project InitiativeTracker/InitiativeTracker.csproj
```

---

## 3. C# 14 Code Style Rules

### Syntax Requirements
- **ALWAYS** use file-scoped namespaces (`namespace X;`) — never block-scoped namespaces
- **ALWAYS** use primary constructors on classes and records where applicable
- **ALWAYS** use collection expressions (`[a, b, c]`) instead of `new List<T> { a, b, c }` or `new[] { ... }`
- **ALWAYS** use the `field` identifier for auto-property initializers with expressions
- **LEVERAGE** C# 14 extension everything syntax for static helper classes extending existing types — as demonstrated in `DiExtensions.cs` using `extension(Type parameter)` primary constructor form
- **ALWAYS** use `nameof()` for member, type, and parameter names instead of string literals
- **PREFER** pattern matching (`is`, `switch` expressions) over casts and `if/else` chains
- **PREFER** `not`, `and`, `or` compound patterns

### Nullable Reference Types
- `<Nullable>enable</Nullable>` is set project-wide — **NEVER** disable nullable context
- **ALWAYS** annotate parameters and return types with nullability (`string?`, `Task<T?>`)
- **ALWAYS** handle nullable returns before dereferencing — use null-forgiving operator (`!`) only when you can prove non-null at runtime, and document why
- **PREFER** the null-conditional operator (`?.`) and null-coalescing (`??`) over explicit `if (x != null)` checks

### Asynchrony
- **ALWAYS** use async/await for I/O-bound operations throughout the SignalR call chain
- **NEVER** use `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` — this will deadlock Blazor Server's synchronization context
- **ALWAYS** propagate `CancellationToken` from components down through service calls to the data access layer

### Naming Conventions
- Service interfaces: `I<ServiceName>` (e.g., `IInitiativeService`)
- Service implementations: `<ServiceName>` (e.g., `InitiativeService`)
- DTOs: suffix `Dto` (e.g., `MiniatureCreateDto`)
- Entity configurations: suffix `Configuration` (e.g., `MiniatureEntityConfiguration`)

---

## 4. Project-Specific Blazor Server Rules

### Dependency Injection Lifetimes
- **NEVER** register per-user stateful services as `Singleton`. User data held in singleton lifetime will be shared across all connected users and cause cross-session corruption
- **SCOPE** interactive UI state to the circuit — use `Scoped` or inject directly into `.razor` components that use `@rendermode InteractiveServer`
- The current codebase registers services as `Singleton` because initiative state is application-wide for a single-user DM tool. If multi-seat support is added in the future, **ALL** these services must be re-evaluated and likely moved to `Scoped`
- DbContext registered as `Singleton` — acceptable only because SQLite with careful transaction handling prevents concurrency conflicts. EF Core recommends `Scoped`. Do NOT change this without a confirmed migration plan

### Memory Leak Prevention
- **ALWAYS** implement `IAsyncDisposable` on Blazor components that subscribe to events, hold JS interop references (`IJSObjectReference`), or open streams
- **ALWAYS** dispose `IJSObjectReference` in component lifecycle via `IAsyncDisposable`
- **NEVER** forget to unsubscribe from custom event handlers or `CancellationToken.Register()` callbacks in component lifetimes

### Component Structure
- **ALWAYS** split Blazor components using partial classes: `.razor` for markup, `.razor.cs` for C# logic (parameters, event handlers, state)
- **ISOLATE** component-specific CSS in `.razor.css` style isolation files — do NOT put component styles in global `app.css`
- Use Blazor.Bootstrap components already referenced in the project
- **ALWAYS** use `@inject` for DI dependencies in `.razor` files only when the dependency is trivial. For non-trivial services, inject via the code-behind `.razor.cs` with primary constructor

### Rendering and Lifecycle
- Components must be marked with interactive render mode where state changes need UI updates: `@rendermode InteractiveServer`
- **NEVER** perform long-running synchronous work in component event handlers on the SignalR thread — this blocks the entire circuit
- Use `InvokeAsync` for cross-thread UI updates from background tasks

---

## 5. DB & Data Access Rules

### DbContext Usage
- The project uses EF Core 10.0.9 with SQLite via `InitiativeTrackerDbContext`
- **ALWAYS** use async EF Core methods (`ToListAsync()`, `SaveChangesAsync()`, `FindAsync()`) — **NEVER** use synchronous equivalents (`ToList()`, `SaveChanges()`, `First()` on queries) to avoid blocking the SignalR thread with thread-pool starvation
- **NEVER** hold a DbContext instance beyond a single logical unit of work
- **ALWAYS** wrap save operations in try/catch at the service layer and log errors with Serilog's structured logging: `logger.LogError(ex, "Message {Context}")`

### Migrations and Schema
- Use EF Core migrations for schema changes going forward
- **NEVER** modify entity classes without considering migration impact

### Connection String
- Read from `appsettings.json` section `"ConnectionStrings:Default"` — format: `"Data Source=initiativetracker.db"`
- **NEVER** hard-code connection strings or database paths in source code

---

## 6. AI Constraints & Forbidden Actions

1. **NEVER use deprecated C# syntax.** Do NOT introduce block-scoped namespaces, `new List<T> { }` initializers where collection expressions `[ ]` work, or verbose constructor patterns when primary constructors are available
2. **NEVER break the existing folder structure.** The layers (`Domain/`, `Application/`, `Infrastructure/`, `Components/`, `Integration/`) must be preserved. New code goes into the appropriate layer
3. **NEVER perform synchronous I/O in Blazor Server event handlers or anywhere in the SignalR request chain.** All database calls, file system operations, and HTTP requests must be fully async to prevent circuit thread blocking
4. **NEVER commit secrets, API keys, or personal data.** The ttg.club API keys and any configuration values belong in `appsettings.Development.json` (which is gitignored) or environment variables
5. **NEVER change DI lifetimes of existing services without explicit confirmation from the project owner.** Current singleton registrations are intentional for this single-user DM tool architecture. Changing to Scoped without understanding the initiative state-sharing model will break functionality

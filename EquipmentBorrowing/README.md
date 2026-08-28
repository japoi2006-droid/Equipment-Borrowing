# Campus Equipment Borrowing System

ITSD 81 — Desktop Application Development
Laboratory Activity 1: *From Requirements to Application Structure*

This activity does **not** build a finished system. It builds the
architectural skeleton — domain models, application services, and
repository abstractions — that a future desktop UI and database will be
plugged into.

The requirements analysis performed *before* any code was written lives in
[`docs/ANALYSIS.md`](docs/ANALYSIS.md) (Part A: actors, use cases, domain
concepts).

---

## 1. Solution Structure

```text
EquipmentBorrowing/
├── EquipmentBorrowing.sln
├── docs/
│   └── ANALYSIS.md                 (Part A requirements analysis)
├── src/
│   ├── EquipmentBorrowing.Domain/          (concepts + rules of the problem itself)
│   ├── EquipmentBorrowing.Application/     (use cases / orchestration)
│   ├── EquipmentBorrowing.Infrastructure/  (in-memory storage today; DB later)
│   └── EquipmentBorrowing.ConsoleDemo/     (the "executable" that demonstrates the flow)
└── tests/
    └── EquipmentBorrowing.Tests/           (automated tests for the application service)
```

**Domain** — `Student`, `Equipment`, `Borrowing`, `BorrowingStatus`. Contains
only the concepts and rules that belong to the problem itself (e.g.
equipment cannot be marked borrowed twice). Has no dependency on anything
else in the solution.

**Application** — `IStudentRepository`, `IEquipmentRepository`,
`IBorrowingRepository` (abstractions), and `BorrowEquipmentService` (the one
use case implemented in this lab). This layer coordinates Domain objects and
repositories to perform an operation; it contains no SQL, no file I/O, and
no UI code.

**Infrastructure** — `InMemoryStudentRepository`,
`InMemoryEquipmentRepository`, `InMemoryBorrowingRepository`. Concrete,
swappable implementations of the Application layer's repository interfaces.
Today they use `Dictionary`/`List` in memory; later this project (and only
this project) would gain a SQLite- or PostgreSQL-backed implementation.

**ConsoleDemo** — a minimal executable that plays the role of "Executable /
Future UI" in the dependency diagram below. It wires the in-memory
repositories to `BorrowEquipmentService` and runs one successful and three
failing borrow attempts, printing the outcome of each.

**Tests** — `BorrowEquipmentServiceTests`, an xUnit project that exercises
`BorrowEquipmentService` against in-memory repositories: one success case
and one test per failure rule.

---

## 2. Dependency Direction

```text
        EquipmentBorrowing.ConsoleDemo
        (Executable / Future Avalonia UI)
                    │
                    ▼
        EquipmentBorrowing.Application
                    │      ▲
                    ▼      │
        EquipmentBorrowing.Domain
                           │
        EquipmentBorrowing.Infrastructure
```

- **ConsoleDemo** depends on **Application** (to call the use case),
  **Infrastructure** (to construct concrete repositories at startup), and
  **Domain** (to build seed data).
- **Application** depends only on **Domain**. It defines repository
  *interfaces* but does not depend on any concrete repository.
- **Infrastructure** depends on **Application** (to implement its
  interfaces) and **Domain** (to store domain objects).
- **Domain** depends on nothing else in the solution.
- **Tests** depends on all three (`Domain`, `Application`, `Infrastructure`)
  because it needs real in-memory repositories to exercise the service
  end-to-end.

The important arrow is the one **Infrastructure → Application**: the
outer, technology-specific layer depends inward on the abstraction, never
the other way around. `BorrowEquipmentService` never mentions
`InMemoryEquipmentRepository` by name — only `IEquipmentRepository`.

---

## 3. Use Case Mapping

```text
Actor: Student
Use Case: Borrow Equipment
Application Service: BorrowEquipmentService.ExecuteAsync
Domain Objects Used: Student, Equipment, Borrowing, BorrowingStatus
Repository Interfaces Used: IStudentRepository, IEquipmentRepository, IBorrowingRepository
Infrastructure Implementations Used: InMemoryStudentRepository, InMemoryEquipmentRepository, InMemoryBorrowingRepository
```

Flow: `ConsoleDemo` builds the in-memory repositories and passes them into
`BorrowEquipmentService`'s constructor → the demo calls
`ExecuteAsync(request)` → the service checks, in order, that the student
exists and is allowed to borrow, the equipment exists and is available, and
the active-borrowing limit is not exceeded → on success it mutates
`Equipment` to unavailable, creates a `Borrowing`, and persists both through
the repository interfaces.

---

## 4. Reflection

**1. Why should the application service depend on a repository interface
instead of directly depending on a database implementation?**
Because the *use case* ("check the rules, create a borrowing") and the
*storage technology* are two different concerns that change for different
reasons. If `BorrowEquipmentService` referenced `InMemoryEquipmentRepository`
or a SQL class directly, swapping storage later (or writing a unit test)
would mean rewriting the service. Depending on `IEquipmentRepository`
instead means the service's logic is fixed and testable, while the storage
underneath can be replaced freely.

**2. Which parts of your current solution could remain unchanged if SQLite
were added later?**
`EquipmentBorrowing.Domain` and `EquipmentBorrowing.Application` in their
entirety — including `BorrowEquipmentService` itself and all three
repository interfaces. Only `EquipmentBorrowing.Infrastructure` would
change: the `InMemory...Repository` classes would be replaced (or
supplemented) with SQLite-backed classes that implement the exact same
interfaces.

**3. Which project would eventually contain Avalonia Views?**
A new UI project sitting where `EquipmentBorrowing.ConsoleDemo` sits today —
depending on `Application` (and `Domain`), and wired up to concrete
`Infrastructure` repositories at startup, exactly the way `ConsoleDemo`
is wired up now.

**4. Should an Avalonia button directly execute database queries? Why or
why not?**
No. A button's click handler should call an **Application service**
(like `BorrowEquipmentService.ExecuteAsync`), the same way `ConsoleDemo`
does. If the button executed SQL directly, the borrowing rules (student
eligibility, equipment availability, active-loan limit) would either have
to be duplicated in the UI layer or skipped entirely, and the UI would
become impossible to test without a real database and a real window.

**5. What part of your implementation represents the actual business
operation requested by the actor?**
`BorrowEquipmentService.ExecuteAsync` in the Application layer. Everything
else exists to support it: `Domain` supplies the concepts and rules it
coordinates, `Infrastructure` supplies where its data actually lives, and
`ConsoleDemo`/`Tests` are simply two different callers of that same method.

> These are our starting answers — both partners should be able to explain
> each one in their own words before submitting, since understanding the
> "why" is graded, not just the working code.

---

## 5. Running the Project

```bash
# Build everything except the test project (Domain/Application/Infrastructure/ConsoleDemo need no NuGet packages)
dotnet build src/EquipmentBorrowing.Domain/EquipmentBorrowing.Domain.csproj
dotnet build src/EquipmentBorrowing.Application/EquipmentBorrowing.Application.csproj
dotnet build src/EquipmentBorrowing.Infrastructure/EquipmentBorrowing.Infrastructure.csproj
dotnet build src/EquipmentBorrowing.ConsoleDemo/EquipmentBorrowing.ConsoleDemo.csproj

# Run the demonstration (1 success case + 3 failure cases)
dotnet run --project src/EquipmentBorrowing.ConsoleDemo/EquipmentBorrowing.ConsoleDemo.csproj

# Restore + run the automated tests (needs internet access to NuGet the first time)
dotnet test tests/EquipmentBorrowing.Tests/EquipmentBorrowing.Tests.csproj

# Or build/test the whole solution at once
dotnet build EquipmentBorrowing.sln
dotnet test EquipmentBorrowing.sln
```

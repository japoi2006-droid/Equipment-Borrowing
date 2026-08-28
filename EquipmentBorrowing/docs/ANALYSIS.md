# Part A — Requirements Analysis

This document was written **before** any C# code, as required by the lab.
It records how we read the scenario and turned it into actors, use cases,
and domain concepts.

---

## A. Actors

| Actor | What they expect from the system |
|---|---|
| **Student** (primary actor) | To request to borrow a specific piece of available equipment, be told immediately and clearly if the request cannot be approved (and why), and to have their borrowing recorded so they can return it later. |
| **Laboratory Staff** (secondary / implied actor) | To trust that the system enforces borrowing rules consistently (no double-borrowing, no bypassing the active-loan limit) and to have an accurate, up-to-date record of what equipment is out and who has it. |

> Note: the scenario only explicitly names the *student* as the one who
> "requests to borrow equipment." It does not name who operates the
> counter or records a return. We inferred **Laboratory Staff** as a
> secondary actor because someone has to trigger "equipment is returned"
> in a real deployment. We flagged this as an assumption rather than
> treating it as stated fact.

---

## B. Use Cases

We identified three major use cases. These map directly to the example
Application-layer operation names given in the lab handout
(`BorrowEquipment`, `ReturnEquipment`, `FindAvailableEquipment`).

### Use Case 1

| Item | Description |
|---|---|
| Use Case | Borrow Equipment |
| Primary Actor | Student |
| Preconditions | The student and the equipment already exist as records in the system. |
| Main Action | The student requests to borrow a specific, identified piece of equipment. |
| Expected Result | A new `Borrowing` is created with status `Active`; the equipment becomes unavailable to anyone else. |
| Possible Failure | Student does not exist; student is not allowed to borrow; equipment does not exist; equipment is already unavailable; student already has the maximum number of active borrowings. |

### Use Case 2

| Item | Description |
|---|---|
| Use Case | Return Equipment |
| Primary Actor | Student (return is physically handed back; recorded by Laboratory Staff) |
| Preconditions | An `Active` borrowing exists linking this student to this equipment. |
| Main Action | The system is told that a specific active borrowing is being returned. |
| Expected Result | The `Borrowing` status changes to `Returned`; the `Equipment` becomes available again. |
| Possible Failure | No matching active borrowing exists for that student/equipment pair; the borrowing was already marked as returned. |

### Use Case 3

| Item | Description |
|---|---|
| Use Case | Find Available Equipment |
| Primary Actor | Student |
| Preconditions | None — the equipment catalog exists (may be empty). |
| Main Action | The student asks to see which equipment is currently available before deciding what to request. |
| Expected Result | The system returns the list of equipment where `IsAvailable = true`. |
| Possible Failure | Not really a failure case — an empty list is a valid, correct result if nothing is currently available. |

> **Scope note:** Per Part E of the lab, only **Borrow Equipment** is fully
> implemented in this activity (`BorrowEquipmentService`). Return Equipment
> and Find Available Equipment are analyzed here so the design accounts for
> them, but their application services are intentionally **not** built yet —
> building them now would go beyond what this lab asks for, and the
> repository-interface rule in Part D says not to add methods before an
> operation actually needs them.

---

## C. Domain Concepts

### Student

| Question | Answer |
|---|---|
| Must contain | Identity (`Id`), display `Name`, and whether the student currently `IsAllowedToBorrow`. |
| Rules/state it owns | Whether borrowing privileges are currently suspended or active. |
| **Not** its responsibility | Knowing which equipment it has borrowed, or how many *active* borrowings it currently has. That information only exists as `Borrowing` records that live outside this object; counting them requires querying a repository, which is an **Application**-layer job, not something `Student` should do to itself. |

### Equipment

| Question | Answer |
|---|---|
| Must contain | Identity (`Id`), `Name`, and current `IsAvailable` state. |
| Rules/state it owns | The transition rule that equipment cannot be marked borrowed while it is already unavailable — this rule and the state it protects live in the same class. |
| **Not** its responsibility | Knowing *who* borrowed it, for how long, or whether that borrower was even eligible. Those are cross-entity concerns that require looking at `Student` and `Borrowing` together, so they belong to the Application layer. |

### Borrowing

| Question | Answer |
|---|---|
| Must contain | Its own identity, the `StudentId` and `EquipmentId` it links, `DateBorrowed`, `ExpectedReturnDate`, and current `Status`. |
| Rules/state it owns | That the expected return date cannot precede the borrow date, and that a `Returned` borrowing cannot be returned a second time. |
| **Not** its responsibility | Deciding *whether* the student was allowed to borrow or the equipment was available — those checks must happen *before* a `Borrowing` is even created, and they need data from other repositories, so they belong to the Application service (`BorrowEquipmentService`), not to `Borrowing` itself. |

These boundaries are what shaped the class designs in
`src/EquipmentBorrowing.Domain`.

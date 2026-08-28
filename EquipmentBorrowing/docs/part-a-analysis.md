# Part A — Analysis of the Campus Equipment Borrowing System

## A. Actors

| Actor | What they expect the system to do |
|---|---|
| **Student** (primary actor) | Wants to request a specific piece of equipment and be told immediately whether the request is approved; expects the system to check their own eligibility, the equipment's existence and availability, and their current borrowing count fairly and consistently. When returning equipment, expects the system to close out their borrowing record and make the item available to others. |
| **Laboratory Staff** (implied secondary actor, out of scope for this activity) | The scenario assumes equipment records already exist and that a student's "allowed to borrow" status is already known. In a complete system, laboratory staff would be the actor who registers equipment and suspends/reinstates students. No use case for this actor is implemented in this lab activity — only the data their actions would produce (an `Equipment` record, a `Student.IsAllowedToBorrow` flag) is modeled. |

The system itself does not act as an actor — it is the thing being described.
The rules it applies (checking eligibility, availability, and borrowing
limits) are automatic reactions to the Student's requests, not actions of a
separate human actor.

## B. Use Cases

### Use Case 1

| Item | Description |
|---|---|
| Use Case | Borrow Equipment |
| Primary Actor | Student |
| Preconditions | The student is a recognized student record in the system; the requested equipment is a recognized equipment record. |
| Main Action | The student requests to borrow a specific piece of equipment. The system checks that the student is allowed to borrow, that the equipment exists and is available, and that the student has not reached the maximum number of active borrowings. |
| Expected Result | A new `Borrowing` record is created with status `Active`; the equipment's status changes to unavailable. |
| Possible Failure | The student does not exist; the student is not allowed to borrow; the equipment does not exist; the equipment is already borrowed; the student already has the maximum number of active borrowings. |

### Use Case 2

| Item | Description |
|---|---|
| Use Case | Return Equipment |
| Primary Actor | Student |
| Preconditions | An `Active` borrowing exists linking this student to this equipment. |
| Main Action | The student returns the equipment. The system locates the matching active borrowing, marks it `Returned`, and marks the equipment available again. |
| Expected Result | The borrowing's status changes to `Returned` (with a recorded return date); the equipment becomes available for the next request. |
| Possible Failure | No active borrowing exists for that student/equipment combination (e.g., wrong equipment specified, or it was already returned). |

### Use Case 3

| Item | Description |
|---|---|
| Use Case | Find Available Equipment |
| Primary Actor | Student |
| Preconditions | The equipment catalog contains at least one registered item. |
| Main Action | The student asks to see which equipment can currently be borrowed. The system filters the equipment catalog to items that are not currently borrowed. |
| Expected Result | The student receives a list of equipment currently marked `Available`. |
| Possible Failure | No equipment is currently available — the system returns an empty list rather than an error, since this is a valid (if unhelpful) outcome. |

*(This lab activity implements Use Case 1 and Use Case 2 as application
services; Use Case 3 is supported by `IEquipmentRepository.GetAvailableAsync`,
which exists specifically because this use case needs it, even though no
dedicated `FindAvailableEquipmentService` class was created — the repository
method alone is enough for such a simple, read-only query.)*

## C. Domain Concepts

### Student

1. **Must contain:** an identity (`Id`), a display name, and whether the
   student is currently allowed to borrow equipment.
2. **Rules/state it owns:** the "allowed to borrow" flag and its transitions
   (`Suspend()` / `Reinstate()`); the rule for whether one more borrowing
   would exceed the allowed maximum (`HasReachedBorrowingLimit`), expressed
   against a count *supplied* to it.
3. **Not its responsibility:** tracking which specific items it currently has
   borrowed (that list is derived from `Borrowing` records via the
   repository); deciding whether a particular piece of equipment exists or is
   available; anything related to persistence.

### Equipment

1. **Must contain:** an identity (`Id`), a display name, and its current
   status (available or borrowed).
2. **Rules/state it owns:** the valid transitions between `Available` and
   `Borrowed` (`MarkAsBorrowed()` throws if it is already borrowed;
   `MarkAsAvailable()` returns it to the pool).
3. **Not its responsibility:** knowing which student currently holds it, or
   until when (that belongs to `Borrowing`); deciding whether a given student
   is allowed to borrow it; anything related to persistence.

### Borrowing

1. **Must contain:** the student and equipment involved (by id), the date
   borrowed, the expected return date, the actual return date (once
   returned), and its status (`Active`/`Returned`).
2. **Rules/state it owns:** the one-way transition from `Active` to
   `Returned` (`MarkAsReturned()` throws if already returned); the invariant
   that the expected/actual return date cannot precede the borrow date; its
   own identity assignment once persisted (`AssignId()`).
3. **Not its responsibility:** deciding *whether* a borrowing should be
   allowed in the first place — that cross-cutting decision (checking the
   student, the equipment, and the borrowing count together) does not belong
   to any single domain object, which is exactly why it lives in
   `BorrowEquipmentService` instead.

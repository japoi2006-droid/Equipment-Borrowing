// This project is the "Executable / Future UI" layer from the architecture
// diagram in README.md. Today it is a console app that only talks to the
// Application layer; later, an Avalonia UI project could sit in exactly
// this position, calling the same BorrowEquipmentService, without any
// change to Domain, Application, or Infrastructure.

using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;

var today = DateOnly.FromDateTime(DateTime.Today);
var oneWeekFromNow = today.AddDays(7);

// ---- Seed data (stands in for what would later come from a database) ----
var students = new List<Student>
{
    new(id: 1, name: "Juan Dela Cruz", isAllowedToBorrow: true),
    new(id: 2, name: "Maria Santos", isAllowedToBorrow: false), // suspended
};

var equipment = new List<Equipment>
{
    new(id: 100, name: "Digital Multimeter", isAvailable: true),
    new(id: 101, name: "Oscilloscope", isAvailable: false), // already out on loan
};

var studentRepository = new InMemoryStudentRepository(students);
var equipmentRepository = new InMemoryEquipmentRepository(equipment);
var borrowingRepository = new InMemoryBorrowingRepository();

var service = new BorrowEquipmentService(studentRepository, equipmentRepository, borrowingRepository);

Console.WriteLine("=== Campus Equipment Borrowing System - Demonstration ===");

await RunCaseAsync(
    "Case 1 (SUCCESS): Eligible student borrows available equipment",
    service,
    new BorrowEquipmentRequest(StudentId: 1, EquipmentId: 100, DateBorrowed: today, ExpectedReturnDate: oneWeekFromNow));

await RunCaseAsync(
    "Case 2 (FAILURE - equipment unavailable): Student tries to borrow equipment already on loan",
    service,
    new BorrowEquipmentRequest(StudentId: 1, EquipmentId: 101, DateBorrowed: today, ExpectedReturnDate: oneWeekFromNow));

await RunCaseAsync(
    "Case 3 (FAILURE - student not allowed): Suspended student tries to borrow",
    service,
    new BorrowEquipmentRequest(StudentId: 2, EquipmentId: 100, DateBorrowed: today, ExpectedReturnDate: oneWeekFromNow));

await RunCaseAsync(
    "Case 4 (FAILURE - equipment not found): Student tries to borrow equipment id 999",
    service,
    new BorrowEquipmentRequest(StudentId: 1, EquipmentId: 999, DateBorrowed: today, ExpectedReturnDate: oneWeekFromNow));

static async Task RunCaseAsync(string label, BorrowEquipmentService service, BorrowEquipmentRequest request)
{
    Console.WriteLine();
    Console.WriteLine(label);

    var result = await service.ExecuteAsync(request);

    if (result.IsSuccess)
    {
        var borrowing = result.Borrowing!;
        Console.WriteLine(
            $"  -> SUCCESS. Borrowing {borrowing.Id} created " +
            $"(Student {borrowing.StudentId}, Equipment {borrowing.EquipmentId}, " +
            $"Status: {borrowing.Status}, Due: {borrowing.ExpectedReturnDate})");
    }
    else
    {
        Console.WriteLine($"  -> FAILED. Reason: {result.FailureReason}");
    }
}

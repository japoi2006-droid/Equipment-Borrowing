using EquipmentBorrowing.Application.Results;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;

// ---------------------------------------------------------------------------
// This program wires the in-memory Infrastructure implementations to the
// Application services and runs through the scenario's success and failure
// paths. Nothing here talks to a database or a UI - it is a plain console
// demonstration, exactly as Part H asks for.
// ---------------------------------------------------------------------------

var studentRepository = new InMemoryStudentRepository(new[]
{
    new Student(1, "Juan Dela Cruz"),
    new Student(2, "Maria Santos", isAllowedToBorrow: false),
    new Student(3, "Pedro Reyes")
});

var equipmentRepository = new InMemoryEquipmentRepository(new[]
{
    new Equipment(1, "Digital Multimeter"),
    new Equipment(2, "Oscilloscope"),
    new Equipment(3, "Soldering Iron"),
    new Equipment(4, "Function Generator")
});

var borrowingRepository = new InMemoryBorrowingRepository();

var borrowEquipmentService = new BorrowEquipmentService(
    studentRepository, equipmentRepository, borrowingRepository);

var returnEquipmentService = new ReturnEquipmentService(
    equipmentRepository, borrowingRepository);

var today = DateOnly.FromDateTime(DateTime.Today);
var dueDate = today.AddDays(7);

Console.WriteLine("Campus Equipment Borrowing System - Console Demonstration");
Console.WriteLine("==========================================================");

Section("Case 1 (SUCCESS) - Juan borrows the Digital Multimeter");
PrintBorrow(await borrowEquipmentService.ExecuteAsync(studentId: 1, equipmentId: 1, today, dueDate));

Section("Case 2 (FAILURE) - Juan tries to borrow equipment that does not exist");
PrintBorrow(await borrowEquipmentService.ExecuteAsync(studentId: 1, equipmentId: 99, today, dueDate));

Section("Case 3 (FAILURE) - Pedro tries to borrow the Multimeter (already borrowed)");
PrintBorrow(await borrowEquipmentService.ExecuteAsync(studentId: 3, equipmentId: 1, today, dueDate));

Section("Case 4 (FAILURE) - Maria is not allowed to borrow");
PrintBorrow(await borrowEquipmentService.ExecuteAsync(studentId: 2, equipmentId: 2, today, dueDate));

Section("Case 5 (SUCCESS) - Juan borrows two more items, reaching his limit of 3");
PrintBorrow(await borrowEquipmentService.ExecuteAsync(studentId: 1, equipmentId: 2, today, dueDate));
PrintBorrow(await borrowEquipmentService.ExecuteAsync(studentId: 1, equipmentId: 3, today, dueDate));

Section("Case 6 (FAILURE) - Juan has reached the maximum number of active borrowings");
PrintBorrow(await borrowEquipmentService.ExecuteAsync(studentId: 1, equipmentId: 4, today, dueDate));

Section("Case 7 (SUCCESS) - Juan returns the Multimeter");
PrintReturn(await returnEquipmentService.ExecuteAsync(studentId: 1, equipmentId: 1, today.AddDays(2)));

Section("Case 8 (SUCCESS) - Pedro can now borrow the Multimeter (available again)");
PrintBorrow(await borrowEquipmentService.ExecuteAsync(studentId: 3, equipmentId: 1, today, dueDate));

static void Section(string title)
{
    Console.WriteLine();
    Console.WriteLine($"=== {title} ===");
}

static void PrintBorrow(BorrowResult result)
{
    Console.WriteLine(result.IsSuccess
        ? $"  Result: SUCCESS - Borrowing #{result.BorrowingId} created."
        : $"  Result: FAILURE - {result.FailureReason}");
}

static void PrintReturn(ReturnResult result)
{
    Console.WriteLine(result.IsSuccess
        ? "  Result: SUCCESS - equipment marked as returned and made available again."
        : $"  Result: FAILURE - {result.FailureReason}");
}

using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Tests;

public class BorrowEquipmentServiceTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);
    private static readonly DateOnly OneWeekFromNow = Today.AddDays(7);

    private static BorrowEquipmentService CreateService(
        IEnumerable<Student>? students = null,
        IEnumerable<Equipment>? equipment = null)
    {
        var studentRepository = new InMemoryStudentRepository(students ?? new[]
        {
            new Student(1, "Juan Dela Cruz", isAllowedToBorrow: true),
            new Student(2, "Maria Santos", isAllowedToBorrow: false)
        });

        var equipmentRepository = new InMemoryEquipmentRepository(equipment ?? new[]
        {
            new Equipment(100, "Digital Multimeter", isAvailable: true),
            new Equipment(101, "Oscilloscope", isAvailable: false)
        });

        var borrowingRepository = new InMemoryBorrowingRepository();

        return new BorrowEquipmentService(studentRepository, equipmentRepository, borrowingRepository);
    }

    [Fact]
    public async Task ExecuteAsync_WithEligibleStudentAndAvailableEquipment_CreatesActiveBorrowing()
    {
        var service = CreateService();

        var result = await service.ExecuteAsync(
            new BorrowEquipmentRequest(StudentId: 1, EquipmentId: 100, DateBorrowed: Today, ExpectedReturnDate: OneWeekFromNow));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Borrowing);
        Assert.Equal(BorrowingStatus.Active, result.Borrowing!.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEquipmentIsUnavailable_ReturnsEquipmentNotAvailable()
    {
        var service = CreateService();

        var result = await service.ExecuteAsync(
            new BorrowEquipmentRequest(StudentId: 1, EquipmentId: 101, DateBorrowed: Today, ExpectedReturnDate: OneWeekFromNow));

        Assert.False(result.IsSuccess);
        Assert.Equal(BorrowFailureReason.EquipmentNotAvailable, result.FailureReason);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStudentIsNotAllowedToBorrow_ReturnsStudentNotAllowedToBorrow()
    {
        var service = CreateService();

        var result = await service.ExecuteAsync(
            new BorrowEquipmentRequest(StudentId: 2, EquipmentId: 100, DateBorrowed: Today, ExpectedReturnDate: OneWeekFromNow));

        Assert.False(result.IsSuccess);
        Assert.Equal(BorrowFailureReason.StudentNotAllowedToBorrow, result.FailureReason);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStudentDoesNotExist_ReturnsStudentNotFound()
    {
        var service = CreateService();

        var result = await service.ExecuteAsync(
            new BorrowEquipmentRequest(StudentId: 999, EquipmentId: 100, DateBorrowed: Today, ExpectedReturnDate: OneWeekFromNow));

        Assert.False(result.IsSuccess);
        Assert.Equal(BorrowFailureReason.StudentNotFound, result.FailureReason);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEquipmentDoesNotExist_ReturnsEquipmentNotFound()
    {
        var service = CreateService();

        var result = await service.ExecuteAsync(
            new BorrowEquipmentRequest(StudentId: 1, EquipmentId: 999, DateBorrowed: Today, ExpectedReturnDate: OneWeekFromNow));

        Assert.False(result.IsSuccess);
        Assert.Equal(BorrowFailureReason.EquipmentNotFound, result.FailureReason);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStudentReachedMaximumActiveBorrowings_ReturnsMaximumActiveBorrowingsReached()
    {
        var equipmentList = new List<Equipment>
        {
            new(100, "Multimeter", true),
            new(101, "Function Generator", true),
            new(102, "Power Supply", true),
            new(103, "Logic Analyzer", true),
        };

        var service = CreateService(equipment: equipmentList);

        // Borrow up to the policy limit (3) first.
        await service.ExecuteAsync(new BorrowEquipmentRequest(1, 100, Today, OneWeekFromNow));
        await service.ExecuteAsync(new BorrowEquipmentRequest(1, 101, Today, OneWeekFromNow));
        await service.ExecuteAsync(new BorrowEquipmentRequest(1, 102, Today, OneWeekFromNow));

        var result = await service.ExecuteAsync(new BorrowEquipmentRequest(1, 103, Today, OneWeekFromNow));

        Assert.False(result.IsSuccess);
        Assert.Equal(BorrowFailureReason.MaximumActiveBorrowingsReached, result.FailureReason);
    }
}

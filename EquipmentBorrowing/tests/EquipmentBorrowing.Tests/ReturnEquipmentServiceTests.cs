using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;
using Xunit;

namespace EquipmentBorrowing.Tests;

public class ReturnEquipmentServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WithActiveBorrowing_ReturnsEquipmentAndMakesItAvailable()
    {
        var studentRepository = new InMemoryStudentRepository(new[] { new Student(1, "Juan Dela Cruz") });
        var equipmentRepository = new InMemoryEquipmentRepository(new[] { new Equipment(1, "Digital Multimeter") });
        var borrowingRepository = new InMemoryBorrowingRepository();

        var borrowService = new BorrowEquipmentService(studentRepository, equipmentRepository, borrowingRepository);
        var returnService = new ReturnEquipmentService(equipmentRepository, borrowingRepository);

        var today = new DateOnly(2026, 8, 27);
        await borrowService.ExecuteAsync(studentId: 1, equipmentId: 1, today, today.AddDays(7));

        var result = await returnService.ExecuteAsync(studentId: 1, equipmentId: 1, today.AddDays(2));

        Assert.True(result.IsSuccess);

        var equipment = await equipmentRepository.GetByIdAsync(1);
        Assert.True(equipment!.IsAvailable);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoActiveBorrowing_Fails()
    {
        var equipmentRepository = new InMemoryEquipmentRepository(new[] { new Equipment(1, "Digital Multimeter") });
        var borrowingRepository = new InMemoryBorrowingRepository();
        var returnService = new ReturnEquipmentService(equipmentRepository, borrowingRepository);

        var result = await returnService.ExecuteAsync(studentId: 1, equipmentId: 1, new DateOnly(2026, 8, 27));

        Assert.False(result.IsSuccess);
        Assert.Equal("No active borrowing found for this student and equipment.", result.FailureReason);
    }
}

using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Results;

namespace EquipmentBorrowing.Application.Services;

/// <summary>
/// Coordinates the "Return Equipment" use case. Not the required service for
/// Part E (BorrowEquipmentService is), but implemented here so the console
/// demonstration in Part H can show the full borrow -> return cycle described
/// in the scenario, and so the "equipment becomes available again" rule is
/// actually exercised somewhere.
/// </summary>
public class ReturnEquipmentService
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;

    public ReturnEquipmentService(
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _equipmentRepository = equipmentRepository;
        _borrowingRepository = borrowingRepository;
    }

    public async Task<ReturnResult> ExecuteAsync(
        int studentId,
        int equipmentId,
        DateOnly returnDate,
        CancellationToken cancellationToken = default)
    {
        var borrowing = await _borrowingRepository.GetActiveByStudentAndEquipmentAsync(
            studentId, equipmentId, cancellationToken);

        if (borrowing is null)
            return ReturnResult.Failure("No active borrowing found for this student and equipment.");

        borrowing.MarkAsReturned(returnDate);
        await _borrowingRepository.UpdateAsync(borrowing, cancellationToken);

        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
        if (equipment is not null)
        {
            equipment.MarkAsAvailable();
            await _equipmentRepository.UpdateAsync(equipment, cancellationToken);
        }

        return ReturnResult.Success();
    }
}

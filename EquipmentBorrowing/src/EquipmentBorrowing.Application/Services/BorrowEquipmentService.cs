using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

/// <summary>
/// Executes the "Borrow Equipment" use case: coordinates the Student,
/// Equipment, and Borrowing repositories to validate and record a new
/// borrowing.
/// </summary>
/// <remarks>
/// This class contains no database connections, no SQL, and no
/// user-interface code — only orchestration of domain objects and the
/// repository abstractions it was given through its constructor
/// (see Part F: dependencies are received, never created with <c>new</c>
/// inside this class).
/// </remarks>
public class BorrowEquipmentService
{
    /// <summary>
    /// Lab-defined policy: a student may not have more than this many
    /// active borrowings at the same time.
    /// </summary>
    private const int MaxActiveBorrowingsPerStudent = 3;

    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;

    public BorrowEquipmentService(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _borrowingRepository = borrowingRepository;
    }

    public async Task<BorrowEquipmentResult> ExecuteAsync(
        BorrowEquipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Does the student exist?
        var student = await _studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
        if (student is null)
        {
            return BorrowEquipmentResult.Failure(BorrowFailureReason.StudentNotFound);
        }

        // 2. Is the student allowed to borrow?
        if (!student.IsAllowedToBorrow)
        {
            return BorrowEquipmentResult.Failure(BorrowFailureReason.StudentNotAllowedToBorrow);
        }

        // 3. Does the equipment exist?
        var equipment = await _equipmentRepository.GetByIdAsync(request.EquipmentId, cancellationToken);
        if (equipment is null)
        {
            return BorrowEquipmentResult.Failure(BorrowFailureReason.EquipmentNotFound);
        }

        // 4. Is the equipment currently available?
        if (!equipment.IsAvailable)
        {
            return BorrowEquipmentResult.Failure(BorrowFailureReason.EquipmentNotAvailable);
        }

        // 5. Has the student reached the allowed number of active borrowings?
        var activeBorrowingCount = await _borrowingRepository.CountActiveBorrowingsForStudentAsync(
            request.StudentId, cancellationToken);

        if (activeBorrowingCount >= MaxActiveBorrowingsPerStudent)
        {
            return BorrowEquipmentResult.Failure(BorrowFailureReason.MaximumActiveBorrowingsReached);
        }

        // 6. All rules satisfied — create the borrowing.
        equipment.MarkAsBorrowed();
        await _equipmentRepository.UpdateAsync(equipment, cancellationToken);

        var borrowing = new Borrowing(
            studentId: student.Id,
            equipmentId: equipment.Id,
            dateBorrowed: request.DateBorrowed,
            expectedReturnDate: request.ExpectedReturnDate);

        await _borrowingRepository.AddAsync(borrowing, cancellationToken);

        return BorrowEquipmentResult.Success(borrowing);
    }
}

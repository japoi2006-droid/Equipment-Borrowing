using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

/// <summary>
/// The outcome of attempting to execute the Borrow Equipment use case.
/// Exactly one of <see cref="Borrowing"/> or <see cref="FailureReason"/> is
/// populated, depending on <see cref="IsSuccess"/>.
/// </summary>
public sealed class BorrowEquipmentResult
{
    public bool IsSuccess { get; }
    public Borrowing? Borrowing { get; }
    public BorrowFailureReason? FailureReason { get; }

    private BorrowEquipmentResult(bool isSuccess, Borrowing? borrowing, BorrowFailureReason? failureReason)
    {
        IsSuccess = isSuccess;
        Borrowing = borrowing;
        FailureReason = failureReason;
    }

    public static BorrowEquipmentResult Success(Borrowing borrowing) => new(true, borrowing, null);

    public static BorrowEquipmentResult Failure(BorrowFailureReason reason) => new(false, null, reason);
}

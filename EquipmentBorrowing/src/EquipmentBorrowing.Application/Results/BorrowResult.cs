namespace EquipmentBorrowing.Application.Results;

/// <summary>
/// Outcome of a "Borrow Equipment" attempt. Modeled as an immutable record
/// instead of a bool/exception so that expected business-rule failures (e.g.,
/// "equipment unavailable") are ordinary return values, not exceptions -
/// exceptions are reserved for truly exceptional/programmer-error situations.
/// </summary>
public sealed record BorrowResult(bool IsSuccess, string? FailureReason, int? BorrowingId)
{
    public static BorrowResult Success(int borrowingId) =>
        new(IsSuccess: true, FailureReason: null, BorrowingId: borrowingId);

    public static BorrowResult Failure(string reason) =>
        new(IsSuccess: false, FailureReason: reason, BorrowingId: null);
}

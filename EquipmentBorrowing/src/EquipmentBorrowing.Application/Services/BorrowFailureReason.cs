namespace EquipmentBorrowing.Application.Services;

/// <summary>
/// Every way a borrow request can be legitimately rejected, mirroring the
/// rules listed in the lab's scenario. Modeling failures explicitly (instead
/// of throwing exceptions for expected business outcomes) lets a caller
/// display a specific, meaningful message for each case.
/// </summary>
public enum BorrowFailureReason
{
    StudentNotFound,
    StudentNotAllowedToBorrow,
    EquipmentNotFound,
    EquipmentNotAvailable,
    MaximumActiveBorrowingsReached
}

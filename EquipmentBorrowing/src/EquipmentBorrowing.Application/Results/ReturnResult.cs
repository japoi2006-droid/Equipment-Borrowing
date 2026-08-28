namespace EquipmentBorrowing.Application.Results;

/// <summary>
/// Outcome of a "Return Equipment" attempt.
/// </summary>
public sealed record ReturnResult(bool IsSuccess, string? FailureReason)
{
    public static ReturnResult Success() =>
        new(IsSuccess: true, FailureReason: null);

    public static ReturnResult Failure(string reason) =>
        new(IsSuccess: false, FailureReason: reason);
}

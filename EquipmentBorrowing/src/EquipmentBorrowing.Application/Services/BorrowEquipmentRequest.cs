namespace EquipmentBorrowing.Application.Services;

/// <summary>
/// The information needed to attempt a borrowing. This is an
/// application-layer input shape, not a domain object — it exists to carry
/// data from whatever caller (console app, future UI, test) into the service.
/// </summary>
public record BorrowEquipmentRequest(
    int StudentId,
    int EquipmentId,
    DateOnly DateBorrowed,
    DateOnly ExpectedReturnDate);

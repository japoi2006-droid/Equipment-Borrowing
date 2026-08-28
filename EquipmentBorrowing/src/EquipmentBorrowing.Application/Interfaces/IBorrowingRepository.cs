using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

/// <summary>
/// Abstraction over wherever borrowing records eventually live.
/// </summary>
public interface IBorrowingRepository
{
    /// <summary>
    /// Counts how many borrowings for the given student are currently
    /// <see cref="BorrowingStatus.Active"/>. Needed to enforce the maximum
    /// active borrowings per student rule.
    /// </summary>
    Task<int> CountActiveBorrowingsForStudentAsync(int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a newly created borrowing. Needed once a borrow request
    /// passes every validation rule.
    /// </summary>
    Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default);
}

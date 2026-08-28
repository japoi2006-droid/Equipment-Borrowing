using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

/// <summary>
/// Abstraction over wherever student records eventually live (database,
/// file, in-memory, etc.). The application layer only knows this interface,
/// never the concrete storage technology.
/// </summary>
public interface IStudentRepository
{
    /// <summary>
    /// Retrieves a student by id, or <c>null</c> if no such student exists.
    /// Needed by BorrowEquipmentService to check that the borrowing student
    /// actually exists and is currently allowed to borrow.
    /// </summary>
    Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}

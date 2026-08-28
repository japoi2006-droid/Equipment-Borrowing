using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

/// <summary>
/// Stores students in a plain in-memory dictionary instead of a real
/// database. The Application layer only ever depends on
/// <see cref="IStudentRepository"/>, so this class could later be swapped
/// for a SQLite- or PostgreSQL-backed implementation with no changes to
/// BorrowEquipmentService.
/// </summary>
public class InMemoryStudentRepository : IStudentRepository
{
    private readonly Dictionary<int, Student> _students;

    public InMemoryStudentRepository(IEnumerable<Student>? seedData = null)
    {
        _students = (seedData ?? Enumerable.Empty<Student>()).ToDictionary(student => student.Id);
    }

    public Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _students.TryGetValue(id, out var student);
        return Task.FromResult(student);
    }
}

using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

/// <summary>
/// Abstraction over wherever equipment records eventually live.
/// </summary>
public interface IEquipmentRepository
{
    /// <summary>
    /// Retrieves equipment by id, or <c>null</c> if it does not exist.
    /// Needed to check the equipment exists and is currently available.
    /// </summary>
    Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a change to an existing piece of equipment (for example,
    /// after it has been marked as borrowed). Needed because the service
    /// mutates an Equipment's availability and that change must be saved.
    /// </summary>
    Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default);
}

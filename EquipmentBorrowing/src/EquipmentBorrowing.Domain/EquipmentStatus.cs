namespace EquipmentBorrowing.Domain;

/// <summary>
/// Represents whether a piece of <see cref="Equipment"/> can currently be borrowed.
/// </summary>
public enum EquipmentStatus
{
    Available,
    Borrowed
}

namespace EquipmentBorrowing.Domain;

/// <summary>
/// Represents a piece of laboratory equipment that can be borrowed.
/// </summary>
/// <remarks>
/// <para><b>Responsibility:</b> holds identity and availability, and enforces
/// the rule that equipment cannot be borrowed twice at the same time. State
/// and the rule that protects that state live together in one place.</para>
/// <para><b>Not this class's responsibility:</b> knowing who borrowed it, for
/// how long, or whether the borrower is allowed to borrow. Those are
/// cross-entity concerns that belong to the Application layer, since they
/// require coordinating with <see cref="Student"/> and <see cref="Borrowing"/>
/// as well.</para>
/// </remarks>
public class Equipment
{
    public int Id { get; }
    public string Name { get; }
    public bool IsAvailable { get; private set; }

    public Equipment(int id, string name, bool isAvailable = true)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Equipment name cannot be empty.", nameof(name));
        }

        Id = id;
        Name = name;
        IsAvailable = isAvailable;
    }

    public void MarkAsBorrowed()
    {
        if (!IsAvailable)
        {
            throw new InvalidOperationException($"Equipment '{Name}' is already borrowed.");
        }

        IsAvailable = false;
    }

    public void MarkAsReturned() => IsAvailable = true;
}

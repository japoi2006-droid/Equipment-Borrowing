namespace EquipmentBorrowing.Domain;

/// <summary>
/// Represents a student who may be permitted to borrow laboratory equipment.
/// </summary>
/// <remarks>
/// <para><b>Responsibility:</b> holds the student's identity and whether the
/// student currently has borrowing privileges.</para>
/// <para><b>Not this class's responsibility:</b> knowing which equipment the
/// student currently has borrowed, or how many active borrowings the student
/// has. That information depends on <see cref="Borrowing"/> records that live
/// elsewhere, so counting/looking them up is an Application-layer concern
/// (it requires querying a repository), not something this object should do
/// to itself.</para>
/// </remarks>
public class Student
{
    public int Id { get; }
    public string Name { get; }
    public bool IsAllowedToBorrow { get; private set; }

    public Student(int id, string name, bool isAllowedToBorrow = true)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Student name cannot be empty.", nameof(name));
        }

        Id = id;
        Name = name;
        IsAllowedToBorrow = isAllowedToBorrow;
    }

    public void SuspendBorrowingPrivileges() => IsAllowedToBorrow = false;

    public void RestoreBorrowingPrivileges() => IsAllowedToBorrow = true;
}

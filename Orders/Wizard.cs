using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Orders;

public class Wizard
{
    public Wizard() : this(Ulid.NewUlid())
    {
    }

    public Wizard(Ulid id)
    {
        Id = id;
        Wizards[Id] = this;
    }

    private static ConcurrentDictionary<Ulid, Wizard> Wizards { get; } = new();

    public static Wizard GetWizard(Ulid id) => Wizards[id];

    public static bool TryGetWizard(Ulid id, [NotNullWhen(true)] out Wizard? wizard)
    {
        return Wizards.TryGetValue(id, out wizard);
    }

    public Ulid Id { get; init; }
    public string? Name { get; set; } = null;
    public DateOnly? BirthDate { get; set; } = null;
    public string? Address { get; set; } = null;

    public override string ToString() => $"Wizard {Name}";
    public override bool Equals(object? obj) => obj is Wizard wizard && wizard.Id == Id;
    public override int GetHashCode() => Id.GetHashCode();
    public static bool operator ==(Wizard left, Wizard right) => left.Equals(right);
    public static bool operator !=(Wizard left, Wizard right) => !(left == right);
}

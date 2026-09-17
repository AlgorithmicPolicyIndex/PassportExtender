using JetBrains.Annotations;

namespace PassportExtender.Core.Tab;

public class TabType
{
    public int Id { get; }
    
    internal TabType(int id) => Id = id;
    
    private bool Equals(TabType other) => Id == other.Id;
    public override bool Equals(object obj) => obj is TabType other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==([NotNull] TabType left, [NotNull] TabType right) => left.Equals(right);
    public static bool operator !=([NotNull] TabType left, [NotNull] TabType right) => !left.Equals(right);

    public override string ToString() => $"TabType({Id})";
}
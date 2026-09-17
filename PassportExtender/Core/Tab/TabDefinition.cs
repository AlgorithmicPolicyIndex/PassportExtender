using UnityEngine;

namespace PassportExtender.Core.Tab;

public sealed class TabDefinition
{
    public string Name { get; init; } = "";
    public TabType Type { get; init; }
    public Texture2D Icon { get; init; } = null!;
    public CustomizationOption[] Options { get; init; } = [];
    public int InitialSelection { get; init; } = -1;
}
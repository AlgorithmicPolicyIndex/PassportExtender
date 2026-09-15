using System;
using JetBrains.Annotations;
using UnityEngine;

namespace PassportExtender.Core.Tab;

public sealed class TabDefinition
{
    public string Name { get; init; } = "";
    public TabType Type { get; init; }
    public Texture2D Icon { get; init; } = null!;
    [CanBeNull] public CustomizationOption[] Options { get; init; }
    [CanBeNull] public Action<int> OnOptionSelected { get; init; }
    [CanBeNull] public Func<Character, int, bool> OnInitialEquip { get; init; }
    public int InitialSelection { get; init; } = -1;
}
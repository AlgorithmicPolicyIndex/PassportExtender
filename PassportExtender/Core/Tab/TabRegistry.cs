using System.Collections.Generic;
using System.Linq;
using PassportExtender.Core.Util;

namespace PassportExtender.Core.Tab;

public static class TabRegistry
{
    private static readonly Dictionary<string, TabDefinition> Definitions = new();
    private static readonly List<TabDefinition> PendingOrder = [];
    private static readonly Dictionary<int, TabDefinition> ByTypeId = new();
    private static readonly Dictionary<int, int> SelectedByTypeId = new();

    private static bool _passportBuilt;

    public static void Register(TabDefinition definition)
    {
        if (Definitions.ContainsKey(definition.Name))
        {
            Log.LogWarning($"[TabRegistry] '{definition.Name}' already registered – ignoring duplicate...");
            return;
        }
        
        if (definition.InitialSelection > 0)
            SetSelected(definition.Type.Id, definition.InitialSelection);
        
        Definitions[definition.Name] = definition;
        ByTypeId[definition.Type.Id] = definition;
        PendingOrder.Add(definition);
        
        Log.LogDebug($"[TabRegistry] queued '{definition.Name} (total: {Definitions.Count})");

        if (_passportBuilt && TabApplier.TryGetCurrentManager(out var manager))
        {
            ApplySingle(manager, definition);
        }
    }
    
    public static bool TryGetDefinition(int typeId, out TabDefinition def)
        => ByTypeId.TryGetValue(typeId, out def!);

    public static void ApplyAll(PassportManager manager)
    {
        foreach (var def in PendingOrder)
        {
            ApplySingle(manager, def);
        }
        _passportBuilt = true;
    }

    private static void ApplySingle(PassportManager manager, TabDefinition def)
    {
        var alreadyThere = manager.tabs.Any(t => t != null && (int)t.type == def.Type.Id);

        if (alreadyThere)
        {
            Log.LogDebug($"[TabRegistry] '{def.Name}' already present in manager – skipping...");
            return;
        }

        TabApplier.CreateTab(manager, def);
    }

    internal static bool RestoreSelections(Character character)
    {
        var allApplied = true;
        foreach (var def in Registered)
        {
            int sel = GetSelected(def.Type.Id);
            if (sel < 0) continue;

            var applied = def.OnInitialEquip?.Invoke(character, sel) ?? true;
            if (applied)
                Log.LogDebug($"[Restore] '{def.Name}' equipped {sel} on {character.gameObject.name}");
            else
            {
                Log.LogDebug($"[Restore] failed to restore.");
                allApplied = false;
            }
        }
        return allApplied;
    }

    private static IReadOnlyList<TabDefinition> Registered => PendingOrder;
    public static int Count => Definitions.Count;
    public static int GetSelected(int typeId)
        => SelectedByTypeId.GetValueOrDefault(typeId, -1);

    public static void SetSelected(int typeId, int index)
        => SelectedByTypeId[typeId] = index;
}
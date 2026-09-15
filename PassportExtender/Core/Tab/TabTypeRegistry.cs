using System.Collections.Generic;
using PassportExtender.Core.Util;

namespace PassportExtender.Core.Tab;

public static class TabTypeRegistry
{
    private static readonly Dictionary<string, TabType> ByName = new();
    private static readonly Dictionary<int, string> NamesById = new();
    private static int _nextId = 80;

    private static readonly object Lock = new();

    public static bool TryRegister(string name, out TabType tabType)
    {
        lock (Lock)
        {
            if (ByName.ContainsKey(name))
            {
                tabType = null;
                Log.LogWarning($"[TabTypeRegistry] Tab name '{name}' already registered — rejecting duplicate");
                return false;
            }

            _nextId += 10;
            tabType = new TabType(_nextId);
            ByName[name] = tabType;
            NamesById[_nextId] = name;

            Log.LogDebug($"[TabTypeRegistry] Registered '{name}' → ID {_nextId}");
            return true;
        }
    }

    public static bool TryResolveName(int id, out string name) => NamesById.TryGetValue(id, out name);

    public static bool HasRegistered(string name) => ByName.ContainsKey(name);

    public static IReadOnlyDictionary<string, TabType> RegisteredTypes => ByName;
}
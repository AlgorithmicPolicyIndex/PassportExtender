using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PassportExtender.Core.Tab;
using PassportExtender.Core.Util;

namespace PassportExtender.Core;

public static class PassportExtenderAPI
{
    private static bool _isAvailable;
    private static PassportManager _currentManager;
    private static readonly List<Action<PassportManager>> ReadyCallbacks = [];
    [CanBeNull] private static event Action<PlayerCustomizationDummy> DummyShownInternal;

    internal static void Initialize()
    {
        _isAvailable = true;
        _currentManager = null;
        Log.LogInfo("[ExtenderAPI] Initialized");
    }

    internal static void SetManager(PassportManager manager)
    {
        _currentManager = manager;

        foreach (var cb in ReadyCallbacks.ToList())
        {
            cb(manager);
        }
        ReadyCallbacks.Clear();
    }
    
    public static bool IsAvailable() => _isAvailable;

    public static void RegisterTab(TabDefinition definition)
    {
        if (!_isAvailable)
        {
            Log.LogError($"[ExtenderAPI] Cannot register tab {definition.Name} – PassportExtender not loaded.");
            return;
        }

        if (string.IsNullOrEmpty(definition.Name))
        {
            Log.LogError("[ExtenderAPI] Tab Name cannot be empty.");
            return;
        }
        
        Log.LogDebug($"[PassportExtender] Registering tab {definition.Name}");
        TabRegistry.Register(definition);
    }

    public static bool TryRegisterType(string name, out TabType tabType)
    {
        var success = TabTypeRegistry.TryRegister(name, out tabType);

        if (!success)
            Log.LogWarning($"[ExtenderAPI] Type registration failed for '{name}' – duplicate name or collision");
        return success;
    }
    
    public static void SubscribeToDummy(Action<PlayerCustomizationDummy> onDummy)
    {
        var current = _currentManager?.dummy;
        if (current != null) { onDummy(current); return; }
        DummyShownInternal += onDummy;
    }

    internal static void RaiseDummyShown(PlayerCustomizationDummy dummy)
        => DummyShownInternal?.Invoke(dummy);
}
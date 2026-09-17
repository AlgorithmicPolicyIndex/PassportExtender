using System;
using System.Collections.Generic;
using PassportExtender.Core.Tab;
using PassportExtender.Core.Util;

namespace PassportExtender;

/// <summary>
/// Public API for dependent mods. All callbacks fire on the Unity main thread.
/// Never throws unless your own callback throws (which is isolated and logged).
/// </summary>
public static class PassportExtenderAPI
{
    public const string PEGuid = Main.Guid;

    private static bool _isInitialized;

    // Cached current instances — see ClearManager for why both are tracked
    private static PassportManager _currentManager;
    private static PlayerCustomizationDummy _currentDummy;
    private static Character _currentCharacter;
    private static readonly Dictionary<int, List<Action<TabDefinition, CustomizationOption>>> SelectionHandlers = new();

    private static readonly List<Action<PassportManager>> ManagerReadyCallbacks = [];
    private static readonly List<Action<PlayerCustomizationDummy>> DummyShownCallbacks = [];
    private static readonly List<Action<Character>> CharacterShownCallbacks = [];

    public static event Action<Character> LocalCharacterChanged;
    

    // ---------- Called by core infrastructure, never by consumers ----------

    internal static void Initialize()
    {
        _isInitialized = true;
        _currentManager = null;
        _currentDummy = null;
        Log.LogInfo("[ExtenderAPI] Initialized");
    }

    internal static void SetManager(PassportManager manager)
    {
        _currentManager = manager;
        var dummy = manager != null ? manager.dummy : null;
        if (dummy != null)
        {
            _currentDummy = dummy;
            FireQueued(dummy, DummyShownCallbacks);
        }
        FireQueued(manager, ManagerReadyCallbacks);
    }
    
    internal static void RaiseLocalCharacterChanged(Character character)
    {
        _currentCharacter = character;
        FireQueued(character, CharacterShownCallbacks);
        
        try { LocalCharacterChanged?.Invoke(character); }
        catch (Exception e) { Log.LogWarning($"[ExtenderAPI] LocalCharacterChanged subscriber threw: {e}"); }
    }
    
    internal static void RaiseOptionSelected(int typeId, int index)
    {
        if (!SelectionHandlers.TryGetValue(typeId, out var list)) return;

        var def = TabRegistry.TryGetDefinition(typeId, out var d) ? d : null;
        CustomizationOption option = null;
    
        if (def?.Options != null && index >= 0 && index < def.Options.Length)
            option = def.Options[index];

        var snapshot = list.ToArray();
        foreach (var cb in snapshot)
            try { cb(def, option); }
            catch (Exception e) { Log.LogWarning($"[ExtenderAPI] selection subscriber threw: {e}"); }
    }

    // ---------- Queries ----------
    public static int GetSelected(TabType tabType)
    {
        return TabRegistry.GetSelected(tabType.Id);
    }

    // ---------- Lifecycle helpers (fire-immediately-or-queue) ----------

    /// <summary>Fires now if a manager exists, else exactly once on next spawn.</summary>
    public static void WhenManagerReady(Action<PassportManager> callback)
    {
        if (!Guard(callback)) return;
        if (_currentManager != null) SafeInvoke(callback, _currentManager);
        else AddUnique(callback, ManagerReadyCallbacks);
    }

    public static void WhenDummyShown(Action<PlayerCustomizationDummy> callback)
    {
        if (!Guard(callback)) return;
        if (_currentDummy != null) SafeInvoke(callback, _currentDummy);
        else AddUnique(callback, DummyShownCallbacks);
    }

    public static void WhenLocalCharacter(Action<Character> callback)
    {
        if (!Guard(callback)) return;
        if (_currentCharacter != null) SafeInvoke(callback, _currentCharacter);
        else AddUnique(callback, CharacterShownCallbacks);
    }
    
    public static IDisposable SubscribeToSelection(TabType type, Action<TabDefinition, CustomizationOption> onChange)
    {
        if (!RequireInit("SubscribeToSelection")) return NoopDisposable.Instance;

        if (!SelectionHandlers.TryGetValue(type.Id, out var list))
            SelectionHandlers[type.Id] = list = [];
    
        lock (list)
            list.Add(onChange);

        return new Unsub(type.Id, onChange);
    }

    private static void AddUnique<T>(Action<T> callback, List<Action<T>> queue)
    {
        if (queue.Contains(callback))
        {
            Log.LogWarning("[ExtenderAPI] duplicate When* callback ignored");
            return;
        }
        queue.Add(callback);
    }

    // ---------- Registration ----------

    public static void RegisterTab(TabDefinition definition)
    {
        if (!RequireInit($"Cannot register tab '{definition?.Name}'")) return;
        if (string.IsNullOrEmpty(definition?.Name))
        {
            Log.LogError("[ExtenderAPI] Tab Name cannot be empty.");
            return;
        }
        TabRegistry.Register(definition);
    }

    public static bool TryRegisterType(string name, out TabType tabType)
    {
        if (!RequireInit($"Cannot register type '{name}'"))
        {
            tabType = null;
            return false;
        }
        var success = TabTypeRegistry.TryRegister(name, out tabType);
        if (!success)
            Log.LogWarning($"[ExtenderAPI] Type registration failed for '{name}' – duplicate name or collision");
        return success;
    }

    // ---------- Shared plumbing ----------
    
    private sealed class NoopDisposable : IDisposable
    {
        public static readonly IDisposable Instance = new NoopDisposable();
        public void Dispose() { }
    }

    private static bool RequireInit(string message)
    {
        if (_isInitialized) return true;
        Log.LogError($"[ExtenderAPI] {message} – PassportExtender not loaded.");
        return false;
    }

    private static bool Guard<T>(Action<T> callback)
    {
        if (callback != null) return RequireInit("Lifecycle API unavailable");
        Log.LogError("[ExtenderAPI] callback was null"); return false;
    }

    private static void FireQueued<T>(T value, List<Action<T>> queue)
    {
        var toFire = queue.ToArray();
        queue.Clear();
        foreach (var cb in toFire) SafeInvoke(cb, value);
    }

    private static void SafeInvoke<T>(Action<T> cb, T value)
    {
        try { cb(value); }
        catch (Exception e) { Log.LogWarning($"[ExtenderAPI] consumer callback threw: {e}"); }
    }
    
    private sealed class Unsub(int typeId, Action<TabDefinition, CustomizationOption> handler)
        : IDisposable
    {
        public void Dispose() => RemoveSelectionHandler(typeId, handler);
    }

    private static void RemoveSelectionHandler(
        int typeId, Action<TabDefinition, CustomizationOption> handler)
    {
        if (!SelectionHandlers.TryGetValue(typeId, out var list)) return;
        lock (list) list.Remove(handler);
    }
}
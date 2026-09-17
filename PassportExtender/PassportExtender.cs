using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using PassportExtender.Core.Util;
using PassportExtender.Core.Watchers;
using PassportExtender.Patcher;

namespace PassportExtender;

[BepInPlugin(
    Guid,
    Name,
    Version
)]
internal sealed class Main : BaseUnityPlugin
{
    public const string Guid = "com.algorithmicpolicyindex.passportextender";
    private const string Name = "PassportExtender";
    private const string Version = "0.3.0";
    
    internal static ConfigEntry<bool> DebugLogs;
    
    private Harmony _harmony;

    private void Awake()
    {
        Log.Source = Logger;
        DebugLogs = Config.Bind("General", "Debug_Logs", false, "Enable Verbose [DEBUG] output.");

        PassportExtenderAPI.Initialize();
        PassportLifecycleWatcher.EnsureExists();
        
        try
        {
            _harmony = new Harmony(Guid);
            // I need to figure out why PatchAll() does not work without specifying.
            // _harmony.PatchAll();
            _harmony.PatchAll(typeof(PassportPatch));
            _harmony.PatchAll(typeof(DummyPatch));
            _harmony.PatchAll(typeof(CustomizationGetListPatch));
            _harmony.PatchAll(typeof(SetOptionPatch));
            _harmony.PatchAll(typeof(SetActiveButtonPatch));
            _harmony.PatchAll(typeof(IsAchievementUnlocked));
                
            foreach (var method in _harmony.GetPatchedMethods())
                Logger.LogDebug($"[Startup] Patched: {method.DeclaringType?.Name}.{method.Name}");
        }
        catch (Exception e)
        {
            Log.LogError($"Patching THREW: {e}");
        }
        
        Log.LogInfo($"{Name} v{Version} loaded");
    }

    private void OnDestroy()
    {
        _harmony.UnpatchSelf();
    }
}

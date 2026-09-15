using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PassportExtender.Core;
using PassportExtender.Core.Util;
using PassportExtender.Patcher;
using PassportExtender.UI;

namespace PassportExtender;

[BepInPlugin(
    Guid,
    Name,
    Version
)]
public sealed class Main : BaseUnityPlugin
{
    public const string Guid = "com.algorithmicpolicyindex.passportextender";
    private const string Name = "PassportExtender";
    private const string Version = "0.2.0";
    
    internal static ConfigEntry<bool> DebugLogs;
    
    private Harmony _harmony;

    private void Awake()
    {
        Log.Source = Logger;
        DebugLogs = Config.Bind("General", "Debug_Logs", false, "Enable Verbose [DEBUG] output.");

        Log.LogInfo($"[Startup] Debug_Logs = {DebugLogs.Value}");
        PassportExtenderAPI.Initialize();
        ExtenderRestorer.EnsureExists();
        
        try
        {
            _harmony = new Harmony(Guid);
            _harmony.PatchAll(typeof(PassportPatch));
            _harmony.PatchAll(typeof(DummyPatch));
            _harmony.PatchAll(typeof(CustomizationGetListPatch));
            _harmony.PatchAll(typeof(SetOptionPatch));
            _harmony.PatchAll(typeof(SetActiveButtonPatch));
            
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

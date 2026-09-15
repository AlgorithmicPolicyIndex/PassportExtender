using HarmonyLib;
using PassportExtender.Core;
using PassportExtender.Core.Tab;
using PassportExtender.Core.Util;
using PassportExtender.UI;

namespace PassportExtender.Patcher;

public class PassportPatch
{
    [HarmonyPatch(typeof(PassportManager), "Awake")]
    [HarmonyPostfix]
    private static void Awake(PassportManager __instance)
    {
        Log.LogDebug($"[Passport.Awake] Fired – Vanilla Tabs: {__instance.tabs.Length}");
        
        TabApplier.Init(__instance);
        
        TabRegistry.ApplyAll(__instance);
        
        Scroller.EnsureScroller(__instance);

        PassportExtenderAPI.SetManager(__instance);
        
        Log.LogDebug($"[Passport.Awake] Complete – Tabs Now: {__instance.tabs.Length}");
    }
}
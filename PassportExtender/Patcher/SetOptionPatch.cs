using HarmonyLib;
using PassportExtender.Core.Tab;
using PassportExtender.Core.Util;

namespace PassportExtender.Patcher;

internal static class SetOptionPatch
{
    [HarmonyPatch(typeof(PassportManager), "SetOption")]
    [HarmonyPrefix]
    private static bool SetOption(PassportManager __instance, CustomizationOption option, int index)
    {
        if (!TabTypeRegistry.TryResolveName((int)option.type, out _))
            return true;

        if (!TabRegistry.TryGetDefinition((int)option.type, out var def)) return false;
        
        def.OnOptionSelected?.Invoke(index);
        TabRegistry.SetSelected((int)option.type, index);
        
        Traverse.Create(__instance).Method("SetActiveButton", false).GetValue();
        __instance.dummy.UpdateDummy();

        Log.LogDebug($"[SetOption] '{option.name}' option {index} selected");
        return false;
    }
}
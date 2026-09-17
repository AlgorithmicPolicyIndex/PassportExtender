using HarmonyLib;
using PassportExtender.Core.Tab;
using PassportExtender.Core.Util;

namespace PassportExtender.Patcher;

[HarmonyPatch(typeof(Customization), "GetList")]
internal static class CustomizationGetListPatch
{
    [HarmonyPrefix]
    private static bool GetList(Customization.Type type, ref CustomizationOption[] __result)
    {
        if (!TabTypeRegistry.TryResolveName((int)type, out _))
            return true;

        if (!TabRegistry.TryGetDefinition((int)type, out var def))
        {
            __result = [];
            return false;
        }
        
        __result = def.Options ?? [];
        Log.LogDebug($"[GetList] serving {__result.Length} options for '{def.Name}'");
        return false;
    }
}
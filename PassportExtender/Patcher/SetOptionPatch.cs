using HarmonyLib;
using PassportExtender.Core.Tab;
using PassportExtender.Core.Util;

namespace PassportExtender.Patcher;

[HarmonyPatch(typeof(PassportManager), "SetOption")]
internal static class SetOptionPatch
{
    [HarmonyPrefix]
    private static void SetOption(CustomizationOption option, int index)
    {
        if (!TabTypeRegistry.TryResolveName((int)option.type, out _))
            return;

        TabRegistry.SetSelected((int)option.type, index);
        PassportExtenderAPI.RaiseOptionSelected((int)option.type, index);
    }
}
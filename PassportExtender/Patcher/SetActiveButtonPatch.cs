using System;
using HarmonyLib;
using PassportExtender.Core.Tab;
using PassportExtender.Core.Util;

namespace PassportExtender.Patcher;

[HarmonyPatch(typeof(PassportManager), "SetActiveButton", new Type[] { typeof(bool) })]
internal static class SetActiveButtonPatch
{
    [HarmonyPostfix]
    private static void Highlight(PassportManager __instance, bool fromTabChange, ref int __result)
    {
        var typeId = (int)__instance.activeType;

        if (!TabTypeRegistry.TryResolveName(typeId, out _)) return;

        var selected = TabRegistry.GetSelected(typeId);
        if (selected < 0) return;

        __result = selected;

        var bpp = (int)AccessTools.Field(typeof(PassportManager), "buttonsPerPage")
            .GetValue(__instance);
        var page = selected / bpp;

        for (var i = 0; i < __instance.buttons.Length; i++)
            __instance.buttons[i].border.color =
                selected == i + page * bpp
                    ? __instance.activeBorderColor
                    : __instance.inactiveBorderColor;

        Log.LogDebug($"[SetActiveButton] tab '{(TabTypeRegistry.TryResolveName(typeId, out var n) ? n : "?")}' highlighted index {selected}");
    }
}
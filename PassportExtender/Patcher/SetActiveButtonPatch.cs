using System;
using System.Reflection;
using HarmonyLib;
using PassportExtender.Core.Tab;
using PassportExtender.Core.Util;

namespace PassportExtender.Patcher;

[HarmonyPatch(typeof(PassportManager), "SetActiveButton", new Type[] { typeof(bool) })]
internal static class SetActiveButtonPatch
{
    private static readonly FieldInfo ButtonsPerPageFi =
        AccessTools.Field(typeof(PassportManager), "buttonsPerPage")
        ?? throw new InvalidOperationException(
            "PassportManager.buttonsPerPage not found — game update may have renamed it");
    
    [HarmonyPostfix]
    private static void Highlight(PassportManager __instance, bool fromTabChange, ref int __result)
    {
        var typeId = (int)__instance.activeType;

        if (!TabTypeRegistry.TryResolveName(typeId, out _)) return;


        var selected = TabRegistry.GetSelected(typeId);
        if (selected < 0)
            selected = __result;

        __result = selected;

        var bpp = (int)ButtonsPerPageFi.GetValue(__instance);
        var page = selected / bpp;

        for (var i = 0; i < __instance.buttons.Length; i++)
            __instance.buttons[i].border.color =
                selected == i + page * bpp
                    ? __instance.activeBorderColor
                    : __instance.inactiveBorderColor;

        Log.LogDebug($"[SetActiveButton] tab '{(TabTypeRegistry.TryResolveName(typeId, out var n) ? n : "?")}' highlighted index {selected}");
    }
}
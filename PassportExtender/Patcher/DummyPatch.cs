using HarmonyLib;
using PassportExtender.UI;

namespace PassportExtender.Patcher;
internal static class DummyPatch
{
    [HarmonyPatch(typeof(PlayerCustomizationDummy), "OnEnable")]
    [HarmonyPostfix]
    private static void OnEnable()
    {
        Scroller.RearmAll();
    }
}
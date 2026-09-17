using HarmonyLib;
using PassportExtender.Core;

namespace PassportExtender.Patcher;

[HarmonyPatch(typeof(AchievementManager), "IsAchievementUnlocked")]
internal static class IsAchievementUnlocked
{
    static bool Prefix(ref ACHIEVEMENTTYPE achievementType, ref bool __result)
    {
        if (!CustomAchievementReq.TryGetHandler((int)achievementType, out var handler)) return true;
        __result = handler();
        return false;

    }
}
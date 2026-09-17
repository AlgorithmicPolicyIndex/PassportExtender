using HarmonyLib;

namespace PassportExtender.Patcher;

[HarmonyPatch(typeof(AchievementManager), "IsAchievementUnlocked")]
static class IsAchievementUnlocked
{
    static bool Prefix(ref ACHIEVEMENTTYPE achievementType, ref bool __result)
    {
        if (CustomAchievementReq.TryGetHandler((int)achievementType, out var handler))
        {
            __result = handler();
            return false;
        }

        return true;
    }
}
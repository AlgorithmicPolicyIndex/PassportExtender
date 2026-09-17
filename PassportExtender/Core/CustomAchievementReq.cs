using System;
using System.Collections.Generic;
using HarmonyLib;

namespace PassportExtender.Patcher;

public static class CustomAchievementReq
{
    private static readonly Dictionary<int, Func<bool>> AchievementHandlers = new();

    public static ACHIEVEMENTTYPE DefineAchievement(string key, Func<bool> unlocked)
    {
        if (!Keys.TryGetValue(key, out var id))
        {
            id = _nextId++;
            Keys[key] = id;
        }
        AchievementHandlers[id] = unlocked;
        return (ACHIEVEMENTTYPE)id;
    }

    public static void Apply(CustomizationOption option, ACHIEVEMENTTYPE customAchievement)
    {
        var t = typeof(CustomizationOption);
        AccessTools.Field(t, "requiresAscent").SetValue(option, false);
        AccessTools.Field(t, "customRequirement").SetValue(option, CustomizationOption.CUSTOMREQUIREMENT.None);
        AccessTools.Field(t, "requiredAchievement").SetValue(option, customAchievement);
    }
    
    internal static bool TryGetHandler(int id, out Func<bool> handler)
        => AchievementHandlers.TryGetValue(id, out handler);
    
    private static readonly Dictionary<string, int> Keys = new();
    private static int _nextId = 1000;
}
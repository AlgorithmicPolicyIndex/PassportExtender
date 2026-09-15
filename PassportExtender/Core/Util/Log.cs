using BepInEx.Logging;

namespace PassportExtender.Core.Util;

internal static class Log
{
    public static ManualLogSource Source;

    public static void LogDebug(string msg)
    {
        if (Main.DebugLogs != null && Main.DebugLogs.Value)
            Source.LogDebug(msg);
    }

    public static void LogInfo(string msg)  => Source.LogInfo(msg);
    public static void LogWarning(string msg) => Source.LogWarning(msg);
    public static void LogError(string msg) => Source.LogError(msg);
}
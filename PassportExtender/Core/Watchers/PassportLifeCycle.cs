using PassportExtender.Core.Util;
using UnityEngine;

namespace PassportExtender.Core.Watchers;

internal sealed class PassportLifecycleWatcher : MonoBehaviour
{
    private Character _lastSeenCharacter;
    private PlayerCustomizationDummy _lastSeenDummy;
    private static PassportLifecycleWatcher _instance;

    internal static void EnsureExists()
    {
        if (_instance != null) return;
        var go = new GameObject("PassportExtender_LifecycleWatcher");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<PassportLifecycleWatcher>();
        Log.LogDebug("[PassportLifecycleWatcher] created");
    }

    private void Update()
    {
        var local = Character.localCharacter;
        if (local != null && local != _lastSeenCharacter)
        {
            _lastSeenCharacter = local;
            PassportExtenderAPI.RaiseLocalCharacterChanged(local);
        }

        var mgr = PassportManager.instance;
        var dummy = mgr != null ? mgr.dummy : null;
        if (dummy == null || dummy == _lastSeenDummy) return;
        
        _lastSeenDummy = dummy;
    }
}
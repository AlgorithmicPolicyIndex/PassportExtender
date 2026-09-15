using System.Collections;
using PassportExtender.Core;
using PassportExtender.Core.Tab;
using PassportExtender.Core.Util;
using UnityEngine;

namespace PassportExtender.UI;

internal class ExtenderRestorer : MonoBehaviour
{
    private static ExtenderRestorer _instance;
    private Character _lastSeenLocal;

    internal static void EnsureExists()
    {
        if (_instance != null) return;
        var go = new GameObject("PassportExtender_Restorer");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<ExtenderRestorer>();
    }

    private void Update()
    {
        var local = Character.localCharacter;
        if (local != null && local != _lastSeenLocal)
        {
            _lastSeenLocal = local;
            StartCoroutine(ApplyWhenReady(local));
        }
        var dummy = PassportManager.instance?.dummy;
        if (dummy == null || dummy == _lastSeenDummy) return;
        _lastSeenDummy = dummy;
        Log.LogDebug($"[Restorer] dummy = {dummy}");
        PassportExtenderAPI.RaiseDummyShown(dummy);
    }

    private PlayerCustomizationDummy _lastSeenDummy;

    private static IEnumerator ApplyWhenReady(Character character)
    {
        const float timeout = 10f;
        var deadline = Time.realtimeSinceStartup + timeout;

        while (Time.realtimeSinceStartup < deadline)
        {
            if (TabRegistry.RestoreSelections(character))
                yield break;

            yield return new WaitForSeconds(0.25f);
        }

        Log.LogWarning("[Restorer] timed out waiting for consumers to apply initial equips");
    }
}
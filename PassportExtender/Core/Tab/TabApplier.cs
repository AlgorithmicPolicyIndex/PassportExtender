using System;
using System.Linq;
using JetBrains.Annotations;
using PassportExtender.Core.Util;
using PassportExtender.UI;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PassportExtender.Core.Tab;

internal static class TabApplier
{
    [CanBeNull] private static GameObject _template;
    [CanBeNull] private static PassportManager _cachedManager;

    public static void Init(PassportManager manager)
    {
        _cachedManager = manager;
        _template = manager.tabs.FirstOrDefault(t => t?.type == Customization.Type.Skin)?.gameObject;
        if (_template != null) return;
        Log.LogError("[TabApplier] No Skin tab found – Tab Creation failed...");
    }

    public static bool TryGetCurrentManager([CanBeNull] out PassportManager manager)
    {
        manager = _cachedManager;
        return manager != null;
    }

    internal static void CreateTab(PassportManager manager, TabDefinition def)
    {
        if (_template == null)
        {
            Log.LogError("[TabApplier] Template uninitialized – Cannot create tab.");
            return;
        }

        var clone = Object.Instantiate(_template, _template.transform.parent);
        clone.name = $"UI_PETab_{def.Name}";

        var passportTab = clone.GetComponent<PassportTab>();
        if (passportTab == null)
        {
            Log.LogWarning($"[TabApplier] {clone.name} lacks PassportTab component.");
            Object.Destroy(clone.gameObject);
            return;
        }

        var iconTransform = clone.transform.Find("Panel/Icon");
        var rawImage = iconTransform != null ? iconTransform.GetComponent<RawImage>() : null;
        if (rawImage == null)
        {
            Log.LogWarning($"[TabApplier] {clone.name}: no RawImage at Panel/Icon – icon not set. Falling back...");
        }
        else rawImage.texture = def.Icon;
        
        passportTab.type = (Customization.Type)def.Type.Id;

        var index = manager.tabs.Length;

        Array.Resize(ref manager.tabs, manager.tabs.Length + 1);
        manager.tabs[^1] = passportTab;
        clone.transform.SetSiblingIndex(index);

        Array.Resize(ref manager.tabs, manager.tabs.Length + 1);
        manager.tabs[^1] = passportTab;

        var grid = clone.transform.parent;
        var layout = grid.GetComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = manager.tabs.Length;
        
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)clone.transform.parent);

        Scroller.RearmArrows(manager);
        Log.LogInfo($"[TabApplier] Created tab '{def.Name}' at index {index}");
    }
}
using System;
using PassportExtender.Core.Util;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Object = UnityEngine.Object;

namespace PassportExtender.UI;

internal static class ArrowBuilder
{
    public static Button Build(PassportManager manager, RectTransform grid, Transform donor, int dir, System.Action onClick)
    {
        var uiParent = (RectTransform)grid.parent;
        var clone = Object.Instantiate(donor.gameObject, uiParent, false);
        clone.name = dir < 0 ? "Extender_ScrollLeft" : "Extender_ScrollRight";
        clone.SetActive(true);

        Object.Destroy(clone.GetComponent<PassportButton>());

        var pageNum = clone.transform.Find($"Box/PageNum{(dir < 0 ? " (1)" : "")}");
        if (pageNum != null)
        {
            var label = pageNum.GetComponent<TextMeshProUGUI>();
            if (label != null)
                label.text = dir < 0 ? "Left" : "Right";
        }
        else
        {
            Log.LogWarning($"[ArrowBuilder] PageNum label not found under {clone.name}");
        }

        var btn = clone.GetComponent<Button>();
        btn.onClick = new Button.ButtonClickedEvent();
        btn.onClick.AddListener(() => SafeScroll(manager, dir));

        var rt = (RectTransform)clone.transform;
        rt.SetAsLastSibling();
        rt.localRotation = Quaternion.Euler(0, 0, 90);
        rt.localRotation = dir < 0 ? Quaternion.Euler(0, 0, 90) :  Quaternion.Euler(0, 0, 270);
        rt.localScale = new Vector3(0.5918f, 0.4364f, 1);
        rt.localPosition = dir > 0
            ? new Vector3(170.8403f, -14.1436f, 0)
            : new Vector3(-170.8403f, -14.1436f, 0);

        return btn;
    }
    
    private static void SafeScroll(PassportManager manager, int dir)
    {
        try
        {
            Scroller.ScrollBy(manager, dir);
        }
        catch (Exception e)
        {
            Log.LogError($"[Scroller] click handler threw:\n{e}");
        }
    }
}
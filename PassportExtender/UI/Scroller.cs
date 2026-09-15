using System.Collections.Generic;
using System.Linq;
using PassportExtender.Core.Tab;
using PassportExtender.Core.Util;
using UnityEngine;
using UnityEngine.UI;

namespace PassportExtender.UI;

internal static class Scroller
{
    private static readonly Dictionary<PassportManager, ScrollState> States = new();

    private sealed class ScrollState
    {
        public RectTransform Grid;
        public GridLayoutGroup Layout;
        public Button Left, Right;
        public float BaselineX;
        public float CachedMaxScroll = -1f;
        public float TargetX;
        public Coroutine ActiveTween;
    }

    public static void EnsureScroller(PassportManager manager)
    {
        if (States.ContainsKey(manager)) return;

        var template = manager.tabs.FirstOrDefault(t => t.type == Customization.Type.Mouth);
        if (template == null)
        {
            Log.LogError("[Scroller] no Mouth tab — cannot locate the tab grid");
            return;
        }

        var grid = (RectTransform)template.transform.parent;
        var layout = grid.GetComponent<GridLayoutGroup>();
        if (layout == null)
        {
            Log.LogError("[Scroller] no GridLayoutGroup on tab container");
            return;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(grid);

        var s = new ScrollState
        {
            Grid = grid,
            Layout = layout,
            BaselineX = grid.anchoredPosition.x
        };
        DumpChildren(s.Grid);
        States.Add(manager, s);

        Log.LogDebug($"[Scroller] state created — baseline {s.BaselineX:F2}");

        BuildArrows(manager, s);
        UpdateArrows(s);
    }

    public static void RearmArrows(PassportManager manager)
    {
        if (!States.TryGetValue(manager, out var s)) return;
        Host.StartCoroutine(MeasureAfterLayout(s));
    }

    private static System.Collections.IEnumerator MeasureAfterLayout(ScrollState s)
    {
        yield return new WaitUntil(() => s.Grid != null && s.Grid.gameObject.activeInHierarchy);

        var prev = -999f;
        var stable = 0;
        var guard = 0;
        while (stable < 2 && guard++ < 40)
        {
            yield return new WaitForSeconds(0.15f);
            var cur = ComputeMaxScroll(s);
            if (Mathf.Abs(cur - prev) < 0.5f) stable++;
            else stable = 0;
            prev = cur;
        }

        s.CachedMaxScroll = prev;
        DumpChildren(s.Grid);
        Log.LogDebug($"[Scroller] measured maxScroll = {s.CachedMaxScroll:F2} (converged after {guard} samples)");
        UpdateArrows(s);
    }

    private static void BuildArrows(PassportManager manager, ScrollState s)
    {
        var donorUp = manager.transform.Find("PassportUI/Canvas/Panel/Panel/BG/Options/Stuff/Options/UI_PassportUp");
        var donorDown =
            manager.transform.Find("PassportUI/Canvas/Panel/Panel/BG/Options/Stuff/Options/UI_PassportDown");
        if (donorUp == null || donorDown == null)
        {
            Log.LogError("[Scroller] arrow donor not found — arrows disabled");
            return;
        }

        s.Left = ArrowBuilder.Build(manager, s.Grid, donorUp, dir: -1, onClick: () => ScrollBy(manager, -1));
        s.Right = ArrowBuilder.Build(manager, s.Grid, donorDown, dir: +1, onClick: () => ScrollBy(manager, +1));
    }

    private static float ComputeMaxScroll(ScrollState s)
    {
        var corners = new Vector3[4];
        s.Grid.GetWorldCorners(corners);
        var gridRight = corners[2].x;

        var contentRight  = gridRight;
        var vanillaRight  = gridRight;
        var   anyCustom    = false;

        foreach (RectTransform child in s.Grid)
        {
            var tab = child.GetComponent<PassportTab>();
            if (tab == null) continue;

            child.GetWorldCorners(corners);
            var r = corners[2].x;
            contentRight = Mathf.Max(contentRight, r);

            if (TabTypeRegistry.TryResolveName((int)tab.type, out _))
                anyCustom = true;
            else
                vanillaRight = Mathf.Max(vanillaRight, r);
        }

        if (!anyCustom) return 0f;

        var designMargin = vanillaRight - gridRight;
        var totalOverflow = contentRight - gridRight;

        var scrollableWorld = Mathf.Max(0f, totalOverflow - designMargin);

        var scale = s.Grid.lossyScale.x != 0f ? s.Grid.lossyScale.x : 1f;
        return scrollableWorld / scale;
    }

    public static void ScrollBy(PassportManager manager, int dir)
    {
        var s = States[manager];
        var step = s.Layout.cellSize.x + s.Layout.spacing.x;
        var maxScroll = GetMaxScroll(s);

        var startX = s.TargetX;

        if (s.ActiveTween == null)
            startX = s.Grid.anchoredPosition.x;

        var target = Mathf.Clamp(startX - dir * step,
            s.BaselineX - maxScroll,
            s.BaselineX);

        s.TargetX = target;

        if (s.ActiveTween != null)
            Host.StopCoroutine(s.ActiveTween);
        s.ActiveTween = Host.StartCoroutine(TweenTo(s, startX, target));

        Log.LogDebug($"[Scroll] step={step:F1} max={maxScroll:F1} from={startX:F1} to={target:F1}");
    }
    
    private static System.Collections.IEnumerator TweenTo(ScrollState s, float from, float to)
    {
        const float duration = 0.18f;
        var elapsed = 0f;
        var delta = to - from;

        if (Mathf.Abs(delta) < 0.01f) { s.ActiveTween = null; yield break; }
        UpdateArrows(s);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Ease-out quad: fast start, gentle settle — no jarring stop
            float eased = 1f - (1f - t) * (1f - t);
            s.Grid.anchoredPosition = new Vector2(from + delta * eased,
                s.Grid.anchoredPosition.y);
            yield return null;
        }
        
        s.Grid.anchoredPosition = new Vector2(to, s.Grid.anchoredPosition.y);
        s.ActiveTween = null;
        UpdateArrows(s);
    }

    // === VISIBILITY ==========================================================

    private static void UpdateArrows(ScrollState state)
    {
        var maxScroll = state.CachedMaxScroll >= 0f ? state.CachedMaxScroll : GetMaxScroll(state);
        var scrollable = maxScroll > 0.5f;

        var x = state.Grid.anchoredPosition.x;
        var thresholdLeft = state.BaselineX - 0.5f;
        var thresholdRight = state.BaselineX - maxScroll + 0.5f;

        var leftCanClick = scrollable && x < thresholdLeft;
        var rightCanClick = scrollable && x > thresholdRight;

        state.Left.interactable = leftCanClick;
        state.Right.interactable = rightCanClick;
        
        SetOpacity(state.Left.transform.Find("Box").gameObject,  leftCanClick  ? 1f : 0.35f);
        SetOpacity(state.Right.transform.Find("Box").gameObject, rightCanClick ? 1f : 0.35f);

        state.Left.image?.CrossFadeAlpha(leftCanClick ? 1f : 0.5f, 0.1f, true); 

        Log.LogDebug($"[Scroller] UpdateArrows: maxScroll={maxScroll:F2}, x={x:F2}, L={leftCanClick}, R={rightCanClick}");
    }
    
    private static void SetOpacity(GameObject root, float alpha)
    {
        foreach (var g in root.GetComponentsInChildren<Graphic>())
        {
            if (g == null) continue;
            var c = g.color;
            c.a = alpha;
            g.color = c;
        }
    }

    private static void DumpChildren(RectTransform grid)
    {
        var corners = new Vector3[4];
        grid.GetWorldCorners(corners);
        var vpRight = corners[2].x;

        Log.LogDebug($"[Scroller] Viewport right = {vpRight:F2}. Children:");
        foreach (RectTransform child in grid)
        {
            child.GetWorldCorners(corners);
            var right = corners[2].x;
            var isTab = child.GetComponent<PassportTab>() != null;
            var pastEdge = right > vpRight;

            Log.LogDebug(
                $"    {child.name}: right={right:F2}, (+{right - vpRight:F2}), isTab={isTab}, pastEdge={pastEdge}");
        }
    }

    public static void RearmAll()
    {
        foreach (var state in States.Keys.ToList().Where(state => state != null))
        {
            Host.StartCoroutine(MeasureAfterLayout(States[state]));
            Log.LogDebug("[Scroller] re-armed via DummyPatch");
        }
    }

    private static float GetMaxScroll(ScrollState s)
        => s.CachedMaxScroll >= 0f ? s.CachedMaxScroll : ComputeMaxScroll(s);

    private sealed class CoroutineHost : MonoBehaviour { }

    private static CoroutineHost _host;

    private static CoroutineHost Host
    {
        get
        {
            if (_host != null) return _host;

            var go = new GameObject("PassportExtender_ScrollerHost");
            _host = go.AddComponent<CoroutineHost>();
            Object.DontDestroyOnLoad(go);
            return _host;
        }
    }
}

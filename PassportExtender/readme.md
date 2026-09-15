# PassportExtender

A UI library mod for **PEAK**. Extends the vanilla passport UI with an API
other mods can consume to add their own tabs, options, and customization
types — without touching the UI themselves.

> **Status:** Early testing. Tabs work, options work, scrolling works.
> It makes me happy. There is a long road ahead.

## What is this?

Just testing a UI mod for PEAK, really. I'm building an API to extend the
vanilla UI for my own mods — custom cosmetic tabs, new body types,
whatever the game's `Customization` system can hold.

The bigger dream: reuse the passport as a *panel*, not just a character sheet.
Imagine dropping a passport down on a table in-game to declare roles, show
text, reveal "innocent" cards for social deduction mods — the passport UI
already looks like a document; it wants to be used that way. Not there yet.
Today it's tabs, and tabs are a start.

## What it does (today)

- Custom passport tabs, created by cloning the game's own tab —
  pixel-identical to vanilla, hover and select behavior included
- Custom `Customization.Type` registry — allocate an ID at runtime,
  no conflicts between mods
- Options served through the game's own grid (paging, highlights,
  lock overlays — all vanilla behavior)
- A tab-strip **scroller** with smooth eased panning and arrow buttons
  (the vanilla strip was never built to hold more tabs; it overflows by
  design — this fixes that for everyone)
- Selection restore: equips your saved customization to the player
  even if they never open the passport
- Dependency-ordered loading — register tabs whenever your plugin wakes
  up, even *after* the passport already exists

## For mod developers
Reference the DLL, and your entire passport integration is one `RegisterTab` call:

```csharp
[BepInDependency(PassportExtender.Main.Guid)]
[BepInPlugin("my.handle.exampleplugin", "ExamplePlugin", "0.0.1")]
public class ExamplePlugin : BaseUnityPlugin { ... }
```

See [Example Plugin](../ExamplePlugin/ExamplePlugin.cs) for a very simple Boilerplate built entirely in its Awake function. 
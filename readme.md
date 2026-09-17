# PassportExtender

A UI library mod for **PEAK**. Extends the vanilla passport UI with an API
other mods can consume to add their own tabs, options, and customization
types — without touching the UI themselves.

> **Status:** Early testing. Specified for Cosmetic mods, currently being generalized.  
> Further expanding on the UI to allow more usage OUTSIDE of Cosmetics and for other mods that use the UI in more special and unique ways.

## What is this?

PassportExtender (PE) is a UI Library mod, originally built for me to make a library for a simple cosmetic mod while I learn C# and Modding, now turning into something a little more feature complete.

Another mod idea I had was built around using the Passport UI, such as the background, to scroll down just a little bit and allow me to put text like "innocent" or "Sabotager" (although a more Among Us based mod now exists.)  
So, now this library is being updated to further expand the UI so much, that you can do ANYTHING with the Passport UI.

## A Note
  PE may never be fully complete or ready to be "released" as a proper, continuously maintained mod. I, as a programmer, tend to get creative ideas every once in a while, write, work, rarely complete projects. I work on 2 sometimes even up to 5 projects at a time, that never see the light of day.  
I have, on occasion, fully completed projects that I release, but then never go further with. In one case, my Carrot Hunter Discord bot, became fully complete, I had to just create shop items. I later started working on a fight command. I eventually stopped working on it. Though I come back to it every couple of months.  
This is just due to the fact I lost motivation to work on it or say lack of use out of it and saw no further point to work on it.

I hope to use this library as a difference for myself, but of course, since Peak isn't receiving MASSIVE game changing updates, there will be a point this mod is "finalized".  
With that being said, I am COMPLETELY OKAY with people forking and editing and distributing my mod

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

## Upcoming Changes
- Breaking
  - Generalized Functions
    - OnInitialEquip -> OnManagerSpawn
    - This is so your mod can use the Passport mod for things OUTSIDE of cosmetics. While you can already do this.
    - Known Issue:
      - Things like the returned Character and Index in OnInitialEquip, will be removed. Depending mods will need to handle the character themselves.
  - Separating Cosmetic functions such as SetOption (see [SetOptionPatch](Patcher/SetOptionPatch.cs))
    - The `__instance.dummy.UpdateDummy();`, used to update the dummy viewer in Passport, will be moved to a more specific function for depending mods.
- Planned
  - Use default Passport for the cosmetic mods (a Cosmetic/Non-Cosmetic typing)
  - Creation of a new Passport menu (switchable from normal Passport menu?)
    - Allows Non-Cosmetic mods an area to create whatever they want using the same methods.
    - This is just so people can create a mod that gets a little less cluttered area.
  - Allow mods to append more options to pre-existing tabs, Including Vanilla

## For mod developers
Reference the DLL, and your entire passport integration is one `RegisterTab` call:

```csharp
[BepInDependency(PassportExtender.Main.Guid)]
[BepInPlugin("my.handle.exampleplugin", "ExamplePlugin", "0.0.1")]
public class ExamplePlugin : BaseUnityPlugin { ... }
```

See [Example Plugin](../ExamplePlugin/ExamplePlugin.cs) for a very simple example (using all functions, albeit simple) built entirely in its Awake function.

## License

PassportExtender is licensed under the
[CC BY-NC-SA 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/) license.

You are free to use, modify, and distribute this mod for non-commercial
purposes. Forks must credit this project, state that they are a fork,
and share under the same license.

---

Copyright (c) 2026 AlgorithmicPolicyIndex · [PassportExtender Github](https://github.com/AlgorithmicPolicyIndex/PassportExtender) ·
PassportExtender is an unofficial community mod, not affiliated with
Aggro Crab, Landfall, or the developers of PEAK.

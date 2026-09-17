/*
 * In this Example Plugin, I am assuming you are making a Cosmetic item Plugin
 * However, you can wire these default functions to do whatever you need your mod to do.
 * PassportExtender (PE) is used to only interact with the UI systems and return functions for you to use.
 * In a later update, I will generalize functions like OnInitialEquip to be something like OnManagerSpawn instead.
 */

using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using PassportExtender.Core;
using PassportExtender.Core.Tab;
using PassportExtender.Patcher;
using UnityEngine;

namespace ExamplePlugin
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(PassportExtender.Main.Guid)]
    public class ExamplePlugin : BaseUnityPlugin
    {
        // Plugin Data
        private const string Guid = "my.example.plugin";
        private const string Name = "ExamplePlugin";
        private const string Version = "0.0.1";
        public static bool randomReq = false;
        
        // Config
        internal static ConfigEntry<bool> DebugLogs;
        private static ConfigEntry<int> SavedItemIndex { get; set; }
        
        private static int _type;
        
        private void Awake()
        {
            Log.Source = Logger;
            
            // Config Binders
            SavedItemIndex = Config.Bind("General", "SavedItemIndex", 0,
                "Last equipped item item from the passport tab.\n0 = none.");
            DebugLogs = Config.Bind("General", "Debug_Logs", false, "Enable Verbose [DEBUG] output.");

            // Get TabType from PE Runtime
            if (!PassportExtenderAPI.TryRegisterType(Guid, out var itemType))
            {
                // Prevent Duplicate tab names.
                Log.LogWarning($"Type ({itemType}) has already been registered. – Skipping tab.");
                return;
            }

            // _type is an int, required to cast to Customization.Type
            _type = itemType.Id;

            var options = new List<CustomizationOption>
            {
                // This is for a NONE item, so you can remove items.
                // Think Hair (Tab 2, (Customization.Type)20), which has an empty slot.
                BuildOption("mouthmesh_none", MakeExampleIcon([
                    new Color32(0xff, 0xff, 0xff, 0x00),
                    new Color32(0xff, 0xff, 0xff, 0x00)
                ])),
                BuildOption("Example1", MakeExampleIcon([
                    new Color32(0xBB, 0x9c, 0x34, 0xff),
                    new Color32(0x2D, 0xBB, 0x79, 0xFF)
                ])),
                BuildOption("LockedItem", MakeExampleIcon([
                    new Color32(0x24, 0x06, 0xBB, 0xFF),
                    new Color32(0x6E,0xBB, 0xAE, 0xFF)
                ]))
            };

            // Using some Func<bool>, say, game completion has to be 80% or higher (SomeMod.completion >= 80)
            // Sets a dynamically item locker. It affects ALL items using this requirement.
            
            // This example dynamically reads randomReq at the call time. So item unlocks when randomReq == true.
            // Inside the C# Console in Unity Explorer, ExamplePlugin.ExamplePlugin.randomReq = true, will unlock the item
            // Note: Item icon is only affected upon reinitialization of Passport Options.
            // Note: However, selection of the item is not and may be selected after setting true with Passport open.
            var requirement = CustomAchievementReq.DefineAchievement("my.achievement", () => randomReq);
            // Apply requirement type to CustomizationOption.
            CustomAchievementReq.Apply(options.Find(t => t.name == "LockedItem"), requirement);
            
            PassportExtenderAPI.RegisterTab(new TabDefinition()
            {
                // Use your Guid, it's easiest
                // Tab will be UI_PETab_my.example.plugin
                Name = Guid,
                // The type the tab will fetch during Options GetList
                Type = itemType,
                
                Icon = MakeExampleIcon([
                    new Color32(0xBB, 0x18, 0x7F, 0xff),
                    new Color32(0x49, 0x32, 0xBB, 0xff)
                ]),
                // Array of Items to be show in tab.
                Options = options.ToArray(),
                InitialSelection = SavedItemIndex.Value, // Sets the selected item in Passport
                OnInitialEquip = (chara, idx) =>
                    // Returns Character and Item Index.
                    // IE: You load into a run, you have no passport.
                    // This will run so you can run your equip function.
                {
                    Log.LogInfo($"Returned Character: {chara}, Item Index: {idx}");
                    return true;
                    // Your mod returns status on 
                    // true: Item successfully equipped
                    // false: Item failed to equip
                },
                
                // When selecting your option, this will return the index selected, so you can equip your item.
                OnOptionSelected = index =>
                {
                    Log.LogInfo($"Selected Option: {index}");
                }
            });
        }
        
        private static Texture2D MakeExampleIcon(Color32[] colors)
        {
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            var px = new Color32[64 * 64];
            for (var y = 0; y < 64; y++)
            for (var x = 0; x < 64; x++)
                px[y * 64 + x] = ((x / 8 + y / 8) % 2 == 0) ? colors[0] : colors[1];
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }
        
        private static CustomizationOption BuildOption(string name, Texture icon)
        {
            var opt = ScriptableObject.CreateInstance<CustomizationOption>();
            opt.name = name;
            opt.type = (Customization.Type)_type;
            opt.texture = icon;
            // you can pass (int) and cast with (ACHIEVEMENT)n to set a custom requirement.
            // casting a unknown ACHIEVEMENT state does cause a Unity Warning, but can be disregarded.
            opt.requiredAchievement = 0; 
            return opt;
        }
    }
}
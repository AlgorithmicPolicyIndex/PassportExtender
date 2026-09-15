using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using PassportExtender.Core;
using PassportExtender.Core.Tab;
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
            
            PassportExtenderAPI.RegisterTab(new TabDefinition()
            {
                // Use your Guid, it's easier. It's the name of the Tab
                Name = Guid,
                // The type the tab will fetch during Options GetList
                Type = itemType,
                
                Icon = MakeExampleIcon([
                    new Color32(0xBB, 0x18, 0x7F, 0xff),
                    new Color32(0x49, 0x32, 0xBB, 0xff)
                ]),
                // Array of Items to be show in tab.
                Options = options.ToArray(),
                InitialSelection = SavedItemIndex.Value,
                // OnInitialEquip = (character, index) => {}
                // Returns Character and Item Index.
                // IE: You load into a run, you have no passport.
                // This will make sure it applies your items to the character
                
                // The 
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
            opt.requiredAchievement = 0; // Require certain game achievement for unlock. (not tested)
            return opt;
        }
    }
}
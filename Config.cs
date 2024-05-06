using RedLoader;
using SonsSdk.Attributes;
using TMPro;
using UnityEngine;

namespace TrapReset;

public static class Config
{
    public static ConfigCategory Category { get; private set; }
    public static ConfigCategory ResetTraps { get; private set; }
    [SettingsUiHeader("Enable this after you add new or remove existing traps(Can cause short lag spike)", TextAlignmentOptions.MidlineLeft, true)]
    public static ConfigEntry<bool> CheckForTrapChange { get; private set; }
    public static ConfigEntry<bool> AnimalTrap { get; private set; }
    public static ConfigEntry<bool> DefenseTraps { get; private set; }
    public static ConfigEntry<bool> SpringTraps { get; private set; }
    public static ConfigEntry<string> ResetTimer { get; private set; }

    public static Dictionary<string, float> multiplierdict = new()
    {
        { "0", 0f }, { "1", 1f }, { "2", 2f }, { "3", 3f }, { "4", 4f }, { "5", 5f },
        { "10", 10f }, { "15", 15f }, { "20", 20f }, { "30", 30f }, { "40", 40f }, { "50", 50f },
        { "60", 60f }, { "120", 120f }, { "180", 180f }, { "240", 240f }, { "300", 300f }
    };

    //public static ConfigEntry<bool> SomeEntry { get; private set; }

    public static void Init()
    {

        string defaultMultiplierKey = "15";

        if (!multiplierdict.ContainsKey(defaultMultiplierKey))
        {
            RLog.Msg("Couldnt find value, shit's borked");
        }
        Category = ConfigSystem.CreateFileCategory("TrapCheck", "TrapCheck", "TrapReset.cfg");
        ResetTraps = ConfigSystem.CreateFileCategory("TrapReset", "TrapReset", "TrapReset.cfg");

        CheckForTrapChange = Category.CreateEntry("Checktrapchange", false, "Check for New/Removed traps", "", false);
        AnimalTrap = ResetTraps.CreateEntry("AnimalTrap", false, "AnimalTrap", "", false);
        DefenseTraps = ResetTraps.CreateEntry("DefenseTrap", false, "DefenseTraps", "", false);
        SpringTraps = ResetTraps.CreateEntry("SpringTrap", false, "SpringTraps", "", false);
        ResetTimer = ResetTraps.CreateEntry("ResetTimer", defaultMultiplierKey, "Trap Reset Interval(In seconds)", "", false);
        ResetTimer.SetOptions(multiplierdict.Keys.ToArray());

    }

}
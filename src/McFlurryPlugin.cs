using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine.SceneManagement;
using System;
using UnityEngine;
using UnityEngine.Analytics;
using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace McFlurryMenu;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
public partial class McFlurryPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    public static McFlurryPlugin Plugin;
    public new static ManualLogSource Log;

    // UI Component References
    public static MenuUI menuUI;
    public static ConsoleUI consoleUI;
    public static RolesUI rolesUI;
    public static DoorsUI doorsUI;
    public static TasksUI tasksUI;
    public static ProtectUI protectUI;
    public static KeybindListener keybindListener;

    public static string mcFlurryVersion = "1.0.0"; 
    public static List<string> supportedAU = new List<string> { "2026.2.24", "2026.3.17", "2026.3.31" };
    public static bool isPanicked = false;
    public static bool inStealthMode = false;

    // Configuration Entries
    public static ConfigEntry<string> menuKeybind;
    public static ConfigEntry<string> menuHtmlColor;
    public static ConfigEntry<bool> menuOpenOnMouse;
    public static ConfigEntry<string> spoofLevel;
    public static ConfigEntry<string> spoofPlatform;
    public static ConfigEntry<bool> spoofDeviceId;
    public static ConfigEntry<bool> noTelemetry;
    public static ConfigEntry<string> guestFriendCode;
    public static ConfigEntry<bool> guestMode;
    public static ConfigEntry<bool> autoLoadProfile;

    public override void Load()
    {
        Log = base.Log;
        Plugin = this;

        Log.LogInfo("Churning the McFlurry... Loading McFlurryMenu.");

        // --- GUI Configuration ---
        menuKeybind = Config.Bind("McFlurryMenu.GUI", "Keybind", "Delete", 
            "The keyboard key used to toggle the GUI on and off.");

        menuHtmlColor = Config.Bind("McFlurryMenu.GUI", "Color", "", 
            "A custom color for your McFlurryMenu GUI. Supports html color codes.");

        menuOpenOnMouse = Config.Bind("McFlurryMenu.GUI", "OpenOnMouse", true, 
            "When enabled, the McFlurryMenu GUI will always be opened at the current mouse position.");

        // --- Profile Configuration ---
        autoLoadProfile = Config.Bind("McFlurryMenu.Profile", "AutoLoadProfile", false, 
            "When enabled, your saved keybind and toggle profile will be automatically loaded at game startup.");

        // --- Spoofing Configuration ---
        spoofLevel = Config.Bind("McFlurryMenu.Spoofing", "Level", "", 
            "A custom player level to display to others (1 - 100001).");

        spoofPlatform = Config.Bind("McFlurryMenu.Spoofing", "Platform", "", 
            "A custom gaming platform to display to others.");

        // --- Privacy Configuration ---
        spoofDeviceId = Config.Bind("McFlurryMenu.Privacy", "HideDeviceId", true, 
            "When enabled, it will hide your unique deviceId from Among Us.");

        noTelemetry = Config.Bind("McFlurryMenu.Privacy", "NoTelemetry", true, 
            "When enabled, it will stop Among Us from sending analytics to Innersloth.");

        // Initialize Passive Passives
        CheatToggles.unlockFeatures = CheatToggles.freeCosmetics = CheatToggles.avoidPenalties = true;

        // Apply Patches
        Harmony.PatchAll();

        // Attach UI Sub-Systems
        menuUI = AddComponent<MenuUI>();
        consoleUI = AddComponent<ConsoleUI>();
        rolesUI = AddComponent<RolesUI>();
        doorsUI = AddComponent<DoorsUI>();
        tasksUI = AddComponent<TasksUI>();
        protectUI = AddComponent<ProtectUI>();

        // Logic Components
        keybindListener = AddComponent<KeybindListener>();

        // Handle Privacy Settings
        if (noTelemetry.Value)
        {
            Analytics.enabled = false;
            Analytics.deviceStatsEnabled = false;
            PerformanceReporting.enabled = false;
            Log.LogInfo("Privacy: Telemetry Disabled.");
        }

        // Auto-load profile if configured
        if (autoLoadProfile.Value)
        {
            CheatToggles.LoadTogglesFromProfile();
        }

        // Handle Scene Transitions
        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) ((scene, _) =>
        {
            if (scene.name == "MainMenu" && !(inStealthMode || isPanicked))
            {
                if (!supportedAU.Contains(Application.version))
                {
                    Utils.ShowPopup($"\nMcFlurryMenu Version {mcFlurryVersion}\nWarning: Unsupported Game Version ({Application.version}) detected.");
                }
            }
        }));
    }
}

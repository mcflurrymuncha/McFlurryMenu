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

namespace MalumMenu;


 [BepInPlugin("com.mcflurry.mcflurrymenu", "McFlurryMenu", "1.0.0")]
// [BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
public partial class MalumMenu : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    public static MalumMenu Plugin;
    public new static ManualLogSource Log;

    public static MenuUI menuUI;
    public static ConsoleUI consoleUI;
    public static RolesUI rolesUI;
    public static DoorsUI doorsUI;
    public static TasksUI tasksUI;
    public static ProtectUI protectUI;
    public static KeybindListener keybindListener;

    // Updated version string name
    public static string mcflurryVersion = "1.0.0"; 
    public static List<string> supportedAU = new List<string> { "2026.2.24", "2026.3.17", "2026.3.31" };
    public static bool isPanicked = false;
    public static bool inStealthMode = false;

    public static ConfigEntry<string> menuKeybind;
    public static ConfigEntry<string> menuHtmlColor;
    public static ConfigEntry<bool> menuOpenOnMouse;
    public static ConfigEntry<bool> menuKeepSubwindowsOpen;
    public static ConfigEntry<string> spoofLevel;
    public static ConfigEntry<string> spoofPlatform;
    public static ConfigEntry<bool> spoofDeviceId;
    public static ConfigEntry<bool> noTelemetry;
    public static ConfigEntry<string> guestFriendCode;
    public static ConfigEntry<bool> guestMode;
    public static ConfigEntry<bool> autoLoadProfile;
    public static ConfigEntry<string> configEditor;

    public override void Load()
    {
        Log = base.Log;
        Plugin = this;

        // Updated Config Section Names to McFlurryMenu.GUI
        menuKeybind = Config.Bind("McFlurryMenu.GUI",
                                "Keybind",
                                "Delete",
                                "The keyboard key used to toggle the GUI on and off.");

        menuHtmlColor = Config.Bind("McFlurryMenu.GUI",
                                "Color",
                                "",
                                "A custom color for your McFlurryMenu GUI.");

        menuOpenOnMouse = Config.Bind("McFlurryMenu.GUI",
                                "OpenOnMouse",
                                false,
                                "When enabled, the McFlurryMenu GUI will always be opened at the current mouse position");

        menuKeepSubwindowsOpen = Config.Bind("McFlurryMenu.GUI",
                                "KeepSubwindowsOpen",
                                false,
                                "When enabled, closing the McFlurryMenu GUI will not automatically close its subwindows");

        autoLoadProfile = Config.Bind("McFlurryMenu.Profile",
                                "AutoLoadProfile",
                                false,
                                "When enabled, your saved profile will be automatically loaded");

        configEditor = Config.Bind("McFlurryMenu.Config",
                                "ConfigEditor",
                                "notepad.exe",
                                "The program used to open the config file");

        spoofLevel = Config.Bind("McFlurryMenu.Spoofing",
                                "Level",
                                "",
                                "A custom player level to display to others");

        spoofPlatform = Config.Bind("McFlurryMenu.Spoofing",
                                "Platform",
                                "",
                                "A custom gaming platform to display to others");

        spoofDeviceId = Config.Bind("McFlurryMenu.Privacy",
                                "HideDeviceId",
                                true,
                                "When enabled, it will hide your unique deviceId");

        noTelemetry = Config.Bind("McFlurryMenu.Privacy",
                                "NoTelemetry",
                                true,
                                "When enabled, it will stop Among Us from collecting analytics");

        // Passives are enabled by default
        CheatToggles.unlockFeatures = CheatToggles.freeCosmetics = CheatToggles.avoidPenalties = true;

        Harmony.PatchAll();

        // UI Initialization
        menuUI = AddComponent<MenuUI>();
        consoleUI = AddComponent<ConsoleUI>();
        rolesUI = AddComponent<RolesUI>();
        doorsUI = AddComponent<DoorsUI>();
        tasksUI = AddComponent<TasksUI>();
        protectUI = AddComponent<ProtectUI>();

        keybindListener = AddComponent<KeybindListener>();

        if (noTelemetry.Value)
        {
            Analytics.enabled = false;
            Analytics.deviceStatsEnabled = false;
            PerformanceReporting.enabled = false;
        }

        if (autoLoadProfile.Value)
        {
            CheatToggles.LoadTogglesFromProfile();
        }

        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) ((scene, _) =>
        {
            if (scene.name == "MainMenu" && !(inStealthMode || isPanicked))
            {
                if (!supportedAU.Contains(Application.version))
                {
                    // Updated the warning message text
                    Utils.ShowPopup("\nThis version of McFlurryMenu and this version of Among Us are incompatible\n\nInstall the right version to avoid problems");
                }
            }
        }));
    }
}

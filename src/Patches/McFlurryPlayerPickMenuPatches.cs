using AmongUs.Data;
using HarmonyLib;
using UnityEngine;
using Il2CppSystem.Collections.Generic;

namespace McFlurryMenu;

[HarmonyPatch(typeof(ShapeshifterMinigame), nameof(ShapeshifterMinigame.Begin))]
public static class ShapeshifterMinigame_Begin
{
    // Prefix patch of ShapeshifterMinigame.Begin to implement player pick menu logic
    public static bool Prefix(ShapeshifterMinigame __instance)
    {
        // Rebranded reference to McFlurryPlayerPickMenu
        if (!McFlurryPlayerPickMenu.isActive) return true; 

        // Custom player list set by openPlayerPickMenu
        List<NetworkedPlayerInfo> playerList = McFlurryPlayerPickMenu.customPlayerList;

        __instance.potentialVictims = new List<ShapeshifterPanel>();
        List<UiElement> selectableElements = new List<UiElement>();

        for (int i = 0; i < playerList.Count; i++)
        {
            NetworkedPlayerInfo playerData = playerList[i];

            int num = i % 3;
            int num2 = i / 3;
            ShapeshifterPanel shapeshifterPanel = Object.Instantiate(__instance.PanelPrefab, __instance.transform);
            shapeshifterPanel.transform.localPosition = new Vector3(__instance.XStart + num * __instance.XOffset, __instance.YStart + num2 * __instance.YOffset, -1f);

            shapeshifterPanel.SetPlayer(i, playerData, (Il2CppSystem.Action) (() =>
            {
                McFlurryPlayerPickMenu.targetPlayerData = playerData; // Save targeted player
                McFlurryPlayerPickMenu.customAction.Invoke(); // Custom action set by openPlayerPickMenu

                __instance.Close();
            }));

            if (playerData.Object != null)
            {
                shapeshifterPanel.NameText.text = Utils.GetNameTag(playerData, playerData.DefaultOutfit.PlayerName);

                // Move and resize the nametag to prevent it overlapping with colorblind text
                if (CheatToggles.seeRoles && CheatToggles.seePlayerInfo)
                {
                    shapeshifterPanel.NameText.transform.localPosition = new Vector3(0.33f, 0.08f, 0f);
                    shapeshifterPanel.NameText.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                }
                else if (CheatToggles.seeRoles || CheatToggles.seePlayerInfo)
                {
                    shapeshifterPanel.NameText.transform.localPosition = new Vector3(0.3384f, 0.1125f, -0.1f);
                    shapeshifterPanel.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                }
                else
                {
                    // Reset nametag to default values
                    shapeshifterPanel.NameText.transform.localPosition = new Vector3(0.3384f, 0.0311f, -0.1f);
                    shapeshifterPanel.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                }
            }

            __instance.potentialVictims.Add(shapeshifterPanel);
            selectableElements.Add(shapeshifterPanel.Button);
        }

        ControllerManager.Instance.OpenOverlayMenu(__instance.name, __instance.BackButton, __instance.DefaultButtonSelected, selectableElements, false);

        McFlurryPlayerPickMenu.isActive = false;

        return false; // Skip original method when McFlurry pick menu is active
    }
}

[HarmonyPatch(typeof(ShapeshifterPanel), nameof(ShapeshifterPanel.SetPlayer))]
public static class ShapeshifterPanel_SetPlayer
{
    // Prefix patch to allow usage of McFlurryPlayerPickMenu in lobbies
    public static bool Prefix(ShapeshifterPanel __instance, int index, NetworkedPlayerInfo playerInfo, Il2CppSystem.Action onShift)
    {
        if (!McFlurryPlayerPickMenu.isActive) return true; 

        __instance.shapeshift = onShift;

        __instance.PlayerIcon.SetFlipX(false);
        __instance.PlayerIcon.ToggleName(false);

        SpriteRenderer[] componentsInChildren = __instance.GetComponentsInChildren<SpriteRenderer>();
        foreach (var spriteRenderer in componentsInChildren)
        {
            spriteRenderer.material.SetInt(PlayerMaterial.MaskLayer, index + 2);
        }

        __instance.PlayerIcon.SetMaskLayer(index + 2);
        __instance.PlayerIcon.UpdateFromEitherPlayerDataOrCache(playerInfo, PlayerOutfitType.Default, PlayerMaterial.MaskType.ComplexUI, false, null);

        __instance.LevelNumberText.text = ProgressionManager.FormatVisualLevel(playerInfo.PlayerLevel);

        // Uses the standard name to avoid breaking the UI layout in lobbies
        __instance.NameText.text = playerInfo.PlayerName;

        DataManager.Settings.Accessibility.OnColorBlindModeChanged += (Il2CppSystem.Action)__instance.SetColorblindText;
        __instance.SetColorblindText();

        return false; // Skips original method when McFlurry pick menu is active
    }
}

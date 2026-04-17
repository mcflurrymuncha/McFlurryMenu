using HarmonyLib;
using InnerNet;
using UnityEngine;
using System;

namespace MalumMenu;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class KickAllCheat
{
    public static void Postfix(HudManager __instance)
    {
        // 1. Basic safety
        if (MalumMenu.isPanicked) return;
        
        // Don't trigger if typing
        if (__instance.Chat && __instance.Chat.IsOpenOrOpening) return;

        // 2. The B Key Logic
        if (Input.GetKeyDown(KeyCode.B))
        {
            // If the client or player list is null, we can't do anything
            if (AmongUsClient.Instance == null || PlayerControl.AllPlayerControls == null) return;

            // Optional: Internal notification to check if the key registered
            Debug.Log("[McFlurry] Attempting to kick everyone...");

            // 3. The Loop
            // We use a try-catch to prevent the game from crashing if a player leaves mid-loop
            try 
            {
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    // Don't kick yourself!
                    if (player == null || player == PlayerControl.LocalPlayer) continue;

                    // Direct call to the network client to boot the player
                    // Parameter 1: Player ID (byte)
                    // Parameter 2: Is Ban? (bool)
                    AmongUsClient.Instance.KickPlayer(player.PlayerId, false);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[McFlurry] Error during Kick-All: {e.Message}");
            }
        }
    }
}

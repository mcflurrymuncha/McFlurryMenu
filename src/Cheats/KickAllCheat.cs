using HarmonyLib;
using InnerNet;
using UnityEngine;

namespace MalumMenu;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class KickAllCheat
{
    // HUD Update runs every frame while the game UI is active
    public static void Postfix()
    {
        // 1. Safety Checks
        if (MalumMenu.isPanicked) return;
        
        // Don't trigger if the chat is open
        if (HudManager.InstanceExists && HudManager.Instance.Chat && HudManager.Instance.Chat.IsOpenOrOpening) return;

        // 2. The B Key Logic
        if (Input.GetKeyDown(KeyCode.B))
        {
            // Ensure we are in a game/lobby and have host authority
            if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
            {
                // Iterate through all players
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    // Skip yourself and invalid players
                    if (player == null || player == PlayerControl.LocalPlayer) continue;

                    // Send the kick request (false = kick, true = ban)
                    AmongUsClient.Instance.KickPlayer(player.PlayerId, false);
                }
            }
        }
    }
}

using HarmonyLib;
using InnerNet;
using UnityEngine;
using System.Collections.Generic;

namespace MalumMenu;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Update))]
public static class KickAllCheat
{
    // This runs every frame on the Local Player
    public static void Postfix(PlayerControl __instance)
    {
        // 1. Only run for the local player to avoid running 15x per frame
        if (!__instance.AmLocalPlayer) return;

        // 2. Safety checks (Panicked or Chat open)
        if (MalumMenu.isPanicked) return;
        if (HudManager.InstanceExists && HudManager.Instance.Chat && HudManager.Instance.Chat.IsOpenOrOpening) return;

        // 3. The B Key Logic
        if (Input.GetKeyDown(KeyCode.B))
        {
            // Among Us server-side kicking ONLY works if you are the Host
            if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
            {
                // Create a copy of the list to avoid collection modified errors
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    // Don't kick yourself
                    if (player == null || player.AmLocalPlayer) continue;

                    // Send the kick request to the server
                    // false = Kick, true = Ban
                    AmongUsClient.Instance.KickPlayer(player.PlayerId, false);
                }
                
                Debug.Log("[McFlurry] Host initiated Kick-All via B key.");
            }
            else
            {
                Debug.LogWarning("[McFlurry] Kick-All failed: You are not the lobby host.");
            }
        }
    }
}

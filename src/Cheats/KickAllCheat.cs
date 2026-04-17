using HarmonyLib;
using InnerNet;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace MalumMenu;

[HarmonyPatch(typeof(StatsManager), nameof(StatsManager.Update))]
public static class KickAllCheat
{
    public static void Postfix()
    {
        if (MalumMenu.isPanicked) return;

        // Trigger on B key
        if (Input.GetKeyDown(KeyCode.B))
        {
            // Verify client and Host status
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.IsHost) return;

            Debug.Log("[McFlurry] IsHost verified. Executing Kick-All...");

            // Use a list copy to prevent "Collection Modified" errors during the loop
            List<PlayerControl> playerList = new List<PlayerControl>(PlayerControl.AllPlayerControls);

            foreach (var player in playerList)
            {
                // Skip if null, local player, or if they have no data
                if (player == null || player == PlayerControl.LocalPlayer || player.Data == null) continue;

                try
                {
                    // Call the network kick
                    // false = Kick, true = Ban
                    AmongUsClient.Instance.KickPlayer(player.PlayerId, false);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[McFlurry] Failed to kick player {player.PlayerId}: {e.Message}");
                }
            }
        }
    }
}

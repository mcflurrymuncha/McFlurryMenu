using UnityEngine;
using InnerNet;

namespace MalumMenu;

public class KeybindListener : MonoBehaviour
{
    public void Update()
    {
        if (MalumMenu.isPanicked) return;

        // Keybinds aren't triggered from typing in the chat
        if (HudManager.InstanceExists && HudManager.Instance.Chat && HudManager.Instance.Chat.IsOpenOrOpening) return;

        // Kick everyone when B is pressed
        if (Input.GetKeyDown(KeyCode.B))
        {
            // Simple debug to see if the key works at all
            Debug.Log("[MalumMenu] B Key Pressed - Attempting Kick All");

            // Check if we are in a game/lobby first
            if (AmongUsClient.Instance == null || PlayerControl.AllPlayerControls == null) return;

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                // Skip yourself and skip null data
                if (player == null || player == PlayerControl.LocalPlayer || player.Data == null) continue;
                
                // Try to kick
                AmongUsClient.Instance.KickPlayer(player.PlayerId, false);
            }
        }

        // Handle your other dynamic keybinds
        if (CheatToggles.Keybinds != null)
        {
            foreach (var (name, key) in CheatToggles.Keybinds)
            {
                if (key == KeyCode.None) continue;
                if (!Input.GetKeyDown(key)) continue;

                if (CheatToggles.ToggleFields.TryGetValue(name, out var field))
                {
                    var current = (bool)field.GetValue(null);
                    field.SetValue(null, !current);
                }
            }
        }
    }
}

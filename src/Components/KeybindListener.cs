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
            // Only the host can effectively kick players from the server
            if (AmongUsClient.Instance.AmHost)
            {
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    // Check if the player is the local user
                    if (player == PlayerControl.LocalPlayer) continue;
                    
                    // Call the singleton instance directly
                    AmongUsClient.Instance.KickPlayer(player.PlayerId, false);
                }
            }
        }

        // Check each keybind to see if the user pressed it and toggle the corresponding cheat
        foreach (var (name, key) in CheatToggles.Keybinds)
        {
            if (key == KeyCode.None) continue;
            if (!Input.GetKeyDown(key)) continue;

            if (!CheatToggles.ToggleFields.TryGetValue(name, out var field)) continue;

            var current = (bool)field.GetValue(null);
            field.SetValue(null, !current);
        }
    }
}

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

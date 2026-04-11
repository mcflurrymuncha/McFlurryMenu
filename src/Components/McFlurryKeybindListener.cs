using UnityEngine;
using BepInEx.Configuration;

namespace McFlurryMenu; // Ensure this matches the rest of your files

public class KeybindListener : MonoBehaviour
{
    private void Update()
    {
        // 1. Check if the Menu Toggle key is pressed (e.g., Delete key)
        if (Input.GetKeyDown(Utils.GetKeyCodeFromConfig(McFlurryPlugin.menuKeybind)))
        {
            if (McFlurryPlugin.isPanicked) return; // Don't allow menu during Panic Mode

            MenuUI.isGUIActive = !MenuUI.isGUIActive;
            
            // If the menu is being opened and 'OpenOnMouse' is enabled, reposition the window
            if (MenuUI.isGUIActive && McFlurryPlugin.menuOpenOnMouse.Value)
            {
                MenuUI.UpdateWindowPositionToMouse();
            }
        }

        // 2. Panic Keybind (Hardcoded or could be added to config)
        // Instantly closes all UI and disables features for stealth
        if (Input.GetKeyDown(KeyCode.End))
        {
            ExecutePanic();
        }
    }

    private void ExecutePanic()
    {
        McFlurryPlugin.isPanicked = true;
        MenuUI.isGUIActive = false;
        
        // Reset dangerous toggles immediately
        CheatToggles.ResetAllToggles();
        
        McFlurryPlugin.Log.LogWarning("PANIC MODE ACTIVATED: All features disabled.");
    }
}

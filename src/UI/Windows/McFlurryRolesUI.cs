using UnityEngine;

namespace McFlurryMenu;

public class RolesUI : MonoBehaviour
{
    private Vector2 _scrollPosition = Vector2.zero;
    private Rect _windowRect = new(320, 10, 450, 100);

    private void OnGUI()
    {
        // Safety check using rebranded McFlurry logic
        if (!CheatToggles.showRolesMenu || !MenuUI.isGUIActive || McFlurryPlugin.isPanicked) return;

        UIHelpers.ApplyUIColor();

        _windowRect = GUI.Window((int)WindowId.RolesUI, _windowRect, (GUI.WindowFunction)RolesWindow, "McFlurry Role Assigner");
    }

    private void RolesWindow(int windowID)
    {
        GUILayout.BeginVertical();

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            // Validation: Only showing the local player for role forcing in this specific iteration
            if (!player.Data || !player.Data.Role || string.IsNullOrEmpty(player.Data.PlayerName) || player != PlayerControl.LocalPlayer) continue;

            GUILayout.BeginHorizontal();

            // Display Local Player Name in their color
            GUILayout.Label($"<color=#{ColorUtility.ToHtmlStringRGB(player.Data.Color)}>{player.Data.PlayerName}</color>", GUILayout.Width(140f));
            
            GUILayout.BeginHorizontal();
            // Shows the currently selected 'forced' role from CheatToggles
            GUILayout.Label($"{CheatToggles.forcedRole}");
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reset", GUILayout.Width(80f)))
            {
                CheatToggles.forcedRole = null;
            }
            
            if (GUILayout.Button("Assign", GUILayout.Width(80f)))
            {
                CheatToggles.forceRole = true;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        // Informative footer for the user
        GUILayout.Label("Roles will be assigned on next game start");
        
        GUI.DragWindow();
    }
}

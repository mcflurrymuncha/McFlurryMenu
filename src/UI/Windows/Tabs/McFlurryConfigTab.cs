using UnityEngine;

namespace McFlurryMenu;

public class ConfigTab : ITab
{
    public string name => "Config";

    public void Draw()
    {
        // Aligned with the McFlurry Menu layout standards
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        // Rebranded configuration management
        CheatToggles.reloadConfig = GUILayout.Toggle(CheatToggles.reloadConfig, " Reload Config");

        CheatToggles.saveProfile = GUILayout.Toggle(CheatToggles.saveProfile, " Save to Profile");

        CheatToggles.loadProfile = GUILayout.Toggle(CheatToggles.loadProfile, " Load from Profile");

        /* Note: The logic for these toggles is typically handled in your 
           McFlurryPlugin.Update or a dedicated ConfigHandler class to 
           ensure the file I/O happens once per click.
        */
    }
}

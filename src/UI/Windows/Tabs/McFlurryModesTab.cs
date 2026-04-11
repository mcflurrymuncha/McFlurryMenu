using UnityEngine;

namespace McFlurryMenu;

public class ModesTab : ITab
{
    public string name => "Modes";

    public void Draw()
    {
        // Maintains the standard McFlurry Menu column width
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        // Visual "Ice Cream" RGB cycling logic
        CheatToggles.rgbMode = GUILayout.Toggle(CheatToggles.rgbMode, " RGB Mode");

        // Stealth and Safety modes
        CheatToggles.stealthMode = GUILayout.Toggle(CheatToggles.stealthMode, " Stealth Mode");

        CheatToggles.panicMode = GUILayout.Toggle(CheatToggles.panicMode, " Panic Mode");
    }
}

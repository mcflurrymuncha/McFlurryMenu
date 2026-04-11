using UnityEngine;

namespace McFlurryMenu;

public class ChatTab : ITab
{
    public string name => "Chat";

    public void Draw()
    {
        // Maintains the signature McFlurry Menu column width
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawTextbox();

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        // Global chat permissions and network bypasses
        CheatToggles.enableChat = GUILayout.Toggle(CheatToggles.enableChat, " Enable Chat");

        CheatToggles.bypassUrlBlock = GUILayout.Toggle(CheatToggles.bypassUrlBlock, " Bypass URL Block");

        CheatToggles.lowerRateLimits = GUILayout.Toggle(CheatToggles.lowerRateLimits, " Lower Rate Limits");
    }

    private void DrawTextbox()
    {
        GUILayout.Label("Textbox", GUIStylePreset.TabSubtitle);

        // Input-specific enhancements
        CheatToggles.unlockCharacters = GUILayout.Toggle(CheatToggles.unlockCharacters, " Unlock Extra Characters");

        CheatToggles.longerMessages = GUILayout.Toggle(CheatToggles.longerMessages, " Allow Longer Messages");

        CheatToggles.unlockClipboard = GUILayout.Toggle(CheatToggles.unlockClipboard, " Unlock Clipboard");
    }
}

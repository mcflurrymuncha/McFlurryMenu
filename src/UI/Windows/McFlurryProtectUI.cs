using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace McFlurryMenu;

public class ProtectUI : MonoBehaviour
{
    private Vector2 _scrollPosition = Vector2.zero;
    private Rect _windowRect = new(320, 10, 500, 300);
    public static List<PlayerControl> playersToProtect = new();
    private bool _keepEveryoneProtected;

    private void OnGUI()
    {
        // Safety checks using rebranded McFlurryPlugin and MenuUI logic
        if (!CheatToggles.showProtectMenu || !MenuUI.isGUIActive || McFlurryPlugin.isPanicked) return;

        UIHelpers.ApplyUIColor();

        _windowRect = GUI.Window((int)WindowId.ProtectUI, _windowRect, (GUI.WindowFunction)ProtectWindow, "McFlurry Protection");
    }

    private void ProtectWindow(int windowID)
    {
        GUILayout.BeginVertical();

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            // Data validation to prevent null reference errors on disconnected or uninitialized players
            if (!player.Data || !player.Data.Role || string.IsNullOrEmpty(player.Data.PlayerName))
            {
                if (playersToProtect.Contains(player))
                {
                    playersToProtect.Remove(player);
                }
                continue;
            }

            GUILayout.BeginHorizontal();

            // Display player name in their actual character color
            GUILayout.Label($"<color=#{ColorUtility.ToHtmlStringRGB(player.Data.Color)}>{player.Data.PlayerName}</color>", GUILayout.Width(140f));

            // Status label for shield state
            if (player.protectedByGuardianId == -1)
            {
                GUILayout.Label("<color=#FF0000>Unprotected</color>", GUILayout.Width(135));
            }
            else
            {
                NetworkedPlayerInfo guardianInfo = GameData.Instance.GetPlayerById((byte)player.protectedByGuardianId);
                GUILayout.Label($"<color=#00FF00>Protected</color> by <color=#{ColorUtility.ToHtmlStringRGB(guardianInfo.Color)}>{guardianInfo._object.Data.PlayerName}</color>", GUILayout.Width(135));
            }

            // Immediate RPC protect (Requires Host)
            if (GUILayout.Button("Protect", GUIStylePreset.NormalButton) && Utils.isHost && !Utils.isLobby)
            {
                PlayerControl.LocalPlayer.RpcProtectPlayer(player, player.cosmetics.ColorId);
            }

            // Persistent protection toggle
            var keepProtected = playersToProtect.Contains(player);
            keepProtected = GUILayout.Toggle(keepProtected, "Keep protected", GUIStylePreset.NormalToggle);

            if (keepProtected && !playersToProtect.Contains(player))
            {
                playersToProtect.Add(player);
            }
            else if (!keepProtected && playersToProtect.Contains(player))
            {
                playersToProtect.Remove(player);
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();

        // Mass protection controls
        if (GUILayout.Button("Protect Everyone") && Utils.isHost && !Utils.isLobby)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                PlayerControl.LocalPlayer.RpcProtectPlayer(player, player.cosmetics.ColorId);
            }
        }

        GUILayout.FlexibleSpace();

        _keepEveryoneProtected = GUILayout.Toggle(_keepEveryoneProtected, "Keep Everyone Protected");

        if (_keepEveryoneProtected)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!playersToProtect.Contains(player))
                {
                    playersToProtect.Add(player);
                }
            }
        }
        else
        {
            // Reset logic for the mass-protect toggle
            if (PlayerControl.AllPlayerControls.Count == playersToProtect.Count)
            {
                playersToProtect.Clear();
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUI.DragWindow();
    }
}

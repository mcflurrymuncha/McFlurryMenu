using Il2CppSystem;
using UnityEngine;
using System.Collections.Generic;

namespace McFlurryMenu;

public class ConsoleUI : MonoBehaviour
{
    private static Vector2 _scrollPosition = Vector2.zero;
    private static List<string> _logEntries = new();
    private const int MaxLogEntries = 300;
    private Rect _windowRect = new(320, 10, 550, 350);
    private GUIStyle _logStyle;

    public static void Log(string message)
    {
        // Limit the number of logs to keep memory usage in check
        if (_logEntries.Count >= MaxLogEntries)
        {
            _logEntries.RemoveAt(0); // Remove the oldest log entry
        }

        _logEntries.Add(message);

        // Auto-scroll to the bottom for new entries
        _scrollPosition.y = float.MaxValue;
    }

    private void OnGUI()
    {
        // Check for rebranded CheatToggles and the McFlurryPlugin panic state
        if (!CheatToggles.showConsole || !MenuUI.isGUIActive || McFlurryPlugin.isPanicked) return;

        _logStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 16
        };

        // Apply the ice-cream themed UI colors
        UIHelpers.ApplyUIColor();

        _windowRect = GUI.Window((int)WindowId.ConsoleUI, _windowRect, (GUI.WindowFunction)ConsoleWindow, "McFlurry Console");
    }

    private void ConsoleWindow(int windowID)
    {
        GUILayout.BeginVertical(GUI.skin.box);

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false);

        foreach (var log in _logEntries)
        {
            GUILayout.Label(log, _logStyle);
        }

        GUILayout.EndScrollView();

        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();

        // UI Controls for log management
        if (GUILayout.Button("Clear Log", GUILayout.Width(260)))
        {
            _logEntries.Clear();
        }

        if (GUILayout.Button("Copy Log to Clipboard"))
        {
            GUIUtility.systemCopyBuffer = String.Join("\n", _logEntries.ToArray());
        }

        GUILayout.EndHorizontal();

        // Allow users to move the console around the screen
        GUI.DragWindow();
    }
}

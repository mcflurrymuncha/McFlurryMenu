using System.Linq;
using UnityEngine;

namespace McFlurryMenu;

public class TasksUI : MonoBehaviour
{
    private Vector2 _scrollPosition = Vector2.zero;
    private Rect _windowRect = new(320, 10, 500, 300);
    private GUIStyle _playerHeaderStyle;
    private Il2CppSystem.Text.StringBuilder _tasksString = new();
    private readonly System.Collections.Generic.Dictionary<string, bool> _expandedPlayers = new();

    private void OnGUI()
    {
        // Safety check using rebranded McFlurryPlugin and MenuUI logic
        if (!CheatToggles.showTasksMenu || !MenuUI.isGUIActive || McFlurryPlugin.isPanicked) return;

        _playerHeaderStyle ??= new GUIStyle(GUI.skin.button)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleLeft
        };

        // Apply the ice-cream themed colors
        UIHelpers.ApplyUIColor();

        _windowRect = GUI.Window((int)WindowId.TasksUI, _windowRect, (GUI.WindowFunction)TasksWindow, "McFlurry Task Tracker");
    }

    private void TasksWindow(int windowID)
    {
        GUILayout.BeginVertical();

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player.Data || !player.Data.Role || string.IsNullOrEmpty(player.Data.PlayerName)) continue;

            GUILayout.BeginVertical();

            var nameKey = player.Data.PlayerName;
            _expandedPlayers.TryGetValue(nameKey, out var expanded);
            var arrow = expanded ? "\u25BC" : "\u25B6"; // ▼ or ▶

            // Calculate progress
            var taskCount = player.myTasks.Count;
            var completeCount = player.myTasks.ToArray().Count(t => t.IsComplete);

            // Filtering out non-actual tasks (dead hints, sabotage info, etc.)
            if (player == PlayerControl.LocalPlayer && player.Data.IsDead)
            {
                taskCount -= 1;
            }
            if (player == PlayerControl.LocalPlayer && Utils.isAnySabotageActive)
            {
                taskCount -= 1;
            }
            if (player == PlayerControl.LocalPlayer && player.Data.Role.IsImpostor)
            {
                taskCount -= 1;
            }

            // Dropdown button for each player
            if (GUILayout.Button($"{arrow} [{completeCount}/{taskCount}] <color=#{ColorUtility.ToHtmlStringRGB(player.Data.Color)}>{nameKey}</color>", _playerHeaderStyle))
            {
                _expandedPlayers[nameKey] = !expanded;
                expanded = !expanded;
            }

            if (expanded)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(15f); // Indent tasks
                GUILayout.BeginVertical();

                foreach (var task in player.myTasks)
                {
                    // Skip technical sabotage/role-related "tasks"
                    if (task.TaskType is TaskTypes.ResetReactor or TaskTypes.RestoreOxy or TaskTypes.FixLights or TaskTypes.FixComms or TaskTypes.ResetSeismic or TaskTypes.StopCharles or TaskTypes.MushroomMixupSabotage) continue;

                    _tasksString.Clear();
                    task.AppendTaskText(_tasksString);
                    var taskText = _tasksString.ToString();

                    // Filter out UI flavor text
                    if (taskText.Contains("You're dead") || taskText.Contains("Sabotage and kill")) continue;

                    GUILayout.BeginHorizontal();
                    // Clean rich text tags for a cleaner console-like look
                    GUILayout.Label(taskText.Replace("\n", "").Replace("</color>", "").Replace("<color=#00DD00FF>", "").Replace("<color=#FFFF00FF>", ""));
                    GUILayout.FlexibleSpace();

                    if (task.IsComplete)
                    {
                        GUILayout.Label("<color=#00ff00>✔ Complete</color>");
                    }
                    else
                    {
                        // Only allow the user to complete their OWN tasks via the UI
                        if (player == PlayerControl.LocalPlayer)
                        {
                            if (GUILayout.Button("Complete", GUIStylePreset.NormalButton))
                            {
                                Utils.CompleteTask(task);
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        GUILayout.EndScrollView();

        // Mass-action button at the bottom
        if (GUILayout.Button("Complete All My Tasks", GUIStylePreset.NormalButton))
        {
            CheatToggles.completeMyTasks = true;
        }

        GUILayout.EndVertical();

        GUI.DragWindow();
    }
}

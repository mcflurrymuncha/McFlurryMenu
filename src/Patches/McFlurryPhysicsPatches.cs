using System;
using HarmonyLib;
using UnityEngine;

namespace McFlurryMenu;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.LateUpdate))]
public static class PlayerPhysics_LateUpdate
{
    public static void Postfix(PlayerPhysics __instance)
    {
        // ESP and Visuals
        McFlurryESP.PlayerNametags(__instance);
        McFlurryESP.SeeGhostsCheat(__instance);

        // General Cheats
        McFlurryCheats.NoClipCheat();
        McFlurryCheats.ReviveCheat();
        McFlurryCheats.ProtectCheat();
        McFlurryCheats.KillAllCheat();
        McFlurryCheats.KillAllCrewCheat();
        McFlurryCheats.KillAllImpsCheat();
        McFlurryCheats.ForceStartGameCheat();
        McFlurryCheats.TeleportCursorCheat();
        McFlurryCheats.CompleteMyTasksCheat();
        McFlurryCheats.PlayAnimationCheat();
        McFlurryCheats.PlayScannerCheat();

        // Player Pick Menu (PPM) Cheats
        McFlurryPPMCheats.EjectPlayerPPM();
        McFlurryPPMCheats.SpectatePPM();
        McFlurryPPMCheats.KillPlayerPPM();
        McFlurryPPMCheats.TelekillPlayerPPM();
        McFlurryPPMCheats.TeleportPlayerPPM();
        McFlurryPPMCheats.ChangeRolePPM();
        McFlurryPPMCheats.ForceRolePPM();

        // Tracers
        McFlurryTracersHandler.DrawPlayerTracer(__instance);

        GameObject[] bodyObjects = GameObject.FindGameObjectsWithTag("DeadBody");
        foreach(GameObject bodyObject in bodyObjects) // Finds and loops through all dead bodies
        {
            DeadBody deadBody = bodyObject.GetComponent<DeadBody>();

            if (!deadBody || deadBody.Reported) continue;  // Only draw tracers for unreported dead bodies
            McFlurryTracersHandler.DrawBodyTracer(deadBody);
        }

        // Control Logic
        try
        {
            if (CheatToggles.invertControls)
            {
                PlayerControl.LocalPlayer.MyPhysics.Speed = -Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.Speed);
                PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = -Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed);
            }
            else
            {
                PlayerControl.LocalPlayer.MyPhysics.Speed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.Speed);
                PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed);
            }
        } 
        catch (NullReferenceException) { }
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleAnimation))]
public static class PlayerPhysics_HandleAnimation
{
    // Prefix patch of PlayerPhysics.HandleAnimation to disable walking animation for Moonwalk
    public static bool Prefix(PlayerPhysics __instance)
    {
        if (CheatToggles.moonWalk && __instance.AmOwner)
        {
            __instance.ResetAnimState();
            return false;
        }

        return true;
    }
}

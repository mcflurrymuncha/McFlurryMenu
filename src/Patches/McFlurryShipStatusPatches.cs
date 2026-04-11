using HarmonyLib;

namespace McFlurryMenu;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
public static class ShipStatus_FixedUpdate
{
    public static void Postfix(ShipStatus __instance)
    {
        // Rebranded Sabotage and Global Cheat processing
        McFlurrySabotageCheats.Process(__instance);
        McFlurryCheats.OpenSabotageMapCheat();

        // Meeting Control Cheats
        McFlurryCheats.CloseMeetingCheat();
        McFlurryCheats.SkipMeetingCheat();
        McFlurryCheats.CallMeetingCheat();
        
        // Vent Interaction Cheats
        McFlurryCheats.WalkInVentCheat();
        McFlurryCheats.KickVentsCheat();

        // Player Pick Menu (PPM) Actions
        McFlurryPPMCheats.ReportBodyPPM();
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
public static class FungleShipStatus_FixedUpdate
{
    public static void Postfix(FungleShipStatus __instance)
    {
        // Specific processing for The Fungle map's unique sabotage logic
        McFlurrySabotageCheats.ProcessFungle(__instance);
    }
}

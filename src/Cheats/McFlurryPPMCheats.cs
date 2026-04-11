using Il2CppSystem.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils;
using System;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace McFlurryMenu;

public static class McFlurryPPMCheats
{
    private static bool _telekillPlayerActive;
    private static bool _killPlayerActive;
    private static bool _spectateActive;
    private static bool _teleportPlayerActive;
    private static bool _reportBodyActive;
    private static bool _ejectPlayerActive;
    private static bool _changeRoleActive;
    private static bool _forceRoleActive;
    private static RoleTypes? _oldRole = null;

    public static void ReportBodyPPM()
    {
        if (CheatToggles.reportBody)
        {
            if (!_reportBodyActive)
            {
                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("reportBody");
                }

                // Player pick menu to choose any body (alive or dead) and report it
                PlayerPickMenu.OpenPlayerPickMenu(Utils.GetAllPlayerData(), (Action) (() =>
                {
                    PlayerControl.LocalPlayer.CmdReportDeadBody(PlayerPickMenu.targetPlayerData);
                }));

                _reportBodyActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.reportBody = false;
            }
        }
        else
        {
            if (_reportBodyActive)
            {
                _reportBodyActive = false;
            }
        }
    }

    public static void EjectPlayerPPM()
    {
        if (CheatToggles.ejectPlayer)
        {
            if (!_ejectPlayerActive)
            {
                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("ejectPlayer");
                }

                if (!Utils.isMeeting)
                {
                    CheatToggles.ejectPlayer = false;
                    return;
                }

                List<NetworkedPlayerInfo> playerInfo = new List<NetworkedPlayerInfo>();
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.Data.IsDead && !player.Data.Disconnected)
                    {
                        playerInfo.Add(player.Data);
                    }
                }

                // Player pick menu to choose any living player and eject them during meeting
                PlayerPickMenu.OpenPlayerPickMenu(playerInfo, (Action)(() =>
                {
                    NetworkedPlayerInfo playerToEject = PlayerPickMenu.targetPlayerData;
                    MeetingHud.Instance.RpcVotingComplete(new Il2CppStructArray<MeetingHud.VoterState>(0L), playerToEject, false);
                }));

                _ejectPlayerActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.ejectPlayer = false;
            }
        }
        else if (_ejectPlayerActive)
        {
            _ejectPlayerActive = false;
        }
    }

    public static void KillPlayerPPM()
    {
        if (CheatToggles.killPlayer)
        {
            if (!_killPlayerActive)
            {
                // Close any player pick menus already open & their cheats
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("killPlayer");
                }

                if (Utils.isLobby)
                {
                    HudManager.Instance.Notifier.AddDisconnectMessage("Killing in lobby disabled for being too buggy");
                    CheatToggles.killPlayer = false;
                    return;
                }

                // Player pick menu made for killing any player by sending a successful MurderPlayer RPC call
                PlayerPickMenu.OpenPlayerPickMenu(Utils.GetAllPlayerData(), (Action)(() =>
                {
                    Utils.MurderPlayer(PlayerPickMenu.targetPlayerData.Object, MurderResultFlags.Succeeded);
                }));

                _killPlayerActive = true;
            }

            // Deactivate cheat if menu is closed
            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.killPlayer = false;
            }
        }
        else if (_killPlayerActive)
        {
            _killPlayerActive = false;
        }
    }

    public static void TelekillPlayerPPM()
    {
        if (CheatToggles.telekillPlayer)
        {
            if (!_telekillPlayerActive)
            {
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("telekillPlayer");
                }

                if (Utils.isLobby)
                {
                    HudManager.Instance.Notifier.AddDisconnectMessage("Killing in lobby disabled for being too buggy");
                    CheatToggles.telekillPlayer = false;
                    return;
                }

                PlayerPickMenu.OpenPlayerPickMenu(Utils.GetAllPlayerData(), (Action)(() =>
                {
                    var oldPos = PlayerControl.LocalPlayer.GetTruePosition();
                    Utils.MurderPlayer(PlayerPickMenu.targetPlayerData.Object, MurderResultFlags.Succeeded);
                    AmongUsClient.Instance.StartCoroutine(Utils.DelayedSnapTo(oldPos));
                }));

                _telekillPlayerActive = true;
            }

            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.telekillPlayer = false;
            }
        }
        else if (_telekillPlayerActive)
        {
            _telekillPlayerActive = false;
        }
    }

    public static void TeleportPlayerPPM()
    {
        if (CheatToggles.teleportPlayer)
        {
            if (!_teleportPlayerActive)
            {
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("teleportPlayer");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.AmOwner)
                    {
                        playerDataList.Add(player.Data);
                    }
                }

                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action)(() =>
                {
                    PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(PlayerPickMenu.targetPlayerData.Object.transform.position);
                }));

                _teleportPlayerActive = true;
            }

            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.teleportPlayer = false;
            }
        }
        else if (_teleportPlayerActive)
        {
            _teleportPlayerActive = false;
        }
    }

    public static void ChangeRolePPM()
    {
        if (CheatToggles.setFakeRole)
        {
            if (!_changeRoleActive)
            {
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("changeRole");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                if (_oldRole == RoleTypes.Shapeshifter || Utils.isFreePlay)
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Shapeshifter", OutfitPreset.Shapeshifter, Utils.GetBehaviourByRoleType(RoleTypes.Shapeshifter)));

                if (_oldRole == RoleTypes.Phantom || Utils.isFreePlay)
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Phantom", OutfitPreset.Phantom, Utils.GetBehaviourByRoleType(RoleTypes.Phantom)));

                if (_oldRole == RoleTypes.Viper || Utils.isFreePlay)
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Viper", OutfitPreset.Viper, Utils.GetBehaviourByRoleType(RoleTypes.Viper)));

                if ((_oldRole != null && Utils.GetBehaviourByRoleType((RoleTypes)_oldRole).TeamType == RoleTeamTypes.Impostor) || Utils.isFreePlay || Utils.isHost)
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Impostor", OutfitPreset.Impostor, Utils.GetBehaviourByRoleType(RoleTypes.Impostor)));

                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Tracker", OutfitPreset.Tracker, Utils.GetBehaviourByRoleType(RoleTypes.Tracker)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Noisemaker", OutfitPreset.Noisemaker, Utils.GetBehaviourByRoleType(RoleTypes.Noisemaker)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Engineer", OutfitPreset.Engineer, Utils.GetBehaviourByRoleType(RoleTypes.Engineer)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Scientist", OutfitPreset.Scientist, Utils.GetBehaviourByRoleType(RoleTypes.Scientist)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Detective", OutfitPreset.Detective, Utils.GetBehaviourByRoleType(RoleTypes.Detective)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Crewmate", OutfitPreset.Crewmate, Utils.GetBehaviourByRoleType(RoleTypes.Crewmate)));

                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action) (() =>
                {
                    if (!Utils.isLobby && !Utils.isFreePlay && _oldRole == null)
                    {
                        _oldRole = PlayerControl.LocalPlayer.Data.RoleType;
                    }

                    if (PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, 
                            PlayerPickMenu.targetPlayerData.Role.TeamType == RoleTeamTypes.Impostor ? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost);
                    }
                    else
                    {
                        RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, PlayerPickMenu.targetPlayerData.Role.Role);
                    }
                }));

                _changeRoleActive = true;
            }

            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.setFakeRole = false;
            }
        }
        else if (_changeRoleActive)
        {
            _changeRoleActive = false;
        }
    }

    public static void ForceRolePPM()
    {
        if (CheatToggles.forceRole)
        {
            if (!_forceRoleActive)
            {
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("forceRole");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Shapeshifter", OutfitPreset.Shapeshifter, Utils.GetBehaviourByRoleType(RoleTypes.Shapeshifter)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Phantom", OutfitPreset.Phantom, Utils.GetBehaviourByRoleType(RoleTypes.Phantom)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Viper", OutfitPreset.Viper, Utils.GetBehaviourByRoleType(RoleTypes.Viper)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Impostor", OutfitPreset.Impostor, Utils.GetBehaviourByRoleType(RoleTypes.Impostor)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Tracker", OutfitPreset.Tracker, Utils.GetBehaviourByRoleType(RoleTypes.Tracker)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Noisemaker", OutfitPreset.Noisemaker, Utils.GetBehaviourByRoleType(RoleTypes.Noisemaker)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Engineer", OutfitPreset.Engineer, Utils.GetBehaviourByRoleType(RoleTypes.Engineer)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Scientist", OutfitPreset.Scientist, Utils.GetBehaviourByRoleType(RoleTypes.Scientist)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Detective", OutfitPreset.Detective, Utils.GetBehaviourByRoleType(RoleTypes.Detective)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Crewmate", OutfitPreset.Crewmate, Utils.GetBehaviourByRoleType(RoleTypes.Crewmate)));

                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action)(() =>
                {
                    CheatToggles.forcedRole = PlayerPickMenu.targetPlayerData.Role.Role;
                }));

                _forceRoleActive = true;
            }

            if (PlayerPickMenu.playerpickMenu == null)
            {
                CheatToggles.forceRole = false;
            }
        }
        else if (_forceRoleActive)
        {
            _forceRoleActive = false;
        }
    }

    public static void SpectatePPM()
    {
        if (CheatToggles.spectate)
        {
            if (!_spectateActive)
            {
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("spectate");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.AmOwner) playerDataList.Add(player.Data);
                }

                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action) (() =>
                {
                    Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(PlayerPickMenu.targetPlayerData.Object);
                }));

                _spectateActive = true;
                PlayerControl.LocalPlayer.moveable = false;
                CheatToggles.freecam = false;
            }

            if (PlayerPickMenu.playerpickMenu == null && Camera.main.gameObject.GetComponent<FollowerCamera>().Target == PlayerControl.LocalPlayer)
            {
                CheatToggles.spectate = false;
                PlayerControl.LocalPlayer.moveable = true;
            }
        }
        else if (_spectateActive)
        {
            _spectateActive = false;
            PlayerControl.LocalPlayer.moveable = true;
            Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(PlayerControl.LocalPlayer);
        }
    }
}

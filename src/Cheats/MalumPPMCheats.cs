using Il2CppSystem.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils;
using System;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace MalumMenu;
public static class MalumPPMCheats
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
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("reportBody");
                }

                PlayerPickMenu.OpenPlayerPickMenu(Utils.GetAllPlayerData(), (Action) (() =>
                {
                    // BYPASS: Sending report command directly
                    PlayerControl.LocalPlayer.CmdReportDeadBody(PlayerPickMenu.targetPlayerData);
                }));

                _reportBodyActive = true;
            }

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
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("ejectPlayer");
                }

                // BYPASS: Removed meeting check to allow attempt at all times
                List<NetworkedPlayerInfo> playerInfo = new List<NetworkedPlayerInfo>();
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.Data.IsDead && !player.Data.Disconnected)
                    {
                        playerInfo.Add(player.Data);
                    }
                }

                PlayerPickMenu.OpenPlayerPickMenu(playerInfo, (Action)(() =>
                {
                    if (MeetingHud.Instance != null) {
                        NetworkedPlayerInfo playerToEject = PlayerPickMenu.targetPlayerData;
                        // BYPASS: Sending RPC to complete voting and eject target instantly
                        MeetingHud.Instance.RpcVotingComplete(new Il2CppStructArray<MeetingHud.VoterState>(0L), playerToEject, false);
                    } else {
                        HudManager.Instance.Notifier.AddDisconnectMessage("Must be in a meeting to use Eject RPC");
                    }
                }));

                _ejectPlayerActive = true;
            }

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
                if (PlayerPickMenu.playerpickMenu != null)
                {
                    PlayerPickMenu.playerpickMenu.Close();
                    CheatToggles.DisablePPMCheats("killPlayer");
                }

                PlayerPickMenu.OpenPlayerPickMenu(Utils.GetAllPlayerData(), (Action)(() =>
                {
                    // BYPASS: Forcing 'Succeeded' flag regardless of cooldown or host status
                    Utils.MurderPlayer(PlayerPickMenu.targetPlayerData.Object, MurderResultFlags.Succeeded);
                }));

                _killPlayerActive = true;
            }

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

                // BYPASS: Removed all 'isHost' and 'oldRole' checks. All roles are now available.
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

                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action) (() =>
                {
                    if (!Utils.isLobby && !Utils.isFreePlay && _oldRole == null)
                    {
                        _oldRole = PlayerControl.LocalPlayer.Data.RoleType;
                    }

                    if (PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        if (PlayerPickMenu.targetPlayerData.Role.TeamType == RoleTeamTypes.Impostor)
                        {
                            RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.ImpostorGhost);
                        }
                        else
                        {
                            RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.CrewmateGhost);
                        }
                    }
                    else
                    {
                        // BYPASS: Setting role locally. Note: Other players might not see this
                        // unless the server accepts the RPC.
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
        else
        {
            if (_changeRoleActive)
            {
                _changeRoleActive = false;
            }
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

                // BYPASS: Forcing roles on others usually requires Host/Master status on the server.
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
        else
        {
            if (_forceRoleActive)
            {
                _forceRoleActive = false;
            }
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
                    if (!player.AmOwner)
                    {
                        playerDataList.Add(player.Data);
                    }
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
        else
        {
            if (_spectateActive)
            {
                _spectateActive = false;
                PlayerControl.LocalPlayer.moveable = true;
                Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(PlayerControl.LocalPlayer);
            }
        }
    }
}

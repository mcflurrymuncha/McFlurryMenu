using System;
using UnityEngine;
using InnerNet;
using System.Linq;
using Il2CppSystem.Collections.Generic;
using System.IO;
using Hazel;
using System.Reflection;
using AmongUs.GameOptions;
using BepInEx;
using HarmonyLib;
using UnityEngine.SceneManagement;
using Sentry.Internal.Extensions;
using System.Runtime.CompilerServices;
using AmongUs.InnerNet.GameDataMessages;
using Il2CppInterop.Runtime.Injection;

namespace McFlurryMenu;

public static class Utils
{
    public static bool isPastingInput;
    public static ReferenceDataManager ReferenceDataManager = DestroyableSingleton<ReferenceDataManager>.Instance; 
    public static SabotageSystemType SabotageSystem => ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
    public static bool isShip => ShipStatus.Instance;
    public static bool isLobby => AmongUsClient.Instance && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined && !isFreePlay;
    public static bool isOnlineGame => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;
    public static bool isLocalGame => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame;
    public static bool isFreePlay => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;
    public static bool isPlayer => PlayerControl.LocalPlayer;
    public static bool isHost => AmongUsClient.Instance && AmongUsClient.Instance.AmHost;
    public static bool isInGame => AmongUsClient.Instance && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started && isPlayer;
    public static bool isMeeting => MeetingHud.Instance;
    public static bool isMeetingVoting => isMeeting && MeetingHud.Instance.state is MeetingHud.VoteStates.Voted or MeetingHud.VoteStates.NotVoted;
    public static bool isMeetingProceeding => isMeeting && MeetingHud.Instance.state is MeetingHud.VoteStates.Proceeding;
    public static bool isExiling => ExileController.Instance && !(isAirshipMap && SpawnInMinigame.Instance.isActiveAndEnabled);
    public static bool isAnySabotageActive => ShipStatus.Instance && SabotageSystem.AnyActive;
    public static bool isNormalGame => GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal;
    public static bool isHideNSeek => GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek;
    public static bool isSkeldMap => (MapNames)GetCurrentMapID() == MapNames.Skeld;
    public static bool isMiraHQMap => (MapNames)GetCurrentMapID() == MapNames.MiraHQ;
    public static bool isPolusMap => (MapNames)GetCurrentMapID() == MapNames.Polus;
    public static bool isDleksMap => (MapNames)GetCurrentMapID() == MapNames.Dleks;
    public static bool isAirshipMap => (MapNames)GetCurrentMapID() == MapNames.Airship;
    public static bool isFungleMap => (MapNames)GetCurrentMapID() == MapNames.Fungle;
    public const float DefaultSpeed = 2.5f;
    public const float DefaultGhostSpeed = 3f;

    public static bool IsSpeedDefault(bool forGhost = false)
    {
        return forGhost ? Mathf.Approximately(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed, DefaultGhostSpeed) :
            Mathf.Approximately(PlayerControl.LocalPlayer.MyPhysics.Speed, DefaultSpeed);
    }

    public static void SnapSpeedToDefault(float snapRange, bool forGhost = false)
    {
        if (forGhost)
        {
            PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed - DefaultGhostSpeed)
                                                             < snapRange ? DefaultGhostSpeed : PlayerControl.LocalPlayer.MyPhysics.GhostSpeed;
        }
        else
        {
            PlayerControl.LocalPlayer.MyPhysics.Speed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.Speed - DefaultSpeed)
                                                        < snapRange ? DefaultSpeed : PlayerControl.LocalPlayer.MyPhysics.Speed;
        }
    }

    public static ClientData GetClientByPlayer(PlayerControl player)
    {
        try
        {
            var client = AmongUsClient.Instance.allClients.ToArray().FirstOrDefault(cd => cd.Character.PlayerId == player.PlayerId);
            return client;
        }
        catch { return null; }
    }

    public static int GetClientIdByPlayer(PlayerControl player)
    {
        if (player == null) return -1;
        var client = GetClientByPlayer(player);
        return client == null ? -1 : client.Id;
    }

    public static (string realName, string displayName, bool isDisguised) GetPlayerIdentity(PlayerControl player)
    {
        if (player == null || player.Data == null) return ("", "", false);

        var realName = $"<color=#{ColorUtility.ToHtmlStringRGB(player.Data.Color)}>{player.Data.PlayerName}</color>";
        var displayName = $"<color=#{ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[player.CurrentOutfit.ColorId])}>{player.CurrentOutfit.PlayerName}</color>";
        var isDisguised = player.CurrentOutfit.PlayerName != player.Data.PlayerName;

        return (realName, displayName, isDisguised);
    }

    public static bool IsVanished(NetworkedPlayerInfo playerInfo)
    {
        PhantomRole phantomRole = playerInfo.Role as PhantomRole;
        return phantomRole != null && (phantomRole.fading || phantomRole.isInvisible);
    }

    public static bool IsValidTarget(NetworkedPlayerInfo target)
    {
        var killAnyoneRequirements = target && !target.Disconnected && target.Object.Visible && target.PlayerId != PlayerControl.LocalPlayer.PlayerId && target.Role && target.Object;
        var fullRequirements = killAnyoneRequirements && !target.IsDead && !target.Object.inVent && !target.Object.inMovingPlat && target.Role.CanBeKilled;
        return CheatToggles.killAnyone ? killAnyoneRequirements : fullRequirements;
    }

    public static List<NetworkedPlayerInfo> GetAllPlayerData()
    {
        var playerDataList = new List<NetworkedPlayerInfo>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player != null && player.Data != null) playerDataList.Add(player.Data);
        }
        return playerDataList;
    }

    public static void AdjustResolution() => ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);

    public static RoleBehaviour GetBehaviourByRoleType(RoleTypes roleType) => RoleManager.Instance.AllRoles.ToArray().First(r => r.Role == roleType);

    public static RoleBehaviour GetBehaviourByTeamType(RoleTeamTypes roleTeamType)
    {
        RoleTypes roleType = (RoleTypes)Enum.Parse(typeof(RoleTypes), roleTeamType.ToString(), true);
        return GetBehaviourByRoleType(roleType);
    }

    public static void ForceSetScanner(PlayerControl player, bool toggle)
    {
        var count = ++player.scannerCount;
        player.SetScanner(toggle, count);
        RpcSetScannerMessage rpcMessage = new(player.NetId, toggle, count);
        AmongUsClient.Instance.LateBroadcastReliableMessage(Unsafe.As<IGameDataMessage>(rpcMessage));
    }

    public static void ForcePlayAnimation(byte animationType)
    {
        PlayerControl.LocalPlayer.PlayAnimation(animationType);
        RpcPlayAnimationMessage rpcMessage = new(PlayerControl.LocalPlayer.NetId, animationType);
        AmongUsClient.Instance.LateBroadcastUnreliableMessage(Unsafe.As<IGameDataMessage>(rpcMessage));
    }

    public static System.Collections.IEnumerator DelayedSnapTo(Vector2 position, float delay = 0.25f)
    {
        yield return new WaitForSeconds(delay);
        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(position);
    }

    public static void MurderPlayer(PlayerControl target, MurderResultFlags result)
    {
        if (isFreePlay)
        {
            PlayerControl.LocalPlayer.MurderPlayer(target, MurderResultFlags.Succeeded);
            return;
        }

        foreach (var item in PlayerControl.AllPlayerControls)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.MurderPlayer, SendOption.None, AmongUsClient.Instance.GetClientIdFromCharacter(item));
            writer.WriteNetObject(target);
            writer.Write((int)result);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }

    public static void CompleteTask(PlayerTask task)
    {
        if (isFreePlay)
        {
            PlayerControl.LocalPlayer.RpcCompleteTask(task.Id);
            return;
        }
        var hostData = AmongUsClient.Instance.GetHost();
        if (hostData == null || hostData.Character.Data.Disconnected || task.IsComplete) return;

        foreach (var item in PlayerControl.AllPlayerControls)
        {
            var messageWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.CompleteTask, SendOption.None, AmongUsClient.Instance.GetClientIdFromCharacter(item));
            messageWriter.WritePacked(task.Id);
            AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        }
    }

    public static void OpenChat()
    {
        if (!DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening)
        {
            DestroyableSingleton<HudManager>.Instance.Chat.chatScreen.SetActive(true);
            PlayerControl.LocalPlayer.NetTransform.Halt();
            DestroyableSingleton<HudManager>.Instance.Chat.StartCoroutine(DestroyableSingleton<HudManager>.Instance.Chat.CoOpen());
            if (DestroyableSingleton<FriendsListManager>.InstanceExists) DestroyableSingleton<FriendsListManager>.Instance.SetFriendButtonColor(true);
            if (DestroyableSingleton<HudManager>.Instance.Chat.chatNotification.gameObject.activeSelf) DestroyableSingleton<HudManager>.Instance.Chat.chatNotification.Close();
        }
    }

    public static void DrawTracer(GameObject sourceObject, GameObject targetObject, Color color)
    {
        var lineRenderer = sourceObject.GetComponent<LineRenderer>() ?? sourceObject.AddComponent<LineRenderer>();
        lineRenderer.SetVertexCount(2);
        lineRenderer.SetWidth(0.02F, 0.02F);
        lineRenderer.material = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;
        lineRenderer.SetColors(color, color);
        lineRenderer.SetPosition(0, sourceObject.transform.position);
        lineRenderer.SetPosition(1, targetObject.transform.position);
    }

    public static bool IsChatUiActive()
    {
        try { return CheatToggles.enableChat || MeetingHud.Instance || !ShipStatus.Instance || PlayerControl.LocalPlayer.Data.IsDead; }
        catch { return false; }
    }

    public static void CloseChat()
    {
        if (DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening) DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
    }

    public static float GetDistanceBetween(PlayerControl source, PlayerControl target) => (target.GetTruePosition() - source.GetTruePosition()).magnitude;

    public static System.Collections.Generic.List<PlayerControl> GetPlayersSortedByDistance(PlayerControl source = null)
    {
        if (source.IsNull()) source = PlayerControl.LocalPlayer;
        System.Collections.Generic.List<PlayerControl> outputList = new();
        foreach (var playerInfo in GameData.Instance.AllPlayers)
        {
            if (playerInfo.Object) outputList.Add(playerInfo.Object);
        }
        return outputList.OrderBy(target => GetDistanceBetween(source, target)).ToList();
    }

    public static byte GetCurrentMapID()
    {
        if (isFreePlay) return (byte)AmongUsClient.Instance.TutorialMapId;
        return GameOptionsManager.Instance?.currentGameOptions?.MapId ?? byte.MaxValue;
    }

    public static SystemTypes GetCurrentRoom() => HudManager.Instance.roomTracker.LastRoom.RoomId;

    public static string GetColoredPingText(int ping) => ping switch
    {
        <= 100 => $"<color=#00ff00ff>PING: {ping} ms</color>",
        < 400 => $"<color=#ffff00ff>PING: {ping} ms</color>",
        _ => $"<color=#ff0000ff>PING: {ping} ms</color>"
    };

    public static KeyCode StringToKeycode(string keyCodeStr)
    {
        if (!string.IsNullOrEmpty(keyCodeStr))
        {
            try { return (KeyCode)Enum.Parse(typeof(KeyCode), keyCodeStr, true); } catch { }
        }
        return KeyCode.Delete;
    }

    public static string PlatformTypeToString(Platforms platform) => platform switch
    {
        Platforms.StandaloneEpicPC => "Epic Games",
        Platforms.StandaloneSteamPC => "Steam",
        Platforms.StandaloneWin10 => "Microsoft Store",
        Platforms.Android => "Android",
        Platforms.Switch => "Nintendo Switch",
        Platforms.Xbox => "Xbox",
        Platforms.Playstation => "PlayStation",
        _ => "Unknown"
    };

    public static string GetRoleName(NetworkedPlayerInfo playerData)
    {
        var translatedRole = DestroyableSingleton<TranslationController>.Instance.GetString(playerData.Role.StringName, Il2CppSystem.Array.Empty<Il2CppSystem.Object>());
        if (translatedRole != "STRMISS") return translatedRole;
        return DestroyableSingleton<TranslationController>.Instance.GetString(GetBehaviourByTeamType(playerData.Role.TeamType).StringName, Il2CppSystem.Array.Empty<Il2CppSystem.Object>());
    }

    public static void ShowPopup(string text)
    {
        var popup = UnityEngine.Object.Instantiate(DiscordManager.Instance.discordPopup, Camera.main!.transform);
        var background = popup.transform.Find("Background").GetComponent<SpriteRenderer>();
        background.size = new Vector2(background.size.x * 2.5f, background.size.y);
        popup.TextAreaTMP.fontSizeMin = 2;
        popup.Show(text);
    }

    public static Dictionary<string, Sprite> CachedSprites = new();
    public static Sprite LoadSprite(string path, float pixelsPerUnit = 1f)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
            Texture2D texture = LoadTextureFromResources(path);
            sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch { McFlurryPlugin.Log.LogError($"Failed to read Texture: {path}"); return null; }
    }

    public static Texture2D LoadTextureFromResources(string path)
    {
        try
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            using MemoryStream ms = new();
            stream.CopyTo(ms);
            ImageConversion.LoadImage(texture, ms.ToArray(), false);
            return texture;
        }
        catch { McFlurryPlugin.Log.LogError($"Failed to read Texture: {path}"); return null; }
    }

    public static void OpenConfigFile()
    {
        var configFilePath = Path.Combine(Paths.ConfigPath, "McFlurryMenu.cfg");
        if (File.Exists(configFilePath))
        {
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = configFilePath, UseShellExecute = true }); }
            catch { McFlurryPlugin.Log.LogError("Failed to open config."); }
        }
    }

    public class PanicCleaner : MonoBehaviour
    {
        public static void Create()
        {
            ClassInjector.RegisterTypeInIl2Cpp<PanicCleaner>();
            var go = new GameObject("McFlurry_PanicCleaner") { hideFlags = HideFlags.HideAndDontSave };
            go.AddComponent<PanicCleaner>();
        }
        private void LateUpdate()
        {
            try { Harmony.UnpatchID(McFlurryPlugin.Id); } catch { }
            Destroy(gameObject);
        }
    }

    public static void Panic()
    {
        McFlurryPlugin.isPanicked = true;
        CheatToggles.DisableAll();
        if (ModManager.Instance.ModStamp) ModManager.Instance.ModStamp.enabled = false;

        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "MainMenu" || scene.name == "MatchMaking") SceneManager.LoadScene(scene.name);

        UnityEngine.Object.Destroy(McFlurryPlugin.menuUI);
        UnityEngine.Object.Destroy(McFlurryPlugin.consoleUI);
        UnityEngine.Object.Destroy(McFlurryPlugin.rolesUI);
        UnityEngine.Object.Destroy(McFlurryPlugin.doorsUI);
        UnityEngine.Object.Destroy(McFlurryPlugin.tasksUI);
        UnityEngine.Object.Destroy(McFlurryPlugin.protectUI);
        UnityEngine.Object.Destroy(McFlurryPlugin.keybindListener);

        PanicCleaner.Create();
    }
}

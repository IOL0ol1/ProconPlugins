using PRoCon.Core.Battlemap;
using PRoCon.Core.HttpServer;
using PRoCon.Core.Maps;
using PRoCon.Core.Players;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.TextChatModeration;
using PRoConEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace PRoCon.Core.Plugin.Custom
{
    public class PRoConPluginAPI : CPRoConMarshalByRefObject
    {
        protected Dictionary<string, CPunkbusterInfo> PunkbusterPlayerInfoList;

        protected Dictionary<string, CPlayerInfo> FrostbitePlayerInfoList;

        public string ClassName
        {
            get;
            set;
        }

        public PRoConPluginAPI()
        {
            PunkbusterPlayerInfoList = new Dictionary<string, CPunkbusterInfo>();
            FrostbitePlayerInfoList = new Dictionary<string, CPlayerInfo>();
        }

        public virtual void OnConnectionClosed()
        {
        }

        public string CreateEnumString(Type enumeration)
        {
            return string.Format("enum.{0}_{1}({2})", GetType().Name, enumeration.Name, string.Join("|", Enum.GetNames(enumeration)));
        }

        public virtual void OnLogin()
        {
        }

        public virtual void OnLogout()
        {
        }

        public virtual void OnQuit()
        {
        }

        public virtual void OnVersion(string serverType, string version)
        {
        }

        public virtual void OnHelp(List<string> commands)
        {
        }

        public virtual void OnRunScript(string scriptFileName)
        {
        }

        public virtual void OnRunScriptError(string scriptFileName, int lineError, string errorDescription)
        {
        }

        public virtual void OnServerInfo(CServerInfo serverInfo)
        {
        }

        public virtual void OnResponseError(List<string> requestWords, string error)
        {
        }

        public virtual void OnYelling(string message, int messageDuration, CPlayerSubset subset)
        {
        }

        public virtual void OnSaying(string message, CPlayerSubset subset)
        {
        }

        public virtual void OnRestartLevel()
        {
        }

        public virtual void OnSupportedMaps(string playlist, List<string> lstSupportedMaps)
        {
        }

        public virtual void OnListPlaylists(List<string> playlists)
        {
        }

        public virtual void OnListPlayers(List<CPlayerInfo> players, CPlayerSubset subset)
        {
            if (subset.Subset == CPlayerSubset.PlayerSubsetType.All)
            {
                List<CPlayerInfo>.Enumerator enumerator = players.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        CPlayerInfo current = enumerator.Current;
                        if (FrostbitePlayerInfoList.ContainsKey(current.SoldierName))
                        {
                            FrostbitePlayerInfoList[current.SoldierName] = current;
                        }
                        else
                        {
                            FrostbitePlayerInfoList.Add(current.SoldierName, current);
                        }
                    }
                }
                finally
                {
                    ((IDisposable)enumerator).Dispose();
                }
                foreach (string key in FrostbitePlayerInfoList.Keys)
                {
                    bool flag = false;
                    enumerator = players.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            if (string.Compare(enumerator.Current.SoldierName, FrostbitePlayerInfoList[key].SoldierName) == 0)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    finally
                    {
                        ((IDisposable)enumerator).Dispose();
                    }
                    if (!flag)
                    {
                        FrostbitePlayerInfoList.Remove(key);
                        PunkbusterPlayerInfoList.Remove(key);
                    }
                }
            }
        }

        public virtual void OnEndRound(int iWinningTeamID)
        {
        }

        public virtual void OnRunNextLevel()
        {
        }

        public virtual void OnCurrentLevel(string mapFileName)
        {
        }

        public virtual void OnPlaylistSet(string playlist)
        {
        }

        public virtual void OnSpectatorListLoad()
        {
        }

        public virtual void OnSpectatorListSave()
        {
        }

        public virtual void OnSpectatorListPlayerAdded(string soldierName)
        {
        }

        public virtual void OnSpectatorListPlayerRemoved(string soldierName)
        {
        }

        public virtual void OnSpectatorListCleared()
        {
        }

        public virtual void OnSpectatorListList(List<string> soldierNames)
        {
        }

        public virtual void OnGameAdminLoad()
        {
        }

        public virtual void OnGameAdminSave()
        {
        }

        public virtual void OnGameAdminPlayerAdded(string soldierName)
        {
        }

        public virtual void OnGameAdminPlayerRemoved(string soldierName)
        {
        }

        public virtual void OnGameAdminCleared()
        {
        }

        public virtual void OnGameAdminList(List<string> soldierNames)
        {
        }

        public virtual void OnFairFight(bool isEnabled)
        {
        }

        public virtual void OnIsHitIndicator(bool isEnabled)
        {
        }

        public virtual void OnCommander(bool isEnabled)
        {
        }

        public virtual void OnAlwaysAllowSpectators(bool isEnabled)
        {
        }

        public virtual void OnForceReloadWholeMags(bool isEnabled)
        {
        }

        public virtual void OnServerType(string value)
        {
        }

        public virtual void OnMaxSpectators(int limit)
        {
        }

        public virtual void OnBanAdded(CBanInfo ban)
        {
        }

        public virtual void OnBanRemoved(CBanInfo ban)
        {
        }

        public virtual void OnBanListClear()
        {
        }

        public virtual void OnBanListSave()
        {
        }

        public virtual void OnBanListLoad()
        {
        }

        public virtual void OnBanList(List<CBanInfo> banList)
        {
        }

        public virtual void OnTextChatModerationAddPlayer(TextChatModerationEntry playerEntry)
        {
        }

        public virtual void OnTextChatModerationRemovePlayer(TextChatModerationEntry playerEntry)
        {
        }

        public virtual void OnTextChatModerationClear()
        {
        }

        public virtual void OnTextChatModerationSave()
        {
        }

        public virtual void OnTextChatModerationLoad()
        {
        }

        public virtual void OnTextChatModerationList(TextChatModerationDictionary moderationList)
        {
        }

        public virtual void OnMaplistConfigFile(string configFileName)
        {
        }

        public virtual void OnMaplistLoad()
        {
        }

        public virtual void OnMaplistSave()
        {
        }

        public virtual void OnMaplistList(List<MaplistEntry> lstMaplist)
        {
        }

        public virtual void OnMaplistCleared()
        {
        }

        public virtual void OnMaplistMapAppended(string mapFileName)
        {
        }

        public virtual void OnMaplistNextLevelIndex(int mapIndex)
        {
        }

        public virtual void OnMaplistGetMapIndices(int mapIndex, int nextIndex)
        {
        }

        public virtual void OnMaplistGetRounds(int currentRound, int totalRounds)
        {
        }

        public virtual void OnMaplistMapRemoved(int mapIndex)
        {
        }

        public virtual void OnMaplistMapInserted(int mapIndex, string mapFileName)
        {
        }

        public virtual void OnPlayerIdleDuration(string soldierName, int idleTime)
        {
        }

        public virtual void OnPlayerIsAlive(string soldierName, bool isAlive)
        {
        }

        public virtual void OnPlayerPingedByAdmin(string soldierName, int ping)
        {
        }

        public virtual void OnSquadLeader(int teamId, int squadId, string soldierName)
        {
        }

        public virtual void OnSquadListActive(int teamId, int squadCount, List<int> squadList)
        {
        }

        public virtual void OnSquadListPlayers(int teamId, int squadId, int playerCount, List<string> playersInSquad)
        {
        }

        public virtual void OnSquadIsPrivate(int teamId, int squadId, bool isPrivate)
        {
        }

        public virtual void OnServerName(string serverName)
        {
        }

        public virtual void OnServerDescription(string serverDescription)
        {
        }

        public virtual void OnServerMessage(string serverMessage)
        {
        }

        public virtual void OnBannerURL(string url)
        {
        }

        public virtual void OnGamePassword(string gamePassword)
        {
        }

        public virtual void OnPunkbuster(bool isEnabled)
        {
        }

        public virtual void OnRanked(bool isEnabled)
        {
        }

        public virtual void OnRankLimit(int iRankLimit)
        {
        }

        public virtual void OnPlayerLimit(int limit)
        {
        }

        public virtual void OnMaxPlayerLimit(int limit)
        {
        }

        public virtual void OnMaxPlayers(int limit)
        {
        }

        public virtual void OnCurrentPlayerLimit(int limit)
        {
        }

        public virtual void OnIdleTimeout(int limit)
        {
        }

        public virtual void OnIdleBanRounds(int limit)
        {
        }

        public virtual void OnProfanityFilter(bool isEnabled)
        {
        }

        public virtual void OnRoundRestartPlayerCount(int limit)
        {
        }

        public virtual void OnRoundStartPlayerCount(int limit)
        {
        }

        public virtual void OnGameModeCounter(int limit)
        {
        }

        public virtual void OnCtfRoundTimeModifier(int limit)
        {
        }

        public virtual void OnRoundTimeLimit(int limit)
        {
        }

        public virtual void OnTicketBleedRate(int limit)
        {
        }

        public virtual void OnRoundLockdownCountdown(int limit)
        {
        }

        public virtual void OnRoundWarmupTimeout(int limit)
        {
        }

        public virtual void OnPremiumStatus(bool isEnabled)
        {
        }

        public virtual void OnGunMasterWeaponsPreset(int preset)
        {
        }

        public virtual void OnVehicleSpawnAllowed(bool isEnabled)
        {
        }

        public virtual void OnVehicleSpawnDelay(int limit)
        {
        }

        public virtual void OnBulletDamage(int limit)
        {
        }

        public virtual void OnOnlySquadLeaderSpawn(bool isEnabled)
        {
        }

        public virtual void OnSoldierHealth(int limit)
        {
        }

        public virtual void OnPlayerManDownTime(int limit)
        {
        }

        public virtual void OnPlayerRespawnTime(int limit)
        {
        }

        public virtual void OnHud(bool isEnabled)
        {
        }

        public virtual void OnNameTag(bool isEnabled)
        {
        }

        public virtual void OnTeamFactionOverride(int teamId, int faction)
        {
        }

        public virtual void OnAllUnlocksUnlocked(bool isEnabled)
        {
        }

        public virtual void OnBuddyOutline(bool isEnabled)
        {
        }

        public virtual void OnHudBuddyInfo(bool isEnabled)
        {
        }

        public virtual void OnHudClassAbility(bool isEnabled)
        {
        }

        public virtual void OnHudCrosshair(bool isEnabled)
        {
        }

        public virtual void OnHudEnemyTag(bool isEnabled)
        {
        }

        public virtual void OnHudExplosiveIcons(bool isEnabled)
        {
        }

        public virtual void OnHudGameMode(bool isEnabled)
        {
        }

        public virtual void OnHudHealthAmmo(bool isEnabled)
        {
        }

        public virtual void OnHudMinimap(bool isEnabled)
        {
        }

        public virtual void OnHudObiturary(bool isEnabled)
        {
        }

        public virtual void OnHudPointsTracker(bool isEnabled)
        {
        }

        public virtual void OnHudUnlocks(bool isEnabled)
        {
        }

        public virtual void OnPlaylist(string playlist)
        {
        }

        public virtual void OnFriendlyFire(bool isEnabled)
        {
        }

        public virtual void OnHardcore(bool isEnabled)
        {
        }

        public virtual void OnUnlockMode(string mode)
        {
        }

        public virtual void OnPreset(string mode, bool isLocked)
        {
        }

        public virtual void OnTeamBalance(bool isEnabled)
        {
        }

        public virtual void OnKillCam(bool isEnabled)
        {
        }

        public virtual void OnMiniMap(bool isEnabled)
        {
        }

        public virtual void OnCrossHair(bool isEnabled)
        {
        }

        public virtual void On3dSpotting(bool isEnabled)
        {
        }

        public virtual void OnMiniMapSpotting(bool isEnabled)
        {
        }

        public virtual void OnThirdPersonVehicleCameras(bool isEnabled)
        {
        }

        public virtual void OnRoundStartReadyPlayersNeeded(int limit)
        {
        }

        public virtual void OnTeamKillCountForKick(int limit)
        {
        }

        public virtual void OnTeamKillValueIncrease(int limit)
        {
        }

        public virtual void OnTeamKillValueDecreasePerSecond(int limit)
        {
        }

        public virtual void OnTeamKillValueForKick(int limit)
        {
        }

        public virtual void OnLevelVariablesList(LevelVariable requestedContext, List<LevelVariable> returnedValues)
        {
        }

        public virtual void OnLevelVariablesEvaluate(LevelVariable requestedContext, LevelVariable returnedValue)
        {
        }

        public virtual void OnLevelVariablesClear(LevelVariable requestedContext)
        {
        }

        public virtual void OnLevelVariablesSet(LevelVariable requestedContext)
        {
        }

        public virtual void OnLevelVariablesGet(LevelVariable requestedContext, LevelVariable returnedValue)
        {
        }

        public virtual void OnTextChatModerationMode(ServerModerationModeType mode)
        {
        }

        public virtual void OnTextChatSpamTriggerCount(int limit)
        {
        }

        public virtual void OnTextChatSpamDetectionTime(int limit)
        {
        }

        public virtual void OnTextChatSpamCoolDownTime(int limit)
        {
        }

        public virtual void OnReservedSlotsConfigFile(string configFileName)
        {
        }

        public virtual void OnReservedSlotsLoad()
        {
        }

        public virtual void OnReservedSlotsSave()
        {
        }

        public virtual void OnReservedSlotsPlayerAdded(string soldierName)
        {
        }

        public virtual void OnReservedSlotsPlayerRemoved(string soldierName)
        {
        }

        public virtual void OnReservedSlotsCleared()
        {
        }

        public virtual void OnReservedSlotsList(List<string> soldierNames)
        {
        }

        public virtual void OnReservedSlotsListAggressiveJoin(bool isEnabled)
        {
        }

        public virtual void OnPlayerKilledByAdmin(string soldierName)
        {
        }

        public virtual void OnPlayerKickedByAdmin(string soldierName, string reason)
        {
        }

        public virtual void OnPlayerMovedByAdmin(string soldierName, int destinationTeamId, int destinationSquadId, bool forceKilled)
        {
        }

        public virtual void OnPlayerJoin(string soldierName)
        {
            if (!FrostbitePlayerInfoList.ContainsKey(soldierName))
            {
                FrostbitePlayerInfoList.Add(soldierName, new CPlayerInfo(soldierName, "", 0, 24));
            }
        }

        public virtual void OnPlayerLeft(CPlayerInfo playerInfo)
        {
            if (PunkbusterPlayerInfoList.ContainsKey(playerInfo.SoldierName))
            {
                PunkbusterPlayerInfoList.Remove(playerInfo.SoldierName);
            }
            if (FrostbitePlayerInfoList.ContainsKey(playerInfo.SoldierName))
            {
                FrostbitePlayerInfoList.Remove(playerInfo.SoldierName);
            }
        }

        public virtual void OnPlayerDisconnected(string soldierName, string reason)
        {
            if (PunkbusterPlayerInfoList.ContainsKey(soldierName))
            {
                PunkbusterPlayerInfoList.Remove(soldierName);
            }
            if (FrostbitePlayerInfoList.ContainsKey(soldierName))
            {
                FrostbitePlayerInfoList.Remove(soldierName);
            }
        }

        public virtual void OnPlayerAuthenticated(string soldierName, string guid)
        {
        }

        public virtual void OnPlayerKilled(Kill kKillerVictimDetails)
        {
        }

        public virtual void OnPlayerKicked(string soldierName, string reason)
        {
        }

        public virtual void OnPlayerSpawned(string soldierName, Inventory spawnedInventory)
        {
        }

        public virtual void OnPlayerTeamChange(string soldierName, int teamId, int squadId)
        {
        }

        public virtual void OnPlayerSquadChange(string soldierName, int teamId, int squadId)
        {
        }

        public virtual void OnGlobalChat(string speaker, string message)
        {
        }

        public virtual void OnTeamChat(string speaker, string message, int teamId)
        {
        }

        public virtual void OnSquadChat(string speaker, string message, int teamId, int squadId)
        {
        }

        public virtual void OnPlayerChat(string speaker, string message, string targetPlayer)
        {
        }

        public virtual void OnRoundOverPlayers(List<CPlayerInfo> players)
        {
        }

        public virtual void OnRoundOverTeamScores(List<TeamScore> teamScores)
        {
        }

        public virtual void OnRoundOver(int winningTeamId)
        {
        }

        public virtual void OnLoadingLevel(string mapFileName, int roundsPlayed, int roundsTotal)
        {
        }

        public virtual void OnLevelStarted()
        {
        }

        public virtual void OnLevelLoaded(string mapFileName, string gamemode, int roundsPlayed, int roundsTotal)
        {
        }

        public virtual void OnPunkbusterMessage(string punkbusterMessage)
        {
        }

        public virtual void OnPunkbusterBanInfo(CBanInfo ban)
        {
        }

        public virtual void OnPunkbusterUnbanInfo(CBanInfo unban)
        {
        }

        public virtual void OnPunkbusterBeginPlayerInfo()
        {
        }

        public virtual void OnPunkbusterEndPlayerInfo()
        {
        }

        public virtual void OnPunkbusterPlayerInfo(CPunkbusterInfo playerInfo)
        {
            if (playerInfo != null)
            {
                if (!PunkbusterPlayerInfoList.ContainsKey(playerInfo.SoldierName))
                {
                    PunkbusterPlayerInfoList.Add(playerInfo.SoldierName, playerInfo);
                }
                else
                {
                    PunkbusterPlayerInfoList[playerInfo.SoldierName] = playerInfo;
                }
            }
        }

        public virtual void OnAccountCreated(string username)
        {
        }

        public virtual void OnAccountDeleted(string username)
        {
        }

        public virtual void OnAccountPrivilegesUpdate(string username, CPrivileges privileges)
        {
        }

        public virtual void OnAccountLogin(string accountName, string ip, CPrivileges privileges)
        {
        }

        public virtual void OnAccountLogout(string accountName, string ip, CPrivileges privileges)
        {
        }

        public virtual void OnAnyMatchRegisteredCommand(string speaker, string text, MatchCommand matchedCommand, CapturedCommand capturedCommand, CPlayerSubset matchedScope)
        {
        }

        public virtual void OnRegisteredCommand(MatchCommand command)
        {
        }

        public virtual void OnUnregisteredCommand(MatchCommand command)
        {
        }

        public virtual void OnZoneTrespass(CPlayerInfo playerInfo, ZoneAction action, MapZone sender, Point3D tresspassLocation, float tresspassPercentage, object trespassState)
        {
        }

        public virtual HttpWebServerResponseData OnHttpRequest(HttpWebServerRequestData data)
        {
            return null;
        }

        public virtual void OnReceiveProconVariable(string variableName, string value)
        {
        }
    }
}
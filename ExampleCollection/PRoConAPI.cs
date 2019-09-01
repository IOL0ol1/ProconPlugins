using System.Collections.Generic;
using System.Linq;

using PRoCon.Core;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Players;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.TextChatModeration;
using Output = System.Diagnostics.Trace;

namespace PRoConEvents
{
    class PRoConAPI : PRoConPluginAPI
    {
        public override void OnServerInfo(CServerInfo serverInfo)
        {
            Output.TraceInformation("OnServerInfo {0}", serverInfo.Map);
        }

        public override void OnRestartLevel()
        {
            Output.TraceInformation("OnRestartLevel");
        }

        public override void OnSupportedMaps(string playlist, List<string> lstSupportedMaps)
        {
            Output.TraceInformation("OnSupportedMaps {0} {1}", playlist, string.Join(",", lstSupportedMaps.ToArray()));
        }

        public override void OnListPlayers(List<CPlayerInfo> players, CPlayerSubset subset)
        {
            base.OnListPlayers(players, subset);
            Output.TraceInformation("OnListPlayers");
        }

        public override void OnEndRound(int iWinningTeamID)
        {
            Output.TraceInformation("OnEndRound {0}", iWinningTeamID);
        }

        public override void OnRunNextLevel()
        {
            Output.TraceInformation("OnRunNextLevel");
        }

        public override void OnCurrentLevel(string mapFileName)
        {
            Output.TraceInformation("OnCurrentLevel {0}", mapFileName);
        }

        public override void OnPlaylistSet(string playlist)
        {
            Output.TraceInformation("OnPlaylistSet {0}", playlist);
        }

        public override void OnGameAdminList(List<string> soldierNames)
        {
            Output.TraceInformation("OnGameAdminList {0}", string.Join(",", soldierNames.ToArray()));
        }

        public override void OnServerType(string value)
        {
            Output.TraceInformation("OnServerType {0}", value);
        }

        public override void OnBanAdded(CBanInfo ban)
        {
            Output.TraceInformation("OnBanAdded {0} {1} {2} {3}", ban.SoldierName, ban.Guid, ban.IdType, ban.Reason);
        }

        public override void OnBanRemoved(CBanInfo ban)
        {
            Output.TraceInformation("OnBanRemoved {0} {1} {2} {3}", ban.SoldierName, ban.Guid, ban.IdType, ban.Reason);
        }

        public override void OnBanList(List<CBanInfo> banList)
        {
            Output.TraceInformation("OnBanList {0}", string.Join(",", banList.Select(_ => _.SoldierName + "|" + _.Guid + "|" + _.IdType + "|" + _.Reason).ToArray()));
        }

        public override void OnTextChatModerationAddPlayer(TextChatModerationEntry playerEntry)
        {
            Output.TraceInformation("OnTextChatModerationAddPlayer {0} {1}", playerEntry.SoldierName, playerEntry.PlayerModerationLevel);
        }

        public override void OnTextChatModerationRemovePlayer(TextChatModerationEntry playerEntry)
        {
            Output.TraceInformation("OnTextChatModerationRemovePlayer {0} {1}", playerEntry.SoldierName, playerEntry.PlayerModerationLevel);
        }

        public override void OnMaplistMapAppended(string mapFileName)
        {
            Output.TraceInformation("OnMaplistMapAppended {0}", mapFileName);
        }

        public override void OnMaplistNextLevelIndex(int mapIndex)
        {
            Output.TraceInformation("OnMaplistNextLevelIndex {0}", mapIndex);
        }

        public override void OnMaplistGetMapIndices(int mapIndex, int nextIndex)
        {
            Output.TraceInformation("OnMaplistGetMapIndices {0} {1}", mapIndex, nextIndex);
        }

        public override void OnPlayerIdleDuration(string soldierName, int idleTime)
        {
            Output.TraceInformation("OnPlayerIdleDuration {0} {1}", soldierName, idleTime);
        }

        public override void OnPlayerIsAlive(string soldierName, bool isAlive)
        {
            Output.TraceInformation("OnPlayerIsAlive {0} {1}", soldierName, isAlive);
        }

        public override void OnPlayerPingedByAdmin(string soldierName, int ping)
        {
            Output.TraceInformation("OnPlayerPingedByAdmin {0} {1}", soldierName, ping);
        }

        public override void OnSquadLeader(int teamId, int squadId, string soldierName)
        {
            Output.TraceInformation("OnSquadLeader {0} {1} {2}", teamId, squadId, soldierName);
        }

        public override void OnSquadListActive(int teamId, int squadCount, List<int> squadList)
        {
            Output.TraceInformation("OnSquadListActive {0} {1} {2}", teamId, squadCount, squadList.Count);
        }

        public override void OnTeamFactionOverride(int teamId, int faction)
        {
            Output.TraceInformation("OnTeamFactionOverride {0} {1}", teamId, faction);
        }

        public override void OnPlaylist(string playlist)
        {
            Output.TraceInformation("OnPlaylist {0}", playlist);
        }

        public override void OnTextChatModerationMode(ServerModerationModeType mode)
        {
            Output.TraceInformation("OnTextChatModerationMode {0}", mode);
        }

        public override void OnReservedSlotsConfigFile(string configFileName)
        {
            Output.TraceInformation("OnReservedSlotsConfigFile {0}", configFileName);
        }

        public override void OnReservedSlotsList(List<string> soldierNames)
        {
            Output.TraceInformation("OnReservedSlotsList {0}", string.Join(",", soldierNames.ToArray()));
        }

        public override void OnPlayerKilledByAdmin(string soldierName)
        {
            Output.TraceInformation("OnPlayerKilledByAdmin {0} TeamID {1}", soldierName, FrostbitePlayerInfoList[soldierName].TeamID);
        }

        public override void OnPlayerKickedByAdmin(string soldierName, string reason)
        {
            Output.TraceInformation("OnPlayerKickedByAdmin {0} {1}", soldierName, reason);
        }

        public override void OnPlayerMovedByAdmin(string soldierName, int destinationTeamId, int destinationSquadId, bool forceKilled)
        {
            Output.TraceInformation("OnPlayerMovedByAdmin {0} {1} {2} {3}", soldierName, destinationTeamId, destinationSquadId, forceKilled);
        }

        public override void OnPlayerJoin(string soldierName)
        {
            base.OnPlayerJoin(soldierName);
            Output.TraceInformation("OnPlayerJoin {0}", soldierName);
        }

        public override void OnPlayerLeft(CPlayerInfo playerInfo)
        {
            base.OnPlayerLeft(playerInfo);
            Output.TraceInformation("OnPlayerLeft {0}", playerInfo.SoldierName);
        }

        public override void OnPlayerDisconnected(string soldierName, string reason)
        {
            base.OnPlayerDisconnected(soldierName, reason);
            Output.TraceInformation("OnPlayerDisconnected {0} {1}", soldierName, reason);
        }

        public override void OnPlayerAuthenticated(string soldierName, string guid)
        {
            Output.TraceInformation("OnPlayerAuthenticated {0} {1}", soldierName, guid);
        }

        public override void OnPlayerKilled(Kill kKillerVictimDetails)
        {
            Output.TraceInformation("OnPlayerKilled {0} {1} {2} {3} {4}", kKillerVictimDetails.Killer.SoldierName, kKillerVictimDetails.DamageType, kKillerVictimDetails.Distance, kKillerVictimDetails.Headshot, kKillerVictimDetails.IsSuicide);
        }

        public override void OnPlayerKicked(string soldierName, string reason)
        {
            Output.TraceInformation("OnPlayerKicked {0} {1}", soldierName, reason);
        }

        public override void OnPlayerSpawned(string soldierName, Inventory spawnedInventory)
        {
            Output.TraceInformation("OnPlayerSpawned {0} {1} {2} {3}", soldierName, spawnedInventory.Kit, spawnedInventory.Weapons.First().Name, spawnedInventory.Specializations.First().Name);
        }

        public override void OnRoundOverPlayers(List<CPlayerInfo> players)
        {
            Output.TraceInformation("OnRoundOverPlayers {0}", string.Join(",", players.Select(_ => _.SoldierName + "|" + _.Score).ToArray()));
        }

        public override void OnRoundOverTeamScores(List<TeamScore> teamScores)
        {
            Output.TraceInformation("OnRoundOverTeamScores {0}", string.Join(",", teamScores.Select(_ => _.TeamID + "|" + _.Score).ToArray()));
        }

        public override void OnRoundOver(int winningTeamId)
        {
            Output.TraceInformation("OnRoundOver {0}", winningTeamId);
        }

        public override void OnLoadingLevel(string mapFileName, int roundsPlayed, int roundsTotal)
        {
            Output.TraceInformation("OnLoadingLevel {0} {1} {2}", mapFileName, roundsPlayed, roundsTotal);
        }

        public override void OnLevelStarted()
        {
            Output.TraceInformation("OnLevelStarted");
        }

        public override void OnLevelLoaded(string mapFileName, string gamemode, int roundsPlayed, int roundsTotal)
        {
            Output.TraceInformation("OnLevelLoaded {0} {1} {2} {3}", mapFileName, gamemode, roundsPlayed, roundsTotal);
        }

        public override void OnPunkbusterMessage(string punkbusterMessage)
        {
            Output.TraceInformation("OnPunkbusterMessage {0}", punkbusterMessage);
        }

        public override void OnPunkbusterBanInfo(CBanInfo ban)
        {
            Output.TraceInformation("OnPunkbusterBanInfo {0}", ban.SoldierName);
        }

        public override void OnPunkbusterUnbanInfo(CBanInfo unban)
        {
            Output.TraceInformation("OnPunkbusterUnbanInfo {0}", unban.SoldierName);
        }

        public override void OnPunkbusterBeginPlayerInfo()
        {
            Output.TraceInformation("OnPunkbusterBeginPlayerInfo");
        }

        public override void OnPunkbusterEndPlayerInfo()
        {
            Output.TraceInformation("OnPunkbusterEndPlayerInfo");
        }

        public override void OnPunkbusterPlayerInfo(CPunkbusterInfo playerInfo)
        {
            base.OnPunkbusterPlayerInfo(playerInfo);
            Output.TraceInformation("OnPunkbusterPlayerInfo {0}", playerInfo.SoldierName);
        }

        public override void OnAnyMatchRegisteredCommand(string speaker, string text, MatchCommand matchedCommand, CapturedCommand capturedCommand, CPlayerSubset matchedScope)
        {
            Output.TraceInformation("OnAnyMatchRegisteredCommand {0} {1} {2} {3} {4}", speaker, text, matchedCommand.Command, capturedCommand.Command, matchedScope.SoldierName);
        }

        public override void OnRegisteredCommand(MatchCommand command)
        {
            Output.TraceInformation("OnRegisteredCommand {0} {1}", command.Command, command.ArgumentsFormat);
        }

        public override void OnUnregisteredCommand(MatchCommand command)
        {
            Output.TraceInformation("OnUnregisteredCommand {0} {1}", command.Command, command.ArgumentsFormat);
        }

        public override void OnZoneTrespass(CPlayerInfo playerInfo, ZoneAction action, MapZone sender, Point3D tresspassLocation, float tresspassPercentage, object trespassState)
        {
            Output.TraceInformation("OnZoneTrespass {0} {1}", playerInfo.SoldierName, action, sender.LevelFileName);
        }
    }
}
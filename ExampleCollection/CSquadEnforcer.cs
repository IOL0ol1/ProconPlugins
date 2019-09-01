/*  Copyright 2011 falcontx
    http://tag.bitgoblin.com

    This file is part of BF3 PRoCon.

    BF3 PRoCon is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    BF3 PRoCon is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with BF3 PRoCon.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;

namespace PRoConEvents
{
    public class CSquadEnforcer : PRoConPluginAPI, IPRoConPluginInterface
    {
        private string m_strHostName;
        private string m_strPort;
        private string m_strPRoConVersion;

        private enumBoolYesNo m_enMoveOnlyNew;
        private enumBoolYesNo m_enMoveOnJoin;
        private enumBoolYesNo m_enLimitSquad;
        private List<string> m_LPlayersToSquad;
        private List<string> m_LPlayersToIgnore;
        private List<string> m_LPlayersLeft;
        private Dictionary<string, long> m_DLastTeamChange;
        private Dictionary<string, int[]> m_DPlayers;
        private Dictionary<int, int>[] m_ASquadCount;
        private int[] m_ATeamCount;
        private int[][] m_ASquadAttempts;
        private int[] m_APlayersToSquadCount;
        private bool m_blRoundRunning;
        private long m_lLastRoundEnded;
        private int m_iCurrentServerSize;
        private string m_strCurrentGameMode;
        private int m_iCurrentPlayerCount;

        private enumBoolYesNo m_enDoDebugOutput;

        private bool m_isPluginEnabled;
        private bool m_isPluginInitialized;

        public CSquadEnforcer()
        {
            this.m_enMoveOnlyNew = enumBoolYesNo.No;
            this.m_enMoveOnJoin = enumBoolYesNo.No;
            this.m_enLimitSquad = enumBoolYesNo.No;
            this.m_LPlayersToSquad = new List<string>();
            this.m_LPlayersToIgnore = new List<string>();
            this.m_LPlayersLeft = new List<string>();
            this.m_DLastTeamChange = new Dictionary<string, long>();
            this.m_DPlayers = new Dictionary<string, int[]>();
            this.m_ASquadCount = new Dictionary<int, int>[5];
            this.m_ASquadCount[1] = new Dictionary<int, int>();
            this.m_ASquadCount[2] = new Dictionary<int, int>();
            this.m_ASquadCount[3] = new Dictionary<int, int>();
            this.m_ASquadCount[4] = new Dictionary<int, int>();
            this.m_ATeamCount = new int[5];
            this.m_ASquadAttempts = new int[5][];
            this.m_APlayersToSquadCount = new int[5];
            this.m_blRoundRunning = false;
            this.m_lLastRoundEnded = DateTime.UtcNow.Ticks / 10000000;
            this.m_iCurrentServerSize = 0;
            this.m_strCurrentGameMode = "";
            this.m_iCurrentPlayerCount = 0;

            this.m_enDoDebugOutput = enumBoolYesNo.No;

            this.m_isPluginEnabled = false;
            this.m_isPluginInitialized = false;
        }

        public string GetPluginName()
        {
            return "Squad Enforcer";
        }

        public string GetPluginVersion()
        {
            return "1.0.3.0";
        }

        public string GetPluginAuthor()
        {
            return "falcontx";
        }

        public string GetPluginWebsite()
        {
            return "www.phogue.net/forumvb/showthread.php?3137";
        }

        public string GetPluginDescription()
        {
            return @"
<p>If you find this plugin useful, please consider supporting falcontx's development efforts. Donations help support the servers used for development and provide incentive for additional features and new plugins! Any amount would be appreciated!</p>

    <table class=""table"" border=""0"" cellpadding=""0"" cellspacing=""0"">
    <tr>
    <td style=""text-align:center"">
    <form action=""https://authorize.payments.amazon.com/pba/paypipeline"" method=""post"" target=""_blank"">
	  <input type=""hidden"" name=""immediateReturn"" value=""0"" >
	  <input type=""hidden"" name=""collectShippingAddress"" value=""0"" >
	  <input type=""hidden"" name=""signature"" value=""G03VqesFp7oCw4XrOPCaKBXknFEjwnaTHQi5DBtj9JI="" >
	  <input type=""hidden"" name=""isDonationWidget"" value=""1"" >
	  <input type=""hidden"" name=""signatureVersion"" value=""2"" >
	  <input type=""hidden"" name=""signatureMethod"" value=""HmacSHA256"" >
	  <input type=""hidden"" name=""description"" value=""Free Plugin Development (Squad Enforcer)"" >
	  <input type=""hidden"" name=""amazonPaymentsAccountId"" value=""PWDEKNSSNGEV5AGJ6TAXZ86M8JBZGIQEI5ACI6"" >
	  <input type=""hidden"" name=""accessKey"" value=""11SEM03K88SD016FS1G2"" >
	  <input type=""hidden"" name=""cobrandingStyle"" value=""logo"" >
	  <input type=""hidden"" name=""processImmediate"" value=""1"" >
    $&nbsp;<input type=""text"" name=""amount"" size=""8"" value=""""> &nbsp;&nbsp;<br><div style=""padding-top:4px;""></div>

    <input type=""image"" src=""http://g-ecx.images-amazon.com/images/G/01/asp/golden_small_donate_withlogo_lightbg.gif"" border=""0"">
    </form>
    </td>
    <td style=""text-align:center; background-color:#ffffff""><br>or
    </td>
    <td style=""text-align:center"">
    <form action=""https://www.paypal.com/cgi-bin/webscr"" method=""post"" target=""_blank"">
    <input type=""hidden"" name=""cmd"" value=""_donations"">
    <input type=""hidden"" name=""business"" value=""XZBACYX9CK6YA"">
    <input type=""hidden"" name=""lc"" value=""US"">
    <input type=""hidden"" name=""item_name"" value=""Support Free Plugin Development (Squad Enforcer)"">
    <input type=""hidden"" name=""currency_code"" value=""USD"">
    <input type=""hidden"" name=""bn"" value=""PP-DonationsBF:btn_donate_LG.gif:NonHosted"">
    <input type=""image"" src=""https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif"" border=""0"" name=""submit"" alt=""PayPal - The safer, easier way to pay online!""><br>
    <br>
    <img alt="""" border=""0"" src=""https://www.paypalobjects.com/en_US/ebook/PP_ExpressCheckout_IntegrationGuide/images/PayPal_mark_50x34.gif""
    </form>
    </td>
    </tr>
    </table>

<h2>Description</h2>
    <p>This plugin is intended to get players into squads automatically. When enabled, squads up players who are not already in a squad. Players are assigned to the most full squads first, then empty squads if all other squads are full. If a player intentionally leaves a squad, they will be ignored by the plugin, as it is assumed that the player is either looking for a new squad to join, or desires not to be in a squad.</p>

    <p>Known issues:<br>
        - Due to a server-side bug, the plugin is not able to move players if a team is full. Hopefully this will be fixed soon, but knowing EA/DICE, it'll be a while.</p>

<h2>Commands</h2>
    <p>This plug-in has no in-game commands.</p>

<h2>Settings</h2>
    <br><h3>Squad Enforcer</h3>
        <blockquote><h4>Move immediately on join?</h4> When set to <b>""Yes""</b>, the plugin will attempt to move player(s) immediately after they join or change teams. Technically, there is no problem with this, but if a player joins with a platoon or party, they may not be placed in the squad that Battlelog intends (it's about a 50/50 chance). When set to <b>""No""</b>, the plugin waits to move the player until after their next death, which alleviates this issue. <i>[Recommended setting for Conquest, Rush, and TDM: No] [Recommended setting for Squad Deathmatch and Squad Rush: Yes]</i></blockquote>
        <blockquote><h4>Move only new players?</h4> When set to <b>""Yes""</b>, only players who have just joined or changed teams may be moved. When set to <b>""No""</b>, all players not currently in squads may be moved. For example, if this is set to ""Yes"", players who are not in a squad when the plugin is enabled will not be moved, because they are not considered new players. <i>[Recommended setting: No]</i></blockquote>
        <blockquote><h4>Limit Squad Rush and Squad Deathmatch to 4 players per squad?</h4> When set to <b>""Yes""</b>, the plugin will not assign more than 4 players to a squad in Squad Rush or Squad Deathmatch. When set to <b>""No""</b>, all players on each team will be assigned to the same squad (Alpha). <i>[Recommended setting: No]</i></blockquote>
    <br><h3>Extras</h3>
        <blockquote><h4>Enable debug output?</h4> If enabled, displays debug info in the console window.</blockquote>

<br><h2>Development</h2>
    <br><h3>Changelog</h3>
        <blockquote><h4>1.0.3.0 (01/13/2012)</h4>
            - added option to limit squad size in Squad Rush and Squad Deathmatch game modes<br/>
        </blockquote>
        <blockquote><h4>1.0.2.3 (01/05/2012)</h4>
            - fixed wrong number of players detected when listplayers not called with 'all'<br/>
        </blockquote>
        <blockquote><h4>1.0.2.2 (12/19/2011)</h4>
            - updated to allow more than 4 per squad for oversize Squad Deathmatch servers (tested)<br/>
            - disabled squad skip for Squad Deathmatch and Squad Rush<br/>
        </blockquote>
        <blockquote><h4>1.0.2.0 (12/05/2011)</h4>
            - fixed first player to join empty server not moved when ""Move immediately on join"" is enabled<br/>
            - fixed plugin trying to move players into full squads due to change in previous version<br/>
            - fixed minor bugs<br/>
            - added PayPal donation option<br/>
        </blockquote>
        <blockquote><h4>1.0.1.0 (12/01/2011)</h4>
            - updated to work with Squad Rush (tested) and Squad Deathmatch (untested)<br/>
        </blockquote>
        <blockquote><h4>1.0.0.0 (12/01/2011)</h4>
            - initial version<br/>
        </blockquote>
";
        }

        #region pluginSetup

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
        {
            this.m_strHostName = strHostName;
            this.m_strPort = strPort;
            this.m_strPRoConVersion = strPRoConVersion;

            this.RegisterEvents(this.GetType().Name, "OnListPlayers", "OnPlayerTeamChange", "OnPlayerSquadChange", "OnPlayerLeft", "OnPlayerKilled", "OnLevelLoaded", "OnPlayerSpawned", "OnRoundOver", "OnPlayerLimit", "OnServerInfo");
        }

        public void OnPluginEnable()
        {
            this.m_isPluginEnabled = true;
            this.m_isPluginInitialized = false;
            this.m_LPlayersToSquad = new List<string>();
            this.m_LPlayersToIgnore = new List<string>();
            this.m_LPlayersLeft = new List<string>();
            this.m_DLastTeamChange = new Dictionary<string, long>();
            this.m_ASquadAttempts = new int[5][];
            this.m_ASquadAttempts[1] = new int[9];
            this.m_ASquadAttempts[2] = new int[9];
            this.m_ASquadAttempts[3] = new int[9];
            this.m_ASquadAttempts[4] = new int[9];
            this.m_DPlayers = new Dictionary<string, int[]>();
            this.m_iCurrentPlayerCount = 0;
            this.m_blRoundRunning = false;
            this.m_strCurrentGameMode = "";
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bSquadEnforcer: ^2Enabled!");
        }

        public void OnPluginDisable()
        {
            this.m_isPluginEnabled = false;
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bSquadEnforcer: ^1Disabled =(");
        }

        // Lists only variables you want shown.. for instance enabling one option might hide another
        // option It's the best I got until I implement a way for plugins to display their own small interfaces.
        public List<CPluginVariable> GetDisplayPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("Squad Enforcer|Move immediately on join?", typeof(enumBoolYesNo), this.m_enMoveOnJoin));
            lstReturn.Add(new CPluginVariable("Squad Enforcer|Move only new players?", typeof(enumBoolYesNo), this.m_enMoveOnlyNew));
            lstReturn.Add(new CPluginVariable("Squad Enforcer|Limit Squad Rush and Squad Deathmatch to 4 players per squad?", typeof(enumBoolYesNo), this.m_enLimitSquad));
            lstReturn.Add(new CPluginVariable("Xtras|Enable debug output?", typeof(enumBoolYesNo), this.m_enDoDebugOutput));

            return lstReturn;
        }

        // Lists all of the plugin variables.
        public List<CPluginVariable> GetPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("Move immediately on join?", typeof(enumBoolYesNo), this.m_enMoveOnJoin));
            lstReturn.Add(new CPluginVariable("Move only new players?", typeof(enumBoolYesNo), this.m_enMoveOnlyNew));
            lstReturn.Add(new CPluginVariable("Limit Squad Rush and Squad Deathmatch to 4 players per squad?", typeof(enumBoolYesNo), this.m_enLimitSquad));
            lstReturn.Add(new CPluginVariable("Enable debug output?", typeof(enumBoolYesNo), this.m_enDoDebugOutput));
            return lstReturn;
        }

        // Allways be suspicious of strValue's actual value. A command in the console can by the user
        // can put any kind of data it wants in strValue. use type.TryParse
        public void SetPluginVariable(string strVariable, string strValue)
        {
            int iValue = 0;

            if (strVariable.CompareTo("Move immediately on join?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enMoveOnJoin = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Move only new players?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enMoveOnlyNew = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Limit Squad Rush and Squad Deathmatch to 4 players per squad?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enLimitSquad = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            /* extras */
            else if (strVariable.CompareTo("Enable debug output?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoDebugOutput = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
        }

        private void UnregisterAllCommands()
        {
        }

        private void SetupHelpCommands()
        {
        }

        private void RegisterAllCommands()
        {
        }

        #endregion

        #region Events

        public override void OnListPlayers(List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset)
        {
            if (cpsSubset.Subset == CPlayerSubset.PlayerSubsetType.All)
            {
                this.m_iCurrentPlayerCount = lstPlayers.Count;
                if (this.m_blRoundRunning)
                {
                    this.m_ASquadCount[1][0] = 0;
                    this.m_ASquadCount[1][1] = 0;
                    this.m_ASquadCount[1][2] = 0;
                    this.m_ASquadCount[1][3] = 0;
                    this.m_ASquadCount[1][4] = 0;
                    this.m_ASquadCount[1][5] = 0;
                    this.m_ASquadCount[1][6] = 0;
                    this.m_ASquadCount[1][7] = 0;
                    this.m_ASquadCount[1][8] = 0;
                    this.m_ASquadCount[2][0] = 0;
                    this.m_ASquadCount[2][1] = 0;
                    this.m_ASquadCount[2][2] = 0;
                    this.m_ASquadCount[2][3] = 0;
                    this.m_ASquadCount[2][4] = 0;
                    this.m_ASquadCount[2][5] = 0;
                    this.m_ASquadCount[2][6] = 0;
                    this.m_ASquadCount[2][7] = 0;
                    this.m_ASquadCount[2][8] = 0;
                    this.m_ASquadCount[3][0] = 0;
                    this.m_ASquadCount[3][1] = 0;
                    this.m_ASquadCount[3][2] = 0;
                    this.m_ASquadCount[3][3] = 0;
                    this.m_ASquadCount[3][4] = 0;
                    this.m_ASquadCount[3][5] = 0;
                    this.m_ASquadCount[3][6] = 0;
                    this.m_ASquadCount[3][7] = 0;
                    this.m_ASquadCount[3][8] = 0;
                    this.m_ASquadCount[4][0] = 0;
                    this.m_ASquadCount[4][1] = 0;
                    this.m_ASquadCount[4][2] = 0;
                    this.m_ASquadCount[4][3] = 0;
                    this.m_ASquadCount[4][4] = 0;
                    this.m_ASquadCount[4][5] = 0;
                    this.m_ASquadCount[4][6] = 0;
                    this.m_ASquadCount[4][7] = 0;
                    this.m_ASquadCount[4][8] = 0;
                    this.m_ATeamCount = new int[5];
                    this.m_APlayersToSquadCount = new int[5];
                    foreach (CPlayerInfo cpiPlayer in lstPlayers)
                    {
                        this.m_ATeamCount[cpiPlayer.TeamID]++;
                        this.m_ASquadCount[cpiPlayer.TeamID][cpiPlayer.SquadID]++;
                        this.m_DPlayers[cpiPlayer.SoldierName] = new int[] { cpiPlayer.TeamID, cpiPlayer.SquadID };
                        if (!this.m_LPlayersToSquad.Contains(cpiPlayer.SoldierName) && !this.m_LPlayersToIgnore.Contains(cpiPlayer.SoldierName) && cpiPlayer.SquadID == 0 && this.m_enMoveOnlyNew == enumBoolYesNo.No)
                        {
                            this.m_LPlayersToSquad.Add(cpiPlayer.SoldierName);
                            this.m_APlayersToSquadCount[cpiPlayer.TeamID]++;
                            WritePluginConsole("TASK -> Need to squad " + cpiPlayer.SoldierName + ".");
                        }
                    }
                }
            }
        }

        public override void OnServerInfo(CServerInfo csiServerInfo)
        {
            this.m_strCurrentGameMode = csiServerInfo.GameMode;
        }

        public override void OnPlayerLimit(int limit)
        {
            this.m_iCurrentServerSize = limit;
        }

        public override void OnPlayerTeamChange(string soldierName, int teamId, int squadId)
        {
            if (this.m_blRoundRunning)
            {
                this.m_DLastTeamChange[soldierName] = DateTime.UtcNow.Ticks / 10000000;
                if (this.m_DPlayers.ContainsKey(soldierName))
                {
                    this.m_ASquadCount[this.m_DPlayers[soldierName][0]][this.m_DPlayers[soldierName][1]]--;
                }
                this.m_ASquadCount[teamId][squadId]++;
                this.m_DPlayers[soldierName] = new int[] { teamId, squadId };

                if (!this.m_LPlayersToSquad.Contains(soldierName) && !this.m_LPlayersToIgnore.Contains(soldierName) && squadId == 0)
                {
                    this.m_LPlayersToSquad.Add(soldierName);
                    this.m_APlayersToSquadCount[teamId]++;
                    WritePluginConsole("TASK -> Need to squad " + soldierName + ". (join/team change)");
                    if (this.m_enMoveOnJoin == enumBoolYesNo.Yes)
                    {
                        CheckPlayer(soldierName);
                    }
                }
            }
        }

        public override void OnPlayerSquadChange(string soldierName, int teamId, int squadId)
        {
            if (this.m_blRoundRunning)
            {
                if (!this.m_LPlayersLeft.Contains(soldierName))
                {
                    this.m_ASquadCount[this.m_DPlayers[soldierName][0]][this.m_DPlayers[soldierName][1]]--;
                    this.m_ASquadCount[teamId][squadId]++;
                    this.m_DPlayers[soldierName] = new int[] { teamId, squadId };
                    if (squadId == 0 && DateTime.UtcNow.Ticks / 10000000 - this.m_DLastTeamChange[soldierName] > 1 && !this.m_LPlayersToIgnore.Contains(soldierName))
                    {
                        this.m_LPlayersToIgnore.Add(soldierName);
                        WritePluginConsole("INFO -> Ignoring " + soldierName + ". (left squad)");
                    }
                    else
                    {
                        if (this.m_LPlayersToSquad.Contains(soldierName))
                        {
                            WritePluginConsole("INFO -> " + soldierName + " moved to Team " + teamId + " Squad " + squadId + ".");
                            this.m_LPlayersToSquad.Remove(soldierName);
                        }
                        this.m_ASquadAttempts[teamId][squadId] = 0;
                    }
                }
                else
                {
                    this.m_LPlayersLeft.Remove(soldierName);
                }
            }
        }

        public override void OnPlayerLeft(CPlayerInfo playerInfo)
        {
            if (this.m_blRoundRunning)
            {
                this.m_ASquadCount[this.m_DPlayers[playerInfo.SoldierName][0]][this.m_DPlayers[playerInfo.SoldierName][1]]--;
                this.m_DPlayers.Remove(playerInfo.SoldierName);

                if (this.m_LPlayersToSquad.Contains(playerInfo.SoldierName))
                {
                    WritePluginConsole("INFO -> " + playerInfo.SoldierName + " quit.");
                    this.m_LPlayersToSquad.Remove(playerInfo.SoldierName);
                }
                if (this.m_LPlayersToIgnore.Contains(playerInfo.SoldierName))
                {
                    this.m_LPlayersToIgnore.Remove(playerInfo.SoldierName);
                }
                if (!this.m_LPlayersLeft.Contains(playerInfo.SoldierName))
                {
                    this.m_LPlayersLeft.Add(playerInfo.SoldierName);
                }
                if (!this.m_DLastTeamChange.ContainsKey(playerInfo.SoldierName))
                {
                    this.m_DLastTeamChange.Remove(playerInfo.SoldierName);
                }
            }
        }

        public override void OnPlayerKilled(Kill kKillerVictimDetails)
        {
            if (this.m_blRoundRunning)
            {
                CheckPlayer(kKillerVictimDetails.Victim.SoldierName);
            }
        }

        public override void OnRoundOver(int iWinningTeamID)
        {
            this.m_lLastRoundEnded = DateTime.UtcNow.Ticks / 10000000;
            this.m_blRoundRunning = false;
        }

        public override void OnLevelLoaded(string mapFileName, string Gamemode, int roundsPlayed, int roundsTotal)
        {
            this.m_blRoundRunning = false;
            this.m_lLastRoundEnded = DateTime.UtcNow.Ticks / 10000000 - 63;
            if (this.m_iCurrentPlayerCount == 0)
            {
                WritePluginConsole("INFO -> Level loaded.");
                StartRound();
            }
        }

        public override void OnPlayerSpawned(string soldierName, Inventory spawnedInventory)
        {
            if (!this.m_blRoundRunning && DateTime.UtcNow.Ticks / 10000000 - this.m_lLastRoundEnded > 60)
            {
                WritePluginConsole("INFO -> Detected round running.");
                StartRound();
            }
        }

        #endregion

        private void StartRound()
        {
            if (this.m_isPluginInitialized)
            {
                this.ExecuteCommand("procon.protected.send", "admin.listPlayers all");
                this.m_blRoundRunning = true;
                this.m_ASquadAttempts[1] = new int[9];
                this.m_ASquadAttempts[2] = new int[9];
                this.m_ASquadAttempts[3] = new int[9];
                this.m_ASquadAttempts[4] = new int[9];
            }
            else
            {
                this.m_lLastRoundEnded = DateTime.UtcNow.Ticks / 10000000 - 63;
                this.m_blRoundRunning = true;
                this.m_isPluginInitialized = true;
                this.ExecuteCommand("procon.protected.send", "vars.maxPlayers");
                this.ExecuteCommand("procon.protected.send", "admin.listPlayers all");
            }
        }

        private void CheckPlayer(string soldierName)
        {
            if (this.m_LPlayersToSquad.Contains(soldierName) && !this.m_LPlayersToIgnore.Contains(soldierName) && this.m_DPlayers[soldierName][1] == 0 && (this.m_ATeamCount[this.m_DPlayers[soldierName][0]] != (int)Math.Round((double)this.m_iCurrentServerSize / 2) || this.m_strCurrentGameMode == "SquadDeathMatch0" && this.m_ATeamCount[this.m_DPlayers[soldierName][0]] != (int)Math.Round((double)this.m_iCurrentServerSize / 4)))
            {
                List<KeyValuePair<int, int>> SortedSquads = new List<KeyValuePair<int, int>>(m_ASquadCount[this.m_DPlayers[soldierName][0]]);
                SortedSquads.Sort(delegate (KeyValuePair<int, int> left, KeyValuePair<int, int> right)
                {
                    if (right.Value.CompareTo(left.Value) == 0)
                    {
                        return left.Key.CompareTo(right.Key);
                    }
                    else
                    {
                        return right.Value.CompareTo(left.Value);
                    }
                });
                foreach (KeyValuePair<int, int> Squad in SortedSquads)
                {
                    if (Squad.Key > 0 && (Squad.Value < 4 && this.m_ASquadAttempts[this.m_DPlayers[soldierName][0]][Squad.Key] <= 1 || this.m_enLimitSquad != enumBoolYesNo.Yes && (this.m_strCurrentGameMode == "SquadRush0" || this.m_strCurrentGameMode == "SquadDeathMatch0")))
                    {
                        WritePluginConsole("WORK -> Attempting to move " + soldierName + " to Team " + this.m_DPlayers[soldierName][0] + " Squad " + Squad.Key + ".");
                        this.m_ASquadAttempts[this.m_DPlayers[soldierName][0]][Squad.Key]++;
                        this.ExecuteCommand("procon.protected.send", "admin.movePlayer", soldierName, this.m_DPlayers[soldierName][0].ToString(), Squad.Key.ToString(), "false");
                        break;
                    }
                }
            }
        }

        #region helper_functions

        private void WritePluginConsole(string message)
        {
            string line = String.Format("SquadEnforcer: {0}", message);
            if (this.m_enDoDebugOutput == enumBoolYesNo.Yes)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", line);
            }
        }

        #endregion
    }
}
/*  Copyright 2011 Nick 'MorpheusX(AUT)' Mueller

    This file is part of MorpheusX(AUT)'s Plugins for Procon.

    MorpheusX(AUT)'s Plugins for Procon is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MorpheusX(AUT)'s Plugins for Procon is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MorpheusX(AUT)'s Plugins for Procon.  If not, see <http://www.gnu.org/licenses/>.

 */

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;

namespace PRoConEvents {
    public class CPingPlayers : PRoConPluginAPI, IPRoConPluginInterface
    {
        private string strHostName;
        private string strPort;
        private string strPRoConVersion;
        private string strGameserverIP;
        private bool blStartupTests;
        private Thread tStartupTester;
        private bool blConnectionToGameserverOK;
        private bool blConnectionToGoogleOK;
        private CServerInfo csiServerInfo;

        private Dictionary<string, CPlayerInfo> dicPlayerInfo;
        private Dictionary<string, CPingPlayer> dicPingPlayer;

        private string strPingInterval;
        private int iMaxPing;
        private int iGracePeriod;
        private int iPingTimeout;
        private string strKickReason;
        private List<string> lstWhiteList;
        private enumBoolYesNo ebIngameWarnings;
        private enumBoolYesNo ebDebugMessages;
        private string strIngameCommandOwn;
        private string strIngameCommandPlayer;
        private int iPingStorageLength;
        private int iMaxPingGameserver;
        private int iMaxPingGoogle;

        private bool blPluginEnabled;

        public CPingPlayers()
        {
            this.blStartupTests = false;
            this.blConnectionToGameserverOK = false;
            this.blConnectionToGoogleOK = false;

            this.dicPlayerInfo = new Dictionary<string, CPlayerInfo>();
            this.dicPingPlayer = new Dictionary<string, CPingPlayer>();

            this.strPingInterval = "60";
            this.iMaxPing = 250;
            this.iGracePeriod = 3;
            this.iPingTimeout = 1000;
            this.strKickReason = "[Ping Players] Your Ping is over %MAXPING%. Goodbye!";
            this.lstWhiteList = new List<string>();
            this.lstWhiteList.Add("MorpheusXAUT");
            this.ebIngameWarnings = enumBoolYesNo.Yes;
            this.ebDebugMessages = enumBoolYesNo.No;
            this.strIngameCommandOwn = "myping";
            this.strIngameCommandPlayer = "ping";
            this.iPingStorageLength = 10;
            this.iMaxPingGameserver = 150;
            this.iMaxPingGoogle = 150;

            this.blPluginEnabled = false;
        }

        public string GetPluginName()
        {
            return "Ping Players";
        }

        public string GetPluginVersion()
        {
            return "1.0.1.1";
        }

        public string GetPluginAuthor()
        {
            return "MorpheusX(AUT)";
        }

        public string GetPluginWebsite()
        {
            return "http://www.phogue.net/forumvb/showthread.php?3139-PingPlayers-(1.0.1.0-02.12.2011)-BF3";
        }

        public string GetPluginDescription()
        {
            return @"<p align='center'>If you like my work, please consider donating!<br /><br />
            <form action='https://www.paypal.com/cgi-bin/webscr' method='post'>
            <input type='hidden' name='cmd' value='_s-xclick'>
            <input type='hidden' name='hosted_button_id' value='PLFJH26HK79AG'>
            <input type='image' src='https://www.paypal.com/en_US/i/btn/btn_donate_LG.gif' border='0' name='submit' alt='PayPal - The safer, easier way to pay online!'>
            <img alt='' border='0' src='https://www.paypal.com/de_DE/i/scr/pixel.gif' width='1' height='1'>
            </form>
            <a href='https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=PLFJH26HK79AG'>Donation-Link</a></p>
            <h2>Description</h2>
            <p>This plugin can be used as a partial replacement of the non-exisiting Ping-functionality in BF3.<br />
            It pings all players (which aren't listed on the whitelist) on a regular basis, thus allowing the admin to kick highpingers automatically.<br />
            Due to some players having routers with strict NAT or simply blocking ICMP-Responses, this Ping-Method will only work for about 30-50% of all players, but at least check them ;-)<br /><br />
            <b>Please be aware that this plugin will NOT work properly if Sandbox is enabled. This will probably make it hard to be run on a rented layer by some GSP. Sorry about that.</b></p>
            
            <h2>Settings</h2>
            <blockquote><h4>Ping Interval</h4>
			<p><i>Seconds between each PingAllPlayers-Command. Recommendend: 60</i></p>
			</blockquote>
            <blockquote><h4>Max. Ping</h4>
			<p><i>Maximum allowed Ping. Values greater will trigger the plugin to warn/kick. Recommended: 250</i></p>
			</blockquote>
            <blockquote><h4>Grace Period</h4>
			<p><i>Number of Highpings before a player gets kicked (Grace Period of 3 -> 3x Highping, on 4th check kick). Recommended: 3</i></p>
			</blockquote>
            <blockquote><h4>Ping Timeout</h4>
			<p><i>Milliseconds the plugin will wait before stating that a player isn't pingable. Higher value means more certainty, but also slower plugin-work. Recommended: 1000</i></p>
			</blockquote>
            <blockquote><h4>Kick Reason</h4>
			<p><i>Reason displayed to the user when he is kicked. %MAXPING% will be replaced with your Max. Ping - Value.</i></p>
			</blockquote>
            <blockquote><h4>White List</h4>
			<p><i>Lists all player which aren't checked by the plugin at all.</i></p>
			</blockquote>
            <blockquote><h4>Ingame Warnings</h4>
			<p><i>Displays warnings about Highping to the player's squad before kicking him. Recommended: Yes</i></p>
			</blockquote>
            <blockquote><h4>Debug Messages</h4>
			<p><i>Shows debug messages and Ping-Replies in the Plugin Console. Recommended: No</i></p>
			</blockquote>
            <blockquote><h4>Ingame Command Own Ping</h4>
			<p><i>Ingame-Command for requesting the own ping. This works with all scopes (@, #, !). Recommended: myping</i></p>
			</blockquote>
            <blockquote><h4>Ingame Command Other Player's Ping</h4>
			<p><i>Ingame-Command for requesting another player's ping. This works with all scopes (@, #, !). Recommended: ping</i></p>
			</blockquote>
            <blockquote><h4>Ping-Reply Storage Length</h4>
			<p><i>Number of Ping-Replies stored for each player. Greater values saves more results to display, but costs more ressources and makes the output more confusing. Recommended: 10</i></p>
			</blockquote>
            <blockquote><h4>Max. Ping to Gameserver</h4>
			<p><i>Maximum allowed Ping to your Gameserver. The plugin repeats a lagcheck ever 4 hours to make sure it's own internetconnection is solid. Recommended: 150</i></p>
			</blockquote>
            <blockquote><h4>Max. Ping to Google</h4>
			<p><i>Maximum allowed Ping to google.com. The plugin repeats a lagcheck ever 4 hours to make sure it's own internetconnection is solid. Recommended: 150</i></p>
			</blockquote>";
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
        {
            this.blStartupTests = false;
            this.strHostName = strHostName;
            this.strPort = strPort;
            this.strPRoConVersion = strPRoConVersion;

			this.RegisterEvents(this.GetType().Name, "OnPlayerLeft", "OnListPlayers", "OnPunkbusterPlayerInfo", "OnServerInfo");
        }

        public void OnPluginEnable()
        {
            this.blStartupTests = false;
            this.dicPlayerInfo.Clear();
            this.dicPingPlayer.Clear();

            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPing Players: ^2Enabled!");
            this.ExecuteCommand("procon.protected.send", "serverInfo");
            this.ExecuteCommand("procon.protected.send", "punkBuster.pb_sv_command", "pb_sv_plist");
            this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");

            this.ExecuteCommand("procon.protected.tasks.remove", "CPingPlayersWorker");
            this.ExecuteCommand("procon.protected.tasks.remove", "CPingPlayersLagCheck");
            this.ExecuteCommand("procon.protected.tasks.add", "CPingPlayersLagCheck", "5", "14400000", "-1", "procon.protected.plugins.call", "CPingPlayers", "LagCheck");
            this.ExecuteCommand("procon.protected.tasks.add", "CPingPlayersWorker", "30", this.strPingInterval, "-1", "procon.protected.plugins.call", "CPingPlayers", "PingAllPlayers"); 

            this.RegisterAllCommands();

            this.blPluginEnabled = true;
        }

        public void OnPluginDisable()
        {
            this.blStartupTests = false;
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bPing Players: ^1Disabled =(" );

            this.ExecuteCommand("procon.protected.tasks.remove", "CPingPlayersWorker");
            this.ExecuteCommand("procon.protected.tasks.remove", "CPingPlayersLagCheck");

            this.UnregisterAllCommands();

            this.dicPlayerInfo.Clear();
            this.dicPingPlayer.Clear();

            this.blPluginEnabled = false;
        }

        // Lists only variables you want shown.. for instance enabling one option might hide another option
        // It's the best I got until I implement a way for plugins to display their own small interfaces.
        public List<CPluginVariable> GetDisplayPluginVariables()
        {

            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("1. Ping Settings|Ping Interval", typeof(string), this.strPingInterval));
            lstReturn.Add(new CPluginVariable("1. Ping Settings|Max. Ping", typeof(int), this.iMaxPing));
            lstReturn.Add(new CPluginVariable("1. Ping Settings|Grace Period", typeof(int), this.iGracePeriod));
            lstReturn.Add(new CPluginVariable("1. Ping Settings|Ping Timeout", typeof(int), this.iPingTimeout));
            lstReturn.Add(new CPluginVariable("2. Miscellaneous|Kick Reason", typeof(string), this.strKickReason));
            lstReturn.Add(new CPluginVariable("2. Miscellaneous|White List", typeof(string[]), this.lstWhiteList.ToArray()));
            lstReturn.Add(new CPluginVariable("2. Miscellaneous|Ingame Warnings", typeof(enumBoolYesNo), this.ebIngameWarnings));
            lstReturn.Add(new CPluginVariable("2. Miscellaneous|Debug Messages", typeof(enumBoolYesNo), this.ebDebugMessages));
            lstReturn.Add(new CPluginVariable("2. Miscellaneous|Ingame Command Own Ping", typeof(string), this.strIngameCommandOwn));
            lstReturn.Add(new CPluginVariable("2. Miscellaneous|Ingame Command Other Player's Ping", typeof(string), this.strIngameCommandPlayer));
            lstReturn.Add(new CPluginVariable("2. Miscellaneous|Ping-Reply Storage Length", typeof(int), this.iPingStorageLength));
            lstReturn.Add(new CPluginVariable("2. Miscellaneous|Max. Ping to Gameserver", typeof(int), this.iMaxPingGameserver));
            lstReturn.Add(new CPluginVariable("2. Miscellaneous|Max. Ping to Google", typeof(int), this.iMaxPingGoogle));

            return lstReturn;
        }

        // Lists all of the plugin variables.
        public List<CPluginVariable> GetPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("Ping Interval", typeof(string), this.strPingInterval));
            lstReturn.Add(new CPluginVariable("Max. Ping", typeof(int), this.iMaxPing));
            lstReturn.Add(new CPluginVariable("Grace Period", typeof(int), this.iGracePeriod));
            lstReturn.Add(new CPluginVariable("Ping Timeout", typeof(int), this.iPingTimeout));
            lstReturn.Add(new CPluginVariable("Kick Reason", typeof(string), this.strKickReason));
            lstReturn.Add(new CPluginVariable("White List", typeof(string[]), this.lstWhiteList.ToArray()));
            lstReturn.Add(new CPluginVariable("Ingame Warnings", typeof(enumBoolYesNo), this.ebIngameWarnings));
            lstReturn.Add(new CPluginVariable("Debug Messages", typeof(enumBoolYesNo), this.ebDebugMessages));
            lstReturn.Add(new CPluginVariable("Ingame Command Own Ping", typeof(string), this.strIngameCommandOwn));
            lstReturn.Add(new CPluginVariable("Ingame Command Other Player's Ping", typeof(string), this.strIngameCommandPlayer));
            lstReturn.Add(new CPluginVariable("Ping-Reply Storage Length", typeof(int), this.iPingStorageLength));

            return lstReturn;
        }

        // Allways be suspicious of strValue's actual value.  A command in the console can
        // by the user can put any kind of data it wants in strValue.
        // use type.TryParse
        public void SetPluginVariable(string strVariable, string strValue) 
        {
            int iTmp;

            this.UnregisterAllCommands();

            if (strVariable.CompareTo("Ping Interval") == 0)
            {
                if (int.TryParse(strValue, out iTmp) && iTmp > 0)
                {
                    this.strPingInterval = strValue;
                    this.ExecuteCommand("procon.protected.tasks.remove", "CPingPlayersWorker");
                    this.ExecuteCommand("procon.protected.tasks.add", "CPingPlayersWorker", "5", this.strPingInterval, "-1", "procon.protected.plugins.call", "CPingPlayers", "PingAllPlayers");
                }
            }
            else if (strVariable.CompareTo("Max. Ping") == 0)
            {
                if (int.TryParse(strValue, out iTmp) && iTmp > 0)
                {
                    this.iMaxPing = iTmp;
                }
            }
            else if (strVariable.CompareTo("Grace Period") == 0)
            {
                if (int.TryParse(strValue, out iTmp) && iTmp > 0)
                {
                    this.iGracePeriod = iTmp;
                }
            }
            else if (strVariable.CompareTo("Ping Timeout") == 0)
            {
                if (int.TryParse(strValue, out iTmp) && iTmp > 0)
                {
                    this.iPingTimeout = iTmp;
                }
            }
            else if (strVariable.CompareTo("Kick Reason") == 0)
            {
                this.strKickReason = strValue;
            }
            else if (strVariable.CompareTo("White List") == 0)
            {
                this.lstWhiteList = new List<string>(CPluginVariable.DecodeStringArray(strValue));
            }
            else if (strVariable.CompareTo("Ingame Warnings") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ebIngameWarnings = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Debug Messages") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.ebDebugMessages = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Ingame Command Own Ping") == 0)
            {
                this.strIngameCommandOwn = strValue;
            }
            else if (strVariable.CompareTo("Ingame Command Other Player's Ping") == 0)
            {
                this.strIngameCommandPlayer = strValue;
            }
            else if (strVariable.CompareTo("Ping-Reply Storage Length") == 0)
            {
                if (int.TryParse(strValue, out iTmp) && iTmp > 0)
                {
                    this.iPingStorageLength = iTmp;
                }
            }

            this.RegisterAllCommands();
        }

        private void UnregisterAllCommands()
        {
            this.UnregisterCommand(new MatchCommand("CPingPlayers", "OnCommandOwnPing", this.Listify<string>("@", "!", "#"), this.strIngameCommandOwn, this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), "Provides a player with their ping to the gameserver (if available)"));
            this.UnregisterCommand(new MatchCommand("CPingPlayers", "OnCommandPingPlayer", this.Listify<string>("@", "!", "#"), this.strIngameCommandPlayer, this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("playername", new List<string>(this.dicPingPlayer.Keys))), new ExecutionRequirements(ExecutionScope.All), "Provides a player with another player's ping to the gameserver (if available)"));
        }

        private void RegisterAllCommands()
        {
            this.RegisterCommand(new MatchCommand("CPingPlayers", "OnCommandOwnPing", this.Listify<string>("@", "!", "#"), this.strIngameCommandOwn, this.Listify<MatchArgumentFormat>(), new ExecutionRequirements(ExecutionScope.All), "Provides a player with their ping to the gameserver (if available)"));
            this.RegisterCommand(new MatchCommand("CPingPlayers", "OnCommandPingPlayer", this.Listify<string>("@", "!", "#"), this.strIngameCommandPlayer, this.Listify<MatchArgumentFormat>(new MatchArgumentFormat("playername", new List<string>(this.dicPingPlayer.Keys))), new ExecutionRequirements(ExecutionScope.All), "Provides a player with another player's ping to the gameserver (if available)"));
        }

        private void SetupHelpCommands()
        {

        }

        #region events

        public override void OnListPlayers(List<CPlayerInfo> players, CPlayerSubset cpsSubset)
        {
            if (cpsSubset.Subset == CPlayerSubset.PlayerSubsetType.All)
            {
                lock (this.dicPlayerInfo)
                {
                    this.dicPlayerInfo.Clear();

                    foreach (CPlayerInfo player in players)
                    {
                        this.dicPlayerInfo.Add(player.SoldierName, player);
                    }
                }
            }

            this.RegisterAllCommands();
        }

        public override void OnPlayerLeft(CPlayerInfo playerInfo)
        {
            lock (this.dicPingPlayer)
            {
                if (this.dicPingPlayer.ContainsKey(playerInfo.SoldierName))
                {
                    this.dicPingPlayer.Remove(playerInfo.SoldierName);
                }
            }

            this.RegisterAllCommands();
        }

        public override void OnPunkbusterPlayerInfo(CPunkbusterInfo playerInfo)
        {
            if (!this.dicPingPlayer.ContainsKey(playerInfo.SoldierName))
            {
                string[] ipPort = playerInfo.Ip.Split(':');
                string ip = ipPort[0];
                this.dicPingPlayer.Add(playerInfo.SoldierName, new CPingPlayer(playerInfo.SoldierName, ip));
            }

            this.RegisterAllCommands();
        }

        public override void OnServerInfo(CServerInfo serverInfo)
        {
            this.csiServerInfo = serverInfo;
            if (!this.blStartupTests)
            {
                this.blStartupTests = true;

                if (this.csiServerInfo.ExternalGameIpandPort != null && this.csiServerInfo.ExternalGameIpandPort.CompareTo(String.Empty) != 0)
                {
                    String[] ipPort = this.csiServerInfo.ExternalGameIpandPort.Split(':');
                    this.strGameserverIP = ipPort[0];

                    if (this.ebDebugMessages == enumBoolYesNo.Yes)
                    {
                        this.PluginConsoleWrite("Plugin seems to be running on a Procon Layer. Assuming " + this.strGameserverIP + " is your gameserver's IP/Hostname.");
                    }
                }
                else
                {
                    this.strGameserverIP = this.strHostName;

                    if (this.ebDebugMessages == enumBoolYesNo.Yes)
                    {
                        this.PluginConsoleWrite("Plugin seems to be connected to a gameserver directly. Assuming " + this.strGameserverIP + " is your gameserver's IP/Hostname.");
                    }
                }
            }
        }

        #endregion

        #region other methods

        public void LagCheck()
        {
            if (this.strGameserverIP != null && this.strGameserverIP.CompareTo(String.Empty) != 0)
            {
                try
                {
                    Ping pGameserver = new Ping();
                    PingReply prGameserver = pGameserver.Send(this.strGameserverIP, 1000);

                    if (prGameserver.Status == IPStatus.Success)
                    {
                        if (prGameserver.RoundtripTime < this.iMaxPingGameserver)
                        {
                            this.blConnectionToGameserverOK = true;
                            if (this.ebDebugMessages == enumBoolYesNo.Yes)
                            {
                                this.PluginConsoleWrite("Ping to your gameserver (" + this.strGameserverIP + ") is " + prGameserver.RoundtripTime + "ms and thus OK!");
                            }
                        }
                        else
                        {
                            this.blConnectionToGameserverOK = false;
                            this.PluginConsoleWrite("Ping to your gameserver (" + this.strGameserverIP + ") is " + prGameserver.RoundtripTime + "ms and thus too high! Plugin will wait for connection to get better...");
                        }
                    }

                    Ping pGoogle = new Ping();
                    PingReply prGoogle = pGoogle.Send("google.com");

                    if (prGoogle.Status == IPStatus.Success)
                    {
                        if (prGoogle.RoundtripTime < this.iMaxPingGoogle)
                        {
                            this.blConnectionToGoogleOK = true;
                            if (this.ebDebugMessages == enumBoolYesNo.Yes)
                            {
                                this.PluginConsoleWrite("Ping to Google (google.com) is " + prGoogle.RoundtripTime + "ms and thus OK!");
                            }
                        }
                        else
                        {
                            this.blConnectionToGoogleOK = false;
                            this.PluginConsoleWrite("Ping to Google (google.com) is " + prGoogle.RoundtripTime + "ms and thus too high! Plugin will wait for connection to get better...");
                        }
                    }

                    if (this.blConnectionToGameserverOK && this.blConnectionToGoogleOK)
                    {
                        if (this.ebDebugMessages == enumBoolYesNo.Yes)
                        {
                            this.PluginConsoleWrite("No lag detected. Repeating lagcheck in 4 hours.");
                        }

                        this.ExecuteCommand("procon.protected.tasks.remove", "CPingPlayersLagCheck");
                        this.ExecuteCommand("procon.protected.tasks.add", "CPingPlayersLagCheck", "14400000", "14400000", "-1", "procon.protected.plugins.call", "CPingPlayers", "LagCheck");
                    }
                    else
                    {
                        if (this.ebDebugMessages == enumBoolYesNo.Yes)
                        {
                            this.PluginConsoleWrite("Lag detected. Repeating lagcheck in 15 minutes.");
                        }

                        this.ExecuteCommand("procon.protected.tasks.remove", "CPingPlayersLagCheck");
                        this.ExecuteCommand("procon.protected.tasks.add", "CPingPlayersLagCheck", "900000", "14400000", "-1", "procon.protected.plugins.call", "CPingPlayers", "LagCheck");
                    }
                }
                catch (Exception e)
                {
                    this.PluginConsoleWrite("EXCEPTION AT LagCheck: " + e.ToString());
                }
            }
            else
            {
                if (this.ebDebugMessages == enumBoolYesNo.Yes)
                {
                    this.PluginConsoleWrite("Gameserver's IP/Hostname was empty. Trying again...");
                }
                this.blStartupTests = false;
                this.ExecuteCommand("procon.protected.send", "serverInfo");
            }
        }

        public void PingAllPlayers()
        {
            if (this.blPluginEnabled && this.dicPlayerInfo.Count > 0 && this.blConnectionToGameserverOK && this.blConnectionToGoogleOK)
            {
                if (this.ebDebugMessages == enumBoolYesNo.Yes)
                {
                    this.PluginConsoleWrite("Starting to ping all players...");
                }

                foreach (KeyValuePair<string, CPingPlayer> kvp in this.dicPingPlayer)
                {
                    if (!this.lstWhiteList.Contains(kvp.Value.Name))
                    {
                        if (!kvp.Value.PingFails)
                        {
                            PingReply reply = this.PingPlayer(kvp.Value.Name);
                            if (kvp.Value.Results.Count > this.iPingStorageLength)
                            {
                                kvp.Value.Results.Clear();
                            }
                            kvp.Value.Results.Add(reply);
                            if (reply.Status != IPStatus.Success)
                            {
                                if (this.ebDebugMessages == enumBoolYesNo.Yes)
                                {
                                    this.PluginConsoleWrite("Pinging '" + kvp.Value.Name + "' fails!");
                                }
                                kvp.Value.PingFails = true;
                            }

                            if (!kvp.Value.PingFails)
                            {
                                if (this.ebDebugMessages == enumBoolYesNo.Yes)
                                {
                                    string pingline = "PING: '" + kvp.Value.Name + "' [";

                                    for (int i = 0; i < kvp.Value.Results.Count; i++)
                                    {
                                        pingline += kvp.Value.Results[i].RoundtripTime + "ms";
                                        if (i < kvp.Value.Results.Count - 1)
                                        {
                                            pingline += ";";
                                        }
                                    }

                                    pingline += "]";
                                    this.PluginConsoleWrite(pingline);
                                }

                                if (kvp.Value.GraceCounter > this.iGracePeriod - 1)
                                {
                                    this.TakeAction(kvp.Value.Name);
                                }
                                else
                                {
                                    if (reply.RoundtripTime > this.iMaxPing)
                                    {
                                        kvp.Value.GraceCounter++;

                                        lock (this.dicPlayerInfo)
                                        {
                                            if (this.ebIngameWarnings == enumBoolYesNo.Yes)
                                            {
                                                this.IngameSaySquad(kvp.Value.Name + ": Your ping is higher than " + this.iMaxPing + ". Warning " + kvp.Value.GraceCounter + "/" + this.iGracePeriod, this.dicPlayerInfo[kvp.Value.Name].TeamID, this.dicPlayerInfo[kvp.Value.Name].SquadID);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (this.ebDebugMessages == enumBoolYesNo.Yes)
                {
                    this.PluginConsoleWrite("Finished pinging all players...");
                }
            }
            else
            {
                if (!this.blConnectionToGameserverOK)
                {
                    if (this.ebDebugMessages == enumBoolYesNo.Yes)
                    {
                        this.PluginConsoleWrite("Seems like your connection to the Gameserver is lagging. Waiting for it to get better before starting actual work.");
                    }
                }
                if (!this.blConnectionToGoogleOK)
                {
                    if (this.ebDebugMessages == enumBoolYesNo.Yes)
                    {
                        this.PluginConsoleWrite("Seems like your connection to Google is lagging. Waiting for it to get better before starting actual work.");
                    }
                }
            }
        }

        private PingReply PingPlayer(string name)
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(this.dicPingPlayer[name].IP, this.iPingTimeout);
                return reply;
            }
            catch (Exception e)
            {
                this.PluginConsoleWrite("EXCEPTION AT PingPlayer: " + e.ToString());
            }

            return null;
        }

        private void TakeAction(string name)
        {
            string reason = this.strKickReason.Replace("%MAXPING%", this.iMaxPing.ToString());
            this.ExecuteCommand("procon.protected.send", "admin.kickPlayer", name, reason);
        }

        public void OnCommandOwnPing(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
        {
            if (this.dicPingPlayer.ContainsKey(strSpeaker))
            {
                if (!this.dicPingPlayer[strSpeaker].PingFails)
                {
                    PingReply reply = this.PingPlayer(this.dicPingPlayer[strSpeaker].Name);
                    if (this.dicPingPlayer[strSpeaker].Results.Count > this.iPingStorageLength)
                    {
                        this.dicPingPlayer[strSpeaker].Results.Clear();
                    }
                    this.dicPingPlayer[strSpeaker].Results.Add(reply);
                    if (reply.Status != IPStatus.Success)
                    {
                        this.dicPingPlayer[strSpeaker].PingFails = true;
                    }

                    if (!this.dicPingPlayer[strSpeaker].PingFails)
                    {
                        lock (this.dicPlayerInfo)
                        {
                            this.IngameSaySquad(strSpeaker + ": Your Ping is " + reply.RoundtripTime + "ms.", this.dicPlayerInfo[strSpeaker].TeamID, this.dicPlayerInfo[strSpeaker].SquadID);
                        }
                    }
                    else
                    {
                        lock (this.dicPlayerInfo)
                        {
                            this.IngameSaySquad(strSpeaker + ": Pinging you fails!", this.dicPlayerInfo[strSpeaker].TeamID, this.dicPlayerInfo[strSpeaker].SquadID);
                        }
                    }
                }
                else
                {
                    lock (this.dicPlayerInfo)
                    {
                        this.IngameSaySquad(strSpeaker + ": Pinging you fails!", this.dicPlayerInfo[strSpeaker].TeamID, this.dicPlayerInfo[strSpeaker].SquadID);
                    }
                }
            }
            else
            {
                if (this.lstWhiteList.Contains(strSpeaker))
                {
                    lock (this.dicPlayerInfo)
                    {
                        this.IngameSaySquad(strSpeaker + ": You're on the whitelist and thus not monitored!", this.dicPlayerInfo[strSpeaker].TeamID, this.dicPlayerInfo[strSpeaker].SquadID);
                    }
                }
                else
                {
                    lock (this.dicPlayerInfo)
                    {
                        this.IngameSaySquad(strSpeaker + ": Your information is not contained in the dictionary...", this.dicPlayerInfo[strSpeaker].TeamID, this.dicPlayerInfo[strSpeaker].SquadID);
                    }
                }
            }
        }

        public void OnCommandPingPlayer(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
        {
            if (this.dicPingPlayer.ContainsKey(capCommand.MatchedArguments[0].Argument))
            {
                if (!this.dicPingPlayer[capCommand.MatchedArguments[0].Argument].PingFails)
                {
                    PingReply reply = this.PingPlayer(this.dicPingPlayer[capCommand.MatchedArguments[0].Argument].Name);
                    if (this.dicPingPlayer[capCommand.MatchedArguments[0].Argument].Results.Count > this.iPingStorageLength)
                    {
                        this.dicPingPlayer[capCommand.MatchedArguments[0].Argument].Results.Clear();
                    }
                    this.dicPingPlayer[capCommand.MatchedArguments[0].Argument].Results.Add(reply);
                    if (reply.Status != IPStatus.Success)
                    {
                        this.dicPingPlayer[capCommand.MatchedArguments[0].Argument].PingFails = true;
                    }

                    if (!this.dicPingPlayer[capCommand.MatchedArguments[0].Argument].PingFails)
                    {
                        lock (this.dicPlayerInfo)
                        {
                            this.IngameSaySquad(strSpeaker + ": '" + capCommand.MatchedArguments[0].Argument + "'s Ping is " + reply.RoundtripTime + "ms.", this.dicPlayerInfo[strSpeaker].TeamID, this.dicPlayerInfo[strSpeaker].SquadID);
                        }
                    }
                    else
                    {
                        lock (this.dicPlayerInfo)
                        {
                            this.IngameSaySquad(strSpeaker + ": Pinging '" + capCommand.MatchedArguments[0].Argument + "' fails!", this.dicPlayerInfo[strSpeaker].TeamID, this.dicPlayerInfo[strSpeaker].SquadID);
                        }
                    }
                }
                else
                {
                    lock (this.dicPlayerInfo)
                    {
                        this.IngameSaySquad(strSpeaker + ": Pinging '" + capCommand.MatchedArguments[0].Argument + "' fails!", this.dicPlayerInfo[strSpeaker].TeamID, this.dicPlayerInfo[strSpeaker].SquadID);
                    }
                }
            }
            else
            {
                if (this.lstWhiteList.Contains(capCommand.MatchedArguments[0].Argument))
                {
                    lock (this.dicPlayerInfo)
                    {
                        this.IngameSaySquad(strSpeaker + ": '" + capCommand.MatchedArguments[0].Argument + "' is on the whitelist and thus not monitored!", this.dicPlayerInfo[strSpeaker].TeamID, this.dicPlayerInfo[strSpeaker].SquadID);
                    }
                }
                else
                {
                    lock (this.dicPlayerInfo)
                    {
                        this.IngameSaySquad(strSpeaker + ": '" + capCommand.MatchedArguments[0].Argument + "'s information is not contained in the dictionary...", this.dicPlayerInfo[strSpeaker].TeamID, this.dicPlayerInfo[strSpeaker].SquadID);
                    }
                }
            }
        }

        private void PluginConsoleWrite(string message)
        {
            string line = String.Format("^bPing Players: ^n{0}", message);
            this.ExecuteCommand("procon.protected.pluginconsole.write", line);
        }

        private void IngameSayAll(string message)
        {
            List<string> wordWrappedLines = this.WordWrap(message, 100);
            foreach (string line in wordWrappedLines)
            {
                string formattedLine = String.Format("[Ping Players] {0}", line);
                this.ExecuteCommand("procon.protected.send", "admin.say", formattedLine, "all");
            }
        }

        private void IngameSaySquad(string message, int teamid, int squadid)
        {
            List<string> wordWrappedLines = this.WordWrap(message, 100);
            foreach (string line in wordWrappedLines)
            {
                string formattedLine = String.Format("[Ping Players] {0}", line);
                this.ExecuteCommand("procon.protected.send", "admin.say", formattedLine, "squad", teamid.ToString(), squadid.ToString());
            }
        }

        #endregion

        #region own classes

        private class CPingPlayer
        {
            private string strName;
            private string strIp;
            private List<PingReply> lstResults;
            private bool blPingFails;
            private int iGraceCounter;

            public CPingPlayer(string name, string ip)
            {
                this.strName = name;
                this.strIp = ip;
                this.lstResults = new List<PingReply>();
                this.blPingFails = false;
                this.iGraceCounter = 0;
            }

            public string Name { get { return this.strName; } }
            public string IP { get { return this.strIp; } }
            public List<PingReply> Results { get { return this.lstResults; } set { this.lstResults = value; } }
            public bool PingFails { get { return this.blPingFails; } set { this.blPingFails = value; } }
            public int GraceCounter { get { return this.iGraceCounter; } set { this.iGraceCounter = value; } }

            public void AddReply(PingReply reply)
            {
                this.lstResults.Add(reply);
            }
        }

        #endregion
    
    }
}

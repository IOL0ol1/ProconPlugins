/*  Copyright 2012 Philipp Kolloczek - philipp.kolloczek@myrcon.com

    This file is part of PRoCon Frostbite.

    ServerRulesOnRequest is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ServerRulesOnRequest is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with ServerRulesOnRequest.  If not, see <http://www.gnu.org/licenses/>.

 */

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;

namespace PRoConEvents
{
    public class CServerRulesOnRequest : PRoConPluginAPI, IPRoConPluginInterface
    {
        /// <summary>
        /// Server Rules on Request: Allows players to request the server rules by typing a command
        /// in chat.
        /// </summary>

        private string m_strHostName;
        private string m_strPort;
        private string m_strPRoConVersion;

        private bool m_isPluginEnabled;

        private enumBoolYesNo m_enDoPlayerChatTab;

        private enumBoolYesNo m_enYellResponses;
        private enumBoolYesNo m_enYellResponsesRules;
        private enumBoolYesNo m_enIgnoreFirstLine;
        private enumBoolYesNo m_enListenToProconChat;

        // Welcome
        private enumBoolYesNo m_bEnableWelcome;         // Enable the welcome message.

        private int m_iDelayBeforeSendingWelcome;       // Time before welcome message is sent to player
        private int m_iWelcomeDisplayTime;              // Time that welcome message is on the screen
        private string m_strWelcomeMessage;             // Welcome message

        // Server Rules
        //string m_strRequestKeyword = @"@rules"; // command typed in chat to retrieve the server rules
        private int m_iDelayBeforeSendingRules;         // Time before responding to request for rules

        private int m_iDelayBetweenRules;               // Time between sending each rule
        private int m_iRuleDisplayTime;                 // Time rules are on the screen
        private int m_iTimeDivider;                     // used for game related correction factor
        private string m_strRulesCommandOld;
        private string m_strRulesCommand;                // Command typed in chat to request the rules
        private string m_strRulesCommandHelp;
        private string m_strChatRulesRequest;
        private string[] m_aServerRules;                // Array of rules

        // Misc
        private string m_strPreviousMessage = string.Empty;

        private string m_audioFile = string.Empty;
        private string m_strChatTabPrefix;

        private string m_strPrivatePrefix;
        private string m_strAdminsPrefix;
        private string m_strPublicPrefix;

        // Status
        private string m_strServerGameType;

        private string m_strGameMod;
        private string m_strServerVersion;

        // Privileges
        private int m_iMinPrivilegesValue;

        private int m_iMinPrivilegesValueDefault;
        private string m_strPrivilegesUser;

        private Dictionary<string, int> call_counts = new Dictionary<string, int>();

        private enumBoolYesNo m_enPluginOnLayer;
        private enumBoolYesNo m_enDoConsoleOutput;
        private enumBoolYesNo m_enDoDebugOutput;
        private enumBoolYesNo m_enDoQuietMode;

        public CServerRulesOnRequest()
        {
            this.m_enYellResponses = enumBoolYesNo.No;
            this.m_enYellResponsesRules = enumBoolYesNo.No;
            this.m_enIgnoreFirstLine = enumBoolYesNo.No;
            this.m_enListenToProconChat = enumBoolYesNo.No;

            this.m_strChatTabPrefix = "RulesOnRequest";
            this.m_strServerGameType = "none";

            this.m_enDoConsoleOutput = enumBoolYesNo.No;
            this.m_enDoDebugOutput = enumBoolYesNo.No;
            this.m_enDoQuietMode = enumBoolYesNo.No;

            this.m_enDoPlayerChatTab = enumBoolYesNo.Yes;

            this.m_strPrivatePrefix = "@";
            this.m_strAdminsPrefix = "#";
            this.m_strPublicPrefix = "!";

            this.m_iMinPrivilegesValueDefault = 8329; // Privileges not to do anything except to login to a layer, 8328 even not to login to layer, but have an existing Account
            this.m_iMinPrivilegesValue = this.m_iMinPrivilegesValueDefault;
            this.m_strPrivilegesUser = "ExampleAccount";

            // Rules
            this.m_aServerRules = new string[] { "Rules go here" };
            this.m_iDelayBetweenRules = 1;
            this.m_iRuleDisplayTime = 5000;
            this.m_iDelayBeforeSendingRules = 1;
            this.m_strRulesCommandOld = String.Empty;
            this.m_strRulesCommand = "rules";
            this.m_strRulesCommandHelp = "Provides a player with the servers rules";
            this.m_strChatRulesRequest = "!show_rules";

            // Welcome
            this.m_bEnableWelcome = enumBoolYesNo.Yes;
            this.m_strWelcomeMessage = "Welcome to our server %pn%, please type %cmd% in chat to see a list of our server rules.";
            this.m_iWelcomeDisplayTime = 30000;
            this.m_iDelayBeforeSendingWelcome = 90;
            this.m_iTimeDivider = 1;
        }

        #region Gets

        public string GetPluginName()
        {
            return "Server Rules on Request";
        }

        public string GetPluginVersion()
        {
            return "2.2.2.0";
        }

        public string GetPluginAuthor()
        {
            return @"Phil_K based on the work of [TG-9th] Lorax74";
        }

        public string GetPluginWebsite()
        {
            return "forum.myrcon.com/showthread.php?4420-Server-Rules-on-Request";
        }

        public string GetPluginDescription()
        {
            return @"<p>If you find my plugins useful, please feel free to donate.<br/>
<form action=""https://www.paypal.com/cgi-bin/webscr"" method=""post"" target=""_blank"">
<input type=""hidden"" name=""cmd"" value=""_s-xclick"">
<input type=""hidden"" name=""encrypted"" value=""-----BEGIN PKCS7-----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-----END PKCS7-----"">
<input type=""image"" src=""https://www.paypalobjects.com/de_DE/DE/i/btn/btn_donate_SM.gif"" border=""0"" name=""submit"" alt=""Jetzt einfach, schnell und sicher online bezahlen ¨C mit PayPal."">
<img alt="""" border=""0"" src=""https://www.paypalobjects.com/de_DE/i/scr/pixel.gif"" width=""1"" height=""1"">
</form>
</p>

<h2>Description</h2>
    <p>This plugin provides an in-game command which allowes the players to request the server rules. You define the text to be displayed in the plugin settings.
    The in-game command is also registered with the BasicInGameInfo plugin and can be requested by using the help command of that plugin.</p>

    <p>The option for a welcome message is 1:1 conversion. It is left in for compatibility reasons and will be droped in a later version.
    You may test if it fits your needs but I advice you to use for example the onSpawn message of my
    <a href='http://www.phogue.net/forumvb/showthread.php?1637-Admin-Announcer-amp-onSpawn-Message-(v1-3-0-0-2012-04-02)-BF3-BC2-MoH'>Admin Announcer & onSpawn Message</a> plugin.
    The method here is a bit unsecure because if a player does not spawn during the time period you set with the delay he does not see the message.</p>

<h2>Note</h2>
    <p>This plugin checks for admin status. A user is an admin if his playername can be matched against a procon account name and this account has any privilege.
    The smallest possible privilege is to be able to login to the layer.</p>

    <p>If a player is detected as admin (see above) using the rules command with the global command scope the output is send to all users.</p>

    <p>The plugin logs its actions or who triggered it for what on the event tab. If an Admin uses Procons chat tab, this is also logged and the admin will get a response
    on the chat tab showing his request was processed. That response may appear above the entered text. This is due to order the server sends events and displays chat messages.</p>

    <p>To hide your rules request on chat use the slash (/) as prefix for your commands.</p>

<h2>Commands</h2>
    <blockquote><h4>@rules</h4>
    	<ul>
    		<li>Requests the server rules for the sending player.
    	</ul>
    </blockquote>

<h2>Command Scopes</h2>
	<blockquote><h4>!</h4>(gobal) Command output will be displayed to everyone in the server. Everyone will see the rules.</blockquote>
	<blockquote><h4>@</h4>(privat) Command output will only be displayed to the account holder that issued the command. The command issuer will see the rules but no one else will.</blockquote>
	<blockquote><h4>#</h4>(group) Not in use. If it is used nothing will happen.</blockquote>

<h2>Settings</h2>
	<h3>Rules</h3>
		<blockquote><h4>Chat Command</h4>Command players type into chat to have the server rules sent back to them.<br/>
                    <b>Without</b> any scope!<br/>
                    Default is rules.</blockquote>
		<blockquote><h4>Delay before sending rules</h4>Delay in seconds before answer the request for the rules.<br/>
                    Default is 1.</blockquote>
		<blockquote><h4>Delay between rules</h4>Delay in Seconds between one rule and the next.<br/>
                    This delay occurs after the display time expires.</blockquote>
		<blockquote><h4>Show rule for how long</h4>How long the rule is displayed on the screen.</blockquote>
		<blockquote><h4>Server Rules</h3>List of rules to be sent to the player upon request.<br/>
                    One rule per line.</blockquote>
		<blockquote><h4>Ignore first line</h3>Sets to ignore the first line of rules definition while counting the rules on dedicated request.<br/>
                    Set to yes if your first line is no rule at all. It is also a good idea to prefix your rules with a number, e.g. [1] rule_text</blockquote>

	<h3>Welcome</h3>
		<blockquote><h4>Delay before sending welcome</h4>Delay before the player joins the server and the welcome is sent.<br/>
                    Please note that the join time is when the player first connects to the the server, it is not when click join.</blockquote>
		<blockquote><h4>Enable welcome message</h4>Yes enables the welcome message, no disables it.<br/>
                    Useful if you have another plugin handling your welcome message.</blockquote>
		<blockquote><h4>Message</h4>Actual message sent to player if wecome message is enabled.<br/>
                    Variables:
                    <ul>
            		    <li>%pn% = Player Name
            		    <li>%cmd% = chat command used to retrieve rules, see Chat Command in Rules above
            	    </ul>
        </blockquote>
		<blockquote><h4>Show welcome for how long</h4>Amount of time in seconds that the welcome is displayed.</blockquote>

	<h3>Xtras</h3>
		<blockquote><h4>Listen to Procon chat</h4>Enable this to have the rules send to all players while using the keyword on the chat tab.<br/>
                    This ignores the target scope and will send all rules. Keyword can be used in sentences.</blockquote>
		<blockquote><h4>GUI chat command</h4>Set the keyword to be used on Procons chat tab.<br/>
                    It is a good idea to choose it carefully not that rules are send on your normal chat.<br/>
                    Command scopes are not checked. The default (!show_rules) uses it to be specific.</blockquote>
		<blockquote><h4>Take privileges of</h4>Detects the value of privileges on the given Procon Account.<br/>
                    This value is taken as the smalles set of privileges a user has to have to send rules to all players.<br/>
                    After processing the field will be empty again.</blockquote>
		<blockquote><h4>Privileges value</h4>The value representing the minimum set of privileges. Can be entered directly, too.</blockquote>
		<blockquote><h4>Debug output</h4>Enable to have the plugin write its debug output to the plugin tab.</blockquote>
<br/><br/>
<h2>Development</h2>
    <h3>Changelog</h3>
        <blockquote><h4>2.2.2.0 (2015-03-31)</h4>
            - added Battlefield Hardline compatibility<br/>
        </blockquote>

        <blockquote><h4>2.2.0.0 (2012-06-18)</h4>
            - added option to trigger rules by using Procon chat tab<br/>
            - added keyword for chat tab trigger. Default: !show_rules<br/>
            - fixed no delay between rules using say. Yell needs to wait until display time is over.<br/>
        </blockquote>

        <blockquote><h4>2.1.0.0 (2012-05-22)</h4>
            - fixed check for admin, now an account needs the privilege to access the remote layer if nothing else is defined<br/>
            - added option to catch and use privilege value of an existing procon account<br/>
            - added plugin action output to event tab for loging who has requested the rules for whom<br/>
            - added ability to request only one rule<br/>
        </blockquote>

        <blockquote><h4>2.0.0.0 (2012-05-06)</h4>
            - nearly 1:1 conversion of RulesOnRequest by Lorax74 because the original plugin seems to be abandoned in support<br/>
            - rules command is now included in the list of commands viewable by using help provided by the BasicInGameInfo plugin<br/>
            - changed plugin structure to use the up to date plugin api<br/>
            - made yell option BF3 compatible<br/>
        </blockquote>
";
        }

        #endregion

        public void OnPluginLoadingEnv(List<string> lstPluginEnv)
        {
            Version PRoConVersion = new Version(lstPluginEnv[0]);
            this.m_strPRoConVersion = PRoConVersion.ToString();
            this.m_strServerGameType = lstPluginEnv[1].ToLower();
            this.m_strGameMod = lstPluginEnv[2];
            this.m_strServerVersion = lstPluginEnv[3];

            if (this.m_strServerGameType == "bf3" || this.m_strServerGameType == "bf4" || this.m_strServerGameType == "bfhl")
            {
                this.m_iTimeDivider = 1000;
            }
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
        {
            this.m_strHostName = strHostName;
            this.m_strPort = strPort;
            this.m_strPRoConVersion = strPRoConVersion;

            if (string.Compare(this.m_strServerGameType, "none", true) == 0)
            {
                this.RegisterEvents(this.GetType().Name, "OnVersion", "OnPlayerJoin", "OnGlobalChat", "OnTeamChat", "OnSquadChat");
            }
            else
            {
                this.RegisterEvents(this.GetType().Name, "OnPlayerJoin", "OnGlobalChat", "OnTeamChat", "OnSquadChat");
            }
        }

        public void OnPluginEnable()
        {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bServer Rules On Request ^2Enabled!");
            this.m_isPluginEnabled = true;

            if (string.Compare(this.m_strServerGameType, "none", true) == 0)
            {
                this.ExecuteCommand("procon.protected.send", "version");
            }

            this.RegisterAllCommands();
        }

        public void OnPluginDisable()
        {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bServer Rules On Request ^1Disabled.");

            this.UnregisterAllCommands();
        }

        public List<CPluginVariable> GetDisplayPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            // Response Scope
            lstReturn.Add(new CPluginVariable("Response Scope|Private Prefix", this.m_strPrivatePrefix.GetType(), this.m_strPrivatePrefix));
            //lstReturn.Add(new CPluginVariable("Response Scope|Admins Prefix", this.m_strAdminsPrefix.GetType(), this.m_strAdminsPrefix));
            lstReturn.Add(new CPluginVariable("Response Scope|Public Prefix", this.m_strPublicPrefix.GetType(), this.m_strPublicPrefix));

            // Welcome Message
            lstReturn.Add(new CPluginVariable("Welcome|Enable welcome", typeof(enumBoolYesNo), this.m_bEnableWelcome));
            lstReturn.Add(new CPluginVariable("Welcome|Message", this.m_strWelcomeMessage.GetType(), this.m_strWelcomeMessage));
            lstReturn.Add(new CPluginVariable("Welcome|Yell welcome", typeof(enumBoolYesNo), this.m_enYellResponses));
            if (this.m_enYellResponses == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Welcome|Show welcome for how long", this.m_iWelcomeDisplayTime.GetType(), this.m_iWelcomeDisplayTime / 1000));
            }
            lstReturn.Add(new CPluginVariable("Welcome|Delay before sending welcome", this.m_iDelayBeforeSendingWelcome.GetType(), this.m_iDelayBeforeSendingWelcome));

            // Rules
            lstReturn.Add(new CPluginVariable("Rules|Yell rules", typeof(enumBoolYesNo), this.m_enYellResponsesRules));
            if (this.m_enYellResponsesRules == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Rules|Show rule for how long", this.m_iRuleDisplayTime.GetType(), this.m_iRuleDisplayTime / 1000));
            }
            lstReturn.Add(new CPluginVariable("Rules|Delay before sending rules", this.m_iDelayBeforeSendingRules.GetType(), this.m_iDelayBeforeSendingRules));
            lstReturn.Add(new CPluginVariable("Rules|Delay between rules", this.m_iDelayBetweenRules.GetType(), this.m_iDelayBetweenRules));
            lstReturn.Add(new CPluginVariable("Rules|Chat Command", this.m_strRulesCommand.GetType(), this.m_strRulesCommand));
            lstReturn.Add(new CPluginVariable("Rules|Server Rules", this.m_aServerRules.GetType(), this.m_aServerRules));
            lstReturn.Add(new CPluginVariable("Rules|Ignore first line", typeof(enumBoolYesNo), this.m_enIgnoreFirstLine));

            //Misc
            lstReturn.Add(new CPluginVariable("Xtras|Listen to Procon chat", typeof(enumBoolYesNo), this.m_enListenToProconChat));
            if (this.m_enListenToProconChat == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Xtras|GUI chat command", this.m_strChatRulesRequest.GetType(), this.m_strChatRulesRequest));
            }
            lstReturn.Add(new CPluginVariable("Xtras|Take privileges of", this.m_strPrivilegesUser.GetType(), this.m_strPrivilegesUser));
            lstReturn.Add(new CPluginVariable("Xtras|Privileges value", this.m_iMinPrivilegesValue.GetType(), this.m_iMinPrivilegesValue));
            lstReturn.Add(new CPluginVariable("Xtras|Debug output", typeof(enumBoolYesNo), this.m_enDoDebugOutput));

            return lstReturn;
        }

        // Lists all of the plugin variables.
        public List<CPluginVariable> GetPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            // Response Scope
            lstReturn.Add(new CPluginVariable("Private Prefix", this.m_strPrivatePrefix.GetType(), this.m_strPrivatePrefix));
            lstReturn.Add(new CPluginVariable("Admins Prefix", this.m_strAdminsPrefix.GetType(), this.m_strAdminsPrefix));
            lstReturn.Add(new CPluginVariable("Public Prefix", this.m_strPublicPrefix.GetType(), this.m_strPublicPrefix));

            // Welcome Message
            lstReturn.Add(new CPluginVariable("Enable welcome", typeof(enumBoolYesNo), this.m_bEnableWelcome));
            lstReturn.Add(new CPluginVariable("Yell welcome", typeof(enumBoolYesNo), this.m_enYellResponses));
            lstReturn.Add(new CPluginVariable("Message", this.m_strWelcomeMessage.GetType(), this.m_strWelcomeMessage));
            lstReturn.Add(new CPluginVariable("Show welcome for how long", this.m_iWelcomeDisplayTime.GetType(), this.m_iWelcomeDisplayTime / 1000));
            lstReturn.Add(new CPluginVariable("Delay before sending welcome", this.m_iDelayBeforeSendingWelcome.GetType(), this.m_iDelayBeforeSendingWelcome));

            // Rules
            lstReturn.Add(new CPluginVariable("Delay before sending rules", this.m_iDelayBeforeSendingRules.GetType(), this.m_iDelayBeforeSendingRules));
            lstReturn.Add(new CPluginVariable("Yell rules", typeof(enumBoolYesNo), this.m_enYellResponsesRules));
            lstReturn.Add(new CPluginVariable("Show rule for how long", this.m_iRuleDisplayTime.GetType(), this.m_iRuleDisplayTime / 1000));
            lstReturn.Add(new CPluginVariable("Delay between rules", this.m_iDelayBetweenRules.GetType(), this.m_iDelayBetweenRules));
            lstReturn.Add(new CPluginVariable("Chat Command", this.m_strRulesCommand.GetType(), this.m_strRulesCommand));
            lstReturn.Add(new CPluginVariable("Server Rules", this.m_aServerRules.GetType(), this.m_aServerRules));
            lstReturn.Add(new CPluginVariable("Ignore first line", typeof(enumBoolYesNo), this.m_enIgnoreFirstLine));

            //Misc
            lstReturn.Add(new CPluginVariable("Listen to Procon chat", typeof(enumBoolYesNo), this.m_enListenToProconChat));
            lstReturn.Add(new CPluginVariable("GUI chat command", this.m_strChatRulesRequest.GetType(), this.m_strChatRulesRequest));
            // lstReturn.Add(new CPluginVariable("Take privileges of",
            // this.m_strPrivilegesUser.GetType(), this.m_strPrivilegesUser));
            lstReturn.Add(new CPluginVariable("Privileges value", this.m_iMinPrivilegesValue.GetType(), this.m_iMinPrivilegesValue));
            lstReturn.Add(new CPluginVariable("Debug output", typeof(enumBoolYesNo), this.m_enDoDebugOutput));

            return lstReturn;
        }

        public void SetPluginVariable(string strVariable, string strValue)
        {
            int iTimeSeconds = 0;
            int iTmpValue = 0;

            // Basics
            if (strVariable.CompareTo("Private Prefix") == 0)
            {
                this.m_strPrivatePrefix = strValue;
            }
            else if (strVariable.CompareTo("Admins Prefix") == 0)
            {
                this.m_strAdminsPrefix = strValue;
            }
            else if (strVariable.CompareTo("Public Prefix") == 0)
            {
                this.m_strPublicPrefix = strValue;
            }
            // Welcome
            else if (strVariable.CompareTo("Enable welcome") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_bEnableWelcome = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Message") == 0)
            {
                this.m_strWelcomeMessage = strValue;
            }
            else if (strVariable.CompareTo("Yell welcome") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enYellResponses = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Delay before sending welcome") == 0 && int.TryParse(strValue, out iTimeSeconds) == true)
            {
                this.m_iDelayBeforeSendingWelcome = iTimeSeconds;
            }
            else if (strVariable.CompareTo("Show welcome for how long") == 0 && int.TryParse(strValue, out iTimeSeconds) == true)
            {
                this.m_iWelcomeDisplayTime = iTimeSeconds * 1000;
            }

            // Rules
            else if (strVariable.CompareTo("Yell rules") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enYellResponsesRules = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Delay before sending rules") == 0 && int.TryParse(strValue, out iTimeSeconds) == true)
            {
                this.m_iDelayBeforeSendingRules = iTimeSeconds;
            }
            else if (strVariable.CompareTo("Show rule for how long") == 0 && int.TryParse(strValue, out iTimeSeconds) == true)
            {
                this.m_iRuleDisplayTime = iTimeSeconds * 1000;
            }
            else if (strVariable.CompareTo("Delay between rules") == 0 && int.TryParse(strValue, out iTimeSeconds) == true)
            {
                this.m_iDelayBetweenRules = iTimeSeconds;
            }
            else if (strVariable.CompareTo("Chat Command") == 0)
            {
                this.m_strRulesCommandOld = this.m_strRulesCommand;
                this.m_strRulesCommand = strValue;
            }
            else if (strVariable.CompareTo("Server Rules") == 0)
            {
                this.m_aServerRules = CPluginVariable.DecodeStringArray(strValue);
                int iloop = 0;
                foreach (string rule in this.m_aServerRules)
                {
                    if (rule.ToLower().IndexOf(this.m_strChatRulesRequest.ToLower()) >= 0)
                    {
                        this.m_aServerRules[iloop] = Regex.Replace(rule, this.m_strChatRulesRequest, "", RegexOptions.IgnoreCase);
                    }
                    iloop++;
                }
            }
            else if (strVariable.CompareTo("Ignore first line") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enIgnoreFirstLine = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            // Misc
            else if (strVariable.CompareTo("Default Sound") == 0)
            {
                this.m_audioFile = strValue;
            }
            else if (strVariable.CompareTo("Listen to Procon chat") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enListenToProconChat = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("GUI chat command") == 0)
            {
                this.m_strChatRulesRequest = strValue;
                int iloop = 0;
                foreach (string rule in this.m_aServerRules)
                {
                    if (rule.ToLower().IndexOf(this.m_strChatRulesRequest.ToLower()) >= 0)
                    {
                        this.m_aServerRules[iloop] = Regex.Replace(rule, this.m_strChatRulesRequest, "", RegexOptions.IgnoreCase);
                    }
                    iloop++;
                }
            }
            else if (strVariable.CompareTo("Privileges value") == 0 && int.TryParse(strValue, out iTmpValue) == true)
            {
                this.m_iMinPrivilegesValue = iTmpValue;
            }
            else if (strVariable.CompareTo("Take privileges of") == 0)
            {
                if (strValue.CompareTo("ExampleAccount") != 0)
                {
                    this.WritePluginConsole(String.Format("Checking privileges on {0}", strValue), 2);
                    CPrivileges cpAccount = this.GetAccountPrivileges(strValue);
                    if (cpAccount != null)
                    {
                        this.WritePluginConsole(String.Format("{0} has privilege value {1}", strValue, cpAccount.PrivilegesFlags.ToString()), 2);
                        this.m_iMinPrivilegesValue = (int)cpAccount.PrivilegesFlags;
                    }
                    else
                    {
                        this.WritePluginConsole(String.Format("There is no account {0}. Using default value.", strValue), 2);
                        this.m_iMinPrivilegesValue = this.m_iMinPrivilegesValueDefault;
                    }
                }
            }
            else if (strVariable.CompareTo("Debug output") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_enDoDebugOutput = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (this.m_enDoDebugOutput == enumBoolYesNo.No && this.m_enDoQuietMode == enumBoolYesNo.Yes)
                {
                    this.m_enDoConsoleOutput = enumBoolYesNo.Yes;
                }
            }

            this.UnregisterAllCommands();
            this.RegisterAllCommands();
        }

        #region (un)Register own commands

        private void UnregisterAllCommands()
        {
            List<string> emptyList = new List<string>();

            this.UnregisterCommand(new MatchCommand("CServerRulesOnRequest",
                                                    "OnCommandRules",
                                                    emptyList,
                                                    this.m_strRulesCommand,
                                                    this.Listify<MatchArgumentFormat>(),
                                                    new ExecutionRequirements(ExecutionScope.All),
                                                    this.m_strRulesCommandHelp
                                                    ));

            this.UnregisterCommand(new MatchCommand("CServerRulesOnRequest",
                                                    "OnCommandRules",
                                                    emptyList,
                                                    this.m_strRulesCommandOld,
                                                    this.Listify<MatchArgumentFormat>(
                                                        new MatchArgumentFormat("optional: Nr of Rule", emptyList)
                                                    ),
                                                    new ExecutionRequirements(ExecutionScope.All),
                                                    this.m_strRulesCommandHelp
                                                    ));
        }

        private void RegisterAllCommands()
        {
            if (this.m_isPluginEnabled == true)
            {
                List<string> scopes = this.Listify<string>(this.m_strPrivatePrefix, this.m_strPublicPrefix);
                List<string> emptyList = new List<string>();

                this.RegisterCommand(new MatchCommand("CServerRulesOnRequest",
                                                      "OnCommandRules",
                                                      scopes,
                                                      this.m_strRulesCommand,
                                                      this.Listify<MatchArgumentFormat>(
                                                          new MatchArgumentFormat("optional: RuleNr", emptyList)
                                                      ),
                                                      new ExecutionRequirements(ExecutionScope.All),
                                                      this.m_strRulesCommandHelp
                                                      ));
            }
        }

        #endregion

        #region command events

        public void OnCommandRules(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
        {
            string taskName;
            int iLineAdjust = 0; if (this.m_enIgnoreFirstLine == enumBoolYesNo.Yes) { iLineAdjust = 1; }
            int iRuleNr = 0;
            int iTmp = 0;
            int iDelay = this.m_iDelayBeforeSendingRules;
            bool blIsAdmin = false;
            CPrivileges cpAccount = this.GetAccountPrivileges(strSpeaker);

            this.WritePluginConsole(strSpeaker + " requested the rules. Checking privileges...", 1);

            // 8328 => has account with no privileges in procon
            if (cpAccount != null && cpAccount.PrivilegesFlags >= this.m_iMinPrivilegesValue) { blIsAdmin = true; }
            this.WritePluginConsole(strSpeaker + " (isAdmin: " + blIsAdmin.ToString() + " - " + (cpAccount != null ? cpAccount.PrivilegesFlags.ToString() : "notExistentAccount") + ") checked...", 1);

            this.WritePluginConsole("... checking for dedicated rule request ...", 1);
            if (capCommand.ExtraArguments.Length != 0)
            {
                this.WritePluginConsole(String.Format("... checking additinal parameter ( {0} ) ...", capCommand.ExtraArguments), 1);
                if (int.TryParse(capCommand.ExtraArguments, out iTmp) == true && iTmp <= this.m_aServerRules.Length - iLineAdjust && iTmp > 0)
                {
                    iRuleNr = int.Parse(capCommand.ExtraArguments);
                    this.WritePluginConsole("... rule number " + iRuleNr.ToString() + " was requested.", 1);
                }
                else
                {
                    this.WritePluginConsole("... could not be parsed / not available...", 1);
                }
            }

            if (blIsAdmin == true && string.Compare(capCommand.ResposeScope, this.m_strPublicPrefix, true) == 0)
            {
                // Admin wants rules to be send to all
                this.SendSayResponse(this.m_strPrivatePrefix, strSpeaker, "At your service. Sending rules to everyone.");
                this.WriteEventTab("reguested the rules to be shown public.", strSpeaker, 2);

                if (iRuleNr == 0)
                {
                    // all rules
                    foreach (string rule in this.m_aServerRules)
                    {
                        taskName = string.Format("ServerRulesOnRequest-{0}", strSpeaker);
                        if (this.m_enYellResponsesRules == enumBoolYesNo.Yes)
                        {
                            this.ExecuteCommand("procon.protected.tasks.add", "CServerRulesOnRequest", iDelay.ToString(), "1", "1", "procon.protected.send", "admin.yell", rule, (this.m_iRuleDisplayTime / this.m_iTimeDivider).ToString(), "all");
                            iDelay += (this.m_iRuleDisplayTime / 1000) + this.m_iDelayBetweenRules;
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.tasks.add", "CServerRulesOnRequest", iDelay.ToString(), "1", "1", "procon.protected.send", "admin.say", rule, "all");
                            iDelay += this.m_iDelayBetweenRules;
                        }
                    }
                }
                else
                {
                    // dedicated rule requested
                    string rule = this.m_aServerRules[iRuleNr - 1 + iLineAdjust];
                    taskName = string.Format("ServerRulesOnRequest-{0}", strSpeaker);
                    if (this.m_enYellResponsesRules == enumBoolYesNo.Yes)
                    {
                        this.ExecuteCommand("procon.protected.tasks.add", "CServerRulesOnRequest", iDelay.ToString(), "1", "1", "procon.protected.send", "admin.yell", rule, (this.m_iRuleDisplayTime / this.m_iTimeDivider).ToString(), "all");
                        iDelay += (this.m_iRuleDisplayTime / 1000) + this.m_iDelayBetweenRules;
                    }
                    else
                    {
                        this.ExecuteCommand("procon.protected.tasks.add", "CServerRulesOnRequest", iDelay.ToString(), "1", "1", "procon.protected.send", "admin.say", rule, "all");
                        iDelay += this.m_iDelayBetweenRules;
                    }
                }
            }
            else
            {
                // player is no admin or admin will see rules privately
                this.WriteEventTab(String.Format("{0} reguested the rules shown to himself", strSpeaker), blIsAdmin == true ? strSpeaker : String.Empty, 2);

                if (iRuleNr == 0)
                {
                    // all rules
                    foreach (string rule in this.m_aServerRules)
                    {
                        taskName = string.Format("ServerRulesOnRequest-{0}", strSpeaker);
                        if (this.m_enYellResponsesRules == enumBoolYesNo.Yes)
                        {
                            this.ExecuteCommand("procon.protected.tasks.add", "CServerRulesOnRequest", iDelay.ToString(), "1", "1", "procon.protected.send", "admin.yell", rule, (this.m_iRuleDisplayTime / this.m_iTimeDivider).ToString(), "player", strSpeaker);
                            iDelay += (this.m_iRuleDisplayTime / 1000) + this.m_iDelayBetweenRules;
                        }
                        else
                        {
                            this.ExecuteCommand("procon.protected.tasks.add", "CServerRulesOnRequest", iDelay.ToString(), "1", "1", "procon.protected.send", "admin.say", rule, "player", strSpeaker);
                            iDelay += this.m_iDelayBetweenRules; ;
                        }
                    } // end foreach
                }
                else
                {
                    // dedicated rule requested
                    string rule = this.m_aServerRules[iRuleNr - 1 + iLineAdjust];
                    taskName = string.Format("ServerRulesOnRequest-{0}", strSpeaker);
                    if (this.m_enYellResponsesRules == enumBoolYesNo.Yes)
                    {
                        this.ExecuteCommand("procon.protected.tasks.add", "CServerRulesOnRequest", iDelay.ToString(), "1", "1", "procon.protected.send", "admin.yell", rule, (this.m_iRuleDisplayTime / this.m_iTimeDivider).ToString(), "all");
                        iDelay += (this.m_iRuleDisplayTime / 1000) + this.m_iDelayBetweenRules;
                    }
                    else
                    {
                        this.ExecuteCommand("procon.protected.tasks.add", "CServerRulesOnRequest", iDelay.ToString(), "1", "1", "procon.protected.send", "admin.say", rule, "all");
                        iDelay += this.m_iDelayBetweenRules; ;
                    }
                }
            }
        }

        // capCommand.ResposeScope, strSpeaker, strText

        #endregion

        #region Plugin events

        public override void OnVersion(string serverType, string version)
        {
            this.m_strServerGameType = serverType.ToLower();

            if (this.m_strServerGameType == "bf3" || this.m_strServerGameType == "bf4" || this.m_strServerGameType == "bfhl")
            {
                this.m_iTimeDivider = 1000;
            }
        }

        // Chat events
        public override void OnGlobalChat(string speaker, string message)
        {
            if (this.m_enListenToProconChat == enumBoolYesNo.Yes)
            {
                if (String.Compare(speaker, "server", true) == 0)
                {
                    this.WritePluginConsole(String.Format("{0} send >> {1} <<", speaker, message), 1);
                    if (message.ToLower().IndexOf(this.m_strChatRulesRequest.ToLower()) >= 0)
                    {
                        this.WritePluginConsole(String.Format("{0} requested rules with >> {1} <<", speaker, this.m_strChatRulesRequest), 1);
                        int iDelay = this.m_iDelayBeforeSendingRules;
                        string strSpeaker = "ProconChat";
                        string taskName = String.Empty;
                        this.WriteEventTab("Admin reguested the rules to be shown public.", strSpeaker, 2);
                        this.WriteChatTab("Your rule request has been processed.", this.m_enDoPlayerChatTab);
                        foreach (string rule in this.m_aServerRules)
                        {
                            taskName = string.Format("ServerRulesOnRequest-{0}", strSpeaker);
                            if (this.m_enYellResponsesRules == enumBoolYesNo.Yes)
                            {
                                this.ExecuteCommand("procon.protected.tasks.add", "CServerRulesOnRequest", iDelay.ToString(), "1", "1", "procon.protected.send", "admin.yell", rule, (this.m_iRuleDisplayTime / this.m_iTimeDivider).ToString(), "all");
                                iDelay += (this.m_iRuleDisplayTime / 1000) + this.m_iDelayBetweenRules;
                            }
                            else
                            {
                                this.ExecuteCommand("procon.protected.tasks.add", "CServerRulesOnRequest", iDelay.ToString(), "1", "1", "procon.protected.send", "admin.say", rule, "all");
                                iDelay += this.m_iDelayBetweenRules;
                            }
                        }
                    }
                }
            }
        }

        public override void OnTeamChat(string speaker, string message, int teamId)
        {
            this.OnGlobalChat(speaker, message);
        }

        public override void OnSquadChat(string speaker, string message, int teamId, int squadId)
        {
            this.OnGlobalChat(speaker, message);
        }

        // Player events
        public override void OnPlayerJoin(string strSoldierName)
        {
            if (this.m_bEnableWelcome == enumBoolYesNo.Yes)
            {
                string taskName = string.Format("ServerRulesOnRequest-{0}", strSoldierName); ;
                string Welcome = Welcome = this.m_strWelcomeMessage.Replace("%pn%", strSoldierName).Replace("%cmd%", this.m_strRulesCommand);
                if (this.m_enYellResponses == enumBoolYesNo.Yes)
                {
                    this.ExecuteCommand("procon.protected.tasks.add", taskName, this.m_iDelayBeforeSendingWelcome.ToString(), "1", "1", "procon.protected.send", "admin.yell", Welcome, (this.m_iWelcomeDisplayTime / this.m_iTimeDivider).ToString(), "player", strSoldierName);
                }
                else
                {
                    this.ExecuteCommand("procon.protected.tasks.add", taskName, this.m_iDelayBeforeSendingWelcome.ToString(), "1", "1", "procon.protected.send", "admin.say", Welcome, "player", strSoldierName);
                }
            }
        }

        #endregion

        #region ownFunctions

        private void WriteChatTab(string message, enumBoolYesNo DoWrite)
        {
            string strTmpMsg = "^4" + this.m_strChatTabPrefix + "^0 - " + message + "^0";
            if (DoWrite == enumBoolYesNo.Yes)
            {
                this.ExecuteCommand("procon.protected.chat.write", strTmpMsg);
            }
        }

        private void WritePluginConsole(string message, int debug)
        {
            string line = String.Format("Server Rules On Request: {0}", message);
            if (this.m_enDoDebugOutput == enumBoolYesNo.Yes || (this.m_enDoConsoleOutput == enumBoolYesNo.Yes && debug == 0) || debug == 2)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", line);
            }
        }

        // this.ExecuteCommand("procon.protected.events.write", "Plugins", "PluginAction", message, strSpeaker);
        private void WriteEventTab(string message, string strSpeaker, int debug)
        {
            string line = message; //String.Format("Sror: {0}", message);
            if (this.m_enDoDebugOutput == enumBoolYesNo.Yes || (this.m_enDoConsoleOutput == enumBoolYesNo.Yes && debug == 0) || debug == 2)
            {
                this.ExecuteCommand("procon.protected.events.write", "Plugins", "PluginAction", line, strSpeaker);
            }
        }

        private void SendSayResponse(string strScope, string strAccountName, string strMessage)
        {
            if (String.Compare(strScope, this.m_strPrivatePrefix) == 0)
            {
                this.ExecuteCommand("procon.protected.send", "admin.say", strMessage, "player", strAccountName);
            }
            else if (String.Compare(strScope, this.m_strPublicPrefix) == 0)
            {
                CPrivileges cpAccount = null;
                cpAccount = this.GetAccountPrivileges(strAccountName);
                if (cpAccount != null && cpAccount.PrivilegesFlags > 0)
                {
                    this.ExecuteCommand("procon.protected.send", "admin.say", strMessage, "all");
                }
            }
        }

        private void SendYellResponse(string strScope, string strAccountName, string strMessage)
        {
            if (String.Compare(strScope, this.m_strPrivatePrefix) == 0)
            {
                this.ExecuteCommand("procon.protected.send", "admin.yell", strMessage, (this.m_iRuleDisplayTime / this.m_iTimeDivider).ToString(), "player", strAccountName);
            }
            else if (String.Compare(strScope, this.m_strPublicPrefix) == 0)
            {
                CPrivileges cpAccount = null;
                cpAccount = this.GetAccountPrivileges(strAccountName);
                if (cpAccount != null && cpAccount.PrivilegesFlags > 0)
                {
                    this.ExecuteCommand("procon.protected.send", "admin.yell", strMessage, (this.m_iRuleDisplayTime / this.m_iTimeDivider).ToString(), "all");
                }
            }
        }

        #endregion
    }
}
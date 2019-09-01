using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Net;
using System.Web;

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;

namespace PRoConEvents
{
  public class CVoteBan : PRoConPluginAPI, IPRoConPluginInterface
  {
    private enumBoolYesNo enableVoteBan;
    private int voteBanThreshold;
    private int startVoteNumber;
    private int votePercentageRequired;
    private int voteDuration;
    private int voteProgressNumber;
    private string banType;
    private string banDuration;
    private int banLength;
    private string banDisplayReason;

    private enumBoolYesNo enableVoteKick;
    private int voteKickThreshold;
    private int startKickVoteNumber;
    private int voteKickPercentageRequired;
    private int voteKickDuration;
    private int voteKickProgressNumber;

    private enumBoolYesNo enableHackCry;
    private int hackCriesNeeded;
    private string hackCryResponse;
    private List<string> additionalTriggers;

    private List<string> privilegedUsers;
    private List<string> privilegedTags;
    private string whitelistActionTaken;
    private int whitelistBanLength;

    private List<string> banCommand;
    private List<string> kickCommand;
    private List<string> yesCommand;
    private List<string> noCommand;
    private List<string> cancelVoteCommand;

    private int maxVoters;
    private int yesVotesNeeded;
    private int yesVotes;
    private int noVotes;
    private int hackCryCount;

    private string resetMessages;
    private List<string> inGameMessages;

    private string votedVictim;
    private string votedVictimGUID;
    private string votedVictimIP;
    private string votedVictimPbGuid;
    private string currentSpeaker;
    private string currentVotedPlayer;
    private string currentVoteType;
    private string currentVoteReason;
    private string voteType;
    private string voteReason;
    private string currentVoteTypeForThreshold;
    private string currentSpeakerForThreshold;
    private string currentMessageForThreshold;
    private CPrivileges currentPrivileges;
    private List<string> alreadyVoted;
    private List<string> playerBeingVotedReason;
    private List<string> playerBeingVoted;
    private List<string> playerBeingKickVoted;
    private List<int> playerBeingVotedCount;
    private List<int> playerBeingKickVotedCount;
    private Dictionary<string, string> voteeVoters;
    private Dictionary<string, string> voteeVotersForKick;
    private List<string> suggestedPlayerName;
    private List<string> suggestedPlayerNameForKick;
    private List<string> awaitedConfirmationPlayer;
    private List<string> awaitedConfirmationPlayerForKick;
    private List<string> awaitedConfirmationPlayerReason;

    private bool voteIsInProgress;
    private bool needPlayerCount;
    private bool needPlayerCountForThreshold;
    private bool needVotedVictimInfo;
    private bool banningPlayerByGUID;
    private bool banningPlayerByIP;
    private bool banningPlayerByName;
    private bool banningPlayerByPbGuid;
    private bool kickingPlayer;
    private bool processingVote;
    private bool foundvotedVictim;
    private bool pluginEnabled;

    private System.Timers.Timer voteInProgress;
    private System.Timers.Timer voteProgressDisplay;

    private BattlelogClient blClient;

    public CVoteBan()
    {
      this.enableVoteBan = enumBoolYesNo.No;
      this.voteBanThreshold = 0;
      this.startVoteNumber = 3;
      this.votePercentageRequired = 40;
      this.voteDuration = 3;
      this.voteProgressNumber = 15;
      this.banType = "GUID";
      this.banDuration = "Permanent";
      this.banLength = 1440;
      this.banDisplayReason = "Banned by player Vote Ban - %player% (Reason: %reason%)";

      this.enableVoteKick = enumBoolYesNo.No;
      this.voteKickThreshold = 0;
      this.startKickVoteNumber = 3;
      this.voteKickPercentageRequired = 40;
      this.voteKickDuration = 3;
      this.voteKickProgressNumber = 15;

      this.enableHackCry = enumBoolYesNo.No;
      this.hackCriesNeeded = 3;
      this.hackCryResponse = "Is there a hacker on? Type %vbcommand% <player_name> to initiate a Vote Ban!";
      this.additionalTriggers = new List<string>();
      this.additionalTriggers.Add("aimbot");

      this.privilegedUsers = new List<string>();
      this.privilegedTags = new List<string>();
      this.whitelistActionTaken = "None";
      this.whitelistBanLength = 60;

      this.banCommand = new List<string>();
      this.kickCommand = new List<string>();
      this.yesCommand = new List<string>();
      this.noCommand = new List<string>();
      this.cancelVoteCommand = new List<string>();
      this.banCommand.Add("!voteban");
      this.banCommand.Add("@voteban");
      this.kickCommand.Add("!votekick");
      this.kickCommand.Add("@votekick");
      this.yesCommand.Add("!yes");
      this.yesCommand.Add("@yes");
      this.noCommand.Add("!no");
      this.noCommand.Add("@no");
      this.cancelVoteCommand.Add("!cancelvote");
      this.cancelVoteCommand.Add("@cancelvote");

      this.maxVoters = 64;
      this.yesVotes = 0;
      this.noVotes = 0;
      this.hackCryCount = 0;

      this.resetMessages = "...";
      this.inGameMessages = new List<string>();
      this.initInGameMessages();

      this.alreadyVoted = new List<string>();
      this.playerBeingVotedReason = new List<string>();
      this.playerBeingVoted = new List<string>();
      this.playerBeingKickVoted = new List<string>();
      this.playerBeingVotedCount = new List<int>();
      this.playerBeingKickVotedCount = new List<int>();
      this.voteeVoters = new Dictionary<string, string>();
      this.voteeVotersForKick = new Dictionary<string, string>();
      this.suggestedPlayerName = new List<string>();
      this.suggestedPlayerNameForKick = new List<string>();
      this.awaitedConfirmationPlayer = new List<string>();
      this.awaitedConfirmationPlayerForKick = new List<string>();
      this.awaitedConfirmationPlayerReason = new List<string>();

      this.voteIsInProgress = false;
      this.needPlayerCount = false;
      this.needPlayerCountForThreshold = false;
      this.needVotedVictimInfo = false;
      this.banningPlayerByGUID = false;
      this.banningPlayerByIP = false;
      this.banningPlayerByName = false;
      this.banningPlayerByPbGuid = false;
      this.kickingPlayer = false;
      this.processingVote = false;
      this.foundvotedVictim = false;
      this.pluginEnabled = false;
    }

    public string GetPluginName()
    {
      return "Vote Ban";
    }

    public string GetPluginVersion()
    {
      return "2.0.1.0";
    }

    public string GetPluginAuthor()
    {
      return "TimSad";
    }

    public string GetPluginWebsite()
    {
      return "www.phogue.net/forumvb/showthread.php?3582";
    }

    public string GetPluginDescription()
    {
      return @"
        <h2>Description</h2>
          <p>This plugin allows players to start a vote to ban or kick another player on the server.  This is particularly useful to work against all the hackers we have seen so much of lately.</p>
        <h2>In-Game Commands</h2>
          <blockquote><h4>!voteban &lt;player_name&gt;</h4>Puts in a request to initiate a Vote Ban on the specified player.</blockquote>
          <blockquote><h4>!votekick &lt;player_name&gt;</h4>Puts in a request to initiate a Vote Kick on the specified player.</blockquote>
          <blockquote><h4>!yes</h4>Votes YES to ban/kick the player who has a Vote Ban/Kick in progress on them.<br \>Also, agrees to the suggested name after misspelling a name when trying to Vote Ban or Vote Kick.</blockquote>
          <blockquote><h4>!no</h4>Votes NO to ban/kick the player who has a Vote Ban/Kick in progress on them.<br \>Also, disagrees to the suggested name after misspelling a name when trying to Vote Ban or Vote Kick.</blockquote>
          <blockquote><h4>!cancelvote</h4>Cancels the current vote in progress.  This command is only available to players who have an account created and are able to connect to the Procon Layer.</blockquote>
          <p><b>NOTE:</b> These commands may be redefined by you in the plugin settings.</p>
        <h2>Settings</h2>
          <h3>Vote Ban</h3>
            <blockquote><h4>Enable Vote Ban?</h4>Allows you to enable or disable the ability for players to Vote Ban.</blockquote>
            <blockquote><h4>Vote Ban Player Count Threshold</h4>The minimum number of players that must be on the server for Vote Banning to be enabled.</blockquote>
            <blockquote><h4>Start Vote Ban Number</h4>The number of <b>!voteban</b> requests needed to initiate a Vote Ban on the specified player.</blockquote>
            <blockquote><h4>Vote Ban Pass Percentage</h4>The percentage of YES votes of the total players needed for a Vote Ban to pass.</blockquote>
            <blockquote><h4>Vote Ban Duration (in minutes)</h4>How long Vote Bans last before they are ended.</blockquote>
            <blockquote><h4>Vote Ban Progress Display Interval (in seconds)</h4>The recurring number of seconds that the progress of the current Vote Ban is displayed in chat.</blockquote>
            <blockquote><h4>Ban Type</h4>The type of ban (GUID, Name, IP, or PB GUID) that is issued upon a successful Vote Ban.</blockquote>
            <blockquote><h4>Ban Duration</h4>How long bans last upon a successful Vote Ban.</blockquote>
            <blockquote><h4>Ban Length (in minutes)</h4>If <b>Ban Duration</b> is set to Temporary, bans last for this length of time before they expire.</blockquote>
            <blockquote><h4>Ban Reason Message</h4>Set this to whatever you would like the ban reason to be upon a successful Vote Ban. (Use <b>%player%</b> for the banned player and <b>%reason%</b> for the reason the players Vote Banned the player.)</blockquote>
          <h3>Vote Kick</h3>
            <blockquote><h4>Enable Vote Kick?</h4>Allows you to enable or disable the ability for players to Vote Kick.</blockquote>
            <blockquote><h4>Vote Kick Player Count Threshold</h4>The minimum number of players that must be on the server for Vote Kicking to be enabled.</blockquote>
            <blockquote><h4>Start Vote Kick Number</h4>The number of <b>!votekick</b> requests needed to initiate a Vote Kick on the specified player.</blockquote>
            <blockquote><h4>Vote Kick Pass Percentage</h4>The percentage of YES votes of the total players needed for a Vote Kick to pass.</blockquote>
            <blockquote><h4>Vote Kick Duration (in minutes)</h4>How long Vote Kicks last before they are ended.</blockquote>
            <blockquote><h4>Vote Kick Progress Display Interval (in seconds)</h4>The recurring number of seconds that the progress of the current Vote Kick is displayed in chat.</blockquote>
          <h3>Hack Cry Responder</h3>
            <blockquote><h4>Enable Hack Cry Responder?</h4>Allows you to enable or disable the Hack Cry Responder.</blockquote>
            <blockquote><h4>Hack Cry Trigger Number</h4>The number of times that the word ""hack"" needs to be said in chat, recurrently, to trigger the responder.</blockquote>
            <blockquote><h4>Hack Cry Trigger Response</h4>The server message response sent when the responder is triggered. (Use <b>%vbcommand%</b> and <b>%vkcommand%</b> for your currently set Vote Ban and Vote Kick commands.)</blockquote>
            <blockquote><h4>Additional Triggers</h4>Any additional words in chat that you would like to trigger the responder.</blockquote>
          <h3>Whitelist</h3>
            <p>This whitelist guards admins as well as additional players of your choice from being Vote Banned/Kicked. It recognizes players as admins if they have an account created and are able to connect to the Procon Layer.</p>
            <blockquote><h4>In-Game Names</h4>Allows you to add additional players to the whitelist.</blockquote>
            <blockquote><h4>Clan Tags</h4>Allows you to add Clan Tags to protect the wearer of any tag from being Vote Banned/Kicked.</blockquote>
            <blockquote><h4>Action Taken</h4>The action taken (None, Kill, Kick, Temporarily Ban, Permanently Ban) against a player that tries to Vote Ban/Kick a player in the whitelist.</blockquote>
            <blockquote><h4>Temporary Ban Length (in minutes)</h4>If <b>Action Taken</b> is set to Temporarily Ban, bans last for this length of time before they expire.</blockquote><br />
          <h3>In-Game Commands</h3>
            <p>These allow you to customize the in-game commands for this plugin.</p>
            <blockquote><h4>Vote Ban Commands</h4>The commands used to initiate a Vote Ban.</blockquote>
            <blockquote><h4>Vote Kick Commands</h4>The commands used to initiate a Vote Kick.</blockquote>
            <blockquote><h4>Yes Commands</h4>The commands used to vote yes to the vote in progress. (Also used to confirm a suggested player name.)</blockquote>
            <blockquote><h4>No Commands</h4>The commands used to vote no to the vote in progress. (Also used to deny a suggested player name.)</blockquote>
            <blockquote><h4>Cancel Vote Commands</h4>The commands used to cancel the vote in progress.</blockquote>
          <h3>In-Game Messages</h3>
            <p>This allows you to customize all the messages that this plugin sends to the server.</p>
            <blockquote><h4>Reset Messages?</h4>Use this to retrieve the default list of messages. This is useful when you may have made a mistake and messages start to not show up.</blockquote>
            <blockquote><h4>Message List</h4>The list containing all the messages that this plugin sends to the server. Be careful not to delete any lines otherwise things will go wrong!</blockquote>
        ";
    }

    public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
    {
      this.RegisterEvents(this.GetType().Name, 
        "OnGlobalChat",
        "OnTeamChat",
        "OnSquadChat",
        "OnListPlayers",
        "OnRoundOver",
        "OnPunkbusterPlayerInfo",
        "OnPunkbusterEndPlayerInfo",
        "OnPlayerLeft",
        "OnLevelLoaded"
      );
    }

    public void OnPluginEnable()
    {
      this.pluginEnabled = true;
      this.ExecuteCommand("procon.protected.pluginconsole.write", "^bVote Ban ^2Enabled!");
    }

    public void OnPluginDisable()
    {
      this.pluginEnabled = false;
      // If a vote is in progress, cancel it
      if (voteIsInProgress)
      {
        cancelVote("PLUGIN-Disabled");
      }
      OnRoundOver(0); // Reset everything, so on next enable make everything like the class was just constructed
      this.ExecuteCommand("procon.protected.pluginconsole.write", "^bVote Ban ^1Disabled =(");
    }

    public List<CPluginVariable> GetDisplayPluginVariables()
    {
      List<CPluginVariable> lstReturn = new List<CPluginVariable>();

      lstReturn.Add(new CPluginVariable("Vote Ban|Enable Vote Ban?", typeof(enumBoolYesNo), enableVoteBan));
      if (enableVoteBan == enumBoolYesNo.Yes)
      {
        lstReturn.Add(new CPluginVariable("Vote Ban|Vote Ban Player Count Threshold", voteBanThreshold.GetType(), voteBanThreshold));
        lstReturn.Add(new CPluginVariable("Vote Ban|Start Vote Ban Number", startVoteNumber.GetType(), startVoteNumber));
        lstReturn.Add(new CPluginVariable("Vote Ban|Vote Ban Pass Percentage", votePercentageRequired.GetType(), votePercentageRequired));
        lstReturn.Add(new CPluginVariable("Vote Ban|Vote Ban Duration (in minutes)", voteDuration.GetType(), voteDuration));
        lstReturn.Add(new CPluginVariable("Vote Ban|Vote Ban Progress Display Interval (in seconds)", voteProgressNumber.GetType(), voteProgressNumber));
        lstReturn.Add(new CPluginVariable("Vote Ban|Ban Type", "enum.BanType(GUID|IP|Name|PB GUID)", banType));
        lstReturn.Add(new CPluginVariable("Vote Ban|Ban Duration", "enum.BanDuration(Permanent|Temporary)", banDuration));
        if (banDuration == "Temporary")
          lstReturn.Add(new CPluginVariable("Vote Ban|Ban Length (in minutes)", banLength.GetType(), banLength));
        lstReturn.Add(new CPluginVariable("Vote Ban|Ban Reason Message", banDisplayReason.GetType(), banDisplayReason));
      }

      lstReturn.Add(new CPluginVariable("Vote Kick|Enable Vote Kick?", typeof(enumBoolYesNo), enableVoteKick));
      if (enableVoteKick == enumBoolYesNo.Yes)
      {
        lstReturn.Add(new CPluginVariable("Vote Kick|Vote Kick Player Count Threshold", voteKickThreshold.GetType(), voteKickThreshold));
        lstReturn.Add(new CPluginVariable("Vote Kick|Start Vote Kick Number", startKickVoteNumber.GetType(), startKickVoteNumber));
        lstReturn.Add(new CPluginVariable("Vote Kick|Vote Kick Pass Percentage", voteKickPercentageRequired.GetType(), voteKickPercentageRequired));
        lstReturn.Add(new CPluginVariable("Vote Kick|Vote Kick Duration (in minutes)", voteKickDuration.GetType(), voteKickDuration));
        lstReturn.Add(new CPluginVariable("Vote Kick|Vote Kick Progress Display Interval (in seconds)", voteKickProgressNumber.GetType(), voteKickProgressNumber));
      }

      lstReturn.Add(new CPluginVariable("Hack Cry Responder|Enable Hack Cry Responder?", typeof(enumBoolYesNo), enableHackCry));
      if (enableHackCry == enumBoolYesNo.Yes)
      {
        lstReturn.Add(new CPluginVariable("Hack Cry Responder|Hack Cry Trigger Number", hackCriesNeeded.GetType(), hackCriesNeeded));
        lstReturn.Add(new CPluginVariable("Hack Cry Responder|Hack Cry Trigger Response", hackCryResponse.GetType(), hackCryResponse));
        lstReturn.Add(new CPluginVariable("Hack Cry Responder|Additional Triggers", typeof(string[]), additionalTriggers.ToArray()));
      }

      lstReturn.Add(new CPluginVariable("Whitelist|In-Game Names", typeof(string[]), privilegedUsers.ToArray()));
      lstReturn.Add(new CPluginVariable("Whitelist|Clan Tags", typeof(string[]), privilegedTags.ToArray()));
      lstReturn.Add(new CPluginVariable("Whitelist|Action Taken", "enum.ActionTaken(None|Kill|Kick|Temporarily Ban|Permanently Ban)", whitelistActionTaken));
      if (whitelistActionTaken == "Temporarily Ban")
        lstReturn.Add(new CPluginVariable("Whitelist|Temporary Ban Length (in minutes)", whitelistBanLength.GetType(), whitelistBanLength));

      lstReturn.Add(new CPluginVariable("In-Game Commands|Vote Ban Commands", typeof(string[]), banCommand.ToArray()));
      lstReturn.Add(new CPluginVariable("In-Game Commands|Vote Kick Commands", typeof(string[]), kickCommand.ToArray()));
      lstReturn.Add(new CPluginVariable("In-Game Commands|Yes Commands", typeof(string[]), yesCommand.ToArray()));
      lstReturn.Add(new CPluginVariable("In-Game Commands|No Commands", typeof(string[]), noCommand.ToArray()));
      lstReturn.Add(new CPluginVariable("In-Game Commands|Cancel Vote Commands", typeof(string[]), cancelVoteCommand.ToArray()));

      lstReturn.Add(new CPluginVariable("In-Game Messages|Reset Messages?", "enum.resetMessages(...|Do it!)", resetMessages));
      lstReturn.Add(new CPluginVariable("In-Game Messages|Message List", typeof(string[]), inGameMessages.ToArray()));

      return lstReturn;
    }

    public List<CPluginVariable> GetPluginVariables()
    {
      List<CPluginVariable> lstReturn = new List<CPluginVariable>();

      lstReturn.Add(new CPluginVariable("Enable Vote Ban?", typeof(enumBoolYesNo), enableVoteBan));
      lstReturn.Add(new CPluginVariable("Vote Ban Player Count Threshold", voteBanThreshold.GetType(), voteBanThreshold));
      lstReturn.Add(new CPluginVariable("Start Vote Ban Number", startVoteNumber.GetType(), startVoteNumber));
      lstReturn.Add(new CPluginVariable("Vote Ban Pass Percentage", votePercentageRequired.GetType(), votePercentageRequired));
      lstReturn.Add(new CPluginVariable("Vote Ban Duration (in minutes)", voteDuration.GetType(), voteDuration));
      lstReturn.Add(new CPluginVariable("Vote Ban Progress Display Interval (in seconds)", voteProgressNumber.GetType(), voteProgressNumber));
      lstReturn.Add(new CPluginVariable("Ban Type", "enum.BanType(GUID|IP|Name|PB GUID)", banType));
      lstReturn.Add(new CPluginVariable("Ban Duration", "enum.BanDuration(Permanent|Temporary)", banDuration));
      lstReturn.Add(new CPluginVariable("Ban Length (in minutes)", banLength.GetType(), banLength));
      lstReturn.Add(new CPluginVariable("Ban Reason Message", banDisplayReason.GetType(), banDisplayReason));

      lstReturn.Add(new CPluginVariable("Enable Vote Kick?", typeof(enumBoolYesNo), enableVoteKick));
      lstReturn.Add(new CPluginVariable("Vote Kick Player Count Threshold", voteKickThreshold.GetType(), voteKickThreshold));
      lstReturn.Add(new CPluginVariable("Start Vote Kick Number", startKickVoteNumber.GetType(), startKickVoteNumber));
      lstReturn.Add(new CPluginVariable("Vote Kick Pass Percentage", voteKickPercentageRequired.GetType(), voteKickPercentageRequired));
      lstReturn.Add(new CPluginVariable("Vote Kick Duration (in minutes)", voteKickDuration.GetType(), voteKickDuration));
      lstReturn.Add(new CPluginVariable("Vote Kick Progress Display Interval (in seconds)", voteKickProgressNumber.GetType(), voteKickProgressNumber));

      lstReturn.Add(new CPluginVariable("Enable Hack Cry Responder?", typeof(enumBoolYesNo), enableHackCry));
      lstReturn.Add(new CPluginVariable("Hack Cry Trigger Number", hackCriesNeeded.GetType(), hackCriesNeeded));
      lstReturn.Add(new CPluginVariable("Hack Cry Trigger Response", hackCryResponse.GetType(), hackCryResponse));
      lstReturn.Add(new CPluginVariable("Additional Triggers", typeof(string[]), additionalTriggers.ToArray()));

      lstReturn.Add(new CPluginVariable("In-Game Names", typeof(string[]), privilegedUsers.ToArray()));
      lstReturn.Add(new CPluginVariable("Clan Tags", typeof(string[]), privilegedTags.ToArray()));
      lstReturn.Add(new CPluginVariable("Action Taken", "enum.ActionTaken(None|Kill|Kick|Temporarily Ban|Permanently Ban)", whitelistActionTaken));
      lstReturn.Add(new CPluginVariable("Temporary Ban Length (in minutes)", whitelistBanLength.GetType(), whitelistBanLength));

      lstReturn.Add(new CPluginVariable("Vote Ban Commands", typeof(string[]), banCommand.ToArray()));
      lstReturn.Add(new CPluginVariable("Vote Kick Commands", typeof(string[]), kickCommand.ToArray()));
      lstReturn.Add(new CPluginVariable("Yes Commands", typeof(string[]), yesCommand.ToArray()));
      lstReturn.Add(new CPluginVariable("No Commands", typeof(string[]), noCommand.ToArray()));
      lstReturn.Add(new CPluginVariable("Cancel Vote Commands", typeof(string[]), cancelVoteCommand.ToArray()));

      lstReturn.Add(new CPluginVariable("Reset Messages?", "enum.resetMessages(...|Do it!)", resetMessages));
      lstReturn.Add(new CPluginVariable("Message List", typeof(string[]), inGameMessages.ToArray()));

      return lstReturn;
    }

    public void SetPluginVariable(string strVariable, string strValue)
    {
      if (strVariable.CompareTo("Enable Vote Ban?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
      {
        enableVoteBan = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
      }
      else if (strVariable.CompareTo("Vote Ban Player Count Threshold") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        voteBanThreshold = valueAsInt;
      }
      else if (strVariable.CompareTo("Start Vote Ban Number") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        startVoteNumber = valueAsInt;
      }
      else if (strVariable.CompareTo("Vote Ban Pass Percentage") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        votePercentageRequired = valueAsInt;
      }
      else if (strVariable.CompareTo("Vote Ban Duration (in minutes)") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        voteDuration = valueAsInt;
      }
      else if (strVariable.CompareTo("Vote Ban Progress Display Interval (in seconds)") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        voteProgressNumber = valueAsInt;
      }
      else if (strVariable.CompareTo("Ban Type") == 0)
      {
        banType = strValue;
      }
      else if (strVariable.CompareTo("Ban Duration") == 0)
      {
        banDuration = strValue;
        //this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Vote Ban] ^n^0DEBUG: Ban Duration set to: ^b" + banDuration);
      }
      else if (strVariable.CompareTo("Ban Length (in minutes)") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        banLength = valueAsInt;
      }
      else if (strVariable.CompareTo("Ban Reason Message") == 0)
      {
        if (strValue == "")
          banDisplayReason = "Banned by player Vote Ban - %player% (Reason: %reason%)";
        else
          banDisplayReason = strValue;
      }
      else if (strVariable.CompareTo("Enable Vote Kick?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
      {
        enableVoteKick = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
      }
      else if (strVariable.CompareTo("Vote Kick Player Count Threshold") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        voteKickThreshold = valueAsInt;
      }
      else if (strVariable.CompareTo("Start Vote Kick Number") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        startKickVoteNumber = valueAsInt;
      }
      else if (strVariable.CompareTo("Vote Kick Pass Percentage") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        voteKickPercentageRequired = valueAsInt;
      }
      else if (strVariable.CompareTo("Vote Kick Duration (in minutes)") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        voteKickDuration = valueAsInt;
      }
      else if (strVariable.CompareTo("Vote Kick Progress Display Interval (in seconds)") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        voteKickProgressNumber = valueAsInt;
      }
      else if (strVariable.CompareTo("Enable Hack Cry Responder?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
      {
        enableHackCry = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
      }
      else if (strVariable.CompareTo("Hack Cry Trigger Number") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        hackCriesNeeded = valueAsInt;
      }
      else if (strVariable.CompareTo("Hack Cry Trigger Response") == 0)
      {
        if (strValue == "")
          hackCryResponse = "Is there a hacker on? Type %vbcommand% <player_name> to initiate a Vote Ban!";
        else
          hackCryResponse = strValue;
      }
      else if (strVariable.CompareTo("Additional Triggers") == 0)
      {
        additionalTriggers = new List<string>(CPluginVariable.DecodeStringArray(strValue));
      }
      else if (strVariable.CompareTo("In-Game Names") == 0)
      {
        privilegedUsers = new List<string>(CPluginVariable.DecodeStringArray(strValue));
      }
      else if (strVariable.CompareTo("Clan Tags") == 0)
      {
        privilegedTags = new List<string>(CPluginVariable.DecodeStringArray(strValue));
      }
      else if (strVariable.CompareTo("Action Taken") == 0)
      {
        whitelistActionTaken = strValue;
      }
      else if (strVariable.CompareTo("Temporary Ban Length (in minutes)") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        whitelistBanLength = valueAsInt;
      }
      else if (strVariable.CompareTo("Vote Ban Commands") == 0)
      {
        banCommand = new List<string>(CPluginVariable.DecodeStringArray(strValue));

        if (banCommand.Count == 1 && banCommand[0] == "")
        {
          banCommand[0] = "!voteban";
          banCommand.Add("@voteban");
        }
      }
      else if (strVariable.CompareTo("Vote Kick Commands") == 0)
      {
        kickCommand = new List<string>(CPluginVariable.DecodeStringArray(strValue));

        if (kickCommand.Count == 1 && kickCommand[0] == "")
        {
          kickCommand[0] = "!votekick";
          kickCommand.Add("@votekick");
        }
      }
      else if (strVariable.CompareTo("Yes Commands") == 0)
      {
        yesCommand = new List<string>(CPluginVariable.DecodeStringArray(strValue));

        if (yesCommand.Count == 1 && yesCommand[0] == "")
        {
          yesCommand[0] = "!yes";
          kickCommand.Add("@yes");
        }
      }
      else if (strVariable.CompareTo("No Commands") == 0)
      {
        noCommand = new List<string>(CPluginVariable.DecodeStringArray(strValue));

        if (noCommand.Count == 1 && noCommand[0] == "")
        {
          noCommand[0] = "!no";
          noCommand.Add("@no");
        }
      }
      else if (strVariable.CompareTo("Cancel Vote Commands") == 0)
      {
        cancelVoteCommand = new List<string>(CPluginVariable.DecodeStringArray(strValue));

        if (cancelVoteCommand.Count == 1 && cancelVoteCommand[0] == "")
        {
          cancelVoteCommand[0] = "!cancelvote";
          cancelVoteCommand.Add("@cancelvote");
        }
      }
      else if (strVariable.CompareTo("Reset Messages?") == 0)
      {
        if (strValue == "Do it!")
        {
          inGameMessages.Clear();
          initInGameMessages();
          this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Vote Ban] ^n^0Message list reset to default!");
        }
      }
      else if (strVariable.CompareTo("Message List") == 0)
      {
        inGameMessages = new List<string>(CPluginVariable.DecodeStringArray(strValue));

        ///// Adding messages to Message List. (version 1.3.0 - 1.5.0) /////
        if (!(inGameMessages.Count >= 79))
        {
          inGameMessages.Add("---------- %1% = voter %2% = Vote Ban player count threshold ----------");
          inGameMessages.Add("say \"Vote Ban is currently disabled! There must be at least %2% players for Vote Ban to be enabled.\" all");
          inGameMessages.Add("---------- %1% = voter %2% = Vote Kick player count threshold ----------");
          inGameMessages.Add("say \"Vote Kick is currently disabled! There must be at least %2% players for Vote Kick to be enabled.\" all");
        }
      }
    }

    private void initInGameMessages()
    {
      inGameMessages.Add("///// Here, you can modify each message that this plugin sends         /////");
      inGameMessages.Add("///// to the server. Keep each message here on their current line.       /////");
      inGameMessages.Add("///// Format is crucial so please try not to violate the rules of               /////");
      inGameMessages.Add("///// the following format including spaces... If you do, your                /////");
      inGameMessages.Add("///// message won't appear. If needed, you can reset everything       /////");
      inGameMessages.Add("///// here back to default with the \"Reset Messages?\" option.          /////");
      inGameMessages.Add("//////////////////////////////////////////////////////////////////////");
      inGameMessages.Add("///// Format: messagetype \"message\" directive seconds(yell only)    /////");
      inGameMessages.Add("///// messagetype = 'say' or 'yell'                                                        /////");
      inGameMessages.Add("///// message = your message (in quotes)                                          /////");
      inGameMessages.Add("///// directive = 'all' or 'player' (sends to relevant player)                    /////"); // 10
      inGameMessages.Add("///// seconds = number of seconds for yell message type to stay up /////");
      inGameMessages.Add("/////////////////////////////////////////////////////////////////////");
      inGameMessages.Add("/////////////////////////////////////////////////////////////////////");
      inGameMessages.Add("---------- %1% = voted player %2% = suggested name %3% = yes command %4% = no command ----------");
      inGameMessages.Add("say \"Player %1% does not exist. Did you mean %2%? Type %3% or %4%...\" player");
      inGameMessages.Add("---------- %1% = voter %2% = vote type %3% = voted player ----------");
      inGameMessages.Add("say \"Player %1% has been killed as a result of attempting a vote to %2% immune player %3%!\" all");
      inGameMessages.Add("say \"Player %1% has been kicked as a result of attempting a vote to %2% immune player %3%!\" all");
      inGameMessages.Add("say \"Player %1% has been banned as a result of attempting a vote to %2% immune player %3%!\" all");
      inGameMessages.Add("---------- %1% = voted player ----------"); // 20
      inGameMessages.Add("----- WARNING! No relevant player for 'player' directive -----");
      inGameMessages.Add("yell \"The vote to ban %1% has expired!\" all");
      inGameMessages.Add("yell \"The vote to kick %1% has expired!\" all");
      inGameMessages.Add("---------- %1% = yes votes %2% = no votes ----------");
      inGameMessages.Add("----- WARNING! No relevant player for 'player' directive -----");
      inGameMessages.Add("----- WARNING! Don't set this next message AND the previous 2 messages to yell as they happen simultaneously! -----");
      inGameMessages.Add("say \"The summary of the vote was - Yes Votes: %1% No Votes: %2%\" all");
      inGameMessages.Add("say \"Vote Progress - Yes Votes: %1% No Votes: %2%\" all");
      inGameMessages.Add("---------- %1% = remaining yes votes needed %2% = vote type (kick or ban) %3% = voted player ----------");
      inGameMessages.Add("----- WARNING! No relevant player for 'player' directive -----"); // 30
      inGameMessages.Add("----- WARNING! Don't set this next message AND the previous message to yell as they happen simultaneously! -----");
      inGameMessages.Add("say \"%1% more Yes votes needed to %2% %3% ...\" all");
      inGameMessages.Add("---------- %1% = admin canceling vote %2% = vote type (kick or ban) %3% = voted player ----------");
      inGameMessages.Add("----- WARNING! No relevant player for 'player' directive -----");
      inGameMessages.Add("yell \"%1% has canceled the vote to %2% %3%!\" all"); // 35
      inGameMessages.Add("---------- %1% = voted player %2% = reason for vote ban ----------");
      inGameMessages.Add("----- WARNING! No relevant player for 'player' directive -----");
      inGameMessages.Add("yell \"Vote Ban successful! Banning %1% for %2%...\" all");
      inGameMessages.Add("---------- %1% = voted player ----------");
      inGameMessages.Add("----- WARNING! No relevant player for 'player' directive -----"); // 40
      inGameMessages.Add("yell \"Vote Kick successful! Kicking %1%...\" all");
      inGameMessages.Add("---------- %1% = voter ----------");
      inGameMessages.Add("say \"Your vote has been registered as YES!\" player");
      inGameMessages.Add("say \"Your vote has been registered as NO!\" player");
      inGameMessages.Add("say \"%1% has already voted!\" all");
      inGameMessages.Add("---------- %1% = ban command %2% = kick command %3% = voter ----------");
      inGameMessages.Add("say \"There is no vote in progress! Type %1% <player_name> or %2% <player_name> to put in a request.\" player");
      inGameMessages.Add("---------- %1% = voted player %2% = yes command %3% = no command %4% = reason for vote ban ----------");
      inGameMessages.Add("----- WARNING! No relevant player for 'player' directive -----");
      inGameMessages.Add("yell \"The vote to ban %1% for \"%4%\" has COMMENCED! Type %2% or %3% in chat to vote!\" all 30"); // 50
      inGameMessages.Add("---------- %1% = voted player %2% = yes command %3% = no command ----------");
      inGameMessages.Add("----- WARNING! No relevant player for 'player' directive -----");
      inGameMessages.Add("yell \"The vote to kick %1% has COMMENCED! Type %2% or %3% in chat to vote!\" all 30");
      inGameMessages.Add("---------- %1% = voter %2% = vote ban command %3% = voted player %4% = remaining votes needed ----------");
      inGameMessages.Add("say \"%1% has put in a %2% request to initiate a Vote Ban on %3%! %4% more needed!\" all");
      inGameMessages.Add("---------- %1% = voter %2% = voted player ----------");
      inGameMessages.Add("say \"%1% has ALREADY put in a request to initiate a Vote Ban on %2%!\" all");
      inGameMessages.Add("---------- %1% = voter %2% = vote kick command %3% = voted player %4% = remaining votes needed ----------");
      inGameMessages.Add("say \"%1% has put in a %2% request to initiate a Vote Kick on %3%! %4% more needed!\" all");
      inGameMessages.Add("---------- %1% = voter %2% = voted player ----------"); // 60
      inGameMessages.Add("say \"%1% has ALREADY put in a request to initiate a Vote Kick on %2%!\" all");
      inGameMessages.Add("---------- %1% = voter ----------");
      inGameMessages.Add("say \"%1% - A vote is already in progress...\" all");
      inGameMessages.Add("---------- %1% = yes command %2% = no command %3% = voter ----------");
      inGameMessages.Add("say \"%3% - Awaiting %1% or %2% confirmation on your last Vote Ban suggested player name!\" player");
      inGameMessages.Add("say \"%3% - Awaiting %1% or %2% confirmation on your last Vote Kick suggested player name!\" player");
      inGameMessages.Add("---------- %1% = voter %2% = voted player ----------");
      inGameMessages.Add("say \"%1% - Player %2% is protected from being Vote Banned!\" all");
      inGameMessages.Add("---------- %1% = voter %2% = vote ban command ----------");
      inGameMessages.Add("say \"%1% - The first %2% request must include the reason... %2% <player_name> <reason>\" all"); // 70
      inGameMessages.Add("---------- %1% = voter -----");
      inGameMessages.Add("say \"%1% - You have declined the suggested player name...\" player");
      inGameMessages.Add("---------- %1% = voter %2% = voted player ----------");
      inGameMessages.Add("say \"%1% - Player %2% is protected from being Vote Kicked!\" all");
      inGameMessages.Add("---------- %1% = player canceling vote ----------");
      inGameMessages.Add("say \"%1% - You do not have the priveleges to cancel a vote!\" all");
      inGameMessages.Add("say \"%1% - There is no vote in progress to cancel!\" player");
      inGameMessages.Add("---------- %1% = voter %2% = Vote Ban player count threshold ----------");
      inGameMessages.Add("say \"Vote Ban is currently disabled! There must be at least %2% players for Vote Ban to be enabled.\" all");
      inGameMessages.Add("---------- %1% = voter %2% = Vote Kick player count threshold ----------"); // 80
      inGameMessages.Add("say \"Vote Kick is currently disabled! There must be at least %2% players for Vote Kick to be enabled.\" all");
    }

    private void processMessage(int messageLine, params object[] items)
    {
      int messageStartIndex = 0;
      int messageEndIndex = 0;
      for (int i = 0; i < inGameMessages[messageLine].Length; i++)
      {
        if (inGameMessages[messageLine][i] == '"')
        {
          if (messageStartIndex == 0)
            messageStartIndex = i;
          else
            messageEndIndex = i;
        }
      }

      string message = inGameMessages[messageLine].Substring(messageStartIndex + 1, messageEndIndex - messageStartIndex - 1);

      for (int i = 1; i < items.Length; i++)
      {
        message = message.Replace("%" + i.ToString() + "%", items[i].ToString());
      }

      string paramaters = inGameMessages[messageLine].Remove(messageStartIndex, messageEndIndex - messageStartIndex + 2);
      string[] paramatersAsArray = paramaters.Split(' ');
      if (paramatersAsArray[0] == "say")
      {
        if (paramatersAsArray[1] == "player")
          this.ExecuteCommand("procon.protected.send", "admin.say", message, "player", items[0].ToString());
        else if (paramatersAsArray[1] == "all")
          this.ExecuteCommand("procon.protected.send", "admin.say", message, "all");
      }
      else if (paramatersAsArray[0] == "yell")
      {
        if (paramatersAsArray[1] == "player")
        {
          if (paramatersAsArray.Length >= 3)
            this.ExecuteCommand("procon.protected.send", "admin.yell", message, paramatersAsArray[2], "player", items[0].ToString());
          else
            this.ExecuteCommand("procon.protected.send", "admin.yell", message, "10", "player", items[0].ToString());
        }
        else if (paramatersAsArray[1] == "all")
        {
          if (paramatersAsArray.Length >= 3)
            this.ExecuteCommand("procon.protected.send", "admin.yell", message, paramatersAsArray[2]);
          else
            this.ExecuteCommand("procon.protected.send", "admin.yell", message);
        }
      }
    }

    private void getPlayerCount()
    {
      needPlayerCount = true;
      this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
    }

    private string getSuggestedPlayerName(List<CPlayerInfo> player, string currentVotedPlayer, ref bool forcePlayer)
    {
      string suggestedName = player[0].SoldierName; // suggested player name initialized with the first player in the list
      List<string> transformedPlayerNames = new List<string>();
      string transformedCurrentVotedPlayer;
      List<int> votedProbability = new List<int>(); // paired against List of current players for number highest probability with greatest number
      int highestProbabilityNumber = 0; // used and assigned to determine which element in votedProbability list is highest

      transformedCurrentVotedPlayer = currentVotedPlayer.Replace('I', 'l');
      transformedCurrentVotedPlayer = transformedCurrentVotedPlayer.ToLower();

      for (int i = 0; i < player.Count; i++)
        transformedPlayerNames.Add(player[i].SoldierName);

      for (int i = 0; i < transformedPlayerNames.Count; i++)
      {
        transformedPlayerNames[i] = transformedPlayerNames[i].Replace('I', 'l');
        transformedPlayerNames[i] = transformedPlayerNames[i].ToLower();
      }

      // if currentVotedPlayer matches a substring of a player from the player list, use that player (return that player string) and skip everything else
      for (int i = 0; i < transformedPlayerNames.Count; i++)
      {
        if (transformedPlayerNames[i].Contains(transformedCurrentVotedPlayer))
        {
          forcePlayer = true;
          return player[i].SoldierName;
        }
      }

      // initializes votedProbability List to be of proportional size to players List
      for (int i = 0; i < transformedPlayerNames.Count; i++)
        votedProbability.Add(0);

      for (int indexStart = 0; indexStart < transformedCurrentVotedPlayer.Length - 1; indexStart++)
      {
        for (int currSubStrSize = 1; currSubStrSize <= transformedCurrentVotedPlayer.Length - indexStart; currSubStrSize++)
        {
          for (int i = 0; i < transformedPlayerNames.Count; i++)
          {
            if (transformedPlayerNames[i].Contains(transformedCurrentVotedPlayer.Substring(indexStart, currSubStrSize)))
            {
              if (votedProbability[i] < currSubStrSize)
                votedProbability[i] = currSubStrSize;
            }
          }
        }
      }

      for (int i = 0; i < transformedPlayerNames.Count; i++)
      {
        if (votedProbability[i] > highestProbabilityNumber)
          highestProbabilityNumber = votedProbability[i];
      }

      // takes player from List proportional to first instance (could be multiple of the same highest number) in votedProbability List with the the highest number
      for (int i = 0; i < votedProbability.Count; i++)
      {
        if (votedProbability[i] == highestProbabilityNumber)
          suggestedName = player[i].SoldierName;
      }

      processMessage(15, currentSpeaker, currentVotedPlayer, suggestedName, yesCommand[0], noCommand[0]);

      return suggestedName;
    }

    private void storeVotedVictimInfo()
    {
      needVotedVictimInfo = true;

      if (banType == "GUID")
        this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
      else
        this.ExecuteCommand("procon.protected.send", "punkBuster.pb_sv_command pb_sv_plist");
    }

    private void banPlayerByName()
    {
      banningPlayerByName = true;

      this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
    }

    private void banPlayerByGuid()
    {
      foundvotedVictim = false;
      banningPlayerByGUID = true;

      this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
    }

    private void banPlayerByIp()
    {
      foundvotedVictim = false;
      banningPlayerByIP = true;

      this.ExecuteCommand("procon.protected.send", "punkBuster.pb_sv_command pb_sv_plist");
    }

    private void banPlayerByPbGuid()
    {
      foundvotedVictim = false;
      banningPlayerByPbGuid = true;

      this.ExecuteCommand("procon.protected.send", "punkBuster.pb_sv_command pb_sv_plist");
    }

    private void kickPlayer()
    {
      kickingPlayer = true;

      this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
    }

    private void processVote(string speaker, string playerVoted, string voteType)
    {
      processingVote = true;

      currentSpeaker = speaker;
      currentVotedPlayer = playerVoted;
      currentVoteType = voteType;
      if (voteType == "ban")
        currentVoteReason = voteReason;

      this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
    }

    private void processVote(string speaker, string playerVoted, string voteType, string voteReason)
    {
      processingVote = true;

      currentSpeaker = speaker;
      currentVotedPlayer = playerVoted;
      currentVoteType = voteType;
      if (voteType == "ban")
        currentVoteReason = voteReason;
      this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
    }

    private void whitelistActionHandler(string speaker, string votedPlayer)
    {
      if (whitelistActionTaken == "Kill")
      {
        this.ExecuteCommand("procon.protected.send", "admin.killPlayer", speaker);
        processMessage(17, speaker, speaker, currentVoteType, votedPlayer);
      }
      else if (whitelistActionTaken == "Kick")
      {
        this.ExecuteCommand("procon.protected.send", "admin.kickPlayer", speaker, "Kicked for attempting a vote to " + currentVoteType + " immune player " + votedPlayer + "!");
        processMessage(18, speaker, speaker, currentVoteType, votedPlayer);
      }
      else if (whitelistActionTaken == "Temporarily Ban")
      {
        this.ExecuteCommand("procon.protected.send", "banList.add", "name", speaker, "seconds", (whitelistBanLength * 60).ToString(), "Banned for attempting a vote to " + currentVoteType + " immune player " + votedPlayer + "!");
        processMessage(19, speaker, speaker, currentVoteType, votedPlayer);
        this.ExecuteCommand("procon.protected.send", "banList.save");
        this.ExecuteCommand("procon.protected.send", "banList.list");
      }
      else if (whitelistActionTaken == "Permanently Ban")
      {
        this.ExecuteCommand("procon.protected.send", "banList.add", "name", speaker, "perm", "Banned for attempting a vote to " + currentVoteType + " immune player " + votedPlayer + "!");
        processMessage(19, speaker, speaker, currentVoteType, votedPlayer);
        this.ExecuteCommand("procon.protected.send", "banList.save");
        this.ExecuteCommand("procon.protected.send", "banList.list");
      }
    }

    private bool hasTriggerWord(string message)
    {
      // Don't trigger if trigger word is in a voteban or votekick reason
      if (isVoteBan(message) || isVoteKick(message))
        return false;

      if (message.Contains("hack"))
        return true;

      for (int i = 0; i < additionalTriggers.Count; i++)
        if (message.Contains(additionalTriggers[i].ToLower()))
          return true;

      return false;
    }

    private bool isAdmin(string speaker)
    {
      bool isAdmin = false;
      currentPrivileges = GetAccountPrivileges(speaker);

      if (currentPrivileges != null)
      {
        if (currentPrivileges.CanLogin)
          isAdmin = true;
      }

      return isAdmin;
    }

    private bool isImmunePlayer(string votedPlayer)
    {
      currentPrivileges = GetAccountPrivileges(votedPlayer);

      if (currentPrivileges != null)
      {
        if (currentPrivileges.CanLogin)
          return true;
      }

      if (privilegedUsers.Contains(votedPlayer))
      {
        return true;
      }

      if (privilegedTags.Count >= 1 && privilegedTags[0] != "")
      {
        this.blClient = new BattlelogClient();

        if (privilegedTags.Contains(this.blClient.getClanTag(votedPlayer)))
        {
          return true;
        }
      }

      return false;
    }

    private bool isVoteBan(string message)
    {
      for (int i = 0; i < banCommand.Count; i++)
      {
        if (message.StartsWith(banCommand[i]))
          return true;
      }

      return false;
    }

    private bool isVoteKick(string message)
    {
      for (int i = 0; i < kickCommand.Count; i++)
      {
        if (message.StartsWith(kickCommand[i]))
          return true;
      }

      return false;
    }

    private void OnVoteInProgressEnd(object source, ElapsedEventArgs e)
    {
      if (!voteIsInProgress || !pluginEnabled)
        return;

      if (voteType == "ban")
      {
        processMessage(22, null, votedVictim);
        processMessage(27, null, yesVotes, noVotes);

        for (int i = 0; i < playerBeingVoted.Count; i++)
        {
          if (playerBeingVoted[i] == votedVictim)
            playerBeingVotedCount[i] = 0;
        }
      }
      else if (voteType == "kick")
      {
        processMessage(23, null, votedVictim);
        processMessage(27, null, yesVotes, noVotes);

        for (int i = 0; i < playerBeingVoted.Count; i++)
        {
          if (playerBeingKickVoted[i] == votedVictim)
            playerBeingKickVotedCount[i] = 0;
        }
      }

      this.voteInProgress.Stop();
      this.voteProgressDisplay.Stop();

      yesVotes = 0;
      noVotes = 0;
      alreadyVoted.Clear();
      voteIsInProgress = false;

      this.voteInProgress.Dispose();
      this.voteProgressDisplay.Dispose();
    }

    private void OnVoteProgressDisplay(object source, ElapsedEventArgs e)
    {
      if (!voteIsInProgress || !pluginEnabled)
        return;
      processMessage(28, null, yesVotes, noVotes);
      processMessage(32, null, (yesVotesNeeded - yesVotes).ToString(), voteType, votedVictim);
    }

    private void cancelVote(string speaker)
    {
      if (voteType == "ban")
      {
        for (int i = 0; i < playerBeingVoted.Count; i++)
        {
          if (playerBeingVoted[i] == votedVictim)
            playerBeingVotedCount[i] = 0;
        }
      }
      else if (voteType == "kick")
      {
        for (int i = 0; i < playerBeingKickVoted.Count; i++)
        {
          if (playerBeingKickVoted[i] == votedVictim)
            playerBeingKickVotedCount[i] = 0;
        }
      }

      this.voteInProgress.Stop();
      this.voteProgressDisplay.Stop();

      yesVotes = 0;
      noVotes = 0;
      alreadyVoted.Clear();
      voteIsInProgress = false;

      this.voteInProgress.Dispose();
      this.voteProgressDisplay.Dispose();

      processMessage(35, null, speaker, voteType, votedVictim);
    }

    private void endVote()
    {
      if (voteType == "ban")
      {
        for (int i = 0; i < playerBeingVoted.Count; i++)
        {
          if (playerBeingVoted[i] == votedVictim)
          {
            voteReason = playerBeingVotedReason[i];
            playerBeingVotedCount[i] = 0;
          }
        }

        processMessage(38, null, votedVictim, voteReason);

        if (banType == "GUID")
          banPlayerByGuid();
        else if (banType == "IP")
          banPlayerByIp();
        else if (banType == "Name")
          banPlayerByName();
        else if (banType == "PB GUID")
          banPlayerByPbGuid();
      }
      else if (voteType == "kick")
      {
        processMessage(41, null, votedVictim);
        kickPlayer();

        for (int i = 0; i < playerBeingKickVoted.Count; i++)
        {
          if (playerBeingKickVoted[i] == votedVictim)
            playerBeingKickVotedCount[i] = 0;
        }
      }

      this.voteInProgress.Stop();
      this.voteProgressDisplay.Stop();

      yesVotes = 0;
      noVotes = 0;
      alreadyVoted.Clear();
      voteIsInProgress = false;

      this.voteInProgress.Dispose();
      this.voteProgressDisplay.Dispose();
    }

    private void voteMainHandler(string speaker, string message)
    {
      if (voteIsInProgress)
      {
        if (yesCommand.Contains(message))
        {
          if (!alreadyVoted.Contains(speaker))
          {
            yesVotes++;
            alreadyVoted.Add(speaker);
            processMessage(43, speaker, speaker);
          }
          else
            processMessage(45, speaker, speaker);
        }
        else if (noCommand.Contains(message))
        {
          if (!alreadyVoted.Contains(speaker))
          {
            noVotes++;
            alreadyVoted.Add(speaker);
            processMessage(44, speaker, speaker);
          }
          else
            processMessage(45, speaker, speaker);
        }

        if (yesVotes == yesVotesNeeded)
        {
          endVote();
        }
      }
      else
      {
        if (!(message == "!yes" && isAdmin(speaker) || message == "!no" && isAdmin(speaker)))
          processMessage(47, speaker, banCommand[0], kickCommand[0], speaker);
      }
    }

    private void initiateVoteBan(string banVictim)
    {
      suggestedPlayerName.Clear();
      awaitedConfirmationPlayer.Clear();
      awaitedConfirmationPlayerReason.Clear();

      votedVictim = banVictim;
      if (banType != "Name")
        storeVotedVictimInfo();
      voteType = "ban";
      voteIsInProgress = true;
      getPlayerCount();

      this.voteInProgress = new System.Timers.Timer((voteDuration * 60) * 1000);
      this.voteInProgress.Elapsed += new ElapsedEventHandler(OnVoteInProgressEnd);
      this.voteInProgress.Start();

      this.voteProgressDisplay = new System.Timers.Timer(voteProgressNumber * 1000);
      this.voteProgressDisplay.Elapsed += new ElapsedEventHandler(OnVoteProgressDisplay);
      this.voteProgressDisplay.Start();

      processMessage(50, null, banVictim, yesCommand[0], noCommand[0], playerBeingVotedReason[playerBeingVoted.IndexOf(banVictim)]);
    }

    private void initiateVoteKick(string kickVictim)
    {
      awaitedConfirmationPlayerForKick.Clear();
      suggestedPlayerNameForKick.Clear();

      votedVictim = kickVictim;
      voteType = "kick";
      voteIsInProgress = true;
      getPlayerCount();

      this.voteInProgress = new System.Timers.Timer((voteKickDuration * 60) * 1000);
      this.voteInProgress.Elapsed += new ElapsedEventHandler(OnVoteInProgressEnd);
      this.voteInProgress.Start();

      this.voteProgressDisplay = new System.Timers.Timer(voteKickProgressNumber * 1000);
      this.voteProgressDisplay.Elapsed += new ElapsedEventHandler(OnVoteProgressDisplay);
      this.voteProgressDisplay.Start();

      processMessage(53, null, kickVictim, yesCommand[0], noCommand[0]);
    }

    private void voteBanStartHandler(string speaker, string playerVoted)
    {
      int remainingVotes = 0;
      bool alreadyVoted = false;

      // adds the one being voted to the List if they are not already and starts their count at 1, otherwise (else) adds to their count if they are in the list
      if (!playerBeingVoted.Contains(playerVoted))
      {
        playerBeingVoted.Add(playerVoted);

        for (int i = 0; i < playerBeingVoted.Count; i++)
        {
          if (playerBeingVoted[i] == playerVoted)
          {
            playerBeingVotedCount.Add(1);
            remainingVotes = startVoteNumber - playerBeingVotedCount[i];

            if (remainingVotes == 0)
              initiateVoteBan(playerVoted);
            else
              processMessage(55, speaker, speaker, banCommand[0], playerVoted, remainingVotes.ToString());
          }
        }

        voteeVoters.Add(playerVoted, speaker);
      }
      else
      {
        string[] voters = new string[maxVoters];
        voters = voteeVoters[playerVoted].Split(' ');

        for (int i = 0; i < voters.Length; i++)
          if (speaker == voters[i])
            alreadyVoted = true;

        if (!alreadyVoted)
        {
          for (int i = 0; i < playerBeingVoted.Count; i++)
          {
            if (playerBeingVoted[i] == playerVoted)
            {
              playerBeingVotedCount[i]++;
              remainingVotes = startVoteNumber - playerBeingVotedCount[i];

              if (remainingVotes == 0)
                initiateVoteBan(playerVoted);
              else
                processMessage(55, speaker, speaker, banCommand[0], playerVoted, remainingVotes.ToString());
            }
          }

          voteeVoters[playerVoted] = voteeVoters[playerVoted] + " " + speaker;
        }
        else
          processMessage(57, speaker, speaker, playerVoted);
      }
    }

    private void voteKickStartHandler(string speaker, string playerVoted)
    {
      int remainingVotes = 0;
      bool alreadyVoted = false;

      // adds the one being voted to the List if they are not already and starts their count at 1, otherwise (else) adds to their count if they are in the list
      if (!playerBeingKickVoted.Contains(playerVoted))
      {
        playerBeingKickVoted.Add(playerVoted);

        for (int i = 0; i < playerBeingKickVoted.Count; i++)
        {
          if (playerBeingKickVoted[i] == playerVoted)
          {
            playerBeingKickVotedCount.Add(1);
            remainingVotes = startKickVoteNumber - playerBeingKickVotedCount[i];

            if (remainingVotes == 0)
              initiateVoteKick(playerVoted);
            else
              processMessage(59, speaker, speaker, kickCommand[0], playerVoted, remainingVotes.ToString());
          }
        }

        voteeVotersForKick.Add(playerVoted, speaker);
      }
      else
      {
        string[] voters = new string[maxVoters];
        voters = voteeVotersForKick[playerVoted].Split(' ');

        for (int i = 0; i < voters.Length; i++)
          if (speaker == voters[i])
            alreadyVoted = true;

        if (!alreadyVoted)
        {
          for (int i = 0; i < playerBeingKickVoted.Count; i++)
          {
            if (playerBeingKickVoted[i] == playerVoted)
            {
              playerBeingKickVotedCount[i]++;
              remainingVotes = startKickVoteNumber - playerBeingKickVotedCount[i];

              if (remainingVotes == 0)
                initiateVoteKick(playerVoted);
              else
                processMessage(59, speaker, speaker, kickCommand[0], playerVoted, remainingVotes.ToString());
            }
          }

          voteeVotersForKick[playerVoted] = voteeVotersForKick[playerVoted] + " " + speaker;
        }
        else
          processMessage(61, speaker, speaker, playerVoted);
      }
    }

    private void getVoteParameters(string message, ref string playerVoted, ref string reason)
    {
      for (int i = 0; i < banCommand.Count; i++)
      {
        if (message.StartsWith(banCommand[i]))
        {
          message = message.Substring(banCommand[i].Length);
          playerVoted = message.Trim();
        }
      }

      for (int i = 0; i < playerVoted.Length; i++)
      {
        if (playerVoted[i] == ' ')
        {
          reason = playerVoted.Substring(i, playerVoted.Length - i);
          reason = reason.Trim();
          playerVoted = playerVoted.Substring(0, i);

          break;
        }
      }
    }

    private void getVoteParameters(string message, ref string playerVoted)
    {
      for (int i = 0; i < kickCommand.Count; i++)
      {
        if (message.StartsWith(kickCommand[i]))
        {
          message = message.Substring(kickCommand[i].Length);
          playerVoted = message.Trim();
        }
      }
    }

    public override void OnTeamChat(string speaker, string message, int teamId)
    {
        if (!pluginEnabled)
            return;
        OnGlobalChat(speaker, message);
    }
    public override void OnSquadChat(string speaker, string message, int teamId, int squadId)
    {
        if (!pluginEnabled)
            return;
        OnGlobalChat(speaker, message);
    }

    public override void OnGlobalChat(string speaker, string message)
    {
      if (!pluginEnabled)
        return;
      if (isVoteBan(message) && enableVoteBan == enumBoolYesNo.Yes)
      {
        if (voteBanThreshold > 0)
        {
          needPlayerCountForThreshold = true;
          currentVoteTypeForThreshold = "ban";
          currentSpeakerForThreshold = speaker;
          currentMessageForThreshold = message;
          this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
        }
        else
        {
          if (voteIsInProgress)
            processMessage(63, speaker, speaker);
          else
          {
            string playerVoted = null;
            string reason = null;
            getVoteParameters(message, ref playerVoted, ref reason);

            if (awaitedConfirmationPlayer.Contains(speaker))
              processMessage(65, speaker, yesCommand[0], noCommand[0], speaker);
            else
              processVote(speaker, playerVoted, "ban", reason);
          }
        }
      }

      if (isVoteKick(message) && enableVoteKick == enumBoolYesNo.Yes)
      {
        if (voteKickThreshold > 0)
        {
          needPlayerCountForThreshold = true;
          currentVoteTypeForThreshold = "kick";
          currentSpeakerForThreshold = speaker;
          currentMessageForThreshold = message;
          this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
        }
        else
        {
          if (voteIsInProgress)
            processMessage(63, speaker, speaker);
          else
          {
            string playerVoted = null;
            getVoteParameters(message, ref playerVoted);

            if (awaitedConfirmationPlayerForKick.Contains(speaker))
              processMessage(66, speaker, yesCommand[0], noCommand[0], speaker);
            else
              processVote(speaker, playerVoted, "kick");
          }
        }
      }

      if (yesCommand.Contains(message) || noCommand.Contains(message))
      {
        if (awaitedConfirmationPlayer.Contains(speaker))
        {
          for (int i = 0; i < awaitedConfirmationPlayer.Count; i++)
          {
            if (awaitedConfirmationPlayer[i] == speaker)
            {
              if (yesCommand.Contains(message))
              {
                if (isImmunePlayer(suggestedPlayerName[i]))
                {
                  processMessage(68, speaker, speaker, suggestedPlayerName[i]);

                  if (whitelistActionTaken != "None")
                    whitelistActionHandler(speaker, suggestedPlayerName[i]);
                }
                else
                {
                  if (!playerBeingVoted.Contains(suggestedPlayerName[i]))
                  {
                    if (awaitedConfirmationPlayerReason[i] == null)
                      processMessage(70, speaker, speaker, banCommand[0]);
                    else
                    {
                      playerBeingVotedReason.Add(awaitedConfirmationPlayerReason[i]);
                      voteBanStartHandler(awaitedConfirmationPlayer[i], suggestedPlayerName[i]);
                    }
                  }
                  else
                    voteBanStartHandler(awaitedConfirmationPlayer[i], suggestedPlayerName[i]);
                }

                awaitedConfirmationPlayer[i] = null;
              }
              else if (noCommand.Contains(message))
              {
                processMessage(72, speaker, speaker);
                awaitedConfirmationPlayer[i] = null;
              }
            }
          }
        }
        else if (awaitedConfirmationPlayerForKick.Contains(speaker))
        {
          for (int i = 0; i < awaitedConfirmationPlayerForKick.Count; i++)
          {
            if (awaitedConfirmationPlayerForKick[i] == speaker)
            {
              if (yesCommand.Contains(message))
              {
                if (isImmunePlayer(suggestedPlayerNameForKick[i]))
                {
                  processMessage(74, speaker, speaker, suggestedPlayerNameForKick[i]);

                  if (whitelistActionTaken != "None")
                    whitelistActionHandler(speaker, suggestedPlayerNameForKick[i]);
                }
                else
                  voteKickStartHandler(awaitedConfirmationPlayerForKick[i], suggestedPlayerNameForKick[i]);

                awaitedConfirmationPlayerForKick[i] = null;
              }
              else if (noCommand.Contains(message))
              {
                processMessage(72, speaker, speaker);
                awaitedConfirmationPlayerForKick[i] = null;
              }
            }
          }
        }
        else
          voteMainHandler(speaker, message);
      }

      if (cancelVoteCommand.Contains(message))
      {
        if (voteIsInProgress)
        {
          if (isAdmin(speaker))
            cancelVote(speaker);
          else
            processMessage(76, speaker, speaker);
        }
        else
          processMessage(77, speaker, speaker);
      }

      if (enableHackCry == enumBoolYesNo.Yes)
      {
        string messageLower = message.ToLower();

        if (hasTriggerWord(messageLower) && speaker != "Server")
        {
          hackCryCount++;

          if ((hackCryCount % hackCriesNeeded) == 0)
            this.ExecuteCommand("procon.protected.send", "admin.say", hackCryResponse.Replace("%vbcommand%", banCommand[0]).Replace("%vkcommand%", kickCommand[0]), "all");
        }
      }
    }

    public override void OnListPlayers(List<CPlayerInfo> players, CPlayerSubset subset)
    {
      if (!pluginEnabled)
        return;
      if (needVotedVictimInfo && banType == "GUID")
      {
        for (int i = 0; i < players.Count; i++)
          if (players[i].SoldierName == votedVictim)
            votedVictimGUID = players[i].GUID;
      }

      if (needPlayerCount)
      {
        int playerCount = players.Count;

        if (voteType == "ban")
          yesVotesNeeded = (Convert.ToInt32((playerCount * (votePercentageRequired * .01))) > 0) ? Convert.ToInt32((playerCount * (votePercentageRequired * .01))) : 1;
        else if (voteType == "kick")
          yesVotesNeeded = (Convert.ToInt32((playerCount * (voteKickPercentageRequired * .01))) > 0) ? Convert.ToInt32((playerCount * (voteKickPercentageRequired * .01))) : 1;

        needPlayerCount = false;
      }

      if (needPlayerCountForThreshold)
      {
        if (currentVoteTypeForThreshold == "ban")
        {
          if (players.Count >= voteBanThreshold)
          {
            if (voteIsInProgress)
              processMessage(63, currentSpeakerForThreshold, currentSpeakerForThreshold);
            else
            {
              string playerVoted = null;
              string reason = null;
              getVoteParameters(currentMessageForThreshold, ref playerVoted, ref reason);

              if (awaitedConfirmationPlayer.Contains(currentSpeakerForThreshold))
                processMessage(65, currentSpeakerForThreshold, yesCommand[0], noCommand[0], currentSpeakerForThreshold);
              else
                processVote(currentSpeakerForThreshold, playerVoted, "ban", reason);
            }
          }
          else
            processMessage(79, currentSpeakerForThreshold, currentSpeakerForThreshold, voteBanThreshold.ToString());
        }
        else if (currentVoteTypeForThreshold == "kick")
        {
          if (players.Count >= voteKickThreshold)
          {
            if (voteIsInProgress)
              processMessage(63, currentSpeakerForThreshold, currentSpeakerForThreshold);
            else
            {
              string playerVoted = null;
              getVoteParameters(currentMessageForThreshold, ref playerVoted);

              if (awaitedConfirmationPlayerForKick.Contains(currentSpeakerForThreshold))
                processMessage(66, currentSpeakerForThreshold, yesCommand[0], noCommand[0], currentSpeakerForThreshold);
              else
                processVote(currentSpeakerForThreshold, playerVoted, "kick");
            }
          }
          else
            processMessage(81, currentSpeakerForThreshold, currentSpeakerForThreshold, voteKickThreshold.ToString());
        }

        needPlayerCountForThreshold = false;
      }

      if (banningPlayerByGUID)
      {
        for (int i = 0; i < players.Count; i++)
        {
          if (players[i].SoldierName == votedVictim)
          {
            if (banDuration == "Permanent")
              this.ExecuteCommand("procon.protected.send", "banList.add", "guid", players[i].GUID, "perm", banDisplayReason.Replace("%player%", votedVictim).Replace("%reason%", voteReason));
            else if (banDuration == "Temporary")
              this.ExecuteCommand("procon.protected.send", "banList.add", "guid", players[i].GUID, "seconds", (banLength * 60).ToString(), banDisplayReason.Replace("%player%", votedVictim).Replace("%reason%", voteReason));

            this.ExecuteCommand("procon.protected.send", "banList.save");
            this.ExecuteCommand("procon.protected.send", "banList.list");
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Vote Ban] ^n^0Player ^2" + votedVictim + " ^0has been ^8BANNED ^0by ^5GUID^0!");

            foundvotedVictim = true;
          }
        }

        if (!foundvotedVictim)
        {
          if (banDuration == "Permanent")
            this.ExecuteCommand("procon.protected.send", "banList.add", "guid", votedVictimGUID, "perm", banDisplayReason.Replace("%player%", votedVictim).Replace("%reason%", voteReason));
          else if (banDuration == "Temporary")
            this.ExecuteCommand("procon.protected.send", "banList.add", "guid", votedVictimGUID, "seconds", (banLength * 60).ToString(), banDisplayReason.Replace("%player%", votedVictim).Replace("%reason%", voteReason));

          this.ExecuteCommand("procon.protected.send", "banList.save");
          this.ExecuteCommand("procon.protected.send", "banList.list");
          this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Vote Ban] ^n^0Player ^2" + votedVictim + " ^0has been ^8BANNED ^0by ^5GUID^0!");
        }

        banningPlayerByGUID = false;
      }

      if (banningPlayerByName)
      {
        if (banDuration == "Permanent")
          this.ExecuteCommand("procon.protected.send", "banList.add", "name", votedVictim, "perm", banDisplayReason.Replace("%player%", votedVictim).Replace("%reason%", voteReason));
        else if (banDuration == "Temporary")
          this.ExecuteCommand("procon.protected.send", "banList.add", "name", votedVictim, "seconds", (banLength * 60).ToString(), banDisplayReason.Replace("%player%", votedVictim).Replace("%reason%", voteReason));

        this.ExecuteCommand("procon.protected.send", "banList.save");
        this.ExecuteCommand("procon.protected.send", "banList.list");
        this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Vote Ban] ^n^0Player ^2" + votedVictim + " ^0has been ^8BANNED ^0by ^5Name^0!");

        banningPlayerByName = false;
      }

      if (kickingPlayer)
      {
        this.ExecuteCommand("procon.protected.send", "admin.kickPlayer", votedVictim, "Kicked by player Vote Kick!");
        this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Vote Ban] ^n^0Player ^2" + votedVictim + " ^0has been ^8KICKED^0!");

        kickingPlayer = false;
      }

      if (processingVote)
      {
        bool playerFound = false;

        for (int i = 0; i < players.Count; i++)
        {
          if (players[i].SoldierName.ToLower() == currentVotedPlayer.ToLower())
          {
            playerFound = true;

            currentVotedPlayer = players[i].SoldierName;

            if (currentVoteType == "ban")
            {
              if (isImmunePlayer(currentVotedPlayer))
              {
                processMessage(68, currentSpeaker, currentSpeaker, currentVotedPlayer);

                if (whitelistActionTaken != "None")
                  whitelistActionHandler(currentSpeaker, currentVotedPlayer);
              }
              else
              {
                if (!playerBeingVoted.Contains(currentVotedPlayer))
                {
                  if (currentVoteReason == null)
                    processMessage(70, currentSpeaker, currentSpeaker, banCommand[0]);
                  else
                  {
                    playerBeingVotedReason.Add(currentVoteReason);
                    voteBanStartHandler(currentSpeaker, currentVotedPlayer);
                  }
                }
                else
                  voteBanStartHandler(currentSpeaker, currentVotedPlayer);
              }
            }
            else if (currentVoteType == "kick")
            {
              if (isImmunePlayer(currentVotedPlayer))
              {
                processMessage(74, currentSpeaker, currentSpeaker, currentVotedPlayer);

                if (whitelistActionTaken != "None")
                  whitelistActionHandler(currentSpeaker, currentVotedPlayer);
              }
              else
                voteKickStartHandler(currentSpeaker, currentVotedPlayer);
            }

            break;
          }
        }

        if (!playerFound)
        {
          bool forcePlayer = false;

          string forcedPlayerName = getSuggestedPlayerName(players, currentVotedPlayer, ref forcePlayer);

          if (currentVoteType == "ban")
          {
            if (forcePlayer)
            {
              if (isImmunePlayer(forcedPlayerName))
              {
                processMessage(68, currentSpeaker, currentSpeaker, forcedPlayerName);

                if (whitelistActionTaken != "None")
                  whitelistActionHandler(currentSpeaker, forcedPlayerName);
              }
              else
              {
                if (!playerBeingVoted.Contains(forcedPlayerName))
                {
                  if (currentVoteReason == null)
                    processMessage(70, currentSpeaker, currentSpeaker, banCommand[0]);
                  else
                  {
                    playerBeingVotedReason.Add(currentVoteReason);
                    voteBanStartHandler(currentSpeaker, forcedPlayerName);
                  }
                }
                else
                  voteBanStartHandler(currentSpeaker, forcedPlayerName);
              }
            }
            else
            {
              suggestedPlayerName.Add(forcedPlayerName);
              awaitedConfirmationPlayer.Add(currentSpeaker);
              awaitedConfirmationPlayerReason.Add(currentVoteReason);
            }
          }
          else if (currentVoteType == "kick")
          {
            if (forcePlayer)
            {
              if (isImmunePlayer(forcedPlayerName))
              {
                processMessage(74, currentSpeaker, currentSpeaker, forcedPlayerName);

                if (whitelistActionTaken != "None")
                  whitelistActionHandler(currentSpeaker, forcedPlayerName);
              }
              else
                voteKickStartHandler(currentSpeaker, forcedPlayerName);
            }
            else
            {
              suggestedPlayerNameForKick.Add(forcedPlayerName);
              awaitedConfirmationPlayerForKick.Add(currentSpeaker);
            }
          }
        }

        processingVote = false;
      }
    }

    public override void OnRoundOver(int winningTeamId)
    {
      playerBeingVoted.Clear();
      playerBeingVotedCount.Clear();
      playerBeingVotedReason.Clear();
      voteeVoters.Clear();
      suggestedPlayerName.Clear();
      awaitedConfirmationPlayer.Clear();
      awaitedConfirmationPlayerReason.Clear();

      playerBeingKickVoted.Clear();
      playerBeingKickVotedCount.Clear();
      voteeVotersForKick.Clear();
      awaitedConfirmationPlayerForKick.Clear();
      suggestedPlayerNameForKick.Clear();

      alreadyVoted.Clear();

      // Reset all counters
      this.yesVotes = 0;
      this.noVotes = 0;
      this.hackCryCount = 0;

      // Reset all flags
      this.voteIsInProgress = false;
      this.needPlayerCount = false;
      this.needPlayerCountForThreshold = false;
      this.needVotedVictimInfo = false;
      this.banningPlayerByGUID = false;
      this.banningPlayerByIP = false;
      this.banningPlayerByName = false;
      this.banningPlayerByPbGuid = false;
      this.kickingPlayer = false;
      this.processingVote = false;
      this.foundvotedVictim = false;
    }

    public override void OnLevelLoaded(String mapFileName, String Gamemode, int roundsPlayed, int roundsTotal)
    {
        OnRoundOver(0); // reset all when level loads, handles mapList.runNextRound better
    }

    public override void OnPlayerLeft(CPlayerInfo playerInfo) {
        if (!pluginEnabled || !voteIsInProgress)
            return;
        if (voteType == "kick" && playerInfo.SoldierName == votedVictim)
        {
            cancelVote("PLUGIN-Player-Left");
        }
    }

    public override void OnPunkbusterPlayerInfo(CPunkbusterInfo playerInfo)
    {
      if (!pluginEnabled)
        return;
      if (needVotedVictimInfo && banType == "IP" || banType == "PB GUID")
      {
        if (playerInfo.SoldierName == votedVictim)
        {
          if (banType == "IP")
          {
            votedVictimIP = playerInfo.Ip;
            needVotedVictimInfo = false;
          }
          else if (banType == "PB GUID")
          {
            votedVictimPbGuid = playerInfo.GUID;
            votedVictimIP = playerInfo.Ip;
            needVotedVictimInfo = false;
          }
        }
      }

      if (banningPlayerByIP)
      {
        if (playerInfo.SoldierName == votedVictim)
        {
          if (banDuration == "Permanent")
            this.ExecuteCommand("procon.protected.send", "banList.add", "ip", playerInfo.Ip, "perm", banDisplayReason.Replace("%player%", votedVictim).Replace("%reason%", voteReason));
          else if (banDuration == "Temporary")
            this.ExecuteCommand("procon.protected.send", "banList.add", "ip", playerInfo.Ip, "seconds", (banLength * 60).ToString(), banDisplayReason.Replace("%player%", votedVictim).Replace("%reason%", voteReason));

          this.ExecuteCommand("procon.protected.send", "banList.save");
          this.ExecuteCommand("procon.protected.send", "banList.list");
          this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Vote Ban] ^n^0Player ^2" + votedVictim + " ^0has been ^8BANNED ^0by ^5IP^0!");

          foundvotedVictim = true;
          banningPlayerByIP = false;
        }
      }

      if (banningPlayerByPbGuid)
      {
        if (playerInfo.SoldierName == votedVictim)
        {
          if (banDuration == "Permanent")
            this.ExecuteCommand("procon.protected.send", "punkBuster.pb_sv_command", String.Format("pb_sv_banguid {0} \"{1}\" \"{2}\" \"{3}\"", votedVictimPbGuid, votedVictim, votedVictimIP, banDisplayReason.Replace("%player%", votedVictim).Replace("%reason%", voteReason)));
            /* this.ExecuteCommand("procon.protected.send", "punkBuster.pb_sv_command", String.Format("pb_sv_ban \"{0}\" \"{1}\"", playerInfo.SoldierName, banDisplayReason.Replace("%player%", votedVictim).Replace("%reason%", voteReason))); */
          else if (banDuration == "Temporary")
            this.ExecuteCommand("procon.protected.send", "punkBuster.pb_sv_command", String.Format("pb_sv_kick \"{0}\" {1} \"{2}\"", playerInfo.SoldierName, banLength, banDisplayReason.Replace("%player%", votedVictim).Replace("%reason%", voteReason)));

          this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Vote Ban] ^n^0Player ^2" + votedVictim + " ^0has been ^8BANNED ^0by ^5PB GUID^0!");

          foundvotedVictim = true;
          banningPlayerByPbGuid = false;
        }
      }
    }

    public override void OnPunkbusterEndPlayerInfo()
    {
      if (!pluginEnabled)
        return;
      if (!foundvotedVictim && banningPlayerByIP || banningPlayerByPbGuid)
      {
        if (banningPlayerByIP)
        {
          if (banDuration == "Permanent")
            this.ExecuteCommand("procon.protected.send", "banList.add", "ip", votedVictimIP, "perm", banDisplayReason.Replace("%player%", votedVictim).Replace("%reason%", voteReason));
          else if (banDuration == "Temporary")
            this.ExecuteCommand("procon.protected.send", "banList.add", "ip", votedVictimIP, "seconds", (banLength * 60).ToString(), banDisplayReason.Replace("%player%", votedVictim).Replace("%reason%", voteReason));

          this.ExecuteCommand("procon.protected.send", "banList.save");
          this.ExecuteCommand("procon.protected.send", "banList.list");
          this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Vote Ban] ^n^0Player ^2" + votedVictim + " ^0has been ^8BANNED ^0by ^5IP^0!");

          banningPlayerByIP = false;
        }
        else if (banningPlayerByPbGuid)
        {
          this.ExecuteCommand("procon.protected.send", "punkBuster.pb_sv_command", String.Format("pb_sv_banguid {0} \"{1}\" \"{2}\" \"{3}\"", votedVictimPbGuid, votedVictim, votedVictimIP, banDisplayReason.Replace("%player%", votedVictim).Replace("%reason%", voteReason)));

          this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Vote Ban] ^n^0Player ^2" + votedVictim + " ^0has been ^8BANNED ^0by ^5PB GUID^0!");

          banningPlayerByPbGuid = false;
        }
      }
    }

    #region BattlelogClient Class

    public class BattlelogClient
    {
      private HttpWebRequest req = null;

      WebClient client = null;

      private String fetchWebPage(ref String html_data, String url)
      {
        try
        {
          if (client == null)
            client = new WebClient();

          html_data = client.DownloadString(url);
          return html_data;
        }
        catch (WebException e)
        {
          if (e.Status.Equals(WebExceptionStatus.Timeout))
            throw new Exception("HTTP request timed-out");
          else
            throw;
        }

        return html_data;
      }

      public String getClanTag(String player)
      {
        try
        {
          /* First fetch the player's main page to get the persona id */
          String result = "";
          fetchWebPage(ref result, "http://battlelog.battlefield.com/bf3/user/" + player);

          String tag = extractClanTag(result, player);

          return tag;
        }
        catch
        {
          //Handle exceptions here however you want
        }

        return null;
      }

      public String extractClanTag(String result, String player)
      {
        /* Extract the player tag */
        Match tag = Regex.Match(result, @"\[\s*([a-zA-Z0-9]+)\s*\]\s*" + player, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (tag.Success)
          return tag.Groups[1].Value;

        return String.Empty;
      }
    }

    #endregion
  }

}
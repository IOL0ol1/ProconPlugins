using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
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
  public class CServerKills : PRoConPluginAPI, IPRoConPluginInterface
  {
    private enumBoolYesNo enableServerKills;
    private string dbHost;
    private string dbPort;
    private string dbName;
    private string dbUser;
    private string dbPassword;
    private string dbTable;
    private int displayInterval;
    private int killsDisplayedTimespan;
    private string addWeaponServerKills;
    private List<string> killsMessageDisplay;
    private enumBoolYesNo displayKillsToConsole;

    private enumBoolYesNo enableRoundStats;
    private string addWeaponRoundStats;
    private List<string> messageDisplay;
    private enumBoolYesNo displayToConsole;
    private enumBoolYesNo displayAceSquad;
    private string aceSquadMessage;
    private string aceSquadDisplayType;
    private enumBoolYesNo displayAceSquadToConsole;

    private enumBoolYesNo enableKillStreaks;
    private int killStreakDisplayStart;
    private int killStreakDisplayInterval;
    private string killStreakMessage;
    private string killStreakDisplayType;
    private List<string> killStreakMessageList;
    private int endKillStreakNumber;
    private string endKillStreakMessage;
    private enumBoolYesNo displayKillStreaksToConsole;
    private int killStreakConsoleValue;

    private enumBoolYesNo enableDeathStreaks;
    private int deathStreakDisplayStart;
    private int deathStreakDisplayInterval;
    private string deathStreakMessage;
    private string deathStreakDisplayType;
    private List<string> deathStreakMessageList;
    private int endDeathStreakNumber;
    private string endDeathStreakMessage;
    private enumBoolYesNo displayDeathStreaksToConsole;
    private int deathStreakConsoleValue;

    private string addKillType;
    private string announcementDisplayType;
    private List<string> killTypeList;
    private enumBoolYesNo displayAnnouncementToConsole;

    private int serverKills;
    private int knifeKills;
    private int defibKills;
    private int repairToolKills;
    private List<int> weaponKills;
    private List<string> formattedKillsMessages;

    private enumBoolYesNo displayFirstBlood;
    private string firstBloodMessage;
    private string firstBloodDisplayType;
    private enumBoolYesNo displayFirstBloodToConsole;

    private enumBoolYesNo displayMultikills;
    private List<string> multikillMessageList;
    private int multikillWindow;
    private enumBoolYesNo displayMultikillToConsole;
    private int multikillConsoleValue;

    private Kill lastKill;
    private bool bRoundEnded;
    private bool bFirstBlood;
    private string mostKillsPlayer;
    private Dictionary<string, int> mostKnifeKills;
    private Dictionary<string, int> killStreak;
    private string highestKillStreak;
    private int iHighestKillStreak;
    private Dictionary<string, int> deathStreak;
    private string highestDeathStreak;
    private int iHighestDeathStreak;
    private List<string> formattedMessages;
    private Dictionary<string, List<int>> playerWeaponKills;
    private List<int> weaponsMostKills;
    private List<string> weaponsMostKillsPlayer;
    private int highestSquadScore;
    private int aceSquadTeamID;
    private int aceSquadSquadID;
    private List<string> aceSquadPlayers;
    private string aceSquadPlayersString;
    private string gameMode;
    private bool needServerInfo;
    private string originalServerMessage;
    private bool needOriginalMessage;
    private bool needMessageReset;
    private string highestScorePlayer;
    private string mostDeathsPlayer;
    private Dictionary<string, int> playerMultikill;
    private List<string> multikillAnnounceQueue;
    private List<string> weaponNames;
    private List<string> weaponsLocalized;
    private List<string> weaponCodes;
    private string gameServerType;
    private CMap currentMapName;
    private List<string> teamNames;

    private OdbcConnection odbcConnection;
    private string sqlCommand;

    private bool pluginEnabled = false;
    Thread newThread;

    public CServerKills()
    {
      this.enableServerKills = enumBoolYesNo.No;
      this.dbHost = "";
      this.dbPort = "";
      this.dbName = "";
      this.dbUser = "";
      this.dbPassword = "";
      this.dbTable = "";
      this.displayInterval = 60;
      this.killsDisplayedTimespan = 24;
      this.addWeaponServerKills = "...";
      this.killsMessageDisplay = new List<string>();
      this.displayKillsToConsole = enumBoolYesNo.No;

      this.enableRoundStats = enumBoolYesNo.Yes;
      this.addWeaponRoundStats = "...";
      this.messageDisplay = new List<string>();
      this.displayToConsole = enumBoolYesNo.No;
      this.displayAceSquad = enumBoolYesNo.Yes;
      this.aceSquadMessage = "Ace Squad - %players% in %squad% Squad on Team %team% with %score% points!";
      this.aceSquadDisplayType = "yell";
      this.displayAceSquadToConsole = enumBoolYesNo.No;

      this.enableKillStreaks = enumBoolYesNo.Yes;
      this.killStreakDisplayStart = 10;
      this.killStreakDisplayInterval = 5;
      this.killStreakMessage = "%pn% is on a %ksn% kill streak!";
      this.killStreakDisplayType = "say";
      this.killStreakMessageList = new List<string>();
      this.initKillStreakMessages();
      this.endKillStreakNumber = 10;
      this.endKillStreakMessage = "%ksepn% has ended %pn%'s kill streak at %ksn% kills!";
      this.displayKillStreaksToConsole = enumBoolYesNo.No;
      this.killStreakConsoleValue = 15;

      this.enableDeathStreaks = enumBoolYesNo.No;
      this.deathStreakDisplayStart = 10;
      this.deathStreakDisplayInterval = 5;
      this.deathStreakMessage = "Someone PLEASE show %pn% how to use a gun. He has died %dsn% times in a row!";
      this.deathStreakDisplayType = "say";
      this.deathStreakMessageList = new List<string>();
      this.initDeathStreakMessages();
      this.endDeathStreakNumber = 10;
      this.endDeathStreakMessage = "%dsepn% was kind enough to grant %pn% a kill on him after he died %dsn% times in a row.";
      this.displayDeathStreaksToConsole = enumBoolYesNo.No;
      this.deathStreakConsoleValue = 20;

      this.addKillType = "...";
      this.announcementDisplayType = "say";
      this.killTypeList = new List<string>();
      this.killTypeList.Add("Defibrillator|WOAH! %kpn% just ZAPPED %vpn% to death with his %kt%!");
      this.killTypeList.Add("Repair Tool|OUCH! %kpn% just killed %vpn% with a %kt%!");
      this.displayAnnouncementToConsole = enumBoolYesNo.No;

      this.serverKills = 0;
      this.knifeKills = 0;
      this.defibKills = 0;
      this.repairToolKills = 0;
      this.weaponKills = new List<int>();
      this.formattedKillsMessages = new List<string>();

      this.displayFirstBlood = enumBoolYesNo.Yes;
      this.firstBloodMessage = "%kfb% has killed %vfb% with a %wfb% for FIRST BLOOD!";
      this.firstBloodDisplayType = "say";
      this.displayFirstBloodToConsole = enumBoolYesNo.No;

      this.displayMultikills = enumBoolYesNo.Yes;
      this.multikillMessageList = new List<string>();
      this.initMultikillMessages();
      this.multikillWindow = 1;
      this.displayMultikillToConsole = enumBoolYesNo.No;
      this.multikillConsoleValue = 5;

      this.lastKill = null;
      this.bRoundEnded = false;
      this.bFirstBlood = false;
      this.mostKillsPlayer = "";
      this.mostKnifeKills = new Dictionary<string, int>();
      this.killStreak = new Dictionary<string, int>();
      this.highestKillStreak = "";
      this.iHighestKillStreak = 0;
      this.deathStreak = new Dictionary<string, int>();
      this.highestDeathStreak = "";
      this.iHighestDeathStreak = 0;
      this.formattedMessages = new List<string>();
      this.playerWeaponKills = new Dictionary<string, List<int>>();
      this.weaponsMostKills = new List<int>();
      this.weaponsMostKillsPlayer = new List<string>();
      this.aceSquadPlayers = new List<string>();
      this.aceSquadPlayersString = "";
      this.needServerInfo = false;
      this.needOriginalMessage = false;
      this.needMessageReset = false;
      this.highestScorePlayer = "";
      this.mostDeathsPlayer = "";
      this.playerMultikill = new Dictionary<string, int>();
      this.multikillAnnounceQueue = new List<string>();
      this.weaponNames = new List<string>();
      this.weaponsLocalized = new List<string>();
      this.weaponCodes = new List<string>();
      this.gameServerType = "";
      this.currentMapName = new CMap();
      this.teamNames = new List<string>();

      this.odbcConnection = new OdbcConnection();
      this.sqlCommand = "";
    }

    public string GetPluginName()
    {
      return "Server Kills";
    }

    public string GetPluginVersion()
    {
      return "1.5.0.0";
    }

    public string GetPluginAuthor()
    {
      return "TimSad";
    }

    public string GetPluginWebsite()
    {
      return "forum.myrcon.com/showthread.php?4487";
    }

    public string GetPluginDescription()
    {
      return @"
        <h2>Description</h2>
          <p>This plugin has multiple server kill type functions... It has kill streaks, death streaks, multikills, first blood, specific kill announcements, and round stats that are displayed at the beginning of each next round. It also has a ""Server Kills Report"" which displays the server's 
            kills of various types over a span of X amount of hours defined by <i>Kills Displayed Timespan (hours)</i>.</p>
        <h2>Requirements</h2>
          <p>For the <b>Server Kills Report</b> to work, you will need to have the <a href=""http://dev.mysql.com/downloads/connector/odbc/5.1.html"" target=""_blank"">MySQL ODBC 5.1 Driver</a> installed on the computer that your Procon or Procon Layer is 
            being run on. Also, if you have the Procon option ""Run plugins in a sandbox"" selected, you will need to make sure to select <i>Allow all outgoing ODBC connections</i>.</p>
        <h2>Settings</h2>
          <h3>Death Streaks</h3>
            <blockquote><h4>Enable Death Streaks?</h4>Allows you to enable or disable the displaying of death streaks.</blockquote>
            <blockquote><h4>Death Streak Start Number</h4>The number of deaths in a row that a player must reach before death streak messages start displaying at an interval of a death streak number.</blockquote>
            <blockquote><h4>Death Streak Display Interval</h4>The recurrent number of deaths in a row a player must get after the <b>Death Streak Start Number</b> for a death streak message to display again.</blockquote>
            <blockquote><h4>Death Streak Default Message</h4>The default message that is displayed for death streaks.
              <br \>" + this.getTable(7) + @"</blockquote>
            <blockquote><h4>Death Streak Display Type</h4>Determines whether death streak messages are displayed as a ""Say"" or a ""Yell"".</blockquote>
            <blockquote><h4>Additional Death Streak Messages</h4>Additional alternate messages for any specific death streak number. The <b>Death Streak Default Message</b> will not be displayed for these numbers. Format is... <i>number</i>|<i>message</i>
              <br \>" + this.getTable(7) + @"</blockquote>
            <blockquote><h4>End Death Streak Number</h4>The death streak number a player must have met or passed for the <b>End Death Streak Message</b> to be displayed when the player finally gets a kill. Set to <i>0</i> to disable.</blockquote>
            <blockquote><h4>End Death Streak Message</h4>The message that is displayed when a player who has met or passed the <b>End Death Streak Number</b> finally gets a kill.
              <br \>" + this.getTable(8) + @"</blockquote>
            <blockquote><h4>Write Death Streaks to Console?</h4>Writes the Death Streak Messages to console if set to <i>Yes</i>.</blockquote>
            <blockquote><h4>Death Streak Console Write Start Number</h4>The Death Streak number a player must reach for the Death Streak Messages to be written to the console.</blockquote>
          <h3>First Blood</h3>
            <blockquote><h4>Display First Blood?</h4>If set to ""Yes"" it will display the first blood (first kill) of each round.</blockquote>
            <blockquote><h4>First Blood Message</h4>The customizable message that will be displayed when a player achieves the first blood of a round.
              <br \>" + this.getTable(9) + @"</blockquote>
            <blockquote><h4>First Blood Display Type</h4>Determines whether the <b>First Blood Message</b> is displayed as a ""Say"" or a ""Yell"".</blockquote>
            <blockquote><h4>Write First Blood to Console?</h4>Writes the First Blood Messages to console if set to <i>Yes</i>.</blockquote>
          <h3>Kill Streaks</h3>
            <blockquote><h4>Enable Kill Streaks?</h4>Allows you to enable or disable the displaying of kill streaks.</blockquote>
            <blockquote><h4>Kill Streak Start Number</h4>The number of kills in a row that a player must reach before kill streak messages start displaying at an interval of a kill streak number.</blockquote>
            <blockquote><h4>Kill Streak Display Interval</h4>The recurrent number of kills in a row a player must get after the <b>Kill Streak Start Number</b> for a kill streak message to display again.</blockquote>
            <blockquote><h4>Default Message</h4>The default message that is displayed for kill streaks.
              <br \>" + this.getTable(1) + @"</blockquote>
            <blockquote><h4>Display Type</h4>Determines whether kill streak messages are displayed as a ""Say"" or a ""Yell"".</blockquote>
            <blockquote><h4>Additional Messages</h4>Additional alternate messages for any specific kill streak number. The <b>Default Message</b> will not be displayed for these numbers. Format is... <i>number</i>|<i>message</i>
              <br \>" + this.getTable(1) + @"</blockquote>
            <blockquote><h4>End Kill Streak Number</h4>The kill streak number a player must have met or passed for the <b>End Kill Streak Message</b> to be displayed when the player is killed. Set to <i>0</i> to disable.</blockquote>
            <blockquote><h4>End Kill Streak Message</h4>The message that is displayed when a player who has met or passed the <b>End Kill Streak Number</b> is killed.
              <br \>" + this.getTable(2) + @"</blockquote>
            <blockquote><h4>Write Kill Streaks to Console?</h4>Writes the Kill Streak Messages to console if set to <i>Yes</i>.</blockquote>
            <blockquote><h4>Kill Streak Console Write Start Number</h4>The Kill Streak number a player must reach for the Kill Streak Messages to be written to the console.</blockquote>
          <h3>Multikills</h3>
            <blockquote><h4>Enable Multkills?</h4>Allows you to enable or disable the displaying of multikills.</blockquote>
            <blockquote><h4>Multikill Messages</h4>The specific messages that will be displayed when a player gets a multikill. Format is... <i>number</i>|<i>message</i>|<i>display type (say or yell)</i>
              <br \>" + this.getTable(10) + @"</blockquote>
            <blockquote><h4>Multikill Span (in seconds)</h4>The time span of seconds defined by you that constitute a multikill. More than 3 seconds is pushing it so you cannot enter a value greater than that.</blockquote>
            <blockquote><h4>Write Multikills to Console?</h4>Writes the Multikill Messages to console if set to <i>Yes</i>.</blockquote>
            <blockquote><h4>Multikill Console Write Start Value</h4>The minimum multikill value that a player must have achieved for it to be written to console.</blockquote>
          <h3>Round Stats</h3>
            <blockquote><h4>Enable Round Stats?</h4>Allows you to enable or disable the displaying of round stats.</blockquote>
            <blockquote><h4>Add Weapon To Display Messages</h4>A drop down list with all the weapons and kill types. Select anything from this list to be added to the <b>Display Messages</b> along with a generic message.</blockquote>
            <blockquote><h4>Display Messages</h4>The customizable messages that are displayed in chat at the beginning of a round for the previous round's stats.
              <br \>" + this.getTable(3) + @"</blockquote>
            <blockquote><h4>Write Display Messages to Console?</h4>Writes the messages to console if set to <i>Yes</i>.</blockquote>
            <blockquote><h4>Display Ace Squad?</h4>Allows you to enable or disable the displaying of Ace Squad.</blockquote>
            <blockquote><h4>Ace Squad Message</h4>The customizable message that is displayed for Ace Squad at the beginning of each round.
              <br \>" + this.getTable(6) + @"</blockquote>
            <blockquote><h4>Ace Squad Display Type</h4>Determines whether the Ace Squad message is displayed as a ""Say"", a ""Yell"", or uses the <i>vars.serverMessage</i> variable (restored after use) to display the message at the beginning of the round.</blockquote>
            <blockquote><h4>Write Ace Squad to Console?</h4>Writes the Ace Squad message to console if set to <i>Yes</i>.</blockquote>
          <h3>Server Kills Report</h3>
            <blockquote><h4>Enable Server Kills Report?</h4>Allows you to enable or disable the displaying of the Server Kills Report.</blockquote>
            <blockquote><h4>DB Host</h4>Your Database Host.</blockquote>
            <blockquote><h4>DB Port</h4>Your Database Port.</blockquote>
            <blockquote><h4>DB Name</h4>Your Database Name.</blockquote>
            <blockquote><h4>DB User</h4>Your Database User.</blockquote>
            <blockquote><h4>DB Password</h4>Your Database Password.</blockquote>
            <blockquote><h4>DB Table</h4>Your Database Table for the specific server. For instance... <i>server1</i> or <i>karkand</i> as I did for my 24/7 TDM Karkand server.</blockquote>
            <blockquote><h4>Kills Display Interval (minutes)</h4>The interval of time in minutes, recurringly, that the Server Kills Report is displayed. This is based off of an even hour e.g. <i>10</i> will display at 1:10, 1:20:, 1:30, etc...</blockquote>
            <blockquote><h4>Kills Displayed Timespan (hours)</h4>How far back in hours that the Server Kills Report goes to fetch the data.</blockquote>
            <blockquote><h4>Add Weapon To Kills Display Messages</h4>A drop down list with all the weapons and kill types. Select anything from this list to be added to the <b>Kills Display Messages</b>.</blockquote>
            <blockquote><h4>Kills Display Messages</h4>The customizable messages that are displayed for the Server Kills Report.
              <br \>" + this.getTable(4) + @"</blockquote>
            <blockquote><h4>Write Kills Display Messages to Console?</h4>Writes the messages to console if set to <i>Yes</i>.</blockquote>
          <h3>Specific Kill Announcements</h3>
            <blockquote><h4>Add Kill Type</h4>A drop down list with all the weapons and kill types. Select anything from this list to be added to the <b>Kill Announcement List</b> along with a generic message.</blockquote>
            <blockquote><h4>Announcement Display Type</h4>Determines whether specific kill announcement messages are displayed as a ""Say"" or a ""Yell"".</blockquote>
            <blockquote><h4>Kill Announcement List</h4>The list of weapons or kill types specified by you associated with their individual messages. Format is... <i>weapon/killType</i>|<i>message</i>
              <br \>" + this.getTable(5) + @"</blockquote>
            <blockquote><h4>Write Kill Announcement to Console?</h4>Writes the Kill Announcement Messages to console if set to <i>Yes</i>.</blockquote>
        ";
    }

    public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
    {
      this.RegisterEvents(this.GetType().Name, "OnPlayerKilled", "OnRoundOver", "OnPlayerSpawned", "OnRoundOverPlayers", "OnServerInfo", "OnServerMessage", "OnGlobalChat");
      this.initializeKillsMessages();
      this.initializeMessages();
      this.weaponNames = new List<string>(this.getRawWeaponNames());
      this.weaponsLocalized = new List<string>(GetWeaponList(DamageTypes.None));
      this.weaponCodes = new List<string>(this.getWeaponCodes());
      this.weaponKills = new List<int>(new int[this.weaponNames.Count]);
    }

    public void OnPluginEnable()
    {
      this.ExecuteCommand("procon.protected.pluginconsole.write", "^bServer Kills ^2Enabled!");
      this.pluginEnabled = true;

      if (this.canConnect())
      {
        if (this.odbcConnection.State != ConnectionState.Open)
          this.odbcConnect();

        this.newThread = new Thread(new ThreadStart(this.createTables));
        this.newThread.Start();
        while (!this.newThread.IsAlive) ;
        this.updateSqlInsertEvent();
        this.updateDisplayEvent();
      }
    }

    public void OnPluginDisable()
    {
      this.ExecuteCommand("procon.protected.pluginconsole.write", "^bServer Kills ^1Disabled =(");
      this.pluginEnabled = false;
    }

    public List<CPluginVariable> GetDisplayPluginVariables()
    {
      List<CPluginVariable> lstReturn = new List<CPluginVariable>();

      lstReturn.Add(new CPluginVariable("Server Kills Report|Enable Server Kills Report?", typeof(enumBoolYesNo), this.enableServerKills));
      if (this.enableServerKills == enumBoolYesNo.Yes)
      {
        lstReturn.Add(new CPluginVariable("Server Kills Report|DB Host", this.dbHost.GetType(), this.dbHost));
        lstReturn.Add(new CPluginVariable("Server Kills Report|DB Port", this.dbPort.GetType(), this.dbPort));
        lstReturn.Add(new CPluginVariable("Server Kills Report|DB Name", this.dbName.GetType(), this.dbName));
        lstReturn.Add(new CPluginVariable("Server Kills Report|DB User", this.dbUser.GetType(), this.dbUser));
        lstReturn.Add(new CPluginVariable("Server Kills Report|DB Password", this.dbPassword.GetType(), this.dbPassword));
        lstReturn.Add(new CPluginVariable("Server Kills Report|DB Table", this.dbTable.GetType(), this.dbTable));
        lstReturn.Add(new CPluginVariable("Server Kills Report|Kills Display Interval (minutes)", this.displayInterval.GetType(), this.displayInterval));
        lstReturn.Add(new CPluginVariable("Server Kills Report|Kills Displayed Timespan (hours)", this.killsDisplayedTimespan.GetType(), this.killsDisplayedTimespan));
        lstReturn.Add(new CPluginVariable("Server Kills Report|Add Weapon To Kills Display Messages", this.getKillTypesEnum(), this.addWeaponServerKills));
        lstReturn.Add(new CPluginVariable("Server Kills Report|Kills Display Messages", typeof(string[]), this.killsMessageDisplay.ToArray()));
        lstReturn.Add(new CPluginVariable("Server Kills Report|Write Kills Display Messages to Console?", typeof(enumBoolYesNo), this.displayKillsToConsole));
      }

      lstReturn.Add(new CPluginVariable("Round Stats|Enable Round Stats?", typeof(enumBoolYesNo), this.enableRoundStats));
      if (this.enableRoundStats == enumBoolYesNo.Yes)
      {
        lstReturn.Add(new CPluginVariable("Round Stats|Add Weapon To Display Messages", this.getKillTypesEnum(), this.addWeaponRoundStats));
        lstReturn.Add(new CPluginVariable("Round Stats|Display Messages", typeof(string[]), this.messageDisplay.ToArray()));
        lstReturn.Add(new CPluginVariable("Round Stats|Write Display Messages to Console?", typeof(enumBoolYesNo), this.displayToConsole));
        lstReturn.Add(new CPluginVariable("Round Stats|Display Ace Squad?", typeof(enumBoolYesNo), this.displayAceSquad));
        if (this.displayAceSquad == enumBoolYesNo.Yes)
        {
          lstReturn.Add(new CPluginVariable("Round Stats|Ace Squad Message", this.aceSquadMessage.GetType(), this.aceSquadMessage));
          lstReturn.Add(new CPluginVariable("Round Stats|Ace Squad Display Type", "enum.AceSquadDisplayType(vars.serverMessage|yell|say)", this.aceSquadDisplayType));
          lstReturn.Add(new CPluginVariable("Round Stats|Write Ace Squad to Console?", typeof(enumBoolYesNo), this.displayAceSquadToConsole));
        }
      }

      lstReturn.Add(new CPluginVariable("Kill Streaks|Enable Kill Streaks?", typeof(enumBoolYesNo), this.enableKillStreaks));
      if (this.enableKillStreaks == enumBoolYesNo.Yes)
      {
        lstReturn.Add(new CPluginVariable("Kill Streaks|Kill Streak Start Number", this.killStreakDisplayStart.GetType(), this.killStreakDisplayStart));
        lstReturn.Add(new CPluginVariable("Kill Streaks|Kill Streak Display Interval", this.killStreakDisplayInterval.GetType(), this.killStreakDisplayInterval));
        lstReturn.Add(new CPluginVariable("Kill Streaks|Default Message", this.killStreakMessage.GetType(), this.killStreakMessage));
        lstReturn.Add(new CPluginVariable("Kill Streaks|Display Type", "enum.DisplayType(say|yell)", this.killStreakDisplayType));
        lstReturn.Add(new CPluginVariable("Kill Streaks|Additional Messages", typeof(string[]), this.killStreakMessageList.ToArray()));
        lstReturn.Add(new CPluginVariable("Kill Streaks|End Kill Streak Number", this.endKillStreakNumber.GetType(), this.endKillStreakNumber));
        lstReturn.Add(new CPluginVariable("Kill Streaks|End Kill Streak Message", this.endKillStreakMessage.GetType(), this.endKillStreakMessage));
        lstReturn.Add(new CPluginVariable("Kill Streaks|Write Kill Streaks to Console?", typeof(enumBoolYesNo), this.displayKillStreaksToConsole));
        if (this.displayKillStreaksToConsole == enumBoolYesNo.Yes)
          lstReturn.Add(new CPluginVariable("Kill Streaks|Kill Streak Console Write Start Number", this.killStreakConsoleValue.GetType(), this.killStreakConsoleValue));
      }

      lstReturn.Add(new CPluginVariable("Death Streaks|Enable Death Streaks?", typeof(enumBoolYesNo), this.enableDeathStreaks));
      if (this.enableDeathStreaks == enumBoolYesNo.Yes)
      {
        lstReturn.Add(new CPluginVariable("Death Streaks|Death Streak Start Number", this.deathStreakDisplayStart.GetType(), this.deathStreakDisplayStart));
        lstReturn.Add(new CPluginVariable("Death Streaks|Death Streak Display Interval", this.deathStreakDisplayInterval.GetType(), this.deathStreakDisplayInterval));
        lstReturn.Add(new CPluginVariable("Death Streaks|Death Streak Default Message", this.deathStreakMessage.GetType(), this.deathStreakMessage));
        lstReturn.Add(new CPluginVariable("Death Streaks|Death Streak Display Type", "enum.DisplayType(say|yell)", this.deathStreakDisplayType));
        lstReturn.Add(new CPluginVariable("Death Streaks|Additional Death Streak Messages", typeof(string[]), this.deathStreakMessageList.ToArray()));
        lstReturn.Add(new CPluginVariable("Death Streaks|End Death Streak Number", this.endDeathStreakNumber.GetType(), this.endDeathStreakNumber));
        lstReturn.Add(new CPluginVariable("Death Streaks|End Death Streak Message", this.endDeathStreakMessage.GetType(), this.endDeathStreakMessage));
        lstReturn.Add(new CPluginVariable("Death Streaks|Write Death Streaks to Console?", typeof(enumBoolYesNo), this.displayDeathStreaksToConsole));
        if (this.displayDeathStreaksToConsole == enumBoolYesNo.Yes)
          lstReturn.Add(new CPluginVariable("Death Streaks|Death Streak Console Write Start Number", this.deathStreakConsoleValue.GetType(), this.deathStreakConsoleValue));
      }

      lstReturn.Add(new CPluginVariable("Specific Kill Announcements|Add Kill Type", this.getKillTypesEnum(), this.addKillType));
      lstReturn.Add(new CPluginVariable("Specific Kill Announcements|Announcement Display Type", "enum.DisplayType(say|yell)", this.announcementDisplayType));
      lstReturn.Add(new CPluginVariable("Specific Kill Announcements|Kill Announcement List", typeof(string[]), this.killTypeList.ToArray()));
      lstReturn.Add(new CPluginVariable("Specific Kill Announcements|Write Kill Announcement to Console?", typeof(enumBoolYesNo), this.displayAnnouncementToConsole));

      lstReturn.Add(new CPluginVariable("First Blood|Display First Blood?", typeof(enumBoolYesNo), this.displayFirstBlood));
      if (this.displayFirstBlood == enumBoolYesNo.Yes)
      {
        lstReturn.Add(new CPluginVariable("First Blood|First Blood Message", this.firstBloodMessage.GetType(), this.firstBloodMessage));
        lstReturn.Add(new CPluginVariable("First Blood|First Blood Display Type", "enum.DisplayType(say|yell)", this.firstBloodDisplayType));
        lstReturn.Add(new CPluginVariable("First Blood|Write First Blood to Console?", typeof(enumBoolYesNo), this.displayFirstBloodToConsole));
      }

      lstReturn.Add(new CPluginVariable("Multikills|Enable Multikills?", typeof(enumBoolYesNo), this.displayMultikills));
      if (this.displayMultikills == enumBoolYesNo.Yes)
      {
        lstReturn.Add(new CPluginVariable("Multikills|Multikill Messages", typeof(string[]), this.multikillMessageList.ToArray()));
        lstReturn.Add(new CPluginVariable("Multikills|Multikill Span (in seconds)", this.multikillWindow.GetType(), this.multikillWindow));
        lstReturn.Add(new CPluginVariable("Multikills|Write Multikills to Console?", typeof(enumBoolYesNo), this.displayMultikillToConsole));
        if (this.displayMultikillToConsole == enumBoolYesNo.Yes)
          lstReturn.Add(new CPluginVariable("Multikills|Multikill Console Write Start Value", this.multikillConsoleValue.GetType(), this.multikillConsoleValue));
      }
      
      return lstReturn;
    }

    public List<CPluginVariable> GetPluginVariables()
    {
      List<CPluginVariable> lstReturn = new List<CPluginVariable>();

      lstReturn.Add(new CPluginVariable("Enable Server Kills Report?", typeof(enumBoolYesNo), this.enableServerKills));
      lstReturn.Add(new CPluginVariable("DB Host", this.dbHost.GetType(), this.dbHost));
      lstReturn.Add(new CPluginVariable("DB Port", this.dbPort.GetType(), this.dbPort));
      lstReturn.Add(new CPluginVariable("DB Name", this.dbName.GetType(), this.dbName));
      lstReturn.Add(new CPluginVariable("DB User", this.dbUser.GetType(), this.dbUser));
      lstReturn.Add(new CPluginVariable("DB Password", this.dbPassword.GetType(), this.dbPassword));
      lstReturn.Add(new CPluginVariable("DB Table", this.dbTable.GetType(), this.dbTable));
      lstReturn.Add(new CPluginVariable("Kills Display Interval (minutes)", this.displayInterval.GetType(), this.displayInterval));
      lstReturn.Add(new CPluginVariable("Kills Displayed Timespan (hours)", this.killsDisplayedTimespan.GetType(), this.killsDisplayedTimespan));
      lstReturn.Add(new CPluginVariable("Add Weapon To Kills Display Messages", this.getKillTypesEnum(), this.addWeaponServerKills));
      lstReturn.Add(new CPluginVariable("Kills Display Messages", typeof(string[]), this.killsMessageDisplay.ToArray()));
      lstReturn.Add(new CPluginVariable("Write Kills Display Messages to Console?", typeof(enumBoolYesNo), this.displayKillsToConsole));

      lstReturn.Add(new CPluginVariable("Enable Round Stats?", typeof(enumBoolYesNo), this.enableRoundStats));
      lstReturn.Add(new CPluginVariable("Add Weapon To Display Messages", this.getKillTypesEnum(), this.addWeaponRoundStats));
      lstReturn.Add(new CPluginVariable("Display Messages", typeof(string[]), this.messageDisplay.ToArray()));
      lstReturn.Add(new CPluginVariable("Write Display Messages to Console?", typeof(enumBoolYesNo), this.displayToConsole));
      lstReturn.Add(new CPluginVariable("Display Ace Squad?", typeof(enumBoolYesNo), this.displayAceSquad));
      lstReturn.Add(new CPluginVariable("Ace Squad Message", this.aceSquadMessage.GetType(), this.aceSquadMessage));
      lstReturn.Add(new CPluginVariable("Ace Squad Display Type", "enum.AceSquadDisplayType(vars.serverMessage|yell|say)", this.aceSquadDisplayType));
      lstReturn.Add(new CPluginVariable("Write Ace Squad to Console?", typeof(enumBoolYesNo), this.displayAceSquadToConsole));

      lstReturn.Add(new CPluginVariable("Enable Kill Streaks?", typeof(enumBoolYesNo), this.enableKillStreaks));
      lstReturn.Add(new CPluginVariable("Kill Streak Start Number", this.killStreakDisplayStart.GetType(), this.killStreakDisplayStart));
      lstReturn.Add(new CPluginVariable("Kill Streak Display Interval", this.killStreakDisplayInterval.GetType(), this.killStreakDisplayInterval));
      lstReturn.Add(new CPluginVariable("Default Message", this.killStreakMessage.GetType(), this.killStreakMessage));
      lstReturn.Add(new CPluginVariable("Display Type", "enum.DisplayType(say|yell)", this.killStreakDisplayType));
      lstReturn.Add(new CPluginVariable("Additional Messages", typeof(string[]), this.killStreakMessageList.ToArray()));
      lstReturn.Add(new CPluginVariable("End Kill Streak Number", this.endKillStreakNumber.GetType(), this.endKillStreakNumber));
      lstReturn.Add(new CPluginVariable("End Kill Streak Message", this.endKillStreakMessage.GetType(), this.endKillStreakMessage));
      lstReturn.Add(new CPluginVariable("Write Kill Streaks to Console?", typeof(enumBoolYesNo), this.displayKillStreaksToConsole));
      lstReturn.Add(new CPluginVariable("Kill Streak Console Write Start Number", this.killStreakConsoleValue.GetType(), this.killStreakConsoleValue));

      lstReturn.Add(new CPluginVariable("Enable Death Streaks?", typeof(enumBoolYesNo), this.enableDeathStreaks));
      lstReturn.Add(new CPluginVariable("Death Streak Start Number", this.deathStreakDisplayStart.GetType(), this.deathStreakDisplayStart));
      lstReturn.Add(new CPluginVariable("Death Streak Display Interval", this.deathStreakDisplayInterval.GetType(), this.deathStreakDisplayInterval));
      lstReturn.Add(new CPluginVariable("Death Streak Default Message", this.deathStreakMessage.GetType(), this.deathStreakMessage));
      lstReturn.Add(new CPluginVariable("Death Streak Display Type", "enum.DisplayType(say|yell)", this.deathStreakDisplayType));
      lstReturn.Add(new CPluginVariable("Additional Death Streak Messages", typeof(string[]), this.deathStreakMessageList.ToArray()));
      lstReturn.Add(new CPluginVariable("End Death Streak Number", this.endDeathStreakNumber.GetType(), this.endDeathStreakNumber));
      lstReturn.Add(new CPluginVariable("End Death Streak Message", this.endDeathStreakMessage.GetType(), this.endDeathStreakMessage));
      lstReturn.Add(new CPluginVariable("Write Death Streaks to Console?", typeof(enumBoolYesNo), this.displayDeathStreaksToConsole));
      lstReturn.Add(new CPluginVariable("Death Streak Console Write Start Number", this.deathStreakConsoleValue.GetType(), this.deathStreakConsoleValue));

      lstReturn.Add(new CPluginVariable("Add Kill Type", this.getKillTypesEnum(), this.addKillType));
      lstReturn.Add(new CPluginVariable("Announcement Display Type", "enum.DisplayType(say|yell)", this.announcementDisplayType));
      lstReturn.Add(new CPluginVariable("Kill Announcement List", typeof(string[]), this.killTypeList.ToArray()));
      lstReturn.Add(new CPluginVariable("Write Kill Announcement to Console?", typeof(enumBoolYesNo), this.displayAnnouncementToConsole));

      lstReturn.Add(new CPluginVariable("Display First Blood?", typeof(enumBoolYesNo), this.displayFirstBlood));
      lstReturn.Add(new CPluginVariable("First Blood Message", this.firstBloodMessage.GetType(), this.firstBloodMessage));
      lstReturn.Add(new CPluginVariable("First Blood Display Type", "enum.DisplayType(say|yell)", this.firstBloodDisplayType));
      lstReturn.Add(new CPluginVariable("Write First Blood to Console?", typeof(enumBoolYesNo), this.displayFirstBloodToConsole));

      lstReturn.Add(new CPluginVariable("Enable Multikills?", typeof(enumBoolYesNo), this.displayMultikills));
      lstReturn.Add(new CPluginVariable("Multikill Messages", typeof(string[]), this.multikillMessageList.ToArray()));
      lstReturn.Add(new CPluginVariable("Multikill Span (in seconds)", this.multikillWindow.GetType(), this.multikillWindow));
      lstReturn.Add(new CPluginVariable("Write Multikills to Console?", typeof(enumBoolYesNo), this.displayMultikillToConsole));
      lstReturn.Add(new CPluginVariable("Multikill Console Write Start Value", this.multikillConsoleValue.GetType(), this.multikillConsoleValue));

      return lstReturn;
    }

    public void SetPluginVariable(string strVariable, string strValue)
    {
      if (strVariable.CompareTo("Enable Server Kills Report?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
      {
        this.enableServerKills = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);

        if (this.canConnect())
        {
          if (this.odbcConnection.State != ConnectionState.Open)
            this.odbcConnect();

          this.newThread = new Thread(new ThreadStart(this.createTables));
          this.newThread.Start();
          while (!this.newThread.IsAlive) ;
          this.updateSqlInsertEvent();
          this.updateDisplayEvent();
        }
      }
      else if (strVariable.CompareTo("DB Host") == 0)
      {
        this.dbHost = strValue;

        if (this.canConnect())
        {
          if (this.odbcConnection.State != ConnectionState.Open)
            this.odbcConnect();

          this.newThread = new Thread(new ThreadStart(this.createTables));
          this.newThread.Start();
          while (!this.newThread.IsAlive) ;
          this.updateSqlInsertEvent();
          this.updateDisplayEvent();
        }
      }
      else if (strVariable.CompareTo("DB Port") == 0)
      {
        this.dbPort = strValue;

        if (this.canConnect())
        {
          if (this.odbcConnection.State != ConnectionState.Open)
            this.odbcConnect();

          this.newThread = new Thread(new ThreadStart(this.createTables));
          this.newThread.Start();
          while (!this.newThread.IsAlive) ;
          this.updateSqlInsertEvent();
          this.updateDisplayEvent();
        }
      }
      else if (strVariable.CompareTo("DB Name") == 0)
      {
        this.dbName = strValue;

        if (this.canConnect())
        {
          if (this.odbcConnection.State != ConnectionState.Open)
            this.odbcConnect();

          this.newThread = new Thread(new ThreadStart(this.createTables));
          this.newThread.Start();
          while (!this.newThread.IsAlive) ;
          this.updateSqlInsertEvent();
          this.updateDisplayEvent();
        }
      }
      else if (strVariable.CompareTo("DB User") == 0)
      {
        this.dbUser = strValue;

        if (this.canConnect())
        {
          if (this.odbcConnection.State != ConnectionState.Open)
            this.odbcConnect();

          this.newThread = new Thread(new ThreadStart(this.createTables));
          this.newThread.Start();
          while (!this.newThread.IsAlive) ;
          this.updateSqlInsertEvent();
          this.updateDisplayEvent();
        }
      }
      else if (strVariable.CompareTo("DB Password") == 0)
      {
        this.dbPassword = strValue;

        if (this.canConnect())
        {
          if (this.odbcConnection.State != ConnectionState.Open)
            this.odbcConnect();

          this.newThread = new Thread(new ThreadStart(this.createTables));
          this.newThread.Start();
          while (!this.newThread.IsAlive);
          this.updateSqlInsertEvent();
          this.updateDisplayEvent();
        }
      }
      else if (strVariable.CompareTo("DB Table") == 0)
      {
        this.dbTable = strValue;

        if (this.canConnect())
        {
          if (this.odbcConnection.State != ConnectionState.Open)
            this.odbcConnect();

          this.newThread = new Thread(new ThreadStart(this.createTables));
          this.newThread.Start();
          while (!this.newThread.IsAlive) ;
          this.updateSqlInsertEvent();
          this.updateDisplayEvent();
        }
      }
      else if (strVariable.CompareTo("Kills Display Interval (minutes)") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        this.displayInterval = valueAsInt;

        this.updateDisplayEvent();
      }
      else if (strVariable.CompareTo("Kills Displayed Timespan (hours)") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        this.killsDisplayedTimespan = valueAsInt;
      }
      else if (strVariable.CompareTo("Add Weapon To Kills Display Messages") == 0)
      {
        if (strValue != "...")
        {
          int weaponIndex = this.weaponsLocalized.IndexOf(strValue);
          string message = this.weaponsLocalized[weaponIndex] + " Kills: %" + this.weaponCodes[weaponIndex] + "%";

          if (this.killsMessageDisplay.Count == 1 && this.killsMessageDisplay[0] == "")
          {
            this.killsMessageDisplay[0] = message;
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^0" + strValue + " ^nhas been added to Kills Display Messages as \"^b" + message + "^n\"");
          }
          else
          {
            this.killsMessageDisplay.Add(message);
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^0" + strValue + " ^nhas been added to Kills Display Messages as \"^b" + message + "^n\"");

            if (this.killsMessageDisplay.Count > 5)
              this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^8WARNING! ^0- ^bKills Display Messages ^nhas exceeded 5 lines of chat! At least 1 line will flow off the screen in game...");
          }
        }
      }
      else if (strVariable.CompareTo("Kills Display Messages") == 0)
      {
        this.killsMessageDisplay = new List<string>(CPluginVariable.DecodeStringArray(strValue));

        if (this.killsMessageDisplay.Count == 1 && this.killsMessageDisplay[0] == "")
        {
          this.killsMessageDisplay.Clear();
          this.initializeKillsMessages();
        }
        else if (this.killsMessageDisplay.Count > 5)
          this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^8WARNING! ^0- ^bKills Display Messages ^nhas exceeded 5 lines of chat! At least 1 line will flow off the screen in game...");
      }
      else if (strVariable.CompareTo("Write Kills Display Messages to Console?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
      {
        this.displayKillsToConsole = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
      }
      else if (strVariable.CompareTo("Enable Round Stats?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
      {
        this.enableRoundStats = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
      }
      else if (strVariable.CompareTo("Add Weapon To Display Messages") == 0)
      {
        if (strValue != "...")
        {
          int weaponIndex = this.weaponsLocalized.IndexOf(strValue);
          string message = "Most " + this.weaponsLocalized[weaponIndex] + " Kills: %" + this.weaponCodes[weaponIndex] + "_player% with %" + this.weaponCodes[weaponIndex] + "% kills!";

          if (this.messageDisplay.Count == 1 && this.messageDisplay[0] == "")
          {
            this.messageDisplay[0] = message;
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^0" + strValue + " ^nhas been added to Display Messages as \"^b" + message + "^n\"");
          }
          else
          {
            this.messageDisplay.Add(message);
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^0" + strValue + " ^nhas been added to Display Messages as \"^b" + message + "^n\"");

            if (this.messageDisplay.Count > 5)
              this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^8WARNING! ^0- ^bDisplay Messages ^nhas exceeded 5 lines of chat! At least 1 line will flow off the screen in game...");
          }
        }
      }
      else if (strVariable.CompareTo("Display Messages") == 0)
      {
        this.messageDisplay = new List<string>(CPluginVariable.DecodeStringArray(strValue));

        if (this.messageDisplay.Count == 1 && this.messageDisplay[0] == "")
        {
          this.messageDisplay.Clear();
          this.initializeMessages();
        }
        else if (this.messageDisplay.Count > 5)
          this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^8WARNING! ^0- ^bDisplay Messages ^nhas exceeded 5 lines of chat! At least 1 line will flow off the screen in game...");
      }
      else if (strVariable.CompareTo("Write Display Messages to Console?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
      {
        this.displayToConsole = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
      }
      else if (strVariable.CompareTo("Display Ace Squad?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
      {
        this.displayAceSquad = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
      }
      else if (strVariable.CompareTo("Ace Squad Message") == 0)
      {
        this.aceSquadMessage = strValue;
      }
      else if (strVariable.CompareTo("Ace Squad Display Type") == 0)
      {
        this.aceSquadDisplayType = strValue;
      }
      else if (strVariable.CompareTo("Write Ace Squad to Console?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
      {
        this.displayAceSquadToConsole = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
      }
      else if (strVariable.CompareTo("Enable Kill Streaks?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
      {
        this.enableKillStreaks = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
      }
      else if (strVariable.CompareTo("Kill Streak Start Number") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        this.killStreakDisplayStart = valueAsInt;
      }
      else if (strVariable.CompareTo("Kill Streak Display Interval") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        this.killStreakDisplayInterval = valueAsInt;
      }
      else if (strVariable.CompareTo("Default Message") == 0)
      {
        this.killStreakMessage = strValue;
      }
      else if (strVariable.CompareTo("Display Type") == 0)
      {
        this.killStreakDisplayType = strValue;
      }
      else if (strVariable.CompareTo("Additional Messages") == 0)
      {
        this.killStreakMessageList = new List<string>(CPluginVariable.DecodeStringArray(strValue));

        if (this.killStreakMessageList.Count > 1)
        {
          int result;

          for (int i = 0; i < this.killStreakMessageList.Count; i++)
          {
            if (int.TryParse(this.killStreakMessageList[i], out result) && !(int.TryParse(this.killStreakMessageList[i + 1], out result)))
            {
              this.killStreakMessageList[i] += "|" + this.killStreakMessageList[i + 1];
              this.killStreakMessageList.RemoveRange(i + 1, 1);
            }
          }
        }
      }
      else if (strVariable.CompareTo("End Kill Streak Number") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        this.endKillStreakNumber = valueAsInt;
      }
      else if (strVariable.CompareTo("End Kill Streak Message") == 0)
      {
        this.endKillStreakMessage = strValue;
      }
      else if (strVariable.CompareTo("Write Kill Streaks to Console?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
      {
        this.displayKillStreaksToConsole = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
      }
      else if (strVariable.CompareTo("Kill Streak Console Write Start Number") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);

        if (valueAsInt < this.killStreakDisplayStart)
          this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^8WARNING! ^0- This value must be greater than or equal to the ^bKill Streak Start Number ^nvalue. Resetting value to " + this.killStreakConsoleValue + "!");
        else
          this.killStreakConsoleValue = valueAsInt;
      }
      else if (strVariable.CompareTo("Enable Death Streaks?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
      {
        this.enableDeathStreaks = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
      }
      else if (strVariable.CompareTo("Death Streak Start Number") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        this.deathStreakDisplayStart = valueAsInt;
      }
      else if (strVariable.CompareTo("Death Streak Display Interval") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        this.deathStreakDisplayInterval = valueAsInt;
      }
      else if (strVariable.CompareTo("Death Streak Default Message") == 0)
      {
        this.deathStreakMessage = strValue;
      }
      else if (strVariable.CompareTo("Death Streak Display Type") == 0)
      {
        this.deathStreakDisplayType = strValue;
      }
      else if (strVariable.CompareTo("Additional Death Streak Messages") == 0)
      {
        this.deathStreakMessageList = new List<string>(CPluginVariable.DecodeStringArray(strValue));

        if (this.deathStreakMessageList.Count > 1)
        {
          int result;

          for (int i = 0; i < this.deathStreakMessageList.Count; i++)
          {
            if (int.TryParse(this.deathStreakMessageList[i], out result) && !(int.TryParse(this.deathStreakMessageList[i + 1], out result)))
            {
              this.deathStreakMessageList[i] += "|" + this.deathStreakMessageList[i + 1];
              this.deathStreakMessageList.RemoveRange(i + 1, 1);
            }
          }
        }
      }
      else if (strVariable.CompareTo("End Death Streak Number") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);
        this.endDeathStreakNumber = valueAsInt;
      }
      else if (strVariable.CompareTo("End Death Streak Message") == 0)
      {
        this.endDeathStreakMessage = strValue;
      }
      else if (strVariable.CompareTo("Write Death Streaks to Console?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
      {
        this.displayDeathStreaksToConsole = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
      }
      else if (strVariable.CompareTo("Death Streak Console Write Start Number") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);

        if (valueAsInt < this.deathStreakDisplayStart)
          this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^8WARNING! ^0- This value must be greater than or equal to the ^bDeath Streak Start Number ^nvalue. Resetting value to " + this.deathStreakConsoleValue + "!");
        else
          this.deathStreakConsoleValue = valueAsInt;
      }
      else if (strVariable.CompareTo("Add Kill Type") == 0)
      {
        if (strValue != "...")
        {
          if (this.killTypeList.Count == 1 && this.killTypeList[0] == "")
          {
            this.killTypeList[0] = strValue + "|" + "%kpn% has killed %vpn% with a %kt%!";
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^0" + strValue + " ^nhas been added to the Kill Announcement List!");
          }
          else
          {
            this.killTypeList.Add(strValue + "|" + "%kpn% has killed %vpn% with a %kt%!");
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^0" + strValue + " ^nhas been added to the Kill Announcement List!");
          }
        }
      }
      else if (strVariable.CompareTo("Announcement Display Type") == 0)
      {
        this.announcementDisplayType = strValue;
      }
      else if (strVariable.CompareTo("Kill Announcement List") == 0)
      {
        this.killTypeList = new List<string>(CPluginVariable.DecodeStringArray(strValue));

        if (this.killTypeList.Count > 1)
        {
          for (int i = 0; i < this.killTypeList.Count; i++)
          {
            if (isValidKillType(killTypeList[i]) && !(isValidKillType(killTypeList[i + 1])))
            {
              this.killTypeList[i] += "|" + this.killTypeList[i + 1];
              this.killTypeList.RemoveRange(i + 1, 1);
            }
          }
        }
      }
      else if (strVariable.CompareTo("Write Kill Announcement to Console?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
      {
        this.displayAnnouncementToConsole = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
      }
      else if (strVariable.CompareTo("Display First Blood?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
      {
        this.displayFirstBlood = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
      }
      else if (strVariable.CompareTo("First Blood Message") == 0)
      {
        this.firstBloodMessage = strValue;
      }
      else if (strVariable.CompareTo("First Blood Display Type") == 0)
      {
        this.firstBloodDisplayType = strValue;
      }
      else if (strVariable.CompareTo("Write First Blood to Console?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
      {
        this.displayFirstBloodToConsole = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
      }
      else if (strVariable.CompareTo("Enable Multikills?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
      {
        this.displayMultikills = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
      }
      else if (strVariable.CompareTo("Multikill Messages") == 0)
      {
        this.multikillMessageList = new List<string>(CPluginVariable.DecodeStringArray(strValue));

        if (this.multikillMessageList.Count > 1)
        {
          int result;

          for (int i = 0; i < this.multikillMessageList.Count; i++)
          {
            if (int.TryParse(this.multikillMessageList[i], out result) && !(int.TryParse(this.multikillMessageList[i + 1], out result)))
            {
              this.multikillMessageList[i] += "|" + this.multikillMessageList[i + 1] + "|" + this.multikillMessageList[i + 2];
              this.multikillMessageList.RemoveRange(i + 1, 2);
            }
          }
        }
        else if (this.multikillMessageList.Count == 1 && this.multikillMessageList[0] == "")
        {
          this.multikillMessageList.Clear();
          this.initMultikillMessages();
        }
      }
      else if (strVariable.CompareTo("Multikill Span (in seconds)") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);

        if (valueAsInt > 3)
          this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^8WARNING! ^0- ^b" + valueAsInt.ToString() + " seconds^n is too long of a time span to be considered a multikill! Value reset to " + this.multikillWindow.ToString() + "!");
        else
          this.multikillWindow = valueAsInt;
      }
      else if (strVariable.CompareTo("Write Multikills to Console?") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
      {
        this.displayMultikillToConsole = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
      }
      else if (strVariable.CompareTo("Multikill Console Write Start Value") == 0)
      {
        int valueAsInt;
        int.TryParse(strValue, out valueAsInt);

        if (valueAsInt > 1)
          this.multikillConsoleValue = valueAsInt;
        else
          this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^8WARNING! ^0- Value must be greater than 1!");
      }
    }

    public void OnPluginLoadingEnv(List<string> lstPluginEnv)
    {
      this.gameServerType = lstPluginEnv[1].ToLower();
      this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^0" + this.gameServerType);
    }

    private string getTable(int callNum)
    {
      if (callNum == 1 || callNum == 2)
      {
        string killStreaksTable = @"<table><tr><th style=""background-color:#3a9bd0;"">Item</th><th style=""background-color:#3a9bd0;"">Replacement Tag</th></tr>";

        if (callNum == 2)
          killStreaksTable += "<tr><th>Kill Streak End Player Name (name of the player who ended the kill streak)</th><td>%ksepn%</td></tr>";

        killStreaksTable += "<tr><th>Player Name (name of the player who is on the kill streak)</th><td>%pn%</td></tr>";
        killStreaksTable += "<tr><th>Kill Streak Number</th><td>%ksn%</td></tr></table>";

        return killStreaksTable;
      }
      else if (callNum == 3)
      {
        string roundStatsTable = @"<table><tr><th style=""background-color:#3a9bd0;"">Item</th><th style=""background-color:#3a9bd0;"">Replacement Tag</th></tr>";
        roundStatsTable += "<tr><th>Last Kill Killer</th><td>%lkk%</td></tr>";
        roundStatsTable += "<tr><th>Last Kill Victim</th><td>%lkv%</td></tr>";
        roundStatsTable += "<tr><th>Last Kill Weapon</th><td>%lkw%</td></tr>";
        roundStatsTable += "<tr><th>Most Kills Player</th><td>%mkp%</td></tr>";
        roundStatsTable += "<tr><th>Most Kills Number</th><td>%mkn%</td></tr>";
        roundStatsTable += "<tr><th>Most Knife Kills Player ('Knife' & 'Melee' combined)</th><td>%mkkp%</td></tr>";
        roundStatsTable += "<tr><th>Most Knife Kills Number ('Knife' & 'Melee' combined)</th><td>%mkkn%</td></tr>";
        roundStatsTable += "<tr><th>Highest Kill Streak Player</th><td>%hksp%</td></tr>";
        roundStatsTable += "<tr><th>Highest Kill Streak Number</th><td>%hksn%</td></tr>";
        roundStatsTable += "<tr><th>Highest Death Streak Player</th><td>%hdsp%</td></tr>";
        roundStatsTable += "<tr><th>Highest Death Streak Number</th><td>%hdsn%</td></tr>";
        roundStatsTable += "<tr><th>Highest Score Player</th><td>%hsp%</td></tr>";
        roundStatsTable += "<tr><th>Highest Score Number</th><td>%hsn%</td></tr>";
        roundStatsTable += "<tr><th>Most Deaths Player</th><td>%mdp%</td></tr>";
        roundStatsTable += "<tr><th>Most Deaths Number</th><td>%mdn%</td></tr></table>";

        string weaponReplacementTable = @"<table><tr><th style=""background-color:#3a9bd0;"">Item</th><th style=""background-color:#3a9bd0;"">Most Player Weapon Kills #</th><th style=""background-color:#3a9bd0;"">Player w/ Most Weapon Kills</th></tr>";

        for (int i = 0; i < this.weaponNames.Count; i++)
          weaponReplacementTable += "<tr><th>" + this.weaponsLocalized[i] + "</th><td>%" + this.weaponCodes[i] + "%</td><td>%" + this.weaponCodes[i] + "_player%</td></tr>";

        weaponReplacementTable += "</table>";

        return roundStatsTable + weaponReplacementTable;
      }
      else if (callNum == 4)
      {
        string serverKillsTable = @"<table><tr><th style=""background-color:#3a9bd0;"">Item</th><th style=""background-color:#3a9bd0;"">Replacement Tag</th></tr>";
        serverKillsTable += "<tr><th>Kills Displayed Timespan</th><td>%kdt%</td></tr>";
        serverKillsTable += "<tr><th>Total Kills Overall</th><td>%tko%</td></tr>";
        serverKillsTable += "<tr><th>Knife Kills ('Knife' & 'Melee' combined)</th><td>%kk%</td></tr></table>";

        string weaponReplacementTable = @"<table><tr><th style=""background-color:#3a9bd0;"">Item</th><th style=""background-color:#3a9bd0;"">Weapon Kills #</th></tr>";

        for (int i = 0; i < this.weaponNames.Count; i++)
          weaponReplacementTable += "<tr><th>" + this.weaponsLocalized[i] + "</th><td>%" + this.weaponCodes[i] + "%</td></tr>";

        weaponReplacementTable += "</table>";

        return serverKillsTable + weaponReplacementTable;
      }
      else if (callNum == 5)
      {
        string killAnnouncementListTable = @"<table><tr><th style=""background-color:#3a9bd0;"">Item</th><th style=""background-color:#3a9bd0;"">Replacement Tag</th></tr>";
        killAnnouncementListTable += "<tr><th>Killer Player Name</th><td>%kpn%</td></tr>";
        killAnnouncementListTable += "<tr><th>Victim Player Name</th><td>%vpn%</td></tr>";
        killAnnouncementListTable += "<tr><th>Kill Type (weapon)</th><td>%kt%</td></tr></table>";

        return killAnnouncementListTable;
      }
      else if (callNum == 6)
      {
        string aceSquadTable = @"<table><tr><th style=""background-color:#3a9bd0;"">Item</th><th style=""background-color:#3a9bd0;"">Replacement Tag</th></tr>";
        aceSquadTable += "<tr><th>Players in the Ace Squad</th><td>%players%</td></tr>";
        aceSquadTable += "<tr><th>Name of the Ace Squad</th><td>%squad%</td></tr>";
        aceSquadTable += "<tr><th>Team of the Ace Squad</th><td>%team%</td></tr>";
        aceSquadTable += "<tr><th>Total Score of the Ace Squad</th><td>%score%</td></tr></table>";

        return aceSquadTable;
      }
      else if (callNum == 7 || callNum == 8)
      {
        string deathStreaksTable = @"<table><tr><th style=""background-color:#3a9bd0;"">Item</th><th style=""background-color:#3a9bd0;"">Replacement Tag</th></tr>";

        if (callNum == 8)
          deathStreaksTable += "<tr><th>Death Streak End Player Name (name of the player who the person on the death streak killed)</th><td>%dsepn%</td></tr>";

        deathStreaksTable += "<tr><th>Player Name (name of the player who is on the death streak)</th><td>%pn%</td></tr>";
        deathStreaksTable += "<tr><th>Death Streak Number</th><td>%dsn%</td></tr></table>";

        return deathStreaksTable;
      }
      else if (callNum == 9)
      {
        string firstBloodTable = @"<table><tr><th style=""background-color:#3a9bd0;"">Item</th><th style=""background-color:#3a9bd0;"">Replacement Tag</th></tr>";
        firstBloodTable += "<tr><th>First Blood Killer</th><td>%kfb%</td></tr>";
        firstBloodTable += "<tr><th>First Blood Victim</th><td>%vfb%</td></tr>";
        firstBloodTable += "<tr><th>First Blood Weapon</th><td>%wfb%</td></tr></table>";

        return firstBloodTable;
      }
      else if (callNum == 10)
      {
        string multikillTable = @"<table><tr><th style=""background-color:#3a9bd0;"">Item</th><th style=""background-color:#3a9bd0;"">Replacement Tag</th></tr>";
        multikillTable += "<tr><th>Multikill Player</th><td>%mp%</td></tr></table>";

        return multikillTable;
      }
      else
        return "";
    }

    private string getKillTypesEnum()
    {
      string killTypesEnum = "enum.KillTypes(...|";

      List<string> weapons = new List<string>(GetWeaponList(DamageTypes.None));

      for (int i = 0; i < weapons.Count; i++)
      {
        if (i == (weapons.Count - 1))
          killTypesEnum += weapons[i] + ")";
        else
          killTypesEnum += weapons[i] + "|";
      }

      return killTypesEnum;
    }

    private List<string> getRawWeaponNames()
    {
      List<string> weaponNames = new List<string>(GetWeaponList(DamageTypes.None));

      for (int i = 0; i < weaponNames.Count; i++)
      {
        weaponNames[i] = GetWeaponByLocalizedName(weaponNames[i]).Name;

        if (weaponNames[i] == "Roadkill")
          weaponNames[i] = "RoadKill";
      }

      return weaponNames;
    }

    private List<string> getWeaponCodes()
    {
      List<string> lstWeaponCodes = new List<string>();

      string weaponCode;
      Regex nonAlphaNumeric = new Regex("[^a-zA-Z0-9]");
      Regex multipleUnderscores = new Regex("_{2,}");
      for (int i = 0; i < this.weaponNames.Count; i++)
      {
        weaponCode = nonAlphaNumeric.Replace(this.weaponsLocalized[i], "_");
        weaponCode = multipleUnderscores.Replace(weaponCode, "_");
        weaponCode = weaponCode.ToLower();
        weaponCode = weaponCode.TrimEnd('_');

        lstWeaponCodes.Add("kills_" + weaponCode);
      }

      return lstWeaponCodes;
    }

    private List<int> getZeroedList()
    {
      List<int> zeroedList = new List<int>();

      return zeroedList;
    }

    private bool isValidKillType(string killType)
    {
      List<string> weapons = new List<string>(GetWeaponList(DamageTypes.None));

      if (weapons.Contains(killType))
        return true;
      else if (killType == "Suicide")
        return true;
      else
        return false;
    }

    private void initKillStreakMessages()
    {
      this.killStreakMessageList.Add("35|WOW! %pn% is on a %ksn% kill streak!");
      this.killStreakMessageList.Add("40|Now %pn% is on a %ksn% kill streak! What is going on?!");
      this.killStreakMessageList.Add("45|Okay, %pn% has %ksn% kills in a row now! Something isn't right!");
      this.killStreakMessageList.Add("50|WTF?! Now %pn% has %ksn% kills in a row! GET AN ADMIN!");
    }

    private void initDeathStreakMessages()
    {
      this.deathStreakMessageList.Add("20|WOW! %pn% has died %dsn% times in a row now!");
      this.deathStreakMessageList.Add("25|Now %pn% has died %dsn% times in a row! What is going on?!");
      this.deathStreakMessageList.Add("30|Okay, %pn% has died %dsn% times in a row now! Someone give him pointers!!");
      this.deathStreakMessageList.Add("35|WTF?! Now %pn% has died %dsn% times in a row! Time to uninstall!");
    }

    private void initMultikillMessages()
    {
      this.multikillMessageList.Add("3|%mp% has performed a TRIPLE KILL!|say");
      this.multikillMessageList.Add("4|%mp% has performed a QUADRUPLE KILL!|say");
      this.multikillMessageList.Add("5|%mp% has performed a QUINTUPLE KILL!|yell");
      this.multikillMessageList.Add("6|%mp% has performed a SEXTUPLE KILL!|yell");
      this.multikillMessageList.Add("7|%mp% has performed a SEPTUPLE KILL!|yell");
      this.multikillMessageList.Add("8|%mp% has performed a OCTUPLE KILL!|yell");
      this.multikillMessageList.Add("9|%mp% has performed a NONUPLE KILL!|yell");
      this.multikillMessageList.Add("10|%mp% has performed a DECUPLE KILL!|yell");
    }

    private bool canConnect()
    {
      bool canConnect = true;

      if (this.enableServerKills == enumBoolYesNo.No)
        canConnect = false;
      else if (this.dbHost == "")
        canConnect = false;
      else if (this.dbPort == "")
        canConnect = false;
      else if (this.dbName == "")
        canConnect = false;
      else if (this.dbUser == "")
        canConnect = false;
      else if (this.dbPassword == "")
        canConnect = false;
      else if (this.dbTable == "")
        canConnect = false;

      return canConnect;
    }

    private void initializeMessages()
    {
      if (this.gameServerType == "bf3")
        this.messageDisplay.Add("--------------------- PREVIOUS ROUND'S STATS ---------------------");
      else
        this.messageDisplay.Add("------------------------ END OF ROUND STATS ------------------------");
        
      this.messageDisplay.Add("Last Kill: %lkk% killed %lkv% with a %lkw%");
      this.messageDisplay.Add("Most Kills: %mkp% with %mkn% kills");
      this.messageDisplay.Add("Most Knife Kills: %mkkp% with %mkkn% knife kills");
      this.messageDisplay.Add("Highest Kill Streak: %hksp% with %hksn% kills in a row");
    }
    
    private void initializeKillsMessages()
    {
      this.killsMessageDisplay.Add("----- THIS SERVER'S KILLS IN THE PAST %kdt% HOURS -----");
      this.killsMessageDisplay.Add("Total Kills Overall: %tko%");
      this.killsMessageDisplay.Add("Knife Kills: %kk% Defib Kills: %kills_defibrillator%");
      this.killsMessageDisplay.Add("Repair Tool Kills: %kills_repair_tool%");
    }
    
    private void updateDisplayEvent()
    {
      this.ExecuteCommand("procon.protected.tasks.remove", "CServerKillsDisplay");

      if (this.pluginEnabled && this.enableServerKills == enumBoolYesNo.Yes)
      {
        int secondsToExecute = ((this.displayInterval * 60) - ((DateTime.Now.Minute * 60) % (this.displayInterval * 60))) - DateTime.Now.Second;
        this.ExecuteCommand("procon.protected.tasks.add", "CServerKillsDisplay", secondsToExecute.ToString(), "1", "1", "procon.protected.plugins.call", "CServerKills", "displayKills");
      }
    }

    private void updateSqlInsertEvent()
    {
      this.ExecuteCommand("procon.protected.tasks.remove", "CServerKillsInsert");

      if (this.pluginEnabled)
      {
        int secondsToExecute = ((5 * 60) - ((DateTime.Now.Minute * 60) % (5 * 60))) - DateTime.Now.Second;
        this.ExecuteCommand("procon.protected.tasks.add", "CServerKillsInsert", secondsToExecute.ToString(), "1", "1", "procon.protected.plugins.call", "CServerKills", "sqlInsert");
      }
    }

    private void odbcConnect()
    {
      string myDbConn = "DRIVER={MySQL ODBC 5.1 Driver};" +
                        "SERVER=" + this.dbHost + ";" +
                        "PORT=" + this.dbPort + ";" +
                        "DATABASE=" + this.dbName + ";" +
                        "UID=" + this.dbUser + ";" +
                        "PASSWORD=" + this.dbPassword + ";" +
                        "OPTION=3;";

      this.odbcConnection.ConnectionString = myDbConn;
    }

    private void createTables()
    {
      while (true)
      {
        if (this.odbcConnection.State != ConnectionState.Open)
        {
          try
          {
            this.odbcConnection.Open();

            this.sqlCommand = "CREATE TABLE IF NOT EXISTS `" + this.dbName + "`.`" + this.dbTable + "_overall_kills` (`Time` TIMESTAMP DEFAULT CURRENT_TIMESTAMP, `Kills` INT UNSIGNED NOT NULL)";

            using (OdbcCommand command = new OdbcCommand(this.sqlCommand, this.odbcConnection))
            {
              command.ExecuteNonQuery();
              command.CommandText = "CREATE TABLE IF NOT EXISTS `" + this.dbName + "`.`" + this.dbTable + "_overall_knife_kills` (`Time` TIMESTAMP DEFAULT CURRENT_TIMESTAMP, `Kills` INT UNSIGNED NOT NULL)";
              command.ExecuteNonQuery();

              for (int i = 0; i < this.weaponCodes.Count; i++)
              {
                command.CommandText = "CREATE TABLE IF NOT EXISTS `" + this.dbName + "`.`" + this.dbTable + "_" + this.weaponCodes[i] + "` (`Time` TIMESTAMP DEFAULT CURRENT_TIMESTAMP, `Kills` INT UNSIGNED NOT NULL)";
                command.ExecuteNonQuery();
              }
            }

            this.odbcConnection.Close();
          }
          catch (OdbcException e)
          {
            string errorMessages = "";

            for (int i = 0; i < e.Errors.Count; i++)
            {
              errorMessages += "^b^5Index #" + i + " ^n^0- ^bMessage: ^n" + e.Errors[i].Message + " ^bNativeError: ^n" + e.Errors[i].NativeError.ToString() + " ^bSource: ^n" + e.Errors[i].Source + " ^bSQL: ^n" + e.Errors[i].SQLState;
            }

            this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^8Error! ^2^bTask: ^ncreateTables() " + errorMessages);
          }

          break;
        }
      }
    }

    public void sqlInsert()
    {
      while (true)
      {
        if (this.odbcConnection.State != ConnectionState.Open)
        {
          try
          {
            this.odbcConnection.Open();

            this.sqlCommand = "INSERT INTO " + this.dbTable + "_overall_kills(`Kills`) VALUES('" + this.serverKills.ToString() + "')";

            using (OdbcCommand command = new OdbcCommand(this.sqlCommand, this.odbcConnection))
            {
              command.ExecuteNonQuery();
              command.CommandText = "INSERT INTO " + this.dbTable + "_overall_knife_kills(`Kills`) VALUES('" + this.knifeKills.ToString() + "')";
              command.ExecuteNonQuery();

              for (int i = 0; i < this.weaponCodes.Count; i++)
              {
                command.CommandText = "INSERT INTO " + this.dbTable + "_" + this.weaponCodes[i] + "(`Kills`) VALUES('" + this.weaponKills[i].ToString() + "')";
                command.ExecuteNonQuery();
              }
            }

            this.odbcConnection.Close();

            for (int i = 0; i < this.weaponKills.Count; i++)
              this.weaponKills[i] = 0;

            this.serverKills = 0;
            this.knifeKills = 0;
          }
          catch (OdbcException e)
          {
            string errorMessages = "";

            for (int i = 0; i < e.Errors.Count; i++)
            {
              errorMessages += "^b^5Index #" + i + " ^n^0- ^bMessage: ^n" + e.Errors[i].Message + " ^bNativeError: ^n" + e.Errors[i].NativeError.ToString() + " ^bSource: ^n" + e.Errors[i].Source + " ^bSQL: ^n" + e.Errors[i].SQLState;
            }

            this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^8Error! ^b^2Task: ^nsqlInsert() " + errorMessages);
          }

          this.updateSqlInsertEvent();

          break;
        }
      }
    }

    private int retrieveNumber(string query)
    {
      int kills = 0;

      using (OdbcCommand command = new OdbcCommand(query, odbcConnection))
      {
        using (OdbcDataReader reader = command.ExecuteReader())
        {
          if (reader.HasRows)
          {
            while (reader.Read())
            {
              int tempNum = reader.GetInt32(1);
              kills += tempNum;
            }
          }
        }
      }

      return kills;
    }

    public void displayKills()
    {
      if (this.enableServerKills == enumBoolYesNo.Yes)
      {
        while (true)
        {
          if (this.odbcConnection.State != ConnectionState.Open)
          {
            try
            {
              DateTime nowDate = DateTime.Now;
              string toDate = nowDate.Year.ToString() + "-" + nowDate.Month.ToString() + "-" + nowDate.Day.ToString() + " " + nowDate.Hour.ToString() + ":" + nowDate.Minute.ToString() + ":" + nowDate.Second.ToString();
              DateTime dtFromDate = nowDate.Subtract(TimeSpan.FromHours(this.killsDisplayedTimespan));
              dtFromDate = dtFromDate.AddMinutes(1);
              string fromDate = dtFromDate.Year.ToString() + "-" + dtFromDate.Month.ToString() + "-" + dtFromDate.Day.ToString() + " " + dtFromDate.Hour.ToString() + ":" + dtFromDate.Minute.ToString() + ":" + dtFromDate.Second.ToString();

              this.odbcConnection.Open();

              this.sqlCommand = "SELECT * FROM " + this.dbTable + "_overall_kills WHERE `Time` >= '" + fromDate + "' AND `Time` <= '" + toDate + "'";
              int totalKills = retrieveNumber(sqlCommand);
              this.sqlCommand = "SELECT * FROM " + this.dbTable + "_overall_knife_kills WHERE `Time` >= '" + fromDate + "' AND `Time` <= '" + toDate + "'";
              int totalKnifeKills = retrieveNumber(sqlCommand);

              List<int> totalWeaponKills = new List<int>();
              for (int i = 0; i < this.weaponCodes.Count; i++)
              {
                this.sqlCommand = "SELECT * FROM " + this.dbTable + "_" + this.weaponCodes[i] + " WHERE `Time` >= '" + fromDate + "' AND `Time` <= '" + toDate + "'";
                totalWeaponKills.Add(retrieveNumber(sqlCommand));
              }

              this.odbcConnection.Close();

              this.formatKillsMessages(totalKills.ToString(), totalKnifeKills.ToString(), totalWeaponKills);
              this.sendKillsMessages();
            }
            catch (OdbcException e)
            {
              string errorMessages = "";

              for (int i = 0; i < e.Errors.Count; i++)
              {
                errorMessages += "^b^5Index #" + i + " ^n^0- ^bMessage: ^n" + e.Errors[i].Message + " ^bNativeError: ^n" + e.Errors[i].NativeError.ToString() + " ^bSource: ^n" + e.Errors[i].Source + " ^bSQL: ^n" + e.Errors[i].SQLState;
              }

              this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^8Error! ^b^2Task: ^ndisplayKills() " + errorMessages);
            }

            this.updateDisplayEvent();

            break;
          }
        }
      }
    }

    private void addKill(Kill killDetails)
    {
      int weaponIndex = this.weaponNames.IndexOf(killDetails.DamageType);

      if (weaponIndex >= 0)
      {
        this.weaponKills[weaponIndex]++;

        if (!this.playerWeaponKills.ContainsKey(killDetails.Killer.SoldierName))
          this.playerWeaponKills.Add(killDetails.Killer.SoldierName, new List<int>(new int[this.weaponNames.Count]));

        this.playerWeaponKills[killDetails.Killer.SoldierName][weaponIndex]++;
      }
    }

    private void formatMessage(string type, params string[] items)
    {
      if (type == "Most Kills")
      {
        for (int i = 0; i < this.formattedMessages.Count; i++)
          this.formattedMessages[i] = this.formattedMessages[i].Replace("%mkp%", items[0]).Replace("%mkn%", items[1]);
      }
      if (type == "Most Deaths")
      {
        for (int i = 0; i < this.formattedMessages.Count; i++)
          this.formattedMessages[i] = this.formattedMessages[i].Replace("%mdp%", items[0]).Replace("%mdn%", items[1]);
      }
      else if (type == "Highest Score")
      {
        for (int i = 0; i < this.formattedMessages.Count; i++)
          this.formattedMessages[i] = this.formattedMessages[i].Replace("%hsp%", items[0]).Replace("%hsn%", items[1]);
      }
      else if (type == "Last Kill")
      {
        for (int i = 0; i < this.formattedMessages.Count; i++)
          this.formattedMessages[i] = this.formattedMessages[i].Replace("%lkk%", items[0]).Replace("%lkv%", items[1]).Replace("%lkw%", items[2]);
      }
      else if (type == "Most Knife Kills")
      {
        for (int i = 0; i < this.formattedMessages.Count; i++)
          this.formattedMessages[i] = this.formattedMessages[i].Replace("%mkkp%", items[0]).Replace("%mkkn%", items[1]);
      }
      else if (type == "Highest Kill Streak")
      {
        for (int i = 0; i < this.formattedMessages.Count; i++)
          this.formattedMessages[i] = this.formattedMessages[i].Replace("%hksp%", items[0]).Replace("%hksn%", items[1]);
      }
      else if (type == "Highest Death Streak")
      {
        for (int i = 0; i < this.formattedMessages.Count; i++)
          this.formattedMessages[i] = this.formattedMessages[i].Replace("%hdsp%", items[0]).Replace("%hdsn%", items[1]);
      }
      else if (type == "Most Weapon Kills")
      {
        for (int i = 0; i < this.weaponCodes.Count; i++)
        {
          for (int j = 0; j < this.formattedMessages.Count; j++)
            this.formattedMessages[j] = this.formattedMessages[j].Replace("%" + this.weaponCodes[i] + "_player%", this.weaponsMostKillsPlayer[i]).Replace("%" + this.weaponCodes[i] + "%", this.weaponsMostKills[i].ToString());
        }

        this.weaponsMostKills.Clear();
        this.weaponsMostKillsPlayer.Clear();
      }
    }
    
    private void formatKillsMessages(string totalKills, string totalKnifeKills, List<int> totalWeaponKills)
    {
      this.formattedKillsMessages.Clear();

      for (int i = 0; i < this.killsMessageDisplay.Count; i++)
      {
        this.formattedKillsMessages.Add(this.killsMessageDisplay[i].Replace("%kdt%", this.killsDisplayedTimespan.ToString()).Replace("%tko%", totalKills).Replace("%kk%", totalKnifeKills));

        for (int j = 0; j < this.weaponCodes.Count; j++)
          this.formattedKillsMessages[i] = this.formattedKillsMessages[i].Replace("%" + this.weaponCodes[j] + "%", totalWeaponKills[j].ToString());
      }
    }
    
    public void sendMessages()
    {
      for (int i = 0; i < this.formattedMessages.Count; i++)
      {
        this.ExecuteCommand("procon.protected.send", "admin.say", this.formattedMessages[i], "all");
        
        if (this.displayToConsole == enumBoolYesNo.Yes)
          this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^0" + this.formattedMessages[i]);
      }

      /*
      // would trigger at end of round screen after round stats show
      if (this.gameServerType == "bf4")
        this.sendAceSquad();
      */
    }
    
    private void sendKillsMessages()
    {
      for (int i = 0; i < this.formattedKillsMessages.Count; i++)
      {
        this.ExecuteCommand("procon.protected.send", "admin.say", this.formattedKillsMessages[i], "all");

        if (this.displayKillsToConsole == enumBoolYesNo.Yes)
          this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^0" + this.formattedKillsMessages[i]);
      }
    }

    public void sendAceSquad()
    {
      this.aceSquadPlayersString = "";

      if (this.aceSquadPlayers.Count == 1)
        this.aceSquadPlayersString = this.aceSquadPlayers[0];
      else if (this.aceSquadPlayers.Count > 1)
      {
        for (int i = 0; i < this.aceSquadPlayers.Count; i++)
        {
          if (i == this.aceSquadPlayers.Count - 1)
            this.aceSquadPlayersString += "& " + this.aceSquadPlayers[i];
          else
          {
            if (this.aceSquadPlayers.Count == 2)
              this.aceSquadPlayersString += this.aceSquadPlayers[i] + " ";
            else
              this.aceSquadPlayersString += this.aceSquadPlayers[i] + ", ";
          }
        }
      }

      if (this.aceSquadDisplayType == "vars.serverMessage" && this.gameServerType == "bf3")
      {
        this.needOriginalMessage = true;
        this.ExecuteCommand("procon.protected.send", "vars.serverMessage");
      }
      else
      {
        if (this.aceSquadDisplayType == "yell")
          this.ExecuteCommand("procon.protected.send", "admin.yell", this.aceSquadMessage.Replace("%players%", this.aceSquadPlayersString).Replace("%squad%", GetLocalized("Squad" + this.aceSquadSquadID.ToString(), String.Format("global.Squad{0}", this.aceSquadSquadID.ToString()))).Replace("%team%", this.teamNames[this.aceSquadTeamID]).Replace("%score%", this.highestSquadScore.ToString()));
        else if (this.aceSquadDisplayType == "say")
          this.ExecuteCommand("procon.protected.send", "admin.say", this.aceSquadMessage.Replace("%players%", this.aceSquadPlayersString).Replace("%squad%", GetLocalized("Squad" + this.aceSquadSquadID.ToString(), String.Format("global.Squad{0}", this.aceSquadSquadID.ToString()))).Replace("%team%", this.teamNames[this.aceSquadTeamID]).Replace("%score%", this.highestSquadScore.ToString()), "all");

        if (this.displayAceSquadToConsole == enumBoolYesNo.Yes)
          this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^0" + this.aceSquadMessage.Replace("%players%", this.aceSquadPlayersString).Replace("%squad%", GetLocalized("Squad" + this.aceSquadSquadID.ToString(), String.Format("global.Squad{0}", this.aceSquadSquadID.ToString()))).Replace("%team%", this.teamNames[this.aceSquadTeamID]).Replace("%score%", this.highestSquadScore.ToString()));

        /*
        if (this.gameMode.Contains("Conquest") || this.gameMode.Contains("TeamDeathMatch") || this.gameMode.Contains("Domination") || this.gameMode.Contains("GunMaster") || this.gameMode.Contains("TankSuperiority") || this.gameMode.Contains("Scavenger") || this.gameMode.Contains("CaptureTheFlag") || this.gameMode.Contains("AirSuperiority") || this.gameMode.Contains("Obliteration") || this.gameMode.Contains("Elimination"))
        {
          if (this.aceSquadDisplayType == "yell")
            this.ExecuteCommand("procon.protected.send", "admin.yell", this.aceSquadMessage.Replace("%players%", this.aceSquadPlayersString).Replace("%squad%", GetLocalized("Squad" + this.aceSquadSquadID.ToString(), String.Format("global.Squad{0}", this.aceSquadSquadID.ToString()))).Replace("%team%", ((this.aceSquadTeamID == 1) ? "US" : "RU")).Replace("%score%", this.highestSquadScore.ToString()));
          else if (this.aceSquadDisplayType == "say")
            this.ExecuteCommand("procon.protected.send", "admin.say", this.aceSquadMessage.Replace("%players%", this.aceSquadPlayersString).Replace("%squad%", GetLocalized("Squad" + this.aceSquadSquadID.ToString(), String.Format("global.Squad{0}", this.aceSquadSquadID.ToString()))).Replace("%team%", ((this.aceSquadTeamID == 1) ? "US" : "RU")).Replace("%score%", this.highestSquadScore.ToString()), "all");

          if (this.displayAceSquadToConsole == enumBoolYesNo.Yes)
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^0" + this.aceSquadMessage.Replace("%players%", this.aceSquadPlayersString).Replace("%squad%", GetLocalized("Squad" + this.aceSquadSquadID.ToString(), String.Format("global.Squad{0}", this.aceSquadSquadID.ToString()))).Replace("%team%", ((this.aceSquadTeamID == 1) ? "US" : "RU")).Replace("%score%", this.highestSquadScore.ToString()));
        }
        else if (this.gameMode.Contains("Rush"))
        {
          if (this.aceSquadDisplayType == "yell")
            this.ExecuteCommand("procon.protected.send", "admin.yell", this.aceSquadMessage.Replace("%players%", this.aceSquadPlayersString).Replace("%squad%", GetLocalized("Squad" + this.aceSquadSquadID.ToString(), String.Format("global.Squad{0}", this.aceSquadSquadID.ToString()))).Replace("%team%", ((this.aceSquadTeamID == 1) ? "Attackers" : "Defenders")).Replace("%score%", this.highestSquadScore.ToString()));
          else if (this.aceSquadDisplayType == "say")
            this.ExecuteCommand("procon.protected.send", "admin.say", this.aceSquadMessage.Replace("%players%", this.aceSquadPlayersString).Replace("%squad%", GetLocalized("Squad" + this.aceSquadSquadID.ToString(), String.Format("global.Squad{0}", this.aceSquadSquadID.ToString()))).Replace("%team%", ((this.aceSquadTeamID == 1) ? "Attackers" : "Defenders")).Replace("%score%", this.highestSquadScore.ToString()), "all");

          if (this.displayAceSquadToConsole == enumBoolYesNo.Yes)
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^0" + this.aceSquadMessage.Replace("%players%", this.aceSquadPlayersString).Replace("%squad%", GetLocalized("Squad" + this.aceSquadSquadID.ToString(), String.Format("global.Squad{0}", this.aceSquadSquadID.ToString()))).Replace("%team%", ((this.aceSquadTeamID == 1) ? "Attackers" : "Defenders")).Replace("%score%", this.highestSquadScore.ToString()));
        }
        */
      }
    }

    public void announceMultikill()
    {
      if (this.playerMultikill[this.multikillAnnounceQueue[0]] > 1 && this.displayMultikills == enumBoolYesNo.Yes)
      {
        string[] messageSplit;

        for (int i = 0; i < this.multikillMessageList.Count; i++)
        {
          messageSplit = this.multikillMessageList[i].Split('|');

          if (messageSplit[0] == this.playerMultikill[this.multikillAnnounceQueue[0]].ToString())
          {
            if (messageSplit[2] == "yell")
              this.ExecuteCommand("procon.protected.send", "admin.yell", messageSplit[1].Replace("%mp%", this.multikillAnnounceQueue[0]));
            else if (messageSplit[2] == "say")
              this.ExecuteCommand("procon.protected.send", "admin.say", messageSplit[1].Replace("%mp%", this.multikillAnnounceQueue[0]), "all");
          }
        }

        if (this.displayMultikillToConsole == enumBoolYesNo.Yes && this.playerMultikill[this.multikillAnnounceQueue[0]] >= this.multikillConsoleValue)
          this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^0" + this.multikillAnnounceQueue[0] + " got a MULTIKILL of " + this.playerMultikill[this.multikillAnnounceQueue[0]].ToString() + "!");
      }

      this.playerMultikill[this.multikillAnnounceQueue[0]] = 0;
      this.multikillAnnounceQueue.RemoveAt(0);
    }

    private void killStreakDisplay(string soldierName, string victimSoldierName)
    {
      bool alreadyDisplayed = false;

      for (int i = 0; i < this.killStreakMessageList.Count; i++)
      {
        string[] messageSplit = this.killStreakMessageList[i].Split('|');

        if (messageSplit[0] == this.killStreak[soldierName].ToString())
        {
          if (this.killStreakDisplayType == "say")
            this.ExecuteCommand("procon.protected.send", "admin.say", messageSplit[1].Replace("%pn%", soldierName).Replace("%ksn%", this.killStreak[soldierName].ToString()), "all");
          else if (this.killStreakDisplayType == "yell")
            this.ExecuteCommand("procon.protected.send", "admin.yell", messageSplit[1].Replace("%pn%", soldierName).Replace("%ksn%", this.killStreak[soldierName].ToString()));

          alreadyDisplayed = true;
        }
      }

      if (this.killStreak[soldierName] >= this.killStreakDisplayStart)
      {
        if (!alreadyDisplayed)
        {
          if (((this.killStreak[soldierName] - this.killStreakDisplayStart) % this.killStreakDisplayInterval) == 0)
          {
            if (this.killStreakDisplayType == "say")
              this.ExecuteCommand("procon.protected.send", "admin.say", this.killStreakMessage.Replace("%pn%", soldierName).Replace("%ksn%", this.killStreak[soldierName].ToString()), "all");
            else if (this.killStreakDisplayType == "yell")
              this.ExecuteCommand("procon.protected.send", "admin.yell", this.killStreakMessage.Replace("%pn%", soldierName).Replace("%ksn%", this.killStreak[soldierName].ToString()));
          }
        }

        if (this.displayKillStreaksToConsole == enumBoolYesNo.Yes)
        {
          if (this.killStreak[soldierName] == this.killStreakConsoleValue)
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^0" + this.killStreakMessage.Replace("%pn%", soldierName).Replace("%ksn%", this.killStreak[soldierName].ToString()));
          else if (this.killStreak[soldierName] > this.killStreakConsoleValue && ((this.killStreak[soldierName] - this.killStreakDisplayStart) % this.killStreakDisplayInterval) == 0)
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^0" + this.killStreakMessage.Replace("%pn%", soldierName).Replace("%ksn%", this.killStreak[soldierName].ToString()));
        }
      }
      
      if (this.killStreak.ContainsKey(victimSoldierName))
      {
        if (this.killStreak[victimSoldierName] >= this.endKillStreakNumber)
        {
          if (this.killStreakDisplayType == "say")
            this.ExecuteCommand("procon.protected.send", "admin.say", this.endKillStreakMessage.Replace("%ksepn%", soldierName).Replace("%pn%", victimSoldierName).Replace("%ksn%", this.killStreak[victimSoldierName].ToString()), "all");
          else if (this.killStreakDisplayType == "yell")
            this.ExecuteCommand("procon.protected.send", "admin.yell", this.endKillStreakMessage.Replace("%ksepn%", soldierName).Replace("%pn%", victimSoldierName).Replace("%ksn%", this.killStreak[victimSoldierName].ToString()));
        }
      }
    }

    private void deathStreakDisplay(string soldierName, string killerSoldierName)
    {
      bool alreadyDisplayed = false;

      for (int i = 0; i < this.deathStreakMessageList.Count; i++)
      {
        string[] messageSplit = this.deathStreakMessageList[i].Split('|');

        if (messageSplit[0] == this.deathStreak[soldierName].ToString())
        {
          if (this.deathStreakDisplayType == "say")
            this.ExecuteCommand("procon.protected.send", "admin.say", messageSplit[1].Replace("%pn%", soldierName).Replace("%dsn%", this.deathStreak[soldierName].ToString()), "all");
          else if (this.deathStreakDisplayType == "yell")
            this.ExecuteCommand("procon.protected.send", "admin.yell", messageSplit[1].Replace("%pn%", soldierName).Replace("%dsn%", this.deathStreak[soldierName].ToString()));

          alreadyDisplayed = true;
        }
      }

      if (this.deathStreak[soldierName] >= this.deathStreakDisplayStart)
      {
        if (!alreadyDisplayed)
        {
          if (((this.deathStreak[soldierName] - this.deathStreakDisplayStart) % this.deathStreakDisplayInterval) == 0)
          {
            if (this.deathStreakDisplayType == "say")
              this.ExecuteCommand("procon.protected.send", "admin.say", this.deathStreakMessage.Replace("%pn%", soldierName).Replace("%dsn%", this.deathStreak[soldierName].ToString()), "all");
            else if (this.deathStreakDisplayType == "yell")
              this.ExecuteCommand("procon.protected.send", "admin.yell", this.deathStreakMessage.Replace("%pn%", soldierName).Replace("%dsn%", this.deathStreak[soldierName].ToString()));
          }
        }

        if (this.displayDeathStreaksToConsole == enumBoolYesNo.Yes)
        {
          if (this.deathStreak[soldierName] == this.deathStreakConsoleValue)
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^0" + this.deathStreakMessage.Replace("%pn%", soldierName).Replace("%dsn%", this.deathStreak[soldierName].ToString()));
          else if (this.deathStreak[soldierName] > this.deathStreakConsoleValue && ((this.deathStreak[soldierName] - this.deathStreakDisplayStart) % this.deathStreakDisplayInterval) == 0)
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^0" + this.deathStreakMessage.Replace("%pn%", soldierName).Replace("%dsn%", this.deathStreak[soldierName].ToString()));
        }
      }

      if (this.deathStreak.ContainsKey(killerSoldierName))
      {
        if (this.deathStreak[killerSoldierName] >= this.endDeathStreakNumber)
        {
          if (this.deathStreakDisplayType == "say")
            this.ExecuteCommand("procon.protected.send", "admin.say", this.endDeathStreakMessage.Replace("%dsepn%", soldierName).Replace("%pn%", killerSoldierName).Replace("%dsn%", this.deathStreak[killerSoldierName].ToString()), "all");
          else if (this.deathStreakDisplayType == "yell")
            this.ExecuteCommand("procon.protected.send", "admin.yell", this.endDeathStreakMessage.Replace("%dsepn%", soldierName).Replace("%pn%", killerSoldierName).Replace("%dsn%", this.deathStreak[killerSoldierName].ToString()));
        }
      }
    }

    private void announceKill(Kill killDetails)
    {
      for (int i = 0; i < this.killTypeList.Count; i++)
      {
        string[] messageSplit = this.killTypeList[i].Split('|');

        if (GetLocalized(killDetails.DamageType, String.Format("global.Weapons.{0}", killDetails.DamageType.ToLower())) == messageSplit[0])
        {
          if (this.announcementDisplayType == "say")
            this.ExecuteCommand("procon.protected.send", "admin.say", messageSplit[1].Replace("%kpn%", killDetails.Killer.SoldierName).Replace("%vpn%", killDetails.Victim.SoldierName).Replace("%kt%", messageSplit[0]), "all");
          else if (this.announcementDisplayType == "yell")
            this.ExecuteCommand("procon.protected.send", "admin.yell", messageSplit[1].Replace("%kpn%", killDetails.Killer.SoldierName).Replace("%vpn%", killDetails.Victim.SoldierName).Replace("%kt%", messageSplit[0]));

          if (this.displayAnnouncementToConsole == enumBoolYesNo.Yes)
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^0" + messageSplit[1].Replace("%kpn%", killDetails.Killer.SoldierName).Replace("%vpn%", killDetails.Victim.SoldierName).Replace("%kt%", messageSplit[0]));

          break;
        }
      }

      if (!this.bFirstBlood)
      {
        if (this.displayFirstBlood == enumBoolYesNo.Yes)
        {
          if (this.firstBloodDisplayType == "say")
            this.ExecuteCommand("procon.protected.send", "admin.say", firstBloodMessage.Replace("%kfb%", killDetails.Killer.SoldierName).Replace("%vfb%", killDetails.Victim.SoldierName).Replace("%wfb%", GetLocalized(killDetails.DamageType, String.Format("global.Weapons.{0}", killDetails.DamageType.ToLower()))), "all");
          else if (this.firstBloodDisplayType == "yell")
            this.ExecuteCommand("procon.protected.send", "admin.yell", firstBloodMessage.Replace("%kfb%", killDetails.Killer.SoldierName).Replace("%vfb%", killDetails.Victim.SoldierName).Replace("%wfb%", GetLocalized(killDetails.DamageType, String.Format("global.Weapons.{0}", killDetails.DamageType.ToLower()))));

          if (this.displayFirstBloodToConsole == enumBoolYesNo.Yes)
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^0" + firstBloodMessage.Replace("%kfb%", killDetails.Killer.SoldierName).Replace("%vfb%", killDetails.Victim.SoldierName).Replace("%wfb%", GetLocalized(killDetails.DamageType, String.Format("global.Weapons.{0}", killDetails.DamageType.ToLower()))));
        }

        this.bFirstBlood = true;
      }
    }

    private void calculateWeaponsMostKills()
    {
      for (int i = 0; i < this.weaponNames.Count; i++)
      {
        int mostCurrentWeaponKills = 0;

        foreach (string playerName in this.playerWeaponKills.Keys)
        {
          if (this.playerWeaponKills[playerName][i] > mostCurrentWeaponKills)
            mostCurrentWeaponKills = this.playerWeaponKills[playerName][i];
        }

        foreach (string playerName in this.playerWeaponKills.Keys)
        {
          if (this.playerWeaponKills[playerName][i] == mostCurrentWeaponKills)
          {
            this.weaponsMostKills.Add(mostCurrentWeaponKills);
            this.weaponsMostKillsPlayer.Add(playerName);

            break;
          }
        }
      }

      this.playerWeaponKills.Clear();
    }

    public void setRoundEnded()
    {
      this.bRoundEnded = true;

      if (this.gameServerType == "bf4")
        this.ExecuteCommand("procon.protected.tasks.add", "CServerKillsSendRoundMessages", "15", "1", "1", "procon.protected.plugins.call", "CServerKills", "sendMessages");

      this.playerMultikill.Clear();
      this.multikillAnnounceQueue.Clear();
    }

    public void resetServerMessage()
    {
      if (this.needMessageReset)
      {
        this.ExecuteCommand("procon.protected.send", "vars.serverMessage", this.originalServerMessage);
        this.needMessageReset = false;
      }
    }
    
    public override void OnPlayerKilled(Kill kKillerVictimDetails)
    {
      this.serverKills++;

      this.addKill(kKillerVictimDetails);

      if (kKillerVictimDetails.DamageType == "Melee" || kKillerVictimDetails.DamageType == "Weapons/Knife/Knife")
      {
        this.knifeKills++;

        if (mostKnifeKills.ContainsKey(kKillerVictimDetails.Killer.SoldierName))
          mostKnifeKills[kKillerVictimDetails.Killer.SoldierName]++;
        else
          mostKnifeKills.Add(kKillerVictimDetails.Killer.SoldierName, 1);
      }
      else if (kKillerVictimDetails.DamageType == "Defib")
        this.defibKills++;
      else if (kKillerVictimDetails.DamageType == "Repair Tool")
        this.repairToolKills++;

      if (kKillerVictimDetails.Killer.SoldierName != kKillerVictimDetails.Victim.SoldierName)
        this.announceKill(kKillerVictimDetails);

      // Kill Streaks
      if (this.killStreak.ContainsKey(kKillerVictimDetails.Killer.SoldierName))
      {
        this.killStreak[kKillerVictimDetails.Killer.SoldierName]++;

        if (this.enableKillStreaks == enumBoolYesNo.Yes)
          this.killStreakDisplay(kKillerVictimDetails.Killer.SoldierName, kKillerVictimDetails.Victim.SoldierName);

        if (this.killStreak.ContainsKey(kKillerVictimDetails.Victim.SoldierName))
          this.killStreak[kKillerVictimDetails.Victim.SoldierName] = 0;

        if (this.killStreak[kKillerVictimDetails.Killer.SoldierName] > this.iHighestKillStreak)
        {
          this.iHighestKillStreak = this.killStreak[kKillerVictimDetails.Killer.SoldierName];
          this.highestKillStreak = kKillerVictimDetails.Killer.SoldierName;
        }
      }
      else if (kKillerVictimDetails.Killer.SoldierName != "")
      {
        this.killStreak.Add(kKillerVictimDetails.Killer.SoldierName, 1);

        if (this.enableKillStreaks == enumBoolYesNo.Yes)
          this.killStreakDisplay(kKillerVictimDetails.Killer.SoldierName, kKillerVictimDetails.Victim.SoldierName);

        if (this.killStreak.ContainsKey(kKillerVictimDetails.Victim.SoldierName))
          this.killStreak[kKillerVictimDetails.Victim.SoldierName] = 0;

        if (this.killStreak[kKillerVictimDetails.Killer.SoldierName] > this.iHighestKillStreak)
        {
          this.iHighestKillStreak = this.killStreak[kKillerVictimDetails.Killer.SoldierName];
          this.highestKillStreak = kKillerVictimDetails.Killer.SoldierName;
        }
      }

      // Death Streaks
      if (this.deathStreak.ContainsKey(kKillerVictimDetails.Victim.SoldierName))
      {
        this.deathStreak[kKillerVictimDetails.Victim.SoldierName]++;

        if (this.enableDeathStreaks == enumBoolYesNo.Yes)
          this.deathStreakDisplay(kKillerVictimDetails.Victim.SoldierName, kKillerVictimDetails.Killer.SoldierName);

        if (this.deathStreak.ContainsKey(kKillerVictimDetails.Killer.SoldierName))
          this.deathStreak[kKillerVictimDetails.Killer.SoldierName] = 0;

        if (this.deathStreak[kKillerVictimDetails.Victim.SoldierName] > this.iHighestDeathStreak)
        {
          this.iHighestDeathStreak = this.deathStreak[kKillerVictimDetails.Victim.SoldierName];
          this.highestDeathStreak = kKillerVictimDetails.Victim.SoldierName;
        }
      }
      else if (kKillerVictimDetails.Victim.SoldierName != "")
      {
        this.deathStreak.Add(kKillerVictimDetails.Victim.SoldierName, 1);

        if (this.enableDeathStreaks == enumBoolYesNo.Yes)
          this.deathStreakDisplay(kKillerVictimDetails.Victim.SoldierName, kKillerVictimDetails.Killer.SoldierName);

        if (this.deathStreak.ContainsKey(kKillerVictimDetails.Killer.SoldierName))
          this.deathStreak[kKillerVictimDetails.Killer.SoldierName] = 0;

        if (this.deathStreak[kKillerVictimDetails.Victim.SoldierName] > this.iHighestDeathStreak)
        {
          this.iHighestDeathStreak = this.deathStreak[kKillerVictimDetails.Victim.SoldierName];
          this.highestDeathStreak = kKillerVictimDetails.Victim.SoldierName;
        }
      }

      // Multikills
      if (kKillerVictimDetails.Killer.SoldierName != "" && kKillerVictimDetails.Killer.SoldierName != kKillerVictimDetails.Victim.SoldierName)
      {
        if (this.playerMultikill.ContainsKey(kKillerVictimDetails.Killer.SoldierName))
        {
          if (this.playerMultikill[kKillerVictimDetails.Killer.SoldierName] == 0)
          {
            this.multikillAnnounceQueue.Add(kKillerVictimDetails.Killer.SoldierName);
            this.ExecuteCommand("procon.protected.tasks.add", "CServerKillsAnnounceMultikill", this.multikillWindow.ToString(), "1", "1", "procon.protected.plugins.call", "CServerKills", "announceMultikill");
          }

          this.playerMultikill[kKillerVictimDetails.Killer.SoldierName]++;
        }
        else
        {
          this.playerMultikill.Add(kKillerVictimDetails.Killer.SoldierName, 1);
          this.multikillAnnounceQueue.Add(kKillerVictimDetails.Killer.SoldierName);
          this.ExecuteCommand("procon.protected.tasks.add", "CServerKillsAnnounceMultikill", this.multikillWindow.ToString(), "1", "1", "procon.protected.plugins.call", "CServerKills", "announceMultikill");
        }
      }

      this.lastKill = kKillerVictimDetails;
    }

    public override void OnPlayerSpawned(string soldierName, Inventory spawnedInventory)
    {
      if (this.bRoundEnded)
      {
        if (this.enableRoundStats == enumBoolYesNo.Yes)
        {
          if (this.gameServerType == "bf3")
            this.ExecuteCommand("procon.protected.tasks.add", "CServerKillsSendRoundMessages", "5", "1", "1", "procon.protected.plugins.call", "CServerKills", "sendMessages");

          if (this.displayAceSquad == enumBoolYesNo.Yes)
          {
            if (this.aceSquadDisplayType == "vars.serverMessage")
              this.ExecuteCommand("procon.protected.tasks.add", "CServerKillsResetServerMessage", "15", "1", "1", "procon.protected.plugins.call", "CServerKills", "resetServerMessage");
            else if (this.aceSquadDisplayType == "yell" || this.aceSquadDisplayType == "say")
              this.ExecuteCommand("procon.protected.tasks.add", "CServerKillsSendAceSquad", "10", "1", "1", "procon.protected.plugins.call", "CServerKills", "sendAceSquad");
          }
        }

        this.bFirstBlood = false;
        this.bRoundEnded = false;
      }
    }

    public override void OnRoundOverPlayers(List<CPlayerInfo> players)
    {
      ///// Get Most Kills Player & Number /////

      int mostKills = 0;

      for (int i = 0; i < players.Count; i++)
      {
        if (players[i].Kills > mostKills)
          mostKills = players[i].Kills;
      }

      for (int i = 0; i < players.Count; i++)
      {
        if (players[i].Kills == mostKills)
          this.mostKillsPlayer = players[i].SoldierName;
      }

      if (mostKills > 0)
        this.formatMessage("Most Kills", this.mostKillsPlayer, mostKills.ToString());
      else
        this.formatMessage("Most Kills", "N/A", "N/A");

      ///// Get Most Deaths Player & Number /////

      int mostDeaths = 0;

      for (int i = 0; i < players.Count; i++)
      {
        if (players[i].Deaths > mostDeaths)
          mostDeaths = players[i].Deaths;
      }

      for (int i = 0; i < players.Count; i++)
      {
        if (players[i].Deaths == mostDeaths)
          this.mostDeathsPlayer = players[i].SoldierName;
      }

      if (mostDeaths > 0)
        this.formatMessage("Most Deaths", this.mostDeathsPlayer, mostDeaths.ToString());
      else
        this.formatMessage("Most Kills", "N/A", "N/A");

      ///// Get Highest Score Player & Number /////

      int highestScore = 0;

      for (int i = 0; i < players.Count; i++)
      {
        if (players[i].Score > highestScore)
          highestScore = players[i].Score;
      }

      for (int i = 0; i < players.Count; i++)
      {
        if (players[i].Score == highestScore)
          this.highestScorePlayer = players[i].SoldierName;
      }

      if (highestScore > 0)
        this.formatMessage("Highest Score", this.highestScorePlayer, highestScore.ToString());
      else
        this.formatMessage("Highest Score", "N/A", "N/A");

      ///// Get Ace Squad /////

      int[,] teamSquadID = new int[,] {{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}};
      this.highestSquadScore = 0;
      this.aceSquadTeamID = 0;
      this.aceSquadSquadID = 0;
      this.aceSquadPlayers.Clear();

      for (int i = 0; i < players.Count; i++)
      {
        if (players[i].TeamID != 0 && players[i].SquadID != 0)
          teamSquadID[players[i].TeamID - 1, players[i].SquadID - 1] += players[i].Score;
      }

      for (int i = 0; i < 2; i++)
      {
        for (int j = 0; j < 32; j++)
        {
          if (teamSquadID[i, j] > this.highestSquadScore)
            this.highestSquadScore = teamSquadID[i, j];
        }
      }

      for (int i = 0; i < 2; i++)
      {
        for (int j = 0; j < 32; j++)
        {
          if (teamSquadID[i, j] == this.highestSquadScore)
          {
            this.aceSquadTeamID = i + 1;
            this.aceSquadSquadID = j + 1;
          }
        }
      }

      for (int i = 0; i < players.Count; i++)
      {
        if (players[i].TeamID == this.aceSquadTeamID && players[i].SquadID == this.aceSquadSquadID)
          this.aceSquadPlayers.Add(players[i].SoldierName);
      }

      this.needServerInfo = true;
      this.ExecuteCommand("procon.protected.send", "serverInfo");
    }

    public override void OnRoundOver(int winningTeamId)
    {
      this.formattedMessages.Clear();
      this.formattedMessages = new List<string>(this.messageDisplay);

      if (this.lastKill != null)
        this.formatMessage("Last Kill", this.lastKill.Killer.SoldierName, this.lastKill.Victim.SoldierName, GetLocalized(this.lastKill.DamageType, String.Format("global.Weapons.{0}", this.lastKill.DamageType.ToLower())));
      else
        this.formatMessage("Last Kill", "N/A", "N/A", "N/A");

      int iMostKnifeKills = 0;
      if (this.mostKnifeKills.Count > 0)
      {
        string[] strTemp = new string[mostKnifeKills.Count];
        int[] intTemp = new int[mostKnifeKills.Count];
        this.mostKnifeKills.Keys.CopyTo(strTemp, 0);
        this.mostKnifeKills.Values.CopyTo(intTemp, 0);

        for (int i = 0; i < mostKnifeKills.Count; i++)
        {
          if (intTemp[i] > iMostKnifeKills)
            iMostKnifeKills = intTemp[i];
        }

        for (int i = 0; i < mostKnifeKills.Count; i++)
        {
          if (intTemp[i] == iMostKnifeKills)
            this.formatMessage("Most Knife Kills", strTemp[i], iMostKnifeKills.ToString());
        }

        mostKnifeKills.Clear();
      }
      else
        this.formatMessage("Most Knife Kills", "N/A", "N/A");

      if (this.iHighestKillStreak > 0)
      {
        this.formatMessage("Highest Kill Streak", this.highestKillStreak, this.iHighestKillStreak.ToString());

        this.iHighestKillStreak = 0;
        this.killStreak.Clear();
      }
      else
        this.formatMessage("Highest Kill Streak", "N/A", "N/A");

      if (this.iHighestDeathStreak > 0)
      {
        this.formatMessage("Highest Death Streak", this.highestDeathStreak, this.iHighestDeathStreak.ToString());

        this.iHighestDeathStreak = 0;
        this.deathStreak.Clear();
      }

      this.calculateWeaponsMostKills();
      this.formatMessage("Most Weapon Kills");

      this.lastKill = null;
      this.ExecuteCommand("procon.protected.tasks.add", "CServerKillsSetRoundEnd", "5", "1", "1", "procon.protected.plugins.call", "CServerKills", "setRoundEnded");
    }

    public override void OnServerInfo(CServerInfo serverInfo)
    {
      /*
      for (int i = 0; i < teamNames.Count; i++)
        this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^0" + teamNames[i]);
      */

      if (this.needServerInfo)
      {
        this.currentMapName = this.GetMapByFilename(serverInfo.Map);

        for (int i = 0; i < this.currentMapName.TeamNames.Count; i++)
        {
          if (this.currentMapName.TeamNames[i].Playlist == serverInfo.GameMode)
            this.teamNames.Add(GetLocalized("Default Team", this.currentMapName.TeamNames[i].LocalizationKey));
        }

        this.gameMode = serverInfo.GameMode;
        
        if (this.enableRoundStats == enumBoolYesNo.Yes && this.displayAceSquad == enumBoolYesNo.Yes && this.aceSquadDisplayType == "vars.serverMessage")
          this.sendAceSquad();

        this.needServerInfo = false;
      }
    }

    public override void OnServerMessage(string serverMessage)
    {
      if (this.needOriginalMessage && !this.needMessageReset)
      {
        this.originalServerMessage = serverMessage;

        /*
        if (this.gameMode.Contains("Conquest") || this.gameMode.Contains("TeamDeathMatch") || this.gameMode.Contains("Domination") || this.gameMode.Contains("GunMaster") || this.gameMode.Contains("TankSuperiority") || this.gameMode.Contains("Scavenger") || this.gameMode.Contains("CaptureTheFlag") || this.gameMode.Contains("AirSuperiority") || this.gameMode.Contains("Obliteration") || this.gameMode.Contains("Elimination"))
          this.ExecuteCommand("procon.protected.send", "vars.serverMessage", this.aceSquadMessage.Replace("%players%", this.aceSquadPlayersString).Replace("%squad%", GetLocalized("Squad" + this.aceSquadSquadID.ToString(), String.Format("global.Squad{0}", this.aceSquadSquadID.ToString()))).Replace("%team%", ((this.aceSquadTeamID == 1) ? "US" : "RU")).Replace("%score%", this.highestSquadScore.ToString()));
        else if (this.gameMode.Contains("Rush"))
          this.ExecuteCommand("procon.protected.send", "vars.serverMessage", this.aceSquadMessage.Replace("%players%", this.aceSquadPlayersString).Replace("%squad%", GetLocalized("Squad" + this.aceSquadSquadID.ToString(), String.Format("global.Squad{0}", this.aceSquadSquadID.ToString()))).Replace("%team%", ((this.aceSquadTeamID == 1) ? "Attackers" : "Defenders")).Replace("%score%", this.highestSquadScore.ToString()));
        */

        this.ExecuteCommand("procon.protected.send", "vars.serverMessage", this.aceSquadMessage.Replace("%players%", this.aceSquadPlayersString).Replace("%squad%", GetLocalized("Squad" + this.aceSquadSquadID.ToString(), String.Format("global.Squad{0}", this.aceSquadSquadID.ToString()))).Replace("%team%", this.teamNames[this.aceSquadTeamID]).Replace("%score%", this.highestSquadScore.ToString()));

        if (this.displayAceSquadToConsole == enumBoolYesNo.Yes)
          this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^0" + this.aceSquadMessage.Replace("%players%", this.aceSquadPlayersString).Replace("%squad%", GetLocalized("Squad" + this.aceSquadSquadID.ToString(), String.Format("global.Squad{0}", this.aceSquadSquadID.ToString()))).Replace("%team%", this.teamNames[this.aceSquadTeamID]).Replace("%score%", this.highestSquadScore.ToString()));

        if (this.enableRoundStats == enumBoolYesNo.Yes && this.displayAceSquad == enumBoolYesNo.Yes && this.aceSquadDisplayType == "vars.serverMessage")
          this.ExecuteCommand("procon.protected.tasks.add", "CServerKillsResetServerMessage", "80", "1", "1", "procon.protected.plugins.call", "CServerKills", "resetServerMessage");

        this.needMessageReset = true;
        this.needOriginalMessage = false;
      }
    }

    public override void OnGlobalChat(string speaker, string message)
    {

      if (message == "!test")
        this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^0" + GetLocalized("Default Team", String.Format("global.Team{0}", "0")));
      else if (message == "!test1")
        this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^0" + GetLocalized("Default Team", String.Format("global.Team{0}", "1")));
      else if (message == "!test2")
        this.ExecuteCommand("procon.protected.pluginconsole.write", "^3^b[Server Kills] ^n^0" + GetLocalized("Default Team", String.Format("global.Team{0}", "2")));
    }

  }
}

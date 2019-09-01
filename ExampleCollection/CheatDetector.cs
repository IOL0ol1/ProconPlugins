/* support:
 * https://forum.myrcon.com/showthread.php?5414-CheatDetector-0-4-3-0
 * 
 * grizzlybeer
 * https://forum.myrcon.com/member.php?13930-grizzlybeer
 * 
 * TODO:
 *  
 */

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Players;

using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using PRoCon.Core.Plugin.Commands;
using MySql.Data.MySqlClient;
using System.Web;

namespace PRoConEvents
{
    public class CheatDetector : PRoConPluginAPI, IPRoConPluginInterface
    {
        #region class, contructor 
       
        private DateTime lastupdatecheck = DateTime.Now.AddHours(-4);

        private object killlock;
        private enumBoolYesNo DoStatsCheck;
        private enumBoolYesNo DoLiveCheck;

        private WebClient webclient;
        private DBConnect DB;
        private List<string> tempplayertocheck;
        private enumBoolYesNo test;

        private enumBoolYesNo publicintegration;
        
        private List<string> whitelist;
        private enumBoolYesNo syncreservedslots;
        //private enumBoolYesNo excludereservedslots;

        private enumBoolYesNo enablekick; //enable action kick
        private string kicktext; //text to be displayed to the kicked player
        private enumBoolYesNo enabletban; //enable action tban
        private string tbantext; //text to be displayed to the tbanned player
        private int tbantime; //tban length
        private enumBoolYesNo enableban; //enable action ban
        private string bantext; //text to be displayed to the banned player  
        private enumBoolYesNo banbyname;
        private enumBoolYesNo banbyeaguid;
        private enumBoolYesNo banbypbguid;
        private enumBoolYesNo enableingamenotify; //enable action notify (std)
        private enumBoolYesNo enableingameprivpyell;
        private enumBoolYesNo enableingameprivpsay;
        private enumBoolYesNo enableingamepubnotifysay;
        private enumBoolYesNo enableingamepubnotifyyell;
        private string publicnotifysaytext;
        private string publicnotifyyelltext;
        private string privatenotifypsaytext;
        private string privatenotifypyelltext;
        private int ingameprivnotifytime; //display time
        private int ingamepublicnotifytime; //display time
        private List<string> ingameaccountnamepyell; //admin ingame username to display notify to
        private List<string> ingameaccountnamepsay; //admin ingame username to display notify to
        private double debuglevel; //spam debug output to console (0-5)
        //private BattlelogClient bclient;
        private List<String> myplayerlist;
        private bool SyncResetPlayerLists;
        private int SyncResetListsHelper;
        //private int cheatindex;
        private enumBoolYesNo advanced;
        //private double yellowacc;
        //private double orangeacc;
        //private double redacc;
        //private double yellowhspk;
        //private double orangehspk;
        //private double redhspk;
        //private double yellowkph;
        //private double orangekph;
        //private double redkph;
        //private double orangekpm;
        //private double redkpm;
        private double orangespm;
        private double redspm;
        private double yellowflagcount;
        private double orangeflagcount;
        private double redflagcount;
        private int minimumkills;
        //private String result;
        //private string playername;

        private Dictionary<string, CPlayerInfo> m_dicPlayerInfo;
        private Dictionary<string, CPunkbusterInfo> m_dicPBPlayerInfo;
        private List<string> queue;
        private DateTime lastcheck;
        private DateTime viplastcheck;
        private DateTime lastcheck2;
        private DateTime lastdbsend;
        private int batchsize;
        private double batchtimeout;
        private double batchtimeoutbase;
        private double batchtimeoutnormal;
        //private double batchtimeoutsleep;
        private double batchtimeoutmax;
        private double batchtimeoutmodinc;
        private double batchtimeoutmoddec;

        private enumBoolYesNo logtofile;
        private string logfilename;

        private enumBoolYesNo timeouttest;

        private Dictionary<string, int> playerErrors;
        private int retries;
        private Dictionary<string, Dictionary<string, string>> CacheDict;
        private enumBoolYesNo useCache;
        //private string magic;

        private enumBoolYesNo lazyQueue;

        private String UserAgent;

        private enumBoolYesNo streamtomaster;

        public string queueServer;
        public string queueDB;
        public string queueUser;
        public string queuePwd;

        private enumBoolYesNo DoConnect;

        private List<PlayerCheckInfo> queuefordb;

        private Dictionary<string, double> kills;
        private Dictionary<string, double> hs;
        private Dictionary<string, DateTime> firstkill;

        private double maxhsklive;
        private double maxkmlive;
        private double minkillsl;

        private enumBoolYesNo strictmode;

        private string Servertype;
        private string servername;
        private string serveripandport;

        private enumBoolYesNo Check4Update;

        public CheatDetector()
        {
            Check4Update = enumBoolYesNo.Yes;

            Servertype = "AUTOMATIC";

            killlock = new object();
            DoStatsCheck = enumBoolYesNo.Yes;
            DoLiveCheck = enumBoolYesNo.Yes;

            publicintegration = enumBoolYesNo.Yes;
            webclient = new WebClient();            
            
            DB = new DBConnect(this);
            enablekick = enumBoolYesNo.No;
            kicktext = "SUSPECTED CHEATING: %playername% %suspiciousstat%";
            enabletban = enumBoolYesNo.Yes;
            tbantext = "SUSPECTED CHEATING: %playername% %suspiciousstat%";
            tbantime = 15;
            enableban = enumBoolYesNo.No;
            bantext = "SUSPECTED CHEATING: %playername% %suspiciousstat%";
            banbyname = enumBoolYesNo.No;
            banbyeaguid = enumBoolYesNo.Yes;
            banbypbguid = enumBoolYesNo.No;
            enableingamenotify = enumBoolYesNo.Yes;
            enableingamepubnotifysay = enumBoolYesNo.Yes;
            enableingamepubnotifyyell = enumBoolYesNo.No;
            enableingameprivpyell = enumBoolYesNo.No;
            enableingameprivpsay = enumBoolYesNo.No;
            ingameaccountnamepyell = new List<string>();
            ingameaccountnamepyell.Add("onegrizzlybeer");
            ingameaccountnamepsay = new List<string>();
            ingameaccountnamepsay.Add("onegrizzlybeer");
            publicnotifysaytext = "CheatDetector - SUSPECTED CHEATING: %playername% %suspiciousstat%";
            publicnotifyyelltext = "CheatDetector - SUSPECTED CHEATING: %playername% %suspiciousstat%";
            privatenotifypsaytext = "CheatDetector - SUSPECTED CHEATING: %playername% %suspiciousstat%";
            privatenotifypyelltext = "CheatDetector - SUSPECTED CHEATING: %playername% %suspiciousstat%";
            ingameprivnotifytime = 30;
            ingamepublicnotifytime = 30;
            debuglevel = 1;
            //bclient = new BattlelogClient(this);
            myplayerlist = new List<String>();
            SyncResetPlayerLists = false;
            SyncResetListsHelper = 0;
            //cheatindex = 0;
            whitelist = new List<string>();
            syncreservedslots = enumBoolYesNo.Yes;
            //excludereservedslots = enumBoolYesNo.Yes;

            playerErrors = new Dictionary<string, int>();
            retries = 1;
            tempplayertocheck = new List<string>();
            test = enumBoolYesNo.No;
            advanced = enumBoolYesNo.No;
            //yellowacc = 2.4;
            //orangeacc = 3.2;
            //redacc = 3.7;
            //yellowhspk = 1.83;
            //orangehspk = 2.7;
            //redhspk = 3.2;
            //yellowkph = 1.84;
            //orangekph = 2.7;
            //redkph = 3.2;
            //orangekpm = 5.0;
            //redkpm = 8.0;
            orangespm = 1200;
            redspm = 2000;
            yellowflagcount = 1.0;
            orangeflagcount = 2.5;
            redflagcount = 5.0;
            minimumkills = 99;
            
            minkillsl = 30;
            maxhsklive = 0.94;
            maxkmlive = 8.0;

            m_dicPlayerInfo = new Dictionary<string, CPlayerInfo>();
            m_dicPBPlayerInfo = new Dictionary<string, CPunkbusterInfo>();
            queue = new List<string>();
            lastcheck = new DateTime();
            lastcheck = DateTime.Now.AddMinutes(-3.0);
            viplastcheck = new DateTime();
            viplastcheck = DateTime.Now.AddMinutes(-3.0);
            lastcheck2 = new DateTime();
            lastcheck2 = DateTime.Now.AddMinutes(-3.0);
            lastdbsend = new DateTime();
            lastdbsend = DateTime.Now.AddMinutes(-3.0);
            batchsize = 1;
            batchtimeoutbase = 29.0;
            batchtimeoutnormal = 29.0;
            batchtimeout = batchtimeoutnormal;
            //batchtimeoutsleep = 150.0;
            batchtimeoutmax = 300.0;
            batchtimeoutmodinc = 60.0;
            batchtimeoutmoddec = 30.0;

            logtofile = enumBoolYesNo.No;
            logfilename = "Plugins/CDLogFile.txt";

            timeouttest = enumBoolYesNo.No;

            CacheDict = new Dictionary<string, Dictionary<string, string>>();
            useCache = enumBoolYesNo.Yes;
            //magic = "|" + GenerateRandomString() + "|";
            //magic = "|sgdfgdsgdfgdfgdfg|";

            lazyQueue = enumBoolYesNo.No;

            UserAgent = "Mozilla/5.0 (compatible; PRoCon 1; CheatDetector)";
            webclient.Headers.Add("user-agent", UserAgent);

            streamtomaster = enumBoolYesNo.No;
            queueServer = "";
            queueDB = "";
            queueUser = "";
            queuePwd = "";
            DoConnect = enumBoolYesNo.No;
            queuefordb = new List<PlayerCheckInfo>();

            kills = new Dictionary<string, double>();
            hs = new Dictionary<string, double>();
            firstkill = new Dictionary<string, DateTime>();

            strictmode = enumBoolYesNo.No;

            servername = "";
            serveripandport = "";
        }
        #endregion

        #region Description, LevelLoaded, PluginEnable, PluginDisable, PluginLoaded
        public string GetPluginName()
        {
            return "CheatDetector";
        }

        public string GetPluginVersion()
        {
            return "0.5.2.1";
        }

        public string GetPluginAuthor()
        {
            return "onegrizzlybeer";
        }

        public string GetPluginWebsite()
        {
            return "www.phogue.net/forumvb/showthread.php?5414-CheatDetector-%28CD%29";
        }

        public string GetPluginDescription()
        {
            return @"
<h1>CD - CheatDetector [BF3]</h1>
<p>If you like this plugin, please donate using the button below. Any amount is considered helpful. Thank you :-)<br>
<form action=""https://www.paypal.com/cgi-bin/webscr"" method=""post"" target=""_blank"">
<input type=""hidden"" name=""cmd"" value=""_s-xclick"">
<input type=""hidden"" name=""hosted_button_id"" value=""CL3WW8C3J7BW6"">
<input type=""image"" src=""https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif"" border=""0"" name=""submit"" alt=""PayPal - The safer, easier way to pay online!"">
<img alt="" border=""0"" src=""https://www.paypalobjects.com/de_DE/i/scr/pixel.gif"" width=""1"" height=""1"">
</form></p>
<h2>Description</h2>
<h3>Stats Check</h3>
<p>This plugin tries to detect cheaters by their BF3 Battlelog Stats. This is done in the following way:<br>
When a new player has joined (by detecting new entries in the playerlist. not by the playerjoined event, to prevent spamming) his stats are fetched from his Battlelog page (3 requests).<br>
This is done for 1 player every 30 seconds, to not trigger any restrictions on the Battlelog side using a fifo-queue (first in, first out. or fcfs (first come, first served))<br>
Then every weapons stat (with more than 130 kills, or more headshots than kills with more than 20 kills) is checked against a collection of averages to decide if its normal, suspicious or impossible.<br><br>

You can also use BattlelogCache to reduce the traffic on Battlelog.<br><br>

The following stats are checked:<br>
Player SPM (score per minute)<br>
Weapon k/m (KPM, kills per minute)<br>
Weapon hs/k (HSPK, headshots per kill)<br>
Weapon k/h (KPH, kills per hit)<br>
Weapon accuracy<br><br>

Strict Mode: A player is considered cheating if he has 4 suspicious stats or 1 IMPOSSIBLE stat.<br>
Normal Mode: A player is considered cheating if he has 5 suspicious stats or 1 IMPOSSIBLE stat and 1 suspicious stat.<br>
Example:
<pre><code>
1:  [10:10:13 19] CD - IMPOSSIBLE STATS: Mong0lFPS M60 k/h: 100% (5,01)
    [10:10:13 19] CD - IMPOSSIBLE STATS: Mong0lFPS M60 k/m: 18,22 (16,71)
    [10:10:13 19] CD - IMPOSSIBLE STATS: Mong0lFPS M240 k/h: 100% (4,98)
    [10:10:13 21] CD - IMPOSSIBLE STATS: Mong0lFPS M240 k/m: 15,81 (14,64)
5:  [10:10:13 26] CD - IMPOSSIBLE STATS: Mong0lFPS spm: 9475,55
    [10:10:13 26] CD - SUSPECTED CHEATING: Mong0lFPS 99%
    [10:10:13 26] ------------------------
    [10:10:43 46] CD - suspicious stats: Kershalt007 Pecheneg hs/k: 67,69% (2,77)
    [10:10:43 46] CD - IMPOSSIBLE STATS: Kershalt007 Pecheneg k/m: 10,44 (8,7)
10: [10:10:43 47] CD - suspicious stats: Kershalt007 M60 hs/k: 74,26% (3,06)
    [10:10:43 47] CD - IMPOSSIBLE STATS: Kershalt007 M60 k/m: 10,76 (9,88)
    [10:10:43 47] CD - suspicious stats: Kershalt007 M27 hs/k: 72,02% (2,97)
    [10:10:43 47] CD - suspicious stats: Kershalt007 XP2 LSAT hs/k: 69,3% (3,15)
    [10:10:43 50] CD - SUSPECTED CHEATING: Kershalt007 99%
15: [10:10:43 50] ------------------------
    [10:11:14 59] CD - IMPOSSIBLE STATS: UMadPanda PDR hs/k: 137,36% (6,53)
    [10:11:14 60] CD - IMPOSSIBLE STATS: UMadPanda Taurus 44 hs/k: 163,16% (6,7)
    [10:11:14 60] CD - IMPOSSIBLE STATS: UMadPanda F2000 hs/k: 125,06% (5,28)
    [10:11:14 60] CD - IMPOSSIBLE STATS: UMadPanda P90 hs/k: 87,93% (4,4)
20: [10:11:14 65] CD - SUSPECTED CHEATING: UMadPanda 99%
    [10:11:14 65] ------------------------
</code></pre>
<br>
As you can see in the first line, the weapon ""M60"" on the player ""Mong0lFPS"" has a ""k/h"" (kills per hit ratio) of 100% (422 hits, 422 kills), which is 5,01 times higher than the average k/h on the M60. This is considered impossible.<br>
Line 6 shows the summary of all checks for the player ""Mong0lFPS"". 99% cheating so this player would be kicked/banned or an admin notified.<br>
Weapons with more headshots than kills (except shotguns, rockets and the like) and more than 20 kills will be treated as IMPOSSIBLE (=cheating).<br><br>
</p>

<h3>Live Check</h3>
<p>
From version 0.4 CheatDetector features a Live Protection.<br>
Every 5 minutes all players (except players in the whitelist) will be checked for their headshots per kills (hs/k) and kills per minute (kpm) during their time on your server.<br>
A player is considered cheating if he has > 91% hs/k or > 8 kpm with at least 30 kills. This is basically (average over all weapons) the same limit CheatDetector uses for Stats Check.<br>
Note that Live Check does not distinguish between weapons or vehicles. (This is not reported by the game server)<br>
Example:
<pre><code>
[16:15:02] CD - LiveCheck SUSPECTED CHEATING OMonkeyOoOoOo kills: 30 hs: 30 time: 9,59 hs/k: 100% kpm: 3,12
</code></pre>
<br>
If 0. Enable Live Check is set to ""Yes"" this player will be kicked (or banned, depending on your settings).<br>
For the message displayed to the kicked player %suspiciousstat% will be replaced with ""too many hs/k or k/min (CD - LiveCheck)"" instead of a certain weapon.<br>
This will protect your server from cheaters with clean (or resetted) stats. You can see the checks for non-cheaters on debug level >=3.<br><br>
</p>

<h3>Stream players to 247FairPlay.com</h3>
<p>
http://www.247FairPlay.com is a website providing stats, forum and ban appeal for my server and my bans.<br>
Since version 0.4 you can stream the players on your server to a database at 247FairPlay.com. This DB will be used to check the players in addition to the CheatDetector plugin on your server. The plugin will not be blocked by this and errors (for example: you cant reach 247fairplay.com) wont show up in the log. its ""fire and forget"".<br>
If enabled CheatDetector will contact 247FairPlay.com every 30 minutes. You can see the requests on debug level >=3.<br>
These checks include extended stats check, linked accounts, history check to name just a few (note that some of these features are still in development)<br>
Detected cheater will be banned through the metabans.com account @CheatDetector (http://metabans.com/CheatDetector)<br><br>

How do you make use of these checks? Simple: install metabans plugin and follow @CheatDetector to enforce the bans on the detected cheaters. Depending on the usage (and several more restrictions), it might take some time to get checks done.<br><br>

You will find further information about the new CheatDetector features in the CheatDetector thread at Procon forums, at 247FairPlay.com forum or here as it develops :-)<br><br>
</p>

<h2>THANK YOU</h2>
THX & Credits go to the following people (and more i'm sure):<br>
all Procon Devs for Procon and their API :-)<br>
i-stats.net & symthic.com & bf3stats & cheatometer for their massive DBs of weapon and player values and known cheaters<br>
Panther for showing how to use that API and other Plugin stuff<br>
Phil & Phogue & MorpheusX for all their support and tips and letting me nag em :-)<br>
EBastard & HexaCanon & PapaCharlie & ty_ger & dyn & IAF SDS & all the other for testing, reporting & various tips<br>
Big thx again to PapaCharlie for showing me how to use the Battlelog Cache<br><br>

<h2>Support & Feedback</h2>
<a href=""http://www.phogue.net/forumvb/showthread.php?5414-CheatDetector-%28CD%29"" target=_blank>http://www.phogue.net/forumvb/showthread.php?5414-CheatDetector-(CD)</a>
<h2>Plugin (recommended) Settings</h2>
<blockquote>
0. Ban Method and Whitelist and BattlelogCache<br>
Servertype: BF3/BF4/Automatic. (Automatic)<br>
TempBan/Ban by name: Not recommended. A Player can easily change their name and join again. (No)<br>
TempBan/Ban by EA GUID: Recommended. This ID is linked to the player's EA account. Works best with Metabans (Yes)<br>
TempBan/Ban by PB GUID: Recommended. This ID is linked to the player's cd key (No)<br>
Whitelist: List of players (ingame-names without clantag) not to check<br>
Sync ReservedSlots/ServerVIPs: Automatically syncronize the players in the ReservedSlot list with the Plugin Whitelist (Yes)<br>
Use BattlelogCache if available: Use the Battlelog Cache to reduce traffic (Yes)<br>
Stream Players to 247FairPlay.com: If enabled CheatDetector will contact http://www.247fairplay.com/cheatdetector.php every 30 minutes to add the players on your server to a database to do stats checks and various other checks. If a cheater is detected, he will be banned through the MetaBans Account @CheatDetector (http://metabans.com/CheatDetector). (Yes)<br>
Enable Stats Check: Enable the stats check through battlelog. (Yes)<br>
Enable Strict Mode: When enabled: 1 IMPOSSIBLE or 4 suspicious stats is considered cheating (=kick/ban). When set to NO: 1 IMPOSSIBLE and 1 suspicous or 5 suspicious stats is considered cheating. (NO)<br>
Enable Live Check: Enable the ckecking of the players hs/k and kpm on your server. (Yes)<br><br>

2. Kick<br>
Enable Kick: No<br>
Message to be displayed to the kicked player: %playername% will be replaced with the players name. %pbguid% will be replaced with the players PunkBuster GUID. %eaguid% will be replaced with the players EA GUID. %suspiciousstat% will be replaced with the most significant detected stat. (SUSPECTED CHEATING: %playername% %suspiciousstat%)<br><br>

3. TBan<br>
Enable Temp: If temp ban is enabled, no kick will happen (Yes)<br>
Message to be displayed to the temp banned player: %playername% will be replaced with the players name. %pbguid% will be replaced with the players PunkBuster GUID. %eaguid% will be replaced with the players EA GUID. %suspiciousstat% will be replaced with the most significant detected stat. (SUSPECTED CHEATING: %playername% %suspiciousstat%)<br>
Length of Temp Ban (min): Length of temp ban in minutes (15)<br><br>

4. Ban<br>
Enable ban: If permanent ban is enabled, no temp ban or kick will happen (No)<br>
Message to be displayed to the banned player: %playername% will be replaced with the players name. %pbguid% will be replaced with the players PunkBuster GUID. %eaguid% will be replaced with the players EA GUID. %suspiciousstat% will be replaced with the most significant detected stat. (SUSPECTED CHEATING: %playername% %suspiciousstat%)<br><br>

5. Notify<br>
Enable ingame notification: Yell a message to an admin (or any other player) when a player is detected by this plugin (Yes)<br>
Enable private/public notification: Enable the desired type(s) of notification<br>
Ingame username: player to receive the ingame notification<br>
Message to be displayed: Message to be displayed to the ingame playername (CheatDetector - SUSPECTED CHEATING: %playername% %suspiciousstat%)<br>
Time to display (sec): Time to display the ingame notification in seconds (30)<br><br>

6. Debug<br>
Debug Level (0-5): Debug level adjusting how many debug messages you will see in the plugin console log (1)<br>
0 - no messages at all (quiet)<br>
1 - only detections/kicks/bans will be displayed<br>
2 - will also show log entries for normal players and some queue timeout operation when neccessary<br>
3 - will also show the queue size and number of players enqueued, adding and removing of players to the playerlist and queue, skipping of whitelisted players, queue timeout operations<br>
4 - will also show checked and skipped weapons, a lot more internal operations, ""expected"" errors (dont worry ;-))<br>
5 - just for development and testing<br><br>
Automatic Update Check: Check for Plugin Updates every 3 hours. (Yes)<br>
Log to file: Log plugin output to a file (CDLogFile.txt) in the Plugin/BF3 directory. Not recommended, this file may get very big and slow your procon down. Only use it to log errors and only use it local (No)<br>
Filename/Path: Filename and path of the logfile relative to the Procon executable (Plugins/BF3/PBSSELogFile.txt)<br>
Advanced: DO NOT PLAY WITH THESE VALUES UNLESS INSTRUCTED BY ME OR YOU WILL BREAK THE PLUGIN (NO)<br><br>

<b>DO NOT EDIT ANY OF THE BELOW SETTINGS - LAST WARNING - VERBOTEN :)</b><br>
Lazy Queue: When enabled, the plugin will also check players that left the server while being enqueued (No)<br>
Orange Flag Score per Min: Set the SPM needed to raise a suspicious flag (1200)<br>
Red Flag Score per Min: Set the SPM needed to raise a IMPOSSIBLE flag (2000)<br>
Min Kills: Minimum Kills needed for a weapon to be analyzed (200)<br>
Min Kills (live): Minimum Kills needed for a player to be checked during Live Check (30)<br>
Max hsklive: Headshots per Kill Percentage needed to raise a IMPOSSIBLE flag during Live Check (0.91)<br>
Max kmlive: Kills per Minute needed to raise a IMPOSSIBLE flag during Live Check (8)<br>
Name: List of Players to check when TEST (Option below) is set to Yes<br>
Test: Check Players listed in NAME (Option Above) (No)<br>
Retries: Retry stats fetching X times when failed (1)<br>
Timeout Test: Let all requests to fetch stats fail. Just for testing (No)<br>
Stream to Master: Settings for streaming to my mysql server. (No)<br>
</blockquote>

<h2>Version History (Changelog):</h2>
<blockquote>
<h4>0.5.2.1</h4>
-Fix: Added placeholders for new Final Stand DLC weapons.<br>
-Fix: Other minor fixes.<br><br>
<h4>0.5.2.0</h4>
-NEW: Adjusted cheat detection algorithm to better fit the new averages.<br>
-Fix: Updated weapon averages (for Dragon's Teeth DLC weapons too).<br>
-Fix: Added Placeholder for new Knife (KNIFEWEAVER).<br>
-Fix: When new weapons get added to Battlelog, plugin will no longer spam endless msgs on debug level 1.<br><br>
<h4>0.5.1.2</h4>
-Fix: Added placeholders for Dragon's Teeth DLC weapons<br>
-Fix: Increased HS/K used in Live Check to 94%<br>
-Fix: Fixed some issues with weapon names in ban msgs<br><br>
<h4>0.5.1.1</h4>
-Fix: Added missing weapons for Naval Strike DLC<br><br>
<h4>0.5.1.0</h4>
-NEW: Updated used averages (i-stats.net)<br>
-Fix: Bug in streaming caused some servers to not transmit its additional info (servername, ip, port, servertype) properly<br>
-Fix: Added missing weapons for Second Assault DLC<br><br>
<h4>0.5.0.1</h4>
-Fix: Persona ID could not be found for some players.<br>
-Fix: Changed Include/Exclude VIPs to Sync VIPs<br>
-Fix: Updated description<br><br>
<h4>0.5.0.0</h4>
-NEW: BF4 Compatibility<br>
-NEW: Automatic update check<br>
-Fix: Increased SPM limits to improve the overall detection<br>
-Fix: Extended Infos streamed to 247FairPlay.com for later use with stuff :-)<br><br>
<h4>0.4.4</h4>
-FIX: Strict Mode: When enabled 1 IMPOSSIBLE and 1 suspicious (or 5 suspicious) stats are considered cheating. When disabled 1 IMPOSSIBLE and 2 suspicious (or 6 suspicious) stats are considered cheating<br>
-FIX: Fixed small bug handling the notification psay/pyell.<br><br>
<h4>0.4.3</h4>
-FIX: More headshots than kills will be treated as headshots equal to kills to reduce the chance of false detections.<br>
-FIX: Increased minimum kills to 200 to reduce the chance of false detections.<br>
-FIX: Live Check: Increased minimum kills to 30 to reduce the chance of false detections.<br><br>
<h4>0.4.2</h4>
-NEW: Settings: Enable Strict Mode (When enabled: 1 IMPOSSIBLE or 4 suspicious stats is considered cheating (=kick/ban). When set to NO: 1 IMPOSSIBLE and 1 suspicous or 5 suspicious stats is considered cheating.)<br>
-FIX: More headshots than kills are only considered IMPOSSIBLE (or suspicious) if they exceed the average by 320% (270%). See https://forum.myrcon.com/showthread.php?5414-CheatDetector-(CD)&p=75230&viewfull=1#post75230.<br><br>
<h4>0.4.1</h4>
-FIX: Increased minimum kills needed for weapons with more headshots than kills back to 130.<br><br>
<h4>0.4</h4>
-NEW: Live Check: CheatDetector will now monitor the players on your server for headshots per kills and kills per minute. Cheating is considered >91% HS/K or >8 KPM with more than 20 kills. A check is made every 5 minutes. Whitelist and messages apply here too. (0. Ban Method and Whitelist and BattlelogCache -> Enable Live Check)<br>
-NEW: Stream Players to 247FairPlay.com: If enabled CheatDetector will contact http://www.247fairplay.com/cheatdetector.php every 30 minutes to add the players on your server to a database (the plugin will not be blocked by this and errors (for example: you cant reach 247fairplay.com) wont show up in the log. its ""fire and forget""). This DB will be used to do stats checks and various other checks. If a cheater is detected, he will be banned through the MetaBans Account @CheatDetector (http://metabans.com/CheatDetector).<br>
-FIX: Stats Check: Weapons with more headshots than kills (except shotguns, rockets and the like) and more than 20 kills will be treated as IMPOSSIBLE (=cheating).<br>
-NEW: Settings: Enable Live Check.<br>
-NEW: Settings: Enable Stats Check.<br>
-NEW: Settings: Stream Players to 247FairPlay.com<br>
-NEW: Settings: Exclude Non-ReservedSlots/ServerVIPs: will automatically remove players from the plugins whitelist that are not in your ReservedSlots List<br>
-FIX: Several internal code improvements.<br><br>
<h4>0.3</h4>
-NEW: You can now use Battlelog Cache to reduce traffic. (0. Ban Method and Whitelist and BattlelogCache -> Use BattlelogCache if available)<br>
-FIX: The Output on certain debug levels have been adjusted to be not so spamming.<br>
-FIX: 2 Typos in Weapon Names have been fixed (M1911 Tactical) which caused this weapon to not be analyzed for k/h and k/m.<br>
-FIX: Under certain circumtances endless ingame notification could happen. This is now fixed.<br>
-FIX: When a player is banned (TBan and Ban) a Kick will make sure the player is gone.<br>
-FIX: Added placeholders %pbguid% and %eaguid% to the messages.<br>
-FIX: Increased the max TBan time to 100 years (was 1 year), but you might as well use permanent there :-)<br>
-FIX: Several internal code improvements.<br><br>
<h4>0.2.1</h4>
-NEW: You can now setup the ingame notification (4. Notify) in detail.<br>
-FIX: Tiny update to the scores to find the best reason (most suspicious/impossible stat) to be used in various messages.<br>
-FIX: SPM is now only taken into account if the playtime is above 1 hour.<br>
-FIX: small code updates.<br><br>
<h4>0.2</h4>
-UPDATE: You can now download/update CheatDetector directly through your procon gui.<br><br>
<h4>0.1.2</h4>
-FIX: The plugin will now automatically detect when the BF3 Battlelog Request Limit is reached. Then it will increase the queue timeout. The timeout will then be decreased step by step to normal.<br>
-FIX: An error happened when there were multiple detections with the same score. This is now fixed<br>
-FIX: not that important error messages are now displayed properly (Debug level 2, 3, 4)<br><br>
<h4>0.1.1</h4>
-FIX: Plugin now identifies itself properly to Battlelog<br>
-NEW: Option to add multiple usernames to get notified (4. Notify -> ingame username)<br><br>
<h4>0.1</h4>
-Public release
</blockquote>
<h2>Known Issues</h2>
<blockquote>
<h4>Cheat Detection</h4>
Since this plugin tries to detect cheaters using their BF3 Battlelog stats, it is limited to that data. If a cheater got his stats reset, there is no way for this plugin (as of now) to detect the cheater.<br>
On the other hand, this plugin might detect someone as a cheater who is not cheating.<br>
Workaround (cheater not detected): None. Inform me about this issue. Maybe i will add other data sources for cheat detection in the future.<br>
Workaround (normal player detected as cheater): Add this player to your whitelist and inform me about this false positive.<br><br>
<h4>Whitelist</h4>
If you add a player to your ReservedSlots/ServerVIP List they might not show up in the whitelist setting immediately. However you can be sure they are added and processed. I think this is because Procon GUI does not reload the setting when its not changed in the GUI. Just reopen the Plugins Tab and they should be displayed. Or restart Procon on your PC (NOT the layer server, just the exe on your PC) and they will show up.<br>
Workaround: Not really needed, however if someone knows a solution i would like to know it too :-)<br><br>
<h4>Logfile</h4>
This file may get very big and slow your procon down. Only use it to log errors and only use it local. The directory must also exist and be writeable by your procon.
</blockquote>
";
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
        {
            RegisterEvents(GetType().Name,
                "OnListPlayers",
                "OnPlayerLeft",
                "OnReservedSlotsList",
                "OnLevelLoaded",
                "OnPunkbusterPlayerInfo",
                "OnPlayerKilled",
                "OnServerInfo"
                );
        }

        public void OnPluginEnable()
        {
            ExecuteCommand("procon.protected.pluginconsole.write", "^bCheatDetector ^2Enabled!");
            IsCacheEnabled(true);
        }

        public void OnPluginDisable()
        {
            ExecuteCommand("procon.protected.pluginconsole.write", "^bCheatDetector ^1Disabled =(");
        }

        public void OnLevelLoaded(string mapFileName, string Gamemode, int roundsPlayed, int roundsTotal)
        {
            if (SyncResetListsHelper > 3)
            {
                SyncResetPlayerLists = true;
                SyncResetListsHelper = 0;
            }
            else
            {
                SyncResetListsHelper++;
            }
        }
        #endregion

        #region settings read
        public List<CPluginVariable> GetDisplayPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("0. Ban Method and Whitelist and BattlelogCache|Servertype", "enum.Servertype(AUTOMATIC|BF3|BF4)", this.Servertype));
            lstReturn.Add(new CPluginVariable("0. Ban Method and Whitelist and BattlelogCache|TempBan/Ban by name", typeof(enumBoolYesNo), banbyname));
            lstReturn.Add(new CPluginVariable("0. Ban Method and Whitelist and BattlelogCache|TempBan/Ban by EA GUID", typeof(enumBoolYesNo), banbyeaguid));
            lstReturn.Add(new CPluginVariable("0. Ban Method and Whitelist and BattlelogCache|TempBan/Ban by PB GUID", typeof(enumBoolYesNo), banbypbguid));
            lstReturn.Add(new CPluginVariable("0. Ban Method and Whitelist and BattlelogCache|Whitelist", typeof(string[]), whitelist.ToArray()));
            lstReturn.Add(new CPluginVariable("0. Ban Method and Whitelist and BattlelogCache|Sync ReservedSlots/ServerVIPs", typeof(enumBoolYesNo), syncreservedslots));
            //lstReturn.Add(new CPluginVariable("0. Ban Method and Whitelist and BattlelogCache|Exclude Non-ReservedSlots/ServerVIPs", typeof(enumBoolYesNo), excludereservedslots));
            lstReturn.Add(new CPluginVariable("0. Ban Method and Whitelist and BattlelogCache|Use BattlelogCache if available", typeof(enumBoolYesNo), useCache));
            lstReturn.Add(new CPluginVariable("0. Ban Method and Whitelist and BattlelogCache|Stream Players to 247FairPlay.com", typeof(enumBoolYesNo), publicintegration));

            lstReturn.Add(new CPluginVariable("0. Ban Method and Whitelist and BattlelogCache|Enable Stats Check", typeof(enumBoolYesNo), DoStatsCheck));
            if (DoStatsCheck == enumBoolYesNo.Yes)
                lstReturn.Add(new CPluginVariable("0. Ban Method and Whitelist and BattlelogCache|Enable Strict Mode", typeof(enumBoolYesNo), strictmode));

            lstReturn.Add(new CPluginVariable("0. Ban Method and Whitelist and BattlelogCache|Enable Live Check", typeof(enumBoolYesNo), DoLiveCheck));
            
            lstReturn.Add(new CPluginVariable("1. Kick|Enable Kick", typeof(enumBoolYesNo), enablekick));
            if (enablekick == enumBoolYesNo.Yes)
                lstReturn.Add(new CPluginVariable("1. Kick|Message to be displayed to the kicked player", kicktext.GetType(), kicktext));

            lstReturn.Add(new CPluginVariable("2. TBan|Enable Temp Ban", typeof(enumBoolYesNo), enabletban));
            if (enabletban == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("2. TBan|Message to be displayed to the temp banned player", tbantext.GetType(), tbantext));
                lstReturn.Add(new CPluginVariable("2. TBan|Length of Temp Ban (min)", tbantime.GetType(), tbantime));
            }

            lstReturn.Add(new CPluginVariable("3. Ban|Enable Ban", typeof(enumBoolYesNo), enableban));
            if (enableban == enumBoolYesNo.Yes)
                lstReturn.Add(new CPluginVariable("3. Ban|Message to be displayed to the banned player", bantext.GetType(), bantext));

            lstReturn.Add(new CPluginVariable("4. Notify|Enable ingame notification", typeof(enumBoolYesNo), enableingamenotify));
            if (enableingamenotify == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("4. Notify|Enable public ingame notification say", typeof(enumBoolYesNo), enableingamepubnotifysay));
                if (enableingamepubnotifysay == enumBoolYesNo.Yes)
                    lstReturn.Add(new CPluginVariable("4. Notify|Message to be displayed (say)", publicnotifysaytext.GetType(), publicnotifysaytext));
                lstReturn.Add(new CPluginVariable("4. Notify|Enable public ingame notification yell", typeof(enumBoolYesNo), enableingamepubnotifyyell));
                if (enableingamepubnotifyyell == enumBoolYesNo.Yes)
                {
                    lstReturn.Add(new CPluginVariable("4. Notify|Time to display (sec) (yell)", ingamepublicnotifytime.GetType(), ingamepublicnotifytime));
                    lstReturn.Add(new CPluginVariable("4. Notify|Message to be displayed (yell)", publicnotifyyelltext.GetType(), publicnotifyyelltext));
                }
                lstReturn.Add(new CPluginVariable("4. Notify|Enable private ingame notification psay", typeof(enumBoolYesNo), enableingameprivpsay));
                if (enableingameprivpsay == enumBoolYesNo.Yes)
                {
                    lstReturn.Add(new CPluginVariable("4. Notify|Ingame username (psay)", typeof(string[]), ingameaccountnamepsay.ToArray()));
                    lstReturn.Add(new CPluginVariable("4. Notify|Message to be displayed (psay)", privatenotifypsaytext.GetType(), privatenotifypsaytext));
                }
                lstReturn.Add(new CPluginVariable("4. Notify|Enable private ingame notification pyell", typeof(enumBoolYesNo), enableingameprivpyell));
                if (enableingameprivpyell == enumBoolYesNo.Yes)
                {
                    lstReturn.Add(new CPluginVariable("4. Notify|Time to display (sec) (pyell)", ingameprivnotifytime.GetType(), ingameprivnotifytime));
                    lstReturn.Add(new CPluginVariable("4. Notify|Ingame username (pyell)", typeof(string[]), ingameaccountnamepyell.ToArray()));
                    lstReturn.Add(new CPluginVariable("4. Notify|Message to be displayed (pyell)", privatenotifypyelltext.GetType(), privatenotifypyelltext));
                }
            }

            lstReturn.Add(new CPluginVariable("5. Debug|Debug Level (0-5)", debuglevel.GetType(), debuglevel));
            lstReturn.Add(new CPluginVariable("5. Debug|Automatic Update Check", typeof(enumBoolYesNo), Check4Update));
            lstReturn.Add(new CPluginVariable("5. Debug|Log to file", typeof(enumBoolYesNo), logtofile));
            if (logtofile == enumBoolYesNo.Yes)
                lstReturn.Add(new CPluginVariable("5. Debug|Filename/Path", logfilename.GetType(), logfilename));

            lstReturn.Add(new CPluginVariable("5. Debug|Advanced", typeof(enumBoolYesNo), advanced));
            if (advanced == enumBoolYesNo.Yes)
            {                
                lstReturn.Add(new CPluginVariable("5. Debug|Lazy Queue", typeof(enumBoolYesNo), lazyQueue));
                //lstReturn.Add(new CPluginVariable("6. Debug|Yellow Flag Accuracy", yellowacc.GetType(), yellowacc));
                //lstReturn.Add(new CPluginVariable("5. Debug|Orange Flag Accuracy", orangeacc.GetType(), orangeacc));
                //lstReturn.Add(new CPluginVariable("5. Debug|Red Flag Accuracy", redacc.GetType(), redacc));
                //lstReturn.Add(new CPluginVariable("6. Debug|Yellow Flag Headshots per Kill", yellowhspk.GetType(), yellowhspk));
                //lstReturn.Add(new CPluginVariable("5. Debug|Orange Flag Headshots per Kill", orangehspk.GetType(), orangehspk));
                //lstReturn.Add(new CPluginVariable("5. Debug|Red Flag Headshots per Kill", redhspk.GetType(), redhspk));
                //lstReturn.Add(new CPluginVariable("6. Debug|Yellow Flag Kills per Hit", yellowkph.GetType(), yellowkph));
                //lstReturn.Add(new CPluginVariable("5. Debug|Orange Flag Kills per Hit", orangekph.GetType(), orangekph));
                //lstReturn.Add(new CPluginVariable("5. Debug|Red Flag Kills per Hit", redkph.GetType(), redkph));
                //lstReturn.Add(new CPluginVariable("5. Debug|Orange Flag Kills per Min", orangekpm.GetType(), orangekpm));
                //lstReturn.Add(new CPluginVariable("5. Debug|Red Flag Kills per Min", redkpm.GetType(), redkpm));
                lstReturn.Add(new CPluginVariable("5. Debug|Orange Flag Score per Min (update1)", orangespm.GetType(), orangespm));
                lstReturn.Add(new CPluginVariable("5. Debug|Red Flag Score per Min (update1)", redspm.GetType(), redspm));
                lstReturn.Add(new CPluginVariable("5. Debug|Min Kills (update2)", minimumkills.GetType(), minimumkills));
                lstReturn.Add(new CPluginVariable("5. Debug|Min Kills Live (update1)", minkillsl.GetType(), minkillsl));
                lstReturn.Add(new CPluginVariable("5. Debug|Max hsklive", maxhsklive.GetType(), maxhsklive));
                lstReturn.Add(new CPluginVariable("5. Debug|Max kmlive", maxkmlive.GetType(), maxkmlive));
                lstReturn.Add(new CPluginVariable("5. Debug|Name", typeof(string[]), tempplayertocheck.ToArray()));
                lstReturn.Add(new CPluginVariable("5. Debug|Test", typeof(enumBoolYesNo), test));
                lstReturn.Add(new CPluginVariable("5. Debug|Retries", retries.GetType(), retries));                
                lstReturn.Add(new CPluginVariable("5. Debug|Timeout Test", typeof(enumBoolYesNo), timeouttest));
                lstReturn.Add(new CPluginVariable("5. Debug|Stream to Master", typeof(enumBoolYesNo), streamtomaster));

                if (streamtomaster == enumBoolYesNo.Yes)
                {
                    lstReturn.Add(new CPluginVariable("5. Debug|Server", queueServer.GetType(), queueServer));
                    lstReturn.Add(new CPluginVariable("5. Debug|DB", queueDB.GetType(), queueDB));
                    lstReturn.Add(new CPluginVariable("5. Debug|User", queueUser.GetType(), queueUser));
                    lstReturn.Add(new CPluginVariable("5. Debug|Pass", queuePwd.GetType(), queuePwd));
                    lstReturn.Add(new CPluginVariable("5. Debug|Connect", typeof(enumBoolYesNo), DoConnect));                    
                }
            }

            return lstReturn;
        }

        public List<CPluginVariable> GetPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();

            lstReturn.Add(new CPluginVariable("Servertype", "enum.Servertype(AUTOMATIC|BF3|BF4)", this.Servertype));            
            lstReturn.Add(new CPluginVariable("TempBan/Ban by name", typeof(enumBoolYesNo), banbyname));
            lstReturn.Add(new CPluginVariable("TempBan/Ban by EA GUID", typeof(enumBoolYesNo), banbyeaguid));
            lstReturn.Add(new CPluginVariable("TempBan/Ban by PB GUID", typeof(enumBoolYesNo), banbypbguid));
            lstReturn.Add(new CPluginVariable("Whitelist", typeof(string[]), whitelist.ToArray()));
            lstReturn.Add(new CPluginVariable("Sync ReservedSlots/ServerVIPs", typeof(enumBoolYesNo), syncreservedslots));
            //lstReturn.Add(new CPluginVariable("Exclude Non-ReservedSlots/ServerVIPs", typeof(enumBoolYesNo), excludereservedslots));
            lstReturn.Add(new CPluginVariable("Use BattlelogCache if available", typeof(enumBoolYesNo), useCache));
            lstReturn.Add(new CPluginVariable("Stream Players to 247FairPlay.com", typeof(enumBoolYesNo), publicintegration));

            lstReturn.Add(new CPluginVariable("Enable Stats Check", typeof(enumBoolYesNo), DoStatsCheck));
            lstReturn.Add(new CPluginVariable("Enable Strict Mode", typeof(enumBoolYesNo), strictmode));

            lstReturn.Add(new CPluginVariable("Enable Live Check", typeof(enumBoolYesNo), DoLiveCheck));
            
            lstReturn.Add(new CPluginVariable("Enable Kick", typeof(enumBoolYesNo), enablekick));
            lstReturn.Add(new CPluginVariable("Message to be displayed to the kicked player", kicktext.GetType(), kicktext));

            lstReturn.Add(new CPluginVariable("Enable Temp Ban", typeof(enumBoolYesNo), enabletban));
            lstReturn.Add(new CPluginVariable("Message to be displayed to the temp banned player", tbantext.GetType(), tbantext));
            lstReturn.Add(new CPluginVariable("Length of Temp Ban (min)", tbantime.GetType(), tbantime));

            lstReturn.Add(new CPluginVariable("Enable Ban", typeof(enumBoolYesNo), enableban));
            lstReturn.Add(new CPluginVariable("Message to be displayed to the banned player", bantext.GetType(), bantext));

            lstReturn.Add(new CPluginVariable("Enable ingame notification", typeof(enumBoolYesNo), enableingamenotify));
            lstReturn.Add(new CPluginVariable("Enable public ingame notification say", typeof(enumBoolYesNo), enableingamepubnotifysay));
            lstReturn.Add(new CPluginVariable("Message to be displayed (say)", publicnotifysaytext.GetType(), publicnotifysaytext));
            lstReturn.Add(new CPluginVariable("Enable public ingame notification yell", typeof(enumBoolYesNo), enableingamepubnotifyyell));
            lstReturn.Add(new CPluginVariable("Time to display (sec) (yell)", ingamepublicnotifytime.GetType(), ingamepublicnotifytime));
            lstReturn.Add(new CPluginVariable("Message to be displayed (yell)", publicnotifyyelltext.GetType(), publicnotifyyelltext));

            lstReturn.Add(new CPluginVariable("Enable private ingame notification psay", typeof(enumBoolYesNo), enableingameprivpsay));
            lstReturn.Add(new CPluginVariable("Ingame username (psay)", typeof(string[]), ingameaccountnamepsay.ToArray()));
            lstReturn.Add(new CPluginVariable("Message to be displayed (psay)", privatenotifypsaytext.GetType(), privatenotifypsaytext));

            lstReturn.Add(new CPluginVariable("Enable private ingame notification pyell", typeof(enumBoolYesNo), enableingameprivpyell));
            lstReturn.Add(new CPluginVariable("Time to display (sec) (pyell)", ingameprivnotifytime.GetType(), ingameprivnotifytime));
            lstReturn.Add(new CPluginVariable("Ingame username (pyell)", typeof(string[]), ingameaccountnamepyell.ToArray()));
            lstReturn.Add(new CPluginVariable("Message to be displayed (pyell)", privatenotifypyelltext.GetType(), privatenotifypyelltext));

            lstReturn.Add(new CPluginVariable("Debug Level (0-5)", debuglevel.GetType(), debuglevel));
            lstReturn.Add(new CPluginVariable("Automatic Update Check", typeof(enumBoolYesNo), Check4Update));
            lstReturn.Add(new CPluginVariable("Log to file", typeof(enumBoolYesNo), logtofile));
            lstReturn.Add(new CPluginVariable("Filename/Path", logfilename.GetType(), logfilename));
            lstReturn.Add(new CPluginVariable("Advanced", typeof(enumBoolYesNo), advanced));
            lstReturn.Add(new CPluginVariable("Lazy Queue", typeof(enumBoolYesNo), lazyQueue));
            //lstReturn.Add(new CPluginVariable("Yellow Flag Accuracy", yellowacc.GetType(), yellowacc));
            //lstReturn.Add(new CPluginVariable("Orange Flag Accuracy", orangeacc.GetType(), orangeacc));
            //lstReturn.Add(new CPluginVariable("Red Flag Accuracy", redacc.GetType(), redacc));
            //lstReturn.Add(new CPluginVariable("Yellow Flag Headshots per Kill", yellowhspk.GetType(), yellowhspk));
            //lstReturn.Add(new CPluginVariable("Orange Flag Headshots per Kill", orangehspk.GetType(), orangehspk));
            //lstReturn.Add(new CPluginVariable("Red Flag Headshots per Kill", redhspk.GetType(), redhspk));
            //lstReturn.Add(new CPluginVariable("Yellow Flag Kills per Hit", yellowkph.GetType(), yellowkph));
            //lstReturn.Add(new CPluginVariable("Orange Flag Kills per Hit", orangekph.GetType(), orangekph));
            //lstReturn.Add(new CPluginVariable("Red Flag Kills per Hit", redkph.GetType(), redkph));
            //lstReturn.Add(new CPluginVariable("Orange Flag Kills per Min", orangekpm.GetType(), orangekpm));
            //lstReturn.Add(new CPluginVariable("Red Flag Kills per Min", redkpm.GetType(), redkpm));
            lstReturn.Add(new CPluginVariable("Orange Flag Score per Min (update1)", orangespm.GetType(), orangespm));
            lstReturn.Add(new CPluginVariable("Red Flag Score per Min (update1)", redspm.GetType(), redspm));
            lstReturn.Add(new CPluginVariable("Min Kills (update2)", minimumkills.GetType(), minimumkills));
            lstReturn.Add(new CPluginVariable("Min Kills Live (update1)", minkillsl.GetType(), minkillsl));
            lstReturn.Add(new CPluginVariable("Max hsklive", maxhsklive.GetType(), maxhsklive));
            lstReturn.Add(new CPluginVariable("Max kmlive", maxkmlive.GetType(), maxkmlive));
            lstReturn.Add(new CPluginVariable("Name", typeof(string[]), tempplayertocheck.ToArray()));
            lstReturn.Add(new CPluginVariable("Test", typeof(enumBoolYesNo), test));
            lstReturn.Add(new CPluginVariable("Retries", retries.GetType(), retries));
            lstReturn.Add(new CPluginVariable("Timeout Test", typeof(enumBoolYesNo), timeouttest));
            lstReturn.Add(new CPluginVariable("Stream to Master", typeof(enumBoolYesNo), streamtomaster));

            if (streamtomaster == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("Server", queueServer.GetType(), queueServer));
                lstReturn.Add(new CPluginVariable("DB", queueDB.GetType(), queueDB));
                lstReturn.Add(new CPluginVariable("User", queueUser.GetType(), queueUser));
                lstReturn.Add(new CPluginVariable("Pass", queuePwd.GetType(), queuePwd));
                lstReturn.Add(new CPluginVariable("Connect", typeof(enumBoolYesNo), DoConnect));                
            }

            return lstReturn;
        }
        #endregion

        #region settings write
        public void SetPluginVariable(string strVariable, string strValue)
        {
            int outvalue;
            //uint uoutvalue;
            double outvaluedouble;

            if (strVariable.CompareTo("Automatic Update Check") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                Check4Update = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Servertype") == 0)
            {
                this.Servertype = strValue;
            }
            else if (strVariable.CompareTo("Whitelist") == 0)
            {
                whitelist = new List<string>(strValue.Split(new char[] { '|' }));
            }
            else if (strVariable.CompareTo("Sync ReservedSlots/ServerVIPs") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                syncreservedslots = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                ExecuteCommand("procon.protected.send", "reservedSlotsList.list");
            }
            else if (strVariable.CompareTo("Enable Strict Mode") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                strictmode = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);                
            }
            //else if (strVariable.CompareTo("Exclude Non-ReservedSlots/ServerVIPs") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            //{
            //    excludereservedslots = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            //    ExecuteCommand("procon.protected.send", "reservedSlotsList.list");
            //}
            else if (strVariable.CompareTo("Use BattlelogCache if available") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                useCache = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Stream Players to 247FairPlay.com") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                publicintegration = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable Stats Check") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                DoStatsCheck = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable Live Check") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                DoLiveCheck = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("TempBan/Ban by name") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                banbyname = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (banbyname == enumBoolYesNo.Yes)
                {
                    banbyeaguid = enumBoolYesNo.No;
                    banbypbguid = enumBoolYesNo.No;
                }
            }
            else if (strVariable.CompareTo("TempBan/Ban by EA GUID") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                banbyeaguid = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (banbyeaguid == enumBoolYesNo.Yes)
                {
                    banbyname = enumBoolYesNo.No;
                    banbypbguid = enumBoolYesNo.No;
                }
            }
            else if (strVariable.CompareTo("TempBan/Ban by PB GUID") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                banbypbguid = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (banbypbguid == enumBoolYesNo.Yes)
                {
                    banbyname = enumBoolYesNo.No;
                    banbyeaguid = enumBoolYesNo.No;
                }
            }
            else if (strVariable.CompareTo("Name") == 0)
            {
                tempplayertocheck = new List<string>(strValue.Split(new char[] { '|' }));
            }
            else if (strVariable.CompareTo("Test") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                test = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Timeout Test") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                timeouttest = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable Kick") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                enablekick = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (enablekick == enumBoolYesNo.Yes)
                {
                    enabletban = enumBoolYesNo.No;
                    enableban = enumBoolYesNo.No;
                }
            }
            else if (strVariable.CompareTo("Enable Temp Ban") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                enabletban = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (enabletban == enumBoolYesNo.Yes)
                {
                    enablekick = enumBoolYesNo.No;
                    enableban = enumBoolYesNo.No;
                }
            }
            else if (strVariable.CompareTo("Enable Ban") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                enableban = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (enableban == enumBoolYesNo.Yes)
                {
                    enabletban = enumBoolYesNo.No;
                    enablekick = enumBoolYesNo.No;
                }
            }
            else if (strVariable.CompareTo("Length of Temp Ban (min)") == 0 && int.TryParse(strValue, out outvalue))
            {
                tbantime = outvalue;

                if (tbantime < 0) //not less than 0
                {
                    tbantime = 0;
                }
                if (tbantime > 53568000)
                {
                    tbantime = 53568000;
                }
            }
            else if (strVariable.CompareTo("Message to be displayed to the kicked player") == 0)
            {
                kicktext = strValue;
            }
            else if (strVariable.CompareTo("Message to be displayed to the temp banned player") == 0)
            {
                tbantext = strValue;
            }
            else if (strVariable.CompareTo("Message to be displayed to the banned player") == 0)
            {
                bantext = strValue;
            }
            else if (strVariable.CompareTo("Message to be displayed (say)") == 0)
            {
                publicnotifysaytext = strValue;
            }
            else if (strVariable.CompareTo("Message to be displayed (yell)") == 0)
            {
                publicnotifyyelltext = strValue;
            }
            else if (strVariable.CompareTo("Message to be displayed (psay)") == 0)
            {
                privatenotifypsaytext = strValue;
            }
            else if (strVariable.CompareTo("Message to be displayed (pyell)") == 0)
            {
                privatenotifypyelltext = strValue;
            }
            else if (strVariable.CompareTo("Enable ingame notification") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                enableingamenotify = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable public ingame notification say") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                enableingamepubnotifysay = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable public ingame notification yell") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                enableingamepubnotifyyell = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable private ingame notification psay") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                enableingameprivpsay = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Enable private ingame notification pyell") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                enableingameprivpyell = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Ingame username (psay)") == 0)
            {
                ingameaccountnamepsay = new List<string>(strValue.Split(new char[] { '|' }));
            }
            else if (strVariable.CompareTo("Ingame username (pyell)") == 0)
            {
                ingameaccountnamepyell = new List<string>(strValue.Split(new char[] { '|' }));
            }
            else if (strVariable.CompareTo("Time to display (sec) (pyell)") == 0 && int.TryParse(strValue, out outvalue))
            {
                if (outvalue < 1)
                {
                    outvalue = 1;
                }
                if (outvalue > 120) //120 secs is enough
                {
                    outvalue = 120;
                }
                ingameprivnotifytime = outvalue;
            }
            else if (strVariable.CompareTo("Time to display (sec) (yell)") == 0 && int.TryParse(strValue, out outvalue))
            {
                if (outvalue < 1)
                {
                    outvalue = 1;
                }
                if (outvalue > 120) //120 secs is enough
                {
                    outvalue = 120;
                }
                ingamepublicnotifytime = outvalue;
            }
            else if (strVariable.CompareTo("Debug Level (0-5)") == 0 && double.TryParse(strValue, out outvaluedouble))
            {
                debuglevel = outvaluedouble;
                if (debuglevel < 0)
                {
                    debuglevel = 0;
                }
                if (debuglevel > 5)
                {
                    debuglevel = 5;
                }
            }
            else if (strVariable.CompareTo("Advanced") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                advanced = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Lazy Queue") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                lazyQueue = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            //else if (strVariable.CompareTo("Yellow Flag Accuracy") == 0 && double.TryParse(strValue, out outvaluedouble))
            //{
            //    yellowacc = outvaluedouble;
            //}
            //else if (strVariable.CompareTo("Orange Flag Accuracy") == 0 && double.TryParse(strValue, out outvaluedouble))
            //{
            //    orangeacc = outvaluedouble;
            //}
            //else if (strVariable.CompareTo("Red Flag Accuracy") == 0 && double.TryParse(strValue, out outvaluedouble))
            //{
            //    redacc = outvaluedouble;
            //}
            //else if (strVariable.CompareTo("Yellow Flag Headshots per Kill") == 0 && double.TryParse(strValue, out outvaluedouble))
            //{
            //    yellowhspk = outvaluedouble;
            //}
            //else if (strVariable.CompareTo("Orange Flag Headshots per Kill") == 0 && double.TryParse(strValue, out outvaluedouble))
            //{
            //    orangehspk = outvaluedouble;
            //}
            //else if (strVariable.CompareTo("Red Flag Headshots per Kill") == 0 && double.TryParse(strValue, out outvaluedouble))
            //{
            //    redhspk = outvaluedouble;
            //}
            //else if (strVariable.CompareTo("Yellow Flag Kills per Hit") == 0 && double.TryParse(strValue, out outvaluedouble))
            //{
            //    yellowkph = outvaluedouble;
            //}
            //else if (strVariable.CompareTo("Orange Flag Kills per Hit") == 0 && double.TryParse(strValue, out outvaluedouble))
            //{
            //    orangekph = outvaluedouble;
            //}
            //else if (strVariable.CompareTo("Red Flag Kills per Hit") == 0 && double.TryParse(strValue, out outvaluedouble))
            //{
            //    redkph = outvaluedouble;
            //}
            //else if (strVariable.CompareTo("Orange Flag Kills per Min") == 0 && double.TryParse(strValue, out outvaluedouble))
            //{
            //    orangekpm = outvaluedouble;
            //}
            //else if (strVariable.CompareTo("Red Flag Kills per Min") == 0 && double.TryParse(strValue, out outvaluedouble))
            //{
            //    redkpm = outvaluedouble;
            //}
            else if (strVariable.CompareTo("Orange Flag Score per Min (update1)") == 0 && double.TryParse(strValue, out outvaluedouble))
            {
                orangespm = outvaluedouble;
            }
            else if (strVariable.CompareTo("Red Flag Score per Min (update1)") == 0 && double.TryParse(strValue, out outvaluedouble))
            {
                redspm = outvaluedouble;
            }
            else if (strVariable.CompareTo("Min Kills (update2)") == 0 && int.TryParse(strValue, out outvalue))
            {
                minimumkills = outvalue;
            }
            else if (strVariable.CompareTo("Min Kills Live (update1)") == 0 && double.TryParse(strValue, out outvaluedouble))
            {
                minkillsl = outvaluedouble;
            }
            else if (strVariable.CompareTo("Max hsklive") == 0 && double.TryParse(strValue, out outvaluedouble))
            {
                maxhsklive = outvaluedouble;
            }
            else if (strVariable.CompareTo("Max kmlive") == 0 && double.TryParse(strValue, out outvaluedouble))
            {
                maxkmlive = outvaluedouble;
            }
            else if (strVariable.CompareTo("Retries") == 0 && int.TryParse(strValue, out outvalue))
            {
                retries = outvalue;
            }
            else if (strVariable.CompareTo("Log to file") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                logtofile = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Filename/Path") == 0)
            {
                logfilename = strValue;
            }
            else if (strVariable.CompareTo("Stream to Master") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                streamtomaster = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Server") == 0)
            {
                queueServer = strValue;
            }
            else if (strVariable.CompareTo("DB") == 0)
            {
                queueDB = strValue;
            }
            else if (strVariable.CompareTo("User") == 0)
            {
                queueUser = strValue;
            }
            else if (strVariable.CompareTo("Pass") == 0)
            {
                queuePwd = strValue;
            }
            else if (strVariable.CompareTo("Connect") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                DoConnect = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
                if (DoConnect == enumBoolYesNo.Yes)
                {
                    DB = new DBConnect(this);
                }
                //else if (DoConnect == enumBoolYesNo.No)
                //{
                //    DB = null;
                //}
            }
        }
        #endregion

        public virtual void OnPlayerKilled(Kill kKillerVictimDetails)
        {
            if (DoLiveCheck == enumBoolYesNo.Yes)
            {
                lock (killlock)
                {
                    if (kKillerVictimDetails.Killer.SoldierName != kKillerVictimDetails.Victim.SoldierName && !kKillerVictimDetails.IsSuicide)
                    {
                        if (!kills.ContainsKey(kKillerVictimDetails.Killer.SoldierName))
                        {
                            kills.Add(kKillerVictimDetails.Killer.SoldierName, 0);
                        }
                        kills[kKillerVictimDetails.Killer.SoldierName]++;

                        if (!hs.ContainsKey(kKillerVictimDetails.Killer.SoldierName))
                        {
                            hs.Add(kKillerVictimDetails.Killer.SoldierName, 0);
                        }

                        if (kKillerVictimDetails.Headshot)
                        {
                            hs[kKillerVictimDetails.Killer.SoldierName]++;
                        }

                        if (!firstkill.ContainsKey(kKillerVictimDetails.Killer.SoldierName))
                        {
                            firstkill.Add(kKillerVictimDetails.Killer.SoldierName, DateTime.Now);
                        }
                    }
                }
            }
        }

        public void OnServerInfo(CServerInfo csiServerInfo)
        {
            servername = csiServerInfo.ServerName;
            serveripandport = csiServerInfo.ExternalGameIpandPort;

            if (Servertype == "AUTOMATIC")
            {
                if (string.IsNullOrEmpty(csiServerInfo.BlazeGameState))
                {
                    log("BF3 detected", 1);
                    Servertype = "BF3";
                }
                else
                {
                    log("BF4 detected", 1);
                    Servertype = "BF4";
                }
            }

            UpdateCheck();
        }

        private void UpdateCheck()
        {
            if (Check4Update == enumBoolYesNo.Yes)
            {
                try
                {
                    DateTime updatehelper = lastupdatecheck.AddHours(3);
                    if (DateTime.Compare(updatehelper, DateTime.Now) <= 0)
                    {
                        WebClient wc = new WebClient();
                        string latestversion = wc.DownloadString("https://forum.myrcon.com/showthread.php?5414");

                        latestversion = latestversion.Substring(latestversion.IndexOf("<title>") + 7);
                        latestversion = latestversion.Substring(0, latestversion.IndexOf("</title>"));
                        latestversion = latestversion.Substring(latestversion.IndexOf("CheatDetector") + 14);

                        if (GetPluginVersion() != latestversion)
                        {
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "CheatDetector: ^b^2UPDATE " + latestversion + " AVAILABLE");
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "CheatDetector: your version: " + GetPluginVersion());
                            this.ExecuteCommand("procon.protected.pluginconsole.write", "CheatDetector: latest version " + latestversion);
                        }
                        lastupdatecheck = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "CD - ERROR checking for Update: " + ex);
                }
            }
        }

        private void livecheck(List<CPlayerInfo> lstPlayers)
        {
            DateTime now = DateTime.Now;

            lock (killlock)
            {
                if (lstPlayers.Count > 0 || debuglevel >= 4)
                {
                    log("CD - LiveCheck -------------------------------", 2);
                }

                try
                {
                    foreach (CPlayerInfo player in lstPlayers)
                    {
                        if (kills.ContainsKey(player.SoldierName) && hs.ContainsKey(player.SoldierName) && firstkill.ContainsKey(player.SoldierName) && !whitelist.Contains(player.SoldierName))
                        {
                            TimeSpan time = now - firstkill[player.SoldierName];

                            log("CD - LiveCheck " + player.SoldierName + " kills: " + kills[player.SoldierName] + " hs: " + hs[player.SoldierName] + " time: " + Math.Round(time.TotalMinutes, 2) + " hs/k: " + Math.Round((hs[player.SoldierName] / kills[player.SoldierName]) * 100, 2) + "% kpm: " + Math.Round(kills[player.SoldierName] / time.TotalMinutes, 2), 2);

                            if (kills[player.SoldierName] > minkillsl)
                            {
                                if (hs[player.SoldierName] / kills[player.SoldierName] > maxhsklive || hs[player.SoldierName] > kills[player.SoldierName] || kills[player.SoldierName] / time.TotalMinutes > maxkmlive)
                                {
                                    log("CD - LiveCheck SUSPECTED CHEATING " + player.SoldierName + " kills: " + kills[player.SoldierName] + " hs: " + hs[player.SoldierName] + " time: " + Math.Round(time.TotalMinutes, 2) + " hs/k: " + Math.Round((hs[player.SoldierName] / kills[player.SoldierName]) * 100, 2) + "% kpm: " + Math.Round(kills[player.SoldierName] / time.TotalMinutes, 2), 1);
                                    punish(player.SoldierName, "too many hs/k or k/min (CD - LiveCheck)");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log("CD - ERROR: CD.livecheck: " + ex, 1);
                }
            }
        }

        #region checkbatch
        public void checkbatch()
        {
            if (queue.Count > 0)
            {
                log("CD - " + queue.Count.ToString() + " players in check queue", 3);
                for (int i = 0; i < batchsize; i++)
                {
                    log("CD - checking: " + queue[0], 3);
                    if (IsCacheEnabled(false))
                    {
                        checkPlayerCached(queue[0]);
                        log("CD - checkPlayerCached returned", 5);
                    }
                    else
                    {
                        checkPlayer(queue[0]);
                    }
                    queue.RemoveAt(0);

                    if (queue.Count < 1)
                    {
                        break;
                    }
                }
            }
            else
                log("CD - nothing to do", 4);
        }
        #endregion

        #region playerleft
        public virtual void OnPlayerLeft(CPlayerInfo playerInfo)
        {
            if (myplayerlist.Contains(playerInfo.SoldierName))
            {
                log("CD - removing from playerlist (PlayerLeft): " + playerInfo.SoldierName, 4);
                myplayerlist.Remove(playerInfo.SoldierName);
            }
            if (lazyQueue != enumBoolYesNo.Yes)
            {
                if (queue.Contains(playerInfo.SoldierName))
                {
                    log("CD - removing from queue (PlayerLeft): " + playerInfo.SoldierName, 4);
                    queue.Remove(playerInfo.SoldierName);
                }
            }
        }
        #endregion

        #region listplayers
        public virtual void OnListPlayers(List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset)
        {
            try
            {
                DateTime viphelper = viplastcheck.AddSeconds(59.0);
                if (DateTime.Compare(viphelper, DateTime.Now) <= 0)
                {
                    this.ExecuteCommand("procon.protected.send", "reservedSlotsList.list");
                    
                    viplastcheck = DateTime.Now;
                }
                DateTime timehelper = lastcheck.AddSeconds(batchtimeout);
                if (DateTime.Compare(timehelper, DateTime.Now) <= 0 && DoStatsCheck == enumBoolYesNo.Yes)
                {
                    log("CD - batch check trigger", 4);

                    checkbatch();

                    lastcheck = DateTime.Now;

                    if (batchtimeoutnormal < batchtimeoutbase)
                    {
                        batchtimeoutnormal = batchtimeoutbase;
                    }

                    if (batchtimeout > batchtimeoutnormal)
                    {
                        batchtimeout = batchtimeoutnormal;
                        log("CD - queue timeout: " + batchtimeout.ToString() + " seconds", 3);
                    }
                }

                DateTime timehelper2 = lastdbsend.AddSeconds(1800);
                if (DateTime.Compare(timehelper2, DateTime.Now) <= 0/* && (DateTime.UtcNow.AddHours(1).TimeOfDay < new TimeSpan(1, 0, 0) || DateTime.UtcNow.AddHours(1).TimeOfDay > new TimeSpan(6, 0, 0))*/)
                {
                    log("CD - batch db send trigger", 4);

                    SendNewPlayersToDB();
                    SendNewPlayersToDBPublic();
                    lastdbsend = DateTime.Now;
                }

                foreach (CPlayerInfo cpiPlayer in lstPlayers)
                {
                    if (m_dicPlayerInfo.ContainsKey(cpiPlayer.SoldierName))
                    {
                        log("CD - maintaining playerdict: " + cpiPlayer.SoldierName, 5);
                        m_dicPlayerInfo[cpiPlayer.SoldierName] = cpiPlayer;
                    }
                    else
                    {
                        log("CD - adding to playerdict: " + cpiPlayer.SoldierName, 4);
                        m_dicPlayerInfo.Add(cpiPlayer.SoldierName, cpiPlayer);
                    }
                }

                //housekeeping the lists: players that exist but not in our list (equal and better than onplayerjoin, because it protects against spamming the join button)
                foreach (CPlayerInfo cpiPlayer in lstPlayers)
                {
                    bool boolplayerexists = false;
                    foreach (String player in myplayerlist)
                    {
                        if (cpiPlayer.SoldierName == player)
                            boolplayerexists = true;
                    }
                    if (!boolplayerexists)
                    {
                        if (whitelist.Contains(cpiPlayer.SoldierName))
                        {
                            log("CD - skipping whitelisted player: " + cpiPlayer.SoldierName, 2);
                            myplayerlist.Add(cpiPlayer.SoldierName);
                        }
                        else
                        {
                            log("CD - adding to playerlist and queue (PlayerJoined): " + cpiPlayer.SoldierName, 4);
                            if (!queue.Contains(cpiPlayer.SoldierName) && !whitelist.Contains(cpiPlayer.SoldierName) && DoStatsCheck == enumBoolYesNo.Yes)
                            {
                                queue.Add(cpiPlayer.SoldierName);
                            }

                            myplayerlist.Add(cpiPlayer.SoldierName);                                                        
                        }

                        if (streamtomaster == enumBoolYesNo.Yes || publicintegration == enumBoolYesNo.Yes)
                        {
                            string pbguid = "";
                            if (m_dicPBPlayerInfo.ContainsKey(cpiPlayer.SoldierName))
                            {
                                pbguid = m_dicPBPlayerInfo[cpiPlayer.SoldierName].GUID;
                            }

                            if (!queuefordb.Contains(new PlayerCheckInfo(cpiPlayer.SoldierName, cpiPlayer.GUID, pbguid, 1))/* && !whitelist.Contains(cpiPlayer.SoldierName)*/)
                            {
                                queuefordb.Add(new PlayerCheckInfo(cpiPlayer.SoldierName, cpiPlayer.GUID, pbguid, 1));
                            }
                        }
                    }
                }
                if (test == enumBoolYesNo.Yes)
                {
                    foreach (string player in tempplayertocheck)
                    {
                        if (!queue.Contains(player))
                        {
                            log("CD - adding to queue: " + player, 1);
                            queue.Insert(0, player);
                        }

                        log("CD - adding to queuefordb: " + player, 1);

                        string eaguid = "";
                        string pbguid = "";
                        if (m_dicPlayerInfo.ContainsKey(player))
                        {
                            eaguid = m_dicPlayerInfo[player].GUID;
                        }
                        if (m_dicPBPlayerInfo.ContainsKey(player))
                        {
                            pbguid = m_dicPBPlayerInfo[player].GUID;
                        }
                        queuefordb.Insert(0, new PlayerCheckInfo(player, eaguid, pbguid, 0));
                    }
                    test = enumBoolYesNo.No;
                }

                if (SyncResetPlayerLists)
                {
                    SyncResetPlayerLists = false;

                    log("CD - resetting/syncing playerdicts", 4);

                    kills = new Dictionary<string, double>();
                    hs = new Dictionary<string, double>();
                    firstkill = new Dictionary<string, DateTime>();

                    m_dicPlayerInfo = new Dictionary<string, CPlayerInfo>();
                    m_dicPBPlayerInfo = new Dictionary<string, CPunkbusterInfo>();
                    CacheDict = new Dictionary<string, Dictionary<string, string>>();
                    playerErrors = new Dictionary<string, int>();
                    myplayerlist = new List<string>();

                    foreach (CPlayerInfo player in lstPlayers)
                    {
                        m_dicPlayerInfo.Add(player.SoldierName, player);
                        myplayerlist.Add(player.SoldierName);
                    }
                }

                if (DoLiveCheck == enumBoolYesNo.Yes)
                {
                    DateTime timehelper3 = lastcheck2.AddSeconds(300);
                    if (DateTime.Compare(timehelper3, DateTime.Now) <= 0 && DoLiveCheck == enumBoolYesNo.Yes)
                    {
                        lastcheck2 = DateTime.Now;
                        livecheck(lstPlayers);
                    }
                }
            }
            catch (Exception ex)
            {
                log("CD - ERROR CD.OnListPlayers: " + ex, 1);
            }
        }

        private void SendNewPlayersToDB()
        {
            if (streamtomaster == enumBoolYesNo.Yes && DoConnect == enumBoolYesNo.Yes && DB != null)
            {
                DB.Insert(queuefordb.ToArray(), servername, serveripandport, Servertype);
                queuefordb = new List<PlayerCheckInfo>();
            }
        }
        #endregion

        private void SendNewPlayersToDBPublic()
        {
            if (publicintegration == enumBoolYesNo.Yes && queuefordb.Count > 0)
            {
                try
                {
                    string query = "";

                    int i = 0;
                    foreach (PlayerCheckInfo pci in queuefordb)
                    {
                        query += pci.name;
                        i++;

                        if (i != queuefordb.Count)
                        {
                            query += ";";
                        }
                    }

                    string url = "http://www.247fairplay.com/cheatdetector.php?add=" + Uri.EscapeDataString(query) + "&servername=" + Uri.EscapeDataString(servername) + "&serveripandport=" + Uri.EscapeDataString(serveripandport) + "&servertype=" + Uri.EscapeDataString(Servertype);

                    log(url, 3);

                    webclient.DownloadStringAsync(new Uri(url));

                    queuefordb = new List<PlayerCheckInfo>();
                }
                catch (Exception ex)
                {
                    log("CD - ERROR CD.SendNewPlayersToDBPublic(): " + ex, 1);                    
                }
            }
        }

        #region getresult
        public void getResult(object sender, DoWorkEventArgs e)
        {
            string playername = e.Argument.ToString();
            try
            {
                /* First fetch the player's main page to get the persona id */
                string[] result = new string[3];
                result[1] = playername;
                BattlelogClient bclient = new BattlelogClient(this);

                if (Servertype == "BF3")
                {
                    bclient.fetchWebPage(ref result[0], "http://battlelog.battlefield.com/bf3/user/" + playername);
                }
                else
                {
                    bclient.fetchWebPage(ref result[0], "http://battlelog.battlefield.com/bf4/user/" + playername);
                }

                /* Extract the persona id */
                MatchCollection pid = Regex.Matches(result[0], @"/soldier/" + playername + @"/stats/(\d+)(/\w*)?/", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                String personaId = "";

                foreach (Match m in pid)
                {
                    if (m.Success && m.Groups[2].Value.Trim() != "/ps3" && m.Groups[2].Value.Trim() != "/xbox" && m.Groups[2].Value.Trim() != "/xbox360" && m.Groups[2].Value.Trim() != "/xboxone" && m.Groups[2].Value.Trim() != "/ps4")
                    {
                        personaId = m.Groups[1].Value.Trim();
                    }
                }

                if (personaId == "")
                {
                    throw new Exception("could not find persona-id for " + playername);
                }

                if (Servertype == "BF3")
                {
                    bclient.fetchWebPage(ref result[0], "http://battlelog.battlefield.com/bf3/weaponsPopulateStats/" + personaId + "/1/");
                    bclient.fetchWebPage(ref result[2], "http://battlelog.battlefield.com/bf3/overviewPopulateStats/" + personaId + "/bf3-us-engineer/1/");
                }
                else
                {
                    bclient.fetchWebPage(ref result[0], "http://battlelog.battlefield.com/bf4/warsawWeaponsPopulateStats/" + personaId + "/1/stats");
                    bclient.fetchWebPage(ref result[2], "http://battlelog.battlefield.com/bf4/warsawoverviewpopulate/" + personaId + "/1/");
                }
                e.Result = result;
            }
            catch (WebException ex)
            {
                log("CD - WebException getResult: " + ex.Message, 3);
                log("CD - Battlelog Request Limit reached.", 2);
                //batchtimeout += batchtimeoutsleep;
                if (batchtimeoutnormal < batchtimeoutmax)
                {
                    log("CD - Increasing queue timeout by " + batchtimeoutmodinc.ToString() + " seconds.", 2);
                    batchtimeoutnormal += batchtimeoutmodinc;
                }
                batchtimeout = batchtimeoutnormal;

                backAdd(playername);
            }
            catch (Exception ex)
            {
                log("CD - Exception getResult: " + ex.Message, 3);
            }
        }
        #endregion

        #region analyzeresult
        public void analyzeResult(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                string[] args = e.Result as string[];
                string playername = args[1];
                string result = args[0];
                string resultforspm = args[2];
                universalAnalyzer(playername, resultforspm, result);
            }
            catch (StatsException ex)
            {
                log("CD - StatsException analyzeResult: " + ex.Message, 4);
            }
            catch (Exception ex)
            {
                log("CD - Exception analyzeResult: " + ex.Message, 4);
            }
        }
        #endregion

        #region checkplayer
        public void checkPlayer(String playername)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += getResult;
            worker.RunWorkerCompleted += analyzeResult;
            worker.RunWorkerAsync(playername);
        }
        #endregion

        #region checkplayercached
        private void checkPlayerCached(string player)
        {
            try
            {
                log("CD - function checkPlayerCached entered", 5);
                lock (CacheDict)
                {
                    log("CD - lock entered", 5);
                    if (!CacheDict.ContainsKey(player))
                    {
                        CacheDict.Add(player, new Dictionary<string, string>());
                        CacheDict[player].Add("overview", null);
                        CacheDict[player].Add("weapon", null);
                        //CacheDict[player] = null;
                        //CacheDict[player]["overview"] = null;
                        //CacheDict[player]["weapon"] = null;
                    }
                }

                log("CD - sending Cache Requests for: " + player, 4);

                SendCacheRequest(player, "overview");
                SendCacheRequest(player, "weapon");
            }
            catch (Exception ex)
            {
                log("CD - Exception in checkPlayerCached: " + ex.Message, 3);
            }
        }
        #endregion

        #region CacheAnalyzeResult
        public void CacheAnalyzeResult(params String[] response)
        {
            string playername = response[0];
            try
            {
                string val = "";
                log("CD - CacheAnalyzeResult called with " + response.Length + " parameters", 5);
                if (debuglevel >= 5)
                { // This is just for debugging, dumps all the result parameters
                    for (int i = 0; i < response.Length; ++i)
                    {
                        log("#" + i + ") Length: " + response[i].Length, 5);
                        val = response[i];
                        if (val.Length > 100) val = val.Substring(0, 100) + " ... ";
                        {
                            if (val.Contains("{")) val = val.Replace('{', '<').Replace('}', '>'); // ConsoleWrite doesn't like messages with "{" in it
                            {
                                log("#" + i + ") Value: " + val, 5);
                            }
                        }
                    }
                }

                //String magicplayerName = response[0]; // Player's name
                //string[] temp = response[0].Split(magic.ToCharArray());
                //string playername = response[0];
                //string responsetype = response[1];
                string jsonstring = response[1]; // JSON string

                // ... now you can call analyzeResult or something similar to it

                lock (CacheDict)
                {
                    if (CacheDict[playername] != null)
                    {
                        Hashtable json = (Hashtable)JSON.JsonDecode(jsonstring);
                        Hashtable statsforspm = null;

                        // check we got a valid response
                        if (!(json.ContainsKey("type") && json.ContainsKey("message")))
                        {
                            throw new Exception("JSON response does not contain \"type\" or \"message\" fields");
                        }

                        String type = (String)json["type"];
                        String message = (String)json["message"];

                        /* verify we got a success message */
                        if (!(type.StartsWith("success") && message.StartsWith("OK")))
                        {
                            throw new Exception("JSON response was type=" + type + ", message=" + message);
                        }

                        /* verify there is data structure */
                        Hashtable data = null;
                        if (!json.ContainsKey("data") || (data = (Hashtable)json["data"]) == null)
                        {
                            throw new Exception("JSON response was does not contain a data field");
                        }
                        //plugin.ExecuteCommand("procon.protected.pluginconsole.write", "1");
                        ArrayList weaponsstats = null;
                        if (!data.ContainsKey("mainWeaponStats") || (weaponsstats = (ArrayList)data["mainWeaponStats"]) == null)
                        {
                            CacheDict[playername]["overview"] = jsonstring;
                        }
                        else if (!data.ContainsKey("overviewStats") || (statsforspm = (Hashtable)data["overviewStats"]) == null)
                        {
                            CacheDict[playername]["weapon"] = jsonstring;
                        }
                        else
                        {
                            throw new StatsException("ERROR: JSON response data does not contain mainWeaponStats or overviewStats");
                        }
                    }
                }
                if (CacheDict[playername]["overview"] != null && CacheDict[playername]["weapon"] != null)
                {
                    universalAnalyzer(playername, CacheDict[playername]["overview"], CacheDict[playername]["weapon"]);
                    CacheDict.Remove(playername);
                }
            }
            catch (Exception ex)
            {
                log("CD - Exception in CacheAnalyzeResult: " + ex.Message, 3);
                backAdd(playername);
            }
        }
        #endregion

        #region universalAnalyzer
        private void universalAnalyzer(string playername, string overviewStats, string weaponStats)
        {
            try
            {
                double cheatindex = 0.0;
                string bestreason = "";
                Dictionary<double, string> reasons = new Dictionary<double, string>();

                Hashtable json = (Hashtable)JSON.JsonDecode(weaponStats);

                // check we got a valid response
                if (!(json.ContainsKey("type") && json.ContainsKey("message")))
                {
                    throw new Exception("JSON response does not contain \"type\" or \"message\" fields");
                }

                String type = (String)json["type"];
                String message = (String)json["message"];

                /* verify we got a success message */
                if (!(type.StartsWith("success") && message.StartsWith("OK")))
                {
                    throw new Exception("JSON response was type=" + type + ", message=" + message);
                }

                /* verify there is data structure */
                Hashtable data = null;
                if (!json.ContainsKey("data") || (data = (Hashtable)json["data"]) == null)
                {
                    throw new Exception("JSON response was does not contain a data field");
                }
                //plugin.ExecuteCommand("procon.protected.pluginconsole.write", "1");

                ArrayList weaponsstats = null;
                if (!data.ContainsKey("mainWeaponStats") || (weaponsstats = (ArrayList)data["mainWeaponStats"]) == null)
                {
                    throw new StatsException("ERROR: JSON response data does not contain mainWeaponStats");
                }

                for (int i = 0; i < weaponsstats.Count; i++)
                {
                    Hashtable weapon = null;
                    if ((weapon = (Hashtable)weaponsstats[i]) == null)
                    {
                        throw new StatsException("ERROR: JSON response mainWeaponStats does not contain " + i);
                    }

                    // get the data fields
                    string weaponname = FixWeaponName(weapon["name"].ToString());
                    double kills = Convert.ToDouble(weapon["kills"].ToString());

                    double shots;
                    if (weapon["shotsFired"] != null)
                    {
                        shots = Convert.ToDouble(weapon["shotsFired"].ToString());
                    }
                    else
                    {
                        shots = 0.0;
                    }

                    double hits;
                    if (weapon["shotsHit"] != null)
                    {
                        hits = Convert.ToDouble(weapon["shotsHit"].ToString());
                    }
                    else
                    {
                        hits = 0.0;
                    }

                    double headshots;
                    if (weapon["headshots"] != null)
                    {
                        headshots = Convert.ToDouble(weapon["headshots"].ToString());
                    }
                    else
                    {
                        headshots = 0.0;
                    }

                    double acc;
                    if (weapon["accuracy"] != null)
                    {
                        acc = Convert.ToDouble(weapon["accuracy"].ToString());
                    }
                    else
                    {
                        acc = 0.0;
                    }

                    double time;
                    if (weapon["timeEquipped"] != null)
                    {
                        time = Convert.ToDouble(weapon["timeEquipped"].ToString());
                    }
                    else
                    {
                        time = 0.0;
                    }

                    //if (kills >= minkills)
                    //{
                        log("CD - checking weapon: " + weaponname, 5);
                        cheatindex += checkAccuracy(ref reasons, Servertype.ToLower(), playername, weaponname, acc, kills);
                        cheatindex += checkHeadshotsperKill(ref reasons, Servertype.ToLower(), playername, weaponname, headshots, kills);
                        cheatindex += checkKillsperHit(ref reasons, Servertype.ToLower(), playername, weaponname, kills, hits);
                        cheatindex += checkKillsperMin(ref reasons, Servertype.ToLower(), playername, weaponname, kills, time);
                    //}
                    //else
                    //{
                    //    log("CD - skipping weapon (less than " + minkills.ToString() + " kills): " + weaponname, 5);
                    //}
                }

                Hashtable jsonforspm = (Hashtable)JSON.JsonDecode(overviewStats);

                // check we got a valid response
                if (!(jsonforspm.ContainsKey("type") && jsonforspm.ContainsKey("message")))
                {
                    throw new Exception("JSON response does not contain \"type\" or \"message\" fields");
                }

                String typeforspm = (String)jsonforspm["type"];
                String messageforspm = (String)jsonforspm["message"];

                /* verify we got a success message */
                if (!(typeforspm.StartsWith("success") && messageforspm.StartsWith("OK")))
                {
                    throw new Exception("JSON response was type=" + typeforspm + ", message=" + messageforspm);
                }


                /* verify there is data structure */
                Hashtable dataforspm = null;
                if (!jsonforspm.ContainsKey("data") || (dataforspm = (Hashtable)jsonforspm["data"]) == null)
                {
                    throw new Exception("JSON response was does not contain a data field");
                }

                Hashtable statsforspm = null;
                if (!dataforspm.ContainsKey("overviewStats") || (statsforspm = (Hashtable)dataforspm["overviewStats"]) == null)
                {
                    throw new StatsException("ERROR: JSON response data does not contain overviewStats");
                }

                double score = 0;
                double timePlayed = 0;
                double spm = 0;

                score = Convert.ToDouble(statsforspm["score"].ToString());
                timePlayed = Convert.ToDouble(statsforspm["timePlayed"].ToString());

                if (score != 0 && timePlayed >= 3600)
                {
                    spm = Math.Round(score / (timePlayed / 60), 2);
                }
                else
                {
                    spm = 0;
                }

                log("CD - spm: " + spm.ToString(), 4);

                if (spm > redspm)
                {
                    log("CD - IMPOSSIBLE STATS: " + playername + " spm: " + spm.ToString(), 1);
                    cheatindex += (orangeflagcount - 0.5);
                }
                else if (spm > orangespm)
                {
                    log("CD - Suspicious stats: " + playername + " spm: " + spm.ToString(), 1);
                    cheatindex += (yellowflagcount - 0.5);
                }

                double cheatpercent = cheatindex / GetCILimit();
                if (cheatpercent > 0.99)
                {
                    cheatpercent = 0.99;
                }
                cheatpercent = cheatpercent * 100;

                if (cheatindex < GetCILimit())
                {
                    if (cheatindex > 0.0)
                    {
                        log("CD - " + playername + ": " + cheatpercent.ToString() + "%", 1);
                    }
                    else
                    {
                        log("CD - " + playername + ": " + cheatpercent.ToString() + "%", 2);
                    }
                }
                else
                {
                    log("CD - SUSPECTED CHEATING: " + playername + " " + cheatpercent.ToString() + "%", 1);
                }

                if (cheatindex >= GetCILimit())
                {
                    double bestscore = 0;
                    foreach (KeyValuePair<double, string> kvp in reasons)
                    {
                        if (kvp.Key > bestscore)
                        {
                            bestscore = kvp.Key;
                            bestreason = kvp.Value;
                        }
                    }

                    if (myplayerlist.Contains(playername) && (enableban == enumBoolYesNo.Yes || enabletban == enumBoolYesNo.Yes || enablekick == enumBoolYesNo.Yes))
                    {
                        myplayerlist.Remove(playername);
                    }

                    punish(playername, bestreason);

                    if (enableingamenotify == enumBoolYesNo.Yes) //is ingamenotify enabled?
                    {
                        if (enableingamepubnotifysay == enumBoolYesNo.Yes)
                        {
                            string msg = formatMsg(publicnotifysaytext, playername, bestreason, m_dicPBPlayerInfo[playername].GUID, m_dicPlayerInfo[playername].GUID);                            
                            ExecuteCommand("procon.protected.tasks.add", "CheatDetector", "0", "1", "1", "procon.protected.send", "admin.say", msg, "all"); //untested, kinda double op here
                        }
                        if (enableingamepubnotifyyell == enumBoolYesNo.Yes)
                        {
                            string msg = formatMsg(publicnotifyyelltext, playername, bestreason, m_dicPBPlayerInfo[playername].GUID, m_dicPlayerInfo[playername].GUID);
                            ExecuteCommand("procon.protected.tasks.add", "CheatDetector", "0", "1", "1", "procon.protected.send", "admin.yell", msg, ingamepublicnotifytime.ToString(), "all"); //untested, kinda double op here
                        }
                        if (enableingameprivpyell == enumBoolYesNo.Yes)
                        {
                            foreach (string name in ingameaccountnamepyell)
                            {
                                string msg = formatMsg(privatenotifypsaytext, playername, bestreason, m_dicPBPlayerInfo[playername].GUID, m_dicPlayerInfo[playername].GUID);
                                ExecuteCommand("procon.protected.tasks.add", "CheatDetector", "0", "1", "1", "procon.protected.send", "admin.yell", name + ": " + msg, ingameprivnotifytime.ToString(), "player", name); //works, kinda double op here
                            }
                        }
                        if (enableingameprivpsay == enumBoolYesNo.Yes)
                        {
                            foreach (string name in ingameaccountnamepsay)
                            {
                                string msg = formatMsg(privatenotifypyelltext, playername, bestreason, m_dicPBPlayerInfo[playername].GUID, m_dicPlayerInfo[playername].GUID);
                                ExecuteCommand("procon.protected.tasks.add", "CheatDetector", "0", "1", "1", "procon.protected.send", "admin.say", name + ": " + msg, "player", name); //untested, kinda double op here
                            }
                        }
                    }

                    //log("CD - SUSPECTED CHEATING: " + playername + " " + cheatpercent.ToString() + "%", 1);
                    log("------------------------", 1);
                    //log("CD - Reason: " + bestreason, 1);
                }

                if (batchtimeoutnormal > batchtimeoutbase)
                {
                    log("CD - decreasing next queue timeout by: " + batchtimeoutmoddec.ToString() + " seconds", 4);
                    batchtimeoutnormal -= batchtimeoutmoddec;
                }
            }
            catch (Exception ex)
            {
                log("CD - Exception universalAnalyzer: " + ex.Message, 3);
            }
        }

        private double GetCILimit()
        {
            if (strictmode == enumBoolYesNo.Yes)
            {
                return 7.5;
            }
            else
            {
                return 10.0;
            }
        }

        #endregion

        private void punish(string name, string bestreason)
        {
            if (enableban == enumBoolYesNo.Yes) //is ban enabled?
            {
                string msg = formatMsg(bantext, name, bestreason, m_dicPBPlayerInfo[name].GUID, m_dicPlayerInfo[name].GUID);
                if (banbyname == enumBoolYesNo.Yes || banbyeaguid == enumBoolYesNo.Yes)
                {
                    if (banbyeaguid == enumBoolYesNo.Yes)
                    {
                        ExecuteCommand("procon.protected.send", "banList.add", "guid", m_dicPlayerInfo[name].GUID, "perm", msg);
                    }
                    if (banbyname == enumBoolYesNo.Yes)
                    {
                        ExecuteCommand("procon.protected.send", "banList.add", "name", name, "perm", msg);
                    }
                    ExecuteCommand("procon.protected.send", "banList.save");
                    ExecuteCommand("procon.protected.send", "banList.list");
                }
                if (banbypbguid == enumBoolYesNo.Yes)
                {
                    ExecuteCommand("procon.protected.send", "punkBuster.pb_sv_command", String.Format("pb_sv_ban \"{0}\" \"{1}\"", name, "BC2! " + msg));
                }
            }
            else if (enabletban == enumBoolYesNo.Yes) //is tban enabled?
            {
                string msg = formatMsg(tbantext, name, bestreason, m_dicPBPlayerInfo[name].GUID, m_dicPlayerInfo[name].GUID);
                if (banbyname == enumBoolYesNo.Yes || banbyeaguid == enumBoolYesNo.Yes)
                {
                    if (banbyeaguid == enumBoolYesNo.Yes)
                    {
                        ExecuteCommand("procon.protected.send", "banList.add", "guid", m_dicPlayerInfo[name].GUID, "seconds", (tbantime * 60).ToString(), msg);
                    }
                    if (banbyname == enumBoolYesNo.Yes)
                    {
                        ExecuteCommand("procon.protected.send", "banList.add", "name", name, "seconds", (tbantime * 60).ToString(), msg);
                    }
                    ExecuteCommand("procon.protected.send", "banList.save");
                    ExecuteCommand("procon.protected.send", "banList.list");
                }
                if (banbypbguid == enumBoolYesNo.Yes)
                {
                    ExecuteCommand("procon.protected.send", "punkBuster.pb_sv_command", String.Format("pb_sv_kick \"{0}\" {1} \"{2}\"", name, tbantime.ToString(), "BC2! " + msg));
                }
            }
            else if (enablekick == enumBoolYesNo.Yes) //is kick enabled?
            {
                string msg = formatMsg(kicktext, name, bestreason, m_dicPBPlayerInfo[name].GUID, m_dicPlayerInfo[name].GUID);
                ExecuteCommand("procon.protected.send", "admin.kickPlayer", name, msg);
            }

            if (enableban == enumBoolYesNo.Yes || enabletban == enumBoolYesNo.Yes || enablekick == enumBoolYesNo.Yes)
            {
                ExecuteCommand("procon.protected.send", "admin.kickPlayer", name, ""); //just to be sure hes gone
            }
        }

        #region SendCacheRequest
        public void SendCacheRequest(String playerName, String requestType)
        {
            /* 
            This is like setting up a BackgroundWorker and launching it asynchronously.
            */
            Hashtable request = new Hashtable();
            request["playerName"] = playerName;
            request["pluginName"] = "CheatDetector"; // your plugin name here
            request["pluginMethod"] = "CacheAnalyzeResult"; // your callback method name here
            request["requestType"] = requestType;

            ExecuteCommand("procon.protected.plugins.call", "CBattlelogCache", "PlayerLookup", JSON.JsonEncode(request));
        }
        #endregion

        #region checkKillsperMin
        public double checkKillsperMin(ref Dictionary<double, string> reasons, string game, string playername, string weaponname, double kills, double time)
        {
            time = time / 60.0;

            if (!IsAllowedWeapon(weaponname))
            {
                return 0.0;
            }
            
            //if (weaponname == "Underslung Shotgun" || 
            //    weaponname == "XP1 Jackhammer" || 
            //    weaponname == "M1014" || 
            //    weaponname == "DAO" || 
            //    weaponname == "XP2 SPAS12" || 
            //    weaponname == "870" || 
            //    weaponname == "USAS" || 
            //    weaponname == "Saiga" || 
            //    weaponname == "RPG-7" || 
            //    weaponname == "Mk153 SMAW" || 
            //    weaponname == "FGM-148 JAVELIN" || 
            //    weaponname == "Underslung Launcher" || 
            //    weaponname == "SA-18 IGLA AA" || 
            //    weaponname == "FIM-92 STINGER AA" || 
            //    weaponname == "Knife") //unreliable
            //    return 0.0;

            //if (weaponname == "Crossbow Scoped" || 
            //    weaponname == "Crossbow kobra") //no avg yet
            //    return 0.0;

            //if (weaponname == "WARSAW_ID_P_INAME_SRAW" ||
            //    weaponname == "WARSAW_ID_P_INAME_M2" ||
            //    weaponname == "WARSAW_ID_P_INAME_V40" ||
            //    weaponname == "WARSAW_ID_P_INAME_MACHETE" ||
            //    weaponname == "WARSAW_ID_P_WNAME_SHORTY" ||
            //    weaponname == "WARSAW_ID_P_INAME_C4" ||
            //    weaponname == "WARSAW_ID_P_WNAME_USAS12" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE4" ||
            //    weaponname == "WARSAW_ID_P_INAME_M32MGL" ||
            //    weaponname == "WARSAW_ID_P_INAME_RPG7" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM" ||
            //    weaponname == "WARSAW_ID_P_INAME_ACB90" ||
            //    weaponname == "WARSAW_ID_P_INAME_STARSTREAK" ||
            //    weaponname == "WARSAW_ID_P_INAME_BAYONETT" ||
            //    weaponname == "WARSAW_ID_P_INAME_M67" ||
            //    weaponname == "WARSAW_ID_P_INAME_MBTLAW" ||
            //    weaponname == "WARSAW_ID_P_SP_WNAME_USAS12NV" ||
            //    weaponname == "WARSAW_ID_P_INAME_FIM92" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE2" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE3" ||
            //    weaponname == "WARSAW_ID_P_INAME_DEFIB" ||
            //    weaponname == "WARSAW_ID_P_WNAME_870" ||
            //    weaponname == "WARSAW_ID_P_INAME_M15" ||
            //    weaponname == "WARSAW_ID_P_INAME_MORTAR" ||
            //    weaponname == "WARSAW_ID_P_XP1_VNAME_UCAV" ||
            //    weaponname == "WARSAW_ID_P_INAME_REPAIR" ||
            //    weaponname == "WARSAW_ID_P_INAME_XM25_SMK" ||
            //    weaponname == "WARSAW_ID_P_WNAME_SAIGA12" ||
            //    weaponname == "WARSAW_ID_P_WNAME_DBV12" ||
            //    weaponname == "WARSAW_ID_P_INAME_XM25" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM_SMK" ||
            //    weaponname == "WARSAW_ID_P_WNAME_M1014" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM_SHG" ||
            //    weaponname == "WARSAW_ID_P_WNAME_SPAS12" ||
            //    weaponname == "WARSAW_ID_P_INAME_XM25_FLECHETTE" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM_LVG" ||
            //    weaponname == "WARSAW_ID_P_INAME_IMPACT" ||
            //    weaponname == "WARSAW_ID_P_INAME_FGM148" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE6" ||
            //    weaponname == "WARSAW_ID_P_INAME_M26_FRAG" ||
            //    weaponname == "WARSAW_ID_P_WNAME_UTAS" ||
            //    weaponname == "WARSAW_ID_P_INAME_M34" ||
            //    weaponname == "WARSAW_ID_P_WNAME_HAWK" ||
            //    weaponname == "WARSAW_ID_P_INAME_M26_MASS" ||
            //    weaponname == "WARSAW_ID_P_INAME_M18" ||
            //    weaponname == "WARSAW_ID_P_INAME_SHANK" ||
            //    weaponname == "WARSAW_ID_P_INAME_IGLA" ||
            //    weaponname == "WARSAW_ID_P_INAME_M26_SLUG" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE5" ||
            //    weaponname == "WARSAW_ID_P_INAME_M26_FLECHETTE" ||
            //    weaponname == "WARSAW_ID_P_INAME_FLASHBANG" ||
            //    weaponname == "WARSAW_ID_P_INAME_CLAYMORE" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM_FLASH" ||
            //    weaponname == "WARSAW_ID_P_INAME_M136" ||
            //    weaponname == "WARSAW_ID_P_INAME_FLARE" ||
            //    weaponname == "WARSAW_ID_P_INAME_SMAW"||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE7" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE8" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE1" ||
            //    weaponname == "WARSAW_ID_P_XP0_WNAME_DAO12"||
            //    weaponname == "WARSAW_ID_P_XP2_INAME_AAMINE" ||
            //    weaponname == "WARSAW_ID_P_INAME_DIVERKNIFE" ||
            //    weaponname == "WARSAW_ID_P_INAME_KNIFE14100BT" ||
            //    weaponname == "WARSAW_ID_P_XP2_INAME_40MM_3GL" ||
            //    weaponname == "WARSAW_ID_P_INAME_KNIFE2142" ||
            //    weaponname == "WARSAW_ID_P_INAME_NECKKNIFE" ||
            //    weaponname == "WARSAW_ID_P_INAME_KNIFEPRECISION")
            //    return 0.0;

            //if (weaponname == "WARSAW_ID_P_INAME_EOD" || //no avg yet
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_BULLDOG" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_DEAGLE" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_MPX" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_SHIELD" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_UN6" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_CS5")
            //    return 0.0;

            //double avgkm;
            double avgkmyellow;
            double avgkmorange;
            double avgkmred;

            switch (game.ToLower() + weaponname)
            {
                case "bf3870": avgkmyellow = 7.34380496596013; avgkmorange = 9.7917399546135; avgkmred = 11.4236966137157; break;
                case "bf3A91": avgkmyellow = 4.81602524247504; avgkmorange = 6.42136698996672; avgkmred = 7.49159482162784; break;
                case "bf3AEK971": avgkmyellow = 6.11283235160259; avgkmorange = 8.15044313547012; avgkmred = 9.50885032471514; break;
                case "bf3AK74M": avgkmyellow = 4.9614921571662; avgkmorange = 6.6153228762216; avgkmred = 7.7178766889252; break;
                case "bf3AKS74U": avgkmyellow = 3.78380641487697; avgkmorange = 5.04507521983595; avgkmred = 5.88592108980861; break;
                case "bf3AN94": avgkmyellow = 6.15855224407648; avgkmorange = 8.21140299210198; avgkmred = 9.57997015745231; break;
                case "bf3AS-VAL": avgkmyellow = 5.31166510034712; avgkmorange = 7.08222013379616; avgkmred = 8.26259015609552; break;
                case "bf3Crossbow kobra": avgkmyellow = 4.35048651042388; avgkmorange = 5.80064868056518; avgkmred = 6.76742346065937; break;
                case "bf3Crossbow Scoped": avgkmyellow = 3.26011950361764; avgkmorange = 4.34682600482352; avgkmred = 5.07129700562744; break;
                case "bf3DAO": avgkmyellow = 7.80034598418042; avgkmorange = 10.4004613122406; avgkmred = 12.1338715309473; break;
                case "bf3F2000": avgkmyellow = 6.32524974755175; avgkmorange = 8.433666330069; avgkmred = 9.8392773850805; break;
                case "bf3FGM-148 JAVELIN": avgkmyellow = 2.17309655292367; avgkmorange = 2.8974620705649; avgkmred = 3.38037241565905; break;
                case "bf3FIM-92 STINGER AA": avgkmyellow = 1.79372684333205; avgkmorange = 2.3916357911094; avgkmred = 2.7902417562943; break;
                case "bf3G36C": avgkmyellow = 4.55982013071387; avgkmorange = 6.07976017428516; avgkmred = 7.09305353666602; break;
                case "bf3G3A4": avgkmyellow = 6.47966385873189; avgkmorange = 8.63955181164252; avgkmred = 10.0794771135829; break;
                case "bf3Glock 17": avgkmyellow = 6.65590075071673; avgkmorange = 8.87453433428898; avgkmred = 10.3536233900038; break;
                case "bf3Glock 17 Silenced": avgkmyellow = 6.70023768402018; avgkmorange = 8.93365024536024; avgkmred = 10.4225919529203; break;
                case "bf3Glock 18": avgkmyellow = 6.74560187907746; avgkmorange = 8.99413583876994; avgkmred = 10.4931584785649; break;
                case "bf3Glock 18 Silenced": avgkmyellow = 6.53297017249616; avgkmorange = 8.71062689666154; avgkmred = 10.1623980461051; break;
                case "bf3KH2002": avgkmyellow = 6.71786182786091; avgkmorange = 8.95714910381454; avgkmred = 10.4500072877836; break;
                case "bf3Knife": avgkmyellow = 30.1665906605025; avgkmorange = 40.2221208806699; avgkmred = 46.9258076941149; break;
                case "bf3M1014": avgkmyellow = 8.02562516253324; avgkmorange = 10.7008335500443; avgkmred = 12.484305808385; break;
                case "bf3M16A4": avgkmyellow = 5.37412140805536; avgkmorange = 7.16549521074048; avgkmred = 8.35974441253056; break;
                case "bf3M1911": avgkmyellow = 6.46634693958021; avgkmorange = 8.62179591944028; avgkmred = 10.0587619060137; break;
                case "bf3M1911 LIT": avgkmyellow = 6.51325541286704; avgkmorange = 8.68434055048938; avgkmred = 10.1317306422376; break;
                case "bf3M1911 SILENCED": avgkmyellow = 5.90996258270316; avgkmorange = 7.87995011027088; avgkmred = 9.19327512864936; break;
                case "bf3M1911 Tactical": avgkmyellow = 8.10126672840981; avgkmorange = 10.8016889712131; avgkmred = 12.6019704664153; break;
                case "bf3M240": avgkmyellow = 5.9865255761022; avgkmorange = 7.9820341014696; avgkmred = 9.3123731183812; break;
                case "bf3M249": avgkmyellow = 4.80827066031022; avgkmorange = 6.4110275470803; avgkmred = 7.47953213826035; break;
                case "bf3M27": avgkmyellow = 4.87158081190884; avgkmorange = 6.49544108254512; avgkmred = 7.57801459630264; break;
                case "bf3M39": avgkmyellow = 4.16089292978913; avgkmorange = 5.54785723971884; avgkmred = 6.47250011300532; break;
                case "bf3M40A5": avgkmyellow = 3.80344208093644; avgkmorange = 5.07125610791525; avgkmred = 5.91646545923446; break;
                case "bf3M412 Rex": avgkmyellow = 6.42665326102015; avgkmorange = 8.56887101469354; avgkmred = 9.99701618380913; break;
                case "bf3M416": avgkmyellow = 5.60839308383372; avgkmorange = 7.47785744511162; avgkmred = 8.72416701929689; break;
                case "bf3M4A1": avgkmyellow = 3.74705645995069; avgkmorange = 4.99607527993425; avgkmred = 5.82875449325662; break;
                case "bf3M60": avgkmyellow = 6.04617359143032; avgkmorange = 8.06156478857376; avgkmred = 9.40515892000272; break;
                case "bf3M9": avgkmyellow = 5.94817416614142; avgkmorange = 7.93089888818856; avgkmred = 9.25271536955332; break;
                case "bf3M9 Flashlight": avgkmyellow = 6.4777604632551; avgkmorange = 8.6370139510068; avgkmred = 10.0765162761746; break;
                case "bf3M9 Silenced": avgkmyellow = 6.21856211112697; avgkmorange = 8.2914161481693; avgkmred = 9.67331883953085; break;
                case "bf3M93R": avgkmyellow = 6.77036955129336; avgkmorange = 9.02715940172448; avgkmred = 10.5316859686786; break;
                case "bf3M98B": avgkmyellow = 3.13157433023949; avgkmorange = 4.17543244031932; avgkmred = 4.87133784703921; break;
                case "bf3MK11": avgkmyellow = 3.70900812076265; avgkmorange = 4.94534416101687; avgkmred = 5.76956818785302; break;
                case "bf3Mk153 SMAW": avgkmyellow = 2.7590535608374; avgkmorange = 3.67873808111653; avgkmred = 4.29186109463595; break;
                case "bf3MP 443": avgkmyellow = 5.57097231475796; avgkmorange = 7.42796308634394; avgkmred = 8.66595693406793; break;
                case "bf3MP443 LIT": avgkmyellow = 7.11840105315783; avgkmorange = 9.49120140421044; avgkmred = 11.0730683049122; break;
                case "bf3MP443 Silenced": avgkmyellow = 6.24570054324696; avgkmorange = 8.32760072432928; avgkmred = 9.71553417838416; break;
                case "bf3MP7": avgkmyellow = 6.40557958076316; avgkmorange = 8.54077277435088; avgkmred = 9.96423490340936; break;
                case "bf3P90": avgkmyellow = 6.59335285673667; avgkmorange = 8.79113714231556; avgkmred = 10.2563266660348; break;
                case "bf3PDR": avgkmyellow = 5.75780948433041; avgkmorange = 7.67707931244054; avgkmred = 8.95659253118063; break;
                case "bf3Pecheneg": avgkmyellow = 6.01051431856279; avgkmorange = 8.01401909141706; avgkmred = 9.34968893998657; break;
                case "bf3PP2000": avgkmyellow = 5.40979984627056; avgkmorange = 7.21306646169408; avgkmred = 8.41524420530976; break;
                case "bf3RPG-7": avgkmyellow = 3.06362670546972; avgkmorange = 4.08483560729296; avgkmred = 4.76564154184179; break;
                case "bf3RPK": avgkmyellow = 4.69105587731975; avgkmorange = 6.25474116975966; avgkmred = 7.29719803138627; break;
                case "bf3SA-18 IGLA AA": avgkmyellow = 1.5717712519806; avgkmorange = 2.0956950026408; avgkmred = 2.44497750308093; break;
                case "bf3Saiga": avgkmyellow = 8.40312455207724; avgkmorange = 11.2041660694363; avgkmred = 13.071527081009; break;
                case "bf3SCAR": avgkmyellow = 4.02913750624579; avgkmorange = 5.37218334166106; avgkmred = 6.2675472319379; break;
                case "bf3SG553": avgkmyellow = 4.97227449871607; avgkmorange = 6.62969933162142; avgkmred = 7.73464922022499; break;
                case "bf3SKS": avgkmyellow = 4.88381341994941; avgkmorange = 6.51175122659922; avgkmred = 7.59704309769909; break;
                case "bf3SV98": avgkmyellow = 2.69624366487831; avgkmorange = 3.59499155317108; avgkmred = 4.19415681203293; break;
                case "bf3SVD": avgkmyellow = 3.77574974703374; avgkmorange = 5.03433299604499; avgkmred = 5.87338849538582; break;
                case "bf3Taurus 44": avgkmyellow = 5.81112670350586; avgkmorange = 7.74816893800782; avgkmred = 9.03953042767579; break;
                case "bf3Taurus 44 scoped": avgkmyellow = 5.10963755441427; avgkmorange = 6.81285007255236; avgkmred = 7.94832508464442; break;
                case "bf3Type88": avgkmyellow = 4.58810896306342; avgkmorange = 6.1174786174179; avgkmred = 7.13705838698755; break;
                case "bf3UMP": avgkmyellow = 5.9712228228375; avgkmorange = 7.96163043045; avgkmred = 9.288568835525; break;
                case "bf3Underslung Launcher": avgkmyellow = 5.22695525048829; avgkmorange = 6.96927366731772; avgkmred = 8.13081927853734; break;
                case "bf3Underslung Shotgun": avgkmyellow = 9.93881644618981; avgkmorange = 13.2517552615864; avgkmred = 15.4603811385175; break;
                case "bf3USAS": avgkmyellow = 7.46327426015988; avgkmorange = 9.95103234687984; avgkmred = 11.6095377380265; break;
                case "bf3XP1 FAMAS": avgkmyellow = 5.95157658039927; avgkmorange = 7.93543544053236; avgkmred = 9.25800801395442; break;
                case "bf3XP1 HK53": avgkmyellow = 5.15000483942539; avgkmorange = 6.86667311923386; avgkmred = 8.01111863910617; break;
                case "bf3XP1 Jackhammer": avgkmyellow = 8.50470723965561; avgkmorange = 11.3396096528741; avgkmred = 13.2295445950198; break;
                case "bf3XP1 L85A2": avgkmyellow = 6.37678095962189; avgkmorange = 8.50237461282918; avgkmred = 9.91943704830071; break;
                case "bf3XP1 L96": avgkmyellow = 3.19118085054413; avgkmorange = 4.25490780072551; avgkmred = 4.96405910084643; break;
                case "bf3XP1 MG36": avgkmyellow = 5.67226728317709; avgkmorange = 7.56302304423612; avgkmred = 8.82352688494214; break;
                case "bf3XP1 PP19": avgkmyellow = 6.33877814209095; avgkmorange = 8.4517041894546; avgkmred = 9.8603215543637; break;
                case "bf3XP1 QBB95": avgkmyellow = 5.64359941004062; avgkmorange = 7.5247992133875; avgkmred = 8.77893241561875; break;
                case "bf3XP1 QBU88": avgkmyellow = 4.93794679801963; avgkmorange = 6.58392906402618; avgkmred = 7.68125057469721; break;
                case "bf3XP1 QBZ95B": avgkmyellow = 5.71311793051213; avgkmorange = 7.61749057401618; avgkmred = 8.88707233635221; break;
                case "bf3XP2 ACR": avgkmyellow = 4.89742684155504; avgkmorange = 6.52990245540672; avgkmred = 7.61821953130784; break;
                case "bf3XP2 AUG": avgkmyellow = 6.76592001498609; avgkmorange = 9.02122668664812; avgkmred = 10.5247644677561; break;
                case "bf3XP2 HK417": avgkmyellow = 6.18684601031766; avgkmorange = 8.24912801375688; avgkmred = 9.62398268271636; break;
                case "bf3XP2 JNG90": avgkmyellow = 3.67149847469365; avgkmorange = 4.89533129959154; avgkmred = 5.71121984952346; break;
                case "bf3XP2 L86A1": avgkmyellow = 5.76649077421; avgkmorange = 7.68865436561334; avgkmred = 8.97009675988223; break;
                case "bf3XP2 LSAT": avgkmyellow = 6.2773725225181; avgkmorange = 8.36983003002414; avgkmred = 9.76480170169483; break;
                case "bf3XP2 MP5K": avgkmyellow = 8.09258408801437; avgkmorange = 10.7901121173525; avgkmred = 12.5884641369112; break;
                case "bf3XP2 MTAR-21": avgkmyellow = 6.23607359210334; avgkmorange = 8.31476478947112; avgkmred = 9.70055892104964; break;
                case "bf3XP2 SCARL": avgkmyellow = 6.90825024591705; avgkmorange = 9.2110003278894; avgkmred = 10.7461670492043; break;
                case "bf3XP2 SPAS12": avgkmyellow = 8.38991581822629; avgkmorange = 11.1865544243017; avgkmred = 13.0509801616853; break;
                case "bf440MM": avgkmyellow = 6.01854217933855; avgkmorange = 8.02472290578474; avgkmred = 9.36217672341553; break;
                case "bf440MM_3GL": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf440MM_FLASH": avgkmyellow = 5.13421461420998; avgkmorange = 6.8456194856133; avgkmred = 7.98655606654885; break;
                case "bf440MM_LVG": avgkmyellow = 7.24932128447483; avgkmorange = 9.6657617126331; avgkmred = 11.276721998072; break;
                case "bf440MM_SHG": avgkmyellow = 9.6642228758423; avgkmorange = 12.8856305011231; avgkmred = 15.0332355846436; break;
                case "bf440MM_SMK": avgkmyellow = 5.97301267723519; avgkmorange = 7.96401690298026; avgkmred = 9.29135305347697; break;
                case "bf4870": avgkmyellow = 6.53145878358391; avgkmorange = 8.70861171144522; avgkmred = 10.1600469966861; break;
                case "bf493R": avgkmyellow = 7.86785492566512; avgkmorange = 10.4904732342202; avgkmred = 12.2388854399235; break;
                case "bf4A91": avgkmyellow = 5.59535792390568; avgkmorange = 7.46047723187424; avgkmred = 8.70389010385328; break;
                case "bf4AAMINE": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4ACB90": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4ACR": avgkmyellow = 4.94064385255755; avgkmorange = 6.5875251367434; avgkmred = 7.6854459928673; break;
                case "bf4AEK971": avgkmyellow = 6.04444452124014; avgkmorange = 8.05925936165352; avgkmred = 9.40246925526244; break;
                case "bf4AK12": avgkmyellow = 4.2149012956022; avgkmorange = 5.61986839413627; avgkmred = 6.55651312649231; break;
                case "bf4AK5C": avgkmyellow = 4.28294666227193; avgkmorange = 5.71059554969591; avgkmred = 6.66236147464523; break;
                case "bf4AKU12": avgkmyellow = 4.82862913498431; avgkmorange = 6.43817217997908; avgkmred = 7.51120087664226; break;
                case "bf4AMR2": avgkmyellow = 3.10909460921267; avgkmorange = 4.14545947895022; avgkmred = 4.83636939210859; break;
                case "bf4AMR2CQB": avgkmyellow = 8.12286892202706; avgkmorange = 10.8304918960361; avgkmred = 12.6355738787088; break;
                case "bf4AMR2MID": avgkmyellow = 6.1105032332604; avgkmorange = 8.1473376443472; avgkmred = 9.5052272517384; break;
                case "bf4AR160": avgkmyellow = 6.01017695343216; avgkmorange = 8.01356927124288; avgkmred = 9.34916414978336; break;
                case "bf4ASVAL": avgkmyellow = 5.65842233129126; avgkmorange = 7.54456310838834; avgkmred = 8.80199029311973; break;
                case "bf4AWS": avgkmyellow = 6.04625044308498; avgkmorange = 8.06166725744664; avgkmred = 9.40527846702108; break;
                case "bf4BAYONETT": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4BPKNIFE1": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4BPKNIFE2": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4BPKNIFE3": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4BPKNIFE4": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4BPKNIFE5": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4BPKNIFE6": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4BPKNIFE7": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4BPKNIFE8": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4BULLDOG": avgkmyellow = 6.45327898658294; avgkmorange = 8.60437198211058; avgkmred = 10.038433979129; break;
                case "bf4C4": avgkmyellow = 6.25351565752015; avgkmorange = 8.33802087669354; avgkmred = 9.72769102280913; break;
                case "bf4CBJMS": avgkmyellow = 6.08944928967846; avgkmorange = 8.11926571957128; avgkmred = 9.47247667283316; break;
                case "bf4CLAYMORE": avgkmyellow = 19.8493990134733; avgkmorange = 26.4658653512977; avgkmred = 30.8768429098473; break;
                case "bf4CS5": avgkmyellow = 4.65424159678299; avgkmorange = 6.20565546237732; avgkmred = 7.23993137277354; break;
                case "bf4CSLR4": avgkmyellow = 2.71370232020676; avgkmorange = 3.61826976027568; avgkmred = 4.22131472032162; break;
                case "bf4CZ75": avgkmyellow = 8.14009367851708; avgkmorange = 10.8534582380228; avgkmred = 12.6623679443599; break;
                case "bf4CZ805": avgkmyellow = 6.49327128522426; avgkmorange = 8.65769504696568; avgkmred = 10.10064422146; break;
                case "bf4DAO12": avgkmyellow = 7.48782180211626; avgkmorange = 9.98376240282168; avgkmred = 11.647722803292; break;
                case "bf4DBV12": avgkmyellow = 7.44963319270354; avgkmorange = 9.93284425693806; avgkmred = 11.5883182997611; break;
                case "bf4DEAGLE": avgkmyellow = 8.60078029356455; avgkmorange = 11.4677070580861; avgkmred = 13.3789915677671; break;
                case "bf4DEFIB": avgkmyellow = 1.79536077007686; avgkmorange = 2.39381436010247; avgkmred = 2.79278342011955; break;
                case "bf4DIVERKNIFE": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4EOD": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4F2000": avgkmyellow = 6.33978994558482; avgkmorange = 8.45305326077976; avgkmred = 9.86189547090972; break;
                case "bf4FAMAS": avgkmyellow = 6.29918680819284; avgkmorange = 8.39891574425712; avgkmred = 9.79873503496664; break;
                case "bf4FGM148": avgkmyellow = 3.29563094645717; avgkmorange = 4.39417459527622; avgkmred = 5.12653702782226; break;
                case "bf4FIM92": avgkmyellow = 2.17646197354172; avgkmorange = 2.90194929805562; avgkmred = 3.38560751439823; break;
                case "bf4FLARE": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4FLASHBANG": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4FN57": avgkmyellow = 7.64876696131944; avgkmorange = 10.1983559484259; avgkmred = 11.8980819398302; break;
                case "bf4FYJS": avgkmyellow = 4.56783086703991; avgkmorange = 6.09044115605322; avgkmred = 7.10551468206209; break;
                case "bf4G36C": avgkmyellow = 4.93433344914903; avgkmorange = 6.57911126553204; avgkmred = 7.67562980978738; break;
                case "bf4GALIL21": avgkmyellow = 5.60447285880731; avgkmorange = 7.47263047840974; avgkmred = 8.71806889147803; break;
                case "bf4GALIL23": avgkmyellow = 6.11522151938339; avgkmorange = 8.15362869251118; avgkmred = 9.51256680792971; break;
                case "bf4GALIL52": avgkmyellow = 5.22368862960172; avgkmorange = 6.9649181728023; avgkmred = 8.12573786826935; break;
                case "bf4GALIL53": avgkmyellow = 5.419917924804; avgkmorange = 7.226557233072; avgkmred = 8.430983438584; break;
                case "bf4GLOCK18": avgkmyellow = 7.37017585752375; avgkmorange = 9.826901143365; avgkmred = 11.4647180005925; break;
                case "bf4GOL": avgkmyellow = 4.39916403206795; avgkmorange = 5.86555204275727; avgkmred = 6.84314404988348; break;
                case "bf4HAWK": avgkmyellow = 6.76772092703187; avgkmorange = 9.02362790270916; avgkmred = 10.527565886494; break;
                case "bf4HK45C": avgkmyellow = 7.76367266572516; avgkmorange = 10.3515635543002; avgkmred = 12.0768241466836; break;
                case "bf4IGLA": avgkmyellow = 2.31608894846573; avgkmorange = 3.08811859795431; avgkmred = 3.6028050309467; break;
                case "bf4IMPACT": avgkmyellow = 24.1805418572343; avgkmorange = 32.2407224763124; avgkmred = 37.6141762223644; break;
                case "bf4JNG90": avgkmyellow = 3.3603115887306; avgkmorange = 4.48041545164081; avgkmred = 5.22715136024761; break;
                case "bf4JS2": avgkmyellow = 5.70473060180846; avgkmorange = 7.60630746907794; avgkmred = 8.87402538059093; break;
                case "bf4KNIFE14100BT": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4KNIFE2142": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4KNIFEPRECISION": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4L85A2": avgkmyellow = 6.51174594306471; avgkmorange = 8.68232792408628; avgkmred = 10.1293825781007; break;
                case "bf4L96A1": avgkmyellow = 3.81209456264897; avgkmorange = 5.08279275019863; avgkmred = 5.92992487523173; break;
                case "bf4LSAT": avgkmyellow = 4.92734542101259; avgkmorange = 6.56979389468346; avgkmred = 7.66475954379737; break;
                case "bf4M1014": avgkmyellow = 6.55000789831205; avgkmorange = 8.73334386441606; avgkmred = 10.1889011751521; break;
                case "bf4M136": avgkmyellow = 1.74829291264422; avgkmorange = 2.33105721685896; avgkmred = 2.71956675300212; break;
                case "bf4M15": avgkmyellow = 7.76689875026204; avgkmorange = 10.3558650003494; avgkmred = 12.0818425004076; break;
                case "bf4M16A4": avgkmyellow = 6.17359039393257; avgkmorange = 8.23145385857676; avgkmred = 9.60336283500622; break;
                case "bf4M18": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4M1911": avgkmyellow = 6.81329827201635; avgkmorange = 9.0843976960218; avgkmred = 10.5984639786921; break;
                case "bf4M2": avgkmyellow = 9.300556429529; avgkmorange = 12.4007419060387; avgkmred = 14.4675322237118; break;
                case "bf4M200": avgkmyellow = 3.64827488195617; avgkmorange = 4.86436650927489; avgkmred = 5.67509426082071; break;
                case "bf4M240": avgkmyellow = 6.02316273524778; avgkmorange = 8.03088364699704; avgkmred = 9.36936425482988; break;
                case "bf4M249": avgkmyellow = 5.59940883188611; avgkmorange = 7.46587844251482; avgkmred = 8.71019151626729; break;
                case "bf4M26_FLECHETTE": avgkmyellow = 12.6691750184222; avgkmorange = 16.8922333578963; avgkmred = 19.7076055842123; break;
                case "bf4M26_FRAG": avgkmyellow = 3.61519533058095; avgkmorange = 4.82026044077459; avgkmred = 5.62363718090369; break;
                case "bf4M26_MASS": avgkmyellow = 11.2827516530638; avgkmorange = 15.0436688707517; avgkmred = 17.550947015877; break;
                case "bf4M26_SLUG": avgkmyellow = 7.75984830744136; avgkmorange = 10.3464644099218; avgkmred = 12.0708751449088; break;
                case "bf4M32MGL": avgkmyellow = 7.34778054292847; avgkmorange = 9.79704072390462; avgkmred = 11.4298808445554; break;
                case "bf4M34": avgkmyellow = 19.3723606981976; avgkmorange = 25.8298142642634; avgkmred = 30.1347833083073; break;
                case "bf4M39": avgkmyellow = 4.89135477275257; avgkmorange = 6.5218063636701; avgkmred = 7.60877409094845; break;
                case "bf4M40A5": avgkmyellow = 3.2965114661196; avgkmorange = 4.3953486214928; avgkmred = 5.12790672507494; break;
                case "bf4M412REX": avgkmyellow = 5.0325667890165; avgkmorange = 6.710089052022; avgkmred = 7.828437227359; break;
                case "bf4M416": avgkmyellow = 5.22607575566871; avgkmorange = 6.96810100755828; avgkmred = 8.12945117548466; break;
                case "bf4M4A1": avgkmyellow = 5.35884689719773; avgkmorange = 7.14512919626364; avgkmred = 8.33598406230758; break;
                case "bf4M60E4": avgkmyellow = 5.64471481000122; avgkmorange = 7.52628641333496; avgkmred = 8.78066748222412; break;
                case "bf4M67": avgkmyellow = 26.7527167214526; avgkmorange = 35.6702889619368; avgkmred = 41.6153371222596; break;
                case "bf4M82A3": avgkmyellow = 6.87683512901331; avgkmorange = 9.16911350535108; avgkmred = 10.6972990895763; break;
                case "bf4M82A3CQB": avgkmyellow = 9.16689397342639; avgkmorange = 12.2225252979019; avgkmred = 14.2596128475522; break;
                case "bf4M82A3MID": avgkmyellow = 5.2137999729945; avgkmorange = 6.951733297326; avgkmred = 8.110355513547; break;
                case "bf4M9": avgkmyellow = 7.19082907906455; avgkmorange = 9.5877721054194; avgkmred = 11.1857341229893; break;
                case "bf4M98B": avgkmyellow = 3.92730004929293; avgkmorange = 5.23640006572391; avgkmred = 6.10913341001123; break;
                case "bf4MACHETE": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4MBTLAW": avgkmyellow = 3.08586494103715; avgkmorange = 4.11448658804953; avgkmred = 4.80023435272445; break;
                case "bf4MG4": avgkmyellow = 6.35310952531195; avgkmorange = 8.47081270041594; avgkmred = 9.88261481715193; break;
                case "bf4MK11": avgkmyellow = 4.4870652018708; avgkmorange = 5.9827536024944; avgkmred = 6.97987920291014; break;
                case "bf4MORTAR": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4MP443": avgkmyellow = 7.4645887753786; avgkmorange = 9.95278503383814; avgkmred = 11.6115825394778; break;
                case "bf4MP7": avgkmyellow = 5.11158202869028; avgkmorange = 6.81544270492038; avgkmred = 7.95134982240711; break;
                case "bf4MPX": avgkmyellow = 6.81604711832165; avgkmorange = 9.08806282442886; avgkmred = 10.6027399618337; break;
                case "bf4MTAR21": avgkmyellow = 5.6016282546797; avgkmorange = 7.46883767290626; avgkmred = 8.71364395172397; break;
                case "bf4MX4": avgkmyellow = 3.98528081622083; avgkmorange = 5.3137077549611; avgkmred = 6.19932571412129; break;
                case "bf4NECKKNIFE": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4P226": avgkmyellow = 6.4411752349103; avgkmorange = 8.58823364654706; avgkmred = 10.0196059209716; break;
                case "bf4P90": avgkmyellow = 5.3556785040452; avgkmorange = 7.14090467206026; avgkmred = 8.33105545073697; break;
                case "bf4PDR": avgkmyellow = 5.46240933647311; avgkmorange = 7.28321244863082; avgkmred = 8.49708119006929; break;
                case "bf4PECHENEG": avgkmyellow = 5.810010681402; avgkmorange = 7.746680908536; avgkmred = 9.037794393292; break;
                case "bf4PP2000": avgkmyellow = 4.66957943067271; avgkmorange = 6.22610590756362; avgkmred = 7.26379022549089; break;
                case "bf4QBB95": avgkmyellow = 5.23131546041334; avgkmorange = 6.97508728055112; avgkmred = 8.13760182730964; break;
                case "bf4QBS09": avgkmyellow = 5.67336663209898; avgkmorange = 7.56448884279864; avgkmred = 8.82523698326508; break;
                case "bf4QBU88": avgkmyellow = 4.82403530039067; avgkmorange = 6.43204706718756; avgkmred = 7.50405491171882; break;
                case "bf4QBZ951": avgkmyellow = 5.7548575017419; avgkmorange = 7.67314333565586; avgkmred = 8.95200055826517; break;
                case "bf4QSZ92": avgkmyellow = 6.84692612483355; avgkmorange = 9.1292348331114; avgkmred = 10.6507739719633; break;
                case "bf4REPAIR": avgkmyellow = 1.11258590193355; avgkmorange = 1.48344786924473; avgkmred = 1.73068918078552; break;
                case "bf4RFBTARGET": avgkmyellow = 4.01033244364064; avgkmorange = 5.34710992485418; avgkmred = 6.23829491232988; break;
                case "bf4RPG7": avgkmyellow = 3.64171967848459; avgkmorange = 4.85562623797946; avgkmred = 5.6648972776427; break;
                case "bf4RPK12": avgkmyellow = 5.23922164309557; avgkmorange = 6.98562885746076; avgkmred = 8.14990033370422; break;
                case "bf4RPK74": avgkmyellow = 4.96026266426195; avgkmorange = 6.61368355234926; avgkmred = 7.71596414440747; break;
                case "bf4SAIGA12": avgkmyellow = 7.18981661957931; avgkmorange = 9.58642215943908; avgkmred = 11.1841591860123; break;
                case "bf4SAR21": avgkmyellow = 5.24955237148683; avgkmorange = 6.99940316198244; avgkmred = 8.16597035564618; break;
                case "bf4SCARH": avgkmyellow = 5.20090364475513; avgkmorange = 6.93453819300684; avgkmred = 8.09029455850798; break;
                case "bf4SCARHSV": avgkmyellow = 5.05037436563299; avgkmorange = 6.73383248751066; avgkmred = 7.85613790209577; break;
                case "bf4SCORP": avgkmyellow = 6.66923751583902; avgkmorange = 8.89231668778536; avgkmred = 10.3743694690829; break;
                case "bf4SCOUTELIT": avgkmyellow = 3.77169743872533; avgkmorange = 5.02892991830044; avgkmred = 5.86708490468384; break;
                case "bf4SG553": avgkmyellow = 4.90515942220681; avgkmorange = 6.54021256294242; avgkmred = 7.63024799009949; break;
                case "bf4SHANK": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4SHIELD": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4SHORTY": avgkmyellow = 7.80046473738973; avgkmorange = 10.400619649853; avgkmred = 12.1340562581618; break;
                case "bf4SKS": avgkmyellow = 5.15590345054903; avgkmorange = 6.87453793406538; avgkmred = 8.02029425640961; break;
                case "bf4SMAW": avgkmyellow = 3.74746537095155; avgkmorange = 4.99662049460206; avgkmred = 5.82939057703574; break;
                case "bf4SPAS12": avgkmyellow = 7.35865249464486; avgkmorange = 9.81153665952648; avgkmred = 11.4467927694476; break;
                case "bf4SR2": avgkmyellow = 5.68032664384188; avgkmorange = 7.57376885845584; avgkmred = 8.83606366819848; break;
                case "bf4SR338": avgkmyellow = 3.85663309461467; avgkmorange = 5.14217745948622; avgkmred = 5.99920703606726; break;
                case "bf4SRAW": avgkmyellow = 3.41621019312056; avgkmorange = 4.55494692416075; avgkmred = 5.31410474485421; break;
                case "bf4SRS": avgkmyellow = 3.94565196455533; avgkmorange = 5.26086928607377; avgkmred = 6.13768083375273; break;
                case "bf4STARSTREAK": avgkmyellow = 4.51180533514104; avgkmorange = 6.01574044685472; avgkmred = 7.01836385466384; break;
                case "bf4STEYRAUG": avgkmyellow = 5.84717944380647; avgkmorange = 7.79623925840862; avgkmred = 9.09561246814339; break;
                case "bf4SV98": avgkmyellow = 3.94439146402529; avgkmorange = 5.25918861870038; avgkmred = 6.13572005515045; break;
                case "bf4SVD12": avgkmyellow = 4.42778809532334; avgkmorange = 5.90371746043112; avgkmred = 6.88767037050297; break;
                case "bf4SW40": avgkmyellow = 6.88004618744444; avgkmorange = 9.17339491659258; avgkmred = 10.702294069358; break;
                case "bf4TAURUS44": avgkmyellow = 5.49815119138021; avgkmorange = 7.33086825517362; avgkmred = 8.55267963103589; break;
                case "bf4TYPE88": avgkmyellow = 4.96152330819858; avgkmorange = 6.61536441093144; avgkmred = 7.71792514608668; break;
                case "bf4TYPE95B1": avgkmyellow = 4.75863918180138; avgkmorange = 6.34485224240184; avgkmred = 7.40232761613548; break;
                case "bf4UCAV": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;
                case "bf4ULTIM": avgkmyellow = 4.06496619170393; avgkmorange = 5.41995492227191; avgkmred = 6.32328074265056; break;
                case "bf4UMP45": avgkmyellow = 5.47831454680301; avgkmorange = 7.30441939573734; avgkmred = 8.52182262836023; break;
                case "bf4UMP9": avgkmyellow = 5.59077524076118; avgkmorange = 7.45436698768158; avgkmred = 8.69676148562851; break;
                case "bf4UN6": avgkmyellow = 6.62289362222351; avgkmorange = 8.83052482963134; avgkmred = 10.3022789679032; break;
                case "bf4USAS12": avgkmyellow = 7.15893186258266; avgkmorange = 9.54524248344354; avgkmred = 11.1361162306841; break;
                case "bf4USAS12NV": avgkmyellow = 7.10787443322771; avgkmorange = 9.47716591097028; avgkmred = 11.0566935627987; break;
                case "bf4UTAS": avgkmyellow = 7.14116283062085; avgkmorange = 9.5215504408278; avgkmred = 11.1084755142991; break;
                case "bf4V40": avgkmyellow = 20.4076245957716; avgkmorange = 27.2101661276955; avgkmred = 31.7451938156448; break;
                case "bf4XM25": avgkmyellow = 4.84379265670434; avgkmorange = 6.45839020893912; avgkmred = 7.53478857709564; break;
                case "bf4XM25_FLECHETTE": avgkmyellow = 11.4744614707241; avgkmorange = 15.2992819609655; avgkmred = 17.8491622877931; break;
                case "bf4XM25_SMK": avgkmyellow = 0; avgkmorange = 0; avgkmred = 0; break;

                default:
                    log("CD - weapon avg kills/min not found: " + weaponname, 1.5);
                    //avgkm = 1.0;
                    avgkmyellow = 100.0; avgkmorange = 100.0; avgkmred = 100.0;
                    break;
            }

            weaponname = FixWeaponName(weaponname);

            double kpm = Math.Round(kills / time, 2);
            double kpmabove = Math.Round((kills / time) / (avgkmyellow/4.5), 2);

            double cheatindex = 0.0;
            if (kills > minimumkills)
            {
                if ((kills / time) > avgkmred)
                {
                    log("CD - IMPOSSIBLE STATS: " + playername + " " + weaponname + " k/m: " + kpm.ToString() + " (" + kpmabove.ToString() + ")", 1);
                    if (!reasons.ContainsKey(kpmabove/3.0))
                        reasons.Add(kpmabove/3.0, weaponname + " k/m: " + kpm.ToString());

                    cheatindex += redflagcount;
                }
                else if ((kills / time) > avgkmorange)
                {
                    log("CD - SUSPICIOUS stats: " + playername + " " + weaponname + " k/m: " + kpm.ToString() + " (" + kpmabove.ToString() + ")", 1);
                    if (!reasons.ContainsKey(kpmabove / 3.0))
                        reasons.Add(kpmabove / 3.0, weaponname + " k/m: " + kpm.ToString());

                    cheatindex += orangeflagcount;
                }
                else if ((kills / time) > avgkmyellow)
                {
                    log("CD - suspicious stats: " + playername + " " + weaponname + " k/m: " + kpm.ToString() + " (" + kpmabove.ToString() + ")", 1);
                    if (!reasons.ContainsKey(kpmabove / 3.0))
                        reasons.Add(kpmabove / 3.0, weaponname + " k/m: " + kpm.ToString());

                    cheatindex += yellowflagcount;
                }
            }
            else
            {
                log("CD - skipping weapon (less than " + minimumkills.ToString() + " kills): " + weaponname, 5);
            }
            log("CD - kills/min: " + kpm.ToString(), 5);
            return cheatindex;
        }
#endregion

        private static string FixWeaponName(string weaponname)
        {
            if (weaponname.Contains("WARSAW_ID_P_XP0_WNAME_"))
            {
                weaponname = weaponname.Replace("WARSAW_ID_P_XP0_WNAME_", "");
            }
            if (weaponname.Contains("WARSAW_ID_P_XP1_WNAME_"))
            {
                weaponname = weaponname.Replace("WARSAW_ID_P_XP1_WNAME_", "");
            }
            if (weaponname.Contains("WARSAW_ID_P_XP2_WNAME_"))
            {
                weaponname = weaponname.Replace("WARSAW_ID_P_XP2_WNAME_", "");
            }
            if (weaponname.Contains("WARSAW_ID_P_XP3_WNAME_"))
            {
                weaponname = weaponname.Replace("WARSAW_ID_P_XP3_WNAME_", "");
            }
            if (weaponname.Contains("WARSAW_ID_P_WNAME_"))
            {
                weaponname = weaponname.Replace("WARSAW_ID_P_WNAME_", "");
            }
            if (weaponname.Contains("WARSAW_ID_P_INAME_"))
            {
                weaponname = weaponname.Replace("WARSAW_ID_P_INAME_", "");
            }
            if (weaponname.Contains("WARSAW_ID_P_XP2_INAME_"))
            {
                weaponname = weaponname.Replace("WARSAW_ID_P_XP2_INAME_", "");
            }
            if (weaponname.Contains("WARSAW_ID_P_XP1_VNAME_"))
            {
                weaponname = weaponname.Replace("WARSAW_ID_P_XP1_VNAME_", "");
            }
            if (weaponname.Contains("WARSAW_ID_P_SP_WNAME_"))
            {
                weaponname = weaponname.Replace("WARSAW_ID_P_SP_WNAME_", "");
            }

            if (weaponname.Contains("WARSAW_ID_P_XP4_INAME_"))
            {
                weaponname = weaponname.Replace("WARSAW_ID_P_XP4_INAME_", "");
            }

            if (weaponname.Contains("WARSAW_ID_P_XP4_WNAME_"))
            {
                weaponname = weaponname.Replace("WARSAW_ID_P_XP4_WNAME_", "");
            }

            return weaponname;
        }    

        #region checkKillsperHit
        public double checkKillsperHit(ref Dictionary<double, string> reasons, string game, string playername, string weaponname, double kills, double hits)
        {
            if (!IsAllowedWeapon(weaponname))
            {
                return 0.0;
            }

            //if (weaponname == "Underslung Shotgun" || 
            //    weaponname == "XP1 Jackhammer" || 
            //    weaponname == "M1014" || 
            //    weaponname == "DAO" || 
            //    weaponname == "XP2 SPAS12" || 
            //    weaponname == "870" || 
            //    weaponname == "USAS" || 
            //    weaponname == "Saiga" || 
            //    weaponname == "RPG-7" || 
            //    weaponname == "Mk153 SMAW" || 
            //    weaponname == "FGM-148 JAVELIN" || 
            //    weaponname == "Underslung Launcher" || 
            //    weaponname == "SA-18 IGLA AA" || 
            //    weaponname == "FIM-92 STINGER AA" || 
            //    weaponname == "Knife") //unreliable
            //    return 0.0;

            //if (weaponname == "Crossbow Scoped" || 
            //    weaponname == "Crossbow kobra") //no avg yet
            //    return 0.0;

            //if (weaponname == "WARSAW_ID_P_INAME_SRAW" ||
            //    weaponname == "WARSAW_ID_P_INAME_M2" ||
            //    weaponname == "WARSAW_ID_P_INAME_V40" ||
            //    weaponname == "WARSAW_ID_P_INAME_MACHETE" ||
            //    weaponname == "WARSAW_ID_P_WNAME_SHORTY" ||
            //    weaponname == "WARSAW_ID_P_INAME_C4" ||
            //    weaponname == "WARSAW_ID_P_WNAME_USAS12" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE4" ||
            //    weaponname == "WARSAW_ID_P_INAME_M32MGL" ||
            //    weaponname == "WARSAW_ID_P_INAME_RPG7" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM" ||
            //    weaponname == "WARSAW_ID_P_INAME_ACB90" ||
            //    weaponname == "WARSAW_ID_P_INAME_STARSTREAK" ||
            //    weaponname == "WARSAW_ID_P_INAME_BAYONETT" ||
            //    weaponname == "WARSAW_ID_P_INAME_M67" ||
            //    weaponname == "WARSAW_ID_P_INAME_MBTLAW" ||
            //    weaponname == "WARSAW_ID_P_SP_WNAME_USAS12NV" ||
            //    weaponname == "WARSAW_ID_P_INAME_FIM92" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE2" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE3" ||
            //    weaponname == "WARSAW_ID_P_INAME_DEFIB" ||
            //    weaponname == "WARSAW_ID_P_WNAME_870" ||
            //    weaponname == "WARSAW_ID_P_INAME_M15" ||
            //    weaponname == "WARSAW_ID_P_INAME_MORTAR" ||
            //    weaponname == "WARSAW_ID_P_XP1_VNAME_UCAV" ||
            //    weaponname == "WARSAW_ID_P_INAME_REPAIR" ||
            //    weaponname == "WARSAW_ID_P_INAME_XM25_SMK" ||
            //    weaponname == "WARSAW_ID_P_WNAME_SAIGA12" ||
            //    weaponname == "WARSAW_ID_P_WNAME_DBV12" ||
            //    weaponname == "WARSAW_ID_P_INAME_XM25" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM_SMK" ||
            //    weaponname == "WARSAW_ID_P_WNAME_M1014" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM_SHG" ||
            //    weaponname == "WARSAW_ID_P_WNAME_SPAS12" ||
            //    weaponname == "WARSAW_ID_P_INAME_XM25_FLECHETTE" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM_LVG" ||
            //    weaponname == "WARSAW_ID_P_INAME_IMPACT" ||
            //    weaponname == "WARSAW_ID_P_INAME_FGM148" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE6" ||
            //    weaponname == "WARSAW_ID_P_INAME_M26_FRAG" ||
            //    weaponname == "WARSAW_ID_P_WNAME_UTAS" ||
            //    weaponname == "WARSAW_ID_P_INAME_M34" ||
            //    weaponname == "WARSAW_ID_P_WNAME_HAWK" ||
            //    weaponname == "WARSAW_ID_P_INAME_M26_MASS" ||
            //    weaponname == "WARSAW_ID_P_INAME_M18" ||
            //    weaponname == "WARSAW_ID_P_INAME_SHANK" ||
            //    weaponname == "WARSAW_ID_P_INAME_IGLA" ||
            //    weaponname == "WARSAW_ID_P_INAME_M26_SLUG" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE5" ||
            //    weaponname == "WARSAW_ID_P_INAME_M26_FLECHETTE" ||
            //    weaponname == "WARSAW_ID_P_INAME_FLASHBANG" ||
            //    weaponname == "WARSAW_ID_P_INAME_CLAYMORE" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM_FLASH" ||
            //    weaponname == "WARSAW_ID_P_INAME_M136" ||
            //    weaponname == "WARSAW_ID_P_INAME_FLARE" ||
            //    weaponname == "WARSAW_ID_P_INAME_SMAW" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE7" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE8" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE1" ||
            //    weaponname == "WARSAW_ID_P_XP0_WNAME_DAO12" || 
            //    weaponname == "WARSAW_ID_P_XP2_INAME_AAMINE" ||
            //    weaponname == "WARSAW_ID_P_INAME_DIVERKNIFE" ||
            //    weaponname == "WARSAW_ID_P_INAME_KNIFE14100BT" ||
            //    weaponname == "WARSAW_ID_P_XP2_INAME_40MM_3GL" ||
            //    weaponname == "WARSAW_ID_P_INAME_KNIFE2142" ||
            //    weaponname == "WARSAW_ID_P_INAME_NECKKNIFE" ||
            //    weaponname == "WARSAW_ID_P_INAME_KNIFEPRECISION")
            //    return 0.0;

            //if (weaponname == "WARSAW_ID_P_INAME_EOD" || //no avg yet
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_BULLDOG" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_DEAGLE" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_MPX" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_SHIELD" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_UN6" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_CS5")
            //    return 0.0;

            //double avgkh;
            double avgkhyellow;
            double avgkhorange;
            double avgkhred;


            switch (game.ToLower() + weaponname)
            {
                case "bf3870": avgkhyellow = 0.637525687771253; avgkhorange = 0.765030825325503; avgkhred = 1.020041100434; break;
                case "bf3A91": avgkhyellow = 0.406945309301017; avgkhorange = 0.488334371161221; avgkhred = 0.651112494881628; break;
                case "bf3AEK971": avgkhyellow = 0.413139603784105; avgkhorange = 0.495767524540926; avgkhred = 0.661023366054568; break;
                case "bf3AK74M": avgkhyellow = 0.403802602951148; avgkhorange = 0.484563123541377; avgkhred = 0.646084164721836; break;
                case "bf3AKS74U": avgkhyellow = 0.39850643023334; avgkhorange = 0.478207716280008; avgkhred = 0.637610288373344; break;
                case "bf3AN94": avgkhyellow = 0.408061265365083; avgkhorange = 0.489673518438099; avgkhred = 0.652898024584132; break;
                case "bf3AS-VAL": avgkhyellow = 0.352016569524657; avgkhorange = 0.422419883429589; avgkhred = 0.563226511239452; break;
                case "bf3Crossbow kobra": avgkhyellow = 1.40825873690104; avgkhorange = 1.68991048428124; avgkhred = 2.25321397904166; break;
                case "bf3Crossbow Scoped": avgkhyellow = 1.28079528528832; avgkhorange = 1.53695434234599; avgkhred = 2.04927245646132; break;
                case "bf3DAO": avgkhyellow = 0.471352150248575; avgkhorange = 0.56562258029829; avgkhred = 0.75416344039772; break;
                case "bf3F2000": avgkhyellow = 0.431604341263013; avgkhorange = 0.517925209515615; avgkhred = 0.69056694602082; break;
                case "bf3FGM-148 JAVELIN": avgkhyellow = 0.51578897309901; avgkhorange = 0.618946767718812; avgkhred = 0.825262356958416; break;
                case "bf3FIM-92 STINGER AA": avgkhyellow = 0.654205222892753; avgkhorange = 0.785046267471303; avgkhred = 1.0467283566284; break;
                case "bf3G36C": avgkhyellow = 0.40553855185855; avgkhorange = 0.48664626223026; avgkhred = 0.64886168297368; break;
                case "bf3G3A4": avgkhyellow = 0.563585394772215; avgkhorange = 0.676302473726658; avgkhred = 0.901736631635544; break;
                case "bf3Glock 17": avgkhyellow = 0.48028311120069; avgkhorange = 0.576339733440828; avgkhred = 0.768452977921104; break;
                case "bf3Glock 17 Silenced": avgkhyellow = 0.4712113007426; avgkhorange = 0.56545356089112; avgkhred = 0.75393808118816; break;
                case "bf3Glock 18": avgkhyellow = 0.393941877091742; avgkhorange = 0.472730252510091; avgkhred = 0.630307003346788; break;
                case "bf3Glock 18 Silenced": avgkhyellow = 0.384307466301818; avgkhorange = 0.461168959562181; avgkhred = 0.614891946082908; break;
                case "bf3KH2002": avgkhyellow = 0.419466669943565; avgkhorange = 0.503360003932278; avgkhred = 0.671146671909704; break;
                case "bf3Knife": avgkhyellow = 1.37346510911604; avgkhorange = 1.64815813093924; avgkhred = 2.19754417458566; break;
                case "bf3M1014": avgkhyellow = 0.490662160805915; avgkhorange = 0.588794592967098; avgkhred = 0.785059457289464; break;
                case "bf3M16A4": avgkhyellow = 0.40636821623621; avgkhorange = 0.487641859483452; avgkhred = 0.650189145977936; break;
                case "bf3M1911": avgkhyellow = 0.626544687707357; avgkhorange = 0.751853625248829; avgkhred = 1.00247150033177; break;
                case "bf3M1911 LIT": avgkhyellow = 0.612678809031537; avgkhorange = 0.735214570837845; avgkhred = 0.98028609445046; break;
                case "bf3M1911 SILENCED": avgkhyellow = 0.592555942627055; avgkhorange = 0.711067131152466; avgkhred = 0.948089508203288; break;
                case "bf3M1911 Tactical": avgkhyellow = 0.570204483227282; avgkhorange = 0.684245379872739; avgkhred = 0.912327173163652; break;
                case "bf3M240": avgkhyellow = 0.523271324728695; avgkhorange = 0.627925589674434; avgkhred = 0.837234119565912; break;
                case "bf3M249": avgkhyellow = 0.404083199392162; avgkhorange = 0.484899839270595; avgkhred = 0.64653311902746; break;
                case "bf3M27": avgkhyellow = 0.412477931070978; avgkhorange = 0.494973517285173; avgkhred = 0.659964689713564; break;
                case "bf3M39": avgkhyellow = 0.794571703938658; avgkhorange = 0.953486044726389; avgkhred = 1.27131472630185; break;
                case "bf3M40A5": avgkhyellow = 1.27285593979389; avgkhorange = 1.52742712775266; avgkhred = 2.03656950367022; break;
                case "bf3M412 Rex": avgkhyellow = 0.897032800688872; avgkhorange = 1.07643936082665; avgkhred = 1.4352524811022; break;
                case "bf3M416": avgkhyellow = 0.412137553907298; avgkhorange = 0.494565064688757; avgkhred = 0.659420086251676; break;
                case "bf3M4A1": avgkhyellow = 0.391654025457478; avgkhorange = 0.469984830548973; avgkhred = 0.626646440731964; break;
                case "bf3M60": avgkhyellow = 0.512747636837645; avgkhorange = 0.615297164205174; avgkhred = 0.820396218940232; break;
                case "bf3M9": avgkhyellow = 0.47786489510448; avgkhorange = 0.573437874125376; avgkhred = 0.764583832167168; break;
                case "bf3M9 Flashlight": avgkhyellow = 0.46919574500811; avgkhorange = 0.563034894009732; avgkhred = 0.750713192012976; break;
                case "bf3M9 Silenced": avgkhyellow = 0.451680256732098; avgkhorange = 0.542016308078517; avgkhred = 0.722688410771356; break;
                case "bf3M93R": avgkhyellow = 0.397206216457808; avgkhorange = 0.476647459749369; avgkhred = 0.635529946332492; break;
                case "bf3M98B": avgkhyellow = 1.25471410791687; avgkhorange = 1.50565692950024; avgkhred = 2.00754257266698; break;
                case "bf3MK11": avgkhyellow = 0.79108458972107; avgkhorange = 0.949301507665284; avgkhred = 1.26573534355371; break;
                case "bf3Mk153 SMAW": avgkhyellow = 0.470277293880128; avgkhorange = 0.564332752656153; avgkhred = 0.752443670208204; break;
                case "bf3MP 443": avgkhyellow = 0.461414377864345; avgkhorange = 0.553697253437214; avgkhred = 0.738263004582952; break;
                case "bf3MP443 LIT": avgkhyellow = 0.46727183431852; avgkhorange = 0.560726201182224; avgkhred = 0.747634934909632; break;
                case "bf3MP443 Silenced": avgkhyellow = 0.42693261833014; avgkhorange = 0.512319141996168; avgkhred = 0.683092189328224; break;
                case "bf3MP7": avgkhyellow = 0.323275305712283; avgkhorange = 0.387930366854739; avgkhred = 0.517240489139652; break;
                case "bf3P90": avgkhyellow = 0.329592830183782; avgkhorange = 0.395511396220539; avgkhred = 0.527348528294052; break;
                case "bf3PDR": avgkhyellow = 0.402626917193175; avgkhorange = 0.48315230063181; avgkhred = 0.64420306750908; break;
                case "bf3Pecheneg": avgkhyellow = 0.534583007686013; avgkhorange = 0.641499609223215; avgkhred = 0.85533281229762; break;
                case "bf3PP2000": avgkhyellow = 0.393723104741223; avgkhorange = 0.472467725689467; avgkhred = 0.629956967585956; break;
                case "bf3RPG-7": avgkhyellow = 0.470377565170438; avgkhorange = 0.564453078204525; avgkhred = 0.7526041042727; break;
                case "bf3RPK": avgkhyellow = 0.408447175211512; avgkhorange = 0.490136610253815; avgkhred = 0.65351548033842; break;
                case "bf3SA-18 IGLA AA": avgkhyellow = 0.60494538614718; avgkhorange = 0.725934463376616; avgkhred = 0.967912617835488; break;
                case "bf3Saiga": avgkhyellow = 0.469155611982785; avgkhorange = 0.562986734379342; avgkhred = 0.750648979172456; break;
                case "bf3SCAR": avgkhyellow = 0.4853460877971; avgkhorange = 0.58241530535652; avgkhred = 0.77655374047536; break;
                case "bf3SG553": avgkhyellow = 0.408867643148685; avgkhorange = 0.490641171778422; avgkhred = 0.654188229037896; break;
                case "bf3SKS": avgkhyellow = 0.65100683262866; avgkhorange = 0.781208199154392; avgkhred = 1.04161093220586; break;
                case "bf3SV98": avgkhyellow = 1.15230641109812; avgkhorange = 1.38276769331774; avgkhred = 1.84369025775699; break;
                case "bf3SVD": avgkhyellow = 0.80663184827341; avgkhorange = 0.967958217928092; avgkhred = 1.29061095723746; break;
                case "bf3Taurus 44": avgkhyellow = 0.965850192408892; avgkhorange = 1.15902023089067; avgkhred = 1.54536030785423; break;
                case "bf3Taurus 44 scoped": avgkhyellow = 0.90507280838056; avgkhorange = 1.08608737005667; avgkhred = 1.4481164934089; break;
                case "bf3Type88": avgkhyellow = 0.387483764241132; avgkhorange = 0.464980517089359; avgkhred = 0.619974022785812; break;
                case "bf3UMP": avgkhyellow = 0.455483329602237; avgkhorange = 0.546579995522685; avgkhred = 0.72877332736358; break;
                case "bf3Underslung Launcher": avgkhyellow = 0.62915173524902; avgkhorange = 0.754982082298824; avgkhred = 1.00664277639843; break;
                case "bf3Underslung Shotgun": avgkhyellow = 0.662778909916928; avgkhorange = 0.795334691900313; avgkhred = 1.06044625586708; break;
                case "bf3USAS": avgkhyellow = 0.448896758876685; avgkhorange = 0.538676110652022; avgkhred = 0.718234814202696; break;
                case "bf3XP1 FAMAS": avgkhyellow = 0.437221926404052; avgkhorange = 0.524666311684863; avgkhred = 0.699555082246484; break;
                case "bf3XP1 HK53": avgkhyellow = 0.40921329294593; avgkhorange = 0.491055951535116; avgkhred = 0.654741268713488; break;
                case "bf3XP1 Jackhammer": avgkhyellow = 0.46970899868002; avgkhorange = 0.563650798416024; avgkhred = 0.751534397888032; break;
                case "bf3XP1 L85A2": avgkhyellow = 0.423325723247942; avgkhorange = 0.507990867897531; avgkhred = 0.677321157196708; break;
                case "bf3XP1 L96": avgkhyellow = 1.23785065898794; avgkhorange = 1.48542079078552; avgkhred = 1.9805610543807; break;
                case "bf3XP1 MG36": avgkhyellow = 0.42217063192683; avgkhorange = 0.506604758312196; avgkhred = 0.675473011082928; break;
                case "bf3XP1 PP19": avgkhyellow = 0.290210198581718; avgkhorange = 0.348252238298061; avgkhred = 0.464336317730748; break;
                case "bf3XP1 QBB95": avgkhyellow = 0.42061792485612; avgkhorange = 0.504741509827344; avgkhred = 0.672988679769792; break;
                case "bf3XP1 QBU88": avgkhyellow = 0.8565964922737; avgkhorange = 1.02791579072844; avgkhred = 1.37055438763792; break;
                case "bf3XP1 QBZ95B": avgkhyellow = 0.405578232866338; avgkhorange = 0.486693879439605; avgkhred = 0.64892517258614; break;
                case "bf3XP2 ACR": avgkhyellow = 0.3374783153652; avgkhorange = 0.40497397843824; avgkhred = 0.53996530458432; break;
                case "bf3XP2 AUG": avgkhyellow = 0.4146110595102; avgkhorange = 0.49753327141224; avgkhred = 0.66337769521632; break;
                case "bf3XP2 HK417": avgkhyellow = 0.836450095663118; avgkhorange = 1.00374011479574; avgkhred = 1.33832015306099; break;
                case "bf3XP2 JNG90": avgkhyellow = 1.26329547870421; avgkhorange = 1.51595457444506; avgkhred = 2.02127276592674; break;
                case "bf3XP2 L86A1": avgkhyellow = 0.412697313416092; avgkhorange = 0.495236776099311; avgkhred = 0.660315701465748; break;
                case "bf3XP2 LSAT": avgkhyellow = 0.40855122807353; avgkhorange = 0.490261473688236; avgkhred = 0.653681964917648; break;
                case "bf3XP2 MP5K": avgkhyellow = 0.409421411805168; avgkhorange = 0.491305694166201; avgkhred = 0.655074258888268; break;
                case "bf3XP2 MTAR-21": avgkhyellow = 0.41892841028288; avgkhorange = 0.502714092339456; avgkhred = 0.670285456452608; break;
                case "bf3XP2 SCARL": avgkhyellow = 0.416221610424905; avgkhorange = 0.499465932509886; avgkhred = 0.665954576679848; break;
                case "bf3XP2 SPAS12": avgkhyellow = 0.66665514906589; avgkhorange = 0.799986178879068; avgkhred = 1.06664823850542; break;
                case "bf440MM": avgkhyellow = 0.818124854475898; avgkhorange = 0.981749825371077; avgkhred = 1.30899976716144; break;
                case "bf440MM_3GL": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf440MM_FLASH": avgkhyellow = 0.663026418858782; avgkhorange = 0.795631702630539; avgkhred = 1.06084227017405; break;
                case "bf440MM_LVG": avgkhyellow = 0.610773088903457; avgkhorange = 0.732927706684149; avgkhred = 0.977236942245532; break;
                case "bf440MM_SHG": avgkhyellow = 1.05540049102022; avgkhorange = 1.26648058922426; avgkhred = 1.68864078563235; break;
                case "bf440MM_SMK": avgkhyellow = 1.82554183467742; avgkhorange = 2.1906502016129; avgkhred = 2.92086693548387; break;
                case "bf4870": avgkhyellow = 0.686036769335775; avgkhorange = 0.82324412320293; avgkhred = 1.09765883093724; break;
                case "bf493R": avgkhyellow = 0.36239629358437; avgkhorange = 0.434875552301244; avgkhred = 0.579834069734992; break;
                case "bf4A91": avgkhyellow = 0.43804861491752; avgkhorange = 0.525658337901024; avgkhred = 0.700877783868032; break;
                case "bf4AAMINE": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4ACB90": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4ACR": avgkhyellow = 0.440552226631925; avgkhorange = 0.52866267195831; avgkhred = 0.70488356261108; break;
                case "bf4AEK971": avgkhyellow = 0.45144678711674; avgkhorange = 0.541736144540088; avgkhred = 0.722314859386784; break;
                case "bf4AK12": avgkhyellow = 0.42774441490965; avgkhorange = 0.51329329789158; avgkhred = 0.68439106385544; break;
                case "bf4AK5C": avgkhyellow = 0.41639652862553; avgkhorange = 0.499675834350636; avgkhred = 0.666234445800848; break;
                case "bf4AKU12": avgkhyellow = 0.426618253496095; avgkhorange = 0.511941904195314; avgkhred = 0.682589205593752; break;
                case "bf4AMR2": avgkhyellow = 2.03772319375879; avgkhorange = 2.44526783251054; avgkhred = 3.26035711001406; break;
                case "bf4AMR2CQB": avgkhyellow = 1.76370660868214; avgkhorange = 2.11644793041857; avgkhred = 2.82193057389143; break;
                case "bf4AMR2MID": avgkhyellow = 2.0105063682978; avgkhorange = 2.41260764195736; avgkhred = 3.21681018927648; break;
                case "bf4AR160": avgkhyellow = 0.457061124795972; avgkhorange = 0.548473349755167; avgkhred = 0.731297799673556; break;
                case "bf4ASVAL": avgkhyellow = 0.538170209938613; avgkhorange = 0.645804251926335; avgkhred = 0.86107233590178; break;
                case "bf4AWS": avgkhyellow = 0.432583756540108; avgkhorange = 0.519100507848129; avgkhred = 0.692134010464172; break;
                case "bf4BAYONETT": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4BPKNIFE1": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4BPKNIFE2": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4BPKNIFE3": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4BPKNIFE4": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4BPKNIFE5": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4BPKNIFE6": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4BPKNIFE7": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4BPKNIFE8": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4BULLDOG": avgkhyellow = 0.593419526584123; avgkhorange = 0.712103431900947; avgkhred = 0.949471242534596; break;
                case "bf4C4": avgkhyellow = 1.04046097110362; avgkhorange = 1.24855316532434; avgkhred = 1.66473755376579; break;
                case "bf4CBJMS": avgkhyellow = 0.431716869471505; avgkhorange = 0.518060243365806; avgkhred = 0.690746991154408; break;
                case "bf4CLAYMORE": avgkhyellow = 1.53896897758032; avgkhorange = 1.84676277309639; avgkhred = 2.46235036412852; break;
                case "bf4CS5": avgkhyellow = 1.54670253602446; avgkhorange = 1.85604304322935; avgkhred = 2.47472405763913; break;
                case "bf4CSLR4": avgkhyellow = 1.32234274219422; avgkhorange = 1.58681129063307; avgkhred = 2.11574838751076; break;
                case "bf4CZ75": avgkhyellow = 0.545099111978935; avgkhorange = 0.654118934374722; avgkhred = 0.872158579166296; break;
                case "bf4CZ805": avgkhyellow = 0.444386749775915; avgkhorange = 0.533264099731098; avgkhred = 0.711018799641464; break;
                case "bf4DAO12": avgkhyellow = 0.567720869689147; avgkhorange = 0.681265043626977; avgkhred = 0.908353391502636; break;
                case "bf4DBV12": avgkhyellow = 0.544365459178357; avgkhorange = 0.653238551014029; avgkhred = 0.870984734685372; break;
                case "bf4DEAGLE": avgkhyellow = 0.896778905523978; avgkhorange = 1.07613468662877; avgkhred = 1.43484624883836; break;
                case "bf4DEFIB": avgkhyellow = 2.08694642099092; avgkhorange = 2.5043357051891; avgkhred = 3.33911427358546; break;
                case "bf4DIVERKNIFE": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4EOD": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4F2000": avgkhyellow = 0.432819279939005; avgkhorange = 0.519383135926806; avgkhred = 0.692510847902408; break;
                case "bf4FAMAS": avgkhyellow = 0.454847225681965; avgkhorange = 0.545816670818358; avgkhred = 0.727755561091144; break;
                case "bf4FGM148": avgkhyellow = 1.06629358050121; avgkhorange = 1.27955229660145; avgkhred = 1.70606972880193; break;
                case "bf4FIM92": avgkhyellow = 1.19069682339478; avgkhorange = 1.42883618807374; avgkhred = 1.90511491743165; break;
                case "bf4FLARE": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4FLASHBANG": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4FN57": avgkhyellow = 0.404509637573452; avgkhorange = 0.485411565088143; avgkhred = 0.647215420117524; break;
                case "bf4FYJS": avgkhyellow = 1.32871537418617; avgkhorange = 1.59445844902341; avgkhred = 2.12594459869788; break;
                case "bf4G36C": avgkhyellow = 0.42995522409671; avgkhorange = 0.515946268916052; avgkhred = 0.687928358554736; break;
                case "bf4GALIL21": avgkhyellow = 0.437277543873645; avgkhorange = 0.524733052648374; avgkhred = 0.699644070197832; break;
                case "bf4GALIL23": avgkhyellow = 0.437505135888458; avgkhorange = 0.525006163066149; avgkhred = 0.700008217421532; break;
                case "bf4GALIL52": avgkhyellow = 0.577670951761695; avgkhorange = 0.693205142114034; avgkhred = 0.924273522818712; break;
                case "bf4GALIL53": avgkhyellow = 0.78613844459952; avgkhorange = 0.943366133519424; avgkhred = 1.25782151135923; break;
                case "bf4GLOCK18": avgkhyellow = 0.371116334346998; avgkhorange = 0.445339601216397; avgkhred = 0.593786134955196; break;
                case "bf4GOL": avgkhyellow = 1.46502876502285; avgkhorange = 1.75803451802742; avgkhred = 2.34404602403656; break;
                case "bf4HAWK": avgkhyellow = 0.623045220101393; avgkhorange = 0.747654264121671; avgkhred = 0.996872352162228; break;
                case "bf4HK45C": avgkhyellow = 0.613225404409642; avgkhorange = 0.735870485291571; avgkhred = 0.981160647055428; break;
                case "bf4IGLA": avgkhyellow = 1.73757359040654; avgkhorange = 2.08508830848784; avgkhred = 2.78011774465046; break;
                case "bf4IMPACT": avgkhyellow = 0.675989513657142; avgkhorange = 0.811187416388571; avgkhred = 1.08158322185143; break;
                case "bf4JNG90": avgkhyellow = 1.39713763027708; avgkhorange = 1.67656515633249; avgkhred = 2.23542020844332; break;
                case "bf4JS2": avgkhyellow = 0.373727872106473; avgkhorange = 0.448473446527767; avgkhred = 0.597964595370356; break;
                case "bf4KNIFE14100BT": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4KNIFE2142": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4KNIFEPRECISION": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4L85A2": avgkhyellow = 0.445936423184578; avgkhorange = 0.535123707821493; avgkhred = 0.713498277095324; break;
                case "bf4L96A1": avgkhyellow = 1.37676360893949; avgkhorange = 1.65211633072739; avgkhred = 2.20282177430318; break;
                case "bf4LSAT": avgkhyellow = 0.437875721294755; avgkhorange = 0.525450865553706; avgkhred = 0.700601154071608; break;
                case "bf4M1014": avgkhyellow = 0.55739084570747; avgkhorange = 0.668869014848964; avgkhred = 0.891825353131952; break;
                case "bf4M136": avgkhyellow = 1.20614035087719; avgkhorange = 1.44736842105263; avgkhred = 1.92982456140351; break;
                case "bf4M15": avgkhyellow = 1.22590290317778; avgkhorange = 1.47108348381334; avgkhred = 1.96144464508445; break;
                case "bf4M16A4": avgkhyellow = 0.445020888701388; avgkhorange = 0.534025066441665; avgkhred = 0.71203342192222; break;
                case "bf4M18": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4M1911": avgkhyellow = 0.611224540516403; avgkhorange = 0.733469448619683; avgkhred = 0.977959264826244; break;
                case "bf4M2": avgkhyellow = 1.05495903929432; avgkhorange = 1.26595084715319; avgkhred = 1.68793446287092; break;
                case "bf4M200": avgkhyellow = 1.43357752403697; avgkhorange = 1.72029302884436; avgkhred = 2.29372403845915; break;
                case "bf4M240": avgkhyellow = 0.615762157969405; avgkhorange = 0.738914589563286; avgkhred = 0.985219452751048; break;
                case "bf4M249": avgkhyellow = 0.435111225577068; avgkhorange = 0.522133470692481; avgkhred = 0.696177960923308; break;
                case "bf4M26_FLECHETTE": avgkhyellow = 0.689874739690167; avgkhorange = 0.827849687628201; avgkhred = 1.10379958350427; break;
                case "bf4M26_FRAG": avgkhyellow = 0.319097428493033; avgkhorange = 0.382916914191639; avgkhred = 0.510555885588852; break;
                case "bf4M26_MASS": avgkhyellow = 0.758784787107453; avgkhorange = 0.910541744528943; avgkhred = 1.21405565937192; break;
                case "bf4M26_SLUG": avgkhyellow = 1.57550085251492; avgkhorange = 1.8906010230179; avgkhred = 2.52080136402387; break;
                case "bf4M32MGL": avgkhyellow = 0.982551495028995; avgkhorange = 1.17906179403479; avgkhred = 1.57208239204639; break;
                case "bf4M34": avgkhyellow = 0.0682517004141653; avgkhorange = 0.0819020404969983; avgkhred = 0.109202720662664; break;
                case "bf4M39": avgkhyellow = 0.802833358599655; avgkhorange = 0.963400030319586; avgkhred = 1.28453337375945; break;
                case "bf4M40A5": avgkhyellow = 1.35866215715984; avgkhorange = 1.63039458859181; avgkhred = 2.17385945145575; break;
                case "bf4M412REX": avgkhyellow = 0.88612140213402; avgkhorange = 1.06334568256082; avgkhred = 1.41779424341443; break;
                case "bf4M416": avgkhyellow = 0.444464953112155; avgkhorange = 0.533357943734586; avgkhred = 0.711143924979448; break;
                case "bf4M4A1": avgkhyellow = 0.434899397689043; avgkhorange = 0.521879277226851; avgkhred = 0.695839036302468; break;
                case "bf4M60E4": avgkhyellow = 0.600352457281387; avgkhorange = 0.720422948737665; avgkhred = 0.96056393165022; break;
                case "bf4M67": avgkhyellow = 0.737309562829192; avgkhorange = 0.884771475395031; avgkhred = 1.17969530052671; break;
                case "bf4M82A3": avgkhyellow = 2.0125786163522; avgkhorange = 2.41509433962264; avgkhred = 3.22012578616352; break;
                case "bf4M82A3CQB": avgkhyellow = 2.0859178483487; avgkhorange = 2.50310141801844; avgkhred = 3.33746855735792; break;
                case "bf4M82A3MID": avgkhyellow = 1.97662539557867; avgkhorange = 2.3719504746944; avgkhred = 3.16260063292586; break;
                case "bf4M9": avgkhyellow = 0.520559809531595; avgkhorange = 0.624671771437914; avgkhred = 0.832895695250552; break;
                case "bf4M98B": avgkhyellow = 1.47264101345505; avgkhorange = 1.76716921614606; avgkhred = 2.35622562152808; break;
                case "bf4MACHETE": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4MBTLAW": avgkhyellow = 0.747712022965095; avgkhorange = 0.897254427558114; avgkhred = 1.19633923674415; break;
                case "bf4MG4": avgkhyellow = 0.43626585183092; avgkhorange = 0.523519022197104; avgkhred = 0.698025362929472; break;
                case "bf4MK11": avgkhyellow = 0.778278736699495; avgkhorange = 0.933934484039394; avgkhred = 1.24524597871919; break;
                case "bf4MORTAR": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4MP443": avgkhyellow = 0.519952505650633; avgkhorange = 0.623943006780759; avgkhred = 0.831924009041012; break;
                case "bf4MP7": avgkhyellow = 0.372928633846815; avgkhorange = 0.447514360616178; avgkhred = 0.596685814154904; break;
                case "bf4MPX": avgkhyellow = 0.47452642132101; avgkhorange = 0.569431705585212; avgkhred = 0.759242274113616; break;
                case "bf4MTAR21": avgkhyellow = 0.45848410229945; avgkhorange = 0.55018092275934; avgkhred = 0.73357456367912; break;
                case "bf4MX4": avgkhyellow = 0.430314103060103; avgkhorange = 0.516376923672123; avgkhred = 0.688502564896164; break;
                case "bf4NECKKNIFE": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4P226": avgkhyellow = 0.521460291399087; avgkhorange = 0.625752349678905; avgkhred = 0.83433646623854; break;
                case "bf4P90": avgkhyellow = 0.400439193691138; avgkhorange = 0.480527032429365; avgkhred = 0.64070270990582; break;
                case "bf4PDR": avgkhyellow = 0.441394122358212; avgkhorange = 0.529672946829855; avgkhred = 0.70623059577314; break;
                case "bf4PECHENEG": avgkhyellow = 0.608276273523617; avgkhorange = 0.729931528228341; avgkhred = 0.973242037637788; break;
                case "bf4PP2000": avgkhyellow = 0.415608624619608; avgkhorange = 0.498730349543529; avgkhred = 0.664973799391372; break;
                case "bf4QBB95": avgkhyellow = 0.449403619122767; avgkhorange = 0.539284342947321; avgkhred = 0.719045790596428; break;
                case "bf4QBS09": avgkhyellow = 0.493246592224365; avgkhorange = 0.591895910669238; avgkhred = 0.789194547558984; break;
                case "bf4QBU88": avgkhyellow = 0.73311894973698; avgkhorange = 0.879742739684376; avgkhred = 1.17299031957917; break;
                case "bf4QBZ951": avgkhyellow = 0.443233155724732; avgkhorange = 0.531879786869679; avgkhred = 0.709173049159572; break;
                case "bf4QSZ92": avgkhyellow = 0.41567762703343; avgkhorange = 0.498813152440116; avgkhred = 0.665084203253488; break;
                case "bf4REPAIR": avgkhyellow = 0.00274206902491738; avgkhorange = 0.00329048282990085; avgkhred = 0.0043873104398678; break;
                case "bf4RFBTARGET": avgkhyellow = 0.765424072796045; avgkhorange = 0.918508887355254; avgkhred = 1.22467851647367; break;
                case "bf4RPG7": avgkhyellow = 0.911677004470027; avgkhorange = 1.09401240536403; avgkhred = 1.45868320715204; break;
                case "bf4RPK12": avgkhyellow = 0.44391166216665; avgkhorange = 0.53269399459998; avgkhred = 0.71025865946664; break;
                case "bf4RPK74": avgkhyellow = 0.455477384686278; avgkhorange = 0.546572861623533; avgkhred = 0.728763815498044; break;
                case "bf4SAIGA12": avgkhyellow = 0.551477438104688; avgkhorange = 0.661772925725625; avgkhred = 0.8823639009675; break;
                case "bf4SAR21": avgkhyellow = 0.435740582067735; avgkhorange = 0.522888698481282; avgkhred = 0.697184931308376; break;
                case "bf4SCARH": avgkhyellow = 0.59636673560066; avgkhorange = 0.715640082720792; avgkhred = 0.954186776961056; break;
                case "bf4SCARHSV": avgkhyellow = 0.812230090185855; avgkhorange = 0.974676108223026; avgkhred = 1.29956814429737; break;
                case "bf4SCORP": avgkhyellow = 0.448029222912313; avgkhorange = 0.537635067494775; avgkhred = 0.7168467566597; break;
                case "bf4SCOUTELIT": avgkhyellow = 1.33529123653888; avgkhorange = 1.60234948384666; avgkhred = 2.13646597846221; break;
                case "bf4SG553": avgkhyellow = 0.44064721483787; avgkhorange = 0.528776657805444; avgkhred = 0.705035543740592; break;
                case "bf4SHANK": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4SHIELD": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4SHORTY": avgkhyellow = 0.728075896347235; avgkhorange = 0.873691075616682; avgkhred = 1.16492143415558; break;
                case "bf4SKS": avgkhyellow = 0.71759959863442; avgkhorange = 0.861119518361304; avgkhred = 1.14815935781507; break;
                case "bf4SMAW": avgkhyellow = 0.938359405446467; avgkhorange = 1.12603128653576; avgkhred = 1.50137504871435; break;
                case "bf4SPAS12": avgkhyellow = 0.701332667421725; avgkhorange = 0.84159920090607; avgkhred = 1.12213226787476; break;
                case "bf4SR2": avgkhyellow = 0.465246488374737; avgkhorange = 0.558295786049685; avgkhred = 0.74439438139958; break;
                case "bf4SR338": avgkhyellow = 0.849452233193962; avgkhorange = 1.01934267983275; avgkhred = 1.35912357311034; break;
                case "bf4SRAW": avgkhyellow = 0.993266953463963; avgkhorange = 1.19192034415676; avgkhred = 1.58922712554234; break;
                case "bf4SRS": avgkhyellow = 1.54038275925648; avgkhorange = 1.84845931110778; avgkhred = 2.46461241481038; break;
                case "bf4STARSTREAK": avgkhyellow = 1.07781199594132; avgkhorange = 1.29337439512958; avgkhred = 1.72449919350611; break;
                case "bf4STEYRAUG": avgkhyellow = 0.446809197495728; avgkhorange = 0.536171036994873; avgkhred = 0.714894715993164; break;
                case "bf4SV98": avgkhyellow = 1.43569968104515; avgkhorange = 1.72283961725418; avgkhred = 2.29711948967224; break;
                case "bf4SVD12": avgkhyellow = 0.780410513413825; avgkhorange = 0.93649261609659; avgkhred = 1.24865682146212; break;
                case "bf4SW40": avgkhyellow = 0.918856154945345; avgkhorange = 1.10262738593441; avgkhred = 1.47016984791255; break;
                case "bf4TAURUS44": avgkhyellow = 0.909444835385948; avgkhorange = 1.09133380246314; avgkhred = 1.45511173661752; break;
                case "bf4TYPE88": avgkhyellow = 0.432447515329282; avgkhorange = 0.518937018395139; avgkhred = 0.691916024526852; break;
                case "bf4TYPE95B1": avgkhyellow = 0.435871226247725; avgkhorange = 0.52304547149727; avgkhred = 0.69739396199636; break;
                case "bf4UCAV": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;
                case "bf4ULTIM": avgkhyellow = 0.429396057448918; avgkhorange = 0.515275268938701; avgkhred = 0.687033691918268; break;
                case "bf4UMP45": avgkhyellow = 0.534378732352843; avgkhorange = 0.641254478823411; avgkhred = 0.855005971764548; break;
                case "bf4UMP9": avgkhyellow = 0.428411018117022; avgkhorange = 0.514093221740427; avgkhred = 0.685457628987236; break;
                case "bf4UN6": avgkhyellow = 0.916816117885872; avgkhorange = 1.10017934146305; avgkhred = 1.4669057886174; break;
                case "bf4USAS12": avgkhyellow = 0.423299058974625; avgkhorange = 0.50795887076955; avgkhred = 0.6772784943594; break;
                case "bf4USAS12NV": avgkhyellow = 0.384456619461813; avgkhorange = 0.461347943354175; avgkhred = 0.6151305911389; break;
                case "bf4UTAS": avgkhyellow = 0.907370109072347; avgkhorange = 1.08884413088682; avgkhred = 1.45179217451576; break;
                case "bf4V40": avgkhyellow = 0.645361227557855; avgkhorange = 0.774433473069426; avgkhred = 1.03257796409257; break;
                case "bf4XM25": avgkhyellow = 0.445966010502617; avgkhorange = 0.535159212603141; avgkhred = 0.713545616804188; break;
                case "bf4XM25_FLECHETTE": avgkhyellow = 0.65361598224858; avgkhorange = 0.784339178698296; avgkhred = 1.04578557159773; break;
                case "bf4XM25_SMK": avgkhyellow = 0; avgkhorange = 0; avgkhred = 0; break;

                default:
                    log("CD - weapon avg kills/hit not found: " + weaponname, 1.5);
                    //avgkh = 1.0;
                    avgkhyellow = 100.0; avgkhorange = 100.0; avgkhred = 100.0;
                    break;
            }

            weaponname = FixWeaponName(weaponname);

            double kph = Math.Round((kills / hits) * 100.0, 2);
            double kphabove = Math.Round((kills / hits) / (avgkhyellow/2.5), 2);
                        
            double cheatindex = 0.0;
            if (kills > minimumkills)
            {                
                if ((kills / hits) > avgkhred)
                {
                    log("CD - IMPOSSIBLE STATS: " + playername + " " + weaponname + " k/h: " + kph.ToString() + "%" + " (" + kphabove.ToString() + ")", 1);
                    if (!reasons.ContainsKey(kphabove))
                        reasons.Add(kphabove, weaponname + " k/h: " + kph.ToString() + "%");

                    cheatindex += redflagcount;
                }
                else if ((kills / hits) > avgkhorange)
                {
                    log("CD - SUSPICIOUS stats: " + playername + " " + weaponname + " k/h: " + kph.ToString() + "%" + " (" + kphabove.ToString() + ")", 1);
                    if (!reasons.ContainsKey(kphabove))
                        reasons.Add(kphabove, weaponname + " k/h: " + kph.ToString() + "%");

                    cheatindex += orangeflagcount;
                }
                else if ((kills / hits) > avgkhyellow)
                {
                    log("CD - suspicious stats: " + playername + " " + weaponname + " k/h: " + kph.ToString() + "%" + " (" + kphabove.ToString() + ")", 1);
                    if (!reasons.ContainsKey(kphabove))
                        reasons.Add(kphabove, weaponname + " k/h: " + kph.ToString() + "%");

                    cheatindex += yellowflagcount;
                }
            }
            else
            {
                log("CD - skipping weapon (less than " + minimumkills.ToString() + " kills): " + weaponname, 5);
            }
            log("CD - kills/hit: " + kph.ToString() + "%", 5);
            return cheatindex;
        }
        #endregion

        #region checkHeadshotsperKill
        public double checkHeadshotsperKill(ref Dictionary<double, string> reasons, string game, string playername, string weaponname, double headshots, double kills)
        {
            if (headshots > kills)
            {
                headshots = kills;
            }

            if (!IsAllowedWeapon(weaponname))
            {
                return 0.0;
            }

            //if (weaponname == "Underslung Shotgun" || 
            //    weaponname == "XP1 Jackhammer" || 
            //    weaponname == "M1014" || 
            //    weaponname == "DAO" || 
            //    weaponname == "XP2 SPAS12" || 
            //    weaponname == "870" || 
            //    weaponname == "USAS" || 
            //    weaponname == "Saiga" || 
            //    weaponname == "RPG-7" || 
            //    weaponname == "Mk153 SMAW" || 
            //    weaponname == "FGM-148 JAVELIN" || 
            //    weaponname == "Underslung Launcher" || 
            //    weaponname == "SA-18 IGLA AA" || 
            //    weaponname == "FIM-92 STINGER AA" || 
            //    weaponname == "Knife") //unreliable
            //    return 0.0;

            //if (weaponname == "Crossbow Scoped" || 
            //    weaponname == "Crossbow kobra") //no avg yet
            //    return 0.0;

            //if (weaponname == "WARSAW_ID_P_INAME_SRAW" ||
            //    weaponname == "WARSAW_ID_P_INAME_M2" ||
            //    weaponname == "WARSAW_ID_P_INAME_V40" ||
            //    weaponname == "WARSAW_ID_P_INAME_MACHETE" ||
            //    weaponname == "WARSAW_ID_P_WNAME_SHORTY" ||
            //    weaponname == "WARSAW_ID_P_INAME_C4" ||
            //    weaponname == "WARSAW_ID_P_WNAME_USAS12" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE4" ||
            //    weaponname == "WARSAW_ID_P_INAME_M32MGL" ||
            //    weaponname == "WARSAW_ID_P_INAME_RPG7" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM" ||
            //    weaponname == "WARSAW_ID_P_INAME_ACB90" ||
            //    weaponname == "WARSAW_ID_P_INAME_STARSTREAK" ||
            //    weaponname == "WARSAW_ID_P_INAME_BAYONETT" ||
            //    weaponname == "WARSAW_ID_P_INAME_M67" ||
            //    weaponname == "WARSAW_ID_P_INAME_MBTLAW" ||
            //    weaponname == "WARSAW_ID_P_SP_WNAME_USAS12NV" ||
            //    weaponname == "WARSAW_ID_P_INAME_FIM92" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE2" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE3" ||
            //    weaponname == "WARSAW_ID_P_INAME_DEFIB" ||
            //    weaponname == "WARSAW_ID_P_WNAME_870" ||
            //    weaponname == "WARSAW_ID_P_INAME_M15" ||
            //    weaponname == "WARSAW_ID_P_INAME_MORTAR" ||
            //    weaponname == "WARSAW_ID_P_XP1_VNAME_UCAV" ||
            //    weaponname == "WARSAW_ID_P_INAME_REPAIR" ||
            //    weaponname == "WARSAW_ID_P_INAME_XM25_SMK" ||
            //    weaponname == "WARSAW_ID_P_WNAME_SAIGA12" ||
            //    weaponname == "WARSAW_ID_P_WNAME_DBV12" ||
            //    weaponname == "WARSAW_ID_P_INAME_XM25" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM_SMK" ||
            //    weaponname == "WARSAW_ID_P_WNAME_M1014" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM_SHG" ||
            //    weaponname == "WARSAW_ID_P_WNAME_SPAS12" ||
            //    weaponname == "WARSAW_ID_P_INAME_XM25_FLECHETTE" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM_LVG" ||
            //    weaponname == "WARSAW_ID_P_INAME_IMPACT" ||
            //    weaponname == "WARSAW_ID_P_INAME_FGM148" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE6" ||
            //    weaponname == "WARSAW_ID_P_INAME_M26_FRAG" ||
            //    weaponname == "WARSAW_ID_P_WNAME_UTAS" ||
            //    weaponname == "WARSAW_ID_P_INAME_M34" ||
            //    weaponname == "WARSAW_ID_P_WNAME_HAWK" ||
            //    weaponname == "WARSAW_ID_P_INAME_M26_MASS" ||
            //    weaponname == "WARSAW_ID_P_INAME_M18" ||
            //    weaponname == "WARSAW_ID_P_INAME_SHANK" ||
            //    weaponname == "WARSAW_ID_P_INAME_IGLA" ||
            //    weaponname == "WARSAW_ID_P_INAME_M26_SLUG" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE5" ||
            //    weaponname == "WARSAW_ID_P_INAME_M26_FLECHETTE" ||
            //    weaponname == "WARSAW_ID_P_INAME_FLASHBANG" ||
            //    weaponname == "WARSAW_ID_P_INAME_CLAYMORE" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM_FLASH" ||
            //    weaponname == "WARSAW_ID_P_INAME_M136" ||
            //    weaponname == "WARSAW_ID_P_INAME_FLARE" ||
            //    weaponname == "WARSAW_ID_P_INAME_SMAW"||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE7" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE8" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE1" ||
            //    weaponname == "WARSAW_ID_P_XP0_WNAME_DAO12"||
            //    weaponname == "WARSAW_ID_P_XP2_INAME_AAMINE" ||
            //    weaponname == "WARSAW_ID_P_INAME_DIVERKNIFE" ||
            //    weaponname == "WARSAW_ID_P_INAME_KNIFE14100BT" ||
            //    weaponname == "WARSAW_ID_P_XP2_INAME_40MM_3GL" ||
            //    weaponname == "WARSAW_ID_P_INAME_KNIFE2142" ||
            //    weaponname == "WARSAW_ID_P_INAME_NECKKNIFE" ||
            //    weaponname == "WARSAW_ID_P_INAME_KNIFEPRECISION")
            //    return 0.0;

            //if (weaponname == "WARSAW_ID_P_INAME_EOD" || //no avg yet
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_BULLDOG" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_DEAGLE" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_MPX" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_SHIELD" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_UN6" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_CS5")
            //    return 0.0;

            //double avghk;
            double avghkyellow;
            double avghkorange;
            double avghkred;

            switch (game.ToLower() + weaponname)
            {
                case "bf3870": avghkyellow = 0.339976430507933; avghkorange = 0.407971716609519; avghkred = 0.543962288812692; break;
                case "bf3A91": avghkyellow = 0.517581639406795; avghkorange = 0.621097967288154; avghkred = 0.828130623050872; break;
                case "bf3AEK971": avghkyellow = 0.533563140188053; avghkorange = 0.640275768225663; avghkred = 0.853701024300884; break;
                case "bf3AK74M": avghkyellow = 0.54059948886715; avghkorange = 0.64871938664058; avghkred = 0.86495918218744; break;
                case "bf3AKS74U": avghkyellow = 0.522267027936353; avghkorange = 0.626720433523623; avghkred = 0.835627244698164; break;
                case "bf3AN94": avghkyellow = 0.520803634349565; avghkorange = 0.624964361219478; avghkred = 0.833285814959304; break;
                case "bf3AS-VAL": avghkyellow = 0.608099145988442; avghkorange = 0.729718975186131; avghkred = 0.972958633581508; break;
                case "bf3Crossbow kobra": avghkyellow = 0.381929239575507; avghkorange = 0.458315087490609; avghkred = 0.611086783320812; break;
                case "bf3Crossbow Scoped": avghkyellow = 0.902476934583623; avghkorange = 1.08297232150035; avghkred = 1.4439630953338; break;
                case "bf3DAO": avghkyellow = 0.34231821947792; avghkorange = 0.410781863373504; avghkred = 0.547709151164672; break;
                case "bf3F2000": avghkyellow = 0.586382938794453; avghkorange = 0.703659526553343; avghkred = 0.938212702071124; break;
                case "bf3FGM-148 JAVELIN": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf3FIM-92 STINGER AA": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf3G36C": avghkyellow = 0.554421616221255; avghkorange = 0.665305939465506; avghkred = 0.887074585954008; break;
                case "bf3G3A4": avghkyellow = 0.608624980722002; avghkorange = 0.730349976866403; avghkred = 0.973799969155204; break;
                case "bf3Glock 17": avghkyellow = 0.4972623986578; avghkorange = 0.59671487838936; avghkred = 0.79561983785248; break;
                case "bf3Glock 17 Silenced": avghkyellow = 0.462145660712503; avghkorange = 0.554574792855003; avghkred = 0.739433057140004; break;
                case "bf3Glock 18": avghkyellow = 0.657168858235272; avghkorange = 0.788602629882327; avghkred = 1.05147017317644; break;
                case "bf3Glock 18 Silenced": avghkyellow = 0.613299841014685; avghkorange = 0.735959809217622; avghkred = 0.981279745623496; break;
                case "bf3KH2002": avghkyellow = 0.543228617435193; avghkorange = 0.651874340922231; avghkred = 0.869165787896308; break;
                case "bf3Knife": avghkyellow = 0.0165952404432102; avghkorange = 0.0199142885318522; avghkred = 0.0265523847091363; break;
                case "bf3M1014": avghkyellow = 0.340935837448152; avghkorange = 0.409123004937783; avghkred = 0.545497339917044; break;
                case "bf3M16A4": avghkyellow = 0.571088203604977; avghkorange = 0.685305844325973; avghkred = 0.913741125767964; break;
                case "bf3M1911": avghkyellow = 0.491690876000885; avghkorange = 0.590029051201062; avghkred = 0.786705401601416; break;
                case "bf3M1911 LIT": avghkyellow = 0.482412998494935; avghkorange = 0.578895598193922; avghkred = 0.771860797591896; break;
                case "bf3M1911 SILENCED": avghkyellow = 0.48568185719811; avghkorange = 0.582818228637732; avghkred = 0.777090971516976; break;
                case "bf3M1911 Tactical": avghkyellow = 0.478819712800963; avghkorange = 0.574583655361155; avghkred = 0.76611154048154; break;
                case "bf3M240": avghkyellow = 0.581971516081042; avghkorange = 0.698365819297251; avghkred = 0.931154425729668; break;
                case "bf3M249": avghkyellow = 0.598854507396175; avghkorange = 0.71862540887541; avghkred = 0.95816721183388; break;
                case "bf3M27": avghkyellow = 0.595230675921253; avghkorange = 0.714276811105503; avghkred = 0.952369081474004; break;
                case "bf3M39": avghkyellow = 0.836480279173442; avghkorange = 1.00377633500813; avghkred = 1.33836844667751; break;
                case "bf3M40A5": avghkyellow = 1.17953754916941; avghkorange = 1.41544505900329; avghkred = 1.88726007867106; break;
                case "bf3M412 Rex": avghkyellow = 0.545786569110915; avghkorange = 0.654943882933098; avghkred = 0.873258510577464; break;
                case "bf3M416": avghkyellow = 0.575645351708025; avghkorange = 0.69077442204963; avghkred = 0.92103256273284; break;
                case "bf3M4A1": avghkyellow = 0.555891811873215; avghkorange = 0.667070174247858; avghkred = 0.889426898997144; break;
                case "bf3M60": avghkyellow = 0.569374462201672; avghkorange = 0.683249354642007; avghkred = 0.910999139522676; break;
                case "bf3M9": avghkyellow = 0.458416598119113; avghkorange = 0.550099917742935; avghkred = 0.73346655699058; break;
                case "bf3M9 Flashlight": avghkyellow = 0.504774483554675; avghkorange = 0.60572938026561; avghkred = 0.80763917368748; break;
                case "bf3M9 Silenced": avghkyellow = 0.444202045760532; avghkorange = 0.533042454912639; avghkred = 0.710723273216852; break;
                case "bf3M93R": avghkyellow = 0.574713054813455; avghkorange = 0.689655665776146; avghkred = 0.919540887701528; break;
                case "bf3M98B": avghkyellow = 1.29752566679251; avghkorange = 1.55703080015101; avghkred = 2.07604106686801; break;
                case "bf3MK11": avghkyellow = 0.870509997063783; avghkorange = 1.04461199647654; avghkred = 1.39281599530205; break;
                case "bf3Mk153 SMAW": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf3MP 443": avghkyellow = 0.485850612074165; avghkorange = 0.583020734488998; avghkred = 0.777360979318664; break;
                case "bf3MP443 LIT": avghkyellow = 0.527223585856177; avghkorange = 0.632668303027413; avghkred = 0.843557737369884; break;
                case "bf3MP443 Silenced": avghkyellow = 0.473629758920918; avghkorange = 0.568355710705101; avghkred = 0.757807614273468; break;
                case "bf3MP7": avghkyellow = 0.409704489967892; avghkorange = 0.491645387961471; avghkred = 0.655527183948628; break;
                case "bf3P90": avghkyellow = 0.477566996403315; avghkorange = 0.573080395683978; avghkred = 0.764107194245304; break;
                case "bf3PDR": avghkyellow = 0.48666044408619; avghkorange = 0.583992532903428; avghkred = 0.778656710537904; break;
                case "bf3Pecheneg": avghkyellow = 0.57836228322992; avghkorange = 0.694034739875904; avghkred = 0.925379653167872; break;
                case "bf3PP2000": avghkyellow = 0.438221143253513; avghkorange = 0.525865371904215; avghkred = 0.70115382920562; break;
                case "bf3RPG-7": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf3RPK": avghkyellow = 0.576168542496422; avghkorange = 0.691402250995707; avghkred = 0.921869667994276; break;
                case "bf3SA-18 IGLA AA": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf3Saiga": avghkyellow = 0.299526565722067; avghkorange = 0.359431878866481; avghkred = 0.479242505155308; break;
                case "bf3SCAR": avghkyellow = 0.619344930022108; avghkorange = 0.743213916026529; avghkred = 0.990951888035372; break;
                case "bf3SG553": avghkyellow = 0.513752384016408; avghkorange = 0.616502860819689; avghkred = 0.822003814426252; break;
                case "bf3SKS": avghkyellow = 0.679858008973035; avghkorange = 0.815829610767642; avghkred = 1.08777281435686; break;
                case "bf3SV98": avghkyellow = 1.28268795877318; avghkorange = 1.53922555052781; avghkred = 2.05230073403708; break;
                case "bf3SVD": avghkyellow = 0.85534345949108; avghkorange = 1.0264121513893; avghkred = 1.36854953518573; break;
                case "bf3Taurus 44": avghkyellow = 0.550824742918883; avghkorange = 0.660989691502659; avghkred = 0.881319588670212; break;
                case "bf3Taurus 44 scoped": avghkyellow = 0.63796993557631; avghkorange = 0.765563922691572; avghkred = 1.0207518969221; break;
                case "bf3Type88": avghkyellow = 0.558621685308663; avghkorange = 0.670346022370395; avghkred = 0.89379469649386; break;
                case "bf3UMP": avghkyellow = 0.473529943691353; avghkorange = 0.568235932429623; avghkred = 0.757647909906164; break;
                case "bf3Underslung Launcher": avghkyellow = 0.088532503227281; avghkorange = 0.106239003872737; avghkred = 0.14165200516365; break;
                case "bf3Underslung Shotgun": avghkyellow = 0.37698005841512; avghkorange = 0.452376070098144; avghkred = 0.603168093464192; break;
                case "bf3USAS": avghkyellow = 0.392411108883545; avghkorange = 0.470893330660254; avghkred = 0.627857774213672; break;
                case "bf3XP1 FAMAS": avghkyellow = 0.60925380877329; avghkorange = 0.731104570527948; avghkred = 0.974806094037264; break;
                case "bf3XP1 HK53": avghkyellow = 0.517542425898413; avghkorange = 0.621050911078095; avghkred = 0.82806788143746; break;
                case "bf3XP1 Jackhammer": avghkyellow = 0.366483859987867; avghkorange = 0.439780631985441; avghkred = 0.586374175980588; break;
                case "bf3XP1 L85A2": avghkyellow = 0.532847021647727; avghkorange = 0.639416425977273; avghkred = 0.852555234636364; break;
                case "bf3XP1 L96": avghkyellow = 1.29365754450633; avghkorange = 1.5523890534076; avghkred = 2.06985207121013; break;
                case "bf3XP1 MG36": avghkyellow = 0.59187824853825; avghkorange = 0.7102538982459; avghkred = 0.9470051976612; break;
                case "bf3XP1 PP19": avghkyellow = 0.459327848462058; avghkorange = 0.551193418154469; avghkred = 0.734924557539292; break;
                case "bf3XP1 QBB95": avghkyellow = 0.585020984511035; avghkorange = 0.702025181413242; avghkred = 0.936033575217656; break;
                case "bf3XP1 QBU88": avghkyellow = 0.868830739810337; avghkorange = 1.0425968877724; avghkred = 1.39012918369654; break;
                case "bf3XP1 QBZ95B": avghkyellow = 0.478835904006505; avghkorange = 0.574603084807806; avghkred = 0.766137446410408; break;
                case "bf3XP2 ACR": avghkyellow = 0.494564872212602; avghkorange = 0.593477846655123; avghkred = 0.791303795540164; break;
                case "bf3XP2 AUG": avghkyellow = 0.471477660628592; avghkorange = 0.565773192754311; avghkred = 0.754364257005748; break;
                case "bf3XP2 HK417": avghkyellow = 0.708652673395595; avghkorange = 0.850383208074714; avghkred = 1.13384427743295; break;
                case "bf3XP2 JNG90": avghkyellow = 1.17082161116576; avghkorange = 1.40498593339891; avghkred = 1.87331457786522; break;
                case "bf3XP2 L86A1": avghkyellow = 0.545897803728955; avghkorange = 0.655077364474746; avghkred = 0.873436485966328; break;
                case "bf3XP2 LSAT": avghkyellow = 0.564557066096647; avghkorange = 0.677468479315977; avghkred = 0.903291305754636; break;
                case "bf3XP2 MP5K": avghkyellow = 0.47220922590851; avghkorange = 0.566651071090212; avghkred = 0.755534761453616; break;
                case "bf3XP2 MTAR-21": avghkyellow = 0.549724142612207; avghkorange = 0.659668971134649; avghkred = 0.879558628179532; break;
                case "bf3XP2 SCARL": avghkyellow = 0.495728143177788; avghkorange = 0.594873771813345; avghkred = 0.79316502908446; break;
                case "bf3XP2 SPAS12": avghkyellow = 0.307601838313247; avghkorange = 0.369122205975897; avghkred = 0.492162941301196; break;
                case "bf440MM": avghkyellow = 0.0443263148004145; avghkorange = 0.0531915777604974; avghkred = 0.0709221036806632; break;
                case "bf440MM_3GL": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf440MM_FLASH": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf440MM_LVG": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf440MM_SHG": avghkyellow = 0.246600440890667; avghkorange = 0.295920529068801; avghkred = 0.394560705425068; break;
                case "bf440MM_SMK": avghkyellow = 0.184426229508197; avghkorange = 0.221311475409836; avghkred = 0.295081967213114; break;
                case "bf4870": avghkyellow = 0.262111930278927; avghkorange = 0.314534316334713; avghkred = 0.419379088446284; break;
                case "bf493R": avghkyellow = 0.497030289326158; avghkorange = 0.596436347191389; avghkred = 0.795248462921852; break;
                case "bf4A91": avghkyellow = 0.477355759356518; avghkorange = 0.572826911227821; avghkred = 0.763769214970428; break;
                case "bf4AAMINE": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4ACB90": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4ACR": avghkyellow = 0.530541028596037; avghkorange = 0.636649234315245; avghkred = 0.84886564575366; break;
                case "bf4AEK971": avghkyellow = 0.542865159461087; avghkorange = 0.651438191353305; avghkred = 0.86858425513774; break;
                case "bf4AK12": avghkyellow = 0.511855356550073; avghkorange = 0.614226427860087; avghkred = 0.818968570480116; break;
                case "bf4AK5C": avghkyellow = 0.478216371700783; avghkorange = 0.573859646040939; avghkred = 0.765146194721252; break;
                case "bf4AKU12": avghkyellow = 0.43856515462782; avghkorange = 0.526278185553384; avghkred = 0.701704247404512; break;
                case "bf4AMR2": avghkyellow = 0.225880293691847; avghkorange = 0.271056352430216; avghkred = 0.361408469906955; break;
                case "bf4AMR2CQB": avghkyellow = 0.231547619047619; avghkorange = 0.277857142857143; avghkred = 0.37047619047619; break;
                case "bf4AMR2MID": avghkyellow = 0.347874725498805; avghkorange = 0.417449670598566; avghkred = 0.556599560798088; break;
                case "bf4AR160": avghkyellow = 0.556809316405652; avghkorange = 0.668171179686783; avghkred = 0.890894906249044; break;
                case "bf4ASVAL": avghkyellow = 0.534232894012263; avghkorange = 0.641079472814715; avghkred = 0.85477263041962; break;
                case "bf4AWS": avghkyellow = 0.501397261496775; avghkorange = 0.60167671379613; avghkred = 0.80223561839484; break;
                case "bf4BAYONETT": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4BPKNIFE1": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4BPKNIFE2": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4BPKNIFE3": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4BPKNIFE4": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4BPKNIFE5": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4BPKNIFE6": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4BPKNIFE7": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4BPKNIFE8": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4BULLDOG": avghkyellow = 0.534778694680388; avghkorange = 0.641734433616465; avghkred = 0.85564591148862; break;
                case "bf4C4": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4CBJMS": avghkyellow = 0.449536257194203; avghkorange = 0.539443508633043; avghkred = 0.719258011510724; break;
                case "bf4CLAYMORE": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4CS5": avghkyellow = 0.754892497861683; avghkorange = 0.905870997434019; avghkred = 1.20782799657869; break;
                case "bf4CSLR4": avghkyellow = 0.901753256805467; avghkorange = 1.08210390816656; avghkred = 1.44280521088875; break;
                case "bf4CZ75": avghkyellow = 0.42544923968315; avghkorange = 0.51053908761978; avghkred = 0.68071878349304; break;
                case "bf4CZ805": avghkyellow = 0.446765347566133; avghkorange = 0.536118417079359; avghkred = 0.714824556105812; break;
                case "bf4DAO12": avghkyellow = 0.271518744128093; avghkorange = 0.325822492953711; avghkred = 0.434429990604948; break;
                case "bf4DBV12": avghkyellow = 0.26718868110332; avghkorange = 0.320626417323984; avghkred = 0.427501889765312; break;
                case "bf4DEAGLE": avghkyellow = 0.52241498109911; avghkorange = 0.626897977318932; avghkred = 0.835863969758576; break;
                case "bf4DEFIB": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4DIVERKNIFE": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4EOD": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4F2000": avghkyellow = 0.483779083210265; avghkorange = 0.580534899852318; avghkred = 0.774046533136424; break;
                case "bf4FAMAS": avghkyellow = 0.519189425810833; avghkorange = 0.623027310972999; avghkred = 0.830703081297332; break;
                case "bf4FGM148": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4FIM92": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4FLARE": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4FLASHBANG": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4FN57": avghkyellow = 0.444345491637373; avghkorange = 0.533214589964847; avghkred = 0.710952786619796; break;
                case "bf4FYJS": avghkyellow = 1.17455167072137; avghkorange = 1.40946200486564; avghkred = 1.87928267315419; break;
                case "bf4G36C": avghkyellow = 0.42739571008269; avghkorange = 0.512874852099228; avghkred = 0.683833136132304; break;
                case "bf4GALIL21": avghkyellow = 0.487237540664692; avghkorange = 0.584685048797631; avghkred = 0.779580065063508; break;
                case "bf4GALIL23": avghkyellow = 0.522157917099545; avghkorange = 0.626589500519454; avghkred = 0.835452667359272; break;
                case "bf4GALIL52": avghkyellow = 0.507187386871213; avghkorange = 0.608624864245455; avghkred = 0.81149981899394; break;
                case "bf4GALIL53": avghkyellow = 0.544611888035177; avghkorange = 0.653534265642213; avghkred = 0.871379020856284; break;
                case "bf4GLOCK18": avghkyellow = 0.544861740959322; avghkorange = 0.653834089151187; avghkred = 0.871778785534916; break;
                case "bf4GOL": avghkyellow = 0.97879396740721; avghkorange = 1.17455276088865; avghkred = 1.56607034785154; break;
                case "bf4HAWK": avghkyellow = 0.254876000167737; avghkorange = 0.305851200201285; avghkred = 0.40780160026838; break;
                case "bf4HK45C": avghkyellow = 0.455134226932732; avghkorange = 0.546161072319279; avghkred = 0.728214763092372; break;
                case "bf4IGLA": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4IMPACT": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4JNG90": avghkyellow = 0.899435731981347; avghkorange = 1.07932287837762; avghkred = 1.43909717117016; break;
                case "bf4JS2": avghkyellow = 0.502317796565648; avghkorange = 0.602781355878777; avghkred = 0.803708474505036; break;
                case "bf4KNIFE14100BT": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4KNIFE2142": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4KNIFEPRECISION": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4L85A2": avghkyellow = 0.537633444465513; avghkorange = 0.645160133358615; avghkred = 0.86021351114482; break;
                case "bf4L96A1": avghkyellow = 1.01961549679861; avghkorange = 1.22353859615833; avghkred = 1.63138479487778; break;
                case "bf4LSAT": avghkyellow = 0.468442390101848; avghkorange = 0.562130868122217; avghkred = 0.749507824162956; break;
                case "bf4M1014": avghkyellow = 0.27064683624182; avghkorange = 0.324776203490184; avghkred = 0.433034937986912; break;
                case "bf4M136": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4M15": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4M16A4": avghkyellow = 0.502379077224578; avghkorange = 0.602854892669493; avghkred = 0.803806523559324; break;
                case "bf4M18": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4M1911": avghkyellow = 0.47008114019546; avghkorange = 0.564097368234552; avghkred = 0.752129824312736; break;
                case "bf4M2": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4M200": avghkyellow = 1.10472636467506; avghkorange = 1.32567163761007; avghkred = 1.7675621834801; break;
                case "bf4M240": avghkyellow = 0.497963705744112; avghkorange = 0.597556446892935; avghkred = 0.79674192919058; break;
                case "bf4M249": avghkyellow = 0.499658985508105; avghkorange = 0.599590782609726; avghkred = 0.799454376812968; break;
                case "bf4M26_FLECHETTE": avghkyellow = 0.223460424562989; avghkorange = 0.268152509475586; avghkred = 0.357536679300782; break;
                case "bf4M26_FRAG": avghkyellow = 0.0706611570247933; avghkorange = 0.0847933884297519; avghkred = 0.113057851239669; break;
                case "bf4M26_MASS": avghkyellow = 0.267933230453775; avghkorange = 0.32151987654453; avghkred = 0.42869316872604; break;
                case "bf4M26_SLUG": avghkyellow = 0.31970340020168; avghkorange = 0.383644080242016; avghkred = 0.511525440322688; break;
                case "bf4M32MGL": avghkyellow = 0.0448879284906675; avghkorange = 0.053865514188801; avghkred = 0.071820685585068; break;
                case "bf4M34": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4M39": avghkyellow = 0.602970464556058; avghkorange = 0.723564557467269; avghkred = 0.964752743289692; break;
                case "bf4M40A5": avghkyellow = 0.913167583950552; avghkorange = 1.09580110074066; avghkred = 1.46106813432088; break;
                case "bf4M412REX": avghkyellow = 0.48533399390743; avghkorange = 0.582400792688916; avghkred = 0.776534390251888; break;
                case "bf4M416": avghkyellow = 0.526065127527123; avghkorange = 0.631278153032547; avghkred = 0.841704204043396; break;
                case "bf4M4A1": avghkyellow = 0.480164714720195; avghkorange = 0.576197657664234; avghkred = 0.768263543552312; break;
                case "bf4M60E4": avghkyellow = 0.454948046675143; avghkorange = 0.545937656010171; avghkred = 0.727916874680228; break;
                case "bf4M67": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4M82A3": avghkyellow = 0.388690476190475; avghkorange = 0.46642857142857; avghkred = 0.62190476190476; break;
                case "bf4M82A3CQB": avghkyellow = 0.2881502868821; avghkorange = 0.34578034425852; avghkred = 0.46104045901136; break;
                case "bf4M82A3MID": avghkyellow = 0.332117386876785; avghkorange = 0.398540864252142; avghkred = 0.531387819002856; break;
                case "bf4M9": avghkyellow = 0.441926836349877; avghkorange = 0.530312203619853; avghkred = 0.707082938159804; break;
                case "bf4M98B": avghkyellow = 0.985675024352282; avghkorange = 1.18281002922274; avghkred = 1.57708003896365; break;
                case "bf4MACHETE": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4MBTLAW": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4MG4": avghkyellow = 0.516316009700855; avghkorange = 0.619579211641026; avghkred = 0.826105615521368; break;
                case "bf4MK11": avghkyellow = 0.528735754493015; avghkorange = 0.634482905391618; avghkred = 0.845977207188824; break;
                case "bf4MORTAR": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4MP443": avghkyellow = 0.475606424558948; avghkorange = 0.570727709470737; avghkred = 0.760970279294316; break;
                case "bf4MP7": avghkyellow = 0.490546459525015; avghkorange = 0.588655751430018; avghkred = 0.784874335240024; break;
                case "bf4MPX": avghkyellow = 0.48067756439215; avghkorange = 0.57681307727058; avghkred = 0.76908410302744; break;
                case "bf4MTAR21": avghkyellow = 0.521657499752145; avghkorange = 0.625988999702574; avghkred = 0.834651999603432; break;
                case "bf4MX4": avghkyellow = 0.505342738683478; avghkorange = 0.606411286420173; avghkred = 0.808548381893564; break;
                case "bf4NECKKNIFE": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4P226": avghkyellow = 0.392243799396272; avghkorange = 0.470692559275527; avghkred = 0.627590079034036; break;
                case "bf4P90": avghkyellow = 0.517827711637755; avghkorange = 0.621393253965306; avghkred = 0.828524338620408; break;
                case "bf4PDR": avghkyellow = 0.462967865144812; avghkorange = 0.555561438173775; avghkred = 0.7407485842317; break;
                case "bf4PECHENEG": avghkyellow = 0.52883666651255; avghkorange = 0.63460399981506; avghkred = 0.84613866642008; break;
                case "bf4PP2000": avghkyellow = 0.41269622730147; avghkorange = 0.495235472761764; avghkred = 0.660313963682352; break;
                case "bf4QBB95": avghkyellow = 0.47166542195567; avghkorange = 0.565998506346804; avghkred = 0.754664675129072; break;
                case "bf4QBS09": avghkyellow = 0.287439209582957; avghkorange = 0.344927051499549; avghkred = 0.459902735332732; break;
                case "bf4QBU88": avghkyellow = 0.632348096088522; avghkorange = 0.758817715306227; avghkred = 1.01175695374164; break;
                case "bf4QBZ951": avghkyellow = 0.460966893468515; avghkorange = 0.553160272162218; avghkred = 0.737547029549624; break;
                case "bf4QSZ92": avghkyellow = 0.436903030336765; avghkorange = 0.524283636404118; avghkred = 0.699044848538824; break;
                case "bf4REPAIR": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4RFBTARGET": avghkyellow = 0.5424797340794; avghkorange = 0.65097568089528; avghkred = 0.86796757452704; break;
                case "bf4RPG7": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4RPK12": avghkyellow = 0.480955818788602; avghkorange = 0.577146982546323; avghkred = 0.769529310061764; break;
                case "bf4RPK74": avghkyellow = 0.431579612976588; avghkorange = 0.517895535571905; avghkred = 0.69052738076254; break;
                case "bf4SAIGA12": avghkyellow = 0.271517084234205; avghkorange = 0.325820501081046; avghkred = 0.434427334774728; break;
                case "bf4SAR21": avghkyellow = 0.444758096674867; avghkorange = 0.533709716009841; avghkred = 0.711612954679788; break;
                case "bf4SCARH": avghkyellow = 0.544012504791542; avghkorange = 0.652815005749851; avghkred = 0.870420007666468; break;
                case "bf4SCARHSV": avghkyellow = 0.52651697049123; avghkorange = 0.631820364589476; avghkred = 0.842427152785968; break;
                case "bf4SCORP": avghkyellow = 0.492144335911975; avghkorange = 0.59057320309437; avghkred = 0.78743093745916; break;
                case "bf4SCOUTELIT": avghkyellow = 0.937427067699747; avghkorange = 1.1249124812397; avghkred = 1.4998833083196; break;
                case "bf4SG553": avghkyellow = 0.476698254746885; avghkorange = 0.572037905696262; avghkred = 0.762717207595016; break;
                case "bf4SHANK": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4SHIELD": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4SHORTY": avghkyellow = 0.258813261532935; avghkorange = 0.310575913839522; avghkred = 0.414101218452696; break;
                case "bf4SKS": avghkyellow = 0.49418654574711; avghkorange = 0.593023854896532; avghkred = 0.790698473195376; break;
                case "bf4SMAW": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4SPAS12": avghkyellow = 0.247541955273615; avghkorange = 0.297050346328337; avghkred = 0.396067128437783; break;
                case "bf4SR2": avghkyellow = 0.470486303570987; avghkorange = 0.564583564285185; avghkred = 0.75277808571358; break;
                case "bf4SR338": avghkyellow = 0.716010764442573; avghkorange = 0.859212917331087; avghkred = 1.14561722310812; break;
                case "bf4SRAW": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4SRS": avghkyellow = 0.83375714410993; avghkorange = 1.00050857293192; avghkred = 1.33401143057589; break;
                case "bf4STARSTREAK": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4STEYRAUG": avghkyellow = 0.461866630852538; avghkorange = 0.554239957023045; avghkred = 0.73898660936406; break;
                case "bf4SV98": avghkyellow = 0.771085512706893; avghkorange = 0.925302615248271; avghkred = 1.23373682033103; break;
                case "bf4SVD12": avghkyellow = 0.536056504242995; avghkorange = 0.643267805091594; avghkred = 0.857690406788792; break;
                case "bf4SW40": avghkyellow = 0.447970894382468; avghkorange = 0.537565073258961; avghkred = 0.716753431011948; break;
                case "bf4TAURUS44": avghkyellow = 0.667555413755395; avghkorange = 0.801066496506474; avghkred = 1.06808866200863; break;
                case "bf4TYPE88": avghkyellow = 0.508989185924448; avghkorange = 0.610787023109337; avghkred = 0.814382697479116; break;
                case "bf4TYPE95B1": avghkyellow = 0.429936377731635; avghkorange = 0.515923653277962; avghkred = 0.687898204370616; break;
                case "bf4UCAV": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4ULTIM": avghkyellow = 0.4520946990824; avghkorange = 0.54251363889888; avghkred = 0.72335151853184; break;
                case "bf4UMP45": avghkyellow = 0.47080989749865; avghkorange = 0.56497187699838; avghkred = 0.75329583599784; break;
                case "bf4UMP9": avghkyellow = 0.41554480215458; avghkorange = 0.498653762585496; avghkred = 0.664871683447328; break;
                case "bf4UN6": avghkyellow = 0.78884791886136; avghkorange = 0.946617502633632; avghkred = 1.26215667017818; break;
                case "bf4USAS12": avghkyellow = 0.0504249442123973; avghkorange = 0.0605099330548767; avghkred = 0.0806799107398356; break;
                case "bf4USAS12NV": avghkyellow = 0.0698678609657993; avghkorange = 0.0838414331589591; avghkred = 0.111788577545279; break;
                case "bf4UTAS": avghkyellow = 0.239433070697301; avghkorange = 0.287319684836761; avghkred = 0.383092913115681; break;
                case "bf4V40": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;
                case "bf4XM25": avghkyellow = 0.0545562589051655; avghkorange = 0.0654675106861986; avghkred = 0.0872900142482648; break;
                case "bf4XM25_FLECHETTE": avghkyellow = 0.214818507938382; avghkorange = 0.257782209526058; avghkred = 0.343709612701411; break;
                case "bf4XM25_SMK": avghkyellow = 0; avghkorange = 0; avghkred = 0; break;

                default:
                    log("CD - weapon avg headshots/kill not found: " + weaponname, 1.5);
                    //avghk = 1.0;
                    avghkyellow = 100; avghkorange = 100; avghkred = 100;
                    break;
            }

            weaponname = FixWeaponName(weaponname);

            double hspk = Math.Round((headshots / kills) * 100.0, 2);
            double hspkabove = Math.Round((headshots / kills) / (avghkyellow/2.5), 2);
            
            double cheatindex = 0.0;
            if (kills > minimumkills/* || (kills > 20 && headshots > kills)*/)
            {
                if ((headshots / kills) > avghkred)
                {
                    log("CD - IMPOSSIBLE STATS: " + playername + " " + weaponname + " hs/k: " + hspk.ToString() + "%" + " (" + hspkabove.ToString() + ")", 1);
                    if (!reasons.ContainsKey(hspkabove))
                        reasons.Add(hspkabove, weaponname + " hs/k: " + hspk.ToString() + "%");

                    cheatindex += redflagcount;
                }
                else if ((headshots / kills) > avghkorange)
                {
                    log("CD - SUSPICIOUS stats: " + playername + " " + weaponname + " hs/k: " + hspk.ToString() + "%" + " (" + hspkabove.ToString() + ")", 1);
                    if (!reasons.ContainsKey(hspkabove))
                        reasons.Add(hspkabove, weaponname + " hs/k: " + hspk.ToString() + "%");

                    cheatindex += orangeflagcount;
                }
                else if ((headshots / kills) > avghkyellow)
                {
                    log("CD - suspicious stats: " + playername + " " + weaponname + " hs/k: " + hspk.ToString() + "%" + " (" + hspkabove.ToString() + ")", 1);
                    if (!reasons.ContainsKey(hspkabove))
                        reasons.Add(hspkabove, weaponname + " hs/k: " + hspk.ToString() + "%");

                    cheatindex += yellowflagcount;
                }
            }
            else
            {
                log("CD - skipping weapon (less than " + minimumkills.ToString() + " kills): " + weaponname, 5);
            }
            log("CD - headshot/kill: " + hspk.ToString() + "%", 5);
            return cheatindex;
        }
        #endregion

        public static bool IsAllowedWeapon(string weaponname)
        {
            bool retval = true;

            if (weaponname == "KNIFE2142" ||
        weaponname == "REPAIR" ||
        weaponname == "KNIFE14100BT" ||
                //weaponname=="BULLDOG"||
        weaponname == "BPKNIFE8" ||
        weaponname == "BPKNIFE7" ||
        weaponname == "BPKNIFE6" ||
        weaponname == "BPKNIFE5" ||
        weaponname == "BPKNIFE4" ||
        weaponname == "BPKNIFE3" ||
        weaponname == "BPKNIFE2" ||
        weaponname == "BPKNIFE1" ||
                //weaponname=="CS5"||
        weaponname == "NECKKNIFE" ||
                //weaponname=="DEAGLE"||
        weaponname == "KNIFEPRECISION" ||
                //weaponname=="M136"||
        weaponname == "M18" ||
        weaponname == "M15" ||
        weaponname == "FLASHBANG" ||
                //weaponname=="M82A3"||
        weaponname == "MACHETE" ||
        weaponname == "MORTAR" ||
                //weaponname=="MPX"||
        weaponname == "EOD" ||
        weaponname == "DIVERKNIFE" ||
        weaponname == "BAYONETT" ||
        weaponname == "XM25_SMK" ||
                //weaponname=="UN6"||
        weaponname == "UCAV" ||
        weaponname == "ACB90" ||
                //weaponname=="40MM_SHG"||
                //weaponname=="AMR2CQB"||        
        weaponname == "SHIELD" ||
        weaponname == "SHANK" ||
        weaponname == "FLARE" ||
                //weaponname=="40MM_SMK"||
        weaponname == "FGM148" ||//!!
        weaponname == "FIM92" ||//!!
        weaponname == "IGLA" ||//!!
        weaponname == "FGM-148 JAVELIN" ||//!!
        weaponname == "FIM-92 STINGER AA" ||//!!
        weaponname == "SA-18 IGLA AA" ||//!!
        weaponname == "M2" ||//!!
        weaponname == "AAMINE" ||
        weaponname == "DEFIB" ||
                weaponname == "Knife" ||
                weaponname == "Underslung Launcher" ||
                weaponname == "Underslung Shotgun" ||
                weaponname == "KNIFEWEAVER" ||
                weaponname == "RPG7" ||
                weaponname == "SMAW" ||
                weaponname == "SRAW" ||
                weaponname == "Mk153 SMAW" ||
                weaponname == "RPG-7" ||
                weaponname == "STARSTREAK" ||
                weaponname == "C4" ||
                weaponname == "XM25" ||
                weaponname == "M32MGL" ||
                weaponname == "XM25_FLECHETTE" ||
                weaponname == "M67" ||
                weaponname == "V40" ||
                weaponname == "IMPACT" ||
                weaponname == "M34" ||
                weaponname == "M18" ||
                weaponname == "M26_MASS" ||
                weaponname == "USAS12" ||
                weaponname == "QBS09" ||
                weaponname == "870" ||
                weaponname == "M1014" ||
                weaponname == "SPAS12" ||
                weaponname == "HAWK" ||
                weaponname == "SAIGA12" ||
                weaponname == "DBV12" ||
                weaponname == "USAS12NV" ||
                weaponname == "SHORTY" ||
                weaponname == "M26_FRAG" ||
                weaponname == "UTAS" ||
                weaponname == "M26_SLUG" ||
                weaponname == "M26_FLECHETTE" ||
                weaponname == "DAO12" ||
                weaponname == "40MM" ||
                weaponname == "40MM_3GL" ||
                weaponname == "40MM_FLASH" ||
                weaponname == "40MM_LVG" ||
                weaponname == "40MM_SHG" ||
                weaponname == "40MM_SMK" ||
                weaponname == "DAO" ||
                weaponname == "Saiga" ||
                weaponname == "USAS" ||
                weaponname == "XP1 Jackhammer" ||
                weaponname == "RAILGUN" ||
                weaponname == "KNIFEBIPOD" ||
                weaponname == "KNIFETANTO" ||
                weaponname == "SPYDER")
                retval = false;

            return retval;
        }

        #region checkAccuracy
        public double checkAccuracy(ref Dictionary<double, string> reasons, string game, string playername, string weaponname, double weaponacc, double kills)
        {
            if (!IsAllowedWeapon(weaponname))
            {
                return 0.0;
            }
            
            //if (weaponname == "Underslung Shotgun" || 
            //    weaponname == "XP1 Jackhammer" || 
            //    weaponname == "M1014" || 
            //    weaponname == "DAO" || 
            //    weaponname == "XP2 SPAS12" || 
            //    weaponname == "870" || 
            //    weaponname == "USAS" || 
            //    weaponname == "Saiga" || 
            //    weaponname == "RPG-7" || 
            //    weaponname == "Mk153 SMAW" || 
            //    weaponname == "FGM-148 JAVELIN" || 
            //    weaponname == "Underslung Launcher" || 
            //    weaponname == "SA-18 IGLA AA" || 
            //    weaponname == "FIM-92 STINGER AA" || 
            //    weaponname == "Knife") //unreliable
            //    return 0.0;

            //if (weaponname == "Crossbow Scoped" || 
            //    weaponname == "Crossbow kobra") //no avg yet
            //    return 0.0;

            //if (weaponname == "WARSAW_ID_P_INAME_SRAW" ||
            //    weaponname == "WARSAW_ID_P_INAME_M2" ||
            //    weaponname == "WARSAW_ID_P_INAME_V40" ||
            //    weaponname == "WARSAW_ID_P_INAME_MACHETE" ||
            //    weaponname == "WARSAW_ID_P_WNAME_SHORTY" ||
            //    weaponname == "WARSAW_ID_P_INAME_C4" ||
            //    weaponname == "WARSAW_ID_P_WNAME_USAS12" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE4" ||
            //    weaponname == "WARSAW_ID_P_INAME_M32MGL" ||
            //    weaponname == "WARSAW_ID_P_INAME_RPG7" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM" ||
            //    weaponname == "WARSAW_ID_P_INAME_ACB90" ||
            //    weaponname == "WARSAW_ID_P_INAME_STARSTREAK" ||
            //    weaponname == "WARSAW_ID_P_INAME_BAYONETT" ||
            //    weaponname == "WARSAW_ID_P_INAME_M67" ||
            //    weaponname == "WARSAW_ID_P_INAME_MBTLAW" ||
            //    weaponname == "WARSAW_ID_P_SP_WNAME_USAS12NV" ||
            //    weaponname == "WARSAW_ID_P_INAME_FIM92" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE2" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE3" ||
            //    weaponname == "WARSAW_ID_P_INAME_DEFIB" ||
            //    weaponname == "WARSAW_ID_P_WNAME_870" ||
            //    weaponname == "WARSAW_ID_P_INAME_M15" ||
            //    weaponname == "WARSAW_ID_P_INAME_MORTAR" ||
            //    weaponname == "WARSAW_ID_P_XP1_VNAME_UCAV" ||
            //    weaponname == "WARSAW_ID_P_INAME_REPAIR" ||
            //    weaponname == "WARSAW_ID_P_INAME_XM25_SMK" ||
            //    weaponname == "WARSAW_ID_P_WNAME_SAIGA12" ||
            //    weaponname == "WARSAW_ID_P_WNAME_DBV12" ||
            //    weaponname == "WARSAW_ID_P_INAME_XM25" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM_SMK" ||
            //    weaponname == "WARSAW_ID_P_WNAME_M1014" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM_SHG" ||
            //    weaponname == "WARSAW_ID_P_WNAME_SPAS12" ||
            //    weaponname == "WARSAW_ID_P_INAME_XM25_FLECHETTE" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM_LVG" ||
            //    weaponname == "WARSAW_ID_P_INAME_IMPACT" ||
            //    weaponname == "WARSAW_ID_P_INAME_FGM148" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE6" ||
            //    weaponname == "WARSAW_ID_P_INAME_M26_FRAG" ||
            //    weaponname == "WARSAW_ID_P_WNAME_UTAS" ||
            //    weaponname == "WARSAW_ID_P_INAME_M34" ||
            //    weaponname == "WARSAW_ID_P_WNAME_HAWK" ||
            //    weaponname == "WARSAW_ID_P_INAME_M26_MASS" ||
            //    weaponname == "WARSAW_ID_P_INAME_M18" ||
            //    weaponname == "WARSAW_ID_P_INAME_SHANK" ||
            //    weaponname == "WARSAW_ID_P_INAME_IGLA" ||
            //    weaponname == "WARSAW_ID_P_INAME_M26_SLUG" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE5" ||
            //    weaponname == "WARSAW_ID_P_INAME_M26_FLECHETTE" ||
            //    weaponname == "WARSAW_ID_P_INAME_FLASHBANG" ||
            //    weaponname == "WARSAW_ID_P_INAME_CLAYMORE" ||
            //    weaponname == "WARSAW_ID_P_INAME_40MM_FLASH" ||
            //    weaponname == "WARSAW_ID_P_INAME_M136" ||
            //    weaponname == "WARSAW_ID_P_INAME_FLARE" ||
            //    weaponname == "WARSAW_ID_P_INAME_SMAW" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE7" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE8" ||
            //    weaponname == "WARSAW_ID_P_INAME_BPKNIFE1" ||
            //    weaponname == "WARSAW_ID_P_XP0_WNAME_DAO12" ||
            //    weaponname == "WARSAW_ID_P_XP2_INAME_AAMINE" ||
            //    weaponname == "WARSAW_ID_P_INAME_DIVERKNIFE" ||
            //    weaponname == "WARSAW_ID_P_INAME_KNIFE14100BT" ||
            //    weaponname == "WARSAW_ID_P_XP2_INAME_40MM_3GL" ||
            //    weaponname == "WARSAW_ID_P_INAME_KNIFE2142" ||
            //    weaponname == "WARSAW_ID_P_INAME_NECKKNIFE" ||
            //    weaponname == "WARSAW_ID_P_INAME_KNIFEPRECISION")
            //    return 0.0;

            //if (weaponname == "WARSAW_ID_P_INAME_EOD" || //no avg yet
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_BULLDOG" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_DEAGLE" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_MPX" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_SHIELD" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_UN6" ||
            //    weaponname == "WARSAW_ID_P_XP3_WNAME_CS5")
            //    return 0.0;

            //double avgacc;
            double avgaccyellow;
            double avgaccorange;
            double avgaccred;

            switch (game.ToLower() + weaponname)
            {
                case "bf3870": avgaccyellow = 1.71781014277095; avgaccorange = 2.06137217132515; avgaccred = 2.74849622843353; break;
                case "bf3A91": avgaccyellow = 0.320502510449675; avgaccorange = 0.38460301253961; avgaccred = 0.51280401671948; break;
                case "bf3AEK971": avgaccyellow = 0.30200522178134; avgaccorange = 0.362406266137608; avgaccred = 0.483208354850144; break;
                case "bf3AK74M": avgaccyellow = 0.343152803607307; avgaccorange = 0.411783364328769; avgaccred = 0.549044485771692; break;
                case "bf3AKS74U": avgaccyellow = 0.329472084315432; avgaccorange = 0.395366501178519; avgaccred = 0.527155334904692; break;
                case "bf3AN94": avgaccyellow = 0.39332183798159; avgaccorange = 0.471986205577908; avgaccred = 0.629314940770544; break;
                case "bf3AS-VAL": avgaccyellow = 0.364986774734585; avgaccorange = 0.437984129681502; avgaccred = 0.583978839575336; break;
                case "bf3Crossbow kobra": avgaccyellow = 0.815786068123035; avgaccorange = 0.978943281747642; avgaccred = 1.30525770899686; break;
                case "bf3Crossbow Scoped": avgaccyellow = 0.794841467356993; avgaccorange = 0.953809760828391; avgaccred = 1.27174634777119; break;
                case "bf3DAO": avgaccyellow = 1.32643639435565; avgaccorange = 1.59172367322678; avgaccred = 2.12229823096904; break;
                case "bf3F2000": avgaccyellow = 0.308591880240695; avgaccorange = 0.370310256288834; avgaccred = 0.493747008385112; break;
                case "bf3FGM-148 JAVELIN": avgaccyellow = 1.58688396734145; avgaccorange = 1.90426076080975; avgaccred = 2.53901434774633; break;
                case "bf3FIM-92 STINGER AA": avgaccyellow = 0.733718325170315; avgaccorange = 0.880461990204378; avgaccred = 1.1739493202725; break;
                case "bf3G36C": avgaccyellow = 0.333941726106695; avgaccorange = 0.400730071328034; avgaccred = 0.534306761770712; break;
                case "bf3G3A4": avgaccyellow = 0.37477855444635; avgaccorange = 0.44973426533562; avgaccred = 0.59964568711416; break;
                case "bf3Glock 17": avgaccyellow = 0.494636156589537; avgaccorange = 0.593563387907445; avgaccred = 0.79141785054326; break;
                case "bf3Glock 17 Silenced": avgaccyellow = 0.521729008468485; avgaccorange = 0.626074810162182; avgaccred = 0.834766413549576; break;
                case "bf3Glock 18": avgaccyellow = 0.326743941436403; avgaccorange = 0.392092729723683; avgaccred = 0.522790306298244; break;
                case "bf3Glock 18 Silenced": avgaccyellow = 0.341720812653535; avgaccorange = 0.410064975184242; avgaccred = 0.546753300245656; break;
                case "bf3KH2002": avgaccyellow = 0.385275246667872; avgaccorange = 0.462330296001447; avgaccred = 0.616440394668596; break;
                case "bf3Knife": avgaccyellow = 0.554533516086453; avgaccorange = 0.665440219303743; avgaccred = 0.887253625738324; break;
                case "bf3M1014": avgaccyellow = 1.52096304586877; avgaccorange = 1.82515565504253; avgaccred = 2.43354087339004; break;
                case "bf3M16A4": avgaccyellow = 0.314245960629278; avgaccorange = 0.377095152755133; avgaccred = 0.502793537006844; break;
                case "bf3M1911": avgaccyellow = 0.505095074459745; avgaccorange = 0.606114089351694; avgaccred = 0.808152119135592; break;
                case "bf3M1911 LIT": avgaccyellow = 0.505045747249832; avgaccorange = 0.606054896699799; avgaccred = 0.808073195599732; break;
                case "bf3M1911 SILENCED": avgaccyellow = 0.531883652179885; avgaccorange = 0.638260382615862; avgaccred = 0.851013843487816; break;
                case "bf3M1911 Tactical": avgaccyellow = 0.563379898307138; avgaccorange = 0.676055877968565; avgaccred = 0.90140783729142; break;
                case "bf3M240": avgaccyellow = 0.198716541199155; avgaccorange = 0.238459849438987; avgaccred = 0.317946465918649; break;
                case "bf3M249": avgaccyellow = 0.201153722211349; avgaccorange = 0.241384466653618; avgaccred = 0.321845955538158; break;
                case "bf3M27": avgaccyellow = 0.26986153400038; avgaccorange = 0.323833840800456; avgaccred = 0.431778454400608; break;
                case "bf3M39": avgaccyellow = 0.539649433439047; avgaccorange = 0.647579320126857; avgaccred = 0.863439093502476; break;
                case "bf3M40A5": avgaccyellow = 0.782432596557348; avgaccorange = 0.938919115868817; avgaccred = 1.25189215449176; break;
                case "bf3M412 Rex": avgaccyellow = 0.562757545901347; avgaccorange = 0.675309055081617; avgaccred = 0.900412073442156; break;
                case "bf3M416": avgaccyellow = 0.325754898085138; avgaccorange = 0.390905877702165; avgaccred = 0.52120783693622; break;
                case "bf3M4A1": avgaccyellow = 0.30361622019218; avgaccorange = 0.364339464230616; avgaccred = 0.485785952307488; break;
                case "bf3M60": avgaccyellow = 0.209495784077962; avgaccorange = 0.251394940893555; avgaccred = 0.33519325452474; break;
                case "bf3M9": avgaccyellow = 0.50260487432326; avgaccorange = 0.603125849187912; avgaccred = 0.804167798917216; break;
                case "bf3M9 Flashlight": avgaccyellow = 0.48895190184878; avgaccorange = 0.586742282218536; avgaccred = 0.782323042958048; break;
                case "bf3M9 Silenced": avgaccyellow = 0.535668299300322; avgaccorange = 0.642801959160387; avgaccred = 0.857069278880516; break;
                case "bf3M93R": avgaccyellow = 0.373736060788077; avgaccorange = 0.448483272945693; avgaccred = 0.597977697260924; break;
                case "bf3M98B": avgaccyellow = 0.76352654016547; avgaccorange = 0.916231848198564; avgaccred = 1.22164246426475; break;
                case "bf3MK11": avgaccyellow = 0.558787194068035; avgaccorange = 0.670544632881642; avgaccred = 0.894059510508856; break;
                case "bf3Mk153 SMAW": avgaccyellow = 1.23605956278415; avgaccorange = 1.48327147534099; avgaccred = 1.97769530045465; break;
                case "bf3MP 443": avgaccyellow = 0.517845840778043; avgaccorange = 0.621415008933651; avgaccred = 0.828553345244868; break;
                case "bf3MP443 LIT": avgaccyellow = 0.488604038677298; avgaccorange = 0.586324846412757; avgaccred = 0.781766461883676; break;
                case "bf3MP443 Silenced": avgaccyellow = 0.52688447283986; avgaccorange = 0.632261367407832; avgaccred = 0.843015156543776; break;
                case "bf3MP7": avgaccyellow = 0.37072255897647; avgaccorange = 0.444867070771764; avgaccred = 0.593156094362352; break;
                case "bf3P90": avgaccyellow = 0.331353927727743; avgaccorange = 0.397624713273291; avgaccred = 0.530166284364388; break;
                case "bf3PDR": avgaccyellow = 0.36588883078698; avgaccorange = 0.439066596944376; avgaccred = 0.585422129259168; break;
                case "bf3Pecheneg": avgaccyellow = 0.203582611227091; avgaccorange = 0.244299133472509; avgaccred = 0.325732177963346; break;
                case "bf3PP2000": avgaccyellow = 0.366851132170657; avgaccorange = 0.440221358604789; avgaccred = 0.586961811473052; break;
                case "bf3RPG-7": avgaccyellow = 1.28146154328827; avgaccorange = 1.53775385194593; avgaccred = 2.05033846926124; break;
                case "bf3RPK": avgaccyellow = 0.273196381723772; avgaccorange = 0.327835658068527; avgaccred = 0.437114210758036; break;
                case "bf3SA-18 IGLA AA": avgaccyellow = 0.718922805846772; avgaccorange = 0.862707367016127; avgaccred = 1.15027648935484; break;
                case "bf3Saiga": avgaccyellow = 1.38231792590626; avgaccorange = 1.65878151108752; avgaccred = 2.21170868145002; break;
                case "bf3SCAR": avgaccyellow = 0.33372359811793; avgaccorange = 0.400468317741516; avgaccred = 0.533957756988688; break;
                case "bf3SG553": avgaccyellow = 0.348696275333283; avgaccorange = 0.418435530399939; avgaccred = 0.557914040533252; break;
                case "bf3SKS": avgaccyellow = 0.46662337774681; avgaccorange = 0.559948053296172; avgaccred = 0.746597404394896; break;
                case "bf3SV98": avgaccyellow = 0.735725522045465; avgaccorange = 0.882870626454558; avgaccred = 1.17716083527274; break;
                case "bf3SVD": avgaccyellow = 0.554928926639485; avgaccorange = 0.665914711967382; avgaccred = 0.887886282623176; break;
                case "bf3Taurus 44": avgaccyellow = 0.639134368064342; avgaccorange = 0.766961241677211; avgaccred = 1.02261498890295; break;
                case "bf3Taurus 44 scoped": avgaccyellow = 0.651852093920293; avgaccorange = 0.782222512704351; avgaccred = 1.04296335027247; break;
                case "bf3Type88": avgaccyellow = 0.217139124164746; avgaccorange = 0.260566948997695; avgaccred = 0.347422598663594; break;
                case "bf3UMP": avgaccyellow = 0.34740871435837; avgaccorange = 0.416890457230044; avgaccred = 0.555853942973392; break;
                case "bf3Underslung Launcher": avgaccyellow = 1.06333404360012; avgaccorange = 1.27600085232015; avgaccred = 1.7013344697602; break;
                case "bf3Underslung Shotgun": avgaccyellow = 1.93969185451718; avgaccorange = 2.32763022542061; avgaccred = 3.10350696722748; break;
                case "bf3USAS": avgaccyellow = 1.05660620356616; avgaccorange = 1.2679274442794; avgaccred = 1.69056992570586; break;
                case "bf3XP1 FAMAS": avgaccyellow = 0.312581036017152; avgaccorange = 0.375097243220583; avgaccred = 0.500129657627444; break;
                case "bf3XP1 HK53": avgaccyellow = 0.346870570894013; avgaccorange = 0.416244685072815; avgaccred = 0.55499291343042; break;
                case "bf3XP1 Jackhammer": avgaccyellow = 1.23818612228624; avgaccorange = 1.48582334674349; avgaccred = 1.98109779565798; break;
                case "bf3XP1 L85A2": avgaccyellow = 0.369852982328137; avgaccorange = 0.443823578793765; avgaccred = 0.59176477172502; break;
                case "bf3XP1 L96": avgaccyellow = 0.760756040129375; avgaccorange = 0.91290724815525; avgaccred = 1.217209664207; break;
                case "bf3XP1 MG36": avgaccyellow = 0.270195069372618; avgaccorange = 0.324234083247141; avgaccred = 0.432312110996188; break;
                case "bf3XP1 PP19": avgaccyellow = 0.36310162304256; avgaccorange = 0.435721947651072; avgaccred = 0.580962596868096; break;
                case "bf3XP1 QBB95": avgaccyellow = 0.281883840654542; avgaccorange = 0.338260608785451; avgaccred = 0.451014145047268; break;
                case "bf3XP1 QBU88": avgaccyellow = 0.560689177719283; avgaccorange = 0.672827013263139; avgaccred = 0.897102684350852; break;
                case "bf3XP1 QBZ95B": avgaccyellow = 0.380901816612438; avgaccorange = 0.457082179934925; avgaccred = 0.6094429065799; break;
                case "bf3XP2 ACR": avgaccyellow = 0.364139616497535; avgaccorange = 0.436967539797042; avgaccred = 0.582623386396056; break;
                case "bf3XP2 AUG": avgaccyellow = 0.35342441635489; avgaccorange = 0.424109299625868; avgaccred = 0.565479066167824; break;
                case "bf3XP2 HK417": avgaccyellow = 0.46796813981925; avgaccorange = 0.5615617677831; avgaccred = 0.7487490237108; break;
                case "bf3XP2 JNG90": avgaccyellow = 0.778011878609245; avgaccorange = 0.933614254331094; avgaccred = 1.24481900577479; break;
                case "bf3XP2 L86A1": avgaccyellow = 0.283206275870145; avgaccorange = 0.339847531044174; avgaccred = 0.453130041392232; break;
                case "bf3XP2 LSAT": avgaccyellow = 0.26624799173021; avgaccorange = 0.319497590076252; avgaccred = 0.425996786768336; break;
                case "bf3XP2 MP5K": avgaccyellow = 0.35935686327564; avgaccorange = 0.431228235930768; avgaccred = 0.574970981241024; break;
                case "bf3XP2 MTAR-21": avgaccyellow = 0.319187274508933; avgaccorange = 0.383024729410719; avgaccred = 0.510699639214292; break;
                case "bf3XP2 SCARL": avgaccyellow = 0.38394888250267; avgaccorange = 0.460738659003204; avgaccred = 0.614318212004272; break;
                case "bf3XP2 SPAS12": avgaccyellow = 1.75123652284402; avgaccorange = 2.10148382741282; avgaccred = 2.80197843655043; break;
                case "bf440MM": avgaccyellow = 1.00274487739705; avgaccorange = 1.20329385287646; avgaccred = 1.60439180383528; break;
                case "bf440MM_3GL": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf440MM_FLASH": avgaccyellow = 1.8262567830258; avgaccorange = 2.19150813963096; avgaccred = 2.92201085284128; break;
                case "bf440MM_LVG": avgaccyellow = 1.36165955320032; avgaccorange = 1.63399146384038; avgaccred = 2.17865528512051; break;
                case "bf440MM_SHG": avgaccyellow = 2.21475723185069; avgaccorange = 2.65770867822083; avgaccred = 3.5436115709611; break;
                case "bf440MM_SMK": avgaccyellow = 0.709625668449195; avgaccorange = 0.851550802139034; avgaccred = 1.13540106951871; break;
                case "bf4870": avgaccyellow = 1.94617691622379; avgaccorange = 2.33541229946855; avgaccred = 3.11388306595807; break;
                case "bf493R": avgaccyellow = 0.41759029627198; avgaccorange = 0.501108355526376; avgaccred = 0.668144474035168; break;
                case "bf4A91": avgaccyellow = 0.338644890123417; avgaccorange = 0.406373868148101; avgaccred = 0.541831824197468; break;
                case "bf4AAMINE": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4ACB90": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4ACR": avgaccyellow = 0.30978779829971; avgaccorange = 0.371745357959652; avgaccred = 0.495660477279536; break;
                case "bf4AEK971": avgaccyellow = 0.308368536127095; avgaccorange = 0.370042243352514; avgaccred = 0.493389657803352; break;
                case "bf4AK12": avgaccyellow = 0.322690981224967; avgaccorange = 0.387229177469961; avgaccred = 0.516305569959948; break;
                case "bf4AK5C": avgaccyellow = 0.342847068828565; avgaccorange = 0.411416482594278; avgaccred = 0.548555310125704; break;
                case "bf4AKU12": avgaccyellow = 0.355985611319888; avgaccorange = 0.427182733583865; avgaccred = 0.56957697811182; break;
                case "bf4AMR2": avgaccyellow = 0.657781924126343; avgaccorange = 0.789338308951611; avgaccred = 1.05245107860215; break;
                case "bf4AMR2CQB": avgaccyellow = 1.01849882710798; avgaccorange = 1.22219859252958; avgaccred = 1.62959812337277; break;
                case "bf4AMR2MID": avgaccyellow = 0.921463636420717; avgaccorange = 1.10575636370486; avgaccred = 1.47434181827315; break;
                case "bf4AR160": avgaccyellow = 0.34879966096647; avgaccorange = 0.418559593159764; avgaccred = 0.558079457546352; break;
                case "bf4ASVAL": avgaccyellow = 0.31154869798557; avgaccorange = 0.373858437582684; avgaccred = 0.498477916776912; break;
                case "bf4AWS": avgaccyellow = 0.255284962954315; avgaccorange = 0.306341955545178; avgaccred = 0.408455940726904; break;
                case "bf4BAYONETT": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4BPKNIFE1": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4BPKNIFE2": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4BPKNIFE3": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4BPKNIFE4": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4BPKNIFE5": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4BPKNIFE6": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4BPKNIFE7": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4BPKNIFE8": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4BULLDOG": avgaccyellow = 0.341219647384592; avgaccorange = 0.409463576861511; avgaccred = 0.545951435815348; break;
                case "bf4C4": avgaccyellow = 0.616541494108612; avgaccorange = 0.739849792930335; avgaccred = 0.98646639057378; break;
                case "bf4CBJMS": avgaccyellow = 0.36468651391519; avgaccorange = 0.437623816698228; avgaccred = 0.583498422264304; break;
                case "bf4CLAYMORE": avgaccyellow = 0.722857596411965; avgaccorange = 0.867429115694358; avgaccred = 1.15657215425914; break;
                case "bf4CS5": avgaccyellow = 0.764150137122592; avgaccorange = 0.916980164547111; avgaccred = 1.22264021939615; break;
                case "bf4CSLR4": avgaccyellow = 0.69749411422616; avgaccorange = 0.836992937071392; avgaccred = 1.11599058276186; break;
                case "bf4CZ75": avgaccyellow = 0.52158296718889; avgaccorange = 0.625899560626668; avgaccred = 0.834532747502224; break;
                case "bf4CZ805": avgaccyellow = 0.371937316209577; avgaccorange = 0.446324779451493; avgaccred = 0.595099705935324; break;
                case "bf4DAO12": avgaccyellow = 1.34956496387348; avgaccorange = 1.61947795664818; avgaccred = 2.15930394219757; break;
                case "bf4DBV12": avgaccyellow = 1.34261742480232; avgaccorange = 1.61114090976278; avgaccred = 2.1481878796837; break;
                case "bf4DEAGLE": avgaccyellow = 0.61719600203763; avgaccorange = 0.740635202445156; avgaccred = 0.987513603260208; break;
                case "bf4DEFIB": avgaccyellow = 0.0629096015247737; avgaccorange = 0.0754915218297285; avgaccred = 0.100655362439638; break;
                case "bf4DIVERKNIFE": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4EOD": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4F2000": avgaccyellow = 0.329029986269205; avgaccorange = 0.394835983523046; avgaccred = 0.526447978030728; break;
                case "bf4FAMAS": avgaccyellow = 0.337804374151757; avgaccorange = 0.405365248982109; avgaccred = 0.540486998642812; break;
                case "bf4FGM148": avgaccyellow = 0.910854271734062; avgaccorange = 1.09302512608087; avgaccred = 1.4573668347745; break;
                case "bf4FIM92": avgaccyellow = 0.46449581520237; avgaccorange = 0.557394978242844; avgaccred = 0.743193304323792; break;
                case "bf4FLARE": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4FLASHBANG": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4FN57": avgaccyellow = 0.525433612419338; avgaccorange = 0.630520334903205; avgaccred = 0.84069377987094; break;
                case "bf4FYJS": avgaccyellow = 0.854567622592327; avgaccorange = 1.02548114711079; avgaccred = 1.36730819614772; break;
                case "bf4G36C": avgaccyellow = 0.368628816471373; avgaccorange = 0.442354579765647; avgaccred = 0.589806106354196; break;
                case "bf4GALIL21": avgaccyellow = 0.34493119029052; avgaccorange = 0.413917428348624; avgaccred = 0.551889904464832; break;
                case "bf4GALIL23": avgaccyellow = 0.348782026540135; avgaccorange = 0.418538431848162; avgaccred = 0.558051242464216; break;
                case "bf4GALIL52": avgaccyellow = 0.31563094205318; avgaccorange = 0.378757130463816; avgaccred = 0.505009507285088; break;
                case "bf4GALIL53": avgaccyellow = 0.508090272916755; avgaccorange = 0.609708327500106; avgaccred = 0.812944436666808; break;
                case "bf4GLOCK18": avgaccyellow = 0.377837874389947; avgaccorange = 0.453405449267937; avgaccred = 0.604540599023916; break;
                case "bf4GOL": avgaccyellow = 0.805285403355273; avgaccorange = 0.966342484026327; avgaccred = 1.28845664536844; break;
                case "bf4HAWK": avgaccyellow = 2.29962650422183; avgaccorange = 2.75955180506619; avgaccred = 3.67940240675492; break;
                case "bf4HK45C": avgaccyellow = 0.483457361561497; avgaccorange = 0.580148833873797; avgaccred = 0.773531778498396; break;
                case "bf4IGLA": avgaccyellow = 0.442419150194925; avgaccorange = 0.53090298023391; avgaccred = 0.70787064031188; break;
                case "bf4IMPACT": avgaccyellow = 0.809592714997407; avgaccorange = 0.971511257996889; avgaccred = 1.29534834399585; break;
                case "bf4JNG90": avgaccyellow = 0.767097174181213; avgaccorange = 0.920516609017455; avgaccred = 1.22735547868994; break;
                case "bf4JS2": avgaccyellow = 0.327525596124917; avgaccorange = 0.393030715349901; avgaccred = 0.524040953799868; break;
                case "bf4KNIFE14100BT": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4KNIFE2142": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4KNIFEPRECISION": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4L85A2": avgaccyellow = 0.361048505713275; avgaccorange = 0.43325820685593; avgaccred = 0.57767760914124; break;
                case "bf4L96A1": avgaccyellow = 0.77006520612561; avgaccorange = 0.924078247350732; avgaccred = 1.23210432980098; break;
                case "bf4LSAT": avgaccyellow = 0.25406425812468; avgaccorange = 0.304877109749616; avgaccred = 0.406502812999488; break;
                case "bf4M1014": avgaccyellow = 1.37559143780038; avgaccorange = 1.65070972536046; avgaccred = 2.20094630048061; break;
                case "bf4M136": avgaccyellow = 0.84070796460177; avgaccorange = 1.00884955752212; avgaccred = 1.34513274336283; break;
                case "bf4M15": avgaccyellow = 0.348405040186215; avgaccorange = 0.418086048223458; avgaccred = 0.557448064297944; break;
                case "bf4M16A4": avgaccyellow = 0.364698244913812; avgaccorange = 0.437637893896575; avgaccred = 0.5835171918621; break;
                case "bf4M18": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4M1911": avgaccyellow = 0.533649462299015; avgaccorange = 0.640379354758818; avgaccred = 0.853839139678424; break;
                case "bf4M2": avgaccyellow = 0.468569246726738; avgaccorange = 0.562283096072085; avgaccred = 0.74971079476278; break;
                case "bf4M200": avgaccyellow = 0.767988634391335; avgaccorange = 0.921586361269602; avgaccred = 1.22878181502614; break;
                case "bf4M240": avgaccyellow = 0.226687730844742; avgaccorange = 0.27202527701369; avgaccred = 0.362700369351587; break;
                case "bf4M249": avgaccyellow = 0.21618158024717; avgaccorange = 0.259417896296603; avgaccred = 0.345890528395471; break;
                case "bf4M26_FLECHETTE": avgaccyellow = 2.47881452245203; avgaccorange = 2.97457742694243; avgaccred = 3.96610323592324; break;
                case "bf4M26_FRAG": avgaccyellow = 0.741195346595438; avgaccorange = 0.889434415914525; avgaccred = 1.1859125545527; break;
                case "bf4M26_MASS": avgaccyellow = 2.35548799804286; avgaccorange = 2.82658559765143; avgaccred = 3.76878079686857; break;
                case "bf4M26_SLUG": avgaccyellow = 0.878637649992618; avgaccorange = 1.05436517999114; avgaccred = 1.40582023998819; break;
                case "bf4M32MGL": avgaccyellow = 0.934407707123645; avgaccorange = 1.12128924854837; avgaccred = 1.49505233139783; break;
                case "bf4M34": avgaccyellow = 7.1062312268968; avgaccorange = 8.52747747227616; avgaccred = 11.3699699630349; break;
                case "bf4M39": avgaccyellow = 0.504926928463925; avgaccorange = 0.60591231415671; avgaccred = 0.80788308554228; break;
                case "bf4M40A5": avgaccyellow = 0.722525496073173; avgaccorange = 0.867030595287807; avgaccred = 1.15604079371708; break;
                case "bf4M412REX": avgaccyellow = 0.622856531534898; avgaccorange = 0.747427837841877; avgaccred = 0.996570450455836; break;
                case "bf4M416": avgaccyellow = 0.325757432297527; avgaccorange = 0.390908918757033; avgaccred = 0.521211891676044; break;
                case "bf4M4A1": avgaccyellow = 0.365208301299517; avgaccorange = 0.438249961559421; avgaccred = 0.584333282079228; break;
                case "bf4M60E4": avgaccyellow = 0.227559249723696; avgaccorange = 0.273071099668435; avgaccred = 0.364094799557913; break;
                case "bf4M67": avgaccyellow = 0.81520745998584; avgaccorange = 0.978248951983008; avgaccred = 1.30433193597734; break;
                case "bf4M82A3": avgaccyellow = 0.981285145267105; avgaccorange = 1.17754217432053; avgaccred = 1.57005623242737; break;
                case "bf4M82A3CQB": avgaccyellow = 0.770121453708343; avgaccorange = 0.924145744450011; avgaccred = 1.23219432593335; break;
                case "bf4M82A3MID": avgaccyellow = 0.678632052473715; avgaccorange = 0.814358462968458; avgaccred = 1.08581128395794; break;
                case "bf4M9": avgaccyellow = 0.485319525834198; avgaccorange = 0.582383431001037; avgaccred = 0.776511241334716; break;
                case "bf4M98B": avgaccyellow = 0.822749509859793; avgaccorange = 0.987299411831751; avgaccred = 1.31639921577567; break;
                case "bf4MACHETE": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4MBTLAW": avgaccyellow = 0.745070879154745; avgaccorange = 0.894085054985694; avgaccred = 1.19211340664759; break;
                case "bf4MG4": avgaccyellow = 0.231787927434146; avgaccorange = 0.278145512920976; avgaccred = 0.370860683894634; break;
                case "bf4MK11": avgaccyellow = 0.496173129511055; avgaccorange = 0.595407755413266; avgaccred = 0.793877007217688; break;
                case "bf4MORTAR": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4MP443": avgaccyellow = 0.490627605946398; avgaccorange = 0.588753127135677; avgaccred = 0.785004169514236; break;
                case "bf4MP7": avgaccyellow = 0.324015625004335; avgaccorange = 0.388818750005202; avgaccred = 0.518425000006936; break;
                case "bf4MPX": avgaccyellow = 0.355479790799603; avgaccorange = 0.426575748959523; avgaccred = 0.568767665279364; break;
                case "bf4MTAR21": avgaccyellow = 0.306673271009745; avgaccorange = 0.368007925211694; avgaccred = 0.490677233615592; break;
                case "bf4MX4": avgaccyellow = 0.30178702589948; avgaccorange = 0.362144431079376; avgaccred = 0.482859241439168; break;
                case "bf4NECKKNIFE": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4P226": avgaccyellow = 0.477809473502202; avgaccorange = 0.573371368202643; avgaccred = 0.764495157603524; break;
                case "bf4P90": avgaccyellow = 0.306675657757302; avgaccorange = 0.368010789308763; avgaccred = 0.490681052411684; break;
                case "bf4PDR": avgaccyellow = 0.357823154271708; avgaccorange = 0.429387785126049; avgaccred = 0.572517046834732; break;
                case "bf4PECHENEG": avgaccyellow = 0.226317999573719; avgaccorange = 0.271581599488463; avgaccred = 0.362108799317951; break;
                case "bf4PP2000": avgaccyellow = 0.3465085417058; avgaccorange = 0.41581025004696; avgaccred = 0.55441366672928; break;
                case "bf4QBB95": avgaccyellow = 0.2789891810559; avgaccorange = 0.33478701726708; avgaccred = 0.44638268968944; break;
                case "bf4QBS09": avgaccyellow = 1.51717653455099; avgaccorange = 1.82061184146118; avgaccred = 2.42748245528158; break;
                case "bf4QBU88": avgaccyellow = 0.542651720142125; avgaccorange = 0.65118206417055; avgaccred = 0.8682427522274; break;
                case "bf4QBZ951": avgaccyellow = 0.37219065990564; avgaccorange = 0.446628791886768; avgaccred = 0.595505055849024; break;
                case "bf4QSZ92": avgaccyellow = 0.52389359608668; avgaccorange = 0.628672315304016; avgaccred = 0.838229753738688; break;
                case "bf4REPAIR": avgaccyellow = 1.45470743989314; avgaccorange = 1.74564892787177; avgaccred = 2.32753190382902; break;
                case "bf4RFBTARGET": avgaccyellow = 0.478986629535695; avgaccorange = 0.574783955442834; avgaccred = 0.766378607257112; break;
                case "bf4RPG7": avgaccyellow = 0.77276579972525; avgaccorange = 0.9273189596703; avgaccred = 1.2364252795604; break;
                case "bf4RPK12": avgaccyellow = 0.289334793063783; avgaccorange = 0.347201751676539; avgaccred = 0.462935668902052; break;
                case "bf4RPK74": avgaccyellow = 0.298061285361965; avgaccorange = 0.357673542434358; avgaccred = 0.476898056579144; break;
                case "bf4SAIGA12": avgaccyellow = 1.34044460978485; avgaccorange = 1.60853353174182; avgaccred = 2.14471137565576; break;
                case "bf4SAR21": avgaccyellow = 0.368269514491572; avgaccorange = 0.441923417389887; avgaccred = 0.589231223186516; break;
                case "bf4SCARH": avgaccyellow = 0.31805496500096; avgaccorange = 0.381665958001152; avgaccred = 0.508887944001536; break;
                case "bf4SCARHSV": avgaccyellow = 0.490289581255668; avgaccorange = 0.588347497506801; avgaccred = 0.784463330009068; break;
                case "bf4SCORP": avgaccyellow = 0.323627016257657; avgaccorange = 0.388352419509189; avgaccred = 0.517803226012252; break;
                case "bf4SCOUTELIT": avgaccyellow = 0.836886732021807; avgaccorange = 1.00426407842617; avgaccred = 1.33901877123489; break;
                case "bf4SG553": avgaccyellow = 0.325658493819697; avgaccorange = 0.390790192583637; avgaccred = 0.521053590111516; break;
                case "bf4SHANK": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4SHIELD": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4SHORTY": avgaccyellow = 1.87756760154445; avgaccorange = 2.25308112185334; avgaccred = 3.00410816247112; break;
                case "bf4SKS": avgaccyellow = 0.453506166120053; avgaccorange = 0.544207399344063; avgaccred = 0.725609865792084; break;
                case "bf4SMAW": avgaccyellow = 0.806166520233355; avgaccorange = 0.967399824280026; avgaccred = 1.28986643237337; break;
                case "bf4SPAS12": avgaccyellow = 1.973587867474; avgaccorange = 2.3683054409688; avgaccred = 3.1577405879584; break;
                case "bf4SR2": avgaccyellow = 0.30938903967393; avgaccorange = 0.371266847608716; avgaccred = 0.495022463478288; break;
                case "bf4SR338": avgaccyellow = 0.631469620220428; avgaccorange = 0.757763544264513; avgaccred = 1.01035139235268; break;
                case "bf4SRAW": avgaccyellow = 0.780154971575237; avgaccorange = 0.936185965890285; avgaccred = 1.24824795452038; break;
                case "bf4SRS": avgaccyellow = 0.802205257129245; avgaccorange = 0.962646308555094; avgaccred = 1.28352841140679; break;
                case "bf4STARSTREAK": avgaccyellow = 2.8820556041404; avgaccorange = 3.45846672496848; avgaccred = 4.61128896662464; break;
                case "bf4STEYRAUG": avgaccyellow = 0.361286881447412; avgaccorange = 0.433544257736895; avgaccred = 0.57805901031586; break;
                case "bf4SV98": avgaccyellow = 0.7870294537514; avgaccorange = 0.94443534450168; avgaccred = 1.25924712600224; break;
                case "bf4SVD12": avgaccyellow = 0.509411280604148; avgaccorange = 0.611293536724977; avgaccred = 0.815058048966636; break;
                case "bf4SW40": avgaccyellow = 0.592184727702838; avgaccorange = 0.710621673243405; avgaccred = 0.94749556432454; break;
                case "bf4TAURUS44": avgaccyellow = 0.713383840776037; avgaccorange = 0.856060608931245; avgaccred = 1.14141414524166; break;
                case "bf4TYPE88": avgaccyellow = 0.215304367250052; avgaccorange = 0.258365240700063; avgaccred = 0.344486987600084; break;
                case "bf4TYPE95B1": avgaccyellow = 0.36294232880912; avgaccorange = 0.435530794570944; avgaccred = 0.580707726094592; break;
                case "bf4UCAV": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;
                case "bf4ULTIM": avgaccyellow = 0.286430482635275; avgaccorange = 0.34371657916233; avgaccred = 0.45828877221644; break;
                case "bf4UMP45": avgaccyellow = 0.346495745817193; avgaccorange = 0.415794894980631; avgaccred = 0.554393193307508; break;
                case "bf4UMP9": avgaccyellow = 0.386693686210925; avgaccorange = 0.46403242345311; avgaccred = 0.61870989793748; break;
                case "bf4UN6": avgaccyellow = 0.647659820393808; avgaccorange = 0.777191784472569; avgaccred = 1.03625571263009; break;
                case "bf4USAS12": avgaccyellow = 0.920120598502375; avgaccorange = 1.10414471820285; avgaccred = 1.4721929576038; break;
                case "bf4USAS12NV": avgaccyellow = 0.791866574892167; avgaccorange = 0.950239889870601; avgaccred = 1.26698651982747; break;
                case "bf4UTAS": avgaccyellow = 1.79540969910046; avgaccorange = 2.15449163892055; avgaccred = 2.87265551856074; break;
                case "bf4V40": avgaccyellow = 0.734818461871268; avgaccorange = 0.881782154245521; avgaccred = 1.17570953899403; break;
                case "bf4XM25": avgaccyellow = 0.726381682507415; avgaccorange = 0.871658019008898; avgaccred = 1.16221069201186; break;
                case "bf4XM25_FLECHETTE": avgaccyellow = 1.85571410338; avgaccorange = 2.226856924056; avgaccred = 2.969142565408; break;
                case "bf4XM25_SMK": avgaccyellow = 0; avgaccorange = 0; avgaccred = 0; break;

                default:
                    log("CD - weapon avg accuracy not found: " + weaponname, 1.5);
                    avgaccyellow = 100; avgaccorange = 100; avgaccred = 100;
                    break;
            }

            weaponname = FixWeaponName(weaponname);

            double acc = Math.Round(weaponacc * 100.0, 2);
            double accabove = Math.Round(weaponacc / (avgaccyellow/2.5), 2);
            
            double cheatindex = 0.0;
            if (kills > minimumkills)
            {                
                if (weaponacc > avgaccred)
                {
                    log("CD - IMPOSSIBLE STATS: " + playername + " " + weaponname + " accuracy: " + acc.ToString() + "%" + " (" + accabove.ToString() + ")", 1);
                    if (!reasons.ContainsKey(accabove))
                    {
                        reasons.Add(accabove, weaponname + " accuracy: " + acc.ToString() + "%");
                    }

                    cheatindex += redflagcount;
                }
                else if (weaponacc > avgaccorange)
                {
                    log("CD - SUSPICIOUS stats: " + playername + " " + weaponname + " accuracy: " + acc.ToString() + "%" + " (" + accabove.ToString() + ")", 1);
                    if (!reasons.ContainsKey(accabove))
                    {
                        reasons.Add(accabove, weaponname + " accuracy: " + acc.ToString() + "%");
                    }

                    cheatindex += orangeflagcount;
                }
                else if (weaponacc > avgaccyellow)
                {
                    log("CD - suspicious stats: " + playername + " " + weaponname + " accuracy: " + acc.ToString() + "%" + " (" + accabove.ToString() + ")", 1);
                    if (!reasons.ContainsKey(accabove))
                    {
                        reasons.Add(accabove, weaponname + " accuracy: " + acc.ToString() + "%");
                    }

                    cheatindex += yellowflagcount;
                }
            }
            else
            {
                log("CD - skipping weapon (less than " + minimumkills.ToString() + " kills): " + weaponname, 5);
            }
            log("CD - Accuracy: " + acc.ToString() + "%", 5);
            return cheatindex;
        }
        #endregion

        #region log
        public void log(string msg, double debuglvl)
        {
            if (debuglevel >= debuglvl)
            {
                ExecuteCommand("procon.protected.pluginconsole.write", msg);

                if (logtofile == enumBoolYesNo.Yes)
                {
                    try
                    {
                        string file = Path.Combine(Directory.GetParent(Application.ExecutablePath).FullName, logfilename);

                        if (!File.Exists(file))
                        {
                            File.Create(file);
                        }

                        using (FileStream fs = File.Open(file, FileMode.Append))
                        {
                            Byte[] bytes = new UTF8Encoding(true).GetBytes(DateTime.Now.ToString() + ": " + msg + Environment.NewLine);
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExecuteCommand("unable to append data to file " + ex.Message);
                    }
                }
            }
        }
        #endregion

        #region OnReservedSlotsList
        public override void OnReservedSlotsList(List<string> soldierNames)
        {
            if (syncreservedslots == enumBoolYesNo.Yes)
            {
                foreach (string vipplayer in soldierNames)
                {
                    if (!whitelist.Contains(vipplayer))
                    {
                        whitelist.Add(vipplayer);
                    }
                }
            //}
            //if (excludereservedslots == enumBoolYesNo.Yes)
            //{
                List<string> playerstoremove = new List<string>();
                foreach (string vipplayer in whitelist)
                {
                    if (!soldierNames.Contains(vipplayer))
                    {
                        playerstoremove.Add(vipplayer);
                    }
                }
                foreach (string player in playerstoremove)
                {
                    whitelist.Remove(player);
                }
            }
        }
        #endregion

        #region IsCacheEnabled
        public bool IsCacheEnabled(bool showMessage)
        {
            List<MatchCommand> registered = GetRegisteredCommands();
            foreach (MatchCommand command in registered)
            {
                if (useCache == enumBoolYesNo.Yes && command.RegisteredClassname.CompareTo("CBattlelogCache") == 0 && command.RegisteredMethodName.CompareTo("PlayerLookup") == 0)
                {
                    if (showMessage)
                    {
                        log("CD - Battlelog Cache plugin will be used for stats fetching.", 1);
                    }
                    return true;
                }
            }
            if (showMessage)
            {
                log("CD - Battlelog Cache plugin is disabled. Installing/updating and enabling the plugin is recommended, if you run other Plugins that require Battlelog Stats (Insane Limits, True Balancer).", 1);
            }
            return false;
        }
        #endregion

        #region GenerateRandomString
        public static string GenerateRandomString()
        {
            string path = Path.GetRandomFileName() + Path.GetRandomFileName() + Path.GetRandomFileName();
            path = path.Replace(".", ""); // Remove period.
            return path;
        }
        #endregion

        #region formatMsg
        private string formatMsg(string msg, string playername, string bestreason, string pbguid, string eaguid)
        {
            string returntext = msg;
            returntext = returntext.Replace("%suspiciousstat%", bestreason.ToString());
            returntext = returntext.Replace("%playername%", playername);
            returntext = returntext.Replace("%pbguid%", pbguid);
            returntext = returntext.Replace("%eaguid%", eaguid);
            return returntext;
        }
        #endregion

        #region OnPunkbusterPlayerInfo
        public override void OnPunkbusterPlayerInfo(CPunkbusterInfo playerInfo)
        {
            log("CD - getting PB Info for player: " + playerInfo.SoldierName, 5);
            if (!m_dicPBPlayerInfo.ContainsKey(playerInfo.SoldierName))
            {
                log("CD - adding PB Info for player: " + playerInfo.SoldierName, 4);
                m_dicPBPlayerInfo.Add(playerInfo.SoldierName, playerInfo);
            }
            else
            {
                log("CD - maintaining PB Info for player: " + playerInfo.SoldierName, 4);
                m_dicPBPlayerInfo[playerInfo.SoldierName]=playerInfo;
            }

            int i = 0;
            int index = -1;
            foreach (PlayerCheckInfo pci in queuefordb)
            {
                if (pci.ToString() == playerInfo.SoldierName)
                {
                    index = i;
                }
                i++;
            }
            if (index > -1)
            {
                queuefordb[index].pbguid = playerInfo.GUID;
            }
        }
        #endregion

        #region backAdd
        private void backAdd(string playername)
        {
            lock (playerErrors)
            {
                if (!playerErrors.ContainsKey(playername))
                {
                    playerErrors.Add(playername, 1);
                }

                if (myplayerlist.Contains(playername) && playerErrors[playername] <= retries)
                {
                    log("CD - Error detected, checking player " + playername + " again later.", 2);
                    myplayerlist.Remove(playername);
                    playerErrors[playername]++;
                }
                else
                {
                    log("CD - NOT checking player " + playername + " again. Too many errors or not in playerlist anymore.", 3);
                }
            }
        }
        #endregion

        #region class BattlelogClient
        public class BattlelogClient
        {
            private CheatDetector plugin = null;

            public BattlelogClient(CheatDetector p)
            {
                plugin = p;
            }

            //private HttpWebRequest req = null;

            WebClient client = null;

            public String fetchWebPage(ref String html_data, String url)
            {
                try
                {
                    ////plugin.timeouttesthelper++;

                    //if (plugin.timeouttesthelper >= 5)
                    //{
                    //    plugin.timeouttesthelper = 0;
                    //    throw new WebException("timeout test");                        
                    //}

                    if (plugin.timeouttest == enumBoolYesNo.Yes)
                    {
                        //Random random = new Random();
                        //int blub = random.Next(1, 6);
                        //if (blub == 3)
                        throw new WebException("timeout test");
                    }

                    if (client == null)
                    {
                        client = new WebClient();
                        client.Headers.Add("user-agent", plugin.UserAgent);
                    }

                    html_data = client.DownloadString(url);
                    return html_data;

                }
                catch (WebException e)
                {
                    if (e.Status.Equals(WebExceptionStatus.Timeout))
                    {
                        throw new Exception("HTTP request timed-out");
                    }
                    else
                    {
                        throw;
                    }

                }
            }
        }
        #endregion

        #region Exceptions
        public class StatsException : Exception
        {
            public StatsException(String message)
                : base(message)
            {
            }
        }
        #endregion

        public class DBConnect
        {
            private MySqlConnection connection;
            //private string server;
            //private string database;
            //private string uid;
            //private string password;
            private CheatDetector plugin;
            private bool connectionisopen = false;

            //Constructor
            public DBConnect(CheatDetector p)
            {
                plugin = p;
                Initialize(p.queueServer, p.queueDB, p.queueUser, p.queuePwd);
            }

            //Initialize values
            private void Initialize(string srv, string db, string user, string pwd)
            {
                string connectionString;
                connectionString = "SERVER=" + srv + ";" + "DATABASE=" + db + ";Connect Timeout=25;" + "UID=" + user + ";" + "PASSWORD=" + pwd + ";";

                connection = new MySqlConnection(connectionString);
            }

            //open connection to database
            private bool OpenConnection()
            {
                if (connectionisopen)
                {
                    return true;
                }

                try
                {
                    connection.Open();
                    connectionisopen = true;
                    return true;
                }
                catch (MySqlException ex)
                {
                    //When handling errors, you can your application's response based 
                    //on the error number.
                    //The two most common error numbers when connecting are as follows:
                    //0: Cannot connect to server.
                    //1045: Invalid user name and/or password.
                    switch (ex.Number)
                    {
                        case 0:
                            plugin.log("CD - Cannot connect to server.  Contact administrator", 1);
                            break;

                        case 1045:
                            plugin.log("CD - Invalid username/password, please try again", 1);
                            break;
                    }
                    return false;
                }
            }

            //Close connection
            private bool CloseConnection()
            {
                try
                {
                    connection.Close();
                    connectionisopen = false;
                    return true;
                }
                catch (MySqlException ex)
                {
                    plugin.log("CD - " + ex, 1);
                    return false;
                }
            }

            //Insert statement
            public bool Insert(PlayerCheckInfo[] p, string servername, string serveripandport, string servertype)
            {
                
                    try
                    {
                        if (p.Length > 0)
                        {
                            plugin.log("CD - adding players to master queue", 3);

                            string tempqueryfordb = "INSERT INTO cd_queue (name, eaguid, pbguid, firstseen, lastseen, prio, lastservername, lastserveripandport, lastservertype) VALUES ";

                            int i = 0;
                            foreach (PlayerCheckInfo player in p)
                            {
                                i++;
                                tempqueryfordb += "('" + MySqlHelper.EscapeString(player.name) + "', '" + player.eaguid + "', '" + player.pbguid + "', '" + DateTime.UtcNow.AddHours(1).ToString("yyyy.MM.dd HH:mm:ss") + "', '" + DateTime.UtcNow.AddHours(1).ToString("yyyy.MM.dd HH:mm:ss") + "', '" + player._prio.ToString() + "', '" + MySqlHelper.EscapeString(servername) + "', '" + MySqlHelper.EscapeString(serveripandport) + "', '" + MySqlHelper.EscapeString(servertype) + "')";
                                if (i < p.Length)
                                {
                                    tempqueryfordb += ", ";
                                }
                            }

                            tempqueryfordb += " ON DUPLICATE KEY UPDATE lastseen=VALUES(lastseen), prio=VALUES(prio), lastservername=VALUES(lastservername), lastserveripandport=VALUES(lastserveripandport), lastservertype=VALUES(lastservertype)";

                            if (RunQuery(tempqueryfordb))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {                            
                            plugin.log("CD - nothing to do for master queue", 3);
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        plugin.log("CD - ERROR DB.Insert(): " + ex, 1);
                        return false;
                    }
                
            }

            ////Update statement
            //public bool Update(MyPlayerinfo p)
            //{
            //    try
            //    {
            //        string query = "UPDATE awards SET time='" + p.time.ToString(new System.Globalization.CultureInfo("en-US")) + "', isVIP='" + (p.isVIP ? "1" : "0") + "', kicks='" + p.kicks.ToString() + "', tbans='" + p.tbans.ToString() + "', bans='" + p.bans.ToString() + "', reset='" + p.reset.ToString() + "' WHERE name='" + p.name.ToString() + "'";

            //        //Open connection
            //        if (this.OpenConnection() == true)
            //        {
            //            //create mysql command
            //            MySqlCommand cmd = new MySqlCommand();
            //            //Assign the query using CommandText
            //            cmd.CommandText = query;
            //            //Assign the connection using Connection
            //            cmd.Connection = connection;

            //            //Execute query
            //            cmd.ExecuteNonQuery();

            //            //close connection
            //            this.CloseConnection();
            //        }
            //        return true;
            //    }
            //    catch (Exception ex)
            //    {
            //        plugin.log("CD - ERROR DB.Update(): " + ex, 1);
            //        return false;
            //    }
            //}

            public bool RunQuery(string query)
            {
                try
                {
                    //Open connection
                    if (this.OpenConnection() == true)
                    {
                        //create mysql command
                        MySqlCommand cmd = new MySqlCommand();
                        //Assign the query using CommandText
                        cmd.CommandText = query;
                        //Assign the connection using Connection
                        cmd.Connection = connection;

                        cmd.CommandTimeout = 25;

                        //Execute query
                        cmd.ExecuteNonQuery();

                        //close connection
                        this.CloseConnection();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    plugin.log("CD - ERROR DB.RunQuery(): " + ex, 1);
                    return false;
                }
            }

            ////Delete statement
            ////public void Delete(MyPlayerinfo p)
            ////{
            ////}

            ////Select statement
            //public MyPlayerinfo GetPlayerStats(string p)
            //{
            //    MyPlayerinfo stats = new MyPlayerinfo();
            //    stats.successfullDBFetch = false;
            //    try
            //    {
            //        string query = "SELECT * FROM awards WHERE name='" + p + "'";

            //        //Open connection
            //        if (this.OpenConnection() == true)
            //        {
            //            //Create Command
            //            MySqlCommand cmd = new MySqlCommand(query, connection);
            //            //Create a data reader and Execute the command
            //            MySqlDataReader dataReader = cmd.ExecuteReader();

            //            //Read the data and store them in the list
            //            while (dataReader.Read())
            //            {
            //                stats.name = p;
            //                stats.time = Convert.ToDouble(dataReader["time"]);
            //                stats.isVIP = Convert.ToBoolean(dataReader["isVIP"]);
            //                stats.kills = Convert.ToInt32(dataReader["kills"]);
            //                stats.moves = Convert.ToInt32(dataReader["moves"]);
            //                stats.kicks = Convert.ToInt32(dataReader["kicks"]);
            //                stats.tbans = Convert.ToInt32(dataReader["tbans"]);
            //                stats.bans = Convert.ToInt32(dataReader["bans"]);
            //                stats.reset = Convert.ToDateTime(dataReader["reset"]);
            //                //stats.resetRightsby = Convert.ToDateTime(dataReader["resetRightsby"]);
            //                stats.successfullDBFetch = true;
            //            }

            //            //close Data Reader
            //            dataReader.Close();

            //            //close Connection
            //            this.CloseConnection();

            //            //return list to be displayed
            //            return stats;
            //        }
            //        else
            //        {
            //            return stats;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        plugin.log("CD - ERROR DB.GetPlayerStats(): " + ex, 1);
            //        return stats;
            //    }
            //}

            //public object GetSetting(string p)
            //{
            //    object value = "";
            //    try
            //    {
            //        string query = "SELECT * FROM awards_settings WHERE name='" + p + "'";

            //        //Open connection
            //        if (this.OpenConnection() == true)
            //        {
            //            //Create Command
            //            MySqlCommand cmd = new MySqlCommand(query, connection);
            //            //Create a data reader and Execute the command
            //            MySqlDataReader dataReader = cmd.ExecuteReader();

            //            //Read the data and store them in the list
            //            while (dataReader.Read())
            //            {

            //                value = dataReader["value"];

            //            }

            //            //close Data Reader
            //            dataReader.Close();

            //            //close Connection
            //            this.CloseConnection();

            //            //return list to be displayed
            //            return value;
            //        }
            //        else
            //        {
            //            return value;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        plugin.log("CD - ERROR DB.GetPlayerStats(): " + ex, 1);
            //        return value;
            //    }
            //}

            ////Count statement
            //public bool PlayerExists(string p)
            //{
            //    try
            //    {
            //        string query = "SELECT Count(*) FROM awards WHERE name='" + p + "'";
            //        int Count = -1;

            //        //Open Connection
            //        if (this.OpenConnection() == true)
            //        {
            //            //Create Mysql Command
            //            MySqlCommand cmd = new MySqlCommand(query, connection);

            //            //ExecuteScalar will return one value
            //            Count = int.Parse(cmd.ExecuteScalar() + "");

            //            //close Connection
            //            this.CloseConnection();

            //            return Count > 0;
            //        }
            //        else
            //        {
            //            return false;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        plugin.log("CD - ERROR DB.PlayerExists() " + ex, 1);
            //        return false;
            //    }
            //}

            ////Backup
            ////public void Backup()
            ////{
            ////}

            //////Restore
            ////public void Restore()
            ////{
            ////}
        }

        public class PlayerCheckInfo
        {            
            public string name;
            public string eaguid;
            public string pbguid;
            public int _prio;

            public PlayerCheckInfo(string n, string eag, string pbg, int prio)
            {                
                name = n;
                eaguid = eag;
                pbguid = pbg;
                _prio = prio;
            }

            public string ToString()
            {
                return name;
            }
        }
                
    }


}
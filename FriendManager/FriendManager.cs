using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

using Microsoft.CSharp;

using PRoCon.Core;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Players;
using PRoCon.Core.Plugin;

namespace PRoConEvents
{
    /// <summary>
    /// Friend Manager.
    /// </summary>
    public class FriendManager : PRoConPluginAPI, IPRoConPluginInterface
    {
        private bool isEnable = false;

        #region Menu List

        public const string FriendHeader = "1 Friend";
        public const string EnemyHeader = "2 Enemy";
        public const string SettingHeader = "3 Setting";
        public const string UpdateHeader = "4 Update";

        [Menu(UpdateHeader, "Update Warning!!")]
        private readonly string updateWarning = "need restart remote procon to take effect.";

        [Menu(FriendHeader, "Friend ClanTag")]
        private string[] friendClanTag = new string[0];

        [Menu(FriendHeader, "Enable Friend List")]
        private enumBoolOnOff friendEnable = enumBoolOnOff.On;

        [Menu(FriendHeader, "Friend GUID")]
        private string[] friendGUID = new string[0];

        [Menu(FriendHeader, "Friend SoldierName")]
        private string[] friendSoldierName = new string[] { "IOL0ol1" };

        [Menu(FriendHeader, "Friend Forces the Move")]
        private bool friendForcesMove = false;

        [Menu(EnemyHeader, "Enemy ClanTag")]
        private string[] enemyClanTag = new string[0];

        [Menu(EnemyHeader, "Enable Enemy List")]
        private enumBoolOnOff enemyEnable = enumBoolOnOff.Off;

        [Menu(EnemyHeader, "Enemy GUID")]
        private string[] enemyGUID = new string[0];

        [Menu(EnemyHeader, "Enemy SoldierName")]
        private string[] enemySoldierName = new string[0];

        [Menu(FriendHeader, "Enemy Forces the Move")]
        private bool enemyForcesMove = false;

        [Menu(SettingHeader, "Enemy Team")]
        private int enemyTeamID = 0;

        [Menu(SettingHeader, "Friend Team")]
        private int friendTeamID = 1;

        [Menu(SettingHeader, "Include VIP/Reserved Slots List into the friendlist?")]
        private enumBoolYesNo vipIsFriend = enumBoolYesNo.Yes;

        [Menu(SettingHeader, "Bool Test")]
        private bool boolValue = false;

        [Menu(UpdateHeader, "Source Uri")]
        private string sourceFileUri = "https://gitee.com/e1ki0lp/ProconPlugins/raw/master/PRoConEvents/FriendManager.cs";

        [Menu(UpdateHeader, "Update Plugin")]
        private enumBoolYesNo updatePlugin = enumBoolYesNo.No;

        [Menu(UpdateHeader, "Confirm Update")]
        private enumBoolYesNo confirmUpdate = enumBoolYesNo.No;

        private List<CPluginVariable> GetVariables(bool isDisplay)
        {
            List<CPluginVariable> pluginVariables = new List<CPluginVariable>();

            pluginVariables.Add(CreateVariable(() => friendEnable, isDisplay));
            if (friendEnable == enumBoolOnOff.On)
            {
                pluginVariables.Add(CreateVariable(() => friendSoldierName, isDisplay));
                pluginVariables.Add(CreateVariable(() => friendGUID, isDisplay));
                pluginVariables.Add(CreateVariable(() => friendClanTag, isDisplay));
            }

            pluginVariables.Add(CreateVariable(() => enemyEnable, isDisplay));
            if (enemyEnable == enumBoolOnOff.On)
            {
                pluginVariables.Add(CreateVariable(() => enemySoldierName, isDisplay));
                pluginVariables.Add(CreateVariable(() => enemyGUID, isDisplay));
                pluginVariables.Add(CreateVariable(() => enemyClanTag, isDisplay));
            }

            pluginVariables.Add(CreateVariable(() => vipIsFriend, isDisplay));
            pluginVariables.Add(CreateVariable(() => friendTeamID, isDisplay));
            pluginVariables.Add(CreateVariable(() => enemyTeamID, isDisplay));
            pluginVariables.Add(CreateVariable(() => boolValue, isDisplay));

            pluginVariables.Add(CreateVariable(() => updateWarning, isDisplay));
            pluginVariables.Add(CreateVariable(() => sourceFileUri, isDisplay));
            pluginVariables.Add(CreateVariable(() => updatePlugin, isDisplay));
            if (updatePlugin == enumBoolYesNo.Yes)
            {
                pluginVariables.Add(CreateVariable(() => confirmUpdate, isDisplay));
            }

            return pluginVariables;
        }

        #endregion

        #region IPRoConPluginInterface

        public List<CPluginVariable> GetDisplayPluginVariables()
        {
            sourceFileUri = sourceFileUri.Replace('\\', '/');

            if (updatePlugin == enumBoolYesNo.Yes && confirmUpdate == enumBoolYesNo.Yes)
            {
                updatePlugin = enumBoolYesNo.No;
                confirmUpdate = enumBoolYesNo.No;
                if (isEnable)
                {
                    UpdatePlugin(sourceFileUri);
                }
                else
                {
                    Output.TraceWarning("Enable plugin to use update function!");
                }
            }

            return GetVariables(true);
        }

        public string GetPluginAuthor()
        {
            return "IOL0ol1";
        }

        public string GetPluginDescription()
        {
            return @"
<h2>&#x000A;Description</h2>&#x000A;
<p>This is a plugin for team work in the game.</p>&#x000A;
<h2>&#x000A;Menu</h2>&#x000A;
<blockquote>&#x000A;<p>Friend</p>&#x000A;</blockquote>&#x000A;
<blockquote>&#x000A;<p>Enemy</p>&#x000A;</blockquote>&#x000A;
<blockquote>&#x000A;<p>Setting</p>&#x000A;</blockquote>&#x000A;
<blockquote>&#x000A;<p>Update</p>&#x000A;</blockquote>&#x000A;
<blockquote>&#x000A;<p>Restart</p>&#x000A;</blockquote>&#x000A;
<h2>&#x000A;History</h2>&#x000A;
<p>TODO</p>";
        }

        public string GetPluginName()
        {
            return ClassName;
        }

        public List<CPluginVariable> GetPluginVariables()
        {
            return GetVariables(false);
        }

        public string GetPluginVersion()
        {
            return "0.0.2.1";
        }

        public string GetPluginWebsite()
        {
            return "gitee.com/e1ki0lp/ProconPlugins";
        }

        public void OnPluginDisable()
        {
            isEnable = false;
            Output.TraceInformation(string.Format("^b{0} {1} ^1Disabled", GetPluginName(), GetPluginVersion()));
        }

        public void OnPluginEnable()
        {
            Output.TraceInformation(string.Format("^b{0} {1} ^2Enabled", GetPluginName(), GetPluginVersion()));
            //Command("admin.listPlayers", "all");
            //Command("reservedSlotsList.list");
            //RegisterAllCommands();
            isEnable = true;
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
        {
            Output.Listeners.Add(new TextWriterTraceListener(VerifyDirectory("Log/{0}_{1}/{2}.log", strHostName, strPort, ClassName))); // output to debug file
            Output.Listeners.Add(new PRoConTraceListener(this)); // output to pluginconsole
            Output.AutoFlush = true;

            // Get and register common events in this class and PRoConPluginAPI
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            IEnumerable<string> baseMethods = typeof(PRoConPluginAPI).GetMethods().Where(_ => _.IsVirtual).Select(_ => _.Name);
            IEnumerable<string> commonMethods = GetType().GetMethods(bindingFlags).Where(_ => _.IsVirtual).Select(_ => _.Name).Intersect(baseMethods);
            RegisterEvents(ClassName, commonMethods.ToArray());
        }

        public void SetPluginVariable(string strVariable, string strValue)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            // search field to set value
            foreach (FieldInfo item in GetType().GetFields(bindingFlags))
            {
                MenuAttribute menu = item.GetCustomAttributes(false).FirstOrDefault(_ => _ is MenuAttribute) as MenuAttribute;
                if (menu != null && menu.Name == strVariable)
                {
                    if (item.IsInitOnly) return; // if it's readonly field, do not set value.
                    object value = strValue;
                    if (item.FieldType.BaseType == typeof(Enum))
                    {
                        value = Enum.Parse(item.FieldType, strValue);
                    }
                    else if (item.FieldType == typeof(string[]))
                    {
                        value = strValue.Split('|');
                    }
                    else
                    {
                        value = Convert.ChangeType(strValue, item.FieldType);
                    }

                    item.SetValue(this, value);
                    return;
                }
            }
        }

        #endregion

        #region Event

        public override void OnLoadingLevel(string mapFileName, int roundsPlayed, int roundsTotal)
        {
            base.OnLoadingLevel(mapFileName, roundsPlayed, roundsTotal);
            Output.TraceInformation("OnLoadingLevel {0} {1} {2}", mapFileName, roundsPlayed, roundsTotal);
        }

        public override void OnLevelLoaded(string mapFileName, string gamemode, int roundsPlayed, int roundsTotal)
        {
            base.OnLevelLoaded(mapFileName, gamemode, roundsPlayed, roundsTotal);
            Output.TraceInformation("OnLevelLoaded {0} {1} {2}", mapFileName, roundsPlayed, roundsTotal);
        }

        public override void OnLevelStarted()
        {
            base.OnLevelStarted();
            Output.TraceInformation("OnLevelStarted");
        }

        public override void OnRunNextLevel()
        {
            base.OnRunNextLevel();
            Output.TraceInformation("OnRunNextLevel");
        }

        public override void OnRestartLevel()
        {
            base.OnRestartLevel();
            Output.TraceInformation("OnRestartLevel");
        }

        public override void OnEndRound(int iWinningTeamID)
        {
            base.OnEndRound(iWinningTeamID);
            Output.TraceInformation("OnEndRound {0}", iWinningTeamID);
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

        public override void OnPlayerIsAlive(string soldierName, bool isAlive)
        {
            base.OnPlayerIsAlive(soldierName, isAlive);
            Output.TraceInformation("OnPlayerIsAlive {0}", soldierName);
        }

        public override void OnPlayerSpawned(string soldierName, Inventory spawnedInventory)
        {
            base.OnPlayerSpawned(soldierName, spawnedInventory);
            //Output.TraceInformation("OnPlayerSpawned {0}", soldierName);
        }

        public override void OnPlayerJoin(string soldierName)
        {
            base.OnPlayerJoin(soldierName);
            //Balance();
        }

        public override void OnPlayerKilled(Kill kKillerVictimDetails)
        {
            base.OnPlayerKilled(kKillerVictimDetails);
            if (kKillerVictimDetails.Victim.SoldierName.ToLower() == "iol0ol1")
                Output.TraceInformation("OnPlayerKilled {0} by {1}", kKillerVictimDetails.Victim.SoldierName, kKillerVictimDetails.Killer.SoldierName);
            //Balance();
        }

        public override void OnPlayerMovedByAdmin(string soldierName, int destinationTeamId, int destinationSquadId, bool forceKilled)
        {
            base.OnPlayerMovedByAdmin(soldierName, destinationTeamId, destinationSquadId, forceKilled);
            Output.TraceInformation("OnPlayerMovedByAdmin {0} {1}", soldierName, forceKilled);
        }

        public override void OnPlayerKilledByAdmin(string soldierName)
        {
            base.OnPlayerKilledByAdmin(soldierName);
            Output.TraceInformation("OnPlayerKilledByAdmin {0}", soldierName);
        }

        public override void OnPlayerLeft(CPlayerInfo playerInfo)
        {
            base.OnPlayerLeft(playerInfo);
            //Output.TraceInformation("OnPlayerLeft {0}", playerInfo.SoldierName);
            //Balance();
        }

        public override void OnListPlayers(List<CPlayerInfo> players, CPlayerSubset subset)
        {
            base.OnListPlayers(players, subset);
        }

        public override void OnPlayerTeamChange(string soldierName, int teamId, int squadId)
        {
            base.OnPlayerTeamChange(soldierName, teamId, squadId);
            Output.TraceInformation("OnPlayerTeamChange {0}", soldierName);
            //Balance();
        }

        public override void OnZoneTrespass(CPlayerInfo playerInfo, ZoneAction action, MapZone sender, Point3D tresspassLocation, float tresspassPercentage, object trespassState)
        {
            Output.TraceInformation("OnZoneTrespass {0} {1}", playerInfo.SoldierName, action, sender.LevelFileName);
        }

        public void Balance()
        {
            foreach (var item in FrostbitePlayerInfoList)
            {
                foreach (var friendName in friendSoldierName)
                {
                    if (friendName.Trim().ToLower() == item.Key.ToLower() && item.Value.TeamID != friendTeamID)
                    {
                        if (friendForcesMove)
                            Command("admin.movePlayer", item.Key, friendTeamID.ToString());
                    }
                }
                foreach (var enemyName in enemySoldierName)
                {
                    if (enemyName.Trim().ToLower() == item.Key.ToLower() && item.Value.TeamID != enemyTeamID)
                    {
                        if (enemyForcesMove)
                            Command("admin.movePlayer", item.Key, enemyTeamID.ToString());
                    }
                }
            }
        }

        #endregion

        #region Custom Command

        /// TODO

        #endregion

        #region Private Methods

        /// <summary>
        /// Update plugin file from <see cref="sourceFileUri"/>
        /// </summary>
        /// <param name="sourceFileUri">
        /// source file uri,for example:
        /// <para>https://xxx/xx.cs</para>
        /// <para>file:///D:/xxx/xx.cs</para>
        /// </param>
        private void UpdatePlugin(string sourceFileUri)
        {
            try
            {
                // NOTE:DO NOT convert to string,it will lost UTF-8 BOM header download remote file
                WebClient webClient = new WebClient();
                List<byte> srcDate = webClient.DownloadData(sourceFileUri).ToList();
                webClient.Dispose();
                // replace '\n' to '\r\n'
                byte CR = Convert.ToByte('\r');
                byte LF = Convert.ToByte('\n');
                for (int i = 0; i < srcDate.Count; i++)
                {
                    if (srcDate[i] == LF && (i == 0 || srcDate[i - 1] != CR))
                        srcDate.Insert(i++, CR);
                }
                byte[] srcBuffer = srcDate.ToArray();

                // load local file
                string currentPluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ClassName + ".cs");
                byte[] dstBuffer = File.ReadAllBytes(currentPluginPath);
                // If the file MD5 is different, update.
                if (!Enumerable.SequenceEqual(MD5.Create().ComputeHash(srcBuffer), MD5.Create().ComputeHash(dstBuffer)))
                {
                    string sourceString = Encoding.UTF8.GetString(srcBuffer);
                    if (!string.IsNullOrEmpty(sourceString) && CompilePlugin(sourceString))
                    {
                        File.WriteAllBytes(currentPluginPath, srcBuffer);
                        Output.TraceInformation("Update succssful!");
                        return;
                    }
                    Output.TraceInformation("Update failed!");
                    return;
                }
                Output.TraceInformation("Already the latest version.");
            }
            catch (Exception ex)
            {
                Output.TraceError(ex.Message);
            }
        }

        private void Command(params string[] args)
        {
            List<string> list = new List<string>
            {
                "procon.protected.send"
            };
            list.AddRange(args);
            ExecuteCommand(list.ToArray());
            Output.TraceInformation(string.Join(" ", list.ToArray()));
        }

        /// <summary>
        /// Precompile code to check error.
        /// </summary>
        /// <param name="sourceCode">source code</param>
        /// <returns></returns>
        public bool CompilePlugin(string sourceCode)
        {
            CompilerParameters parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Data.dll");
            parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.dll");
            parameters.ReferencedAssemblies.Add("MySql.Data.dll");
            parameters.ReferencedAssemblies.Add("PRoCon.Core.dll");
            parameters.GenerateInMemory = false;
            parameters.GenerateExecutable = false;
#if DEBUG
            parameters.IncludeDebugInformation = true;
#else
            parameters.IncludeDebugInformation = false;
#endif
            using (CodeDomProvider codeDomProvider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } }))
            {
                CompilerResults results = codeDomProvider.CompileAssemblyFromSource(parameters, sourceCode);
                // check for syntax and reference errors
                if (results.Errors.HasErrors == true && results.Errors[0].ErrorNumber != "CS0016")
                {
                    Output.TraceError("Update file compilation error!");
                    foreach (CompilerError cError in results.Errors)
                    {
                        if (cError.ErrorNumber != "CS0016" && cError.IsWarning == false)
                        {
                            Output.TraceError("(Line: {0}, C: {1}) {2}: {3}", cError.Line, cError.Column, cError.ErrorNumber, cError.ErrorText);
                        }
                    }
                    return false;
                }
                // check for interface error
                Assembly assembly = results.CompiledAssembly;
                Type objType = assembly.GetTypes().FirstOrDefault(_ => _.GetInterfaces().Contains(typeof(IPRoConPluginInterface)));
                if (objType != null)
                {
                    IPRoConPluginInterface obj = assembly.CreateInstance(objType.FullName) as IPRoConPluginInterface;
                    Output.TraceInformation("Plugin:{0} {1}", obj.GetPluginName(), obj.GetPluginVersion());
                    return true;
                }
                Output.TraceError("Not found implementation of {0}!", typeof(IPRoConPluginInterface).Name);
                return false;
            }
        }

        private string VerifyDirectory(string format, params object[] args)
        {
            string fileName = string.Format(format, args);
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            return fileName;
        }

        private CPluginVariable CreateVariable<T>(Expression<Func<T>> exp, bool isHeader)
        {
            /// only valid for remote,it's useless.
            /// <see cref="SetPluginVariable(string, string)"/>
            bool isReadOnly = false;
            string varName = ((MemberExpression)exp.Body).Member.Name;

            // reflect to get variable names
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            MemberInfo memberInfo = GetType().GetMember(varName, bindingFlags).FirstOrDefault();
            if (memberInfo != null)
            {
                MenuAttribute attr = memberInfo.GetCustomAttributes(false).FirstOrDefault(_ => _ is MenuAttribute) as MenuAttribute;
                if (attr != null)
                {
                    varName = isHeader ? attr.ToString() : attr.Name;
                }
            }

            // enum type
            if (typeof(T).BaseType == typeof(Enum))
            {
                return new CPluginVariable(varName, CreateEnumString(typeof(T)), Enum.GetName(typeof(T), exp.Compile()()), isReadOnly);
            }

            // other type
            return new CPluginVariable(varName, typeof(T), exp.Compile()(), isReadOnly);
        }

        #endregion
    }

    #region Menu Attribute

    /// <summary>
    /// Menu attribute for field. Get the variable by reflection according to the <see cref="Name"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal class MenuAttribute : Attribute
    {
        /// <summary>
        /// The header of the variable in plugin setting tab.
        /// </summary>
        public readonly string Header;

        /// <summary>
        /// The name of the variable in plugin setting tab.it's unique value!!
        /// </summary>
        public readonly string Name;

        public override string ToString()
        {
            return Header + "|" + Name;
        }

        public MenuAttribute(string header, string name)
        {
            Header = header;
            Name = name;
        }
    }

    #endregion

    #region Procon Output

    /// <summary>
    /// <para><see cref="Trace"/> will be ignore when plugin compiled.</para>
    /// <para><see cref="Debug"/> need checked 'enable plugin debug' in PRoCon.</para>
    /// <para><see cref="Output"/> can be used anytime and anywhere in pcocon.</para>
    /// </summary>
    internal static class Output
    {
        public static bool AutoFlush { get; set; }

        public static TraceListenerCollection Listeners { get; private set; }

        static Output()
        {
            //Listeners = Debug.Listeners; // same as Debug.Listeners,it's golbal.
            Listeners = typeof(TraceListenerCollection)
                .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null)
                .Invoke(null) as TraceListenerCollection; // it's plug-in private
        }

        public static void Close()
        {
            foreach (TraceListener item in Listeners)
            {
                item.Flush();
                item.Close();
            }
        }

        public static void Flush()
        {
            foreach (TraceListener item in Listeners)
            {
                item.Flush();
            }
        }

        public static void TraceError(string format, params object[] args)
        {
            TraceError(string.Format(format, args));
        }

        public static void TraceError(string message)
        {
            WriteLine("Error: " + "^8" + message + "^n"); // Red
        }

        public static void TraceInformation(string format, params object[] args)
        {
            TraceInformation(string.Format(format, args));
        }

        public static void TraceInformation(string message)
        {
            WriteLine("Information: " + "^4" + message + "^n"); // Royal Blue
        }

        public static void TraceWarning(string format, params object[] args)
        {
            TraceWarning(string.Format(format, args));
        }

        public static void TraceWarning(string message)
        {
            WriteLine("Warning: " + "^3" + message + "^n"); // Dark Orange
        }

        /// <summary>
        /// Write line message, support some escape character.
        /// <para>^0 Black</para>
        /// <para>^1 Maroon</para>
        /// <para>^2 Medium Sea Green</para>
        /// <para>^3 Dark Orange</para>
        /// <para>^4 Royal Blue</para>
        /// <para>^5 Cornflower Blue</para>
        /// <para>^6 Dark Violet</para>
        /// <para>^7 Deep Pink</para>
        /// <para>^8 Red</para>
        /// <para>^9 Grey</para>
        /// <para>^b Bold</para>
        /// <para>^n Normal</para>
        /// <para>^i Italicized</para>
        /// <para>^^ ^(Escape character)</para>
        /// </summary>
        /// <param name="message">direct output</param>
        public static void WriteLine(string message)
        {
            foreach (TraceListener item in Listeners)
            {
                item.WriteLine(message);
                if (AutoFlush)
                {
                    item.Flush();
                }
            }
        }
    }

    /// <summary>
    /// Procon trace listener.
    /// </summary>
    internal class PRoConTraceListener : TraceListener
    {
        private readonly int output;

        private readonly CPRoConMarshalByRefObject plugin;

        /// <summary>
        /// Construct, use pluginconsole output.
        /// </summary>
        /// <param name="pRoConPlugin">plugin instance</param>
        public PRoConTraceListener(CPRoConMarshalByRefObject pRoConPlugin) : this(pRoConPlugin, 0)
        { }

        /// <summary>
        /// Construct with output type.
        /// </summary>
        /// <param name="pRoConPlugin">plugin instance</param>
        /// <param name="outputType">0 pluginconsole;1 console;2 chat</param>
        public PRoConTraceListener(CPRoConMarshalByRefObject pRoConPlugin, int outputType)
        {
            plugin = pRoConPlugin;
            output = outputType;
        }

        /// <summary>
        /// As the same as <see cref="WriteLine(string)"/>.
        /// NOTE: procon NOT supported write.
        /// </summary>
        /// <param name="message"></param>
        public override void Write(string message)
        {
            switch (output)
            {
                case 0:
                    plugin.ExecuteCommand("procon.protected.pluginconsole.write", message);
                    return;

                case 1:
                    plugin.ExecuteCommand("procon.protected.console.write", message);
                    return;

                case 2:
                    plugin.ExecuteCommand("procon.protected.chat.write", message);
                    return;

                default:
                    return;
            }
        }

        public override void WriteLine(string message)
        {
            Write(message);
        }
    }

    #endregion
}
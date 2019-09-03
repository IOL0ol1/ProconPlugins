using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
                    Output.Warning("Enable plugin to use update function!");
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
            return "This is a plugin for team work in the game.";
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
            return "https://github.com/IOL0ol1/ProconPlugins/blob/master/FriendManager/FriendManager.cs";
        }

        public void OnPluginDisable()
        {
            isEnable = false;
            Output.Information(string.Format("^b{0} {1} ^1Disabled", GetPluginName(), GetPluginVersion()));
        }

        public void OnPluginEnable()
        {
            Output.Information(string.Format("^b{0} {1} ^2Enabled", GetPluginName(), GetPluginVersion()));
            isEnable = true;
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
        {
            Output.Listeners.Add(new TextWriterTraceListener(ClassName + "_" + strHostName + "_" + strPort + ".log") { TraceOutputOptions = TraceOptions.DateTime }); // output to debug file
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
            Output.Information("OnLoadingLevel {0} {1} {2}", mapFileName, roundsPlayed, roundsTotal);
        }

        public override void OnLevelLoaded(string mapFileName, string gamemode, int roundsPlayed, int roundsTotal)
        {
            base.OnLevelLoaded(mapFileName, gamemode, roundsPlayed, roundsTotal);
            Output.Information("OnLevelLoaded {0} {1} {2}", mapFileName, roundsPlayed, roundsTotal);
        }

        public override void OnLevelStarted()
        {
            base.OnLevelStarted();
            Output.Information("OnLevelStarted");
        }

        public override void OnRunNextLevel()
        {
            base.OnRunNextLevel();
            Output.Information("OnRunNextLevel");
        }

        public override void OnRestartLevel()
        {
            base.OnRestartLevel();
            Output.Information("OnRestartLevel");
        }

        public override void OnEndRound(int iWinningTeamID)
        {
            base.OnEndRound(iWinningTeamID);
            Output.Information("OnEndRound {0}", iWinningTeamID);
        }

        public override void OnRoundOverPlayers(List<CPlayerInfo> players)
        {
            Output.Information("OnRoundOverPlayers {0}", string.Join(",", players.Select(_ => _.SoldierName + "|" + _.Score).ToArray()));
        }

        public override void OnRoundOverTeamScores(List<TeamScore> teamScores)
        {
            Output.Information("OnRoundOverTeamScores {0}", string.Join(",", teamScores.Select(_ => _.TeamID + "|" + _.Score).ToArray()));
        }

        public override void OnRoundOver(int winningTeamId)
        {
            Output.Information("OnRoundOver {0}", winningTeamId);
        }

        public override void OnPlayerIsAlive(string soldierName, bool isAlive)
        {
            base.OnPlayerIsAlive(soldierName, isAlive);
            Output.Information("OnPlayerIsAlive {0}", soldierName);
        }

        public override void OnPlayerSpawned(string soldierName, Inventory spawnedInventory)
        {
            base.OnPlayerSpawned(soldierName, spawnedInventory);
            //Output.Information("OnPlayerSpawned {0}", soldierName);
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
                Output.Information("OnPlayerKilled {0} by {1}", kKillerVictimDetails.Victim.SoldierName, kKillerVictimDetails.Killer.SoldierName);
            //Balance();
        }

        public override void OnPlayerMovedByAdmin(string soldierName, int destinationTeamId, int destinationSquadId, bool forceKilled)
        {
            base.OnPlayerMovedByAdmin(soldierName, destinationTeamId, destinationSquadId, forceKilled);
            Output.Information("OnPlayerMovedByAdmin {0} {1}", soldierName, forceKilled);
        }

        public override void OnPlayerKilledByAdmin(string soldierName)
        {
            base.OnPlayerKilledByAdmin(soldierName);
            Output.Information("OnPlayerKilledByAdmin {0}", soldierName);
        }

        public override void OnPlayerLeft(CPlayerInfo playerInfo)
        {
            base.OnPlayerLeft(playerInfo);
            //Output.Information("OnPlayerLeft {0}", playerInfo.SoldierName);
            //Balance();
        }

        public override void OnListPlayers(List<CPlayerInfo> players, CPlayerSubset subset)
        {
            base.OnListPlayers(players, subset);
        }

        public override void OnPlayerTeamChange(string soldierName, int teamId, int squadId)
        {
            base.OnPlayerTeamChange(soldierName, teamId, squadId);
            Output.Information("OnPlayerTeamChange {0}", soldierName);
            //Balance();
        }

        public override void OnZoneTrespass(CPlayerInfo playerInfo, ZoneAction action, MapZone sender, Point3D tresspassLocation, float tresspassPercentage, object trespassState)
        {
            Output.Information("OnZoneTrespass {0} {1}", playerInfo.SoldierName, action, sender.LevelFileName);
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
                        Output.Information("Update succssful!");
                        return;
                    }
                    Output.Information("Update failed!");
                    return;
                }
                Output.Information("Already the latest version.");
            }
            catch (Exception ex)
            {
                Output.Error(ex.Message);
            }
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
                    Output.Error("Update file compilation error!");
                    foreach (CompilerError cError in results.Errors)
                    {
                        if (cError.ErrorNumber != "CS0016" && cError.IsWarning == false)
                        {
                            Output.Error("(Line: {0}, C: {1}) {2}: {3}", cError.Line, cError.Column, cError.ErrorNumber, cError.ErrorText);
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
                    Output.Information("Plugin:{0} {1}", obj.GetPluginName(), obj.GetPluginVersion());
                    return true;
                }
                Output.Error("Not found implementation of {0}!", typeof(IPRoConPluginInterface).Name);
                return false;
            }
        }

        private string VerifyDirectory(string format, params object[] args)
        {
            string fileName = string.Format(format, args);
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            return fileName;
        }

        private void Command(params string[] args)
        {
            if (args.Length == 0)
                return;
            List<string> list = new List<string> { "procon.protected.send" };
            foreach (var item in args)
            {
                list.AddRange(item.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }
            ExecuteCommand(list.ToArray());
        }

        private CPluginVariable CreateVariable<T>(Expression<Func<T>> exp, bool isAddHeader)
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
                    varName = isAddHeader ? attr.ToString() : attr.Name;
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

    #region Template

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
            Listeners = typeof(TraceListenerCollection)
                .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null)
                .Invoke(null) as TraceListenerCollection;
        }

        public static void Error(string format, params object[] args)
        {
            WriteLine(string.Format(format, args), TraceEventType.Error);
        }

        public static void Information(string format, params object[] args)
        {
            WriteLine(string.Format(format, args), TraceEventType.Information);
        }

        public static void Warning(string format, params object[] args)
        {
            WriteLine(string.Format(format, args), TraceEventType.Warning);
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

        private static void WriteLine(string message, TraceEventType eventType)
        {
            foreach (TraceListener item in Listeners)
            {
                item.TraceEvent(new TraceEventCache(), string.Empty, eventType, 0, message);
                if (AutoFlush)
                {
                    item.Flush();
                }
            }
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
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void WriteLine(string format, params object[] args)
        {
            foreach (TraceListener item in Listeners)
            {
                item.WriteLine(string.Format(format, args));
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
        private readonly string prefix;

        private readonly PRoConPluginAPI plugin;

        /// <summary>
        /// Construct, use pluginconsole output.
        /// </summary>
        /// <param name="pRoConPlugin">plugin instance</param>
        public PRoConTraceListener(PRoConPluginAPI pRoConPlugin) : this(pRoConPlugin, 0)
        { }

        /// <summary>
        /// Construct with output type.
        /// </summary>
        /// <param name="pRoConPlugin">plugin instance</param>
        /// <param name="outputType">0 pluginconsole;1 console;2 chat</param>
        public PRoConTraceListener(PRoConPluginAPI pRoConPlugin, int outputType)
        {
            plugin = pRoConPlugin;
            switch (outputType)
            {
                case 2:
                    prefix = "procon.protected.console.write";
                    break;

                case 1:
                    prefix = "procon.protected.chat.write";
                    break;

                case 0:
                default:
                    prefix = "procon.protected.pluginconsole.write";
                    break;
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, message, null, null, null))
            {
                return;
            }
            WriteLine(AddHeader(source, eventType, id) + message);
            WriteFooter(eventCache);
        }

        private string AddHeader(string source, TraceEventType eventType, int id)
        {
            string eventTypeName = eventType.ToString();
            switch (eventType)
            {
                case TraceEventType.Critical:
                    eventTypeName = "^7" + eventTypeName + ":^0";
                    break;
                case TraceEventType.Error:
                    eventTypeName = "^8" + eventTypeName + ":^0";
                    break;
                case TraceEventType.Warning:
                    eventTypeName = "^3" + eventTypeName + ":^0";
                    break;
                case TraceEventType.Information:
                    eventTypeName = "^4" + eventTypeName + ":^0";
                    break;
                case TraceEventType.Verbose:
                    eventTypeName = "^2" + eventTypeName + ":^0";
                    break;
                default:
                    eventTypeName = "^0" + eventTypeName + ":^0";
                    break;
            }
            return string.Format(CultureInfo.InvariantCulture, "{0}{1} ", new object[]
            {
                string.IsNullOrEmpty(source) ? string.Empty : string.Format("[{0}] ",source),
                eventTypeName,
            });
        }

        private void WriteFooter(TraceEventCache eventCache)
        {
            if (eventCache == null)
                return;
            IndentLevel++;
            if (IsEnabled(TraceOptions.ProcessId))
            {
                WriteLine("ProcessId=" + eventCache.ProcessId);
            }
            if (IsEnabled(TraceOptions.LogicalOperationStack))
            {
                string stack = "LogicalOperationStack=";
                Stack logicalOperationStack = eventCache.LogicalOperationStack;
                bool flag = true;
                foreach (object obj in logicalOperationStack)
                {
                    if (!flag)
                    {
                        stack += ", ";
                    }
                    else
                    {
                        flag = false;
                    }
                    stack += obj.ToString();
                }
                WriteLine(stack);
            }
            if (IsEnabled(TraceOptions.ThreadId))
            {
                WriteLine("ThreadId=" + eventCache.ThreadId);
            }
            if (IsEnabled(TraceOptions.DateTime))
            {
                WriteLine("DateTime=" + eventCache.DateTime.ToString("o", CultureInfo.InvariantCulture));
            }
            if (IsEnabled(TraceOptions.Timestamp))
            {
                WriteLine("Timestamp=" + eventCache.Timestamp);
            }
            if (IsEnabled(TraceOptions.Callstack))
            {
                WriteLine("Callstack=" + eventCache.Callstack);
            }
            IndentLevel--;
        }

        private bool IsEnabled(TraceOptions opts)
        {
            return (opts & TraceOutputOptions) > TraceOptions.None;
        }

        public override void Write(string message)
        {
            plugin.ExecuteCommand(prefix, message);
        }

        public override void WriteLine(string message)
        {
            Write(string.Format("[{0}] {1}", plugin.ClassName, message));
        }
    }

    #endregion

    #endregion
}
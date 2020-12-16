using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;

using MySql.Data.MySqlClient;

using PRoCon.Core;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Players;
using PRoCon.Core.Plugin;

namespace PRoConEvents
{
    public class AdminLogger : PRoConPluginAPI, IPRoConPluginInterface
    {
        private bool isEnable = false;

        #region Menu List

        public const string MySQLSettings = "1 MySQL Settings";
        public const string ServerSettings = "2 Server Settings";

        [Menu(MySQLSettings, "Hostname/IP")]
        public string hostName = "localhost";

        [Menu(MySQLSettings, "Port")]
        public int hostPort = 3306;

        [Menu(MySQLSettings, "User")]
        public string hostUser = string.Empty;

        [Menu(MySQLSettings, "Password")]
        public string hostPassword = string.Empty;

        [Menu(MySQLSettings, "Databases")]
        public string hostDatabase = string.Empty;

        [Menu(ServerSettings, "Server Id")]
        public int hostServerId = 0;


        private string serverType;
        private bool isGetVersion;

        private List<CPluginVariable> GetVariables(bool isAddHeader)
        {
            List<CPluginVariable> pluginVariables = new List<CPluginVariable>();

            pluginVariables.Add(CreateVariable(() => hostName, isAddHeader));
            pluginVariables.Add(CreateVariable(() => hostPort, isAddHeader));
            pluginVariables.Add(CreateVariable(() => hostDatabase, isAddHeader));
            pluginVariables.Add(CreateVariable(() => hostUser, isAddHeader));
            pluginVariables.Add(CreateVariable(() => hostPassword, isAddHeader));
            pluginVariables.Add(CreateVariable(() => hostServerId, isAddHeader));

            return pluginVariables;
        }

        #endregion



        #region IPRoConPluginInterface

        public List<CPluginVariable> GetDisplayPluginVariables()
        {
            // Add some code. example code block:


            return GetVariables(true);
        }

        public void OnPluginDisable()
        {
            Output.Information(string.Format("^b{0} {1} ^1Disabled^n", GetPluginName(), GetPluginVersion()));
            isEnable = false;
            // Add some code
        }

        public void OnPluginEnable()
        {
            Output.Information(string.Format("^b{0} {1} ^2Enabled^n", GetPluginName(), GetPluginVersion()));
            /// Add some code
            isEnable = true;
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
        {
            Output.Listeners.Add(new TextWriterTraceListener(ClassName + ".log", ClassName));
            Output.Listeners.Add(new PRoConTraceListener(this)); // output to pluginconsole
            Output.AutoFlush = true;

            // Get and register common events in this class and PRoConPluginAPI
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            IEnumerable<string> baseMethods = typeof(PRoConPluginAPI).GetMethods().Where(_ => _.IsVirtual).Select(_ => _.Name);
            IEnumerable<string> commonMethods = GetType().GetMethods(bindingFlags).Where(_ => _.IsVirtual).Select(_ => _.Name).Intersect(baseMethods);
            RegisterEvents(ClassName, commonMethods.ToArray());
            // Add some code
        }

        public string GetPluginAuthor()
        {
            return "Author";
        }

        public string GetPluginDescription()
        {
            return "Description";
        }

        public string GetPluginVersion()
        {
            return "Version";
        }

        public string GetPluginWebsite()
        {
            return "Website";
        }

        public string GetPluginName()
        {
            return ClassName;
        }

        public List<CPluginVariable> GetPluginVariables()
        {
            return GetVariables(false);
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


        /*
         * 
         * // player AiR-Cote kill player jinddog
            OnPlayerKilled kClanTag: knoClan:False kSoldierName:AiR-Cote kGUID:EA_11AA9AE1D2A2E4414E7F5529E0F44220 kTeamID:1 kSquadID:2 kScore:3572 kKills:24 kDeaths:15 kPing:94 kRank:140 kType:0 kKdr:1.533333 kJoinTime:452663 kSessionTime:1069 vClanTag: vnoClan:False vSoldierName:jinddog vGUID:EA_A26FE0BE5D2DF1EA0DF91417B8DFE129 vTeamID:2 vSquadID:3 vScore:9717 vKills:66 vDeaths:15 vPing:8 vRank:140 vType:0 vKdr:4.714286 vJoinTime:446617 vSessionTime:7115 KillerLocation:[0,0,0] VictimLocation:[0,0,0] Headshot:True IsSuicide:False TimeOfDeath:2020/12/14 22:49:29 Distance:0


            // Kill by admin( A squad is 1, H squad is 8)
            OnPlayerKilled kClanTag: knoClan:False kSoldierName: kGUID: kTeamID:0 kSquadID:0 kScore:0 kKills:0 kDeaths:0 kPing:0 kRank:0 kType:0 kKdr:0 kJoinTime:0 kSessionTime:0 vClanTag: vnoClan:False vSoldierName:IOL0ol1 vGUID:EA_82AEE57A8442ED84572D96BE2C74D325 vTeamID:2 vSquadID:8 vScore:0 vKills:0 vDeaths:1 vPing:57 vRank:140 vType:0 vKdr:0 vJoinTime:453732 vSessionTime:0 KillerLocation:[0,0,0] VictimLocation:[0,0,0] Headshot:False IsSuicide:False TimeOfDeath:2020/12/14 22:49:46 Distance:0
            OnPlayerKilled kClanTag: knoClan:False kSoldierName: kGUID: kTeamID:0 kSquadID:0 kScore:0 kKills:0 kDeaths:0 kPing:0 kRank:0 kType:0 kKdr:0 kJoinTime:0 kSessionTime:0 vClanTag: vnoClan:False vSoldierName:IOL0ol1 vGUID:EA_82AEE57A8442ED84572D96BE2C74D325 vTeamID:2 vSquadID:8 vScore:0 vKills:0 vDeaths:1 vPing:54 vRank:140 vType:0 vKdr:0 vJoinTime:453732 vSessionTime:86 KillerLocation:[0,0,0] VictimLocation:[0,0,0] Headshot:False IsSuicide:False TimeOfDeath:2020/12/14 22:51:00 Distance:0

         */

        public override void OnAccountLogin(string accountName, string ip, CPrivileges privileges)
        {
            base.OnAccountLogin(accountName, ip, privileges);
            Output.WriteLine("OnAccountLogin {0} {1} {2}", accountName, ip, privileges.PrivilegesFlags);
        }
        public override void OnAccountLogout(string accountName, string ip, CPrivileges privileges)
        {
            base.OnAccountLogout(accountName, ip, privileges);
            Output.WriteLine("OnAccountLogout {0} {1} {2}", accountName, ip, privileges.PrivilegesFlags);

        }

        public override void OnAccountPrivilegesUpdate(string username, CPrivileges privileges)
        {
            base.OnAccountPrivilegesUpdate(username, privileges);
        }

        public override void OnLevelLoaded(string mapFileName, string gamemode, int roundsPlayed, int roundsTotal)
        {
            base.OnLevelLoaded(mapFileName, gamemode, roundsPlayed, roundsTotal);
            Output.WriteLine("OnLevelLoaded {0} {1} {2} {3}", mapFileName, gamemode, roundsPlayed, roundsTotal);

        }

        public override void OnLevelStarted()
        {
            base.OnLevelStarted();
            Output.WriteLine("OnLevelStarted");

        }

        public override void OnLoadingLevel(string mapFileName, int roundsPlayed, int roundsTotal)
        {
            base.OnLoadingLevel(mapFileName, roundsPlayed, roundsTotal);
            Output.WriteLine("OnLoadingLevel {0} {1} {2}", mapFileName, roundsPlayed, roundsTotal);
        }
        public override void OnRunNextLevel()
        {
            base.OnRunNextLevel();
            Output.WriteLine("OnRunNextLevel");

        }

        public override void OnEndRound(int iWinningTeamID)
        {
            base.OnEndRound(iWinningTeamID);
            Output.WriteLine("OnEndRound {0}", iWinningTeamID);
        }

        public override void OnRunScript(string scriptFileName)
        {
            base.OnRunScript(scriptFileName);
        }

        public override void OnPlayerKilled(Kill kKillerVictimDetails)
        {
            base.OnPlayerKilled(kKillerVictimDetails);
        }


        private IEnumerable<string> Kill2String(Kill k)
        {
            foreach (var item in k.GetType().GetProperties())
            {
                var o = item.GetValue(k, null);
                if (o.GetType().BaseType == typeof(ValueType))
                    yield return string.Format("{0}:{1}", item.Name, o.ToString());
                else if (o.GetType() == typeof(Point3D))
                {
                    Point3D p = (Point3D)o;
                    yield return string.Format("{0}:[{1},{2},{3}]", item.Name, p.X, p.Y, p.Z);
                }
                else if (o.GetType() == typeof(CPlayerInfo))
                {
                    foreach (var pp in CPlayerInfo2String((CPlayerInfo)o))
                    {
                        yield return (item.Name.Contains("Kill") ? "k" : "v") + pp;
                    }
                }
            }
        }

        private IEnumerable<string> CPlayerInfo2String(CPlayerInfo p)
        {
            foreach (var item in p.GetType().GetProperties())
            {
                yield return string.Format("{0}:{1}", item.Name, item.GetValue(p, null));
            }
        }

        public override void OnPlayerKickedByAdmin(string soldierName, string reason)
        {
            base.OnPlayerKickedByAdmin(soldierName, reason);
        }

        public override void OnPlayerKilledByAdmin(string soldierName)
        {
            base.OnPlayerKilledByAdmin(soldierName);
            Output.WriteLine("OnPlayerKilledByAdmin {0}", soldierName);

        }

        public override void OnPlayerMovedByAdmin(string soldierName, int destinationTeamId, int destinationSquadId, bool forceKilled)
        {
            base.OnPlayerMovedByAdmin(soldierName, destinationTeamId, destinationSquadId, forceKilled);
        }

        public override void OnPlayerPingedByAdmin(string soldierName, int ping)
        {
            base.OnPlayerPingedByAdmin(soldierName, ping);
        }

        public override void OnBanAdded(CBanInfo ban)
        {
            base.OnBanAdded(ban);

        }

        public override void OnZoneTrespass(CPlayerInfo playerInfo, ZoneAction action, MapZone sender, Point3D tresspassLocation, float tresspassPercentage, object trespassState)
        {
            base.OnZoneTrespass(playerInfo, action, sender, tresspassLocation, tresspassPercentage, trespassState);
        }

        public override void OnBanRemoved(CBanInfo ban)
        {
            base.OnBanRemoved(ban);
        }

        public override void OnPunkbusterBanInfo(CBanInfo ban)
        {
            base.OnPunkbusterBanInfo(ban);
        }

        /// <summary>
        /// get version
        /// </summary>
        /// <param name="serverType"></param>
        /// <param name="version"></param>
        public override void OnVersion(string serverType, string version)
        {
            base.OnVersion(serverType, version);
            this.serverType = serverType;
            isGetVersion = true;
        }

        public override void OnListPlayers(List<CPlayerInfo> players, CPlayerSubset subset)
        {
            base.OnListPlayers(players, subset);
            if (!isEnable) return;
            // Add some code.
        }

        #endregion

        #region Private Methods



        /// <summary>
        /// Create database by <see cref="hostDatabase"/> if not exist.
        /// </summary>
        private void InitDatabase()
        {
            DbConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder();
            connectionStringBuilder.Add("server", hostName);
            connectionStringBuilder.Add("port", hostPort);
            connectionStringBuilder.Add("user", hostUser);
            connectionStringBuilder.Add("password", hostPassword);
            connectionStringBuilder.Add("database", hostDatabase);
            try
            {
                using (DbConnection connection = new MySqlConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();
                    using (DbTransaction transaction = connection.BeginTransaction())
                    {
                        using (DbCommand command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = string.Format("CREATE TABLE IF NOT EXISTS `{0}`.`{1}` (" +
                                "`id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT," +
                                "`serverid` INT(10) DEFAULT NULL," +
                                "`gametype` VARCHAR(10) NULL DEFAULT NULL," +
                                "`ipaddress` VARCHAR(22) NULL DEFAULT NULL," +
                                "`clantag` VARCHAR(5) NULL DEFAULT NULL," +
                                "`soldiername` VARCHAR(20) NULL DEFAULT NULL," +
                                "`eaguid` VARCHAR(35) NULL DEFAULT NULL ," +
                                "`pbguid` VARCHAR(32) NULL DEFAULT NULL ," +
                                "`country` VARCHAR(30) NULL DEFAULT NULL ," +
                                "`countrycode` VARCHAR(10) NULL DEFAULT NULL ," +
                                "`firsttime` datetime DEFAULT NULL," +
                                "`lasttime` datetime DEFAULT NULL," +
                                "PRIMARY KEY(`id`), " +
                                "UNIQUE INDEX `Index 2` (`eaguid`), " +
                                "UNIQUE INDEX `Index 3` (`pbguid`)" +
                                ")", hostDatabase, ClassName);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Output.WriteLine("-------------------");
                Output.Error(ex.ToString());
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
        }

        private CPluginVariable CreateVariable<T>(Expression<Func<T>> exp, bool isAddHeader)
        {
            /// only valid for remote,it's useless.
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
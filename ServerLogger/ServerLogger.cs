using MySql.Data.MySqlClient;
using PRoCon.Core;
using PRoCon.Core.Players;
using PRoCon.Core.Plugin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;

namespace PRoConEvents
{
    public class ServerLogger : PRoConPluginAPI, IPRoConPluginInterface
    {
        private bool isEnable = false;

        #region Menu List

        public const string MySQLSettings = "1 MySQL Settings";

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

        // Add some code

        private List<CPluginVariable> GetVariables(bool isAddHeader)
        {
            List<CPluginVariable> pluginVariables = new List<CPluginVariable>();

            pluginVariables.Add(CreateVariable(() => hostName, isAddHeader));
            pluginVariables.Add(CreateVariable(() => hostPort, isAddHeader));
            pluginVariables.Add(CreateVariable(() => hostDatabase, isAddHeader));
            pluginVariables.Add(CreateVariable(() => hostUser, isAddHeader));
            pluginVariables.Add(CreateVariable(() => hostPassword, isAddHeader));
            // Add some code

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
            isEnable = false;
            Output.Information(string.Format("^b{0} {1} ^1Disabled^0", GetPluginName(), GetPluginVersion()));
            // Add some code

        }

        public void OnPluginEnable()
        {
            Output.Information(string.Format("^b{0} {1} ^2Enabled^0", GetPluginName(), GetPluginVersion()));


            // Add some code
            InitDatabase();
            InitInsert();

            isEnable = true;
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
        {
            Output.Listeners.Add(new TextWriterTraceListener(ClassName + "_" + strHostName + "_" + strPort + ".log") { TraceOutputOptions = TraceOptions.DateTime }); // output to debug file
            //Output.Listeners.Add(new ConsoleTraceListener());
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
            return "IOL0ol1";
        }

        public string GetPluginDescription()
        {
            return "Log server player's EAGUID , PBGUID ,IPAddress, Country";
        }

        public string GetPluginVersion()
        {
            return "0.0.0.1";
        }

        public string GetPluginWebsite()
        {
            return "https://github.com/IOL0ol1/ProconPlugins/blob/master/ServerLogger/ServerLogger.cs";
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

        /// <summary>
        /// this event trigger on player join server
        /// </summary>
        /// <param name="soldierName"></param>
        /// <param name="guid"></param>
        public override void OnPlayerAuthenticated(string soldierName, string guid)
        {
            base.OnPlayerAuthenticated(soldierName, guid);
            if (string.IsNullOrEmpty(soldierName) || guid.Length != 35)
                return;
            DbParameter[] parameters = new DbParameter[]
            {
                    new MySqlParameter("@ipaddress", DBNull.Value),
                    new MySqlParameter("@clantag", DBNull.Value),
                    new MySqlParameter("@soldiername", soldierName),
                    new MySqlParameter("@eaguid",guid),
                    new MySqlParameter("@pbguid", DBNull.Value),
                    new MySqlParameter("@country", DBNull.Value),
                    new MySqlParameter("@countrycode", DBNull.Value),
            };
            InsertEAInfo(parameters);
        }

        /// <summary>
        /// this event trigger after "admin.listPlayers all"
        /// </summary>
        /// <param name="players"></param>
        /// <param name="subset"></param>
        public override void OnListPlayers(List<CPlayerInfo> players, CPlayerSubset subset)
        {
            base.OnListPlayers(players, subset);
            TestDatabase();
            hasListPlayers = true;
        }

        /// <summary>
        /// this event trigger after "punkBuster.pb_sv_command pb_sv_plist" started
        /// <para>pb_sv_plist will list all player with punkbuster info in server</para>
        /// </summary>
        public override void OnPunkbusterBeginPlayerInfo()
        {
            base.OnPunkbusterBeginPlayerInfo();
            if (hasListPlayers && !isAcceptConnection)
                isFirstEnable = false;
            isAcceptConnection = true;
        }

        /// <summary>
        /// this event trigger after "punkBuster.pb_sv_command pb_sv_plist" finished
        /// </summary>
        public override void OnPunkbusterEndPlayerInfo()
        {
            base.OnPunkbusterEndPlayerInfo();
            isAcceptConnection = false;
        }

        /// <summary>
        /// this event will trigger when punkbuster get new player connect and run "pb_sv_plist" to list player.
        /// <para>1, MUST waie "admin.listPlayers all" finished (<see cref="hasListPlayers"/>)</para>
        /// <para>2, Only get new punkbuster player info (<see cref="isAcceptConnection"/>)</para>
        /// <para>3, If first enable plugin,get all punkbuster player info (<see cref="isFirstEnable"/>)</para>
        /// </summary>
        /// <param name="playerInfo"></param>
        public override void OnPunkbusterPlayerInfo(CPunkbusterInfo playerInfo)
        {
            base.OnPunkbusterPlayerInfo(playerInfo);

            if ((isAcceptConnection || isFirstEnable) && hasListPlayers)
            {
                string soldierName = playerInfo.SoldierName;
                if (!FrostbitePlayerInfoList.ContainsKey(soldierName) ||
                    FrostbitePlayerInfoList[soldierName].GUID.Length != 35 ||
                    playerInfo.GUID.Length != 32)
                    return;
                CPlayerInfo info = FrostbitePlayerInfoList[soldierName];
                Func<object, object> NullChecker = o => o ?? DBNull.Value;
                DbParameter[] parameters = new DbParameter[]
                {
                    new MySqlParameter("@ipaddress", NullChecker(playerInfo.Ip)),
                    new MySqlParameter("@clantag", NullChecker(info.ClanTag)),
                    new MySqlParameter("@soldiername", NullChecker(soldierName)),
                    new MySqlParameter("@eaguid",NullChecker(info.GUID)),
                    new MySqlParameter("@pbguid", NullChecker(playerInfo.GUID)),
                    new MySqlParameter("@country", NullChecker(playerInfo.PlayerCountry)),
                    new MySqlParameter("@countrycode", NullChecker(playerInfo.PlayerCountryCode)),
                };
                InsertPbInfo(parameters);
            }
        }

        #endregion

        #region Private Methods

        private bool hasListPlayers = false;
        private bool isAcceptConnection = true;
        private bool isFirstEnable = true;
        private int retryTestCount = 0;

        private void InitInsert()
        {
            hasListPlayers = false;
            isAcceptConnection = true;
            isFirstEnable = true;
            retryTestCount = 0;
            Command("admin.listPlayers all");
        }

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
            try
            {
                using (DbConnection connection = new MySqlConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();
                    using (DbTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (DbCommand command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandText = string.Format("CREATE DATABASE if NOT EXISTS `{0}`", hostDatabase);
                                command.ExecuteNonQuery();
                                command.CommandText = string.Format("USE `{0}`", hostDatabase);
                                command.ExecuteNonQuery();
                                command.CommandText = string.Format("CREATE TABLE IF NOT EXISTS `{0}` (" +
                                    "`id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT," +
                                    "`gameid` INT(10) UNSIGNED NOT NULL DEFAULT '0'," +
                                    "`ipaddress` VARCHAR(22) NULL DEFAULT NULL COLLATE 'utf8mb4_general_ci'," +
                                    "`clantag` VARCHAR(5) NULL DEFAULT NULL COLLATE 'utf8mb4_general_ci'," +
                                    "`soldiername` VARCHAR(20) NULL DEFAULT NULL COLLATE 'utf8mb4_general_ci'," +
                                    "`eaguid` VARCHAR(35) NULL DEFAULT NULL COLLATE 'utf8mb4_general_ci'," +
                                    "`pbguid` VARCHAR(32) NULL DEFAULT NULL COLLATE 'utf8mb4_general_ci'," +
                                    "`country` VARCHAR(30) NULL DEFAULT NULL COLLATE 'utf8mb4_general_ci'," +
                                    "`countrycode` VARCHAR(10) NULL DEFAULT NULL COLLATE 'utf8mb4_general_ci'," +
                                    "PRIMARY KEY(`id`), UNIQUE INDEX `Index 2` (`eaguid`), UNIQUE INDEX `Index 3` (`pbguid`)" +
                                    ") COLLATE = 'utf8mb4_general_ci'", ClassName);
                                command.ExecuteNonQuery();
                                transaction.Commit();
                            }
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
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

        public void TestDatabase()
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
                }
                retryTestCount = 0;
            }
            catch (Exception ex)
            {
                Output.Error(ex.ToString());
                if (retryTestCount++ > 5)
                {
                    retryTestCount = 0;
                    Output.Error("{0} is disable", ClassName);
                    EnablePlugin(false);
                }
            }
        }

        /// <summary>
        /// Insert all player info (if it's exist update by eaguid).
        /// </summary>
        /// <param name="parameters"></param>
        private void InsertPbInfo(DbParameter[] parameters)
        {
            DbConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder();
            connectionStringBuilder.Add("server", hostName);
            connectionStringBuilder.Add("port", hostPort);
            connectionStringBuilder.Add("user", hostUser);
            connectionStringBuilder.Add("password", hostPassword);
            try
            {
                using (DbConnection connection = new MySqlConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();
                    using (DbTransaction transaction = connection.BeginTransaction())
                    {
                        DbCommand command = connection.CreateCommand();

                        try
                        {
                            // select playerid by eaguid
                            command.Transaction = transaction;

                            command.Parameters.AddRange(parameters);
                            command.CommandText = string.Format("SELECT `id` from `{0}`.`{1}` WHERE `eaguid` = @eaguid", hostDatabase, ClassName);
                            DbDataReader reader = command.ExecuteReader();
                            if (!reader.Read())
                            {
                                // if not found playerid, insert player info to database.
                                reader.Close();
                                command.CommandText = string.Format(
                                    "INSERT INTO `{0}`.`{1}` (" +
                                    "`ipaddress`," +
                                    "`clantag`," +
                                    "`soldiername`," +
                                    "`eaguid`," +
                                    "`pbguid`," +
                                    "`country`," +
                                    "`countrycode`" +
                                    ") VALUES (" +
                                    "@ipaddress," +
                                    "@clantag," +
                                    "@soldiername," +
                                    "@eaguid," +
                                    "@pbguid," +
                                    "@country," +
                                    "@countrycode" +
                                    ")", hostDatabase, ClassName);
                                command.ExecuteNonQuery();
                            }
                            else
                            {
                                // if found id by eaguid, update player info to database.
                                var id = reader.GetInt64(0);
                                reader.Close();
                                command.CommandText = string.Format(
                                    "UPDATE `{0}`.`{1}` SET " +
                                    "`ipaddress`   = @ipaddress, " +
                                    "`clantag`     = @clantag, " +
                                    "`soldiername` = @soldiername, " +
                                    "`pbguid`      = @pbguid, " +
                                    "`country`     = @country, " +
                                    "`countrycode` = @countrycode " +
                                    " WHERE `id`   = @id", hostDatabase, ClassName);
                                command.Parameters.Add(new MySqlParameter("@id", id));
                                command.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                        finally
                        {
                            command.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Output.Error(ex.ToString());
            }
        }

        /// <summary>
        /// Insert soldiername and eaguid (Update soldiername if eaguid is exist).
        /// </summary>
        /// <param name="parameters"></param>
        private void InsertEAInfo(DbParameter[] parameters)
        {
            try
            {
                DbConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder();
                connectionStringBuilder.Add("server", hostName);
                connectionStringBuilder.Add("port", hostPort);
                connectionStringBuilder.Add("user", hostUser);
                connectionStringBuilder.Add("password", hostPassword);
                using (DbConnection connection = new MySqlConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();
                    using (DbTransaction transaction = connection.BeginTransaction())
                    {
                        DbCommand command = connection.CreateCommand();

                        try
                        {
                            // select playerid by eaguid
                            command.Transaction = transaction;
                            command.Parameters.AddRange(parameters);
                            command.CommandText = string.Format(
                                "INSERT IGNORE INTO `{0}`.`{1}` (`soldiername`,`eaguid`) VALUE (@soldiername, @eaguid);" +
                                "UPDATE `{0}`.`{1}` SET `soldiername` = @soldiername WHERE eaguid = @eaguid;", hostDatabase, ClassName);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                        finally
                        {
                            command.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Output.Error(ex.ToString());
            }
        }

        private void EnablePlugin(bool isEnable)
        {
            string[] command = new string[]
            {
                "procon.protected.plugins.enable",
                ClassName,
                isEnable.ToString(),
            };
            ExecuteCommand(command);
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
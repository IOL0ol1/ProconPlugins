using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using PRoCon.Core;
using PRoCon.Core.Plugin;

namespace PRoConEvents
{
    public class RemoteManager : PRoConPluginAPI, IPRoConPluginInterface
    {
        private bool isEnable = false;
        private int isRestart = 0;
        private RemoteCommand remoteCommand = new RemoteCommand();

        #region Menu List

        public const string RconCommand = "1 RconCmd";
        public const string RestartHeader = "2 Restart";
        public const string RemotShellHeader = "3 RemoteShell";

        [Menu(RconCommand, "Rcon Cmd")]
        private string rconCmd = string.Empty;

        [Menu(RconCommand, "Rcon Cmd History")]
        private string[] rconCmdHistory = new string[0];

        [Menu(RestartHeader, "Restart Warning!!")]
        private readonly string restartWarning = "Must check 'auto connection' on remote procon.";

        [Menu(RestartHeader, "Restart Remote Procon")]
        private enumBoolYesNo restartProcon = enumBoolYesNo.No;

        [Menu(RestartHeader, "Confirm Restart")]
        private enumBoolYesNo confirmRestart = enumBoolYesNo.No;

        [Menu(RemotShellHeader, "RemoteShell Warning!!")]
        private readonly string remoteShellWarning = "Change 'shell exec' need reenable shell to take effect";

        [Menu(RemotShellHeader, "Shell Exec")]
        private string remoteShellName = "powershell.exe";

        [Menu(RemotShellHeader, "Enable Shell")]
        private bool remoteShellEnable = false;

        [Menu(RemotShellHeader, "Remote Cmd")]
        private string remotrCmd = string.Empty;

        [Menu(RemotShellHeader, "Remote Cmd History")]
        private string[] remoteCmdHistory = new string[0];

        private List<CPluginVariable> GetVariables(bool isDisplay)
        {
            List<CPluginVariable> pluginVariables = new List<CPluginVariable>();

            pluginVariables.Add(CreateVariable(() => rconCmd, isDisplay));
            pluginVariables.Add(CreateVariable(() => rconCmdHistory, isDisplay));
            pluginVariables.Add(CreateVariable(() => restartWarning, isDisplay));
            pluginVariables.Add(CreateVariable(() => restartProcon, isDisplay));
            if (restartProcon == enumBoolYesNo.Yes)
            {
                pluginVariables.Add(CreateVariable(() => confirmRestart, isDisplay));
            }

            pluginVariables.Add(CreateVariable(() => remoteShellWarning, isDisplay));
            pluginVariables.Add(CreateVariable(() => remoteShellName, isDisplay));
            pluginVariables.Add(CreateVariable(() => remoteShellEnable, isDisplay));
            if (remoteShellEnable)
            {
                pluginVariables.Add(CreateVariable(() => remotrCmd, isDisplay));
                pluginVariables.Add(CreateVariable(() => remoteCmdHistory, isDisplay));
            }

            return pluginVariables;
        }

        #endregion

        #region IPRoConPluginInterface

        public List<CPluginVariable> GetDisplayPluginVariables()
        {
            if (!string.IsNullOrEmpty(rconCmd.Trim()))
            {
                var console = new PRoConTraceListener(this, 1);
                Output.Listeners.Add(console);
                Command(rconCmd.Split(' '));
                var tmpList = rconCmdHistory.ToList();
                tmpList.Add(rconCmd);
                rconCmdHistory = tmpList.ToArray();
                rconCmd = string.Empty;
                Output.Listeners.Remove(console);
            }

            if (remoteShellEnable)
            {
                if (!remoteCommand.IsRunning)
                    remoteCommand.Start(remoteShellName);

                if (!string.IsNullOrEmpty(remotrCmd.Trim()))
                {
                    remoteCommand.Executed(remotrCmd);
                    var tmpList = remoteCmdHistory.ToList();
                    tmpList.Add(remotrCmd);
                    remoteCmdHistory = tmpList.ToArray();
                    remotrCmd = string.Empty;
                }
            }
            else
            {
                remoteCommand.Close();
            }

            if (restartProcon == enumBoolYesNo.Yes && confirmRestart == enumBoolYesNo.Yes)
            {
                confirmRestart = enumBoolYesNo.No;
                restartProcon = enumBoolYesNo.No;
                isRestart = 1;
            }
            else
            {
                // NOTE: MUST check autoconnection on remote PRoCon before restart.
                if (isRestart > 0)
                {
                    isRestart = 0;
                    RestartPRoCon();
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

            remoteCommand.OnError = _ => Output.TraceError(_);
            remoteCommand.OnOutput = _ => Output.TraceInformation(_);

            // Get common events in this class and PRoConPluginAPI
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            IEnumerable<string> baseMethods = typeof(PRoConPluginAPI).GetMethods().Where(_ => _.IsVirtual).Select(_ => _.Name);
            IEnumerable<string> commonMethods = GetType().GetMethods(bindingFlags).Where(_ => _.IsVirtual).Select(_ => _.Name).Intersect(baseMethods);
            RegisterEvents(ClassName, commonMethods.ToArray());
        }

        public void SetPluginVariable(string strVariable, string strValue)
        {
            if (!isEnable)
            {
                Output.TraceWarning("Enable plugin to use all function!");
                return;
            }
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

        #region Private Methods

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
        /// Restart remote PRoCon use powershell.
        /// <para>NOTE: MUST check the auto connection on remote PRoCon.</para>
        /// </summary>
        private void RestartPRoCon()
        {
            try
            {
                Output.TraceWarning("Restart PRoCon {0}", DateTime.Now);
                string startProcessCmd = string.Empty;
                string processName = Process.GetCurrentProcess().ProcessName;
                string processFileName = Process.GetCurrentProcess().MainModule.FileName;

                // check for procon.service.exe
                if (processName.ToLower().Contains("service"))
                {
                    // Reflection load missing dll
                    string assemblyString = "System.Management, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
                    string typeName = "System.Management.ManagementObjectSearcher";
                    object searcher = Assembly.Load(assemblyString).CreateInstance(typeName);
                    object query = searcher.GetType().GetProperty("Query").GetValue(searcher, null);
                    query.GetType().GetProperty("QueryString").SetValue(query, "Select * from Win32_Service where ProcessId = " + Process.GetCurrentProcess().Id.ToString(), null);
                    IEnumerable collection = searcher.GetType().GetMethod("Get", Type.EmptyTypes).Invoke(searcher, null) as IEnumerable;
                    string serviceName = string.Empty;
                    foreach (object item in collection)
                    {
                        serviceName = item.GetType().GetProperty("Item").GetValue(item, new object[] { "Name" }).ToString();
                    }

                    if (string.IsNullOrEmpty(serviceName))
                    {
                        Output.TraceError("Restart failed! Not found service");
                        return;
                    }
                    startProcessCmd = string.Format("start-service '{0}';", serviceName);
                }
                else
                {
                    string commandLine = Environment.CommandLine;
                    string argumentList = commandLine.Split(new string[] { "\"" + processFileName + "\"" }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault().Trim();
                    if (!string.IsNullOrEmpty(argumentList))
                    {
                        argumentList = string.Format("-argumentlist '{0}'", argumentList);
                    }

                    startProcessCmd = string.Format("start-process '{0}' {1};", processFileName, argumentList);
                }

                int seconds = 10;
                string stopProcessCmd = string.Format("stop-process -id {0}", Process.GetCurrentProcess().Id);
                string startSleepCmd = string.Format("start-sleep -seconds {0}", seconds);
                string checkAndStartCmd = string.Format("if(!(get-process -name '{0}'|where path -eq '{1}')){{{2}}}", processName, processFileName, startProcessCmd);
                string cmd = string.Format("-windowstyle hidden -command \"{0};{1};{2}\"", stopProcessCmd, startSleepCmd, checkAndStartCmd);
                Output.TraceWarning("Restart after {0} seconds", seconds);
                // use powershell can hidden powershell window
                Process.Start("powershell.exe", cmd);
            }
            catch (Exception ex)
            {
                Output.TraceError(ex.Message);
                Output.TraceError(ex.StackTrace);
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

    #region Remote Command

    internal class RemoteCommand : IDisposable
    {
        public Action<string> OnOutput;
        public Action<string> OnError;
        private Process process = new Process();
        public bool IsRunning { get; set; }

        public RemoteCommand()
        {
            IsRunning = false;
        }

        public void Start(string fileName)
        {
            try
            {
                process = new Process();
                process.StartInfo.FileName = fileName;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false; // for redirect
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                process.StandardInput.AutoFlush = true;
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.ErrorDataReceived += Process_ErrorDataReceived;
                process.OutputDataReceived += Process_OutputDataReceived;
                IsRunning = true;
            }
            catch (Exception ex)
            {
                if (OnError != null)
                    OnError(ex.Message);
            }
        }

        public void Executed(string command)
        {
            if (IsRunning)
                process.StandardInput.WriteLine(command);
        }

        public void Close()
        {
            process.Dispose();
            GC.Collect(); // close shell faster
            IsRunning = false;
        }

        public void Dispose()
        {
            Close();
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (OnError != null)
                OnError(e.Data);
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (OnOutput != null)
                OnOutput(e.Data);
        }

        #endregion
    }
}
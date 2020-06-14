using PRoCon.Core;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Players;
using PRoCon.Core.Plugin;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PRoConEvents
{
    public class PlayerLogger : PRoConPluginAPI, IPRoConPluginInterface
    {
        private bool isEnable = false;

        #region Menu List

 

        // Add some code

        private List<CPluginVariable> GetVariables(bool isAddHeader)
        {
            List<CPluginVariable> pluginVariables = new List<CPluginVariable>();
            return pluginVariables;
        }

        #endregion

        #region IPRoConPluginInterface

        public List<CPluginVariable> GetDisplayPluginVariables()
        {
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
            Output.Listeners.Add(new TextWriterTraceListener(ClassName + "_" + strHostName + "_" + strPort + ".log")); // output to debug file
            Output.Listeners.Add(new PRoConTraceListener(this, 1)); // output to chat
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
 
        public override void OnResponseError(List<string> requestWords, string error)
        {
            base.OnResponseError(requestWords, error);
            Output.Information("{0}: Error:{1}", "OnResponseError", error);
            foreach (var item in requestWords)
            {
                Output.Information("{0}: Request:{1}", "OnResponseError", item);
            }
        }

        public override void OnRunScript(string scriptFileName)
        {
            base.OnRunScript(scriptFileName);
            Output.Information("{0}: ScripFileName:{1}", "OnRunScript", scriptFileName);
        }
        public override void OnRunScriptError(string scriptFileName, int lineError, string errorDescription)
        {
            base.OnRunScriptError(scriptFileName, lineError, errorDescription);
            Output.Information("{0}: ScripFileName:{1} LineError:{2} Error:{3}", "OnRunScriptError", scriptFileName, lineError, errorDescription);
        }

        public override void OnReceiveProconVariable(string variableName, string value)
        {
            base.OnReceiveProconVariable(variableName, value);
            Output.Information("{0}: VarName:{1} Value:{2}", "OnReceiveProconVariable", variableName, value);
        }

        public override void OnLevelVariablesEvaluate(LevelVariable requestedContext, LevelVariable returnedValue)
        {
            base.OnLevelVariablesEvaluate(requestedContext, returnedValue);
            Output.Information("{0}: Request VarName:{1} Value:{2} Type:{3} Target:{4}", "OnLevelVariablesEvaluate", requestedContext.VariableName, requestedContext.RawValue, requestedContext.Context.ContextType, requestedContext.Context.ContextTarget);
            Output.Information("{0}: Return  VarName:{1} Value:{2} Type:{3} Target:{4}", "OnLevelVariablesEvaluate", returnedValue.VariableName, returnedValue.RawValue, returnedValue.Context.ContextType, returnedValue.Context.ContextTarget);
        }

        public override void OnLevelVariablesList(LevelVariable requestedContext, List<LevelVariable> returnedValues)
        {
            base.OnLevelVariablesList(requestedContext, returnedValues);
            Output.Information("{0}: Request VarName:{1} Value:{2} Type:{3} Target:{4}", "OnLevelVariablesList", requestedContext.VariableName, requestedContext.RawValue, requestedContext.Context.ContextType, requestedContext.Context.ContextTarget);
            foreach (var returnedValue in returnedValues)
            {
                Output.Information("{0}: Return  VarName:{1} Value:{2} Type:{3} Target:{4}", "OnLevelVariablesList", returnedValue.VariableName, returnedValue.RawValue, returnedValue.Context.ContextType, returnedValue.Context.ContextTarget);
            }
        }

        public override void OnZoneTrespass(CPlayerInfo playerInfo, ZoneAction action, MapZone sender, Point3D tresspassLocation, float tresspassPercentage, object trespassState)
        {
            base.OnZoneTrespass(playerInfo, action, sender, tresspassLocation, tresspassPercentage, trespassState);
            Output.Information("{0}: SoldierName:{1} Action:{2} Level:{3} Uid:{4} ZoneInclusive:{5} Location:({6},{7},{8}) Percentage:{9} State:{10}",
                "OnZoneTrespass",
                playerInfo.SoldierName,
                action,
                sender.LevelFileName,
                sender.UID,
                sender.ZoneInclusive,
                tresspassLocation.X, tresspassLocation.Y, tresspassLocation.Z,
                tresspassPercentage,
                trespassState);
            foreach (var item in sender.Tags)
            {
                Output.Information("{0}: MapZone Tag:{1}", "OnZoneTrespass", item);
            }
            foreach (var item in sender.ZonePolygon)
            {
                Output.Information("{0}: MapZone Polygon:({1},{2},{3})", "OnZoneTrespass", item.X, item.Y, item.Z);
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
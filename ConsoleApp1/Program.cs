using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Web.UI.WebControls;
using PRoCon.Core;
using PRoCon.Core.Players;
using PRoConEvents;

namespace ConsoleApp1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                ServerLogger serverLogger = new ServerLogger();
                serverLogger.ClassName = nameof(ServerLogger);
                serverLogger.OnPluginLoaded("", "", "");
                serverLogger.OnPluginEnable();
                serverLogger.OnPluginDisable();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message + ex.StackTrace);
            }

            Console.ReadLine();
        }
    }
}
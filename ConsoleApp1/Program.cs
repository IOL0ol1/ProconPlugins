using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Web.UI.WebControls;
using PRoCon.Core;
using PRoCon.Core.Players;
using PRoCon.Core.Plugin;
using PRoConEvents;

namespace ConsoleApp1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {

                FriendManager friendManager = CreatePlugin<FriendManager>();
                friendManager.OnPluginLoaded("", "", "");
                friendManager.OnPluginEnable();
                friendManager.OnPluginDisable();


                RemoteManager remoteManager = CreatePlugin<RemoteManager>();
                remoteManager.OnPluginLoaded("", "", "");
                remoteManager.OnPluginEnable();
                remoteManager.OnPluginDisable();


                ServerLogger serverLogger = CreatePlugin<ServerLogger>();
                serverLogger.OnPluginLoaded("", "", "");
                serverLogger.OnPluginEnable();
                serverLogger.OnPluginDisable();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
            Console.ReadLine();
        }

        private static T CreatePlugin<T>() where T : PRoConPluginAPI, new()
        {
            T plugin = new T();
            plugin.ClassName = typeof(T).Name;
            return plugin;
        }

    }
}
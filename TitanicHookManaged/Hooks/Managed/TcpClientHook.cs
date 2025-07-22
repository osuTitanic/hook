using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Windows.Forms;
using Harmony;
using TitanicHookShared;

namespace TitanicHookManaged.Hooks.Managed;

public class TcpClientHook
{
    public const string HookName = "sh.Titanic.Hook.TcpClient";
    
    public static void Initialize()
    {
        Logging.HookStart(HookName);
        
        // Get Titanic's Bancho IP address
        Logging.HookStep(HookName, $"Resolving server.{EntryPoint.Config.ServerName} IP");
        _newIp = Dns.GetHostAddresses($"server.{EntryPoint.Config.ServerName}")[0];
        if (_newIp == null)
        {
            Logging.HookError(HookName, $"Couldn't resolve server.{EntryPoint.Config.ServerName}");
            return;
        }
        Logging.HookStep(HookName, "Bancho service IP: " + _newIp);
        
        var harmony = HarmonyInstance.Create(HookName);
        
        // Look for BeginConnect(string, int, AsyncCallback, object) overload
        MethodInfo? beginConnect = typeof(TcpClient)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(m => m.Name == "BeginConnect" &&
                                 m.GetParameters().Length == 4 &&
                                 m.GetParameters()[0].ParameterType.FullName == "System.String");
        if (beginConnect == null)
        {
            Logging.HookError(HookName, "Could not find TcpClient.BeginConnect(string, int, AsyncCallback, object)");
        }
        
        // Look for Connect(string, int) overload
        MethodInfo? connect = typeof(TcpClient)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(m => m.Name == "Connect" &&
                                 m.GetParameters().Length == 2 &&
                                 m.GetParameters()[0].ParameterType.FullName == "System.String");
        if (connect == null)
        {
            Logging.HookError(HookName, "Could not find TcpClient.BeginConnect(string, int, AsyncCallback, object)");
        }
        
        var prefix = typeof(TcpClientHook).GetMethod("TcpConnectPrefix", Constants.HookBindingFlags);

        try
        {
            Logging.HookStep(HookName, "Patching");
            if (beginConnect != null) harmony.Patch(beginConnect, new HarmonyMethod(prefix));
            if (connect != null) harmony.Patch(connect, new HarmonyMethod(prefix));
        }
        catch (Exception e)
        {
            Logging.HookError(HookName, e.ToString());
        }
        
        Logging.HookDone(HookName);
    }

    #region Hook

    private static void TcpConnectPrefix(ref string __0)
    {
        Logging.HookTrigger(HookName);
        if (_banchoIpList.Contains(__0))
        {
            Logging.HookOutput(HookName, "Replacing IP");
            __0 = _newIp.ToString();
        }
    }

    #endregion
    
    #region Properties
    
    /// <summary>
    /// List of IP addresses originally used for Bancho
    /// </summary>
    static readonly List<string> _banchoIpList =
    [
        "50.23.74.93", "219.117.212.118", "192.168.1.106", "174.34.145.226", "216.6.228.50",
        "50.228.6.216", "69.147.233.10", "167.83.161.203", "10.233.147.69", "1.0.0.127",
        "53.228.6.216", "52.228.6.216", "51.228.6.216", "50.228.6.216", "151.0.0.10"
    ];

    /// <summary>
    /// Redirected Bancho IP address
    /// </summary>
    private static IPAddress? _newIp = null;

    #endregion
}

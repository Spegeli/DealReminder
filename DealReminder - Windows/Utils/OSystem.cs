using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using DealReminder_Windows.Logging;
using Microsoft.Win32;

namespace DealReminder_Windows.Utils
{
    internal class OSystem
    {
        public static readonly RegistryKey RegistryKey = Registry.CurrentUser.OpenSubKey
            ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        public static void RegisterInStartup(bool isChecked)
        {
            if (isChecked)
                RegistryKey.SetValue("DealReminder", Application.ExecutablePath);
            else
                RegistryKey.DeleteValue("DealReminder", false);
        }

        public static bool RegisterInStartupExists()
        {
            return RegistryKey.GetValue("DealReminder") != null;
        }

        public static bool IsWindows()
        {
            var os = RunningPlatform().ToString();
            return os == "Windows";
        }

        public enum Platform
        {
            Windows,
            Linux,
            Mac
        }
        public static Platform RunningPlatform()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    // Well, there are chances MacOSX is reported as Unix instead of MacOSX.
                    // Instead of platform check, we'll do a feature checks (Mac specific root folders)
                    if (Directory.Exists("/Applications")
                        & Directory.Exists("/System")
                        & Directory.Exists("/Users")
                        & Directory.Exists("/Volumes"))
                        return Platform.Mac;
                    else
                        return Platform.Linux;

                case PlatformID.MacOSX:
                    return Platform.Mac;

                default:
                    return Platform.Windows;
            }
        }

        public static bool IsAvailableNetworkActive()
        {
            // only recognizes changes related to Internet adapters
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                Logger.Write("Es besteht wohl aktuell keine Internetverbindung! - Code #1");
                return false;
            }

            // however, this will include all adapters -- filter by opstatus and activity
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            var connectionexists = (from face in interfaces
                where face.OperationalStatus == OperationalStatus.Up
                where face.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                      face.NetworkInterfaceType != NetworkInterfaceType.Loopback
                where !(face.Name.ToLower().Contains("virtual") || face.Description.ToLower().Contains("virtual"))
                select face.GetIPv4Statistics()).Any(
                statistics => statistics.BytesReceived > 0 && statistics.BytesSent > 0);
            if (!connectionexists)
            {
                Logger.Write("Es liegt wohl ein Fehler mit der Internetverbindung vor! - Code #2");
                return false;
            }

            try
            {
                String host = "google.com";
                byte[] buffer = new byte[32];
                int timeout = 1000; //120?
                PingReply reply = new Ping().Send(host, timeout, buffer);
                if (reply != null && reply.Status == IPStatus.Success) return true;
                Logger.Write("Es liegt wohl ein Fehler mit der Internetverbindung vor! - Code #3");
                return false;
            }
            catch (Exception)
            {
                Logger.Write("Es liegt wohl ein Fehler mit der Internetverbindung vor! - Code #3.1");
                return false;
            }
        }
    }
}

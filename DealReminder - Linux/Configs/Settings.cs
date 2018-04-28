using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using DealReminder_Linux.Logging;
using DealReminder_Linux.Properties;
using Mono.Data.Sqlite;

namespace DealReminder_Linux.Configs
{
    internal class Settings
    {
        public static readonly Dictionary<string, string> SettingsKeys = new Dictionary<string, string>();
        public static string SettingsFile = Path.Combine(FoldersFilesAndPaths.Settings, "Settings.config");
        public static Configuration Config = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap { ExeConfigFilename = SettingsFile }, ConfigurationUserLevel.None);

        public static bool IsTrial = true;
        public static int MaxActiveProducts = 1;

        public static int ActiveProducts()
        {
            Database.OpenConnection();
            SqliteCommand checkEntry = new SqliteCommand(
                "SELECT COUNT(*) FROM Products WHERE Status = '0'",
                Database.Connection);
            return Convert.ToInt32(checkEntry.ExecuteScalar());
        }

        public static void StartUpCheck()
        {
            Logger.Write("Überprüfe Settings Config Datei...");

            CreateConfigFileIfNotExists();

            SettingsKeys.Add("Debug", "False");
            SettingsKeys.Add("StartWithWindows", "False"); //Renamed V0.0.1.9
            SettingsKeys.Add("MinimizeToTray", "False");
            SettingsKeys.Add("AutoLogin", "False");
            SettingsKeys.Add("SaveLoginCredits", "False");
            SettingsKeys.Add("LoginCredits", null);
            SettingsKeys.Add("ReminderEmail", null);
            SettingsKeys.Add("ReminderTelegram", null);
            SettingsKeys.Add("ShowOnlyDealConditions", "False"); //V0.0.1.4
            SettingsKeys.Add("ScanNew", "True"); //V0.0.1.7 //Renamed V0.0.1.9
            SettingsKeys.Add("ScanUsed", "True"); //V0.0.1.7 //Renamed V0.0.1.9
            SettingsKeys.Add("SellerForNew", "Amazon"); //V0.0.1.7
            SettingsKeys.Add("SellerForUsed", "Amazon"); //V0.0.1.7
            SettingsKeys.Add("StartCrawlerAfterStartup", "False"); //V0.0.1.9

            if (Config.AppSettings.Settings["StartUp"] != null) //V0.0.1.9
            {
                Config.AppSettings.Settings.Add("StartWithWindows", Config.AppSettings.Settings["StartUp"].Value);
                Config.AppSettings.Settings.Remove("StartUp");
                Logger.Write("[UMBENANNT] Key: StartUp in StartWithWindows - Value: " + Config.AppSettings.Settings["StartWithWindows"].Value);
            }
            if (Config.AppSettings.Settings["UseNew"] != null) //V0.0.1.9
            {
                Config.AppSettings.Settings.Add("ScanNew", Config.AppSettings.Settings["UseNew"].Value);
                Config.AppSettings.Settings.Remove("UseNew");
                Logger.Write("[UMBENANNT] Key: UseNew in ScanNew - Value: " + Config.AppSettings.Settings["ScanNew"].Value);
            }
            if (Config.AppSettings.Settings["UseUsed"] != null) //V0.0.1.9
            {
                Config.AppSettings.Settings.Add("ScanUsed", Config.AppSettings.Settings["UseUsed"].Value);
                Config.AppSettings.Settings.Remove("UseUsed");
                Logger.Write("[UMBENANNT] Key: UseUsed in ScanUsed - Value: " + Config.AppSettings.Settings["ScanUsed"].Value);
            }
            foreach (var pair in SettingsKeys)
                if (Config.AppSettings.Settings[pair.Key] == null)
                {
                    Logger.Write("[HINZUGEFÜGT] Key: " + pair.Key + " - Value: " + pair.Value);
                    Config.AppSettings.Settings.Add(pair.Key, pair.Value);
                }
                else
                {
                    switch (pair.Key)
                    {
                        case "LoginCredits":
                            Logger.Write("[VORHANDEN] Key: " + pair.Key + " - Value: CENSORED");
                            break;
                        default:
                            Logger.Write("[VORHANDEN] Key: " + pair.Key + " - Value: " + Config.AppSettings.Settings[pair.Key].Value);
                            break;
                    }
                }
            Config.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection("appSettings");

            Logger.Write("Settings Config Überprüfung beendet...");
        }

        public static void CreateConfigFileIfNotExists()
        {
            if (File.Exists(SettingsFile)) return;
            Logger.Write("Settings Config nicht vorhanden. Neue Settings Config (Settings\\Settings.config) wird erstellt.");
            try
            {
                File.WriteAllText(SettingsFile, Resources.Settings);
                long fileSize = 0;
                var currentFile = new FileInfo(SettingsFile);
                while (fileSize < currentFile.Length) //check size is stable or increased
                {
                    fileSize = currentFile.Length; //get current size
                    Thread.Sleep(500); //wait a moment for processing copy
                    currentFile.Refresh(); //refresh length value
                }
                Logger.Write("Settings Config (Settings\\Settings.config) Erfolgreich erstellt!");
            }
            catch (Exception ex)
            {
                Logger.Write("Erstellen der Settings Config (Settings\\Settings.config) Fehlgeschlagen - Grund: " + ex.Message + "\r\nDealReminder wird beendet!");
                Application.Exit();
            }
        }

        public static T Get<T>(string key, T defaultValue = default(T)) where T : IConvertible
        {
            string val = Config.AppSettings.Settings[key].Value ?? string.Empty;
            T result = defaultValue;
            if (!string.IsNullOrEmpty(val))
            {
                T typeDefault = default(T);
                if (typeof(T) == typeof(String))
                {
                    typeDefault = (T) (object) String.Empty;
                }
                result = (T) Convert.ChangeType(val, typeDefault.GetTypeCode());
            }
            return result;
        }
    }
}

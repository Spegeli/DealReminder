using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using DealReminder_Windows.Logging;
using DealReminder_Windows.Properties;
using DealReminder_Windows.Utils;

namespace DealReminder_Windows.Configs
{
    internal class Settings
    {
        public static string SettingsFile = Path.Combine(FoldersFilesAndPaths.Settings, "Settings.config");
        public static bool IsPremium = false;
        public static DateTime? PremiumExpiryDate = null;

        public static int ActiveProducts()
        {
            Database.OpenConnection();
            SQLiteCommand checkEntry = new SQLiteCommand(
                "SELECT COUNT(*) FROM Products WHERE Status = '0'",
                Database.Connection);
            return Convert.ToInt32(checkEntry.ExecuteScalar());
        }

        public static void StartUpCheck()
        {
            Logger.Write("Überprüfe Settings Config Datei...");

            CreateConfigFileIfNotExists();

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            //Rename Entrys
            Dictionary<string, string> renameKeys = new Dictionary<string, string>
            {
                {"StartUp", "StartWithWindows"}, //Renamed V0.0.1.9
                {"UseNew", "ScanNew"}, //Renamed V0.0.1.9
                {"UseUsed", "ScanUsed"}, //Renamed V0.0.1.9
                {"LoginCredits", "PremiumEmail"} //Renamed V0.0.1.9
            };
            foreach (var pair in renameKeys)
            {
                if (config.AppSettings.Settings[pair.Key] == null) continue;
                config.AppSettings.Settings.Add(pair.Value, config.AppSettings.Settings[pair.Key].Value);
                Logger.Write($"[UMBENANNT] Key: {pair.Key} in {pair.Value} - Value: " +
                             config.AppSettings.Settings[pair.Value].Value);
            }

            //Delete Entrys
            List<string> deleteKeys = new List<string>
            {
                "StartUp", //Renamed V0.0.1.9
                "UseNew", //Renamed V0.0.1.9
                "UseUsed", //Renamed V0.0.1.9
                "LoginCredits", //Renamed V0.0.1.9     
                "AutoLogin", //Removed V0.0.1.9       
                "SaveLoginCredits", //Removed V0.0.1.9  
                "UseNormalProxies" //Removed V2.0.14.6
            };
            foreach (string entry in deleteKeys)
            {
                if (config.AppSettings.Settings[entry] == null) continue;               
                Logger.Write("[ENTFERNT] Key: " + entry + " - Value: " + config.AppSettings.Settings[entry].Value);
                config.AppSettings.Settings.Remove(entry);
            }

            //Check & Add Entrys
            Dictionary<string, string> settingsKeys = new Dictionary<string, string>
            {
                {"Debug", "False"},
                {"StartWithWindows", "False"}, //Renamed V0.0.1.9
                {"MinimizeToTray", "False"},
                {"PremiumEmail", null},
                {"PremiumKey", null},
                {"ReminderEmail", null},
                {"ReminderTelegram", null},
                {"ShowOnlyDealConditions", "False"}, //Added V0.0.1.4
                {"ScanNew", "True"}, //Added V0.0.1.7 //Renamed V0.0.1.9
                {"ScanUsed", "True"}, //Added V0.0.1.7 //Renamed V0.0.1.9
                {"SellerForNew", "Amazon"}, //Added V0.0.1.7
                {"SellerForUsed", "Amazon"}, //Added V0.0.1.7
                {"StartCrawlerAfterStartup", "False"}, //Added V0.0.1.9
                {"StartMinimized", "False"}, //Added V0.0.1.9
                {"RemindResendAfterMinutes", "60"}, //Added V0.0.1.11
                {"UseTorProxies", "True"}, //Added V0.0.1.11
                {"ProxyAlwaysActive", "False"}, //Added V0.0.1.11
                {"ScanMethod", "0"}, //Added V0.0.1.11
                {"DeleteOldLogsAfterDays", "7"} //Added V2.0.11.1 
            };
            foreach (var pair in settingsKeys)
                if (config.AppSettings.Settings[pair.Key] == null)
                {
                    config.AppSettings.Settings.Add(pair.Key, pair.Value);
                    Logger.Write("[HINZUGEFÜGT] Key: " + pair.Key + " - Value: " + pair.Value);
                }
                else
                {
                    switch (pair.Key)
                    {
                        case "PremiumEmail":
                            Logger.Write("[VORHANDEN] Key: " + pair.Key + " - Value: CENSORED");
                            break;
                        case "PremiumKey":
                            Logger.Write("[VORHANDEN] Key: " + pair.Key + " - Value: CENSORED");
                            break;
                        case "ReminderEmail":
                            Logger.Write("[VORHANDEN] Key: " + pair.Key + " - Value: CENSORED");
                            break;
                        case "ReminderTelegram":
                            Logger.Write("[VORHANDEN] Key: " + pair.Key + " - Value: CENSORED");
                            break;
                        default:
                            Logger.Write("[VORHANDEN] Key: " + pair.Key + " - Value: " + config.AppSettings.Settings[pair.Key].Value);
                            break;
                    }
                }

            if (!OSystem.RegisterInStartupExists())
                config.AppSettings.Settings["StartWithWindows"].Value = Convert.ToString(OSystem.RegisterInStartupExists());
            //Evtl. else so das der StartUp Path in der RegEdit Aktualisiert wird.
            //Tools.RegisterInStartup(Convert.ToBoolean(config.AppSettings.Settings["StartUp"].Value));
            config.Save(ConfigurationSaveMode.Modified);
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
            string val = ConfigurationManager.AppSettings[key] ?? string.Empty;
            T result = defaultValue;
            if (!string.IsNullOrEmpty(val))
            {
                T typeDefault = default(T);
                if (typeof(T) == typeof(String))
                {
                    typeDefault = (T)(object)String.Empty;
                }
                result = (T)Convert.ChangeType(val, typeDefault.GetTypeCode());
            }
            return result;
        }

        /// <summary>
        /// Use your own App.Config file instead of the default.
        /// </summary>
        /// <param name="NewAppConfigFullPathName"></param>
        public static void ChangeAppConfig(string NewAppConfigFullPathName)
        {
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", NewAppConfigFullPathName);
            ResetConfigMechanism();
        }

        /// <summary>
        /// Remove cached values from ClientConfigPaths.
        /// Call this after changing path to App.Config.
        /// </summary>
        private static void ResetConfigMechanism()
        {
            BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Static;
            typeof(ConfigurationManager)
                .GetField("s_initState", Flags)
                .SetValue(null, 0);

            typeof(ConfigurationManager)
                .GetField("s_configSystem", Flags)
                .SetValue(null, null);

            typeof(ConfigurationManager)
                .Assembly.GetTypes()
                .Where(x => x.FullName == "System.Configuration.ClientConfigPaths")
                .First()
                .GetField("s_current", Flags)
                .SetValue(null, null);
        }
    }
}

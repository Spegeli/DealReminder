using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using DealReminder_Windows.Configs;
using DealReminder_Windows.GUI;
using DealReminder_Windows.Logging;
using DealReminder_Windows.Tasks;
using DealReminder_Windows.Utils;
using Settings = DealReminder_Windows.Configs.Settings;

namespace DealReminder_Windows
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data"));
            Settings.ChangeAppConfig(Settings.SettingsFile);

            // For the sake of this example, we're just printing the arguments to the console.
            /*
            for (int i = 0; i < args.Length; i++)
            {
                MessageBox.Show("args["+ i + "] == " + args[i]);
            }
            */

            Logger.SetLogger();

            if (Updater.UpdateAvailable())
            {
                Logger.Write("Loading Updater GUI...");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Updater());
                return;
            }

            FoldersFilesAndPaths.StartUpCheck();
            Settings.StartUpCheck();
            Database.StartUpCheck();

            if (!String.IsNullOrWhiteSpace(Settings.Get<string>("PremiumEmail")) &&
                !String.IsNullOrWhiteSpace(Settings.Get<string>("PremiumKey")))
            {
                Premium.CheckAutomatic();
            }

            if (!Settings.IsPremium && Process
                    .GetProcessesByName(
                        Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location))
                    .Length > 1)
            {
                Logger.Write(
                    "DealReminder läuft bereits....Multi Instanzen nur mit Premium möglich...");
                MessageBox.Show(@"DealReminder läuft bereits!" + Environment.NewLine +
                                @"Nur mit Premium sind keine mehrfach Instanzen von DealReminder erlaubt!" +
                                Environment.NewLine +
                                @"Diese DealReminder Instanz wird beendet!",
                    @"DealReminder für Amazon", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            //Browser Link zum Premium Kaufen
            //if (!Settings.IsPremium)
                //WebUtils.OpenBrowser("http://amzn.to/2rPwKP1");

            Logger.Write("Loading Main GUI...");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}

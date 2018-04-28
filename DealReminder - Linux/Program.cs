using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using DealReminder_Linux.Configs;
using DealReminder_Linux.GUI;
using DealReminder_Linux.Logging;

namespace DealReminder_Linux
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

            if (Settings.Get<bool>("AutoLogin") && Login.AutoLoginSuccess())
            {
                Logger.Write("Loading Main GUI...");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Main());
            }
            else
            {
                Logger.Write("Loading Login GUI...");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Login());
            }
        }
    }
}

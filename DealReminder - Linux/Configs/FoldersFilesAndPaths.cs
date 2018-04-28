using System;
using System.IO;
using System.Windows.Forms;
using DealReminder_Linux.Logging;

namespace DealReminder_Linux.Configs
{
    internal class FoldersFilesAndPaths
    {
        public static readonly string Main = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string Data = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        public static readonly string Export = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Export");
        public static readonly string Settings = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");
        public static readonly string Logs = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        //Path.Combine(Folders.Main, filename);

        public static void StartUpCheck()
        {
            Logger.Write("Überprüfe Ordnerstruktur...");
            if (!Directory.Exists(Data))
            {
                Logger.Write("Ordner \"Data\" nicht vorhanden...neu erstellen.");
                try
                {
                    Directory.CreateDirectory(Data);
                }
                catch (Exception ex)
                {
                    Logger.Write("Erstellen des Ordners \"Data\" Fehlgeschlagen - Grund: " + ex.Message + "\r\nDealReminder wird beendet!");
                    Application.Exit();
                }
            }
            if (!Directory.Exists(Export))
            {
                Logger.Write("Ordner \"Export\" nicht vorhanden...neu erstellen.");
                try
                {
                    Directory.CreateDirectory(Export);
                }
                catch (Exception ex)
                {
                    Logger.Write("Erstellen des Ordners \"Data\" Fehlgeschlagen - Grund: " + ex.Message);
                }
            }
            if (!Directory.Exists(Settings))
            {
                Logger.Write("Ordner \"Settings\" nicht vorhanden...neu erstellen.");
                try
                {
                    Directory.CreateDirectory(Settings);
                }
                catch (Exception ex)
                {
                    Logger.Write("Erstellen des Ordners \"Settings\" Fehlgeschlagen - Grund: " + ex.Message + "\r\nDealReminder wird beendet!");
                    Application.Exit();
                }
            }
            Logger.Write("Ordnerstruktur Überprüfung beendet...");

            Logger.Write("Überprüfe Dateienstruktur...");
            var filemono = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mono.Data.Sqlite.dll");
            if (!File.Exists(filemono))
            {
                try
                {
                    File.WriteAllBytes(filemono, Properties.Resources.Mono_Data_Sqlite);
                }
                catch (Exception ex)
                {
                    Logger.Write("SQLite Mono Erstellen Fehlgeschlagen - Grund: " + ex.Message + "\r\nDealReminder wird beendet!");
                    Application.Exit();
                }
            }
            else
            {
                Logger.Write("Mono ist vorhanden...");
            }
            Logger.Write("Dateienstruktur Überprüfung beendet...");
        }
    }
}

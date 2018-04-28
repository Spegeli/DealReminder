using System;
using System.IO;
using System.Windows.Forms;
using DealReminder_Windows.Logging;
using DealReminder_Windows.Properties;

namespace DealReminder_Windows.Configs
{
    internal class FoldersFilesAndPaths
    {
        public static readonly string Main = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string Data = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        public static readonly string Export = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Export");
        public static readonly string Settings = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");
        public static readonly string Logs = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        public static readonly string Temp = Path.GetTempPath();
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
            var osBit = IntPtr.Size == 4 ? "x86" : "x64";
            var sqLiteFile = osBit == "x86" ? Resources.SQLite_Interop_x86 : Resources.SQLite_Interop_x64;
            var pathBit = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, osBit);
            var fileBit = Path.Combine(pathBit, "SQLite.Interop.dll");
            if (!File.Exists(fileBit))
            {
                try
                {
                    if (!Directory.Exists(pathBit))
                        Directory.CreateDirectory(pathBit);
                    File.WriteAllBytes(fileBit, sqLiteFile);
                }
                catch (Exception ex)
                {
                    Logger.Write("SQLite " + osBit + " Erstellen Fehlgeschlagen - Grund: " + ex.Message + "\r\nDealReminder wird beendet!");
                    Application.Exit();
                }
            }
            //Delete obsolete SQLite Files
            var removeBit = osBit == "x86" ? "x64" : "x86";
            var pathRemove = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, removeBit);
            if (Directory.Exists(pathRemove))
                new DirectoryInfo(pathRemove).Delete(true);

            Logger.Write("Dateienstruktur Überprüfung beendet...");
        }
    }
}

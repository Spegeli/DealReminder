using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using DealReminder_Windows.Configs;
using DealReminder_Windows.Logging;
using DealReminder_Windows.Utils;
using MetroFramework;
using MetroFramework.Forms;

namespace DealReminder_Windows.GUI
{
    public partial class Updater : MetroForm
    {
        private static readonly string UpdateFile = Path.Combine(FoldersFilesAndPaths.Main, "update.tmp");
        private static readonly string UpdateBatFile = Path.Combine(FoldersFilesAndPaths.Main, "update.bat");
        private readonly Stopwatch _sw = new Stopwatch();

        private static string _serverVersion = "0.0.0.0";
        private static string _serverHash;
        private static string _serverUrl;

        public Updater()
        {
            InitializeComponent();
        }

        public static bool UpdateAvailable()
        {
            Logger.Write("Überprüfe auf Updates...");
            DownloadInfosFromServer();
            var localversion = new Version(LocalVersion());
            var serverversion = new Version(_serverVersion);
            Logger.Write("Lokale Version: " + localversion + " - Server Version: " + serverversion);
            return localversion.CompareTo(serverversion) < 0;
        }

        public static string LocalVersion() => Application.ProductVersion;

        public static void DownloadInfosFromServer()
        {
            Logger.Write("Update Informationen werden vom Server abgefragt....");
            try
            {
                var xmlDoc = new XmlDocument();
                // Ignore Certificate validation failures (aka untrusted certificate + certificate chains)
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                xmlDoc.LoadXml(new BetterWebClient { Timeout = 10000 }.DownloadString("https://updates.speg-dev.de/GetSpecific.php?filter=DealReminder"));
                _serverVersion = xmlDoc.GetElementsByTagName("version")[0].InnerText;
                _serverHash = xmlDoc.GetElementsByTagName("hash")[0].InnerText;
                _serverUrl = xmlDoc.GetElementsByTagName("url")[0].InnerText;
                Logger.Write("Aktuelle Server Version: " + _serverVersion);
                Logger.Write("Aktuelle Server Download Hash: " + _serverHash);
                Logger.Write("Aktuelle Server Download URL: " + _serverUrl);
            }
            catch (Exception ex)
            {
                Logger.Write("Update Informationen abrufen Fehlgeschlagen - Grund: " + ex.Message);
                if (OSystem.IsAvailableNetworkActive()) { }
            }
        }

        private void Updater_Load(object sender, EventArgs e)
        {
            metroLabel6.Text = LocalVersion();
            metroLabel7.Text = _serverVersion;
        }

        private void metroTile2_Click(object sender, EventArgs e) => Application.Exit();

        private void metroTile1_Click(object sender, EventArgs e) => Application.Restart();

        private async void metroButton1_Click(object sender, EventArgs e)
        {
            metroButton1.Enabled = false;
            await Download();
            if (!Tools.CheckFileMd5(UpdateFile, _serverHash))
            {
                metroButton1.Enabled = true;
                return;
            }
            await UpdateSelf();
        }

        private async Task Download()
        {
            while (Tools.CheckFileMd5(UpdateFile, _serverHash) == false)
            {
                metroLabel2.Text = @"0 MB/s";
                metroLabel3.Text = @"0 von 0 MB's";
                metroProgressBar1.Value = 0;

                if (File.Exists(UpdateFile))
                {
                    Logger.Write("Beschädigte Update Datei gefunden...");
                    try
                    {
                        File.Delete(UpdateFile);
                        Logger.Write("Datei wurde Erfolgreich gelöscht...");
                    }
                    catch (Exception ex)
                    {
                        Logger.Write("Datei Löschen Fehlgeschlagen - Grund: " + ex.Message);
                        MetroMessageBox.Show(this, "Die Temporäre Update Datei (update.tmp) konnte nicht gelöscht werden." + Environment.NewLine + 
                            "Bitte Lösche die Datei Manuell und Versuche es anschließend Erneut!", "Löschen Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                    }                                 
                }
                Logger.Write("Update wird heruntergeladen...");
                try
                {
                    var wClient = new BetterWebClient {Timeout = 10000};
                    wClient.DownloadFileCompleted += _DownloadFileCompleted;
                    wClient.DownloadProgressChanged += _DownloadProgressChanged;
                    _sw.Start();
                    await wClient.DownloadFileTaskAsync(new Uri(_serverUrl), UpdateFile);
                }
                catch (Exception ex)
                {
                    Logger.Write("Download Fehlgeschlagen - Grund: " + ex.Message);
                    if (OSystem.IsAvailableNetworkActive()) { }
                }
                if (Tools.CheckFileMd5(UpdateFile, _serverHash)) break;
                DialogResult result = MetroMessageBox.Show(this, "Möchten Sie es erneut versuchen?", "Download Fehlgeschlagen", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                    break;
            }
        }

        private void _DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            metroLabel2.Text = $@"{e.BytesReceived/1024d/1024d/_sw.Elapsed.TotalSeconds:0.00} MB/s";
            metroLabel3.Text = $@"{e.BytesReceived/1024d/1024d:0.00} von {e.TotalBytesToReceive/1024d/1024d:0.00} MB's";
            metroProgressBar1.Value = e.ProgressPercentage;
        }

        private void _DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            _sw.Reset();
            Tools.WaitNSeconds(Tools.RandomNumber(1, 2));
            Logger.Write("Die Temporäre Update Datei (update.tmp) wurde Erfolgreich hertunergeladen...", LogLevel.Debug);
        }

        private async Task UpdateSelf()
        {
            Logger.Write("Update Installation wird vorbereitet...");
            if (File.Exists(UpdateBatFile))
                File.Delete(UpdateBatFile);
            var appName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var filePath = Path.GetDirectoryName(appName);
            var fileName = Path.GetFileName(appName);
            using (var batFile = new StreamWriter(File.Create(UpdateBatFile)))
            {
                batFile.WriteLine("@ECHO OFF");
                batFile.WriteLine("TIMEOUT /t 1 /nobreak > NUL");
                batFile.WriteLine("TASKKILL /IM \"{0}\"", fileName);
                batFile.WriteLine("DEL \"{0}\"", appName);
                batFile.WriteLine("RENAME \"{0}\" \"{1}\"", UpdateFile, fileName);
                batFile.WriteLine("DEL \"%~f0\" & START \"\" /B \"{0}\"", appName);
            }
            Logger.Write("DealReminder wird beendet und Update wird Installiert...");
            ProcessStartInfo startInfo = new ProcessStartInfo(UpdateBatFile)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = filePath
            };
            Process.Start(startInfo);
            Environment.Exit(0);
            await Task.FromResult(0);
        }
    }
}

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using DealReminder_Linux.Configs;
using DealReminder_Linux.Logging;
using DealReminder_Linux.Utils;

namespace DealReminder_Linux.GUI
{
    public partial class Updater : Form
    {
        private const string UpdateFileName = "update.tmp";
        private static readonly string UpdateFile = Path.Combine(FoldersFilesAndPaths.Main, UpdateFileName);
        private static readonly string UpdateBatFile = Path.Combine(FoldersFilesAndPaths.Main, "update.sh");
        private readonly Stopwatch _sw = new Stopwatch();
        private bool _download;
        private bool _downloadComplete;

        private static string _serverVersion = "0.0.0.0";
        private static string _serverHash = String.Empty;
        private static string _serverUrl = String.Empty;

        public Updater()
        {
            InitializeComponent();
        }

        public static bool UpdateAvailable()
        {
            Logger.Write("Überprüfe auf Updates...");
            try
            {
                DownloadInfosFromServer();
                var localversion = new Version(LocalVersion());
                var serverversion = new Version(_serverVersion);

                Logger.Write("Lokale Version: " + localversion + " - Server Version: " + serverversion);
                return localversion.CompareTo(serverversion) < 0;
            }
            catch (Exception ex)
            {
                Logger.Write("Überprüfen auf Updates Fehlgeschlagen - Grund: " + ex.Message);
            }
            return false;
        }

        public static string LocalVersion() => Application.ProductVersion;

        public static void DownloadInfosFromServer()
        {
            Logger.Write("Update Informationen werden vom Server abgefragt....");
            try
            {
                var wClient = new WebClient(); //BetterWebClient mit Timout 10000
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(wClient.DownloadString("https://updates.speg-dev.de/GetSpecific.php?filter=DealReminderLinux"));
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
                if (Tools.IsAvailableNetworkActive()) { }
            }
        }

        private void Updater_Load(object sender, EventArgs e)
        {
            label6.Text = LocalVersion();
            label7.Text = _serverVersion;
        }

        private void button1_Click(object sender, EventArgs e) => Application.Exit();

        private void button2_Click(object sender, EventArgs e) => Application.Restart();

        private void button3_Click(object sender, EventArgs e)
        {
            _download = true;
            _downloadComplete = false;

            DownloadInfosFromServer();
            Download();

            if (!Tools.CheckFileMd5(UpdateFile, _serverHash)) return;
            UpdateSelf();
        }

        public void Download()
        {
            Logger.Write("Update wird heruntergeladen...");
            while (Tools.CheckFileMd5(UpdateFile, _serverHash) == false && _download)
            {
                if (File.Exists(UpdateFile))
                {
                    Logger.Write($"Beschädigte Update Datei ({UpdateFileName}) gefunden...");
                    try
                    {
                        File.Delete(UpdateFile);
                        Logger.Write("Datei wurde Erfolgreich gelöscht...");
                    }
                    catch (Exception ex)
                    {
                        Logger.Write("Datei Löschen Fehlgeschlagen - Grund: " + ex.Message);
                        _download = false;
                        continue;
                    }
                }
                Logger.Write("Update wird heruntergeladen...");
                try
                {
                    var wClient = new WebClient(); //BetterWebClient mit Timout 10000
                    wClient.DownloadFileCompleted += _DownloadFileCompleted;
                    wClient.DownloadProgressChanged += _DownloadProgressChanged;
                    _sw.Start();
                    wClient.DownloadFileAsync(new Uri(_serverUrl), UpdateFile);
                    while (!_downloadComplete)
                        Application.DoEvents();
                }
                catch (Exception ex)
                {
                    Logger.Write("Download Fehlgeschlagen - Grund: " + ex.Message);
                    if (Tools.IsAvailableNetworkActive()) { }
                }
                if (Tools.CheckFileMd5(UpdateFile, _serverHash)) continue;
                DialogResult result = MessageBox.Show(this, "Möchten Sie es erneut versuchen?", "Download Fehlgeschlagen", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                    _download = false;
                else
                {
                    DownloadInfosFromServer();
                    _downloadComplete = false;
                    label2.Text = @"0 MB/s";
                    label3.Text = @"0 von 0 MB's";
                }
            }
        }

        private void _DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            label2.Text = $@"{e.BytesReceived / 1024d / 1024d / _sw.Elapsed.TotalSeconds:0.00} MB/s";
            label3.Text = $@"{e.BytesReceived / 1024d / 1024d:0.00} von {e.TotalBytesToReceive / 1024d / 1024d:0.00} MB's";
            progressBar1.Value = e.ProgressPercentage;
        }

        private void _DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            _sw.Reset();
            Tools.WaitNSeconds(Tools.RandomNumber(1, 2));
            _downloadComplete = true;
        }

        public static void UpdateSelf()
        {
            Logger.Write("Update Installation wird vorbereitet...");
            if (File.Exists(UpdateBatFile))
                File.Delete(UpdateBatFile);
            var appName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            if (appName == null) return;
            var filePath = Path.GetDirectoryName(appName);
            var fileName = Path.GetFileName(appName);
            using (var batFile = new StreamWriter(File.Create(UpdateBatFile)))
            {
                batFile.WriteLine("#!/bin/bash");
                batFile.WriteLine("sleep 1");
                batFile.WriteLine("killall -KILL \"{0}\"", fileName);
                batFile.WriteLine("rm \"{0}\"", appName);
                batFile.WriteLine("mv \"{0}\" \"{1}\"", UpdateFile, appName);
                //batFile.WriteLine("sleep 5");
                //batFile.WriteLine("sudo chmod a+x \"{0}\"", "/home/spegeli/Schreibtisch/DealReminder/DealReminder - Linux1.exe");
                //batFile.WriteLine("sudo \"{0}\"", "/home/spegeli/Schreibtisch/DealReminder/DealReminder - Linux1.exe");
                batFile.WriteLine("rm \"{0}\"", UpdateBatFile);
                //batFile.WriteLine("sudo mono \"{0}\"", appName);
            }
            Logger.Write("DealReminder wird beendet und Update wird Installiert...");
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "/bin/bash",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                Arguments = UpdateBatFile
            };
            Process.Start(startInfo);
            Environment.Exit(0);
        }
    }
}

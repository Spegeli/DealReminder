using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using DealReminder_Windows.Configs;
using DealReminder_Windows.Logging;
using DealReminder_Windows.Tasks;
using DealReminder_Windows.Utils;
using MetroFramework;
using MetroFramework.Forms;
using File = System.IO.File;

namespace DealReminder_Windows.GUI
{
    public partial class Main : MetroForm
    {
        public static CancellationTokenSource CancelCrawler;
        TextWriter _writer = null;

        public Main()
        {
            InitializeComponent();

            //Register Events
            Application.ApplicationExit += Application_ApplicationExit;
            var deleteOldLogsTimer = new System.Timers.Timer(1000 * 60 * 60);
            deleteOldLogsTimer.Elapsed += deleteOldLogs;
            deleteOldLogsTimer.Enabled = true;

            Shown += AfterLoad;
        }

        private void AfterLoad(object sender, EventArgs e)
        {
            if (Settings.IsPremium && Settings.Get<bool>("StartCrawlerAfterStartup"))
                StartCrawler();
            if (Settings.Get<bool>("StartMinimized"))
                WindowState = FormWindowState.Minimized;
        }

        private async void Main_Load(object sender, EventArgs e)
        {
            textBox3.Text = File.ReadAllText(Path.Combine(FoldersFilesAndPaths.Logs, Logger.CurrentFile + ".txt"));
            _writer = new TextBoxStreamWriter(textBox3);
            Console.SetOut(_writer);

            LoadSettingsToGui();
            metroLabel46.Text = Convert.ToString(await WebUtils.GetCurrentIpAddressAsync());
            metroLabel49.Text = Convert.ToString(Settings.PremiumExpiryDate);

            ProductDatabase.Display();
            MyReminder.Display();
            LoadChangelogs();

            if (Settings.IsPremium)
                EnablePremium();
            else
                DisablePremium();

            deleteOldLogs(this, null);

            metroTabControl1.SelectedTab = metroTabPage1;
        }

        void deleteOldLogs(object source, ElapsedEventArgs e)
        {
            DirectoryInfo di = new DirectoryInfo(FoldersFilesAndPaths.Logs);
            foreach (FileInfo file in di.GetFiles())
            {
                if (Path.GetFileNameWithoutExtension(file.Name) == Logger.CurrentFile || file.LastWriteTime > DateTime.Now.AddDays(Settings.Get<int>("DeleteOldLogsAfterDays"))) continue;
                try
                {
                    file.Delete();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        void Application_ApplicationExit(object sender, EventArgs e)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["Debug"].Value = Settings.Get<string>("Debug");

            //Crawler
            config.AppSettings.Settings["ScanNew"].Value = Settings.Get<string>("ScanNew");
            config.AppSettings.Settings["SellerForNew"].Value = Settings.Get<string>("SellerForNew");
            config.AppSettings.Settings["ScanUsed"].Value = Settings.Get<string>("ScanUsed");
            config.AppSettings.Settings["SellerForUsed"].Value = Settings.Get<string>("SellerForUsed");
            config.AppSettings.Settings["UseTorProxies"].Value = Settings.Get<string>("UseTorProxies");
            config.AppSettings.Settings["ProxyAlwaysActive"].Value = Settings.Get<string>("ProxyAlwaysActive");
            config.AppSettings.Settings["ScanMethod"].Value = Settings.Get<string>("ScanMethod");

            //Settings
            config.AppSettings.Settings["StartWithWindows"].Value = Settings.Get<string>("StartWithWindows");
            config.AppSettings.Settings["StartMinimized"].Value = Settings.Get<string>("StartMinimized");
            config.AppSettings.Settings["MinimizeToTray"].Value = Settings.Get<string>("MinimizeToTray");
            config.AppSettings.Settings["StartCrawlerAfterStartup"].Value = Settings.Get<string>("StartCrawlerAfterStartup");
            config.AppSettings.Settings["ShowOnlyDealConditions"].Value = Settings.Get<string>("ShowOnlyDealConditions");
            config.AppSettings.Settings["DeleteOldLogsAfterDays"].Value = Settings.Get<string>("DeleteOldLogsAfterDays");

            //Reminder
            config.AppSettings.Settings["ReminderEmail"].Value = Settings.Get<string>("ReminderEmail");
            config.AppSettings.Settings["ReminderTelegram"].Value = Settings.Get<string>("ReminderTelegram");
            config.AppSettings.Settings["RemindResendAfterMinutes"].Value = Settings.Get<string>("RemindResendAfterMinutes");

            //Premium
            config.AppSettings.Settings["PremiumEmail"].Value = Settings.Get<string>("PremiumEmail");
            config.AppSettings.Settings["PremiumKey"].Value = Settings.Get<string>("PremiumKey");

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        //--Main START
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => WebUtils.OpenBrowser("https://t.me/joinchat/AAAAAEK4xY7BkQ1QlLoNhQ");

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => WebUtils.OpenBrowser("https://s.dealreminder.de/setup-video");

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => WebUtils.OpenBrowser("https://tracker.speg-dev.de");

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => WebUtils.OpenBrowser("http://www.elitepvpers.com/forum/elite-gold-trading/4227471-spegeli-dealreminder-deine-schn-ppchen-app-f-r-amazon.html#post35666973");

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => WebUtils.OpenBrowser("https://telegram.org/");

        private async void LoadChangelogs()
        {
            Logger.Write("Lade Changelog...");
            try
            {
                // Ignore Certificate validation failures (aka untrusted certificate + certificate chains)
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                Stream data = await new BetterWebClient{ Timeout = 5000 }.OpenReadTaskAsync("https://updates.speg-dev.de/GetHistoricalChangelog.php?filter=DealReminder");
                using (var reader = new StreamReader(data))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        textBox1.AppendText(line.Replace("<br />", Environment.NewLine));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Fehler beim Laden des Changelogs - Grund: " + ex.Message);
                textBox1.AppendText("Fehler beim Laden des Changelogs..." + Environment.NewLine);
            }
            Logger.Write("Lade Changelog der nächsten Version...");
            try
            {
                // Ignore Certificate validation failures (aka untrusted certificate + certificate chains)
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                Stream data = await new BetterWebClient { Timeout = 5000 }.OpenReadTaskAsync("https://updates.speg-dev.de/GetHistoricalChangelog.php?filter=DealReminder&nextversion");
                using (var reader = new StreamReader(data))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        textBox2.AppendText(line.Replace("<br />", Environment.NewLine));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Fehler beim Laden des Changelogs der nächsten Version - Grund: " + ex.Message);
                textBox2.AppendText("Fehler beim Laden des Changelogs der nächsten Version..." + Environment.NewLine);
            }
        }

        private void metroToggle3_CheckedChanged(object sender, EventArgs e)
        {
            ConfigurationManager.AppSettings["Debug"] = Convert.ToString(metroToggle3.Checked);
        }
        //--Main ENDE

        //--Produkt Datenbank START
        private async void metroButton5_Click(object sender, EventArgs e)
        {
            string[] stores = checkedListBox2.CheckedItems.OfType<string>().ToArray();
            string asin_isbn = metroTextBox1.Text.Trim();

            checkedListBox2.Enabled = metroTextBox1.Enabled = metroButton5.Enabled = false;
            metroProgressSpinner2.Visible = true;
            await ProductDatabase.Add(stores, asin_isbn);
            checkedListBox2.Enabled = metroTextBox1.Enabled = metroButton5.Enabled = true;
            metroProgressSpinner2.Visible = false;
        }

        private void metroGrid1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || e.RowIndex <= -1) return;
            if (metroGrid1.CurrentCell.RowIndex != e.RowIndex)
                metroGrid1.ClearSelection();

            metroGrid1.CurrentCell = metroGrid1.Rows[e.RowIndex].Cells["DG1_StoreImage"];

            metroContextMenu1.Items.Clear();
            var status = Convert.ToInt16(metroGrid1.Rows[e.RowIndex].Cells["DG1_Status"].Value);
            switch (status)
            {
                case 0:
                    ToolStripItem deaktivieren = metroContextMenu1.Items.Add("Deaktivieren");
                    deaktivieren.Click += new EventHandler(cm1_deaktivieren_Click);
                    break;
                case 1:
                    ToolStripItem aktivieren = metroContextMenu1.Items.Add("Aktivieren");
                    aktivieren.Click += new EventHandler(cm1_aktivieren_Click);
                    break;
            }
            ToolStripItem loeschen = metroContextMenu1.Items.Add("Löschen");
            loeschen.Click += new EventHandler(cm1_loeschen_Click);
            ToolStripItem erinnerungsetzen = metroContextMenu1.Items.Add("Erinnerung setzen");
            erinnerungsetzen.Click += new EventHandler(cm1_erinnerungsetzen_Click);
            metroContextMenu1.Show(MousePosition);
        }

        private void cm1_deaktivieren_Click(object sender, EventArgs e)
        {
            Database.OpenConnection();
            SQLiteCommand updateEntry =
                new SQLiteCommand(
                    "UPDATE Products SET Status = '1' WHERE ID = @id",
                    Database.Connection);
            updateEntry.Parameters.AddWithValue("@id", Convert.ToInt16(metroGrid1.Rows[metroGrid1.CurrentCell.RowIndex].Cells["DG1_ID"].Value));
            updateEntry.ExecuteNonQuery();
            ProductDatabase.Display(metroComboBox2.SelectedIndex == -1 ? "ALLE" : metroComboBox2.Text);
        }

        private void cm1_aktivieren_Click(object sender, EventArgs e)
        {
            Database.OpenConnection();
            SQLiteCommand updateEntry =
                new SQLiteCommand(
                    "UPDATE Products SET Status = '0' WHERE ID = @id",
                    Database.Connection);
            updateEntry.Parameters.AddWithValue("@id", Convert.ToInt16(metroGrid1.Rows[metroGrid1.CurrentCell.RowIndex].Cells["DG1_ID"].Value));
            updateEntry.ExecuteNonQuery();
            ProductDatabase.Display(metroComboBox2.SelectedIndex == -1 ? "ALLE" : metroComboBox2.Text);
        }

        private void cm1_erinnerungsetzen_Click(object sender, EventArgs e)
        {
            metroTabControl1.SelectedTab = metroTabPage2;
            MyReminder.ProductId = metroGrid1.Rows[metroGrid1.CurrentCell.RowIndex].Cells["DG1_ID"].Value.ToString();
            metroLabel38.Text = metroGrid1.Rows[metroGrid1.CurrentCell.RowIndex].Cells["DG1_StoreName"].Value.ToString();
            metroLabel39.Text = metroGrid1.Rows[metroGrid1.CurrentCell.RowIndex].Cells["DG1_ASIN_ISBN"].Value.ToString();
            metroLabel37.Text = metroGrid1.Rows[metroGrid1.CurrentCell.RowIndex].Cells["DG1_Name"].Value.ToString();
        }

        private void cm1_loeschen_Click(object sender, EventArgs e)
        {
            ProductDatabase.Delete();
            ProductDatabase.Display(metroComboBox2.SelectedIndex == -1 ? "ALLE" : metroComboBox2.Text);
            MyReminder.Display();
        }

        private void metroButton2_Click(object sender, EventArgs e) => ProductDatabase.Delete();

        private void metroComboBox2_SelectedValueChanged(object sender, EventArgs e)
        {
            ProductDatabase.Display(metroComboBox2.SelectedIndex == -1 ? "ALLE" : metroComboBox2.Text);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            ProductDatabase.Display(metroComboBox2.SelectedIndex == -1 ? "ALLE" : metroComboBox2.Text);
        }

        private void metroGrid1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex <= -1 || e.ColumnIndex != metroGrid1.Columns["DG1_URL"].Index) return;
            string url = metroGrid1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            if (!String.IsNullOrWhiteSpace(url))
                Process.Start(url);
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            try
            {
                CsvExport myExport = new CsvExport();
                foreach (DataGridViewRow row in metroGrid1.Rows)
                {
                    myExport.AddRow();
                    myExport["Store"] = row.Cells["DG1_StoreName"].Value;
                    myExport["ASIN / ISBN"] = row.Cells["DG1_ASIN_ISBN"].Value;
                    myExport["Name"] = row.Cells["DG1_Name"].Value;
                    myExport["Preis: Neu"] = row.Cells["DG1_PreisNeu"].Value;
                    myExport["Preis: Wie Neu"] = row.Cells["DG1_PreisWieNeu"].Value;
                    myExport["Preis: Sehr Gut"] = row.Cells["DG1_PreisSehrGut"].Value;
                    myExport["Preis: Gut"] = row.Cells["DG1_PreisGut"].Value;
                    myExport["Preis: Akzeptabel"] = row.Cells["DG1_PreisAkzeptabel"].Value;
                    myExport["URL"] = row.Cells["DG1_URL"].Value;
                    myExport["Letzter Check"] = row.Cells["DG1_LetzterCheck"].Value;
                }
                string filename = "Export_" + DateTime.Now.ToString("dd.MM.yyyy_HH.mm.ss") + "_" + (metroComboBox2.SelectedIndex == -1 ? "ALLE" : metroComboBox2.Text) + ".csv";
                myExport.ExportToFile(Path.Combine(FoldersFilesAndPaths.Export, filename));
                MetroMessageBox.Show(this, "Du findest die Datei: " + filename + Environment.NewLine + "in deinem Export Ordner.", "Excel Export erfolgreich", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
            catch (Exception ex)
            {
                Logger.Write("Escel Export Fehlgeschlagen - Grund: " + ex.Message);
                MetroMessageBox.Show(this, "Bitte versuche es erneut oder Kontaktiere den Entwickler." + Environment.NewLine + Environment.NewLine + @"Fehlercode: " + ex.Message, "Excel Export Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void metroButton13_Click(object sender, EventArgs e) //Checke
        {
            if (Application.OpenForms.OfType<WishlistImporter>().Count() == 1)
                Application.OpenForms.OfType<WishlistImporter>().First().Close();

            WishlistImporter wishlister = new WishlistImporter();
            if (TopMost)
                wishlister.TopMost = true;
            wishlister.Show();
        }

        private void metroButton14_Click(object sender, EventArgs e) //Checke
        {
            if (Application.OpenForms.OfType<WishlistImporter>().Count() == 1)
                Application.OpenForms.OfType<WishlistImporter>().First().Close();

            AmazonSearch amasearch = new AmazonSearch();
            if (TopMost)
                amasearch.TopMost = true;
            amasearch.Show();
        }
        //--Produkt Datenbank ENDE

        //--Crawler START
        private void metroButton4_Click(object sender, EventArgs e)
        {
            StartCrawler();
        }

        private async void StartCrawler()
        {
            metroButton4.Visible = false;
            metroButton3.Visible = true;
            metroComboBox3.Enabled = false;
            CancelCrawler = new CancellationTokenSource();
            await Crawler.Run(CancelCrawler.Token, metroComboBox3.SelectedIndex == -1 ? "ALLE" : metroComboBox3.Text);
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            CancelCrawler.Cancel();
            CancelCrawler = null;

            Crawler.Timer.Tick -= new EventHandler(Crawler.Timer_Tick);
            metroProgressBar1.Value = 0;
            metroLabel29.Text = "00:00";

            metroButton4.Visible = true;
            metroButton3.Visible = false;
            metroComboBox3.Enabled = true;

            webBrowser1.DocumentText = webBrowser2.DocumentText = null;

            metroLabel10.Text = metroLabel11.Text = metroLabel12.Text =
                metroLabel13.Text = metroLabel14.Text = metroLabel15.Text =
                    metroLabel16.Text = metroLabel17.Text = metroLabel18.Text = metroLabel24.Text = metroLabel25.Text =
                        metroLabel26.Text = metroLabel27.Text = metroLabel28.Text = null;

            metroLabel41.Text = null;
        }

        private void metroCheckBox7_CheckedChanged(object sender, EventArgs e)
        {
            var sellerForNew = new List<string>();
            if (!String.IsNullOrWhiteSpace(Settings.Get<string>("SellerForNew")))
                sellerForNew = Settings.Get<string>("SellerForNew").Split(',').Select(s => s.Trim()).ToList();
            if (metroCheckBox7.Checked && !sellerForNew.Contains("Amazon"))
                sellerForNew.Add("Amazon");
            else if (!metroCheckBox7.Checked && sellerForNew.Contains("Amazon"))
                sellerForNew.RemoveAll(item => item != null & item == "Amazon");

            ConfigurationManager.AppSettings["SellerForNew"] = sellerForNew.Any() ? string.Join(",", sellerForNew.ToArray()) : null;
        }

        private void metroCheckBox8_CheckedChanged(object sender, EventArgs e)
        {
            var sellerForNew = new List<string>();
            if (!String.IsNullOrWhiteSpace(Settings.Get<string>("SellerForNew")))
                sellerForNew = Settings.Get<string>("SellerForNew").Split(',').Select(s => s.Trim()).ToList();
            if (metroCheckBox8.Checked)
            {
                if (!sellerForNew.Contains("Drittanbieter"))
                    sellerForNew.Add("Drittanbieter");
                ConfigurationManager.AppSettings["SellerForNew"] = sellerForNew.Any() ? string.Join(",", sellerForNew.ToArray()) : null;
                metroCheckBox9.Enabled = true;
                return;
            }
            if (!metroCheckBox8.Checked)
            {
                if (sellerForNew.Contains("Drittanbieter"))
                    sellerForNew.RemoveAll(item => item != null & item == "Drittanbieter");
                ConfigurationManager.AppSettings["SellerForNew"] = sellerForNew.Any() ? string.Join(",", sellerForNew.ToArray()) : null;
                metroCheckBox9.Enabled = false;
                metroCheckBox9.Checked = false;
            }
        }

        private void metroCheckBox9_CheckedChanged(object sender, EventArgs e)
        {
            var sellerForNew = new List<string>();
            if (!String.IsNullOrWhiteSpace(Settings.Get<string>("SellerForNew")))
                sellerForNew = Settings.Get<string>("SellerForNew").Split(',').Select(s => s.Trim()).ToList();
            if (metroCheckBox9.Checked && !sellerForNew.Contains("Versand per Amazon"))
                sellerForNew.Add("Versand per Amazon");
            else if (!metroCheckBox9.Checked && sellerForNew.Contains("Versand per Amazon"))
                sellerForNew.RemoveAll(item => item != null & item == "Versand per Amazon");

            ConfigurationManager.AppSettings["SellerForNew"] = sellerForNew.Any() ? string.Join(",", sellerForNew.ToArray()) : null;
        }

        private void metroCheckBox12_CheckedChanged(object sender, EventArgs e)
        {
            var sellerForUsed = new List<string>();
            if (!String.IsNullOrWhiteSpace(Settings.Get<string>("SellerForUsed")))
                sellerForUsed = Settings.Get<string>("SellerForUsed").Split(',').Select(s => s.Trim()).ToList();
            if (metroCheckBox12.Checked && !sellerForUsed.Contains("Amazon"))
                sellerForUsed.Add("Amazon");
            else if (!metroCheckBox12.Checked && sellerForUsed.Contains("Amazon"))
                sellerForUsed.RemoveAll(item => item != null & item == "Amazon");

            ConfigurationManager.AppSettings["SellerForUsed"] = sellerForUsed.Any() ? string.Join(",", sellerForUsed.ToArray()) : null;
        }

        private void metroCheckBox11_CheckedChanged(object sender, EventArgs e)
        {
            var sellerForUsed = new List<string>();
            if (!String.IsNullOrWhiteSpace(Settings.Get<string>("SellerForUsed")))
                sellerForUsed = Settings.Get<string>("SellerForUsed").Split(',').Select(s => s.Trim()).ToList();
            if (metroCheckBox11.Checked)
            {
                if (!sellerForUsed.Contains("Drittanbieter"))
                    sellerForUsed.Add("Drittanbieter");
                ConfigurationManager.AppSettings["SellerForUsed"] = sellerForUsed.Any() ? string.Join(",", sellerForUsed.ToArray()) : null;
                metroCheckBox10.Enabled = true;
                return;
            }
            if (!metroCheckBox11.Checked)
            {
                if (sellerForUsed.Contains("Drittanbieter"))
                    sellerForUsed.RemoveAll(item => item != null & item == "Drittanbieter");
                ConfigurationManager.AppSettings["SellerForUsed"] = sellerForUsed.Any() ? string.Join(",", sellerForUsed.ToArray()) : null;
                metroCheckBox10.Enabled = false;
                metroCheckBox10.Checked = false;
            }
        }

        private void metroCheckBox10_CheckedChanged(object sender, EventArgs e)
        {
            var sellerForUsed = new List<string>();
            if (!String.IsNullOrWhiteSpace(Settings.Get<string>("SellerForUsed")))
                sellerForUsed = Settings.Get<string>("SellerForUsed").Split(',').Select(s => s.Trim()).ToList();
            if (metroCheckBox10.Checked && !sellerForUsed.Contains("Versand per Amazon"))
                sellerForUsed.Add("Versand per Amazon");
            else if (!metroCheckBox10.Checked && sellerForUsed.Contains("Versand per Amazon"))
                sellerForUsed.RemoveAll(item => item != null & item == "Versand per Amazon");

            ConfigurationManager.AppSettings["SellerForUsed"] = sellerForUsed.Any() ? string.Join(",", sellerForUsed.ToArray()) : null;
        }

        private void metroToggle2_CheckedChanged(object sender, EventArgs e)
        {
            metroCheckBox7.Enabled = metroCheckBox8.Enabled = metroToggle2.Checked;
            if (!metroToggle2.Checked)
                metroCheckBox9.Enabled = false;
            else if (metroCheckBox8.Checked)
                metroCheckBox9.Enabled = true;
            ConfigurationManager.AppSettings["ScanNew"] = Convert.ToString(metroToggle2.Checked);
        }

        private void metroToggle1_CheckedChanged(object sender, EventArgs e)
        {
            metroCheckBox12.Enabled = metroCheckBox11.Enabled = metroToggle1.Checked;
            if (!metroToggle1.Checked)
                metroCheckBox10.Enabled = false;
            else if (metroCheckBox11.Checked)
                metroCheckBox10.Enabled = true;
            ConfigurationManager.AppSettings["ScanUsed"] = Convert.ToString(metroToggle1.Checked);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Proxies.Proxielist.Clear();
        }

        private void metroToggle5_CheckedChanged(object sender, EventArgs e)
        {
            ConfigurationManager.AppSettings["UseTorProxies"] = Convert.ToString(metroToggle5.Checked);
        }

        private void metroToggle4_CheckedChanged(object sender, EventArgs e)
        {
            ConfigurationManager.AppSettings["ProxyAlwaysActive"] = Convert.ToString(metroToggle4.Checked);
        }
        private void metroComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConfigurationManager.AppSettings["ScanMethod"] = Convert.ToString(metroComboBox1.SelectedIndex);
        }
        //--Crawler ENDE

        //--Einstellungen START
        private void metroCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            ConfigurationManager.AppSettings["StartWithWindows"] = Convert.ToString(metroCheckBox1.Checked);
            OSystem.RegisterInStartup(metroCheckBox1.Checked);
        }
        private void metroCheckBox3_CheckedChanged(object sender, EventArgs e)
        {
            ConfigurationManager.AppSettings["StartMinimized"] = Convert.ToString(metroCheckBox3.Checked);
        }

        private void metroCheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            ConfigurationManager.AppSettings["MinimizeToTray"] = Convert.ToString(metroCheckBox2.Checked);
        }

        private void metroCheckBox13_CheckedChanged(object sender, EventArgs e)
        {
            ConfigurationManager.AppSettings["StartCrawlerAfterStartup"] = Convert.ToString(metroCheckBox13.Checked);
        }

        private void metroCheckBox6_CheckedChanged(object sender, EventArgs e)
        {
            ConfigurationManager.AppSettings["ShowOnlyDealConditions"] = Convert.ToString(metroCheckBox6.Checked);
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            ConfigurationManager.AppSettings["DeleteOldLogsAfterDays"] = $"{(long) numericUpDown4.Value}";
        }
        //--Einstellungen ENDE

        //--Allgemeines START
        private void LoadSettingsToGui()
        {
            //Crawler
            metroToggle2.Checked = metroCheckBox7.Enabled = metroCheckBox8.Enabled = Settings.Get<bool>("ScanNew");
            metroToggle1.Checked = metroCheckBox12.Enabled = metroCheckBox11.Enabled = Settings.Get<bool>("ScanUsed");
            if (!String.IsNullOrWhiteSpace(Settings.Get<string>("SellerForNew")))
            {
                string[] sellerForNew = Settings.Get<string>("SellerForNew").Split(',').Select(s => s.Trim()).ToArray();
                metroCheckBox7.Checked = sellerForNew.Contains("Amazon");
                if (sellerForNew.Contains("Drittanbieter"))
                {
                    metroCheckBox8.Checked = true;
                    metroCheckBox9.Enabled = metroCheckBox8.Enabled;
                }
                metroCheckBox9.Checked = metroCheckBox8.Checked && sellerForNew.Contains("Versand per Amazon");
            }

            if (!String.IsNullOrWhiteSpace(Settings.Get<string>("SellerForUsed")))
            {
                string[] sellerForUsed = Settings.Get<string>("SellerForUsed").Split(',').Select(s => s.Trim()).ToArray();
                metroCheckBox12.Checked = sellerForUsed.Contains("Amazon");
                if (sellerForUsed.Contains("Drittanbieter"))
                {
                    metroCheckBox11.Checked = true;
                    metroCheckBox10.Enabled = metroCheckBox11.Enabled;
                }
                metroCheckBox10.Checked = metroCheckBox11.Checked && sellerForUsed.Contains("Versand per Amazon");
            }
            metroToggle5.Checked = Settings.Get<bool>("UseTorProxies");
            metroToggle4.Checked = Settings.Get<bool>("ProxyAlwaysActive");
            metroComboBox1.SelectedIndex = Settings.Get<int>("Scanmethod");

            //Reminder
            numericUpDown1.Value = Settings.Get<int>("RemindResendAfterMinutes");
            metroTextBox8.Text = Settings.Get<string>("ReminderEmail");
            metroTextBox9.Text = Settings.Get<string>("ReminderTelegram");

            //Settings
            metroCheckBox6.Checked = Settings.Get<bool>("ShowOnlyDealConditions");
            metroCheckBox1.Checked = Settings.Get<bool>("StartWithWindows");
            metroCheckBox2.Checked = Settings.Get<bool>("MinimizeToTray");
            metroCheckBox13.Checked = Settings.Get<bool>("StartCrawlerAfterStartup");
            metroCheckBox3.Checked = Settings.Get<bool>("StartMinimized");
            metroToggle3.Checked = Settings.Get<bool>("Debug");
            numericUpDown4.Value = Settings.Get<int>("DeleteOldLogsAfterDays");

            //Premium
            metroTextBox5.Text = Settings.Get<string>("PremiumEmail");
            metroTextBox6.Text = Settings.Get<string>("PremiumKey");
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized || !metroCheckBox2.Checked) return;
            Hide();
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(1000);
        }
        //--Allgemeines ENDE

        //--Feedback START
        private async void metroTile2_Click(object sender, EventArgs e)
        {
            if (metroComboBox5.SelectedIndex == -1)
            {
                MetroMessageBox.Show(this,
                    "Feedback konnte nicht gesendet werden.\nAuswahl Fehlt", "Feedback Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (String.IsNullOrWhiteSpace(metroTextBox3.Text))
            {
                MetroMessageBox.Show(this,
                    "Feedback konnte nicht gesendet werden.\nBetreff Leer", "Feedback Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (String.IsNullOrWhiteSpace(metroTextBox2.Text))
            {
                MetroMessageBox.Show(this,
                    "Feedback konnte nicht gesendet werden.\nNachricht Leer", "Feedback Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string subject = null;
            switch (metroComboBox5.SelectedIndex)
            {
                case 0:
                    subject = "[PROBLEM] " + metroTextBox3.Text;
                    break;
                case 1:
                    subject = "[VORSCHLAG] " + metroTextBox3.Text;
                    break;
                case 2:
                    subject = "[SONSTIGES] " + metroTextBox3.Text;
                    break;
            }
            string body = metroTextBox2.Text + "\n";
            if (!String.IsNullOrWhiteSpace(metroTextBox11.Text))
                body += "\nEmail: " + metroTextBox11.Text;
            if (!String.IsNullOrWhiteSpace(metroTextBox12.Text))
                body += "\nTelegram: " + metroTextBox12.Text;

            metroComboBox5.Enabled = metroTextBox3.Enabled =
                metroTextBox2.Enabled = metroTextBox4.Enabled = metroButton6.Enabled = metroTextBox11.Enabled = metroTextBox12.Enabled = metroTile2.Enabled = false;
            metroProgressSpinner1.Visible = true;
            await TelegramApi.SendFeedbackMessage(subject, body, metroTextBox4.Text);
            metroComboBox5.Enabled = metroTextBox3.Enabled =
                metroTextBox2.Enabled = metroTextBox4.Enabled = metroButton6.Enabled = metroTextBox11.Enabled = metroTextBox12.Enabled = metroTile2.Enabled = true;
            metroProgressSpinner1.Visible = false;
        }

        private void metroButton6_Click(object sender, EventArgs e)
        {
            var FD = new OpenFileDialog();
            switch (FD.ShowDialog())
            {
                case DialogResult.OK:
                    if (new FileInfo(FD.FileName).Length > 47185920)
                        MetroMessageBox.Show(this,
                            "Die Ausgewählte Datei ist größer als 45MB", "Anhang Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    else
                    {
                        metroTextBox4.Text = FD.FileName;
                    }
                    break;
            }
        }
        //--Feedback ENDE

        //--Meine Erinnerungen START
        private void metroTextBox7_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
                && !char.IsDigit(e.KeyChar)
                && e.KeyChar != ',')
            {
                e.Handled = true;
            }

            if (e.KeyChar == (Int32) Keys.Enter)
                metroTextBox7_Leave(null, null);

            //check if ',' pressed
            char sepratorChar = 's';
            if (e.KeyChar == ',')
            {
                // check if it's in the beginning of text not accept
                if (metroTextBox7.Text.Length == 0) e.Handled = true;
                // check if it's in the beginning of text not accept
                if (metroTextBox7.SelectionStart == 0) e.Handled = true;
                // check if there is already exist a '.' , ','
                if (alreadyExist(metroTextBox7.Text, ref sepratorChar)) e.Handled = true;
                //check if '.' or ',' is in middle of a number and after it is not a number greater than 99
                if (metroTextBox7.SelectionStart != metroTextBox7.Text.Length && e.Handled == false)
                {
                    // '.' or ',' is in the middle
                    string AfterDotString = metroTextBox7.Text.Substring(metroTextBox7.SelectionStart);

                    if (AfterDotString.Length > 2)
                    {
                        e.Handled = true;
                    }
                }
            }
            //check if a number pressed
            if (Char.IsDigit(e.KeyChar))
            {
                //check if a coma or dot exist
                if (alreadyExist(metroTextBox7.Text, ref sepratorChar))
                {
                    int sepratorPosition = metroTextBox7.Text.IndexOf(sepratorChar);
                    string afterSepratorString = metroTextBox7.Text.Substring(sepratorPosition + 1);
                    if (metroTextBox7.SelectionStart > sepratorPosition && afterSepratorString.Length > 1)
                    {
                        e.Handled = true;
                    }

                }
            }
        }

        private bool alreadyExist(string _text, ref char KeyChar)
        {
            if (_text.IndexOf(',') > -1)
            {
                KeyChar = ',';
                return true;
            }
            return false;
        }

        private void metroTextBox7_Leave(object sender, EventArgs e)
        {
            metroTextBox7.Text = String.IsNullOrWhiteSpace(metroTextBox7.Text) ? @"0,00" : Convert.ToDecimal(metroTextBox7.Text).ToString("#,0.00").Replace(".", string.Empty);
        }

        private void metroTextBox8_TextChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(metroTextBox8.Text))
            {
                if (!new RegexUtilities().IsValidEmail(metroTextBox8.Text))
                {
                    metroLabel32.Text = @"Email Format is not valid ;-(";
                    metroLabel32.ForeColor = Color.Red;
                }
                else
                {
                    metroLabel32.Text = @"Email Format is valid ;-)";
                    metroLabel32.ForeColor = Color.Green;
                }
            }
            else
            {
                metroLabel32.Text = null;
            }
            ConfigurationManager.AppSettings["ReminderEmail"] = !String.IsNullOrWhiteSpace(metroTextBox8.Text) && new RegexUtilities().IsValidEmail(metroTextBox8.Text) ? metroTextBox8.Text : null;
        }

        private async void metroButton10_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(metroTextBox9.Text)) return;
            metroButton10.Enabled = false;
            await TelegramApi.SendMessage(metroTextBox9.Text,
                "Wenn du diese Nachricht erhälst hast du alles richtig Konfiguriert ;-)");
            metroButton10.Enabled = true;
        }

        private void metroButton9_Click(object sender, EventArgs e)
        {
            string zustand = string.Join(",", checkedListBox1.CheckedItems.Cast<string>().ToArray());
            string preis = metroTextBox7.Text;

            string email = metroCheckBox4.Checked && new RegexUtilities().IsValidEmail(Settings.Get<string>("ReminderEmail")) ? Settings.Get<string>("ReminderEmail") : null;
            string telegram = metroCheckBox5.Checked && !String.IsNullOrWhiteSpace(Settings.Get<string>("ReminderTelegram")) ? Settings.Get<string>("ReminderTelegram") : null;
            
            metroProgressSpinner3.Visible = true;
            metroButton9.Enabled = false;
            MyReminder.Add(MyReminder.ProductId, zustand, preis, email, telegram);
            metroProgressSpinner3.Visible = false;
            metroButton9.Enabled = true;
        }

        private void metroButton11_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                checkedListBox1.SetItemChecked(i, true);
        }

        private void metroButton12_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                checkedListBox1.SetItemChecked(i, false);
        }

        private void metroGrid2_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || e.RowIndex <= -1) return;
            if (metroGrid2.CurrentCell.RowIndex != e.RowIndex)
                metroGrid2.ClearSelection();

            metroGrid2.CurrentCell = metroGrid2.Rows[e.RowIndex].Cells["DG2_StoreImage"];
            metroContextMenu2.Show(MousePosition);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e) => MyReminder.Delete();

        private void metroButton16_Click(object sender, EventArgs e) => MyReminder.Delete();

        private void toolStripMenuItem4_Click(object sender, EventArgs e) => MyReminder.Reset();

        private void metroButton17_Click(object sender, EventArgs e) => MyReminder.Reset();

        private void metroComboBox7_SelectedIndexChanged(object sender, EventArgs e) => MyReminder.Display(metroComboBox7.SelectedIndex == -1 ? "ALLE" : metroComboBox7.Text);

        private void metroButton18_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<TelegramSetupWizard>().Count() == 1)
                Application.OpenForms.OfType<TelegramSetupWizard>().First().Close();

            TelegramSetupWizard telegramSetup = new TelegramSetupWizard();
            if (TopMost)
                telegramSetup.TopMost = true;
            telegramSetup.Show();
        }

        private void metroTextBox9_TextChanged(object sender, EventArgs e)
        {
            ConfigurationManager.AppSettings["ReminderTelegram"] = !String.IsNullOrWhiteSpace(metroTextBox9.Text) ? metroTextBox9.Text : null;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            ConfigurationManager.AppSettings["RemindResendAfterMinutes"] = $"{(long)numericUpDown1.Value}";
        }
        //--Meine Erinnerungen ENDE

        //--Premium START
        private async void metroButton15_Click(object sender, EventArgs e)
        {
            metroButton15.Enabled = false;
            await Premium.CheckManuell();
            if (Settings.IsPremium)
                EnablePremium();
            else
                DisablePremium();
            metroButton15.Enabled = true;
        }

        public void EnablePremium()
        {
            metroCheckBox13.Enabled = true;
            metroToggle4.Enabled = true;
        }

        public void DisablePremium()
        {
            metroCheckBox13.Enabled = false;
            metroToggle4.Enabled = false;
        }

        private void metroLabel60_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {

        }
        //--Premium ENDE
    }
}

using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using DealReminder_Linux.Configs;
using DealReminder_Linux.Logging;
using DealReminder_Linux.Utils;
using DealReminder_Windows.Utils;

namespace DealReminder_Linux.GUI
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = Settings.Get<bool>("SaveLoginCredits");
            if (!String.IsNullOrWhiteSpace(Settings.Get<string>("LoginCredits")))
                textBox1.Text = Settings.Get<string>("LoginCredits");
        }

        private void button1_Click(object sender, EventArgs e) => Application.Exit();

        private void button2_Click(object sender, EventArgs e)
        {
            if (!LoginSuccess()) return;
            Logger.Write("Loading Main GUI...");
            this.Hide();
            Main main = new Main();
            main.Closed += (s, args) => this.Close();
            main.Show();
        }

        private void Login_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData != Keys.Enter) return;
            button2.Focus();
            LoginSuccess();
        }

        public bool LoginSuccess()
        {
            Logger.Write("Starte Login...");
            if (String.IsNullOrWhiteSpace(textBox1.Text) || !new RegexUtilities().IsValidEmail(textBox1.Text))
            {
                if (String.IsNullOrWhiteSpace(textBox1.Text))
                    Logger.Write("Login Fehlgeschlagen - Grund: Die eingegebenen Login Daten sind NullOrWhitespace...");
                else if (!new RegexUtilities().IsValidEmail(textBox1.Text))
                    Logger.Write("Login Fehlgeschlagen - Grund: Die eingegebenen Login Daten entsprechen nicht dem Email Format...");
                //--SETTINGS
                Settings.Config.AppSettings.Settings["LoginCredits"].Value = null;
                Settings.Config.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection("appSettings");
                //--SETTINGS
                MessageBox.Show(this,
                    "Die eingabe entspricht nicht dem valid email format!" + Environment.NewLine +
                    "Bitte gebe deine registrierte E-Mail-Adresse ein.", "Login Fehlgeschlagen",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            try
            {
                NameValueCollection parameters = new NameValueCollection
                {
                    {"filter", "DealReminder"},
                    {"hash", Crypto.HashMD5(Crypto.HashSHA512(textBox1.Text.ToLower()))}
                };
                // Ignore Certificate validation failures (aka untrusted certificate + certificate chains)
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                byte[] bArr = new WebClient { Proxy = { Credentials = CredentialCache.DefaultCredentials } }.UploadValues("https://auth.speg-dev.de/DoLoginCheck.php", "POST", parameters);
                var xmlDoc = Tools.GetXmlDocFromBytes(bArr);
                var status = xmlDoc.GetElementsByTagName("Status")[0].InnerText;
                if (status.Contains("200"))
                {
                    if (xmlDoc.GetElementsByTagName("IsTrial")[0].InnerText != "0" && Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1)
                    {
                        Logger.Write("Login Erfolgreich, mit einem Trial Account ist aber eine mehrfach Instanz nicht erlaubt...");
                        MessageBox.Show(this,
                            "DealReminder läuft bereits!" + Environment.NewLine +
                            "Mit einem Trial Account ist keine mehrfach Instanz von DealReminder erlaubt!",
                            "Login Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    Settings.MaxActiveProducts = Convert.ToInt16(xmlDoc.GetElementsByTagName("AllowedActiveProducts")[0].InnerText);
                    Logger.Write("Login erfolgreich...");
                    //--SETTINGS
                    Settings.Config.AppSettings.Settings["SaveLoginCredits"].Value = Convert.ToString(checkBox1.Checked);
                    Settings.Config.AppSettings.Settings["LoginCredits"].Value = checkBox1.Checked ? textBox1.Text : null;
                    Settings.Config.Save(ConfigurationSaveMode.Full);
                    ConfigurationManager.RefreshSection("appSettings");
                    //--SETTINGS
                    MessageBox.Show(this, "Viel Spaß mit DealReminder ;-)", "Login Erfolgreich", MessageBoxButtons.OK, MessageBoxIcon.Question);

                    return true;
                }
                if (status.Contains("401"))
                {
                    Logger.Write("Login Fehlgeschlagen - Grund: Die eingegebenen Login Daten wurden nicht in der Server Datenbank gefunden...");
                    //--SETTINGS
                    Settings.Config.AppSettings.Settings["LoginCredits"].Value = null;
                    Settings.Config.Save(ConfigurationSaveMode.Full);
                    ConfigurationManager.RefreshSection("appSettings");
                    //--SETTINGS
                    MessageBox.Show(this, "Die eingegebenen Login Daten wurden nicht in der Server Datenbank gefunden!", "Login Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Login Fehlgeschlagen - Grund: " + ex.Message);
                if (!Tools.IsAvailableNetworkActive())
                    MessageBox.Show(this, "Besteht eine Internetverbindung?" + Environment.NewLine + "Bitte versuche es erneut oder Kontaktiere den Entwickler." + Environment.NewLine + Environment.NewLine + @"Fehlercode: " + ex.Message, "Login Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(this, "Bitte versuche es erneut oder Kontaktiere den Entwickler." + Environment.NewLine + Environment.NewLine + @"Fehlercode: " + ex.Message, "Login Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return false;
        }

        public static bool AutoLoginSuccess()
        {
            Logger.Write("Starte Auto-Login...");
            if (String.IsNullOrWhiteSpace(Settings.Get<string>("LoginCredits")) || !new RegexUtilities().IsValidEmail(Settings.Get<string>("LoginCredits")))
            {
                if (String.IsNullOrWhiteSpace(Settings.Get<string>("LoginCredits")))
                    Logger.Write("Login Fehlgeschlagen - Grund: Login Daten in der (Settings\\Settings.config) nicht vorhanden...");
                else if (!new RegexUtilities().IsValidEmail(Settings.Get<string>("LoginCredits")))
                    Logger.Write("Login Fehlgeschlagen - Grund: Login Daten in der (Settings\\Settings.config) entsprechen nicht dem Email Format...");
                //--SETTINGS
                Settings.Config.AppSettings.Settings["LoginCredits"].Value = null;
                Settings.Config.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection("appSettings");
                //--SETTINGS
                return false;
            }
            try
            {
                NameValueCollection parameters = new NameValueCollection
                {
                    {"filter", "DealReminder"},
                    {"hash", Crypto.HashMD5(Crypto.HashSHA512(Settings.Get<string>("LoginCredits").ToLower()))}
                };
                // Ignore Certificate validation failures (aka untrusted certificate + certificate chains)
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                byte[] bArr = new WebClient { Proxy = { Credentials = CredentialCache.DefaultCredentials } }.UploadValues("https://auth.speg-dev.de/DoLoginCheck.php", "POST", parameters);
                var xmlDoc = Tools.GetXmlDocFromBytes(bArr);
                var status = xmlDoc.GetElementsByTagName("Status")[0].InnerText;
                if (status.Contains("200"))
                {
                    if (xmlDoc.GetElementsByTagName("IsTrial")[0].InnerText != "0" && Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1)
                    {
                        Logger.Write("Login Erfolgreich, mit einem Trial Account ist aber eine mehrfach Instanz nicht erlaubt...");
                        return false;
                    }
                    Settings.MaxActiveProducts = Convert.ToInt16(xmlDoc.GetElementsByTagName("AllowedActiveProducts")[0].InnerText);
                    Logger.Write("Login erfolgreich...");
                    return true;
                }
                if (status.Contains("401") || status.Contains("Unerlaubte Zeichen enthalten!"))
                {
                    if (status.Contains("401"))
                        Logger.Write("Login Fehlgeschlagen - Grund: Die gespeicherten Login Daten wurden nicht in der Server Datenbank gefunden...");
                    else if (status.Contains("Unerlaubte Zeichen enthalten!"))
                        Logger.Write("Login Fehlgeschlagen - Grund: Die gespeicherten Login Daten erhalten unerlaubte Zeichen...");
                    //--SETTINGS
                    Settings.Config.AppSettings.Settings["LoginCredits"].Value = null;
                    Settings.Config.Save(ConfigurationSaveMode.Full);
                    ConfigurationManager.RefreshSection("appSettings");
                    //--SETTINGS
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Login Fehlgeschlagen - Grund: " + ex.Message);
                return false;
            }
            return false;
        }
    }
}

using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using DealReminder_Windows.Configs;
using DealReminder_Windows.GUI;
using DealReminder_Windows.Logging;
using DealReminder_Windows.Utils;
using MetroFramework;

namespace DealReminder_Windows.Tasks
{
    internal class Premium
    {
        public static async Task CheckManuell()
        {
            Main mf = Application.OpenForms["Main"] as Main;

            Logger.Write("Checke Premium...");
            if (String.IsNullOrWhiteSpace(mf.metroTextBox5.Text) || !new RegexUtilities().IsValidEmail(mf.metroTextBox5.Text))
            {
                if (String.IsNullOrWhiteSpace(mf.metroTextBox5.Text))
                    Logger.Write("Premium Check Fehlgeschlagen - Grund: Die eingegebenen Premium Email ist NullOrWhitespace...");
                else if (!new RegexUtilities().IsValidEmail(mf.metroTextBox5.Text))
                    Logger.Write("Premium Check Fehlgeschlagen - Grund: Die eingegebenen Premium Email entspricht nicht dem Email Format...");
                //--SETTINGS
                ConfigurationManager.AppSettings["PremiumEmail"] = null;
                //--SETTINGS
                MetroMessageBox.Show(mf,
                    "Die eingegebene Premium Email entspricht nicht dem validen email format!" + Environment.NewLine +
                    "Bitte gebe deine Premium E-Mail-Adresse ein.", "Premium Check Fehlgeschlagen",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (String.IsNullOrWhiteSpace(mf.metroTextBox6.Text))
            {
                if (String.IsNullOrWhiteSpace(mf.metroTextBox6.Text))
                    Logger.Write("Premium Check Fehlgeschlagen - Grund: Der eingegebenen Premium Key ist NullOrWhitespace...");
                //--SETTINGS
                ConfigurationManager.AppSettings["PremiumKey"] = null;
                //--SETTINGS
                MetroMessageBox.Show(mf,
                    "Du hast wohl vergessen deinen Premium Key einzugeben!" + Environment.NewLine +
                    "Bitte gebe deinen Premium Key ein.", "Premium Check Fehlgeschlagen",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                NameValueCollection parameters = new NameValueCollection
                {
                    {"filter", "DealReminder"},
                    {"email", Crypto.HashMD5(Crypto.HashSHA512(mf.metroTextBox5.Text.ToLower()))},
                    {"key", Crypto.HashMD5(Crypto.HashSHA512(mf.metroTextBox6.Text))}
                };
                // Ignore Certificate validation failures (aka untrusted certificate + certificate chains)
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                byte[] bArr = await new BetterWebClient { Timeout = 10000 }.UploadValuesTaskAsync(new Uri("https://auth.speg-dev.de/DoPremiumCheck.php"), "POST", parameters);
                var xmlDoc = Tools.GetXmlDocFromBytes(bArr);
                var status = xmlDoc.GetElementsByTagName("Status")[0].InnerText;
                if (status.Contains("200"))
                {
                    DateTime myDate = DateTime.ParseExact(xmlDoc.GetElementsByTagName("ExpiryDate")[0].InnerText, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    mf.metroLabel49.Text = Convert.ToString(myDate, CultureInfo.CurrentCulture);
                    //Change to Expiry Date

                    if (DateTime.Now > myDate)
                    {
                        MessageBox.Show("Premium abgelaufen am: " + Convert.ToString(myDate, CultureInfo.CurrentCulture));
                        Logger.Write("Premium abgelaufen am: " + Convert.ToString(myDate, CultureInfo.CurrentCulture));
                        return;
                    }

                    Logger.Write("Premium Check erfolgreich...");
                    Logger.Write("Premium gültig bis: " + Convert.ToString(myDate, CultureInfo.CurrentCulture));
                    //--SETTINGS
                    ConfigurationManager.AppSettings["PremiumEmail"] = mf.metroTextBox5.Text;
                    ConfigurationManager.AppSettings["PremiumKey"] = mf.metroTextBox6.Text;
                    //--SETTINGS
                    Settings.IsPremium = true;
                    Settings.PremiumExpiryDate = myDate;
                    MetroMessageBox.Show(mf, "Viel Spaß mit DealReminder Premium ;-)", "Premium Check Erfolgreich", MessageBoxButtons.OK, MessageBoxIcon.Question);
                    return;
                }
                if (status.Contains("400"))
                {
                    Logger.Write("Premium Check Fehlgeschlagen - Grund: Application not found...");
                    Settings.IsPremium = false;
                    Settings.PremiumExpiryDate = null;
                    MetroMessageBox.Show(mf, "Es gab einen Fehler beim Abfrage des Premium Status." + Environment.NewLine + "Bitte Kontaktiere den Entwickler." + Environment.NewLine + Environment.NewLine + @"Fehlercode: 400", "Premium Check Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (status.Contains("401"))
                {
                    Logger.Write("Premium Check Fehlgeschlagen - Grund: User not found on Server Database...");
                    //--SETTINGS
                    ConfigurationManager.AppSettings["PremiumEmail"] = null;
                    //--SETTINGS
                    Settings.IsPremium = false;
                    Settings.PremiumExpiryDate = null;
                    MetroMessageBox.Show(mf, "Die eingegebenen Premium Email wurden nicht in der Server Datenbank gefunden!", "Premium Check Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (status.Contains("402"))
                {
                    Logger.Write("Premium Check Fehlgeschlagen - Grund: Premium Key not found on Server Database...");
                    //--SETTINGS
                    ConfigurationManager.AppSettings["PremiumKey"] = null;
                    //--SETTINGS
                    Settings.IsPremium = false;
                    Settings.PremiumExpiryDate = null;
                    MetroMessageBox.Show(mf, "Der eingegebenen Premium Key wurden nicht in der Server Datenbank gefunden!", "Premium Check Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Premium Check Fehlgeschlagen - Grund: " + ex.Message);
                if (!OSystem.IsAvailableNetworkActive())
                    MetroMessageBox.Show(mf, "Besteht eine Internetverbindung?" + Environment.NewLine + "Bitte versuche es erneut oder Kontaktiere den Entwickler." + Environment.NewLine + Environment.NewLine + @"Fehlercode: " + ex.Message, "Premium Check Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MetroMessageBox.Show(mf, "Bitte versuche es erneut oder Kontaktiere den Entwickler." + Environment.NewLine + Environment.NewLine + @"Fehlercode: " + ex.Message, "Premium Check Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void CheckAutomatic()
        {
            Logger.Write("Premium Auto-Check...");
            if (String.IsNullOrWhiteSpace(Settings.Get<string>("PremiumEmail")) || !new RegexUtilities().IsValidEmail(Settings.Get<string>("PremiumEmail")))
            {
                if (String.IsNullOrWhiteSpace(Settings.Get<string>("PremiumEmail")))
                    Logger.Write("Premium Check Fehlgeschlagen - Grund: Premium Email in der (Settings\\Settings.config) nicht vorhanden...");
                else if (!new RegexUtilities().IsValidEmail(Settings.Get<string>("PremiumEmail")))
                    Logger.Write("Premium Check Fehlgeschlagen - Grund: Premium Email in der (Settings\\Settings.config) entspricht nicht dem Email Format...");
                //--SETTINGS
                ConfigurationManager.AppSettings["PremiumEmail"] = null;
                //--SETTINGS
                return;
            }
            if (String.IsNullOrWhiteSpace(Settings.Get<string>("PremiumKey")))
            {
                if (String.IsNullOrWhiteSpace(Settings.Get<string>("PremiumKey")))
                    Logger.Write("Premium Check Fehlgeschlagen - Grund: Premium Key in der (Settings\\Settings.config) nicht vorhanden...");
                //--SETTINGS
                ConfigurationManager.AppSettings["PremiumKey"] = null;
                //--SETTINGS
                return;
            }
            try
            {
                NameValueCollection parameters = new NameValueCollection
                {
                    {"filter", "DealReminder"},
                    {"email", Crypto.HashMD5(Crypto.HashSHA512(Settings.Get<string>("PremiumEmail").ToLower()))},
                    {"key", Crypto.HashMD5(Crypto.HashSHA512(Settings.Get<string>("PremiumKey")))}
                };
                // Ignore Certificate validation failures (aka untrusted certificate + certificate chains)
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                byte[] bArr = new BetterWebClient { Timeout = 10000 }.UploadValues("https://auth.speg-dev.de/DoPremiumCheck.php", "POST", parameters);
                var xmlDoc = Tools.GetXmlDocFromBytes(bArr);
                var status = xmlDoc.GetElementsByTagName("Status")[0].InnerText;
                if (status.Contains("200"))
                {
                    DateTime myDate = DateTime.ParseExact(xmlDoc.GetElementsByTagName("ExpiryDate")[0].InnerText, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                    if (DateTime.Now > myDate)
                    {
                        MessageBox.Show("Premium abgelaufen am: " + Convert.ToString(myDate, CultureInfo.CurrentCulture));
                        Logger.Write("Premium abgelaufen am: " + Convert.ToString(myDate, CultureInfo.CurrentCulture));
                        return;
                    }

                    Logger.Write("Premium Check erfolgreich...");
                    Logger.Write("Premium gültig bis: " + Convert.ToString(myDate, CultureInfo.CurrentCulture));
                    Settings.IsPremium = true;
                    Settings.PremiumExpiryDate = myDate;
                    return;
                }
                if (status.Contains("400"))
                {
                    Logger.Write("Premium Check Fehlgeschlagen - Grund: Application not found...");
                    MessageBox.Show("Es gab einen Fehler beim Abfrage des Premium Status." + Environment.NewLine + "Bitte Kontaktiere den Entwickler." + Environment.NewLine + Environment.NewLine + @"Fehlercode: 400", "Premium Check Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (status.Contains("401"))
                {
                    Logger.Write("Premium Check Fehlgeschlagen - Grund: User not found on Server Database...");
                    //--SETTINGS
                    ConfigurationManager.AppSettings["PremiumEmail"] = null;
                    //--SETTINGS
                    MessageBox.Show("Die eingegebenen Premium Email wurden nicht in der Server Datenbank gefunden!", "Premium Check Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (status.Contains("402"))
                {
                    Logger.Write("Premium Check Fehlgeschlagen - Grund: Premium Key not found on Server Database...");
                    //--SETTINGS
                    ConfigurationManager.AppSettings["PremiumKey"] = null;
                    //--SETTINGS
                    MessageBox.Show("Der eingegebenen Premium Key wurden nicht in der Server Datenbank gefunden!", "Premium Check Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Premium Check Fehlgeschlagen - Grund: " + ex.Message);
                if (!OSystem.IsAvailableNetworkActive())
                    MessageBox.Show("Besteht eine Internetverbindung?" + Environment.NewLine + "Bitte versuche es erneut oder Kontaktiere den Entwickler." + Environment.NewLine + Environment.NewLine + @"Fehlercode: " + ex.Message, "Premium Check Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show("Bitte versuche es erneut oder Kontaktiere den Entwickler." + Environment.NewLine + Environment.NewLine + @"Fehlercode: " + ex.Message, "Premium Check Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

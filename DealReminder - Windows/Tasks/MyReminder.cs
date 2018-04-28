using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using DealReminder_Windows.Configs;
using DealReminder_Windows.GUI;
using DealReminder_Windows.Logging;
using DealReminder_Windows.Properties;
using DealReminder_Windows.Utils;
using MetroFramework;
using Settings = DealReminder_Windows.Configs.Settings;

namespace DealReminder_Windows.Tasks
{
    internal class MyReminder
    {
        public static Main mf = Application.OpenForms["Main"] as Main;
        public static string ProductId = null;

        public static void Display(string store = "ALLE")
        {
            Database.OpenConnection();
            SQLiteCommand getRemindEntrys;
            switch (store)
            {
                case "ALLE":
                    getRemindEntrys = new SQLiteCommand("SELECT * FROM Reminders ORDER BY [Erinnerung Gesendet] ASC",
                        Database.Connection);
                    break;
                default:
                    getRemindEntrys =
                        new SQLiteCommand(
                            "SELECT * FROM Reminders INNER JOIN Products ON Products.Store = @store AND Products.ID = Reminders.ProductID ORDER BY [Erinnerung Gesendet] ASC",
                            Database.Connection);
                    getRemindEntrys.Parameters.AddWithValue("@store", store);
                    break;
            }
            using (SQLiteDataReader remind = getRemindEntrys.ExecuteReader())
            {
                mf.metroGrid2.Rows.Clear();
                while (remind.Read())
                {
                    Database.OpenConnection();
                    var getProductEntry =
                        new SQLiteCommand("SELECT * FROM Products WHERE ID = @id",
                            Database.Connection);
                    getProductEntry.Parameters.AddWithValue("@id", remind["ProductID"]);
                    using (SQLiteDataReader product = getProductEntry.ExecuteReader())
                    {
                        if (!product.Read()) continue;

                        ResourceManager rm = Resources.ResourceManager;                        
                        mf.metroGrid2.Rows.Add(remind["ID"],
                            remind["ProductID"],
                            product["Store"],
                            (Image)rm.GetObject("Flagge_" + Convert.ToString(product["Store"])),
                            product["ASIN / ISBN"],
                            Convert.ToString(product["Name"]).Trim(),
                            remind["Zustand"],
                            remind["Preis"],
                            remind["Email"],
                            remind["Telegram"],
                            remind["Erinnerung Gesendet"]
                            );          
                    }
                }
            }
        }

        public static void Add(string productid, string zustand, string preis, string email, string telegram)
        {
            if (String.IsNullOrWhiteSpace(productid))
            {
                MetroMessageBox.Show(mf,
                    "Eintrag konnte nicht hinzugefügt werden!" + Environment.NewLine +
                    "Keine Produkt Informationen vorhanden!", "Eintrag Hinzufügen Fehlgeschlagen",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (String.IsNullOrWhiteSpace(zustand) || String.IsNullOrWhiteSpace(preis))
            {
                MetroMessageBox.Show(mf,
                    "Eintrag konnte nicht hinzugefügt werden!" + Environment.NewLine +
                    "Bitte geben einen Zustand und Preis an!", "Eintrag Hinzufügen Fehlgeschlagen",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Match isDecimal = Regex.Match(preis, "(^[0-9]+(,[0-9]{2,2})$)");
            if (!isDecimal.Success)
            {
                MetroMessageBox.Show(mf,
                    "Eintrag konnte nicht hinzugefügt werden!" + Environment.NewLine +
                    "Bitte Überprüfe das Format des Preis an (Format mit Komma: 0,00)!", "Eintrag Hinzufügen Fehlgeschlagen",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (String.IsNullOrWhiteSpace(email) && String.IsNullOrWhiteSpace(telegram))
            {
                MetroMessageBox.Show(mf,
                    "Eintrag konnte nicht hinzugefügt werden!" + Environment.NewLine +
                    "Bitte wähle mind. 1 Benachrichtungs Element aus!", "Eintrag Hinzufügen Fehlgeschlagen",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Database.OpenConnection();
            SQLiteCommand insertEntry =
                new SQLiteCommand(
                    "INSERT INTO Reminders (ProductID, Zustand, Preis, Email, Telegram) Values (@productid, @zustand, @preis, @email, @telegram)",
                    Database.Connection);
            insertEntry.Parameters.AddWithValue("@productid", productid);
            insertEntry.Parameters.AddWithValue("@zustand", zustand);
            insertEntry.Parameters.AddWithValue("@preis", preis);
            insertEntry.Parameters.AddWithValue("@email", email);
            insertEntry.Parameters.AddWithValue("@telegram", telegram);
            insertEntry.ExecuteNonQuery();

            ProductId = null;
            mf.metroLabel37.Text = null;
            mf.metroLabel38.Text = null;
            mf.metroLabel39.Text = null;

            mf.metroTextBox7.Clear();
            for (int i = 0; i < mf.checkedListBox1.Items.Count; i++)
            {
                var i1 = i;
                mf.checkedListBox1.SetItemChecked(i1, false);
            }
            mf.metroCheckBox4.Checked = false;
            mf.metroCheckBox5.Checked = false;

            Display(mf.metroComboBox7.SelectedIndex == -1 ? "ALLE" : mf.metroComboBox7.Text);
        }

        public static void Delete()
        {
            Int32 selectedRowCount = mf.metroGrid2.Rows.GetRowCount(DataGridViewElementStates.Selected);
            if (selectedRowCount <= 0) return;
            for (int i = 0; i < selectedRowCount; i++)
            {
                int selectedIndex = mf.metroGrid2.SelectedRows[i].Index;
                string id = Convert.ToString(mf.metroGrid2["DG2_ID", selectedIndex].Value);

                Database.OpenConnection();
                SQLiteCommand deleteEntry = new SQLiteCommand(
                    "DELETE FROM Reminders WHERE ID = @id", Database.Connection);
                deleteEntry.Parameters.AddWithValue("@id", id);
                deleteEntry.ExecuteNonQuery();
            }
            Display(mf.metroComboBox7.SelectedIndex == -1 ? "ALLE" : mf.metroComboBox7.Text);
        }

        public static void Reset()
        {
            Int32 selectedRowCount = mf.metroGrid2.Rows.GetRowCount(DataGridViewElementStates.Selected);
            if (selectedRowCount <= 0) return;
            for (int i = 0; i < selectedRowCount; i++)
            {
                int selectedIndex = mf.metroGrid2.SelectedRows[i].Index;
                string id = Convert.ToString(mf.metroGrid2["DG2_ID", selectedIndex].Value);

                Database.OpenConnection();
                SQLiteCommand resetRemind =
                    new SQLiteCommand(
                        "UPDATE Reminders SET [Erinnerung Gesendet] = @lastsend WHERE ID = @id",
                        Database.Connection);
                resetRemind.Parameters.AddWithValue("@id", id);
                resetRemind.Parameters.AddWithValue("@lastsend", DBNull.Value);
                resetRemind.ExecuteNonQuery();
            }
            Display(mf.metroComboBox7.SelectedIndex == -1 ? "ALLE" : mf.metroComboBox7.Text);
        }

        public static async Task DoRemindWhenPossible(string productid)
        {
            Database.OpenConnection();
            var getReminderEntrys =
                new SQLiteCommand("SELECT * FROM Reminders INNER JOIN Products ON Products.ID = Reminders.ProductID WHERE Reminders.ProductID = @productid AND (Reminders.[Erinnerung Gesendet] IS NULL OR Reminders.[Erinnerung Gesendet] < @datetime)",
                Database.Connection);
            getReminderEntrys.Parameters.AddWithValue("@productid", productid);
            getReminderEntrys.Parameters.AddWithValue("@datetime", DateTime.Now.AddMinutes(-Settings.Get<int>("RemindResendAfterMinutes")).ToString("yyyy-MM-dd HH:mm:ss"));
            using (SQLiteDataReader remind = getReminderEntrys.ExecuteReader())
            {
                List<string> remindsToSend = new List<string>();
                while (remind.Read())
                {
                    Logger.Write("Erinnerungen gefunden für ProductID: " + productid + " mit dem Erinnerungspreis: " + remind["Preis"], LogLevel.Debug);

                    List<string> conditions = new List<string>();
                    Convert.ToString(remind["Zustand"]).TrimEnd().Split(',')
                        .ToList()
                        .ForEach(item =>
                        {
                            if (String.IsNullOrWhiteSpace(Convert.ToString(remind["Preis: " + item])) || Convert.ToDecimal(remind["Preis: " + item]) > Convert.ToDecimal(remind["Preis"])) return;
                            remindsToSend.Add(item + ": " + Convert.ToString(remind["Preis: " + item]));
                            conditions.Add(item);
                            Logger.Write("Treffer zur Erinnerung hinzugefügt: " + item + ": " + Convert.ToString(remind["Preis: " + item]), LogLevel.Debug);
                        });

                    if (remindsToSend.Any())
                    {
                        /* Erstellt einen neuen Warenkorb
                        var catInfo = AmazonApi.CreateCart("DE", "B00YUIM2J0");
                        if (catInfo?.Cart.PurchaseURL != null)
                        {
                            Debug.WriteLine(catInfo?.Cart.PurchaseURL);
                        }
                        */

                        string shortUrl = Settings.Get<bool>("ShowOnlyDealConditions")
                            ? await URLShortener.Generate(Amazon.MakeReferralLink(Convert.ToString(remind["Store"]),
                                Convert.ToString(remind["ASIN / ISBN"]), conditions))
                            : null;
                        shortUrl = shortUrl ?? Convert.ToString(remind["URL"]);

                        string notificationText = remind["Name"] + "\n" +
                                                  "Store: " + remind["Store"] + " - ASIN/ISBN: " +
                                                  remind["ASIN / ISBN"] + "\n\n" +
                                                  "Erinnerungs Preis: " + remind["Preis"] + "\n\n" +
                                                  "Aktuelle Amazon Preise:\n";
                        remindsToSend.ForEach(i => notificationText += i + "\n");
                        notificationText += "\nLink: " + shortUrl;

                        if (!String.IsNullOrWhiteSpace(Convert.ToString(remind["Telegram"])))
                            await TelegramApi.SendMessage(Convert.ToString(remind["Telegram"]), notificationText);

                        if (!String.IsNullOrWhiteSpace(Convert.ToString(remind["Email"])))
                            await Mail.NotificationSend(Convert.ToString(remind["Email"]), notificationText,
                                Convert.ToString(remind["Name"]));

                        Database.OpenConnection();
                        SQLiteCommand updateRemind =
                            new SQLiteCommand(
                                "UPDATE Reminders SET [Erinnerung Gesendet] = @lastsend WHERE ID = @id",
                                Database.Connection);
                        updateRemind.Parameters.AddWithValue("@id", remind["ID"]);
                        updateRemind.Parameters.AddWithValue("@lastsend", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        updateRemind.ExecuteNonQuery();

                        Display();
                    }
                }
            }
        }
    }
}

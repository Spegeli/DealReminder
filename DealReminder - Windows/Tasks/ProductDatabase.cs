using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Forms;
using DealReminder_Windows.Configs;
using DealReminder_Windows.GUI;
using DealReminder_Windows.Logging;
using DealReminder_Windows.Properties;
using DealReminder_Windows.Utils;
using MetroFramework;
using Nager.AmazonProductAdvertising.Model;
using Image = System.Drawing.Image;

namespace DealReminder_Windows.Tasks
{
    internal class ProductDatabase
    {
        public static Main mf = Application.OpenForms["Main"] as Main;

        public static async Task Add(string[] stores, string asin_isbn)
        {
            if (Tools.ArrayIsNullOrEmpty(stores))
            {
                MetroMessageBox.Show(mf,
                    "Eintrag konnte nicht hinzugefügt werden!" + Environment.NewLine +
                    "Bitte wähle mind. 1 Store für dieses Produkt aus!", "Eintrag Hinzufügen Fehlgeschlagen",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (String.IsNullOrWhiteSpace(asin_isbn))
            {
                MetroMessageBox.Show(mf,
                    "Eintrag konnte nicht hinzugefügt werden!" + Environment.NewLine +
                    "Bitte gebe die Produkt ASIN / ISBN ein.!", "Eintrag Hinzufügen Fehlgeschlagen",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (string store in stores)
            {
                Database.OpenConnection();
                SQLiteCommand checkEntry = new SQLiteCommand(
                    "SELECT COUNT(*) FROM Products WHERE Store = @store AND [ASIN / ISBN] = @asin_isbn", Database.Connection);
                checkEntry.Parameters.AddWithValue("@store", store);
                checkEntry.Parameters.AddWithValue("@asin_isbn", asin_isbn);
                int entryExist = Convert.ToInt32(checkEntry.ExecuteScalar());
                if (entryExist > 0) continue;
                AmazonItemResponse itemInfo;
                try
                {
                    itemInfo = await Task.Run(() => AmazonApi.ItemLookup(store, asin_isbn));
                    if (itemInfo.Items == null)
                        continue;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Abfrage Fehler: " + ex.Message, LogLevel.Debug);
                    continue;
                }
                if (itemInfo.Items.Request.Errors != null && itemInfo.Items.Request.Errors.Any())
                {
                    foreach (var error in itemInfo.Items.Request.Errors)
                    {
                        Logger.Write("AmazonAPI Abfrage Fehlgeschlagen - Grund: " + error.Message, LogLevel.Debug);
                    }
                    continue;
                }
                string name = itemInfo.Items.Item[0].ItemAttributes.Title;
                var shortUrl = await URLShortener.Generate(Amazon.MakeReferralLink(store, asin_isbn), name, store);
                if (shortUrl == null) continue;
                Database.OpenConnection();
                SQLiteCommand insertEntry =
                    new SQLiteCommand(
                        "INSERT INTO Products (Store, [ASIN / ISBN], Name, URL) Values (@store, @asin_isbn, @name, @shorturl)",
                        Database.Connection);
                insertEntry.Parameters.AddWithValue("@store", store);
                insertEntry.Parameters.AddWithValue("@asin_isbn", asin_isbn);
                insertEntry.Parameters.AddWithValue("@name", name);
                insertEntry.Parameters.AddWithValue("@shorturl", shortUrl);
                insertEntry.ExecuteNonQuery();
            }
            mf.metroTextBox1.Clear();
            Display(mf.metroComboBox2.SelectedIndex == -1 ? "ALLE" : mf.metroComboBox2.Text);
        }

        public static void Display(string store = "ALLE")
        {
            Database.OpenConnection();
            SQLiteCommand getEntrys;
            switch (store)
            {
                case "ALLE":
                    getEntrys = new SQLiteCommand("SELECT * FROM Products ORDER BY [Letzter Check] ASC",
                        Database.Connection);
                    break;
                default:
                    getEntrys =
                        new SQLiteCommand("SELECT * FROM Products WHERE Store = @store ORDER BY [Letzter Check] ASC",
                            Database.Connection);
                    getEntrys.Parameters.AddWithValue("@store", store);
                    break;
            }
            using (SQLiteDataReader remind = getEntrys.ExecuteReader())
            {
                mf.metroGrid1.Rows.Clear();
                while (remind.Read())
                {
                    ResourceManager rm = Resources.ResourceManager;
                    mf.metroGrid1.Rows.Add(remind["ID"],
                        remind["Status"],
                        (Image)rm.GetObject("Icon_Status_" + Convert.ToInt16(remind["Status"])),
                        remind["Store"],
                        (Image)rm.GetObject("Flagge_" + Convert.ToString(remind["Store"])),
                        remind["ASIN / ISBN"],
                        Convert.ToString(remind["Name"]).Trim(),
                        remind["Preis: Neu"],
                        remind["Preis: Wie Neu"],
                        remind["Preis: Sehr Gut"],
                        remind["Preis: Gut"],
                        remind["Preis: Akzeptabel"],
                        remind["URL"],
                        remind["Letzter Check"]);
                }
            }
        }

        public static void Delete()
        {
            Int32 selectedRowCount = mf.metroGrid1.Rows.GetRowCount(DataGridViewElementStates.Selected);
            if (selectedRowCount <= 0) return;
            for (int i = 0; i < selectedRowCount; i++)
            {
                int selectedIndex = mf.metroGrid1.SelectedRows[i].Index;
                string id = Convert.ToString(mf.metroGrid1["DG1_ID", selectedIndex].Value);

                Database.OpenConnection();
                SQLiteCommand deleteEntry = new SQLiteCommand(
                    "DELETE FROM Products WHERE ID = @id", Database.Connection);
                deleteEntry.Parameters.AddWithValue("@id", id);
                deleteEntry.ExecuteNonQuery();

                Database.OpenConnection();
                SQLiteCommand deleteReminderEntrys = new SQLiteCommand(
                    "DELETE FROM Reminders WHERE ProductID = @id", Database.Connection);
                deleteReminderEntrys.Parameters.AddWithValue("@id", id);
                deleteReminderEntrys.ExecuteNonQuery();

                if (MyReminder.ProductId != id) continue;
                MyReminder.ProductId = null;
                mf.metroLabel37.Text = null;
                mf.metroLabel38.Text = null;
                mf.metroLabel39.Text = null;
            }
            Display(mf.metroComboBox2.SelectedIndex == -1 ? "ALLE" : mf.metroComboBox2.Text);
            MyReminder.Display(mf.metroComboBox7.SelectedIndex == -1 ? "ALLE" : mf.metroComboBox7.Text);
        }
    }
}

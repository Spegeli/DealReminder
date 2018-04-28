using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using DealReminder_Windows.Configs;
using DealReminder_Windows.Tasks;
using DealReminder_Windows.Utils;
using MetroFramework.Forms;
using Newtonsoft.Json;

namespace DealReminder_Windows.GUI
{
    public partial class WishlistImporter : MetroForm
    {
        public WishlistImporter()
        {
            InitializeComponent();
        }

        private void WishlistImporter_Load(object sender, EventArgs e)
        {
            metroComboBox1.SelectedIndex = 2;
        }

        private async void metroButton1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(metroTextBox1.Text)) return;
            Match wishlistUrl = Regex.Match(metroTextBox1.Text, "(.*)amazon.(.*)/gp/registry/wishlist/([a-zA-Z0-9_]*)");
            if (!wishlistUrl.Success)
            {
                metroLabel1.Text = @"Store oder Wunschliste in URL nicht erkannt.";
                return;
            }
            var store = wishlistUrl.Groups[2].Value == "co.uk" ? "UK" : wishlistUrl.Groups[2].Value.ToUpper();
            var wishlistid = wishlistUrl.Groups[3].Value;
            var reveal = metroComboBox1.SelectedIndex;
            metroComboBox1.Enabled = metroTextBox1.Enabled = metroButton1.Enabled = false;
            metroLabel1.Text = $@"Importiere von Store: {store} und Wunschliste: {wishlistid}";
            await Import(store, wishlistid, reveal);
            metroComboBox1.Enabled = metroTextBox1.Enabled = metroButton1.Enabled = true;
        }

        private async Task Import(string store, string wishlist, int status = 2)
        {
            Main mf = Application.OpenForms["Main"] as Main;

            string reveal = null;
            switch (status)
            {
                case 0:
                    reveal = "all";
                    break;
                case 1:
                    reveal = "purchased";
                    break;
                case 2:
                    reveal = "unpurchased";
                    break;
            }
            string result = await new BetterWebClient {Timeout = 15000}.DownloadStringTaskAsync(new Uri(
                "https://tools.dealreminder.de/wish-lister/wishlist.php?tld=" +
                Amazon.GetTld(store) + "&id=" + wishlist + "&reveal=" + reveal +
                "&format=json"));
            dynamic jsonObj = JsonConvert.DeserializeObject(result);
            if (jsonObj == null)
            {
                metroLabel1.Text = @"Fehler beim Abrufen der Wunschliste. Falsche ID? Nicht Öffentlicht?";
                return;
            }
            Dictionary<string, string> resultList = new Dictionary<string, string>();
            foreach (var obj in jsonObj)
            {
                resultList.Add(Convert.ToString(obj.ASIN), Convert.ToString(obj.name));
            }
            foreach (var item in resultList.ToList())
            {
                Database.OpenConnection();
                SQLiteCommand checkEntry = new SQLiteCommand(
                    "SELECT COUNT(*) FROM Products WHERE Store = @store AND [ASIN / ISBN] = @asin_isbn",
                    Database.Connection);
                checkEntry.Parameters.AddWithValue("@store", store);
                checkEntry.Parameters.AddWithValue("@asin_isbn", item.Key);
                int entryExist = Convert.ToInt32(checkEntry.ExecuteScalar());
                if (entryExist > 0)
                    resultList.Remove(item.Key);
            }
            if (!resultList.Any())
            {
                metroLabel1.Text = @"Alle Produkte dieser Wunschliste bereits in der Datenbank vorhanden.";
                return;
            }
            foreach (var item in resultList)
            {
                string asin_isbn = item.Key;
                string name = item.Value;
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
            ProductDatabase.Display(mf.metroComboBox2.SelectedIndex == -1 ? "ALLE" : mf.metroComboBox2.Text);
            this.Close();
        }
    }
}

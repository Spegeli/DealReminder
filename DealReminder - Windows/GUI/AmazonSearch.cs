using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Forms;
using DealReminder_Windows.Configs;
using DealReminder_Windows.Logging;
using DealReminder_Windows.Properties;
using DealReminder_Windows.Tasks;
using DealReminder_Windows.Utils;
using MetroFramework.Forms;
using Nager.AmazonProductAdvertising.Model;
using Image = System.Drawing.Image;

namespace DealReminder_Windows.GUI
{
    public partial class AmazonSearch : MetroForm
    {
        private string _currentsearchstore = null;

        public AmazonSearch()
        {
            InitializeComponent();

            new ToolTip().SetToolTip(pictureBox1, "The desired tool-tip text.");
        }

        private async void metroButton5_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(metroTextBox1.Text)) return;
            metroGrid1.Rows.Clear();
            string store = metroComboBox2.SelectedIndex == -1 ? "DE" : metroComboBox2.Text;
            string categorie = metroComboBox1.SelectedIndex == -1 ? "All" : metroComboBox1.Text;
            int site = metroComboBox3.SelectedIndex == -1 ? 1 : Convert.ToInt16(metroComboBox3.Text);
            string searchwords = metroTextBox1.Text;
            metroComboBox2.Enabled = metroComboBox1.Enabled =
                metroTextBox1.Enabled = metroComboBox3.Enabled = metroButton5.Enabled = metroButton1.Enabled = false;
            var searchInfo = await Task.Run(() => AmazonApi.CustomItemSearch(store, categorie, searchwords, site));
            if (searchInfo.Items.Request.Errors != null && searchInfo.Items.Request.Errors.Any())
            {
                foreach (var error in searchInfo.Items.Request.Errors)
                {
                    Logger.Write("AmazonAPI Abfrage Fehlgeschlagen - Grund: " + error.Message, LogLevel.Debug);
                    metroLabel1.Text = error.Message;
                }
            }
            else
            {
                foreach (var item in searchInfo.Items.Item)
                {
                    ResourceManager rm = Resources.ResourceManager;
                    string asin_isbn = item.ASIN;
                    var previewimage = item.SmallImage?.URL != null ? await Task.Run(() => WebUtils.GetImageFromUrl(item.SmallImage.URL)) : (Image)rm.GetObject("No_Image");
                    string name = item.ItemAttributes.Title;
                    Price pricenew = item.OfferSummary.LowestNewPrice;
                    Price priceused = item.OfferSummary.LowestUsedPrice;
                    metroGrid1.Rows.Add(false,
                        previewimage,
                        asin_isbn,
                        name,
                        pricenew != null ? pricenew.FormattedPrice : string.Empty,
                        priceused != null ? priceused.FormattedPrice : string.Empty);
                }
                _currentsearchstore = store;
            }
            metroComboBox2.Enabled = metroComboBox1.Enabled =
                metroTextBox1.Enabled = metroComboBox3.Enabled = metroButton5.Enabled = metroButton1.Enabled = true;
        }

        private void metroGrid1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex <= -1 || e.ColumnIndex == metroGrid1.Columns["DG3_CheckBox"].Index) return;
            string store = _currentsearchstore;
            string asin_isbn = Convert.ToString(metroGrid1.Rows[e.RowIndex].Cells["DG3_ASIN_ISBN"].Value);
            if (String.IsNullOrEmpty(asin_isbn)) return;
            Process.Start($"https://www.amazon.{Amazon.GetTld(store)}/dp/{asin_isbn}/&tag=" + AmazonApi.AssociateTag(store));
        }

        private void metroGrid1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex <= -1 || e.ColumnIndex != metroGrid1.Columns["DG3_CheckBox"].Index) return;
            metroGrid1.Rows[e.RowIndex].Cells["DG3_CheckBox"].Value = !Convert.ToBoolean(metroGrid1.Rows[e.RowIndex].Cells["DG3_CheckBox"].Value);
        }

        private async void metroButton1_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> entryList = metroGrid1.Rows.Cast<DataGridViewRow>().Where(row => (bool) row.Cells["DG3_CheckBox"].Value).ToDictionary(row => Convert.ToString(row.Cells["DG3_ASIN_ISBN"].Value), row => Convert.ToString(row.Cells["DG3_Name"].Value));
            if (!entryList.Any()) return;
            metroButton5.Enabled = false;
            metroButton1.Enabled = false;
            var entryNumber = entryList.GetType().GetGenericArguments().Length;
            metroLabel1.Text = $@"Füge {entryNumber} Einträge aus der Suche zur Datenbank hinzu.";
            await Add(_currentsearchstore, entryList);
            metroButton1.Enabled = true;
            metroButton5.Enabled = true;
        }

        public async Task Add(string store, Dictionary<string, string> entryList)
        {
            Main mf = Application.OpenForms["Main"] as Main;

            foreach (var item in entryList.ToList())
            {
                Database.OpenConnection();
                SQLiteCommand checkEntry = new SQLiteCommand(
                    "SELECT COUNT(*) FROM Products WHERE Store = @store AND [ASIN / ISBN] = @asin_isbn",
                    Database.Connection);
                checkEntry.Parameters.AddWithValue("@store", store);
                checkEntry.Parameters.AddWithValue("@asin_isbn", item.Key);
                int entryExist = Convert.ToInt32(checkEntry.ExecuteScalar());
                if (entryExist > 0)
                    entryList.Remove(item.Key);
            }
            if (!entryList.Any())
            {
                metroLabel1.Text = @"Alle ausgewählten Produkte bereits in der Datenbank vorhanden.";
                return;
            }
            foreach (var item in entryList)
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
            metroLabel1.Text = @"Alle ausgewählten Produkte Erfolgreich Hinzugefügt.";
        }
    }
}

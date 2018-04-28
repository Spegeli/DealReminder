using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nager.AmazonProductAdvertising;
using Nager.AmazonProductAdvertising.Model;

namespace DealReminder_Windows.Utils
{
    internal class AmazonApi
    {
        //https://github.com/tinohager/Nager.AmazonProductAdvertising
        public static AmazonItemResponse ItemLookup(string store, string asin_isbn)
        {
            var authentication = GetAuth();
            AmazonEndpoint endpoint = GetEndpoint(store);
            var wrapper = new AmazonWrapper(authentication, endpoint, AssociateTag(store));
            wrapper.XmlReceived += (xml) => { System.Diagnostics.Debug.WriteLine(xml); };
            wrapper.ErrorReceived += (errorResonse) => { System.Diagnostics.Debug.WriteLine(errorResonse.Error.Message); };
            return wrapper.Lookup(asin_isbn);
        }

        public static AmazonItemResponse MultiItemLookup(string store, string[] asin_isbn)
        {
            var authentication = GetAuth();
            AmazonEndpoint endpoint = GetEndpoint(store);
            var wrapper = new AmazonWrapper(authentication, endpoint, AssociateTag(store));
            wrapper.XmlReceived += (xml) => { System.Diagnostics.Debug.WriteLine(xml); };
            wrapper.ErrorReceived += (errorResonse) => { System.Diagnostics.Debug.WriteLine(errorResonse.Error.Message); };
            return wrapper.Lookup(asin_isbn);
        }

        public static CartCreateResponse CreateCart(string store, string asin_isbn)
        {
            var authentication = GetAuth();
            AmazonEndpoint endpoint = GetEndpoint(store);
            var items = new List<AmazonCartItem> {new AmazonCartItem(asin_isbn)};
            var wrapper = new AmazonWrapper(authentication, endpoint, AssociateTag(store));
            wrapper.XmlReceived += (xml) => { System.Diagnostics.Debug.WriteLine(xml); };
            wrapper.ErrorReceived += (errorResonse) => { System.Diagnostics.Debug.WriteLine(errorResonse.Error.Message); };
            return wrapper.CartCreate(items);
        }

        public static ItemSearchResponse CustomItemSearch(string store, string categorie, string searchword, int site = 1)
        {
            var authentication = GetAuth();
            AmazonEndpoint endpoint = GetEndpoint(store);
            var wrapper = new AmazonWrapper(authentication, endpoint, AssociateTag(store));
            wrapper.XmlReceived += (xml) => { System.Diagnostics.Debug.WriteLine(xml); };
            wrapper.ErrorReceived += (errorResonse) => { System.Diagnostics.Debug.WriteLine(errorResonse.Error.Message); };
            var searchOperation = wrapper.ItemSearchOperation(searchword, GetSearchIndex(categorie));
            if (categorie != "All")
                searchOperation.Sort(AmazonSearchSort.Price, AmazonSearchSortOrder.Descending);
            searchOperation.Skip(site);
            var xmlResponse = wrapper.Request(searchOperation);
            return XmlHelper.ParseXml<ItemSearchResponse>(xmlResponse.Content);
        }

        public static bool ProductHasISBN(AmazonItemResponse itemInfo)
        {
            return itemInfo.Items.Item[0].ItemAttributes.ISBN != null;
        }

        public static string AssociateTag(string store)
        {
            switch (store.ToUpper())
            {
                case "DE":
                    return ""; //Hier den Amazon TAG für das jeweilige Land Eintragen
                case "IT":
                    return ""; //Hier den Amazon TAG für das jeweilige Land Eintragen
                case "FR":
                    return ""; //Hier den Amazon TAG für das jeweilige Land Eintragen
                case "ES":
                    return ""; //Hier den Amazon TAG für das jeweilige Land Eintragen
                case "UK":
                    return ""; //Hier den Amazon TAG für das jeweilige Land Eintragen
            }
            return String.Empty;
        }

        public static AmazonEndpoint GetEndpoint(string store)
        {
            switch (store.ToUpper())
            {
                case "DE":
                    return AmazonEndpoint.DE;
                case "IT":
                    return AmazonEndpoint.IT;
                case "FR":
                    return AmazonEndpoint.FR;
                case "ES":
                    return AmazonEndpoint.ES;
                case "UK":
                    return AmazonEndpoint.UK;
            }
            return AmazonEndpoint.US;
        }

        //API von @Mattes0303
        public static AmazonAuthentication GetAuth()
        {
            return new AmazonAuthentication
            {
                AccessKey = "", //Hier den Amazon API AccessKey Eintragen
                SecretKey = "" //Hier den Amazon API SecretKey Eintragen
            };
        }

        public static AmazonSearchIndex GetSearchIndex(string categorie)
        {
            switch (categorie)
            {
                case "All":
                    return AmazonSearchIndex.All;
                case "Apparel":
                    return AmazonSearchIndex.Apparel;
                case "Automotive":
                    return AmazonSearchIndex.Automotive;
                case "Baby":
                    return AmazonSearchIndex.Baby;
                case "Beauty":
                    return AmazonSearchIndex.Beauty;
                case "Blended":
                    return AmazonSearchIndex.Blended;
                case "Books":
                    return AmazonSearchIndex.Books;
                case "Classical":
                    return AmazonSearchIndex.Classical;
                case "DigitalMusic":
                    return AmazonSearchIndex.DigitalMusic;
                case "DVD":
                    return AmazonSearchIndex.DVD;
                case "Electronics":
                    return AmazonSearchIndex.Electronics;
                case "ForeignBooks":
                    return AmazonSearchIndex.ForeignBooks;
                case "GourmetFood":
                    return AmazonSearchIndex.GourmetFood;
                case "Grocery":
                    return AmazonSearchIndex.Grocery;
                case "HealthPersonalCare":
                    return AmazonSearchIndex.HealthPersonalCare;
                case "Hobbies":
                    return AmazonSearchIndex.Hobbies;
                case "HomeGarden":
                    return AmazonSearchIndex.HomeGarden;
                case "Industrial":
                    return AmazonSearchIndex.Industrial;
                case "Jewelry":
                    return AmazonSearchIndex.Jewelry;
                case "KindleStore":
                    return AmazonSearchIndex.KindleStore;
                case "Kitchen":
                    return AmazonSearchIndex.Kitchen;
                case "Magazines":
                    return AmazonSearchIndex.Magazines;
                case "Merchants":
                    return AmazonSearchIndex.Merchants;
                case "Miscellaneous":
                    return AmazonSearchIndex.Miscellaneous;
                case "MP3Downloads":
                    return AmazonSearchIndex.MP3Downloads;
                case "Music":
                    return AmazonSearchIndex.Music;
                case "MusicalInstruments":
                    return AmazonSearchIndex.MusicalInstruments;
                case "MusicTracks":
                    return AmazonSearchIndex.MusicTracks;
                case "OfficeProducts":
                    return AmazonSearchIndex.OfficeProducts;
                case "OutdoorLiving":
                    return AmazonSearchIndex.OutdoorLiving;
                case "PCHardware":
                    return AmazonSearchIndex.PCHardware;
                case "PetSupplies":
                    return AmazonSearchIndex.PetSupplies;
                case "Photo":
                    return AmazonSearchIndex.Photo;
                case "Software":
                    return AmazonSearchIndex.Software;
                case "SoftwareVideoGames":
                    return AmazonSearchIndex.SoftwareVideoGames;
                case "SportingGoods":
                    return AmazonSearchIndex.SportingGoods;
                case "Tools":
                    return AmazonSearchIndex.Tools;
                case "Toys":
                    return AmazonSearchIndex.Toys;
                case "VHS":
                    return AmazonSearchIndex.VHS;
                case "Video":
                    return AmazonSearchIndex.Video;
                case "VideoGames":
                    return AmazonSearchIndex.VideoGames;
                case "Watches":
                    return AmazonSearchIndex.Watches;
                case "Wireless":
                    return AmazonSearchIndex.Wireless;
                case "WirelessAccessories":
                    return AmazonSearchIndex.WirelessAccessories;
            }
            return AmazonSearchIndex.All;
        }
    }

    internal class Amazon
    {
        public static string GetTld(string store)
        {
            switch (store.ToUpper())
            {
                case "DE":
                    return "de";
                case "IT":
                    return "it";
                case "FR":
                    return "fr";
                case "ES":
                    return "es";
                case "UK":
                    return "co.uk";
            }
            return String.Empty;
        }

        public static string MakeReferralLink(string store, string asin_isbn, List<string> conditions = null)
        {
            if (conditions != null && conditions.Any())
            {
                string conditionResult = String.Empty;
                if (conditions.Contains("Neu"))
                    conditionResult += "&f_new=true";
                if (conditions.Contains("Wie Neu") || conditions.Contains("Sehr Gut") ||
                    conditions.Contains("Gut") || conditions.Contains("Akzeptabel"))
                {
                    conditionResult += "&f_used=true";
                    if (conditions.Contains("Wie Neu"))
                        conditionResult += "&f_usedLikeNew=true";
                    if (conditions.Contains("Sehr Gut"))
                        conditionResult += "&f_usedVeryGood=true";
                    if (conditions.Contains("Gut"))
                        conditionResult += "&f_usedGood=true";
                    if (conditions.Contains("Akzeptabel"))
                        conditionResult += "&f_usedAcceptable=true";
                }
                return
                    $"http://www.amazon.{GetTld(store)}/gp/offer-listing/{asin_isbn}/ref=as_li_ss_tl?ie=UTF8{conditionResult}&tag={AmazonApi.AssociateTag(store)}";
            }
            return $"http://www.amazon.{GetTld(store)}/gp/offer-listing/{asin_isbn}/ref=nosim?tag={AmazonApi.AssociateTag(store)}";
        }
    }
}

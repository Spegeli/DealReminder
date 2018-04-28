using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DealReminder_Windows.Configs;
using DealReminder_Windows.GUI;
using DealReminder_Windows.Logging;
using DealReminder_Windows.Utils;
using Knapcode.TorSharp;
using MetroFramework.Controls;

namespace DealReminder_Windows.Tasks
{
    internal class Crawler
    {
        public static Main mf = Application.OpenForms["Main"] as Main;

        public static System.Windows.Forms.Timer Timer = new System.Windows.Forms.Timer();
        private static DateTime _dt = new DateTime();
        private static int _randomWait = 30;

        public static async Task Run(CancellationToken cancelToken, string store)
        {
            Timer.Tick += new EventHandler(Timer_Tick);
            Timer.Interval = 1000;

            if (!Settings.Get<bool>("ScanNew") && !Settings.Get<bool>("ScanUsed"))
            {
                mf.textBox4.AppendText("Crawler wird gestoppt - Reason: Neu & Gebraucht sind Deaktiviert" + Environment.NewLine);
                mf.metroButton3.PerformClick();
                return;
            }

            mf.textBox4.AppendText("Übernehme Neu & Gebraucht Einstellungen" + Environment.NewLine);
            var sellerForNew = new List<string>();
            if (!String.IsNullOrWhiteSpace(Settings.Get<string>("SellerForNew")))
                sellerForNew = Settings.Get<string>("SellerForNew").Split(',').Select(s => s.Trim()).ToList();
            else
                sellerForNew.Add("Amazon");
            var sellerForUsed = new List<string>();
            if (!String.IsNullOrWhiteSpace(Settings.Get<string>("SellerForUsed")))
                sellerForUsed = Settings.Get<string>("SellerForUsed").Split(',').Select(s => s.Trim()).ToList();
            else
                sellerForUsed.Add("Amazon");


            TorSharpSettings torSettings = null;
            TorSharpProxy torProxy = null;
            if (Settings.Get<bool>("UseTorProxies"))
            {
                mf.textBox4.AppendText("Initialisiere TOR Proxy Funktion" + Environment.NewLine);
                try
                {
                    torSettings = new TorSharpSettings
                    {
                        ReloadTools = true,
                        ZippedToolsDirectory = Path.Combine(FoldersFilesAndPaths.Temp, "TorZipped"),
                        ExtractedToolsDirectory = Path.Combine(FoldersFilesAndPaths.Temp, "TorExtracted"),
                        PrivoxySettings =
                        {
                            Port = 1337,
                        },
                        TorSettings =
                        {
                            SocksPort = 1338,
                            ControlPort = 1339,
                            ControlPassword = "foobar",
                        },
                    };
                    await new TorSharpToolFetcher(torSettings, new HttpClient()).FetchAsync();
                    torProxy = new TorSharpProxy(torSettings);
                    await torProxy.ConfigureAndStartAsync(); 
                }
                catch (Exception ex)
                {
                    mf.textBox4.AppendText("Fehler beim Initialisiere der TOR Proxy Funktion ;-(" + Environment.NewLine + "TOR Proxy Funktion wird deaktiviert!" + Environment.NewLine);
                    mf.textBox4.AppendText(ex.Message);
                    mf.metroToggle5.Checked = false;
                }

                if (Settings.IsPremium && Settings.Get<bool>("UseTorProxies") && Settings.Get<bool>("ProxyAlwaysActive"))
                {
                    mf.textBox4.AppendText("Set TOR Proxy - Reason: P. always Active" + Environment.NewLine);
                    mf.metroLabel45.Text = Convert.ToString(await WebUtils.GetCurrentIpAddressAsync(torSettings));
                    Proxies.TorProxyActive = true;
                }
            }

            string sourcecode;
            int sites, loops;

            while (true)
            {
                Database.OpenConnection();
                SQLiteCommand getEntry;
                switch (store)
                {
                    case "ALLE":
                        getEntry = new SQLiteCommand("SELECT * FROM Products WHERE Status = '0' ORDER BY [Letzter Check] ASC LIMIT 0,1",
                            Database.Connection);
                        break;
                    default:
                        getEntry =
                            new SQLiteCommand("SELECT * FROM Products WHERE Status = '0' AND Store = @store ORDER BY [Letzter Check] ASC LIMIT 0,1",
                                Database.Connection);
                        getEntry.Parameters.AddWithValue("@store", store);
                        break;
                }
                SQLiteDataReader entry = getEntry.ExecuteReader();
                if (!entry.HasRows)
                {
                    mf.metroButton3.PerformClick();
                    Timer.Stop();
                    torProxy.Stop();
                    return;
                }
                while (entry.Read())
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        Timer.Stop();
                        torProxy.Stop();
                        return;
                    }
                    mf.metroLabel10.Text = Convert.ToString(entry["Store"]);
                    mf.metroLabel11.Text = Convert.ToString(entry["ASIN / ISBN"]);
                    mf.metroLabel12.Text = Convert.ToString(entry["Name"]);
                    mf.metroLabel13.Text = Convert.ToString(entry["Preis: Neu"]);
                    mf.metroLabel14.Text = Convert.ToString(entry["Preis: Wie Neu"]);
                    mf.metroLabel15.Text = Convert.ToString(entry["Preis: Sehr Gut"]);
                    mf.metroLabel16.Text = Convert.ToString(entry["Preis: Gut"]);
                    mf.metroLabel17.Text = Convert.ToString(entry["Preis: Akzeptabel"]);
                    mf.metroLabel18.Text = Convert.ToString(entry["Letzter Check"]);
                    mf.metroLabel24.Text = mf.metroLabel25.Text = mf.metroLabel26.Text = mf.metroLabel27.Text = mf.metroLabel28.Text = null;
                    mf.metroLabel41.Text = null;
                    mf.webBrowser1.DocumentText = mf.webBrowser2.DocumentText = null;
                    await Task.Delay(Tools.RandomNumber(250, 500), cancelToken).ContinueWith(tsk => { });

                    string priceNew = null;
                    if (Settings.Get<bool>("ScanNew"))
                    {
                        List<string> priceNewList = new List<string>();
                        mf.tabControl2.SelectedTab = mf.TabPage4;
                        sites = 1;
                        loops = 0;
                        do
                        {
                            if (cancelToken.IsCancellationRequested)
                            {
                                Timer.Stop();
                                torProxy.Stop();
                                return;
                            }
                            try
                            {
                                string url = "https://www.amazon." + Amazon.GetTld(Convert.ToString(entry["Store"])) +
                                             "/gp/aw/ol/" + entry["ASIN / ISBN"] +
                                             "/ref=mw_dp_olp?ca=" + entry["ASIN / ISBN"] + "&o=New&op=" + (loops + 1) +
                                             "&vs=1";

                                var handler = new HttpClientHandler();
                                if (Settings.Get<bool>("UseTorProxies"))
                                {
                                    if (Settings.IsPremium && Settings.Get<bool>("ProxyAlwaysActive") ||
                                        Proxies.TorProxyActive && Proxies.RealIPnexttime > DateTime.Now)
                                    {
                                        Debug.WriteLine("Add TOR Proxy to HttpClientHandler...");
                                        handler.Proxy =
                                            new WebProxy(new Uri("http://localhost:" + torSettings.PrivoxySettings.Port));
                                    }
                                    else if (Proxies.TorProxyActive)
                                    {
                                        Proxies.TorProxyActive = false;
                                        Proxies.RealIPnexttime = DateTime.Now;
                                    }
                                }
                                handler.CookieContainer = new CookieContainer();
                                handler.AutomaticDecompression =
                                    DecompressionMethods.GZip | DecompressionMethods.Deflate;
                                var httpClient = new HttpClient(handler) {Timeout = TimeSpan.FromSeconds(10)};
                                var useragent = await UserAgents.GetRandom();
                                httpClient.DefaultRequestHeaders.Add("User-Agent",
                                    !String.IsNullOrEmpty(useragent)
                                        ? useragent
                                        : "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
                                httpClient.DefaultRequestHeaders.Add("Referer", WebUtils.RandomReferal());

                                HttpResponseMessage response =
                                    await httpClient.GetAsync(new Uri(url, UriKind.Absolute), cancelToken);
                                sourcecode = await response.Content.ReadAsStringAsync();
                            }
                            catch (HttpRequestException ex)
                            {
                                Logger.Write("HttpRequestException: " + ex.Message, LogLevel.Debug);
                                if (Settings.Get<bool>("UseTorProxies"))
                                {
                                    if (Settings.IsPremium && Settings.Get<bool>("ProxyAlwaysActive") ||
                                        Proxies.TorProxyActive)
                                    {
                                        await torProxy.GetNewIdentityAsync();
                                        mf.metroLabel45.Text =
                                            Convert.ToString(await WebUtils.GetCurrentIpAddressAsync(torSettings));
                                    }
                                }
                                continue;
                            }
                            catch (TaskCanceledException ex)
                            {
                                Logger.Write("TaskCanceledException: " + ex.Message, LogLevel.Debug);
                                if (Settings.Get<bool>("UseTorProxies"))
                                {
                                    if (Settings.IsPremium && Settings.Get<bool>("ProxyAlwaysActive") ||
                                        Proxies.TorProxyActive)
                                    {
                                        await torProxy.GetNewIdentityAsync();
                                        mf.metroLabel45.Text =
                                            Convert.ToString(await WebUtils.GetCurrentIpAddressAsync(torSettings));
                                    }
                                }
                                continue;
                            }
                            catch (Exception ex)
                            {
                                Logger.Write("Exception: " + ex.Message, LogLevel.Debug);
                                continue;
                            }
                            mf.webBrowser1.DocumentText = sourcecode;
                            await Task.Delay(500, cancelToken).ContinueWith(tsk => { });

                            //ERROR Detection
                            Match errordetection = Regex.Match(sourcecode.RemoveLineEndings(),
                                @"<title> (Tut uns Leid!|Toutes nos excuses)</title>");
                            Match errordetection1 = Regex.Match(sourcecode.RemoveLineEndings(),
                                @"<title>(503 - Service Unavailable Error)</title>");
                            if (errordetection.Success || errordetection1.Success)
                            {
                                Debug.WriteLine("Issue Found...");
                                continue;
                            }
                            //CAPTCHA Detection
                            Match captchadetection =
                                Regex.Match(sourcecode,
                                    "<title dir=\"ltr\">(Amazon CAPTCHA|Bot Check|Robot Check)</title>");
                            if (captchadetection.Success)
                            {
                                Debug.WriteLine("Captcha detected...");
                                if (Settings.Get<bool>("UseTorProxies"))
                                {
                                    await torProxy.GetNewIdentityAsync();
                                    mf.metroLabel45.Text =
                                        Convert.ToString(await WebUtils.GetCurrentIpAddressAsync(torSettings));
                                    Proxies.TorProxyActive = true;
                                    if (!Settings.Get<bool>("ProxyAlwaysActive") &&
                                        Proxies.RealIPnexttime < DateTime.Now)
                                        Proxies.RealIPnexttime = DateTime.Now.AddMinutes(15);
                                }
                                continue;
                            }

                            //How many Sites?
                            Match sitesNumber = Regex.Match(sourcecode, "<a name=\"New\">([^\"]*) / ([^\"]*)</a>");
                            if (sitesNumber.Success && loops == 0 && Settings.Get<int>("ScanMethod") == 0)
                                sites = Convert.ToInt32(
                                    Math.Ceiling(Convert.ToDecimal(sitesNumber.Groups[2].Value) / 10));
                            mf.metroLabel41.Text = $@"{loops + 1} / {sites} (NEU)";

                            //Get Price for NEW
                            if (sellerForNew.Contains("Amazon"))
                            {
                                Match newama = Regex.Match(sourcecode,
                                    "<a href=(.*)ca=([a-zA-Z0-9_]*)&vs=1(.*)>(Neu|Nuovo|Neuf|Nuevo|New) - (EUR |£)(.*)</a>");
                                if (newama.Success)
                                    priceNewList.Add(await CurrencyConverter.ConvertToEuro(newama.Groups[6].Value,
                                        Convert.ToString(entry["Store"])));
                            }
                            if (sellerForNew.Contains("Drittanbieter"))
                            {
                                if (sellerForNew.Contains("Versand per Amazon"))
                                {
                                    Regex new3rd =
                                        new Regex(
                                            "(Versand durch Amazon.de|Spedito da Amazon|EXPÉDIÉ PAR AMAZON|Distribuido por Amazon|Fulfilled by Amazon)(\r\n|\r|\n)</font>(\r\n|\r|\n)(\r\n|\r|\n)<br />(\r\n|\r|\n)(\r\n|\r|\n)(\r\n|\r|\n)" +
                                            "<a href=(.*)ca=([a-zA-Z0-9_]*)&eid=(.*)>(Neu|Nuovo|Neuf|Nuevo|New) - (EUR |£)(.*)</a>",
                                            RegexOptions.Compiled);
                                    foreach (Match itemMatch in new3rd.Matches(sourcecode))
                                        priceNewList.Add(
                                            await CurrencyConverter.ConvertToEuro(itemMatch.Groups[13].Value,
                                                Convert.ToString(entry["Store"])));
                                }
                                else
                                {
                                    Regex new3rd =
                                        new Regex(
                                            @"<a href=(.*)ca=([a-zA-Z0-9_]*)&eid=(.*)>(Neu|Nuovo|Neuf|Nuevo|New) - (EUR |£)(.*)</a>",
                                            RegexOptions.Compiled);
                                    foreach (Match itemMatch in new3rd.Matches(sourcecode))
                                        priceNewList.Add(
                                            await CurrencyConverter.ConvertToEuro(itemMatch.Groups[6].Value,
                                                Convert.ToString(entry["Store"])));
                                }
                            }
                            loops++;
                            if (sites > loops)
                                await Task.Delay(
                                        Tools.RandomNumber(Convert.ToInt32(mf.numericUpDown3.Value) * 1000 - 500,
                                            Convert.ToInt32(mf.numericUpDown3.Value) * 1000 + 500), cancelToken)
                                    .ContinueWith(tsk => { });
                        } while (sites > loops);

                        RemoveInvalidEntrys(priceNewList);
                        priceNew = priceNewList.OrderBy(price => price).FirstOrDefault();
                        if (!String.IsNullOrEmpty(priceNew))
                        {
                            mf.metroLabel24.Text = priceNew;
                            ComparePrice(entry["Preis: Neu"], priceNew, mf.metroLabel24);
                        }
                    }                  
                    await Task.Delay(Tools.RandomNumber(500, 1000), cancelToken).ContinueWith(tsk => { });

                    string priceLikeNew = null;
                    string priceVeryGood = null;
                    string priceGood = null;
                    string priceAcceptable = null;
                    if (Settings.Get<bool>("ScanUsed"))
                    {
                        List<string> priceLikeNewList = new List<string>();
                        List<string> priceVeryGoodList = new List<string>();
                        List<string> priceGoodList = new List<string>();
                        List<string> priceAcceptableList = new List<string>();
                        mf.tabControl2.SelectedTab = mf.TabPage5;
                        var conditionList = new List<Tuple<List<string>, string>>
                        {
                            new Tuple<List<string>, string>(priceLikeNewList,
                                "Wie neu|Condizioni pari al nuovo|Comme neuf|Como nuevo|Mint"),
                            new Tuple<List<string>, string>(priceVeryGoodList,
                                "Sehr gut|Ottimo|Très bon état|Muy bueno|Very good"),
                            new Tuple<List<string>, string>(priceGoodList,
                                "Gut|Buono|D'occasion - très bon état|Bueno|Good"),
                            new Tuple<List<string>, string>(priceAcceptableList,
                                "Akzeptabel|Accettabile|Acceptable|Aceptable|Acceptable")
                        };
                        sites = 1;
                        loops = 0;
                        do
                        {
                            if (cancelToken.IsCancellationRequested)
                            {
                                Timer.Stop();
                                torProxy.Stop();
                                return;
                            }
                            try
                            {
                                string url = "https://www.amazon." + Amazon.GetTld(Convert.ToString(entry["Store"])) +
                                             "/gp/aw/ol/" + entry["ASIN / ISBN"] +
                                             "/ref=mw_dp_olp?o=Used&op=" + (loops + 1);

                                var handler = new HttpClientHandler();
                                if (Settings.Get<bool>("UseTorProxies"))
                                {
                                    if (Settings.IsPremium && Settings.Get<bool>("ProxyAlwaysActive") || Proxies.TorProxyActive && Proxies.RealIPnexttime > DateTime.Now)
                                    {
                                        Debug.WriteLine("Add TOR Proxy to HttpClientHandler...");
                                        handler.Proxy = new WebProxy(new Uri("http://localhost:" + torSettings.PrivoxySettings.Port));
                                    }
                                    else if (Proxies.TorProxyActive)
                                    {
                                        Proxies.TorProxyActive = false;
                                        Proxies.RealIPnexttime = DateTime.Now;
                                    }
                                }
                                handler.CookieContainer = new CookieContainer();
                                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                                var httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
                                var useragent = await UserAgents.GetRandom();
                                httpClient.DefaultRequestHeaders.Add("User-Agent",
                                    !String.IsNullOrEmpty(useragent)
                                        ? useragent
                                        : "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
                                httpClient.DefaultRequestHeaders.Add("Referer", WebUtils.RandomReferal());

                                HttpResponseMessage response = await httpClient.GetAsync(new Uri(url, UriKind.Absolute), cancelToken);
                                sourcecode = await response.Content.ReadAsStringAsync();
                            }
                            catch (HttpRequestException ex)
                            {
                                Logger.Write("HttpRequestException: " + ex.Message, LogLevel.Debug);
                                if (Settings.Get<bool>("UseTorProxies"))
                                {
                                    if (Settings.IsPremium && Settings.Get<bool>("ProxyAlwaysActive") || Proxies.TorProxyActive)
                                    {
                                        await torProxy.GetNewIdentityAsync();
                                        mf.metroLabel45.Text = Convert.ToString(await WebUtils.GetCurrentIpAddressAsync(torSettings));
                                    }
                                }
                                continue;
                            }
                            catch (TaskCanceledException ex)
                            {
                                Logger.Write("TaskCanceledException: " + ex.Message, LogLevel.Debug);
                                if (Settings.Get<bool>("UseTorProxies"))
                                {
                                    if (Settings.IsPremium && Settings.Get<bool>("ProxyAlwaysActive") || Proxies.TorProxyActive)
                                    {
                                        await torProxy.GetNewIdentityAsync();
                                        mf.metroLabel45.Text = Convert.ToString(await WebUtils.GetCurrentIpAddressAsync(torSettings));
                                    }
                                }
                                continue;
                            }
                            catch (Exception ex)
                            {
                                Logger.Write("Exception: " + ex.Message, LogLevel.Debug);
                                continue;
                            }
                            mf.webBrowser2.DocumentText = sourcecode;
                            await Task.Delay(500, cancelToken).ContinueWith(tsk => { });

                            //ERROR Detection
                            Match errordetection = Regex.Match(sourcecode.RemoveLineEndings(), @"<title> (Tut uns Leid!|Toutes nos excuses)</title>");
                            Match errordetection1 = Regex.Match(sourcecode.RemoveLineEndings(), @"<title>(503 - Service Unavailable Error)</title>");
                            if (errordetection.Success || errordetection1.Success)
                            {
                                Debug.WriteLine("Issue Found...");
                                continue;
                            }
                            //CAPTCHA Detection
                            Match captchadetection = Regex.Match(sourcecode, "<title dir=\"ltr\">(Amazon CAPTCHA|Bot Check|Robot Check)</title>");
                            if (captchadetection.Success)
                            {
                                Debug.WriteLine("Captcha detected...");
                                if (Settings.Get<bool>("UseTorProxies"))
                                {
                                    await torProxy.GetNewIdentityAsync();
                                    mf.metroLabel45.Text = Convert.ToString(await WebUtils.GetCurrentIpAddressAsync(torSettings));
                                    Proxies.TorProxyActive = true;
                                    if (!Settings.Get<bool>("ProxyAlwaysActive") && Proxies.RealIPnexttime < DateTime.Now)
                                        Proxies.RealIPnexttime = DateTime.Now.AddMinutes(15);
                                }
                                continue;
                            }

                            //How many WHD Sites?
                            Match sitesNumber = Regex.Match(sourcecode, "<a name=\"Used\">([^\"]*) / ([^\"]*)</a>");
                            if (sitesNumber.Success && loops == 0 && Settings.Get<int>("ScanMethod") == 0)
                                sites = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(sitesNumber.Groups[2].Value) / 10));
                            mf.metroLabel41.Text = $@"{loops + 1} / {sites} (GEBRAUCHT)";

                            //Get Price for WHDs
                            //.de WHD Seller ID: A8KICS1PHF7ZO
                            //.it WHD Seller ID: A1HO9729ND375Y
                            //.fr WHD Seller ID: A2CVHYRTWLQO9T
                            //.es WHD Seller ID: A6T89FGPU3U0Q                     
                            //.co.uk WHD Seller ID: A2OAJ7377F756P  

                            if (sellerForUsed.Contains("Amazon"))
                            {
                                foreach (var condition in conditionList)
                                {
                                    Regex search =
                                        new Regex(
                                            @"<a href=(.*)(A8KICS1PHF7ZO|A1HO9729ND375Y|A2CVHYRTWLQO9T|A6T89FGPU3U0Q|A2OAJ7377F756P)(.*)>(" + condition.Item2 + ") - (EUR |£)(.*)</a>",
                                            RegexOptions.Compiled);
                                    foreach (Match itemMatch in search.Matches(sourcecode))
                                        condition.Item1.Add(await CurrencyConverter.ConvertToEuro(itemMatch.Groups[6].Value, Convert.ToString(entry["Store"])));
                                }
                            }
                            if (sellerForUsed.Contains("Drittanbieter"))
                            {
                                if (sellerForUsed.Contains("Versand per Amazon"))
                                {
                                    foreach (var condition in conditionList)
                                    {
                                        Regex search =
                                            new Regex(
                                                "(Versand durch Amazon.de|Spedito da Amazon|EXPÉDIÉ PAR AMAZON|Distribuido por Amazon|Fulfilled by Amazon)(\r\n|\r|\n)</font>(\r\n|\r|\n)(\r\n|\r|\n)<br />(\r\n|\r|\n)(\r\n|\r|\n)(\r\n|\r|\n)" +
                                                "<a href=(.*)&me=(?!(A8KICS1PHF7ZO|A1HO9729ND375Y|A2CVHYRTWLQO9T|A6T89FGPU3U0Q|A2OAJ7377F756P).)(.*)>(" + condition.Item2 + ") - (EUR |£)(.*)</a>",
                                                RegexOptions.Compiled);
                                        foreach (Match itemMatch in search.Matches(sourcecode))
                                            condition.Item1.Add(await CurrencyConverter.ConvertToEuro(itemMatch.Groups[13].Value, Convert.ToString(entry["Store"])));
                                    }
                                }
                                else
                                {
                                    foreach (var condition in conditionList)
                                    {
                                        Regex search =
                                            new Regex(
                                                @"<a href=(.*)&me=(?!(A8KICS1PHF7ZO|A1HO9729ND375Y|A2CVHYRTWLQO9T|A6T89FGPU3U0Q|A2OAJ7377F756P).)(.*)>(" + condition.Item2 + ") - (EUR |£)(.*)</a>",
                                                RegexOptions.Compiled);
                                        foreach (Match itemMatch in search.Matches(sourcecode))
                                            condition.Item1.Add(await CurrencyConverter.ConvertToEuro(itemMatch.Groups[6].Value, Convert.ToString(entry["Store"])));
                                    }
                                }
                            }
                            loops++;
                            if (sites > loops)
                                await Task.Delay(Tools.RandomNumber(Convert.ToInt32(mf.numericUpDown3.Value) * 1000 - 500, Convert.ToInt32(mf.numericUpDown3.Value) * 1000 + 500), cancelToken).ContinueWith(tsk => { });
                        } while (sites > loops);

                        foreach (var condition in conditionList)
                        {
                            RemoveInvalidEntrys(condition.Item1);
                        }
                        priceLikeNew = priceLikeNewList.OrderBy(price => price).FirstOrDefault();
                        if (!String.IsNullOrEmpty(priceLikeNew))
                        {
                            mf.metroLabel25.Text = priceLikeNew;
                            ComparePrice(entry["Preis: Wie Neu"], priceLikeNew, mf.metroLabel25);
                        }
                        priceVeryGood = priceVeryGoodList.OrderBy(price => price).FirstOrDefault();
                        if (!String.IsNullOrEmpty(priceVeryGood))
                        {
                            mf.metroLabel26.Text = priceVeryGood;
                            ComparePrice(entry["Preis: Sehr Gut"], priceVeryGood, mf.metroLabel26);
                        }
                        priceGood = priceGoodList.OrderBy(price => price).FirstOrDefault();
                        if (!String.IsNullOrEmpty(priceGood))
                        {
                            mf.metroLabel27.Text = priceGood;
                            ComparePrice(entry["Preis: Gut"], priceGood, mf.metroLabel27);
                        }
                        priceAcceptable = priceAcceptableList.OrderBy(price => price).FirstOrDefault();
                        if (!String.IsNullOrEmpty(priceAcceptable))
                        {
                            mf.metroLabel28.Text = priceAcceptable;
                            ComparePrice(entry["Preis: Akzeptabel"], priceAcceptable, mf.metroLabel28);
                        }
                    }
                    await Task.Delay(Tools.RandomNumber(500, 1000), cancelToken).ContinueWith(tsk => { });

                    Database.OpenConnection();
                    SQLiteCommand updateEntry =
                        new SQLiteCommand(
                            "UPDATE Products SET [Preis: Neu] = @priceNew, [Preis: Wie Neu] = @priceLikenew, [Preis: Sehr Gut] = @priceVerygood, [Preis: Gut] = @priceGood, [Preis: Akzeptabel] = @priceAcceptable, [Letzter Check] = @lastcheck WHERE ID = @id",
                            Database.Connection);
                    updateEntry.Parameters.AddWithValue("@id", entry["ID"]);
                    updateEntry.Parameters.AddWithValue("@priceNew", !String.IsNullOrEmpty(priceNew) ? priceNew : null);
                    updateEntry.Parameters.AddWithValue("@priceLikenew", !String.IsNullOrEmpty(priceLikeNew) ? priceLikeNew : null);
                    updateEntry.Parameters.AddWithValue("@priceVerygood", !String.IsNullOrEmpty(priceVeryGood) ? priceVeryGood : null);
                    updateEntry.Parameters.AddWithValue("@priceGood", !String.IsNullOrEmpty(priceGood) ? priceGood : null);
                    updateEntry.Parameters.AddWithValue("@priceAcceptable", !String.IsNullOrEmpty(priceAcceptable) ? priceAcceptable : null);
                    updateEntry.Parameters.AddWithValue("@lastcheck", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    updateEntry.ExecuteNonQuery();

                    await Task.Delay(250, cancelToken).ContinueWith(tsk => { });
                  
                    if (mf.metroComboBox2.SelectedIndex == -1 || mf.metroComboBox2.Text == "ALLE" || store == mf.metroComboBox2.Text)
                        ProductDatabase.Display(mf.metroComboBox2.SelectedIndex == -1 ? "ALLE" : mf.metroComboBox2.Text);


                    await MyReminder.DoRemindWhenPossible(Convert.ToString(entry["ID"]));


                    _randomWait = Tools.RandomNumber(Convert.ToInt32(mf.numericUpDown2.Value) - 1, Convert.ToInt32(mf.numericUpDown2.Value) + 1);
                    mf.metroProgressBar1.Value = 0;
                    mf.metroProgressBar1.Maximum = _randomWait * 1000;
                    Timer.Start();
                    mf.metroLabel29.Text = _dt.AddSeconds(_randomWait).ToString("mm:ss");

                    await Task.Delay((_randomWait + 1) * 1000, cancelToken).ContinueWith(tsk => { });
                }
            }
        }

        public static void RemoveInvalidEntrys(List<string> list)
        {
            foreach (var entry in list)
            {
                if (double.TryParse(entry, out double num)) continue;
                var itemToRemove = list.SingleOrDefault(x => x == entry);
                if (itemToRemove != null)
                    list.Remove(itemToRemove);
            }
        }

        public static void ComparePrice(object db_price, string grabbed_priced, MetroLabel label)
        {
            if (String.IsNullOrWhiteSpace(Convert.ToString(db_price)))
            {
                label.ForeColor = System.Drawing.Color.Black;
            }
            else if (Convert.ToDecimal(grabbed_priced) == Convert.ToDecimal(db_price))
            {
                label.ForeColor = System.Drawing.Color.Orange;
                label.Text = label.Text + " ●";
            }
            else if (Convert.ToDecimal(grabbed_priced) > Convert.ToDecimal(db_price))
            {
                Decimal difference = Convert.ToDecimal(grabbed_priced) - Convert.ToDecimal(db_price);
                label.ForeColor = System.Drawing.Color.Red;
                label.Text = label.Text + " ▲ (+ " + Convert.ToString(difference) + ")";
            }
            else if (Convert.ToDecimal(grabbed_priced) < Convert.ToDecimal(db_price))
            {
                Decimal difference = Convert.ToDecimal(db_price) - Convert.ToDecimal(grabbed_priced);
                label.ForeColor = System.Drawing.Color.Green;
                label.Text = label.Text + " ▼ (- " + Convert.ToString(difference) + ")";
            }
        }

        public static void Timer_Tick(object sender, EventArgs e)
        {
            _randomWait--;

            // Perform one step...
            mf.metroProgressBar1.PerformStep();

            if (_randomWait < 0)
                Timer.Stop();
            else
                mf.metroLabel29.Text = _dt.AddSeconds(_randomWait).ToString("mm:ss");
        }
    }
}

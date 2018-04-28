using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DealReminder_Windows.Configs;
using DealReminder_Windows.Logging;

namespace DealReminder_Windows.Utils
{
    internal class Proxies
    {
        public static Dictionary<string, int> Proxielist = new Dictionary<string, int>();
        private static Dictionary<string, int> _proxiewithRepsonsetime = new Dictionary<string, int>();
        public static string CurrentProxy = null;
        public static DateTime RealIPnexttime = DateTime.Now;
        public static bool TorProxyActive = false;

        public static async Task<string> GetProxie()
        {
            if (CurrentProxy != null)
                RemoveProxy();

            Logger.Write("Get Proxy...", LogLevel.Debug);
            if (Tools.DictionaryIsNullOrEmpty(Proxielist))
            {
                Logger.Write("Proxy Liste ist Leer, hole neue Proxies von Website", LogLevel.Debug);
                Proxielist = Tools.DictionaryMerge(Proxielist , await DealReminderDe()).GroupBy(pair => pair.Key)
                    .Select(group => group.First())
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
                if (Tools.DictionaryIsNullOrEmpty(Proxielist))
                {
                    Logger.Write("Keine Proxies gefunden. Checke Internetverbindung.", LogLevel.Debug);
                    if (OSystem.IsAvailableNetworkActive()) { }
                    return null;
                }
            }

            foreach (KeyValuePair<string, int> proxy in Proxielist)
            {
                Logger.Write(proxy.Key + " - Response Time: " + proxy.Value, LogLevel.Debug);
                if (proxy.Key == Proxielist.Keys.Last())
                    Logger.Write("------", LogLevel.Debug);
            }

            _proxiewithRepsonsetime.Clear();
            await Tools.ProcessParalell(Proxielist, CheckResponseTime, 10);
            Proxielist = _proxiewithRepsonsetime.Where(p => true).OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            Logger.Write("Working Proxies: " + Proxielist.Count, LogLevel.Debug);
            Logger.Write("------", LogLevel.Debug);

            foreach (KeyValuePair<string, int> proxy in Proxielist)
            {
                Logger.Write(proxy.Key + " - Response Time: " + proxy.Value, LogLevel.Debug);
                if (proxy.Key == Proxielist.Keys.Last())
                    Logger.Write("------", LogLevel.Debug);
            }

            var result = !Tools.DictionaryIsNullOrEmpty(Proxielist) ? Proxielist.First().Key : null;
            Logger.Write("Proxie Ausgwählt: " + result, LogLevel.Debug);

            if (!Settings.Get<bool>("ProxyAlwaysActive") && result != null && RealIPnexttime < DateTime.Now)
                RealIPnexttime = DateTime.Now.AddMinutes(15);

            return result;
        }

        private static async Task CheckResponseTime(KeyValuePair<string, int> proxy)
        {
            int responseTime = await CheckProxyAsync(new WebProxy(proxy.Key), 5000);
            if (responseTime > 0)
            {
                Debug.WriteLine("Working Proxy: " + proxy.Key + " with Response Time: " + responseTime);
                if (!_proxiewithRepsonsetime.ContainsKey(proxy.Key))
                    _proxiewithRepsonsetime.Add(proxy.Key, responseTime);
            }
            else
            {
                Debug.WriteLine("Unresponsive Proxy: " + proxy.Key);
            }
            await Task.FromResult(0);
        }

        private static async Task<int> CheckProxyAsync(WebProxy proxy, int to)
        {
            var httpClientHandler = new HttpClientHandler
            {
                Proxy = proxy,
                UseProxy = true
            };
            var client = new HttpClient(httpClientHandler) { Timeout = TimeSpan.FromMilliseconds(to) };
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");

            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                HttpResponseMessage response = await client.GetAsync(new Uri("https://api.ipify.org"));
                sw.Stop();

                Debug.WriteLine("StatusCode: " + response.StatusCode);
                return response.IsSuccessStatusCode ? (int)sw.Elapsed.TotalMilliseconds : 0;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine("HttpRequestException - Reason: " + ex.Message);
                return 0;
            }
            catch (TaskCanceledException)
            {
                sw.Stop();
                Debug.WriteLine("TaskCanceledException - Timeout: " + (int)sw.Elapsed.TotalMilliseconds + "ms");
                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static async Task<Dictionary<string, int>> DealReminderDe()
        {
            Dictionary<string, int> proxieList = new Dictionary<string, int>();
            try
            {
                // Ignore Certificate validation failures (aka untrusted certificate + certificate chains)
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                Stream data = await new BetterWebClient { Timeout = 2500 }.OpenReadTaskAsync("https://tools.dealreminder.de/proxies/get.php");
                using (var reader = new StreamReader(data))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!proxieList.ContainsKey(line))
                            proxieList.Add(line, 0);
                    }
                }
            }
            catch
            {
                // ignored
            }
            Logger.Write("Grabbed " + proxieList.Count + " from: dealreminder.de", LogLevel.Debug);
            return proxieList;
        }

        public static void RemoveProxy(bool resetTime = false)
        {
            Proxielist.Remove(CurrentProxy);
            CurrentProxy = null;
            if (resetTime)
                RealIPnexttime = DateTime.Now;
        }
    }
}

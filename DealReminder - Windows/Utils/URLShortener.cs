using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using DealReminder_Windows.Logging;

namespace DealReminder_Windows.Utils
{
    internal class URLShortener
    {
        public static async Task<string> Generate(string urlToShorten, string customAlias = null, string store = null)
        {
            try
            {
                for (int i = 0; i < 2; i++)
                {
                    string url = "https://s.dealreminder.de/api/?api=IVHho58w7dwI&url=" + HttpUtility.UrlEncode(urlToShorten);
                    if (customAlias != null)
                    {
                        customAlias = HttpUtility.UrlEncode(Tools.ReplaceGermanAccents(customAlias));
                        customAlias = customAlias?.Substring(0, customAlias.Length >= 125 ? 125 : customAlias.Length);
                        url = url + "&custom=" + customAlias;
                        if (store != null)
                            url = url + "_" + store;
                    }
                    Debug.WriteLine(url);
                    var json = await new BetterWebClient { Timeout = 5000 }.DownloadStringTaskAsync(new Uri(url));
                    var result = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(json);
                    var error = result["error"];
                    switch (error)
                    {
                        case "0":
                            return Convert.ToString(result["short"]);
                        case "1":
                            if (result["msg"] !=
                                "Der Aliasname \u200b\u200bist bereits vergeben. Bitte w\u00e4hle einen anderen.")
                                return null;
                            customAlias = null;
                            continue;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Write("URL Shorten Fehlgeschlagen - Grund: " + ex.Message, LogLevel.Debug);
                return null;
            }
        }
    }
}

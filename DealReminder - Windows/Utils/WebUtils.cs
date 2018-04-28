using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DealReminder_Windows.Logging;
using Knapcode.TorSharp;

namespace DealReminder_Windows.Utils
{
    public static class WebUtils
    {
        public static string RandomReferal()
        {
            string[] words =
            {
                "Amazon", "Amazon Prime", "Amazon Echo", "Amazon Video", "Amazon Fire TV",
                "Amazon Fire TV Stick"
            };

            string[] referal =
            {
                "Referer: http://www.google.de/search?sclient=psy&hl=de&site=&source=hp&q=" + Tools.RandomWord(words) +
                "&btnG=Suche"
                ,
                "Referer: http://www.google.ch/search?sclient=psy&hl=ch&site=&source=hp&q=" + Tools.RandomWord(words) +
                "&btnG=Google-Suche"
                ,
                "Referer: http://www.google.fr/search?sclient=psy&hl=fr&site=&source=hp&q=" + Tools.RandomWord(words) +
                "&btnG=Rechercher"
                ,
                "Referer: http://www.google.se/search?sclient=psy&hl=se&site=&source=hp&q=" + Tools.RandomWord(words) +
                "&btnG=Sök+på+Google"
                ,
                "Referer: http://www.google.co.uk/search?sclient=psy&hl=en&site=&source=hp&q=" + Tools.RandomWord(words) +
                "&btnG=Search"
                ,
                "Referer: http://www.google.li/search?sclient=psy&hl=de&site=&source=hp&q=" + Tools.RandomWord(words) +
                "&btnG=Suche"
                ,
                "Referer: http://www.google.com/search?sclient=psy&hl=en&site=&source=hp&q=" + Tools.RandomWord(words) +
                "&btnG=Search"
                ,
                "Referer: http://www.google.be/search?sclient=psy&hl=be&site=&source=hp&q=" + Tools.RandomWord(words) +
                "&btnG=Google+zoeken"
                ,
                "Referer: http://www.google.us/search?sclient=psy&hl=en&site=&source=hp&q=" + Tools.RandomWord(words) +
                "&btnG=Search"
                ,
                "Referer: http://www.google.nl/search?sclient=psy&hl=nl&site=&source=hp&q=" + Tools.RandomWord(words) +
                "&btnG=Google+zoeken"
            };
            return referal[Tools.RandomNumber(0, referal.Length)];
        }

        public static string RemoveLineEndings(this string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return value;
            }
            string lineSeparator = ((char)0x2028).ToString();
            string paragraphSeparator = ((char)0x2029).ToString();

            return value.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(lineSeparator, string.Empty).Replace(paragraphSeparator, string.Empty);
        }

        public static async Task<IPAddress> GetCurrentIpAddressAsync(TorSharpSettings settings = null)
        {
            var handler = new HttpClientHandler();
            if (settings != null)
                handler.Proxy = new WebProxy(new Uri("http://localhost:" + settings.PrivoxySettings.Port));
            try
            {
                using (var httpClient = new HttpClient(handler))
                {
                    var ip = (await httpClient.GetStringAsync("http://ipv4.icanhazip.com")).Trim();
                    return IPAddress.Parse(ip);
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Fehler beim Abfragen der IP-Adresse - Grund: " + ex.Message, LogLevel.Debug);
                return IPAddress.Parse("0.0.0.0");
            }
        }

        public static async Task<string> DownloadStringAsync(Uri uri, int timeOut = 60000, WebProxy proxy = null)
        {
            string output = null;
            bool cancelledOrError = false;
            using (var client = new BetterWebClient())
            {
                if (proxy != null)
                    client.Proxy = proxy;
                client.DownloadStringCompleted += (sender, e) =>
                {
                    if (e.Error != null || e.Cancelled)
                    {
                        cancelledOrError = true;
                    }
                    else
                    {
                        output = e.Result;
                    }
                };
                client.DownloadStringAsync(uri);
                var n = DateTime.Now;
                while (output == null && !cancelledOrError && DateTime.Now.Subtract(n).TotalMilliseconds < timeOut)
                {
                    await Task.Delay(100); // wait for respsonse
                }
            }
            return output;
        }

        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception ex)
            {
                Logger.Write("Browser öffnen mit URL \"https://t.me/joinchat/AAAAAEK4xY7BkQ1QlLoNhQ\" Fehlgeschlagen - Grund: " + ex.Message, LogLevel.Debug);
            }
        }

        public static Image GetImageFromUrl(string url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            using (HttpWebResponse httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (Stream stream = httpWebReponse.GetResponseStream())
                {
                    return Image.FromStream(stream);
                }
            }
        }
    }

    public static class WebClientExtensions
    {
        public static Encoding GetEncodingFrom(
            NameValueCollection responseHeaders,
            Encoding defaultEncoding = null)
        {
            if (responseHeaders == null)
                throw new ArgumentNullException(nameof(responseHeaders));

            //Note that key lookup is case-insensitive
            var contentType = responseHeaders["Content-Type"];
            if (contentType == null)
                return defaultEncoding;

            var contentTypeParts = contentType.Split(';');
            if (contentTypeParts.Length <= 1)
                return defaultEncoding;

            var charsetPart =
                contentTypeParts.Skip(1).FirstOrDefault(
                    p => p.TrimStart().StartsWith("charset", StringComparison.InvariantCultureIgnoreCase));
            if (charsetPart == null)
                return defaultEncoding;

            var charsetPartParts = charsetPart.Split('=');
            if (charsetPartParts.Length != 2)
                return defaultEncoding;

            var charsetName = charsetPartParts[1].Trim();
            if (charsetName == "")
                return defaultEncoding;

            try
            {
                return Encoding.GetEncoding(charsetName);
            }
            catch (ArgumentException)
            {
                //throw new InvalidOperationException("The server returned data in an unknown encoding: " + charsetName + ex);
                return null;
            }
        }

        public static async Task<string> DownloadStringAwareOfEncoding(this WebClient webClient, Uri uri)
        {
            byte[] rawData = null;
            string error;
            try
            {
                error = "loading";
                rawData = await webClient.DownloadDataTaskAsync(uri);
            }
            catch (NullReferenceException)
            {
                error = null;
            }
            catch (ArgumentNullException)
            {
                error = null;
            }
            catch (WebException)
            {
                error = null;
            }
            catch (SocketException)
            {
                error = null;
            }

            if (error == null || rawData == null)
                return null;

            var encoding = GetEncodingFrom(webClient.ResponseHeaders, Encoding.UTF8);
            return encoding.GetString(rawData);
        }
    }

    /// <summary>
    /// A (slightly) better version of .Net's default <see cref="WebClient"/>.
    /// The extra features include:
    /// ability to disable automatic redirect handling,
    /// sessions through a cookie container,
    /// indicate to the webserver that GZip compression can be used,
    /// exposure of the HTTP status code of the last request,
    /// exposure of any response header of the last request,
    /// ability to modify the request before it is send.
    /// </summary>
    /// <seealso cref="System.Net.WebClient" />
    public class BetterWebClient : WebClient
    {
        private WebRequest _request = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="BetterWebClient" /> class.
        /// </summary>
        /// <param name="cookies">The cookies. If set to <c>null</c> a container will be created.</param>
        /// <param name="autoRedirect">if set to <c>true</c> the client should handle the redirect automatically. Default value is <c>true</c></param>
        public BetterWebClient(CookieContainer cookies = null, bool autoRedirect = true)
        {
            CookieContainer = cookies ?? new CookieContainer();
            AutoRedirect = autoRedirect;
        }

        /// <summary>
        /// Gets or sets a value for Timeout.
        /// </summary>
        /// <value>
        ///   The Timeout Time-
        /// </value>
        public int? Timeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically redirect when a 301 or 302 is returned by the request.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the client should handle the redirect automatically; otherwise, <c>false</c>.
        /// </value>
        public bool AutoRedirect { get; set; }

        /// <summary>
        /// Gets or sets the cookie container. This contains all the cookies for all the requests.
        /// </summary>
        /// <value>
        ///   The cookie container.
        /// </value>
        public CookieContainer CookieContainer { get; set; }

        /// <summary>
        /// Gets the cookies header (Set-Cookie) of the last request.
        /// </summary>
        /// <value>
        ///   The cookies or <c>null</c>.
        /// </value>
        public string Cookies => GetHeaderValue("Set-Cookie");

        /// <summary>
        /// Gets the location header for the last request.
        /// </summary>
        /// <value>
        ///   The location or <c>null</c>.
        /// </value>
        public string Location => GetHeaderValue("Location");

        /// <summary>
        /// Gets the status code. When no request is present, <see cref="HttpStatusCode.Gone"/> will be returned.
        /// </summary>
        /// <value>
        ///   The status code or <see cref="HttpStatusCode.Gone"/>.
        /// </value>
        public HttpStatusCode StatusCode
        {
            get
            {
                var result = HttpStatusCode.Gone;

                if (_request != null)
                {

                    if (base.GetWebResponse(_request) is HttpWebResponse response)
                    {
                        result = response.StatusCode;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets or sets the setup that is called before the request is done.
        /// </summary>
        /// <value>
        ///   The setup.
        /// </value>
        public Action<HttpWebRequest> Setup { get; set; }

        /// <summary>
        /// Gets the header value.
        /// </summary>
        /// <param name="headerName">Name of the header.</param>
        /// <returns>The value.</returns>
        public string GetHeaderValue(string headerName)
        {
            return _request != null ? GetWebResponse(_request)?.Headers?[headerName] : null;
        }

        /// <summary>
        /// Returns a <see cref="T:System.Net.WebRequest" /> object for the specified resource.
        /// </summary>
        /// <param name="address">A <see cref="T:System.Uri" /> that identifies the resource to request.</param>
        /// <returns>
        /// A new <see cref="T:System.Net.WebRequest" /> object for the specified resource.
        /// </returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address) as HttpWebRequest;
            if (request == null) return base.GetWebRequest(address);

            IWebProxy wp = WebRequest.DefaultWebProxy;
            wp.Credentials = CredentialCache.DefaultCredentials;
            request.Proxy = wp;

            if (Timeout.HasValue) request.Timeout = Timeout.Value;
            request.AllowAutoRedirect = AutoRedirect;
            request.CookieContainer = CookieContainer;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            Setup?.Invoke(request);

            return request;
        }
    }
}

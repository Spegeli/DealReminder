using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DealReminder_Windows.Utils
{
    internal class CurrencyConverter
    {
        private static DateTime _lastCheck;
        private static string _gbpRate = String.Empty;

        public static async Task GetCurrencyRate()
        {
            try
            {
                using (var wClient = new BetterWebClient { Timeout = 30000 })
                {
                    string sourcecode =
                        await wClient.DownloadStringAwareOfEncoding(
                            new Uri("http://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml"));;
                    Match gpdregex = Regex.Match(sourcecode, "<Cube currency='GBP' rate='(.*)'/>");
                    if (gpdregex.Success)
                        _gbpRate = gpdregex.Groups[1].Value;
                }
                _lastCheck = DateTime.Now;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static async Task<string> ConvertToEuro(string current, string currentcountry)
        {
            currentcountry = currentcountry.ToUpper();
            switch (currentcountry)
            {
                case "UK":
                    current = Convert.ToString(Double.Parse(current, new CultureInfo("en-UK")), new CultureInfo("en-UK"));
                    return await CurrencyCalculator(current, "GBP");
            }
            return Convert.ToString(Convert.ToDecimal(current), new CultureInfo("de-DE"));
        }

        public static async Task<string> CurrencyCalculator(string current, string currentcurrency)
        {
            if (_lastCheck.AddMinutes(15) < DateTime.Now)
                await GetCurrencyRate();

            if (currentcurrency.ToUpper() == "GBP")
                currentcurrency = _gbpRate;
            if (String.IsNullOrEmpty(currentcurrency))
                return null;

            current = current.Replace(",", ".");
            decimal currentconverted = decimal.Parse(current, NumberStyles.Currency, CultureInfo.GetCultureInfo("en-US"));
            decimal rate = Convert.ToDecimal(currentcurrency, new CultureInfo("en-US"));
            return $"{currentconverted / rate:0.00}";
        }
    }
}

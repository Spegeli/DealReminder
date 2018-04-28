using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DealReminder_Windows.Utils
{
    internal class UserAgents
    {
        private static List<string> _randomGeneratedUserAgents = new List<string>();
        private static Random _random = new Random();

        public static async Task<string> GetRandom()
        {
            string result = null;
            if (!_randomGeneratedUserAgents.Any())
            {
                _randomGeneratedUserAgents.Clear();
                var browserList = new Dictionary<int, string>
                {
                    {4, "Opera"},
                    {6, "EDGE"},
                    {8, "Safari"},
                    {11, "IExplorer"},
                    {34, "Chrome"},
                    {37, "FireFox"}
                };

                var operaOS = new Dictionary<int, string>
                {
                    {3, "mac"},
                    {6, "lin"},
                    {91, "win"}
                };
                var edgeOS = new Dictionary<int, string>
                {
                    {100, "win"}
                };
                var safariOS = new Dictionary<int, string>
                {
                    {100, "mac"}
                };
                var iexplorerOS = new Dictionary<int, string>
                {
                    {100, "win"}
                };
                var chromeOS = new Dictionary<int, string>
                {
                    {2, "lin"},
                    {9, "mac"},
                    {89, "win"}
                };
                var firefoxOS = new Dictionary<int, string>
                {
                    {1, "lin"},
                    {16, "mac"},
                    {83, "win"}
                };

                for (int i = 0; i < 250; i++)
                {
                    string browser = browserList.ChooseByRandom();
                    string OS = String.Empty;
                    string UA = String.Empty;
                    switch (browser)
                    {
                        case "Opera":
                            OS = operaOS.ChooseByRandom();
                            UA = "Mozilla/5.0 " + Opera(OS);
                            break;
                        case "EDGE":
                            OS = edgeOS.ChooseByRandom();
                            UA = "Mozilla/5.0 " + EDGE(OS);
                            break;
                        case "Safari":
                            OS = safariOS.ChooseByRandom();
                            UA = "Mozilla/5.0 " + Safari(OS);
                            break;
                        case "IExplorer":
                            OS = iexplorerOS.ChooseByRandom();
                            UA = "Mozilla/5.0 " + IExplorer(OS);
                            break;
                        case "Chrome":
                            OS = chromeOS.ChooseByRandom();
                            UA = "Mozilla/5.0 " + Chrome(OS);
                            break;
                        case "FireFox":
                        default:
                            OS = firefoxOS.ChooseByRandom();
                            UA = "Mozilla/5.0 " + FireFox(OS);
                            break;
                    }
                    if (!_randomGeneratedUserAgents.Contains(UA))
                        _randomGeneratedUserAgents.Add(UA);
                }
            }

            int r = _random.Next(_randomGeneratedUserAgents.Count);
            result = _randomGeneratedUserAgents[r];
            _randomGeneratedUserAgents.RemoveAt(r);

            await Task.FromResult(0);

            return result;
        }

        private static string Opera(string OS)
        {
            string webkitVer = webkitVersion();
            switch (OS)
            {
                case "lin":
                    return "(X11; Linux " + proc("lin") + ") AppleWebKit/" + webkitVer + " (KHTML, like Gecko) Chrome/" + chromeVersion() + " Safari/" + webkitVer + " OPR/" + operaVersion();
                case "mac":
                    return "(Macintosh; " + proc("mac") + " Mac OS X " + osxVersion() + ") AppleWebKit/" + webkitVer + " (KHTML, like Gecko) Chrome/" + chromeVersion() + " Safari/" + webkitVer + " OPR/" + operaVersion();
                case "win":
                default:
                    return "(Windows NT " + ntVersion() + "; " + proc("win") + ") AppleWebKit/" + webkitVer + " (KHTML, like Gecko) Chrome/" + chromeVersion() + " Safari/" + webkitVer + " OPR/" + operaVersion();
            }
        }
        private static string EDGE(string OS)
        {
            string webkitVer = webkitVersion();
            int r = _random.Next(12, 15);
            string edgeVer = r + "." + _random.Next(r * 1000, r * 1000 + 999);
            switch (OS)
            {
                case "win":
                default:
                    return "(Windows NT " + ntVersion() + "; " + proc("win") + ") AppleWebKit/" + webkitVer + " (KHTML, like Gecko) Chrome/" + chromeVersion() + " Safari/" + webkitVer + " Edge/" + edgeVer;
            }
        }
        private static string Safari(string OS)
        {
            string webkitVer = webkitVersion(true);
            string version = "10." + _random.Next(0, 1) + "." + _random.Next(1, 3);
            switch (OS)
            {
                case "mac":
                default:
                    return "(Macintosh; " + proc("mac") + " Mac OS X " + osxVersion() + ") AppleWebKit/" + webkitVer + " (KHTML, like Gecko) Version/" + version + " Safari/" + webkitVer;
            }
        }
        private static string IExplorer(string OS)
        {
            string ieVersion = _random.Next(8, 11) + ".0";
            string tridentVersion = _random.Next(4, 7) + ".0";
            switch (OS)
            {
                case "win":
                default:
                    return "(compatible; MSIE " + ieVersion + "; Windows NT " + ntVersion() + "; " + proc("win") + "; Trident/" + tridentVersion + ")";
            }
        }
        private static string Chrome(string OS)
        {
            string webkitVer = webkitVersion();
            switch (OS)
            {
                case "lin":
                    return "(X11; Linux " + proc("lin") + ") AppleWebKit/" + webkitVer + " (KHTML, like Gecko) Chrome/" + chromeVersion() + " Safari/" + webkitVer;
                case "mac":
                    return "(Macintosh; " + proc("mac") + " Mac OS X " + osxVersion() + ") AppleWebKit/" + webkitVer + " (KHTML, like Gecko) Chrome/" + chromeVersion() + " Safari/" + webkitVer;
                case "win":
                default:
                    return "(Windows NT " + ntVersion() + "; " + proc("win") + ") AppleWebKit/" + webkitVer + " (KHTML, like Gecko) Chrome/" + chromeVersion() + " Safari/" + webkitVer;
            }
        }
        private static string FireFox(string OS)
        {
            int geckoVersion = _random.Next(18, 53);
            switch (OS)
            {
                case "lin":
                    return "(X11; Linux " + proc("lin") + "; rv:" + geckoVersion + ".0) Gecko/20100101 Firefox/" + geckoVersion + ".0";
                case "mac":
                    return "(Macintosh; " + proc("mac") + " Mac OS X " + osxVersion() + "; rv:" + geckoVersion + ".0) Gecko/20100101 Firefox/" + geckoVersion + ".0";
                case "win":
                default:
                    return "(Windows NT " + ntVersion() + "; " + proc("win") + "; rv:" + geckoVersion + ".0) Gecko/20100101 Firefox/" + geckoVersion + ".0";
            }
        }

        private static string webkitVersion(bool forSafari = false)
        {
            if (forSafari)
                return _random.Next(600, 603) + "." + _random.Next(1, 4) + "." + _random.Next(1, 15);
            return _random.Next(531, 537) + "." + _random.Next(0, 36);
        }
        private static string operaVersion()
        {
            int r = _random.Next(15, 45);
            return r + ".0." + _random.Next(r * 56, r * 58) + "." + _random.Next(1, 500);
        }
        private static string chromeVersion()
        {
            int r = _random.Next(40, 58);
            return r + ".0." + _random.Next(r * 52, r * 55) + "." + _random.Next(0, 75);
        }
        private static string ntVersion()
        {
            string[] ver = { "5.0", "5.1", "5.2", "6.0", "6.1", "6.2", "6.3", "10.0" };
            return ver[_random.Next(ver.Length)];
        }
        private static string osxVersion()
        {
            return "10_" + _random.Next(5, 11) + "_" + _random.Next(0, 5);
        }
        private static string proc(string OS)
        {
            string[] procs = { };
            switch (OS)
            {
                case "lin":
                    procs = new[] { "i686", "x86_64", "i686 on x86_64" };
                    break;
                case "mac":
                    procs = new[] { "Intel", "PPC", "U; Intel", "U; PPC" };
                    break;
                case "win":
                    procs = new[] { "WOW64", "Win64; x64" };
                    break;
            }
            return procs[_random.Next(procs.Length)];
        }
    }

    public static class ProportionValue
    {
        private static Random _random = new Random();
        public static string ChooseByRandom(this Dictionary<int, string> collection)
        {
            int perCent = _random.Next(0, 100);
            int sum = 0;
            foreach (var entry in collection)
            {
                sum += entry.Key;
                if (perCent <= sum)
                {
                    //Debug.WriteLine("perCent: " + perCent + " / Sum: " + sum + " - Value: " + entry.Value);
                    return entry.Value;
                }
            }
            return null;
        }
    }
}

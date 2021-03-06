﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Forms;
using System.Xml;
using Timer = System.Threading.Timer;

namespace DealReminder_Windows.Utils
{
    internal class Tools
    {
        public static XmlDocument GetXmlDocFromBytes(byte[] bytes)
        {
            XmlDocument xmlDoc = new XmlDocument();
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                xmlDoc.Load(ms);
            }
            return xmlDoc;
        }

        public static async Task ProcessParalell<T>(IEnumerable<T> items, Func<T, Task> func, int maxparalell)
        {
            ExecutionDataflowBlockOptions edfbo = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = maxparalell
            };

            ActionBlock<T> ab = new ActionBlock<T>(func, edfbo);

            foreach (T item in items)
            {
                await ab.SendAsync(item);
            }

            ab.Complete();
            await ab.Completion;
        }

        public static Dictionary<K, V> DictionaryMerge<K, V>(Dictionary<K, V> me, params IDictionary<K, V>[] others)
        {
            var newMap = new Dictionary<K, V>(me, me.Comparer);
            foreach (IDictionary<K, V> src in
                (new List<IDictionary<K, V>> { me }).Concat(others))
            {
                // ^-- echk. Not quite there type-system.
                foreach (KeyValuePair<K, V> p in src)
                {
                    newMap[p.Key] = p.Value;
                }
            }
            return newMap;
        }

        public static bool DictionaryIsNullOrEmpty<T, U>(IDictionary<T, U> dictionary)
        {
            return dictionary == null || dictionary.Count < 1;
        }

        public static bool ArrayIsNullOrEmpty<T>(T[] array)
        {
            return array == null || array.Length == 0;
        }

        public static string ReplaceGermanAccents(string text)
        {
            var map = new Dictionary<char, string>
            {
                {'ä', "ae"},
                {'ö', "oe"},
                {'ü', "ue"},
                {'Ä', "Ae"},
                {'Ö', "Oe"},
                {'Ü', "Ue"},
                {'ß', "ss"}
            };
            return text.Aggregate(
                new StringBuilder(),
                (sb, c) =>
                {
                    string r;
                    return map.TryGetValue(c, out r) ? sb.Append(r) : sb.Append(c);
                }).ToString();
        }

        public static bool CheckFileMd5(string fileNameInclPath, string checksum)
        {
            if (!File.Exists(fileNameInclPath))
                return false;

            FileStream fileCheck = null;
            try
            {
                using (fileCheck = new FileStream(fileNameInclPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    MD5 md5 = new MD5CryptoServiceProvider();
                    byte[] md5Hash = md5.ComputeHash(fileCheck);
                    fileCheck.Close();

                    string berechnet = BitConverter.ToString(md5Hash).Replace("-", "").ToUpper();
                    return berechnet == checksum.ToUpper();
                }
            }
            finally
            {
                fileCheck?.Dispose();
            }
        }

        public static int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        public static string RandomWord(string[] words)
        {
            return words[RandomNumber(0, words.Length)];
        }

        public static void WaitNSeconds(int seconds)
        {
            if (seconds < 1) return;
            DateTime desired = DateTime.Now.AddSeconds(seconds);
            while (DateTime.Now < desired)
            {
                Application.DoEvents();
            }
        }

        //https://github.com/tinohager/Nager.ArticleNumber
        public static bool IsASINFormat(string asin_isbn)
        {
            return Regex.IsMatch(asin_isbn, "^B[0-9]{2}[0-9A-Z]{7}|[0-9]{9}(X|0-9])$", RegexOptions.IgnoreCase);
        }

        public static bool IsISBNFormat(string asin_isbn)
        {
            if (!asin_isbn.All(Char.IsDigit))
                return false;
            string clearedIn = asin_isbn.ToUpper().Replace("-", "").Replace(" ", "").Trim();
            int[] numbers = clearedIn.ToCharArray().Select<char, int>(i => i == 'X' ? 10 : Int32.Parse(i.ToString())).ToArray();
            int sum = 0;
            for (int i = 0; i < 10; i++)
            {
                sum += numbers[i] * (10 - i);
            }
            return sum % 11 == 0;
        }

        public static string Base64Decode(string encodedString)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encodedString));
        }

        public class AutoClosingMessageBox
        {
            Timer _timeoutTimer;
            string _caption;
            AutoClosingMessageBox(string text, string caption, int timeout)
            {
                _caption = caption;
                _timeoutTimer = new Timer(OnTimerElapsed,
                    null, timeout, Timeout.Infinite);
                MessageBox.Show(text, caption);
            }
            public static void Show(string text, string caption, int timeout)
            {
                new AutoClosingMessageBox(text, caption, timeout);
            }
            void OnTimerElapsed(object state)
            {
                IntPtr mbWnd = FindWindow(null, _caption);
                if (mbWnd != IntPtr.Zero)
                    SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                _timeoutTimer.Dispose();
            }
            const int WM_CLOSE = 0x0010;
            [DllImport("user32.dll", SetLastError = true)]
            static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        }
    }

    public class Rot13
    {
        private Dictionary<char, char> _rot13 = new Dictionary<char, char>();

        /// <summary>
        /// Initialise the dictionary on a per object basis. I guess it could be made static as well?!
        /// </summary>
        public Rot13()
        {
            string lowLower = "abcdefghijklm", highLower = "nopqrstuvwxyz";
            string lowUpper = "ABCDEFGHIJKLM", highUpper = "NOPQRSTUVWXYZ";

            for (int i = 0; i < lowUpper.Length; i++)
            {
                // Convert a => n and A => N.
                _rot13.Add(lowLower[i], highLower[i]);
                _rot13.Add(highLower[i], lowLower[i]);

                // Convert n => a and N => A.
                _rot13.Add(lowUpper[i], highUpper[i]);
                _rot13.Add(highUpper[i], lowUpper[i]);
            }
        }

        /// <summary>
        /// Decode a Rot13 string.
        /// </summary>
        /// <param name="data">A Rot13 encoded string.</param>
        /// <returns>The original string.</returns>
        public string Decode(string data)
        {
            return Encode(data);
        }

        /// <summary>
        /// Encode a string to using Rot13.
        /// </summary>
        /// <param name="data">A string to be encoded.</param>
        /// <returns>An encoded string.</returns>
        public string Encode(string data)
        {
            char rotated = new char();
            char[] array = data.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                if (_rot13.TryGetValue(array[i], out rotated))
                {
                    array[i] = rotated;
                }
            }
            return new string(array);
        }
    }

    internal class RegexUtilities
    {
        bool invalid = false;

        public bool IsValidEmail(string strIn)
        {
            invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", this.DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

            if (invalid)
                return false;

            // Return true if strIn is in valid e-mail format.
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }
    }
}

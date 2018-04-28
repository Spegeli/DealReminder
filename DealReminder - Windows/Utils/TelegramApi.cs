using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using DealReminder_Windows.GUI;
using DealReminder_Windows.Logging;
using MetroFramework;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace DealReminder_Windows.Utils
{
    internal class TelegramApi
    {
        public static async Task SendMessage(string chatid, string message)
        {
            try
            {
                var bot = new Telegram.Bot.TelegramBotClient("312420207:AAGnEn6CztWMs8ExE9L808M2ZXApqTS2yPA");
                await bot.SendTextMessageAsync(chatid, message);
            }
            catch (Exception ex)
            {
                Logger.Write("Telegram Nachricht Senden Fehlgeschlagen - Grund: " + ex.Message);
                if (OSystem.IsAvailableNetworkActive()) { }
            }
        }

        public static async Task SendFeedbackMessage(string subject, string message, string attachment = null)
        {
            Main mf = Application.OpenForms["Main"] as Main;
            if (mf == null) return;

            var bot = new Telegram.Bot.TelegramBotClient("397478316:AAFx_E18FZ5bxNdO-F3wQtbPcw7OlrPddfU");
            try
            {
                await bot.SendTextMessageAsync(251417296, subject + "\n\n" + message);
                if (!String.IsNullOrWhiteSpace(attachment))
                {
                    try
                    {
                        string file = attachment;
                        var fileName = file.Split('\\').Last();
                        using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            var fts = new FileToSend(fileName, fileStream);
                            if (System.Web.MimeMapping.GetMimeMapping(file).StartsWith("text/") || System.Web.MimeMapping.GetMimeMapping(file).StartsWith("application/"))
                                await bot.SendDocumentAsync(251417296, fts, subject);
                            if (System.Web.MimeMapping.GetMimeMapping(file).StartsWith("image/"))
                                await bot.SendPhotoAsync(251417296, fts, subject);
                            if (System.Web.MimeMapping.GetMimeMapping(file).StartsWith("video/"))
                                await bot.SendVideoAsync(251417296, fts);
                        }
                    }
                    catch (FileNotFoundException ex)
                    {
                        Logger.Write("Telegram Feedback Anhang Senden Fehlgeschlagen - Grund: " + ex.Message);
                        //Text Info Output
                    }
                }
                MetroMessageBox.Show(mf, "Dein Feedback wurde Erfolgreich verschickt ;-)", "Feedback Erfolgreich",
                    MessageBoxButtons.OK, MessageBoxIcon.Question);

                mf.metroComboBox5.SelectedIndex = -1;
                mf.metroTextBox2.Clear();
                mf.metroTextBox3.Clear();
                mf.metroTextBox4.Clear();
                mf.metroTextBox11.Clear();
                mf.metroTextBox12.Clear();
            }
            catch (Exception ex)
            {
                Logger.Write("Telegram Feedback Nachricht Senden Fehlgeschlagen - Grund: " + ex.Message);
                if (OSystem.IsAvailableNetworkActive()) { }
                MetroMessageBox.Show(mf,
                    "Leider gab es ein Problem und dein Feedback konnte nicht erfolgreich verschickt werden." +
                    Environment.NewLine + Environment.NewLine + "Bitte versuche es erneut oder Kontaktiere den Entwickler.",
                    "Feedback Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static async Task<int> GetChatIdViaUsername(string username)
        {
            try
            {
                var json =
                    await new BetterWebClient{ Timeout = 10000 }.DownloadStringTaskAsync(
                        new Uri(
                            "https://api.telegram.org/bot312420207:AAGnEn6CztWMs8ExE9L808M2ZXApqTS2yPA/getUpdates"));
                dynamic dynJson = JsonConvert.DeserializeObject(json);
                username = username.Replace("@", "");
                foreach (var result in dynJson["result"])
                {
                    if (result["message"] == null) continue;
                    string usernameGrabbed = result["message"]["chat"].username;
                    if (!String.Equals(usernameGrabbed, username, StringComparison.CurrentCultureIgnoreCase)) continue;
                    return result["message"]["chat"].id;
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Telegram Chat Id Abfragen Fehlgeschlagen - Grund: " + ex.Message);
            }
            return 0;
        }
    }
}

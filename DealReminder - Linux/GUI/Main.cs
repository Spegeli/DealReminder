using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DealReminder_Linux.Configs;
using DealReminder_Linux.Logging;

namespace DealReminder_Linux.GUI
{
    public partial class Main : Form
    {
        public static CancellationTokenSource CancelCrawler;
        TextWriter _writer = null;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            txtConsole.Text = File.ReadAllText(Path.Combine(FoldersFilesAndPaths.Logs, Logger.CurrentFile + ".txt"));
            _writer = new TextBoxStreamWriter(txtConsole);
            Console.SetOut(_writer);

            LoadChangelogs();
        }

        //--Main START
        private void linkLabel2_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Logger.Write("Open Browser with URL: http://sendvid.com/98j9fccc");
            Process.Start("http://sendvid.com/98j9fccc");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Logger.Write("Open Browser with URL: https://t.me/joinchat/AAAAAEK4xY7BkQ1QlLoNhQ");
            Process.Start("https://t.me/joinchat/AAAAAEK4xY7BkQ1QlLoNhQ");
        }

        private void LoadChangelogs()
        {
            try
            {
                Stream data = new WebClient().OpenRead("https://updates.speg-dev.de/GetHistoricalChangelog.php?filter=DealReminderLinux");
                if (data != null)
                {
                    StreamReader read = new StreamReader(data);
                    while (read.Peek() >= 0)
                        textBox1.AppendText(read.ReadLine()?.Replace("<br />", Environment.NewLine));
                }
            }
            catch
            {
                textBox1.AppendText("Fehler beim Laden des Changelogs..." + Environment.NewLine);
            }
            try
            {
                Stream data = new WebClient().OpenRead("https://updates.speg-dev.de/GetHistoricalChangelog.php?filter=DealReminderLinux&nextversion");
                if (data != null)
                {
                    StreamReader read = new StreamReader(data);
                    while (read.Peek() >= 0)
                        textBox2.AppendText(read.ReadLine()?.Replace("<br />", Environment.NewLine));
                }
            }
            catch
            {
                textBox2.AppendText("Fehler beim Laden des Changelogs der nächsten Version..." + Environment.NewLine);
            }
        }
        //--Main ENDE
    }
}

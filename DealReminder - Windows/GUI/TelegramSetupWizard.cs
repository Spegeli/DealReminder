using System;
using System.Drawing;
using System.Windows.Forms;
using DealReminder_Windows.Configs;
using DealReminder_Windows.Utils;
using MetroFramework.Forms;

namespace DealReminder_Windows.GUI
{
    public partial class TelegramSetupWizard : MetroForm
    {
        public TelegramSetupWizard()
        {
            InitializeComponent();
        }

        private static int _chatId;

        private void linkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            WebUtils.OpenBrowser("https://t.me/DealReminderBot");
        }

        private async void metroButton8_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(metroTextBox10.Text)) return;
            metroButton8.Enabled = false;
            metroButton10.Enabled = false;
            _chatId = 0;
            label4.Text = null;
            _chatId = await TelegramApi.GetChatIdViaUsername(metroTextBox10.Text);
            if (_chatId != 0)
            {
                label4.Text = @"Deine Chat Id lautet " + _chatId + @".";
                label4.ForeColor = Color.Green;
                metroButton10.Enabled = true;
            }
            else
            {
                label4.Text = @"Deine Chat Id konnte nicht gefunden werden. Bitte wiederhole den vorgang oder Kontakte den Entwickler.";
                label4.ForeColor = Color.Red;
            }
            metroButton8.Enabled = true;
        }

        private async void metroButton10_Click(object sender, EventArgs e)
        {
            if (_chatId == 0) return;
            metroButton10.Enabled = false;
            await TelegramApi.SendMessage(_chatId.ToString(),"Wenn du diese Nachricht erhälst hast du alles richtig Konfiguriert ;-)");
            metroButton10.Enabled = true;
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            if (_chatId != 0 && Settings.Get<int>("ReminderTelegram") != _chatId)
            {
                Main mf = Application.OpenForms["Main"] as Main;
                mf.metroTextBox9.Text = Convert.ToString(_chatId);
            }
            this.Close();
        }
    }
}

using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Windows.Forms;
using DealReminder_Windows.GUI;
using DealReminder_Windows.Logging;
using MailMessage = System.Net.Mail.MailMessage;
using MailPriority = System.Net.Mail.MailPriority;

namespace DealReminder_Windows.Utils
{
    internal class Mail
    {
        public static Main mf = Application.OpenForms["Main"] as Main;

        /// <summary>
        /// Send an email from [DELETED]
        /// </summary>
        /// <param name="recipient">Message to address</param>
        /// <param name="body">Text of message to send</param>
        /// <param name="subject">Subject line of message</param>
        public static async Task NotificationSend(string recipient,
            string body,
            string subject)
        {
            try
            {
                // Setup mail message
                MailMessage msg = new MailMessage(new MailAddress("no-reply@dealreminder.de", "DealReminder App"), new MailAddress(recipient))
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false,
                    Priority = MailPriority.Normal
                };

                // Ignore Certificate validation failures (aka untrusted certificate + certificate chains)
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                // Setup SMTP client and send message
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    smtpClient.Host = "mail.dealreminder.de";
                    smtpClient.EnableSsl = true;
                    //smtpClient.Port = 465;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential("no-reply@dealreminder.de", "_+[G2,[;Qq.T");
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    await smtpClient.SendMailAsync(msg);
                }
            }
            catch (SmtpFailedRecipientsException sfrEx)
            {
                // TODO: Handle exception
                // When email could not be delivered to all receipients.
                Logger.Write("Email Erinnerung Senden Fehlgeschlagen - Grund: " + sfrEx.Message);
            }
            catch (SmtpException sEx)
            {
                // TODO: Handle exception
                // When SMTP Client cannot complete Send operation.
                Logger.Write("Email Erinnerung Senden Fehlgeschlagen - Grund: " + sEx.Message);
            }
            catch (Exception ex)
            {
                // TODO: Handle exception
                // Any exception that may occur during the send process.
                Logger.Write("Email Erinnerung Senden Fehlgeschlagen - Grund: " + ex.Message);
            }
        }
    }

    internal class MailAttachment
    {
        #region Fields
        private MemoryStream stream;
        private string filename;
        private string mediaType;
        #endregion
        #region Properties
        /// <summary>
        /// Gets the data stream for this attachment
        /// </summary>
        public Stream Data { get { return stream; } }
        /// <summary>
        /// Gets the original filename for this attachment
        /// </summary>
        public string Filename { get { return filename; } }
        /// <summary>
        /// Gets the attachment type: Bytes or String
        /// </summary>
        public string MediaType { get { return mediaType; } }
        /// <summary>
        /// Gets the file for this attachment (as a new attachment)
        /// </summary>
        public Attachment File { get { return new Attachment(Data, Filename, MediaType); } }
        #endregion
        #region Constructors
        /// <summary>
        /// Construct a mail attachment form a byte array
        /// </summary>
        /// <param name="data">Bytes to attach as a file</param>
        /// <param name="filename">Logical filename for attachment</param>
        public MailAttachment(byte[] data, string filename)
        {
            this.stream = new MemoryStream(data);
            this.filename = filename;
            this.mediaType = MediaTypeNames.Application.Octet;
        }
        /// <summary>
        /// Construct a mail attachment from a string
        /// </summary>
        /// <param name="data">String to attach as a file</param>
        /// <param name="filename">Logical filename for attachment</param>
        public MailAttachment(string data, string filename)
        {
            this.stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(data));
            this.filename = filename;
            this.mediaType = MediaTypeNames.Text.Html;
        }
        #endregion
    }
}

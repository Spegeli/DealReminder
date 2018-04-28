using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DealReminder_Windows.Logging
{
    internal class TextBoxStreamWriter : TextWriter
    {
        private readonly TextBox _output = null;

        public TextBoxStreamWriter(TextBox output)
        {
            _output = output;
        }

        public override void Write(char value)
        {
            MethodInvoker action = delegate { _output.AppendText(value.ToString()); };
            _output.BeginInvoke(action);
        }

        public override Encoding Encoding => Encoding.UTF8;
    }
}

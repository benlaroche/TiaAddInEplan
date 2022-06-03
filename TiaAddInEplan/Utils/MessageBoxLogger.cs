using System.Windows.Forms;

namespace TiaAddInEplan
{
    public class MessageBoxLogger : ILogger
    {
        public void Log(string message)
        {
            MessageBox.Show(message);
        }
    }
}

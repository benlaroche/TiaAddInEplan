using System.IO;

namespace TiaAddInEplan
{
    public class FileLogger : ILogger
    {
        private readonly string path;
        public FileLogger()
        {
            path = @"C:\Users\blaroche\source\repos\TiaAddInEplan\TiaAddInEplan\Log.text";
        }
        public FileLogger(string path)
        {
            this.path = path;
        }


        public void Log(string message)
        {
            using (var streamWriter = new StreamWriter(path, true))
            {
                streamWriter.WriteLine(message);
            }
        }
    }
}
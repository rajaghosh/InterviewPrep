using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace SingletonLogger.Logger
{
    public sealed class Log:ILog
    {
        private Log()
        {
        }

        private readonly static Lazy<Log> _log = new Lazy<Log>(() => new Log());

        public static Log CustomExceptionLogger
        {
            get { return _log.Value; }
        }

        public void LogException(string message)
        {
            string fileName = string.Format(@"{0}_{1}.log", "Exception", DateTime.Now.ToShortDateString()).Replace(@"/","_");
            string logFilePath = string.Format(@"{0}{1}", AppDomain.CurrentDomain.BaseDirectory, fileName);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("----------------------------------------");
            sb.AppendLine(DateTime.Now.ToString());
            sb.AppendLine(message);
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.Write(sb.ToString());
                writer.Flush();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Configuration;

namespace cat.utils
{
    public class Logger
    {
        private static String LOG_FOLDER = System.Configuration.ConfigurationSettings.AppSettings["Log"];

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Log(String logFile, String msg)
        {
            var logPath = Path.Combine(LOG_FOLDER, logFile);
            StreamWriter sw = new StreamWriter(logPath, true);
            sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm") + ": " + msg);
            sw.Close();
        }

        public void Warn(String msg)
        { 
        }
    }
}
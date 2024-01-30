/*
 *  Written by David Barrett, Microsoft Ltd.  Use at your own risk.  No warranties are given. 
 *  
 *  DISCLAIMER:
 * THIS CODE IS SAMPLE CODE. THESE SAMPLES ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND.
 * MICROSOFT FURTHER DISCLAIMS ALL IMPLIED WARRANTIES INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OF MERCHANTABILITY OR OF FITNESS FOR
 * A PARTICULAR PURPOSE. THE ENTIRE RISK ARISING OUT OF THE USE OR PERFORMANCE OF THE SAMPLES REMAINS WITH YOU. IN NO EVENT SHALL
 * MICROSOFT OR ITS SUPPLIERS BE LIABLE FOR ANY DAMAGES WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
 * BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR OTHER PECUNIARY LOSS) ARISING OUT OF THE USE OF OR INABILITY TO USE THE
 * SAMPLES, EVEN IF MICROSOFT HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES. BECAUSE SOME STATES DO NOT ALLOW THE EXCLUSION OR LIMITATION
 * OF LIABILITY FOR CONSEQUENTIAL OR INCIDENTAL DAMAGES, THE ABOVE LIMITATION MAY NOT APPLY TO YOU.
 * */

using System;
using System.IO;

namespace Logging
{
    public class Logger
    {
        bool bLogToConsole = false;
        String sLogToFile = "";     
        StreamWriter oLogFile = null;
        private readonly object _lockObject = new object();
        public delegate void LogAddedEventHandler
            (String NotificationEvent);            
        public event LogAddedEventHandler LogAdded;             

        public Logger()
        {
            bLogToConsole = false;
            sLogToFile = "";
        }

        public Logger(bool LogToConsole, String LogToFile)
        {
            bLogToConsole = LogToConsole;
            sLogToFile = LogToFile;
            if (sLogToFile != "")
                oLogFile = File.AppendText(LogToFile);
        }

        ~Logger()
        {
            this.Close();
        }

        public string LogFile
        {
            get { return sLogToFile; }
            set
            {
                try
                {
                    oLogFile.Close();
                }
                catch { }
                oLogFile = null;
                sLogToFile = value;
                if (sLogToFile != "")
                    oLogFile = File.AppendText(sLogToFile);
            }
        }

        public LogAddedEventHandler LogAddedDelegate
        {
            set { LogAdded = value; }
        }

        public void Log(String LogText, bool SectionBreakBefore = false, bool DoNotLogToFile = false)
        {
            // Add a log entry

            DateTime now = DateTime.Now;
            string logEntry = $"{now:d} {now:T}   " + LogText;

            try
            {
                if (bLogToConsole)
                {
                    if (SectionBreakBefore)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("");
                    }
                    Console.WriteLine(logEntry);
                }
            }
            catch
            {
            }

            try
            {
                if (!(LogAdded == null)) LogAdded(logEntry);
            }
            catch { }

            if (DoNotLogToFile) return;

            lock (_lockObject)
                try
                {
                    if (oLogFile != null)
                    {
                        if (SectionBreakBefore)
                        {
                            oLogFile.WriteLine();
                            oLogFile.WriteLine(new String('-', 80));
                            oLogFile.WriteLine();
                        }
                        oLogFile.WriteLine(logEntry);
                        oLogFile.Flush();
                    }
                }
                catch
                {
                }
        }

        public void Close()
        {
            try
            {
                oLogFile?.Flush();
                oLogFile?.Close();
            }
            catch
            {
                // We do not care about any errors here
            }
        }

    }
}

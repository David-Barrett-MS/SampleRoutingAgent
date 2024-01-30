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

using System.Text;
using System.IO;
using Microsoft.Exchange.Data.Transport;
using Microsoft.Exchange.Data.Transport.Routing;
using System.Text.RegularExpressions;


// Install-TransportAgent -Name "SampleRoutingTransportAgent" -TransportAgentFactory SampleRoutingAgent.SampleRoutingTransportAgentFactory -AssemblyPath "C:\TA\SampleRoutingAgent\SampleRoutingAgent.dll"


namespace SampleRoutingAgent
{
    public class SampleRoutingTransportAgent: RoutingAgent
    {
        private Logging.Logger _logger = null;

        public SampleRoutingTransportAgent()
        {
            OnSubmittedMessage += SampleRoutingTransportAgent_OnSubmittedMessage;
            OnResolvedMessage += SampleRoutingTransportAgent_OnResolvedMessage;
            OnRoutedMessage += SampleRoutingTransportAgent_OnRoutedMessage;
            OnCategorizedMessage += SampleRoutingTransportAgent_OnCategorizedMessage;
        }

        public SampleRoutingTransportAgent(Logging.Logger logger): this()
        {
            _logger = logger;
        }

        private void Log(string message)
        {
            if (_logger != null)
                _logger.Log(message);
        }

        private void SampleRoutingTransportAgent_OnResolvedMessage(ResolvedMessageEventSource source, QueuedMessageEventArgs e)
        {
            Log($"OnResolvedMessage: {e.MailItem.EnvelopeId}");
            DumpMessage(e.MailItem, "OnResolvedMessage");
        }

        private void SampleRoutingTransportAgent_OnSubmittedMessage(SubmittedMessageEventSource source, QueuedMessageEventArgs e)
        {
            Log($"OnSubmittedMessage: {e.MailItem.EnvelopeId}");
            DumpMessage(e.MailItem, "OnSubmittedMessage");
        }

        private void SampleRoutingTransportAgent_OnRoutedMessage(RoutedMessageEventSource source, QueuedMessageEventArgs e)
        {
            Log($"OnRoutedMessage: {e.MailItem.EnvelopeId}");
            DumpMessage(e.MailItem, "OnRoutedMessage");
        }

        private void SampleRoutingTransportAgent_OnCategorizedMessage(CategorizedMessageEventSource source, QueuedMessageEventArgs e)
        {
            // This is the only event that occurs after message conversion
            // (i.e. the only chance that this will be MIME without TNEF)
            Log($"OnCategorizedMessage: {e.MailItem.EnvelopeId}");
            DumpMessage(e.MailItem, "OnCategorizedMessage");
        }

        /// <summary>
        /// Dump the MIME content of the message to a file
        /// </summary>
        /// <param name="mailItem">The MailItem to write to file</param>
        /// <param name="fileSuffix">Suffix to be appended to filename (before .eml)</param>
        private void DumpMessage(MailItem mailItem, string fileSuffix)
        {
            if (mailItem.Message.MimeDocument != null)
            {
                try
                {
                    string exportTo = $"c:\\Temp\\TA\\Messages\\{SampleRoutingTransportAgent.RemoveIllegalFileNameChars(mailItem.Message.MessageId)}.{fileSuffix}.eml";
                    using (StreamReader mimeReader = new StreamReader(mailItem.GetMimeReadStream()))
                    {
                        using (var writeFileStream = new FileStream(exportTo, FileMode.Create, FileAccess.Write))
                        {
                            using (var streamWriter = new StreamWriter(writeFileStream, Encoding.UTF8))
                            {
                                streamWriter.Write(mimeReader.ReadToEnd());
                            }
                        }
                    }
                    Log($"MIME written to: {exportTo}");
                    return;
                }
                catch { }
            }
        }

        public static string RemoveIllegalFileNameChars(string input, string replacement = "")
        {
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex($"[{Regex.Escape(regexSearch)}]");
            return r.Replace(input, replacement);
        }
    }
}

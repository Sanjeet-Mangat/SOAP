

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HL7parser
{
    public static class Log
    {

        /// <summary>
        /// Will output a consumer header to a log file
        /// </summary>
        /// <param name="teamName">Team name that will be used in the header</param>
        public static void ConsumerRunTimeLogHeader (string teamName)
        {
            string logEvent = string.Empty;
            string logFile = "The Consumer Run-Time Log." + DateTime.Now.ToString ("yyyy-MM-dd") + ".log";
            logEvent = DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + " ======================================================= ";
            logEvent += Environment.NewLine;
            logEvent += DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + "                  -- USER APP LOG --                     ";
            logEvent += Environment.NewLine;
            logEvent += DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + "Team   : " + teamName + "(Constantine Grigoriadis, Sunny Mangat, Dylan Sawchuk, Nick Whitney )";
            logEvent += Environment.NewLine;
            logEvent += DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + " ======================================================= ";

            using (StreamWriter writeToLogFile = OpenForAppend (logFile))
            {
                writeToLogFile.WriteLine (logEvent);
                CloseFile (writeToLogFile);
            }
        }

        /// <summary>
        /// Will output a a SOA registry message call to a log file
        /// </summary>
        /// <param name="msg">The contents of the msg that will be printed</param>
        public static void SOARegistryMessge (string msg, bool isServer = false)
        {
            int count = 0;
            List<string> msgSegments = new List<string> ();
            string logEvent = string.Empty;
            string logFile = "The " + ( isServer ? "Service" : "Consumer") + " Run-Time Log." + DateTime.Now.ToString ("yyyy-MM-dd") + ".log";

            logEvent = DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + " ---";
            logEvent += Environment.NewLine;
            logEvent += DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + " Calling SOA-Registry with message :";
            logEvent += Environment.NewLine;
            msgSegments = ParseLogSegments (msg);

            for (count = 0; count < msgSegments.Count - 2; count++)
            {
                logEvent += DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + "    >> " + msgSegments[count];
                logEvent += Environment.NewLine;
            }

            using (StreamWriter writeToLogFile = OpenForAppend (logFile))
            {
                writeToLogFile.WriteLine (logEvent);
                CloseFile (writeToLogFile);
            }
        }


        /// <summary>
        /// Will output a message recieved from the SOA Registry
        /// </summary>
        /// <param name="msg">The contents of the msg that will be printed</param>
        public static void SOAResponseRegistry (string msg, bool isServer = false)
        {
            int count = 0;
            List<string> msgSegments = new List<string> ();
            string logEvent = string.Empty;
            string logFile = "The " + (isServer ? "Service" : "Consumer") + " Run-Time Log." + DateTime.Now.ToString ("yyyy-MM-dd") + ".log";

            logEvent += DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + " Response from SOA Registry :";
            logEvent += Environment.NewLine;
            msgSegments = ParseLogSegments (msg);

            for (count = 0; count < msgSegments.Count - 2; count++)
            {
                logEvent += DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + "        >> " + msgSegments[count];
                logEvent += Environment.NewLine;
            }

            using (StreamWriter writeToLogFile = OpenForAppend (logFile))
            {
                writeToLogFile.WriteLine (logEvent);
                CloseFile (writeToLogFile);
            }
        }


        /// <summary>
        /// Will output a message being sent for request
        /// </summary>
        /// <param name="ip">IP address consumer is talking to</param>
        /// <param name="portNumber">Port of the service</param>
        /// <param name="msg">The contents of the msg that will be printed</param>
        public static void ConsumerSendingServiceRequest (string ip, string portNumber, string msg)
        {
            int count = 0;
            List<string> msgSegments = new List<string> ();
            string logEvent = string.Empty;
            string logFile = "The Consumer Run-Time Log." + DateTime.Now.ToString ("yyyy-MM-dd") + ".log";

            logEvent = DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + " ---";
            logEvent += Environment.NewLine;
            logEvent += DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + " Sending service request to IP" + ip + ", PORT " + portNumber + " :";
            logEvent += Environment.NewLine;
            msgSegments = ParseLogSegments (msg);

            for (count = 0; count < msgSegments.Count - 2; count++)
            {
                logEvent += DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + "  >> " + msgSegments[count];
                logEvent += Environment.NewLine;
            }

            using (StreamWriter writeToLogFile = OpenForAppend (logFile))
            {
                writeToLogFile.WriteLine (logEvent);
                CloseFile (writeToLogFile);
            }
        }

        /// <summary>
        /// Will output a service header to a log file
        /// </summary>
        /// <param name="teamName">Team name that will be used in the header</param>
        /// <param name="tagName">Tag name that will be used in the header</param>
        /// <param name="serviceName">Service name that will be used in the header</param>
        public static void ServiceRunTimeLogHeader (string teamName, string tagName, string serviceName)
        {
            string logEvent = string.Empty;
            string logFile = "The Service Run-Time Log." + DateTime.Now.ToString ("yyyy-MM-dd") + ".log";
            logEvent = DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + " ======================================================= ";
            logEvent += Environment.NewLine;
            logEvent += DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + "Team        : " + teamName + "(Constantine Grigoriadis, Sunny Mangat, Dylan Sawchuk, Nick Whitney   ";
            logEvent += Environment.NewLine;
            logEvent += DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + "Tag-Name    : " + tagName;
            logEvent += Environment.NewLine;
            logEvent += DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + "Service     :" + serviceName;
            logEvent += Environment.NewLine;
            logEvent += DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + " ======================================================= ";

            using (StreamWriter writeToLogFile = OpenForAppend (logFile))
            {
                writeToLogFile.WriteLine (logEvent);
                CloseFile (writeToLogFile);
            }
        }

        /// <summary>
        /// Will output a message that the service receieves
        /// </summary>
        /// <param name="msg">The contents of the msg that will be printed</param>
        public static void ServiceReceivingServiceRequest (string msg)
        {
            int count = 0;
            List<string> msgSegments = new List<string> ();
            string logEvent = string.Empty;
            string logFile = "The Service Run-Time Log." + DateTime.Now.ToString ("yyyy-MM-dd") + ".log";

            logEvent = DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + " ---";
            logEvent += Environment.NewLine;
            logEvent += DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + " Receiving service request :";
            logEvent += Environment.NewLine;
            msgSegments = ParseLogSegments (msg);

            for (count = 0; count < msgSegments.Count - 2; count++)
            {
                logEvent += DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + "  >> " + msgSegments[count];
                logEvent += Environment.NewLine;
            }

            using (StreamWriter writeToLogFile = OpenForAppend (logFile))
            {
                writeToLogFile.WriteLine (logEvent);
                CloseFile (writeToLogFile);
            }
        }

        public static void responseFromService (string msg)
        {
            int count = 0;
            List<string> msgSegments = new List<string> ();
            string logEvent = string.Empty;
            string logFile = "The Consumer Run-Time Log." + DateTime.Now.ToString ("yyyy-MM-dd") + ".log";

            logEvent = DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + " ---";
            logEvent += Environment.NewLine;
            logEvent += DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + " Response from service request :";
            logEvent += Environment.NewLine;
            msgSegments = ParseLogSegments (msg);

            for (count = 0; count < msgSegments.Count - 2; count++)
            {
                logEvent += DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + "    >> " + msgSegments[count];
                logEvent += Environment.NewLine;
            }

            using (StreamWriter writeToLogFile = OpenForAppend (logFile))
            {
                writeToLogFile.WriteLine (logEvent);
                CloseFile (writeToLogFile);
            }
        }

        /// <summary>
        /// Will output a message responding to service request
        /// </summary>
        /// <param name="msg">The contents of the msg that will be printed</param>
        public static void ServiceRespondingToServiceRequest (string msg)
        {
            int count = 0;
            List<string> msgSegments = new List<string> ();
            string logEvent = string.Empty;
            string logFile = "The Service Run-Time Log." + DateTime.Now.ToString ("yyyy-MM-dd") + ".log";

            logEvent = DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + " ---";
            logEvent += Environment.NewLine;
            logEvent += DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + " Responding to service request :";
            logEvent += Environment.NewLine;
            msgSegments = ParseLogSegments (msg);

            for (count = 0; count < msgSegments.Count - 2; count++)
            {
                logEvent += DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss") + "  >> " + msgSegments[count];
                logEvent += Environment.NewLine;
            }

            using (StreamWriter writeToLogFile = OpenForAppend (logFile))
            {
                writeToLogFile.WriteLine (logEvent);
                CloseFile (writeToLogFile);
            }
        }

        /// <summary>
        /// Parse the msg to be printed to the log file correctly
        /// </summary>
        /// <param name="response">The message that will be parsed</param>
        /// <returns></returns>
        private static List<String> ParseLogSegments (String response)
        {

            List<String> data = new List<String> ();
            String[] myData = response.Split ('\r');

            foreach (String seg in myData)
            {
                data.Add (seg);
            }

            return data;
        }

        /// <summary>
        /// Open a log file for appedning
        /// </summary>
        /// <param name="fileName">Name of the log file</param>
        /// <returns></returns>
        private static StreamWriter OpenForAppend (string fileName)
        {
            StreamWriter w = File.AppendText (fileName);
            return w;
        }

        /// <summary>
        /// Close the log file
        /// </summary>
        /// <param name="w">streamwriter used to write to log file</param>
        private static void CloseFile (StreamWriter w)
        {
            w.Close ();
        }

    }
}

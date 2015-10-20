/*
* FILE : Registry.cs
* PROJECT : PROG3080 - Assignment #1
* PROGRAMMER : Constantine Grigoriadis, Sunny Mangat, Dylan Sawchuk, Nick Whitey
* FIRST VERSION : 2014-11-28
* DESCRIPTION : Class to send messages to the registry given an IP and Port
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using HL7parser;

namespace GIROP_Purchase_Totaller
{
    /// <summary>
    /// Contacts the registry at a given IP and port with a given message
    /// </summary>
    public class Registry
    {
        // send things to the registry
        public string RegistryIP { get; set; }
        public int RegistryPort { get; set; }

        /// <summary>
        /// Registry Constructor
        /// </summary>
        /// <param name="ip">IP of Registry</param>
        /// <param name="port">Port of Registry</param>
        public Registry (string ip, int port)
        {
            RegistryIP = ip;
            RegistryPort = port;
        }

        /// <summary>
        /// Sends a message to the registry
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>Result of message</returns>
        public string SendMessage (string message)
        {
            string returnHL7 = "";
            // Data buffer for incoming data.
            byte[] bytes = new byte[1024];
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // This example uses port 11000 on the local computer.
                IPHostEntry ipHostInfo = Dns.Resolve (RegistryIP);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint (ipAddress, RegistryPort);

                // Create a TCP/IP  socket.
                Socket sender = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Connect the socket to the remote endpoint. Catch any errors.

                sender.Connect (remoteEP);
                // Encode the data string into a byte array.
                byte[] msg = Encoding.ASCII.GetBytes (message);
                // Send the data through the socket.
                int bytesSent = sender.Send (msg);
                // Receive the response from the remote device.
                int bytesRec = sender.Receive (bytes);
                returnHL7 = Encoding.ASCII.GetString (bytes, 0, bytesRec);
                // Release the socket.
                sender.Shutdown (SocketShutdown.Both);
                sender.Close ();
            }
            catch (Exception e)
            {
                throw e;
            }

            return returnHL7;
        }
    }
}

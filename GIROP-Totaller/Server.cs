/*
* FILE : Server.cs
* PROJECT : PROG3080 - Assignment #1
* PROGRAMMER : Constantine Grigoriadis, Sunny Mangat, Dylan Sawchuk, Nick Whitey
* FIRST VERSION : 2014-11-28
* DESCRIPTION : Creates a server to listen and execute HL7 commands to total a purchase order
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HL7parser;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace GIROP_Purchase_Totaller
{
    /// <summary>
    /// Main Server loop for the service
    /// </summary>
    public class Server
    {
        public bool Running { get; set; }
        int MaxMessageSize = 2048;
        FunctionCall MyFunction;
        int permissionLevel;
        //Server data members
        string myIP;
        int myPort;
        Registry myRegistry;

        DateTime myExpiry;

        //constructor
        public Server (FunctionCall information, string registryIP, int registryPort, int permissionLevel)
        {
            MyFunction = information;
            myIP = information.IP;
            myPort = information.port;
            this.permissionLevel = permissionLevel;


            myRegistry = new Registry (registryIP, registryPort);

            Log.ServiceRunTimeLogHeader (MyFunction.teamName, MyFunction.tagName, MyFunction.serviceName);
        }


        /// <summary>
        /// Main listener thread. Creates threads to handle clients
        /// </summary>
        public void Listener ()
        {
            Running = true;

            bool valid = true;
            // set up team in registry
            {
                try
                {
                    string request = HealthLevel7.RegisterTeamMessage (MyFunction.teamName);
                    string HL7 = myRegistry.SendMessage (request);

                    Log.SOARegistryMessge (request, true);
                    Log.SOAResponseRegistry (HL7, true);

                    SoaResponse result = (SoaResponse)HealthLevel7.parse (HL7, true);
                    if (result.OK)
                    {
                        MyFunction.teamID = result.teamID;
                        myExpiry = result.expiration;

                        request = HealthLevel7.PublishServiceMessage (MyFunction, permissionLevel);
                        HL7 = myRegistry.SendMessage (request);

                        Log.SOARegistryMessge (request, true);
                        Log.SOAResponseRegistry (HL7, true);

                        SoaResponse functResult = (SoaResponse)HealthLevel7.parse (HL7, true);
                        if (!functResult.OK)
                        {
                            throw new Exception ();
                        }
                    }
                    else
                    {
                        valid = false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine ("Failed to register with SOA registry");
                    Console.WriteLine ("ERROR: " + e.Message);
                    Console.ReadKey ();
                    valid = false;
                }
            }

            if (valid)
            {
                // Get host information
                IPHostEntry ipHostInfo = Dns.Resolve (myIP);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint hostEndPoint = new IPEndPoint (ipAddress, myPort);

                // Reference to Client
                Socket client = null;

                // Instantiate Socket
                Socket listener = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    // Bind the socket to the endpoint and start listening
                    listener.Bind (hostEndPoint);
                    //Place a socket in a listening state and specify how many client sockets could connect to it
                    listener.Listen (100);

                    while (Running)
                    {
                        Console.WriteLine ("Server : waiting for a connection on : " + myIP.ToString () + ":" + myPort.ToString ());

                        //Blocked until a connection 
                        client = listener.Accept ();

                        //starts a thread for the new connection
                        Thread messageThread = new Thread (ReadClientInfo);
                        messageThread.Start (client);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine ("Server Socket Error: " + ex.Message + Environment.NewLine);
                }
                finally
                {
                    try
                    {
                        string request = HealthLevel7.UnRegisterTeamMessage (MyFunction.teamName, MyFunction.teamID);
                        Log.SOARegistryMessge (request, true);
                        Log.SOAResponseRegistry (myRegistry.SendMessage (request), true);

                        if (client.Connected)
                        {
                            client.Shutdown (SocketShutdown.Send);
                        }

                        if (client != null)
                        {
                            client.Close ();
                        }
                        if (listener.Connected)
                        {
                            listener.Shutdown (SocketShutdown.Receive);
                        }
                        if (listener != null)
                        {
                            listener.Close ();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine ("Error occured while attempting to unregister team");
                        Console.WriteLine ("EROR: " + e.Message);
                    }
                }
            }
        }//public void Listener()


        /// <summary>
        /// Thread to service a cilent
        /// </summary>
        /// <param name="clientObj">(object) socket to client</param>
        public void ReadClientInfo (object clientObj)
        {
            Socket clientSocket = (Socket)clientObj;
            // attempts to get data from the user
            try
            {
                if (clientSocket.Connected)
                {
                    Byte[] byteMessage = new Byte[MaxMessageSize];
                    //the server receives the HTTP request from the browser 
                    int i = clientSocket.Receive (byteMessage, byteMessage.Length, 0);
                    string HL7Message = Encoding.ASCII.GetString (byteMessage, 0, i);
                    HandleMessage (HL7Message, clientSocket);
                    clientSocket.Shutdown (SocketShutdown.Both);
                    clientSocket.Close ();
                }
            }
            catch (Exception e) // exception while trying to get data from socket
            {
                Console.WriteLine ("Error while recieving data from client");
                Console.WriteLine ("ERROR: " + e.Message);
            }
        }// public void ReadClientInfo(object clientObj)


        /// <summary>
        /// Handles a message given the data and a client to respond to
        /// </summary>
        /// <param name="HL7Message">Message to Handle</param>
        /// <param name="client">Client to respond to</param>
        private void HandleMessage (string HL7Message, Socket client)
        {
            bool valid = true;
            string returnHL7 = "";
            FunctionCall theirData = null;

            Log.ServiceReceivingServiceRequest (HL7Message);

            string region = "ON"; // set from user input
            double subTotal = 100.00; // set from user input

            // attempts to parse user input
            try
            {
                theirData = (FunctionCall)HealthLevel7.parse (HL7Message, false);
            }
            catch (ThortonsSOAException e) //parsing checkHL7;
            {
                returnHL7 = HealthLevel7.ExecuteServiceResponse (e.errorCode, e.Message);
                valid = false;
            }
            catch (Exception e)
            {
                returnHL7 = HealthLevel7.ExecuteServiceResponse (-6, "Unknown error: " + e.Message);
            }

            // check if they are allowed to use this service
            if (valid) // if passes parsing
            {
                try
                {
                    string request = HealthLevel7.QueryTeamMessage (MyFunction.teamName, MyFunction.teamID, theirData.teamName, theirData.teamID, MyFunction.tagName);
                    string checkHL7 = myRegistry.SendMessage (request);
                    SoaResponse result = (SoaResponse)HealthLevel7.parse (checkHL7, true);

                    Log.SOARegistryMessge (request, true);
                    Log.SOAResponseRegistry (checkHL7, true);

                    if (!result.OK)
                    {
                        returnHL7 = HealthLevel7.ExecuteServiceResponse (result.ErrorCode, result.ErrorMessage);
                        valid = false;
                    }
                }
                catch (ThortonsSOAException e) //parsing checkHL7;
                {
                    returnHL7 = HealthLevel7.ExecuteServiceResponse (e.errorCode, e.Message);
                    valid = false;
                }
                catch (Exception e)
                {
                    returnHL7 = HealthLevel7.ExecuteServiceResponse (-6, "Unknown error: " + e.Message);
                }
            }

            // execute service
            if (valid) // if they have permission
            {
                foreach (Parameter param in theirData.parameters)
                {
                    if (param.name == "region")
                    {
                        region = (string)param.value;
                    }
                    if (param.name == "subTotal")
                    {
                        subTotal = (double)param.value;
                    }
                }

                try
                {
                    PurchaseReciept reciept = new PurchaseReciept (region, subTotal);
                    reciept.CalculateTotalPurchase ();

                    theirData.responses = new List<Response> ();
                    theirData.responses.Add (GenerateResp (1, "sub", typeof (double), reciept.SubTotal));
                    theirData.responses.Add (GenerateResp (2, "pst", typeof (double), reciept.cal_PST));
                    theirData.responses.Add (GenerateResp (3, "hst", typeof (double), reciept.cal_HST));
                    theirData.responses.Add (GenerateResp (4, "gst", typeof (double), reciept.cal_GST));
                    theirData.responses.Add (GenerateResp (5, "total", typeof (double), reciept.GrandTotal));
                    theirData.numResponses = 5;

                    returnHL7 = HealthLevel7.ExecuteServiceResponse (theirData);
                }
                catch (ThortonsSOAException e)
                {
                    returnHL7 = HealthLevel7.ExecuteServiceResponse (e.errorCode, e.Message);
                }
                catch (Exception e)
                {
                    returnHL7 = HealthLevel7.ExecuteServiceResponse (-6, "Unknown error: " + e.Message);
                }

            }

            Log.ServiceRespondingToServiceRequest (returnHL7);

            // attempts to send resulting info
            try
            {
                Respond (returnHL7, client);
            }
            catch (Exception e) // exception sending to client
            {
                Console.WriteLine ("an error occured while sending a message to the client");
                Console.WriteLine ("ERROR: " + e.Message);
            }
        }


        /// <summary>
        /// Writes HL7 to the client
        /// </summary>
        /// <param name="HL7Message">Message to send to client</param>
        /// <param name="client">Socket to send to</param>
        private void Respond (string HL7Message, Socket client)
        {
            using (StreamWriter writer = new StreamWriter (new NetworkStream (client)))
            {
                writer.Write (HL7Message);
                writer.Flush ();
            }
        }


        /// <summary>
        /// Creates a Response given the input
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="name">Name of Response</param>
        /// <param name="dataType">Data type of Response</param>
        /// <param name="value">Value of Response</param>
        /// <returns></returns>
        private static Response GenerateResp (int pos, string name, Type dataType, object value)
        {
            Response resp = new Response ();
            resp.position = pos;
            resp.name = name;
            resp.dataType = dataType;
            resp.value = value;

            return resp;
        }
    }//class Server
}//namespace GIROP_Purchase_Totaller

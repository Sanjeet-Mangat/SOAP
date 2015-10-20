/*
* FILE : Program.cs
* PROJECT : PROG3080 - Assignment #1
* PROGRAMMER : Constantine Grigoriadis, Sunny Mangat, Dylan Sawchuk, Nick Whitey
* FIRST VERSION : 2014-11-28
* DESCRIPTION : Creates a server to listen and execute HL7 commands to total a purchase order
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HL7parser;
using System.Threading;

namespace GIROP_Purchase_Totaller
{
    class Program
    {
        /// <summary>
        /// Loads config file, starts server
        /// </summary>
        /// <param name="args"></param>
        static void Main (string[] args)
        {
            string teamName = "";
            string tagName = "GIORP-TOTAL";
            string serviceName = "";
            string ip = "127.0.0.1";
            int port = 0;
            int permissionLevel = 0;
            string description = "Given a customers total as well as the region in Canada this service determines the sub total pst hst gst and total";

            bool isConfig = true;

            string registryIP = "";
            int registryPort = 0;

            try
            {
                ConfigFile config = new ConfigFile ("serviceConfig.ini");

                teamName = config.getValue ("teamName");
                serviceName = config.getValue ("serviceName");
                ip = config.getValue ("ip");
                port = int.Parse (config.getValue ("port"));
                registryIP = config.getValue ("registryIP");
                registryPort = int.Parse (config.getValue ("registryPort"));
                permissionLevel = int.Parse (config.getValue ("permissionLevel"));
            }
            catch
            {
                Console.WriteLine ("error reading values from config file");
                Console.ReadLine();
                isConfig = false;
            }
            if (isConfig)
            {
                FunctionCall myFunction = new FunctionCall ();
                myFunction.teamName = teamName;
                myFunction.teamID = 0;
                myFunction.tagName = tagName;
                myFunction.serviceName = serviceName;
                myFunction.IP = ip;
                myFunction.port = port;
                myFunction.description = description;

                myFunction.parameters = new List<Parameter> ();
                myFunction.parameters.Add (GenerateParam (1, "region", typeof (string), true));
                myFunction.parameters.Add (GenerateParam (2, "subTotal", typeof (double), true));
                myFunction.numParameters = 2;

                myFunction.responses = new List<Response> ();
                myFunction.responses.Add (GenerateResp (1, "sub", typeof (double)));
                myFunction.responses.Add (GenerateResp (2, "pst", typeof (double)));
                myFunction.responses.Add (GenerateResp (3, "hst", typeof (double)));
                myFunction.responses.Add (GenerateResp (4, "gst", typeof (double)));
                myFunction.responses.Add (GenerateResp (5, "total", typeof (double)));
                myFunction.numResponses = 5;

                Server myServer = new Server (myFunction, registryIP, registryPort, permissionLevel);
                Thread serverThread = new Thread (new ThreadStart (myServer.Listener));
                serverThread.Start ();

                Console.WriteLine ("Press <enter> to stop server");
                Console.ReadLine ();
                myServer.Running = false;
            }
        }

        /// <summary>
        /// Creates a parameter line
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="name">Name of Parameter</param>
        /// <param name="dataType">Datatype of Parameter</param>
        /// <param name="mandatory">Manditoryness of Parameter</param>
        /// <returns>Parameter</returns>
        private static Parameter GenerateParam (int pos, string name, Type dataType, bool mandatory)
        {
            Parameter param = new Parameter ();
            param.position = pos;
            param.name = name;
            param.dataType = dataType;
            param.mandatory = mandatory;
            param.value = null;

            return param;
        }

        /// <summary>
        /// Creates a Response Line
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="name">Name of Response</param>
        /// <param name="dataType">Datatype of Response</param>
        /// <returns>Response</returns>
        private static Response GenerateResp (int pos, string name, Type dataType)
        {
            Response resp = new Response ();
            resp.position = pos;
            resp.name = name;
            resp.dataType = dataType;
            resp.value = null;

            return resp;
        }
    }
}

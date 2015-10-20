/*
* FILE : FunctionCall.cs
* PROJECT : PROG3080 - Assignment #1
* PROGRAMMER : Constantine Grigoriadis, Sunny Mangat, Dylan Sawchuk, Nick Whitey
* FIRST VERSION : 2014-11-14
* DESCRIPTION : This file contains gettor and settors for the Function Call from the consumer
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HL7parser
{
    /// <summary>
    /// Gets and sets the incoming message from the consumer
    /// </summary>
    public class FunctionCall
    {
        public string teamName { get; set; }
        public int teamID { get; set; }
        public string serviceName { get; set; }
        public string tagName { get; set; }
        public int numParameters { get; set; }
        public int numResponses { get; set; }
        public string description { get; set; }

        public List<Parameter> parameters { get; set; }
        public List<Response> responses { get; set; }

        public string IP { get; set; }
        public int port { get; set; }
    }
}

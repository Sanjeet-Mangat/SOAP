/*
* FILE : HealthLevel7.cs
* PROJECT : PROG3080 - Assignment #1
* PROGRAMMER : Constantine Grigoriadis, Sunny Mangat, Dylan Sawchuk, Nick Whitey
* FIRST VERSION : 2014-11-14
* DESCRIPTION :This file will contain class methods to generate a message for the registry or service and methods to parse the incoming messages
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HL7parser
{
    public static class HealthLevel7
    {
        #region Char Lookup
        //ASCII Values
        private static readonly char BOM = (char)11;
        private static readonly char EOS = (char)13;
        private static readonly char EOM = (char)28;

        #endregion

        #region write
        /// <summary>
        /// Message to register a team name
        /// </summary>
        /// <param name="teamName">Team name that will be registered</param>
        /// <returns>String in HL7 format with team name</returns>
        public static string RegisterTeamMessage (string teamName)
        {
            return BOM + "DRC|REG-TEAM|||" + EOS + "INF|" + teamName + "|||" + EOS + EndOfMessage ();
        }

        /// <summary>
        /// Message to un register a team
        /// </summary>
        /// <param name="teamName">Team name that will be unregistered</param>
        /// <param name="teamID">TeamID associated with team name</param>
        /// <returns>String in HL7 format with team name and teamID</returns>
        public static string UnRegisterTeamMessage (string teamName, int teamID)
        {
            return BOM + "DRC|UNREG-TEAM|" + teamName + "|" + teamID.ToString () + "|" + EOS + EndOfMessage ();
        }

        /// <summary>
        ///  Response from this message indicates if the calling team is still valid (and hasn’t expired) and if they have the necessary security level to call your service
        /// </summary>
        /// <param name="ourTeamName">Our Team name </param>
        /// <param name="ourTeamID">Our TeamID</param>
        /// <param name="theirTeamName">Consumer team name</param>
        /// <param name="theirTeamID">Consumer teamID</param>
        /// <param name="serviceTag">Service name</param>
        /// <returns>String in HL7 format with according contents</returns>
        public static string QueryTeamMessage (string ourTeamName, int ourTeamID, string theirTeamName, int theirTeamID, string serviceTag)
        {
            return BOM + "DRC|QUERY-TEAM|" + ourTeamName + "|" + ourTeamID.ToString () + "|" + EOS +
                   "INF|" + theirTeamName + "|" + theirTeamID.ToString () + "|" + serviceTag + "|" + EOS + EndOfMessage ();
        }

        /// <summary>
        /// Build a message in order to publish a service for your team so that other teams may begin to call it
        /// </summary>
        /// <param name="function">publish service name</param>
        /// <param name="securityLevel">level of security allowed</param>
        /// <returns>String in HL7 format with according contents</returns>
        public static string PublishServiceMessage (FunctionCall function, int securityLevel = 2)
        {
            string HL7 = BOM + "DRC|PUB-SERVICE|" + function.teamName + "|" + function.teamID.ToString () + "|" + EOS +
                         "SRV|" + function.tagName + "|" + function.serviceName + "|" + securityLevel.ToString () + "|" +
                         function.numParameters.ToString () + "|" + function.numResponses.ToString () + "|" + function.description + "|" + EOS;

            foreach (Parameter param in function.parameters)
            {
                HL7 += ArgLine (param, false);
            }

            foreach (Response resp in function.responses)
            {
                HL7 += RespLine (resp, false);
            }

            HL7 += "MCH|" + function.IP.ToString () + "|" + function.port.ToString () + "|" + EOS + EndOfMessage ();
            return HL7;
        }

        /// <summary>
        ///  Message asking for information about a published service capable of doing that work for you
        /// </summary>
        /// <param name="ourTeamName">Our team name</param>
        /// <param name="ourTeamID">Our teamID</param>
        /// <param name="serviceTag">Name of service</param>
        /// <returns>String in HL7 format with according contents</returns>
        public static string QueryServiceMessage (string ourTeamName, int ourTeamID, string serviceTag)
        {
            return BOM + "DRC|QUERY-SERVICE|" + ourTeamName + "|" + ourTeamID.ToString () + "|" + EOS +
                   "SRV|" + serviceTag + "||||||" + EOS + EndOfMessage ();
        }

        /// <summary>
        /// Parse out the response(assuming it was successful) and construct the following message which you send to the service’s machine on its designated port
        /// </summary>
        /// <param name="function"></param>
        /// <returns>String in HL7 format with according contents</returns>
        public static string ExecuteServiceMessage (FunctionCall function)
        {
            string HL7 = BOM + "DRC|EXEC-SERVICE|" + function.teamName + "|" + function.teamID.ToString () + "|" + EOS +
                         "SRV||" + function.serviceName + "||" + function.numParameters.ToString () + "|||" + EOS;

            foreach (Parameter param in function.parameters)
            {
                HL7 += ArgLine (param, true);
            }

            HL7 += EndOfMessage ();
            return HL7;
        }

        /// <summary>
        /// Builds response to the client when a execute service message is recieved
        /// </summary>
        /// <param name="function"></param>
        /// <returns>String in HL7 format with according contents</returns>
        public static string ExecuteServiceResponse (FunctionCall function)
        {
            string HL7 = BOM + "PUB|OK|||" + function.numResponses.ToString () + "|" + EOS;

            foreach (Response resp in function.responses)
            {
                HL7 += RespLine (resp, true);
            }

            HL7 += EndOfMessage ();
            return HL7;
        }

        /// <summary>
        /// Builds response to the client when a execute service message is recieved
        /// </summary>
        /// <param name="errorCode">error code from response</param>
        /// <param name="errorMessage">error message from response</param>
        /// <returns>String in HL7 format with according contents</returns>
        public static string ExecuteServiceResponse (int errorCode, string errorMessage)
        {
            return BOM + "PUB|NOT-OK|" + errorCode.ToString () + "|" + errorMessage + "||" + EOS + EndOfMessage ();
        }

        /// <summary>
        /// Creates the ARG line in HL7 format
        /// </summary>
        /// <param name="param">all info with parameter</param>
        /// <param name="value">true puts in value, false puts mandatory or optional</param>
        /// <returns>String in HL7 format with according contents</returns>
        private static string ArgLine (Parameter param, bool value)
        {
            string HL7 = "ARG|" + param.position.ToString () + "|" + param.name + "|" + StringOfType (param.dataType);
            if (value)
            {
                HL7 += "|" + param.value.ToString ();
            }
            else
            {
                if ((bool)param.mandatory)
                {
                    HL7 += "mandatory|";
                }
                else
                {
                    HL7 += "optional|";
                }
            }
            return (HL7 + "|" + EOS);
        }

        /// <summary>
        /// Makes HL7 line of what is being recieved
        /// </summary>
        /// <param name="resp"></param>
        /// <param name="value"></param>
        /// <returns>String in HL7 format with according contents</returns>
        private static string RespLine (Response resp, bool value)
        {
            string HL7 = "RSP|" + resp.position.ToString () + "|" + resp.name + "|" + StringOfType (resp.dataType);
            if (value)
            {
                HL7 += resp.value.ToString ();
            }
            return (HL7 + "|" + EOS);
        }

        /// <summary>
        /// Checks to see what data type is passed
        /// </summary>
        /// <param name="toTest">data type</param>
        /// <returns>String in HL7 format with according contents</returns>
        private static string StringOfType (Type toTest)
        {
            string type = "";

            if (toTest == typeof (char))
            {
                type = "char";
            }
            if (toTest == typeof (short))
            {
                type = "short";
            }
            if (toTest == typeof (int))
            {
                type = "int";
            }
            if (toTest == typeof (long))
            {
                type = "long";
            }
            if (toTest == typeof (float))
            {
                type = "float";
            }
            if (toTest == typeof (double))
            {
                type = "double";
            }
            if (toTest == typeof (string))
            {
                type = "string";
            }


            return (type + "|");
        }

        /// <summary>
        ///  Appends a end of message
        /// </summary>
        /// <returns>String in HL7 format with according contents</returns>
        private static string EndOfMessage ()
        {
            return EOM + "" + EOS;
        }

        #endregion


        #region parse

        public enum SegmentType { SOA, SRV, ARG, RSP, MCH, PUB, DRC };

        /// <summary>
        /// Breaks message into segments
        /// </summary>
        /// <param name="message">message that will be parsed into segments</param>
        /// <returns>parsed segements</returns>
        private static List<string> parseMessage (string message)
        {
            if (message == null) throw new ThortonsSOAException ("Message cannot be null", -1);
            if (message.Length == 0) throw new ThortonsSOAException ("Message cannot be blank", -1);
            if (message[0] != BOM) throw new ThortonsSOAException ("Message does not start with a BOM character", -1);
            if (message.Count (chr => chr == BOM) > 1) throw new ThortonsSOAException ("Message cannot contain more than one BOM character", -1);

            string[] segmentList = message.Substring (1).Split (EOS);

            if (segmentList.Last () != "" && segmentList[segmentList.Count () - 2] != EOM.ToString ()) throw new ThortonsSOAException ("The message does not end with an EOM segment", -1);
            if (segmentList.Count (str => str.Contains (EOM)) > 1) throw new ThortonsSOAException ("The message cannot contain more than one EOM character", -1);

            List<string> segments = new List<string> ();

            int i = -1;
            while (++i < segmentList.Count () && segmentList[i] != EOM.ToString ())
            {
                segments.Add (segmentList[i]);
            }

            return segments;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        private static SegmentType parseSegment (string segment, out List<string> values)
        {
            if (segment == null) throw new ThortonsSOAException ("Segment cannot be NULL", -2);
            if (segment.Length == 0) throw new ThortonsSOAException ("Segment cannot be empty", -2);
            if (segment.Last () != '|') throw new ThortonsSOAException ("Segment must end with a | character", -2);

            string[] tokens = segment.Substring (0, segment.Length - 1).Split ('|');

            SegmentType type;
            try
            {
                type = (SegmentType)Enum.Parse (typeof (SegmentType), tokens.First (), true);
            }
            catch
            {
                throw new ThortonsSOAException ("Invalid segment directive found (" + tokens.First () + ")", -1);
            }

            values = new List<string> ();

            for (int i = 1; i < tokens.Count (); ++i)
            {
                values.Add (tokens[i]);
            }

            return type;
        }

        /// <summary>
        /// Gets message that needs to be parsed and will send to parseMessage
        /// </summary>
        /// <param name="message">recieved message</param>
        /// <param name="response"> true if is response message, false if it is not</param>
        /// <returns>parsed segments</returns>
        public static object parse (string message, bool response = true)
        {
            List<string> segments = parseMessage (message);

            object returnValue = null;

            if (response)
            {
                List<string> values;
                SegmentType type = parseSegment (segments[0], out values);

                if (type != SegmentType.SOA && type != SegmentType.PUB)
                {
                    throw new ThortonsSOAException ("A response message must start with a SOA or a PUB segment", -1);
                }

                SoaResponse success = parseSOA (values, type);

                if (success.OK == true && success.NumSegments != 0)
                {
                    if (success.type == SegmentType.SOA)
                    {
                        returnValue = parseService (segments.GetRange (1, segments.Count - 1), success.NumSegments);
                    }
                    else if (success.type == SegmentType.PUB)
                    {
                        returnValue = parseResponses (segments.GetRange (1, segments.Count - 1), success.NumSegments);
                    }
                }
                else
                {
                    returnValue = success;
                }
            }
            else
            {
                returnValue = parseRequest (segments);
            }

            return returnValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static SoaResponse parseSOA (List<string> values, SegmentType type)
        {
            switch (values[0])
            {
                case "OK":
                    SoaResponse resp = new SoaResponse () { OK = true, type = type };

                    int teamID = 0;
                    if (int.TryParse (values[1], out teamID))
                    {
                        resp.teamID = teamID;
                    }

                    DateTime expiration;
                    if (DateTime.TryParseExact (values[2], "hh:mm:ss", new CultureInfo ("en-US"), DateTimeStyles.AllowWhiteSpaces, out expiration))
                    {
                        resp.expiration = expiration;
                    }

                    int numSegments = 0;
                    if (int.TryParse (values[3], out numSegments))
                    {
                        resp.NumSegments = numSegments;
                    }

                    return resp;
                case "NOT-OK":
                    int errorCode;
                    if (int.TryParse (values[1], out errorCode))
                    {
                        return new SoaResponse () { OK = false, ErrorCode = errorCode, ErrorMessage = values[2], type = type };
                    }
                    else throw new ThortonsSOAException ("Failed to parse error code", -3);
                default:
                    throw new ThortonsSOAException ("Failed to parse <" + values[0] + "> as <OK> or <NOT-OK>", -2);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segments"></param>
        /// <param name="numSegments"></param>
        /// <returns></returns>
        private static FunctionCall parseService (List<string> segments, int numSegments)
        {
            if (numSegments != -1 && segments.Count != numSegments) throw new ThortonsSOAException ("The message contains a number of segments that does not match it's specified segment count", -1);

            FunctionCall returnValue = new FunctionCall ();

            parseSRV (segments[0], ref returnValue);

            if (2 + returnValue.numParameters + returnValue.numResponses != numSegments) throw new ThortonsSOAException ("The message contains a number of segments that does not match it's specified segment and parameter counts", -1);

            returnValue.parameters = parseArguments (segments.GetRange (1, returnValue.numParameters), returnValue.numParameters, false);
            returnValue.responses = parseResponses (segments.GetRange (1 + returnValue.numParameters, returnValue.numResponses), returnValue.numResponses);

            parseMCH (segments[segments.Count - 1], ref returnValue);

            return returnValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="info"></param>
        private static void parseSRV (string segment, ref FunctionCall info)
        {
            List<string> values;
            SegmentType type = parseSegment (segment, out values);
            if (type != SegmentType.SRV) throw new ThortonsSOAException ("expected SRV segment, found " + type.ToString () + " segment instead", -1);

            if (values[0] != "")
            {
                info.teamName = values[0];
            }
            info.serviceName = values[1];
            info.description = values[5];

            int temp;
            if (int.TryParse (values[3], out temp) == false)
            {
                throw new ThortonsSOAException ("failed to parse <" + values[3] + "> as an integer", -2);
            }
            info.numParameters = temp;

            if (int.TryParse (values[4], out temp) == true)
            {
                info.numResponses = temp;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="numParameters"></param>
        /// <returns></returns>
        private static List<Parameter> parseArguments (List<string> list, int numParameters, bool isValue = true)
        {
            List<Parameter> parameters = new List<Parameter> ();

            foreach (string segment in list)
            {
                List<string> values;
                SegmentType type = parseSegment (segment, out values);
                if (type != SegmentType.ARG) throw new ThortonsSOAException ("expected segment of type ARG found type " + type.ToString (), -1);

                Parameter newParameter = new Parameter ();

                int temp;
                if (int.TryParse (values[0], out temp) == false) throw new ThortonsSOAException ("Unable to parse <" + values[0] + "> as an integer", -2);

                newParameter.position = temp;
                newParameter.name = values[1];

                Type t;
                object o;
                tryParse (values[2], out t, values[4], out o, isValue);
                newParameter.dataType = t;
                newParameter.value = o;

                if (!isValue)
                {
                    switch (values[3].ToLower ())
                    {
                        case "mandatory":
                            newParameter.mandatory = true;
                            break;
                        case "optional":
                            newParameter.mandatory = false;
                            break;
                        case "":
                            newParameter.mandatory = null;
                            break;
                        default:
                            throw new ThortonsSOAException ("failed to interpret <" + values[3] + "> as mandatory or optional", -2);
                    }
                }

                parameters.Add (newParameter);
            }
            return parameters;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeString"></param>
        /// <param name="type"></param>
        /// <param name="valueString"></param>
        /// <param name="value"></param>
        private static void tryParse (string typeString, out Type type, string valueString, out object value, bool parseValue = true)
        {
            value = null;
            switch (typeString.ToLower ())
            {
                case "char":
                    type = typeof (char);
                    if (valueString == "")
                    {
                        value = null;
                    }
                    else if (parseValue)
                    {
                        char temp;
                        if (char.TryParse (valueString, out temp) == false) throw new ThortonsSOAException ("Cannot parse <" + valueString + "> as a " + typeString, -2);
                        value = temp;
                    }
                    break;
                case "short":
                    type = typeof (short);
                    if (valueString == "")
                    {
                        value = null;
                    }
                    else if (parseValue)
                    {
                        short temp;
                        if (short.TryParse (valueString, out temp) == false) throw new ThortonsSOAException ("Cannot parse <" + valueString + "> as a " + typeString, -2);
                        value = temp;
                    }
                    break;
                case "int":
                    type = typeof (int);
                    if (valueString == "")
                    {
                        value = null;
                    }
                    else if (parseValue)
                    {
                        int temp;
                        if (int.TryParse (valueString, out temp) == false) throw new ThortonsSOAException ("Cannot parse <" + valueString + "> as a " + typeString, -2);
                        value = temp;
                    }
                    break;
                case "long":
                    type = typeof (long);
                    if (valueString == "")
                    {
                        value = null;
                    }
                    else if (parseValue)
                    {
                        long temp;
                        if (long.TryParse (valueString, out temp) == false) throw new ThortonsSOAException ("Cannot parse <" + valueString + "> as a " + typeString, -2);
                        value = temp;
                    }
                    break;
                case "float":
                    type = typeof (float);
                    if (valueString == "")
                    {
                        value = null;
                    }
                    else if (parseValue)
                    {
                        float temp;
                        if (float.TryParse (valueString, out temp) == false) throw new ThortonsSOAException ("Cannot parse <" + valueString + "> as a " + typeString, -2);
                        value = temp;
                    }
                    break;
                case "double":
                    if (valueString == "")
                    {
                        value = null;
                    }
                    else if (parseValue)
                    {
                        double temp;
                        if (double.TryParse (valueString, out temp) == false) throw new ThortonsSOAException ("Cannot parse <" + valueString + "> as a " + typeString, -2);
                        value = temp;
                    }
                    type = typeof (double);
                    break;
                case "string":
                    type = typeof (string);
                    value = valueString;
                    break;
                default:
                    throw new ThortonsSOAException ("cannot parse values of type: " + typeString, -3);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="numResponses"></param>
        /// <returns></returns>
        private static List<Response> parseResponses (List<string> list, int numResponses)
        {
            if (list.Count != numResponses) throw new ThortonsSOAException ("There are not as many arguments as expected", -1);

            List<Response> responses = new List<Response> ();

            foreach (string segment in list)
            {
                List<string> values;
                SegmentType type = parseSegment (segment, out values);
                if (type != SegmentType.RSP) throw new ThortonsSOAException ("expected segment of type RSP found type " + type.ToString (), -1);

                Response newResponse = new Response ();

                int temp;
                if (int.TryParse (values[0], out temp) == false) throw new ThortonsSOAException ("Unable to parse <arg position> as an integer", -1);

                newResponse.position = temp;
                newResponse.name = values[1];

                Type t;
                object o;
                tryParse (values[2], out t, values[3], out o);
                newResponse.dataType = t;
                newResponse.value = o;

                responses.Add (newResponse);
            }

            return responses;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="returnValue"></param>
        private static void parseMCH (string segment, ref FunctionCall returnValue)
        {
            List<string> values;
            SegmentType type = parseSegment (segment, out values);
            if (type != SegmentType.MCH) throw new ThortonsSOAException ("expected MCH segment, found " + type.ToString (), -1);

            returnValue.IP = values[0];

            int temp;
            if (int.TryParse (values[1], out temp) == false) throw new ThortonsSOAException ("Failed to parse IP as integer", -2);
            returnValue.port = temp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        private static FunctionCall parseRequest (List<string> segments)
        {
            FunctionCall returnValue = new FunctionCall ();

            List<string> values;
            SegmentType type = parseSegment (segments[0], out values);

            if (type != SegmentType.DRC) throw new ThortonsSOAException ("Expected segment of type DRC, found " + type.ToString (), -1);
            if (values[0] != "EXEC-SERVICE") throw new ThortonsSOAException ("expected <EXEC-SERVICE>, found <" + values[0] + ">", -2);

            returnValue.teamName = values[1];

            int temp;
            if (int.TryParse (values[2], out temp))
                returnValue.teamID = temp;

            parseSRV (segments[1], ref returnValue);

            returnValue.parameters = parseArguments (segments.GetRange (2, segments.Count - 2), returnValue.numParameters);
            return returnValue;
        }

        #endregion
    }
}

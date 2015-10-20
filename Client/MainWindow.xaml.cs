/*
* FILE : MainWindow.xaml.cs
* PROJECT : PROG3080 - Assignment #1
* PROGRAMMER : Constantine Grigoriadis, Sunny Mangat, Dylan Sawchuk, Nick Whitey
* FIRST VERSION : 2014-11-28
* DESCRIPTION : contains the client logic for the thortons SOA 
*/

using HL7parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FunctionCall function;
        string teamName;
        int teamID;
        DateTime exporation;
        string registryIP;
        int registryPort;
        bool configFileFound = false;
        int MaxMessageSize = 2048;
        List<ParameterEntry> entryLines = new List<ParameterEntry> ();

        /// <summary>
        /// initialises xaml components, reads config file, and registers team
        /// </summary>
        public MainWindow ()
        {
            InitializeComponent ();

            readConfigFile ();

            if (configFileFound)
            {
                Log.ConsumerRunTimeLogHeader (teamName);
                registerTeam ();
            }
            else
            {
                new noConfigFile ().ShowDialog ();
                this.Close ();
            }
        }

        /// <summary>
        /// reads the needed values from the config file
        /// </summary>
        private void readConfigFile ()
        {
            try
            {
                ConfigFile config = new ConfigFile ("clientConfig.ini");

                teamName = config.getValue ("teamName");
                registryIP = config.getValue ("registryIP");
                registryPort = int.Parse (config.getValue ("registryPort"));

                configFileFound = true;
            }
            catch
            {
                configFileFound = false;
            }
        }

        /// <summary>
        /// registers the team with the SOA registry server
        /// </summary>
        private void registerTeam ()
        {
            string response;
            try
            {
                Socket client = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect (registryIP, registryPort);

                string request = HealthLevel7.RegisterTeamMessage (teamName);

                using (StreamWriter writer = new StreamWriter (new NetworkStream (client)))
                {
                    writer.Write (request);
                    writer.Flush ();
                }
                Log.SOARegistryMessge (request);

                using (StreamReader reader = new StreamReader (new NetworkStream (client)))
                {
                    response = reader.ReadToEnd ();
                }
                Log.SOAResponseRegistry (response);

                client.Disconnect (false);
            }
            catch (Exception e)
            {
                parameterList.Children.Add (new TextBlock () { Text = "Error occured while communicating with registry server: " + e.Message, FontSize = 20 });
                return;
            }

            object parsedResponse;
            try
            {
                parsedResponse = HealthLevel7.parse (response);
            }
            catch (ThortonsSOAException e)
            {
                parameterList.Children.Add (new TextBlock () { Text = "Error occured while parsing response " + e.Message, FontSize = 20 });
                return;
            }
            catch (Exception e)
            {
                parameterList.Children.Add (new TextBlock () { Text = "Unexpected error occured while parsing response " + e.Message, FontSize = 20 });
                return;
            }

            if (parsedResponse.GetType () == typeof (SoaResponse))
            {
                SoaResponse soaResponse = (SoaResponse)parsedResponse;

                if (soaResponse.OK)
                {
                    teamID = soaResponse.teamID;
                    exporation = soaResponse.expiration;
                }
                else
                {
                    addSoaResponse (soaResponse);
                }
            }
            else
            {
                parameterList.Children.Add (new TextBlock () { Text = "Recieved unexpected message type <" + parsedResponse.GetType () + ">, expected <SoaResponse>", FontSize = 20 });
            }
        }

        /// <summary>
        /// calls updateParameterEntry() with the proper service tag
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        private void requestServiceButton_Click (object sender, RoutedEventArgs e)
        {
            parameterList.Children.Clear ();

            switch (serviceSelection.SelectedIndex)
            {
                case -1:
                    parameterList.Children.Add (new TextBlock () { Text = "You must select a service", FontSize = 20 });
                    break;
                case 0:
                    updateParameterEntry ("GIORP-TOTAL");
                    break;
                case 1:
                    updateParameterEntry ("PAYROLL");
                    break;
                case 2:
                    updateParameterEntry ("CAR-LOAN");
                    break;
                case 3:
                    updateParameterEntry ("POSTAL");
                    break;
                default:
                    parameterList.Children.Add (new TextBlock () { Text = "There was an unexpected error with your service selection\n    please try again", FontSize = 20 });
                    break;
            }
        }

        /// <summary>
        /// calls the registry service for service info and updates the parameter UI
        /// </summary>
        /// <param name="serviceTag">the tag of the service to inquire about</param>
        private void updateParameterEntry (string serviceTag)
        {
            string response;
            try
            {
                Socket client = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect (registryIP, registryPort);

                string request = HealthLevel7.QueryServiceMessage (teamName, teamID, serviceTag);

                using (StreamWriter writer = new StreamWriter (new NetworkStream (client)))
                {
                    writer.Write (request);
                    writer.Flush ();
                }
                Log.SOARegistryMessge (request);

                using (StreamReader reader = new StreamReader (new NetworkStream (client)))
                {
                    response = reader.ReadToEnd ();
                }
                Log.SOAResponseRegistry (response);

                client.Disconnect (false);
            }
            catch (Exception e)
            {
                parameterList.Children.Add (new TextBlock () { Text = "Error occured while communicating with registry server: " + e.Message, FontSize = 20 });
                return;
            }

            object parsedResponse;
            try
            {
                parsedResponse = HealthLevel7.parse (response);
            }
            catch (ThortonsSOAException e)
            {
                parameterList.Children.Add (new TextBlock () { Text = "Error occured while parsing response " + e.Message, FontSize = 20 });
                return;
            }
            catch (Exception e)
            {
                parameterList.Children.Add (new TextBlock () { Text = "Unexpected error occured while parsing response " + e.Message, FontSize = 20 });
                return;
            }

            if (parsedResponse.GetType () == typeof (FunctionCall))
            {

                function = (FunctionCall)parsedResponse;
                parameterList.Children.Clear ();
                entryLines.Clear ();


                serviceInfo.Text = function.teamName + " - " + function.description;

                if (function.numParameters == 0)
                {
                    parameterList.Children.Add (new TextBlock () { Text = "No Parameters", FontSize = 20 });
                }
                else foreach (Parameter param in function.parameters)
                    {
                        ParameterEntry newEntry = new ParameterEntry ();
                        newEntry.parameterName.Text = param.name;
                        newEntry.parameterType.Text = param.dataType.ToString () + " - " + (param.mandatory == true ? "mandatory" : "optional");

                        parameterList.Children.Add (newEntry);
                        entryLines.Add (newEntry);
                    }
            }
            else if (parsedResponse.GetType () == typeof (SoaResponse))
            {
                function = null;
                parameterList.Children.Clear ();
                entryLines.Clear ();

                addSoaResponse ((SoaResponse)parsedResponse);
            }
            else
            {
                function = null;
                parameterList.Children.Clear ();
                entryLines.Clear ();

                parameterList.Children.Add (new TextBlock () { Text = "Recieved an unexpected response type <" + parsedResponse.GetType () + ">", FontSize = 20 });
            }
        }

        /// <summary>
        /// adds a SoaResponse object to the UI
        /// </summary>
        /// <param name="soaResponse">the SoaResponse object to display</param>
        private void addSoaResponse (SoaResponse soaResponse)
        {
            parameterList.Children.Add (new TextBlock () { Text = (soaResponse.type == HealthLevel7.SegmentType.SOA ? "SOA" : "PUB") + " Response: " + (soaResponse.OK ? "" : "NOT ") + "OK", FontSize = 20 });
            if (!soaResponse.OK)
            {
                parameterList.Children.Add (new TextBlock () { Text = "Error Code: " + soaResponse.ErrorCode, FontSize = 20 });
                parameterList.Children.Add (new TextBlock () { Text = "Error Message: " + soaResponse.ErrorMessage, FontSize = 20 });
            }
        }

        /// <summary>
        /// validates the information entered for the parameters and calls callService()
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        private void callServiceButton_Click (object sender, RoutedEventArgs e)
        {
            if (function != null)
            {
                resetParameterList ();

                bool entryError = false;

                foreach (ParameterEntry entry in entryLines)
                {
                    TextBlock errorElement;
                    if (!entryValid (entry, out errorElement))
                    {
                        entryError = true;
                        parameterList.Children.Add (errorElement);
                    }
                }

                if (!entryError)
                {
                    callService ();
                }
            }
        }

        /// <summary>
        /// calls a service from the information recieved from the Soa registry server
        /// </summary>
        private void callService ()
        {
            bool parametersValid = true;
            foreach (Parameter param in function.parameters)
            {
                if (param.mandatory == true && param.value == null)
                {
                    parametersValid = false;
                    parameterList.Children.Add (new TextBlock () { Text = "Mandatory parameter " + param.name + " is missing" });
                }
            }

            if (parametersValid)
            {
                string response = string.Empty;
                try
                {
                    Socket client = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    client.Connect (function.IP, function.port);

                    function.teamID = teamID;
                    function.teamName = teamName;

                    string request = HealthLevel7.ExecuteServiceMessage (function);

                    using (StreamWriter writer = new StreamWriter (new NetworkStream (client)))
                    {
                        writer.Write (request);
                        writer.Flush ();
                    }
                    Log.ConsumerSendingServiceRequest (function.IP, function.port.ToString (), request);

                    if (client.Connected)
                    {
                        Byte[] byteMessage = new Byte[MaxMessageSize];
                        //the server receives the HTTP request from the browser 
                        int i = client.Receive (byteMessage, byteMessage.Length, 0);
                        response = Encoding.ASCII.GetString (byteMessage, 0, i);
                        client.Shutdown (SocketShutdown.Both);
                        client.Close ();
                    }
                    Log.responseFromService (response);
                }
                catch (Exception e)
                {
                    parameterList.Children.Add (new TextBlock () { Text = "Error occured while communicating with registry server: " + e.Message, FontSize = 20 });
                    return;
                }

                object parsedResponse;
                try
                {
                    parsedResponse = HealthLevel7.parse (response);
                }
                catch (ThortonsSOAException e)
                {
                    parameterList.Children.Add (new TextBlock () { Text = "Error occured while parsing response " + e.Message, FontSize = 20 });
                    return;
                }
                catch (Exception e)
                {
                    parameterList.Children.Add (new TextBlock () { Text = "Unexpected error occured while parsing response " + e.Message, FontSize = 20 });
                    return;
                }

                if (parsedResponse.GetType () == typeof (SoaResponse))
                {
                    addSoaResponse ((SoaResponse)parsedResponse);
                }
                else if (parsedResponse.GetType () == typeof (List<Response>))
                {
                    foreach (Response resp in (List<Response>)parsedResponse)
                    {
                        ParameterEntry entry = new ParameterEntry ();
                        entry.parameterValue.Text = resp.value.ToString ();
                        entry.parameterValue.IsEnabled = false;

                        entry.parameterName.Text = resp.name.ToString ();
                        entry.parameterType.Text = resp.dataType.ToString ();

                        parameterList.Children.Add (entry);
                    }
                }
                else
                {
                    parameterList.Children.Add (new TextBlock () { Text = "Recieved an unexpected response type <" + parsedResponse.GetType () + ">", FontSize = 20 });
                }
            }
        }

        /// <summary>
        /// parses a single parameter entry for validity and mandatory-ness
        /// </summary>
        /// <param name="entry">the UI element containing the data to parse</param>
        /// <param name="errorElement">the UI element to contain any parsing errors</param>
        /// <returns>bool - indicating if the entry is valid</returns>
        private bool entryValid (ParameterEntry entry, out TextBlock errorElement)
        {
            Parameter parameter = function.parameters.Where (param => param.name == entry.parameterName.Text).FirstOrDefault ();
            errorElement = new TextBlock () { FontSize = 20 };

            if (parameter == null)
            {
                errorElement.Text = "failed to find parameter: " + entry.parameterName.Text + " in parameter list";
                return false;
            }
            else
            {
                if (entry.parameterValue.Text == "")
                {
                    if (parameter.mandatory == true)
                    {
                        errorElement.Text = parameter.name + " is a mandatory field";
                        return false;
                    }
                    else
                    {
                        parameter.value = null;
                    }
                }
                else
                {
                    if (parameter.dataType == typeof (string))
                    {
                        parameter.value = entry.parameterValue.Text;
                    }
                    else if (parameter.dataType == typeof (char))
                    {
                        char temp;
                        if (char.TryParse (entry.parameterValue.Text, out temp) == false)
                        {
                            errorElement.Text = "Cannot parse " + parameter.name + " as a char";
                            return false;
                        }
                        parameter.value = temp;
                    }
                    else if (parameter.dataType == typeof (short))
                    {
                        short temp;
                        if (short.TryParse (entry.parameterValue.Text, out temp) == false)
                        {
                            errorElement.Text = "Cannot parse " + parameter.name + " as a short";
                            return false;
                        }
                        parameter.value = temp;
                    }
                    else if (parameter.dataType == typeof (int))
                    {
                        int temp;
                        if (int.TryParse (entry.parameterValue.Text, out temp) == false)
                        {
                            errorElement.Text = "Cannot parse " + parameter.name + " as a nt";
                            return false;
                        }
                        parameter.value = temp;
                    }
                    else if (parameter.dataType == typeof (long))
                    {
                        long temp;
                        if (long.TryParse (entry.parameterValue.Text, out temp) == false)
                        {
                            errorElement.Text = "Cannot parse " + parameter.name + " as a long";
                            return false;
                        }
                        parameter.value = temp;
                    }
                    else if (parameter.dataType == typeof (char))
                    {
                        char temp;
                        if (char.TryParse (entry.parameterValue.Text, out temp) == false)
                        {
                            errorElement.Text = "Cannot parse " + parameter.name + " as a char";
                            return false;
                        }
                        parameter.value = temp;
                    }
                    else if (parameter.dataType == typeof (float))
                    {
                        float temp;
                        if (float.TryParse (entry.parameterValue.Text, out temp) == false)
                        {
                            errorElement.Text = "Cannot parse " + parameter.name + " as a float";
                            return false;
                        }
                        parameter.value = temp;
                    }
                    else if (parameter.dataType == typeof (double))
                    {
                        double temp;
                        if (double.TryParse (entry.parameterValue.Text, out temp) == false)
                        {
                            errorElement.Text = "Cannot parse " + parameter.name + " as a double";
                            return false;
                        }
                        parameter.value = temp;
                    }
                    else
                    {
                        errorElement.Text = "Cannot parse parameter of type: " + parameter.dataType;
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// clears any non parameter entry UI from the screen
        /// </summary>
        private void resetParameterList ()
        {
            parameterList.Children.Clear ();

            foreach (ParameterEntry entry in entryLines)
                parameterList.Children.Add (entry);
        }

        /// <summary>
        /// attempts to unregister the team from the soa registry server while the window is closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing (object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (configFileFound)
            {
                try
                {
                    Socket client = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    client.Connect (registryIP, registryPort);

                    string request = HealthLevel7.UnRegisterTeamMessage (teamName, teamID);
                    string response;

                    using (StreamWriter writer = new StreamWriter (new NetworkStream (client)))
                    {
                        writer.Write (request);
                        writer.Flush ();
                    }
                    Log.SOARegistryMessge (request);

                    using (StreamReader reader = new StreamReader (new NetworkStream (client)))
                    {
                        response = reader.ReadToEnd ();
                    }
                    Log.SOAResponseRegistry (response);

                    client.Disconnect (false);
                }
                catch
                {
                    //do nothing
                }
            }
        }
    }
}

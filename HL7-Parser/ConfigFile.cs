/*
* FILE : ConfigFile.cs
* PROJECT : PROG3080 - Assignment #1
* PROGRAMMER : Constantine Grigoriadis, Sunny Mangat, Dylan Sawchuk, Nick Whitey
* FIRST VERSION : 2014-11-14
* DESCRIPTION : This file will read and parse information from the config file
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HL7parser
{
    /// <summary>
    /// Reads info from config file and parses the data
    /// </summary>
    public class ConfigFile
    {
        List<string> contents;

        /// <summary>
        /// Will get the config filename
        /// </summary>
        /// <param name="fileName">name of the config file</param>
        public ConfigFile (string fileName)
        {
            if (fileName == null) throw new ArgumentNullException ("fileName");
            if (fileName == "") throw new ArgumentException ("parameter cannot be blank", "fileName");

            using (StreamReader reader = new StreamReader (new FileStream (fileName, FileMode.Open, FileAccess.Read)))
            {
                contents = reader.ReadToEnd ().Split ('\n').Select (line => line.Trim ()).Where (line => line != "").ToList<string> ();
            }

        }

        /// <summary>
        /// will get the value from a config file
        /// </summary>
        /// <param name="valueName">name of the value being read from file</param>
        /// <returns>the values from the config file</returns>
        public string getValue (string valueName)
        {
            var matches = contents.Where (value => new Regex ("^" + valueName + " ?= ?.+", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace).IsMatch (value));
            if (matches.Count () != 1) throw new FormatException ("expected 1 match of \"" + valueName + "=<value>\", found " + matches.Count ());

            return matches.First ().Substring (valueName.Length + 1).Trim ();
        }
    }
}

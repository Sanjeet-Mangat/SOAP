
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HL7parser
{
    /// <summary>
    /// Will set the position, name, dataType and value and parse it back for response
    /// </summary>
    public class Response
    {
        public int position { get; set; }
        public string name { get; set; }
        public Type dataType { get; set; }
        public object value { get; set; }
    }
}

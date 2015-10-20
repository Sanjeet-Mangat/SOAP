

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HL7parser
{
    /// <summary>
    /// Gets and sets the parameter name, type, value and is mandatory 
    /// </summary>
    public class Parameter
    {
        public int position { get; set; }
        public string name { get; set; }
        public Type dataType { get; set; }
        public bool? mandatory { get; set; }
        public object value { get; set; }
    }
}

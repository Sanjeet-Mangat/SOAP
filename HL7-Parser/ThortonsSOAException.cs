
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HL7parser
{
    /// <summary>
    /// contains exception using a message and an error code
    /// </summary>
    public class ThortonsSOAException : Exception
    {
        public int errorCode { get; set; }

        /// <summary>
        /// initialises a thortonSOAException with specified message and error code
        /// </summary>
        /// <param name="message">the message of the exception</param>
        /// <param name="errorCode">the error code of the exception</param>
        public ThortonsSOAException (string message, int errorCode) : base (message)
        {
            this.errorCode = errorCode;
        }
    }
}

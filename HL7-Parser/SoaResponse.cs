
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HL7parser
{
    /// <summary>
    /// Gets and Sets the values to be sent from the SOA response
    /// </summary>
    public class SoaResponse
    {
        public HealthLevel7.SegmentType type { get; set; }
        public bool OK { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public int NumSegments { get; set; }
        public int teamID { get; set; }
        public DateTime expiration { get; set; }
    }
}

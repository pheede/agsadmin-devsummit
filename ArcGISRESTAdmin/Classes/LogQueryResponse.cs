// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ArcGISRESTAdmin.Classes
{
    public class LogQueryResponse
    {
        public bool hasMore { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime startTime { get; set; }
        
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime endTime { get; set; }

        public LogMessage[] logMessages { get; set; }
    }
}

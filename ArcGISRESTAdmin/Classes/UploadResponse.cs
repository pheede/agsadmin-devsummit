// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using Newtonsoft.Json;

namespace ArcGISRESTAdmin.Classes
{

    public class UploadResponse
    {
        public string status { get; set; }
        public UploadItem item { get; set; }
    }

    public class UploadItem
    {
        public string itemID { get; set; }
        public string itemName { get; set; }
        public string description { get; set; }
        public string pathOnServer { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime date { get; set; }
        public bool committed { get; set; }
        public string serviceName { get; set; }
    }

}

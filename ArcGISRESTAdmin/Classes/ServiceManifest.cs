using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGISRESTAdmin.Classes
{
    public class ServiceManifest
    {
        public Database[] databases { get; set; }
        public Resource[] resources { get; set; }
    }

    public class Database
    {
        public bool byReference { get; set; }
        public string onServerWorkspaceFactoryProgID { get; set; }
        public string onServerConnectionString { get; set; }
        public string onPremiseConnectionString { get; set; }
        public string onServerName { get; set; }
        public string onPremisePath { get; set; }
        public Dataset[] datasets { get; set; }
    }

    public class Dataset
    {
        public string onServerName { get; set; }
    }

    public class Resource
    {
        public string onPremisePath { get; set; }
        public string clientName { get; set; }
        public string serverPath { get; set; }
    }
}

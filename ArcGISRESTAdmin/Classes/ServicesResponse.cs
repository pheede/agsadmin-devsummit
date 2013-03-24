// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace ArcGISRESTAdmin.Classes
{
    public class ServicesResponse
    {
        public string folderName { get; set; }
        public string description { get; set; }
        public bool webEncrypted { get; set; }
        public bool isDefault { get; set; }
        public string[] folders { get; set; }
        public FolderDetail[] foldersDetail { get; set; }
        public Service[] services { get; set; }
    }

    public class FolderDetail
    {
        public string folderName { get; set; }
        public string description { get; set; }
        public bool webEncrypted { get; set; }
        public bool isDefault { get; set; }
    }

    public class Service
    {
        public string folderName { get; set; }
        public string serviceName { get; set; }
        public string type { get; set; }
        public string description { get; set; }
    }
}

// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using Newtonsoft.Json;

namespace ArcGISRESTAdmin.Classes
{

    public class ServicesReportResponse
    {
        public ServiceReport[] reports { get; set; }
    }

    public class ServiceReport
    {
        public string folderName { get; set; }
        public string serviceName { get; set; }
        public ServiceType type { get; set; }
        public string description { get; set; }
        public bool isDefault { get; set; }
        public bool isPrivate { get; set; }
        public Status status { get; set; }
        public Instances instances { get; set; }
        public Properties properties { get; set; }
        public Iteminfo iteminfo { get; set; }
        public Permission[] permissions { get; set; }
    }

    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum ServiceType
    {
        MapServer,
        GPServer
        // there are more types not included here..

    }
    
    public class Status
    {
        public ServiceStatus configuredState { get; set; }
        public ServiceStatus realTimeState { get; set; }
    }

    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum ServiceStatus
    {
        Started,
        Stopped
    }

    public class Instances
    {
        public string folderName { get; set; }
        public string serviceName { get; set; }
        public string type { get; set; }
        public int max { get; set; }
        public int busy { get; set; }
        public int free { get; set; }
        public int initializing { get; set; }
        public int notCreated { get; set; }
        public int transactions { get; set; }
        public int totalBusyTime { get; set; }
        public bool isStatisticsAvailable { get; set; }
    }

    public class Properties
    {
        public int maxImageHeight { get; set; }
        public string virtualCacheDir { get; set; }
        public string textAntialiasingMode { get; set; }
        public int maxImageWidth { get; set; }
        public bool enableDynamicLayers { get; set; }
        public string dynamicDataWorkspaces { get; set; }
        public string supportedImageReturnTypes { get; set; }
        public bool disableIdentifyRelates { get; set; }
        public bool isCached { get; set; }
        public double maxScale { get; set; }
        public int maxBufferCount { get; set; }
        public int maxRecordCount { get; set; }
        public bool schemaLockingEnabled { get; set; }
        public string filePath { get; set; }
        public bool cacheOnDemand { get; set; }
        public bool useLocalCacheDir { get; set; }
        public string virtualOutputDir { get; set; }
        public string outputDir { get; set; }
        public double minScale { get; set; }
        public int maxDomainCodeCount { get; set; }
        public bool ignoreCache { get; set; }
        public string antialiasingMode { get; set; }
        public bool clientCachingAllowed { get; set; }
        public string cacheDir { get; set; }
        public string tilingScheme { get; set; }
        public string showMessages { get; set; }
        public string toolbox { get; set; }
        public string resultMapServer { get; set; }
        public string jobsDirectory { get; set; }
        public string executionType { get; set; }
        public string jobsVirtualDirectory { get; set; }
        public string maximumRecords { get; set; }
    }

    public class Iteminfo
    {
        public string culture { get; set; }
        public string name { get; set; }
        public string guid { get; set; }
        public string catalogPath { get; set; }
        public string snippet { get; set; }
        public string description { get; set; }
        public string summary { get; set; }
        public string title { get; set; }
        public string[] tags { get; set; }
        public string type { get; set; }
        public string[] typeKeywords { get; set; }
        public string thumbnail { get; set; }
        public string url { get; set; }
        public float[][] extent { get; set; }
        public string spatialReference { get; set; }
        public string accessInformation { get; set; }
        public string licenseInfo { get; set; }
    }

    public class Permission
    {
        public string principal { get; set; }
        public Permission1 permission { get; set; }
        public object childURL { get; set; }
        public object operation { get; set; }
    }

    public class Permission1
    {
        public bool isAllowed { get; set; }
        public string constraint { get; set; }
    }
}

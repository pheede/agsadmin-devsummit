// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGISRESTAdmin;
using ArcGISRESTAdmin.Classes;

namespace ArcGISRESTAdminCLIDemo
{
    /// <summary>
    /// Small simple command line program to demonstrate functionality of ArcGISRESTAdmin class library.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            DoWork().Wait();

            Console.Out.WriteLine("Press enter to continue.");
            Console.In.ReadLine();
        }

        static async Task DoWork()
        {
            var ags = new AGSClient("https://philmbprowin.esri.com:6443/arcgis/admin/", "admin", "demopw");

            await ags.Authenticate();

            Console.Out.WriteLine("Authenticated against {0}: {1}", ags.ServerUrl, ags.IsAuthenticated);
            Console.Out.WriteLine("Session expires at {0}", ags.TokenExpiration.ToLocalTime());
            Console.Out.WriteLine("------------------");

            // get last 12 hours of log entries, do some statistics on them

            var logEntries = await ags.GetLogs(LogMessage.LogType.Info, DateTime.Now, DateTime.Now.AddHours(-12));

            int severeCount = (from entry in logEntries where entry.type == LogMessage.LogType.Severe select entry).Count();

            Console.Out.WriteLine(string.Format("The server had {0} severe events in the past hour!", severeCount));
            Console.Out.WriteLine("------------------");

            // get status of all services in all folders

            var folders = await ags.GetFolderNames();

            Console.Out.WriteLine("The following folders are defined on the server:");
            Console.Out.WriteLine("/");
            foreach (string folder in folders)
            {
                Console.Out.WriteLine(folder);
            }
            Console.Out.WriteLine("------------------");

            var serviceStatus = await ags.GetAllServiceReports();

            foreach (string folder in serviceStatus.Keys)
            {
                Console.Out.WriteLine(folder);
                foreach (var report in serviceStatus[folder])
                {
                    Console.Out.WriteLine(string.Format(" - {0}: {1}", report.serviceName, report.status.realTimeState.ToString()));
                }
            }
            Console.Out.WriteLine("------------------");

            // upload a pre-created .sd to create a new service

            Console.Out.WriteLine(@"Uploading C:\Presentations\Demo.sd..");

            var publishResponse = await ags.PublishServiceDefinition(new System.IO.FileInfo(@"C:\Presentations\Demo.sd"));
        }
    }
}

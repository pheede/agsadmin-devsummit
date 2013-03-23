// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace ArcGISRESTAdminUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private Timer timer;
        private ArcGISRESTAdmin.AGSClient ags;

        public MainWindow()
        {
            InitializeComponent();

            ags = new ArcGISRESTAdmin.AGSClient("http://philmbprowin.esri.com:6080/arcgis/admin/", "admin", "demopw");

            timer = new Timer(TimeSpan.FromSeconds(2).TotalMilliseconds);
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        async Task<IEnumerable<ServiceStatusWrapper>> GetServiceStatus()
        {
            var statuses = await ags.GetAllServiceReports();

            var res = from status in statuses.SelectMany(kv => kv.Value) select new ServiceStatusWrapper(status);

            return res;
        }

        async void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();

            var statuses = await GetServiceStatus();

            ServiceItems.Dispatcher.Invoke(() => { ServiceItems.ItemsSource = statuses; });

            timer.Start();
        }
    }
}

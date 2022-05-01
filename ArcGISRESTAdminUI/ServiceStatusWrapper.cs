using System.Windows.Media;
using ArcGISRESTAdmin.Classes;

namespace ArcGISRESTAdminUI
{
    public class ServiceStatusWrapper
    {
        private static Brush RedBrush = new SolidColorBrush(Colors.Red);
        private static Brush GreenBrush = new SolidColorBrush(Colors.Green);
        private static Brush BlackBrush = new SolidColorBrush(Colors.Black);

        private ServiceReport serviceReport { get; set; }

        public string serviceName { get { return serviceReport.folderName == "/" ? serviceReport.serviceName : serviceReport.folderName + "/" + serviceReport.serviceName; } }

        public Brush statusColor
        {
            get
            {
                switch(serviceReport.status.realTimeState)
                {
                    case ServiceStatus.Started: return GreenBrush;
                    case ServiceStatus.Stopped: return RedBrush;
                    default: return BlackBrush;
                }
            }
        }

        public ServiceStatusWrapper(ServiceReport serviceReport)
        {
            this.serviceReport = serviceReport;
        }

    }
}

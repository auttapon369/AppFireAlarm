using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Reflection;
using Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using LineAPI;




namespace AlarmCollector
{
    public partial class Form1 : Form
    {
        static string url = "http://localhost:19000";
        WebServiceHost _host;
        private IDisposable SignalR { get; set; }
        //ServiceHost svc;
        bool _isExit = false;
       
        public Form1()
        {            
            InitializeComponent();
            notifyIcon1.Text = "AlarmCollector V.1";
            
            AlarmWcfService.AlarmWcfService myService = new AlarmWcfService.AlarmWcfService();

            WebHttpBinding binNew = new WebHttpBinding();
            _host = new WebServiceHost(myService, new Uri(url));
            var endpoint = _host.AddServiceEndpoint(typeof(AlarmWcfService.IAlarmWcfService), binNew, "");
            endpoint.EndpointBehaviors.Add(new WebHttpBehavior());
            _host.Open();
            
            myService.OnGetData += myService_OnGetData;
            SignalR = WebApp.Start<SignalRStartup>("http://*:18000/");
            
            if (SignalR != null)
                textBox1.Text += "Server Start!!!\n";
            this.ShowInTaskbar = false;
            this.Opacity = 0;
          
        }

        void myService_OnGetData(object sender, AlarmWcfService.GetDataEventArgs e)
        {
            

            //Show Display Form
            textBox1.Text = e.Value;

            //SendDataToClient
             GlobalHost.ConnectionManager
             .GetHubContext<QueueMonitorServiceHub>()
             .Clients.All.writeMessage(e.Value);



        }
        void Form1_OnGetData(object sender, EventArgs e)
        {
            textBox1.Text += "OK ";
        }
        internal void WriteMessages(String message)
        {
            if (this.textBox1.InvokeRequired)
            {
                this.Invoke((Action)(() =>
                    WriteMessages(message)
                ));
                return;
            }
            textBox1.AppendText(message + Environment.NewLine);
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _isExit = true;
            Application.Exit();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_isExit)
            {
                this.Hide();
                e.Cancel = true;
                return;
            }
        }
        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Opacity = 100;
            this.Show();
        }
       
    }

    public class QueueMonitorServiceHub : Hub
    {
        public override Task OnConnected()
        {
            //Program Form1.WriteMessages("Client connected: " + Context.ConnectionId);
            return base.OnConnected();
        }
        /// <summary>
        /// Writes a message to the client that displays on the status bar
        /// </summary>
        public void StatusMessage(string message, bool allClients = false)
        {
            if (allClients)
                Clients.All.statusMessage(message);
            else
                Clients.Caller.statusMessage(message);
        }


        /// <summary>
        /// Context instance to access client connections to broadcast to
        /// </summary>
        public static IHubContext HubContext
        {
            get
            {
                if (_context == null)
                    _context = GlobalHost.ConnectionManager.GetHubContext<QueueMonitorServiceHub>();

                return _context;
            }
        }
        static IHubContext _context = null;


        /// <summary>
        /// Writes out message to all connected SignalR clients
        /// </summary>
        /// <param name="message"></param>
        public static void WriteMessage(string message)
        {
            // Write out message to SignalR clients  
            HubContext.Clients.All.writeMessage(message);
        }

        
    
    }
    public class SignalRStartup
    {
        public static IAppBuilder App = null;

        public void Configuration(IAppBuilder app)
        {
            app.Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);

                var hubConfiguration = new HubConfiguration
                {
                    EnableDetailedErrors = true,
                    EnableJSONP = true
                };

                map.RunSignalR(hubConfiguration);
            });

        }
    }
    public class AlarmData
            {
                public AlarmData(DateTime eventDateTime = new DateTime(), String eventType = "", String eventLocation = "", String eventClassification = "", String deviceMessage = "")
                {
                    EventDateTime = eventDateTime;
                    EventType = eventType;
                    EventLocation = eventLocation;
                    eventClassification = Classcification;
                    DeviceMessage = deviceMessage;
                }

                public DateTime EventDateTime
                {
                    get;
                    set;
                }

                public String EventType
                {
                    get;
                    set;
                }

                public String EventLocation
                {
                    get;
                    set;
                }

                public String Classcification
                {
                    get;
                    set;
                }

                public String DeviceMessage
                {
                    get;
                    set;
                }
            }
    
}

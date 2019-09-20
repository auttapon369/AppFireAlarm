using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Web;
using System.Windows.Forms;
using System.ServiceModel.Description;
using System.ServiceModel;


namespace AlarmCollector
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace AlarmWcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single,
        IncludeExceptionDetailInFaults=true)]
    public class AlarmWcfService : IAlarmWcfService
    {
        public string GetData(string value)
        {
            if (OnGetData != null)
                OnGetData(this, new GetDataEventArgs(value));
            return string.Format("{0}", value);
        }
        public string PostData(Stream data)
        {
            // convert Stream Data to StreamReader
            StreamReader reader = new StreamReader(data);

            // Read StreamReader data as string
            string value = reader.ReadToEnd();
            if (OnGetData != null)
                OnGetData(this, new GetDataEventArgs(value));
            return string.Format("{0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
           
            return composite;
        }

        public event GetDataEventHandler OnGetData;
    }

    public class GetDataEventArgs : EventArgs
    {
        public string Value { get; set; }
        public GetDataEventArgs(string value)
        {
            Value = value;
        }
    }

    public delegate void GetDataEventHandler(object sender, GetDataEventArgs e);
}

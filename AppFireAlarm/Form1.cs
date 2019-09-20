using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AppFireAlarm
{
    public partial class Form1 : Form
    {
        private IConnection _conn;
        private List<Byte> _packet = new List<byte>();
        private bool _isPacketStart = false;
        static string Estring="";
        public Form1()
        {
            InitializeComponent();

            _conn = new SerialPortConnection();
            _conn.Connect();
            _conn.ByteReceived += new ConnectionByteReceivedEventHandler(_conn_ByteReceived);
            
            
        }

        void SendAlarmData(string text_msg)
        {

            try
            {

                // Create a request using a URL that can receive a post.
                WebRequest request = WebRequest.Create("http://192.168.111.209:19000/PostData/Data");

                // Set the Method property of the request to POST.
                request.Method = "POST";
                // Create POST data and convert it to a byte array.
                //string postData = "This is a test that posts this string to a Web server.";
                string strData = text_msg;
                byte[] byteArray = Encoding.Default.GetBytes(strData);
                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/x-www-form-urlencoded";
                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;
                // Get the request stream.
                Stream dataStream;
                using (dataStream = request.GetRequestStream())
                {
                    // Write the data to the request stream.
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    // Close the Stream object.
                    dataStream.Close();

                    // Get the response.
                    WebResponse response = request.GetResponse();
                    // Display the status.
                    //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                    // Get the stream containing content returned by the server.
                    dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    using (StreamReader reader = new StreamReader(dataStream))
                    {
                        // Read the content.
                        string responseFromServer = reader.ReadToEnd();
                        // TODO Display the content.

                        // Clean up the streams.
                        reader.Close();
                        dataStream.Close();
                        response.Close();
                    }
                }
            }
            catch (Exception)
            {
            }
        }
       
        void _conn_ByteReceived(object sender, ConnectionByteReceivedEventArgs e)
        {
            if (e.ByteData == 0x02)
            {
                _isPacketStart = true;
                _packet.Clear();
            }
            else if (e.ByteData == 0x03)
            {
                _isPacketStart = false;
                // TODO: process packet
                string strPacket = Encoding.ASCII.GetString(_packet.ToArray());
                string[] dataLine = strPacket.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                EventDetail detail = new EventDetail();
                string[] dataFields = dataLine[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                detail.EventDateTime = DateTime.ParseExact(dataLine[0].Substring(0, 16), "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                detail.EventLocation = dataFields[2];
                detail.EventType = Regex.Replace(dataLine[1].Trim(), " {2,}", " ");
                detail.Classcification = dataLine[2].Trim();
                detail.DeviceMessage = dataLine[3].Trim();

                Estring = detail.EventDateTime.ToString()+",";
                Estring += detail.EventType + ",";
                Estring += detail.EventLocation+ ",";               
                Estring += detail.Classcification + ",";
                Estring += detail.DeviceMessage ;
                this.Invoke(new EventHandler(DisplayText));

                SendAlarmData(Estring);
            

            }
            else if (_isPacketStart)
            {
                _packet.Add(e.ByteData);
            }
        }

        private void DisplayText(object sender, EventArgs e)
        {
            textBox1.Text = Estring;
           
        }
      
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }
       
    }

    public class EventDetail
    {
        public EventDetail(DateTime eventDateTime = new DateTime(), String eventType = "", String eventLocation = "", String eventClassification = "", String deviceMessage = "")
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

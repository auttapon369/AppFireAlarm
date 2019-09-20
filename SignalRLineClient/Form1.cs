using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
﻿using Microsoft.AspNet.SignalR.Client;
using System.Net.Http;
using System.Data.SqlClient;
using LineAPI;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;


namespace SignalRLineClient
{
    public partial class Form1 : Form
    {
        LineClient _line;
        private String UserName { get; set; }
        private IHubProxy HubProxy { get; set; }
        const string ServerURI = "http://192.168.111.209:18000/signalr";
        private HubConnection Connection { get; set; } 
        public Form1()
        {
            try
            {
                _line = new LineClient("auttapon369@gmail.com", "30072528");
            }
            catch { }
            _line = new LineClient(_line.AuthToken);
            InitializeComponent();
            ConnectAsync();
        }
        private async void ConnectAsync()
        {
            Connection = new HubConnection(ServerURI);
            //Connection.Closed += Connection_Closed;
            HubProxy = Connection.CreateHubProxy("QueueMonitorServiceHub");
            //Handle incoming event from server: use Invoke to write to console from SignalR's thread 
            HubProxy.On<string>("WriteMessage", (message) =>
            this.Invoke((Action)(() =>SendLine(message))));

            HubProxy.On<string>("WriteMessage", (message) =>
                this.Invoke((Action)(() =>new SendSMS().Send(message))));

            try
            {
                await Connection.Start();
            }
            catch (HttpRequestException)
            {
                //StatusText.Text = "Unable to connect to server: Start server before connecting clients.";
                //No connection: Don't enable Send button or show chat UI 
                return;
            }

        }

        //private void InserDB(string msg)
        //{
        //    string[] data = msg.Split(',');
        //    string sql = "Insert into [dbo].[AlarmData](EventDateTime,EventType,EventLocation,eventClassification,DeviceMessage) " +
        //           " Values(@EventDateTime,@EventType,@EventLocation,@eventClassification,@DeviceMessage) ";
        //    SqlParameterCollection param2 = new SqlCommand().Parameters;
        //    param2.AddWithValue("EventDateTime", SqlDbType.DateTime).Value = DateTime.Parse(data[0]);
        //    param2.AddWithValue("EventType", SqlDbType.VarChar).Value = data[1];
        //    param2.AddWithValue("EventLocation", SqlDbType.VarChar).Value = data[2];
        //    param2.AddWithValue("eventClassification", SqlDbType.VarChar).Value = data[3];
        //    param2.AddWithValue("DeviceMessage", SqlDbType.VarChar).Value = data[4];
        //    int i2 = new DbClass().ExecuteData(sql, param2);
        //}

        void SendLine(string message)
        {
            string[] data = message.Split(',');        
            string msg = data[0]+"\n"+data[2]+"\n"+data[1]+"\n"+data[3]+"\n"+data[4];
       

            string sql = "Select * from dbo.AlarmLine";
            DataSet ds = new DbClass().SelectData(sql, "AlarmLine");
            foreach (DataRow r in ds.Tables["AlarmLine"].Rows)
            {
                Contact contact = _line.Client.findContactByUserid(r["LineID"].ToString());
                _line.sendMessage(contact.Mid, msg);
            }

        }
      
       
    }
 
    
    public class SendSMS
    {
       
       public void Send(string message)
        {
            string[] data = message.Split(',');
            string msg = data[0] + "\n" + data[2] + "\n" + data[1] + "\n" + data[3] + "\n" + data[4];

            if (data[1] == "Alarm On" || data[1]=="Alert On")
            {
                string sql = "Select * from dbo.AlarmSMS";
                DataSet ds = new DbClass().SelectData(sql, "AlarmSMS");
                foreach (DataRow r in ds.Tables["AlarmSMS"].Rows)
                {
                sms(msg,r["Phone"].ToString().Trim());
                }
            }
        }
        void sms(string text_msg, string phone)
        {


            string strMessageX = text_msg;
            string strRecipient = phone.Trim();
            string strSMSAccount = "scadask";
            string strSMSPassword = "skscada";
            string strLang = "E";

            try
            {

                // Create a request using a URL that can receive a post.
                WebRequest request = WebRequest.Create("http://smsgateway.applymail.com/api/send.php");
                // Set the Method property of the request to POST.
                request.Method = "POST";
                // Create POST data and convert it to a byte array.
                //string postData = "This is a test that posts this string to a Web server.";
                string strData = "msisdn=" + strRecipient + "&user=" + strSMSAccount + "&pass=" + strSMSPassword + "&lang=" + strLang + "&msg=" + strMessageX;
                byte[] byteArray = Encoding.Default.GetBytes(strData);
                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/x-www-form-urlencoded";
                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;
                // Get the request stream.
                Stream dataStream = request.GetRequestStream();
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
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                // Display the content.
                string[] arr1 = Regex.Split(responseFromServer, "<DETAIL>");
                string[] arr2 = Regex.Split(arr1[1], "</DETAIL>");
                string val = null;
                if (arr2[0] == "") { val = "OK"; }
                else { val = arr2[0]; }
                // Clean up the streams.
                reader.Close();
                dataStream.Close();
                response.Close();
                //return val;
            }
            catch (Exception)
            { //return "SMS Not work"; 
            }

        }      
    }
    public class ConnectDB
    {

        public ConnectDB()
        {
            //
            // TODO: Add constructor logic here
            //

        }

        private string strConnection()
        {
            return "Server=192.168.5.22;Database=FireAlarm" +
            ";User ID=sa;Password=ata+ee&c";
        }

        public SqlConnection connection()
        {
            SqlConnection conn = new SqlConnection(strConnection());
            return conn;
        }
    }
    public class DbClass
    {
        public DbClass()
        {
            //
            // TODO: Add constructor logic here
            //
        }


        public int ExecuteData(string sql)
        {
            int i = 0;
            SqlConnection conn = new ConnectDB().connection();
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                try
                {
                    conn.Open();
                }
                catch (Exception)
                {
                    return i;
                }

                i = cmd.ExecuteNonQuery();
            }
            conn.Close();
            return i;
        }

        public int ExecuteData(string sql, SqlParameterCollection parameters)
        {
            int i = 0;
            SqlConnection conn = new ConnectDB().connection();

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                foreach (SqlParameter param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.ParameterName, param.SqlDbType).Value = param.Value;
                }
                try
                {
                    conn.Open();
                }
                catch (Exception)
                {
                    return i;
                }
                i = cmd.ExecuteNonQuery();

            }
            conn.Close();
            return i;
        }

        public DataSet SelectData(string sql, string tblName)
        {
            SqlConnection conn = new ConnectDB().connection();
            SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            DataSet ds = new DataSet();
            da.Fill(ds, tblName);
            return ds;
        }
        public DataSet SelectData(string sql, string tblName, SqlParameterCollection parameters)
        {
            SqlConnection conn = new ConnectDB().connection();
            SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            DataSet ds = new DataSet();
            foreach (SqlParameter param in parameters)
            {
                da.SelectCommand.Parameters.AddWithValue(param.ParameterName, param.SqlDbType).Value = param.Value;
            }
            da.Fill(ds, tblName);
            return ds;
        }

        /* การใช้งาน Class SQL
        //อ่านข้อมูลจาก Database แบบไม่มี Parameters
        string sql1 = "Select * from Employees";
        DataSet dsEmp1 = new DBClass().SqlGet(sql1,"tblEmployee");

        //อ่านข้อมูล แบบใช้ parameters
        string sql2 = "select * from Employees where empId=@empId";
        SqlParameterCollection param = new SqlCommand().Parameters;
        param.AddWithValue("empId",SqlDbType.Int).Value = 1;
        DataSet dsEmp2 = new DBClass().SqlGet(sql2, "tblEmployee", param);

        //Insert,delete,update
        string sql3 = "Insert into Employees(empId,empName) " +
                " Values(1,'pheak')";
        int i = new DBClass().SqlExecute(sql3);

        //Insert,delete,update แบบใช้ Parameters
        string sql4 = "Insert into Employees(empId,empName) " +
            " Values(@empId,@empName) ";
        SqlParameterCollection param2 = new SqlCommand().Parameters;
        param2.AddWithValue("empId", SqlDbType.Int).Value = 1;
        param2.AddWithValue("empName",SqlDbType.VarChar).Value = "pheak";
        int i2 = new DBClass().SqlExecute(sql4, param2);
         */
    }
}

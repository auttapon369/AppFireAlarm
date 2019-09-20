using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Data.SqlClient;

namespace SignalRInserDBClient
{
    public partial class Form1 : Form
    {
        bool _isExit = false;
        private String UserName { get; set; }
        private IHubProxy HubProxy { get; set; }
        const string ServerURI = "http://192.168.111.209:18000/signalr";
        private HubConnection Connection { get; set; }
        public Form1()
        {
            
            InitializeComponent();
            ConnectAsync();
            this.ShowInTaskbar = false;
            this.Opacity = 0;
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
        private async void ConnectAsync()
        {
            Connection = new HubConnection(ServerURI);
            //Connection.Closed += Connection_Closed;
            HubProxy = Connection.CreateHubProxy("QueueMonitorServiceHub");
            //Handle incoming event from server: use Invoke to write to console from SignalR's thread 
            HubProxy.On<string>("WriteMessage", (message) =>
                this.Invoke((Action)(() => InserDB(message)

                ))
            );
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

        private void InserDB(string msg)
        {
            string[] data = msg.Split(',');
            string sql = "Insert into [dbo].[AlarmData](EventDateTime,EventType,EventLocation,eventClassification,DeviceMessage) " +
                   " Values(@EventDateTime,@EventType,@EventLocation,@eventClassification,@DeviceMessage) ";
            SqlParameterCollection param2 = new SqlCommand().Parameters;
            param2.AddWithValue("EventDateTime", SqlDbType.DateTime).Value = DateTime.Parse(data[0]);
            param2.AddWithValue("EventType", SqlDbType.VarChar).Value = data[1];
            param2.AddWithValue("EventLocation", SqlDbType.VarChar).Value = data[2];
            param2.AddWithValue("eventClassification", SqlDbType.VarChar).Value = data[3];
            param2.AddWithValue("DeviceMessage", SqlDbType.VarChar).Value = data[4];
            int i2 = new DbClass().ExecuteData(sql, param2);
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
            return "Server=192.168.111.30;Database=FireAlarm" +
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using System.IO;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo.Agent;
using Microsoft.Win32.TaskScheduler;
using System.Net;
using System.Globalization;


namespace BP_Automation
{
    public partial class Form1 : Form
    {
        private PerformanceCounter theCPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private PerformanceCounter theMemCounter = new PerformanceCounter("Memory", "Available MBytes");
        public Form1()
        {
            InitializeComponent();
        }

        
        private void CPU()
        {
            CPULabel.Text = this.theCPUCounter.NextValue().ToString();

            AvailLabel.Text = this.theMemCounter.NextValue().ToString() + "MB";
            
                StreamWriter log;

                if (!File.Exists("Log.txt"))
                {
                    log = new StreamWriter("Log.txt");
                }
                else
                {
                    log = File.AppendText(@"C:\Backup\Log.txt");
                }
                log.WriteLine(DateTime.Now +  "\t"  + "Hike in CPU Usage: " + CPULabel.Text + "%");
                log.WriteLine( DateTime.Now+ "\t" + "AvailableMB: " + AvailLabel.Text + "MB");
                log.Flush();
                log.Close();
            
        }
       
        private void Form1_Load(object sender, EventArgs e)
        {
            HostName();
        }

        private void HostName()
        {
            string hostName = Dns.GetHostName();

            if (hostName=="BP1XGBAP1662")
                {
                    BP1XGBAP1662();
                 }
               else if(hostName=="BP1XGBAP1599")
                {
                    BP1XGBAP1599();
                }
               else if(hostName=="BP1XGBII078")
                {
                    BP1XGBII078();
                 }
                else if(hostName=="BP1XGBDB267")
                {
                     BP1XGBDB267();
                }
                else if(hostName == "BP1XGBDB268")
                {
                    BP1XGBDB268();
                }
        }
        private void Service()
        {
         
            ServiceController ctrl = new ServiceController(ServiceList.Text);
            ServiceDisplayName.Text = ctrl.DisplayName;
            ServiceType.Text = ctrl.ServiceType.ToString();
            ServiceStatus.Text = ctrl.Status.ToString();
            StreamWriter log;
            String TodayDate = DateTime.Now.ToString("MM/dd/yyyy");
            if (!File.Exists("Log"+"TodayDate"+".txt"))
            {
                log = new StreamWriter("Log" + "TodayDate" + ".txt");
            }
            else
            {
                log = File.AppendText(@"E:\BPAutomation\Log.txt");
            }
            log.WriteLine(DateTime.Now + "\t" + "Service Name: " + ServiceDisplayName.Text +"\t"+"Status "+ ctrl.Status.ToString());
            log.Flush();
            log.Close();
        }

            

                //Function to check the drive space
        private void myDrive()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                textBox3.Text = "Drive Name: " + d.Name;
                textBox3.AppendText("Available Free Space for users: " + Convert.ToString((d.AvailableFreeSpace)/(1024*1024*1024)) + " GB");
                StreamWriter log;
                String TodayDate = DateTime.Now.ToString("MM/dd/yyyy");
                if (!File.Exists("Log" + "TodayDate" + ".txt"))
                {
                    log = new StreamWriter("Log" + "TodayDate" + ".txt");
                }
                else
                {
                    log = File.AppendText(@"C:\Backup\Log.txt");
                }
                log.WriteLine(DateTime.Now + "\t" + "Drive Name: " + d.Name +"\t"+ "Available Free Space " + Convert.ToString((d.AvailableFreeSpace) / (1024 * 1024 * 1024)) + " GB");
                log.Flush();
                log.Close();
                String Connection = ConfigurationManager.ConnectionStrings["Dbconnection"].ConnectionString;
                SqlConnection conn = new SqlConnection(Connection);
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_BP_Diskdetails";
                cmd.Parameters.Add("@DiskName", SqlDbType.VarChar).Value = d.Name;
                cmd.Parameters.Add("@DiskSize", SqlDbType.VarChar).Value = Convert.ToString((d.AvailableFreeSpace) / (1024 * 1024 * 1024));
                cmd.Parameters.Add("Date", SqlDbType.DateTime).Value = DateTime.Now.ToString("MM/dd/yy H:mm");
                conn.Open();
                cmd.ExecuteNonQuery();
                //result = Convert.ToInt32(cmd.Parameters["@ReturnPara"]);
                conn.Close();

            }
        }


        static readonly string SqlServer = @"BDC4-D-4697N62";
        //static readonly string conn = "Data Source=BDC4B-D-75CMX52;Initial Catalog=msdb;Integrated Security=True";
        private void sqlJob()
        {
            textBox4.Text = "";
           ServerConnection conn = new ServerConnection(SqlServer);
            Server server = new Server(conn);
            JobCollection jobs = server.JobServer.Jobs;
            foreach (Job job in jobs)
            {

                textBox4.AppendText(job.Name + "" + job.LastRunOutcome);
                //textBox4.AppendText("Last Run staus" +job.LastRunOutcome);

                StreamWriter log;
                String TodayDate = DateTime.Now.ToString("MM/dd/yyyy");
                if (!File.Exists("Log" + "TodayDate" + ".txt"))
                {
                    log = new StreamWriter("Log" + "TodayDate" + ".txt");
                }
                else
                {
                    log = File.AppendText(@"C:\Backup\Log.txt");
                }
                log.WriteLine(DateTime.Now + "\t" + "Job Name " + job.Name + "\t" + "Last Run staus" + job.LastRunOutcome + "\t" + "JOb last run date" + job.LastRunDate);
                log.Flush();
                log.Close();
                String Connection = ConfigurationManager.ConnectionStrings["Dbconnection"].ConnectionString;
                SqlConnection conn1 = new SqlConnection(Connection);
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn1;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_BP_BackupDetails";
                cmd.Parameters.Add("@JobName", SqlDbType.VarChar).Value = job.Name;
                cmd.Parameters.Add("@LastJobRanStatus", SqlDbType.VarChar).Value = job.LastRunOutcome;
                cmd.Parameters.Add("@LastJobRanDate", SqlDbType.DateTime).Value = job.LastRunDate;
                cmd.Parameters.Add("Date", SqlDbType.DateTime).Value = DateTime.Now.ToString("MM/dd/yy H:mm");
                conn1.Open();
                cmd.ExecuteNonQuery();
                conn1.Close();
            }

        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            errorLog();

        }
        private void errorLog()
        {
            StreamReader sr = new StreamReader(ConfigurationManager.AppSettings["FILEPATH"]);
            string line = string.Empty;
            error_Box.Text = "";
            try
            {
                //Read the first line of text
                line = sr.ReadLine();

                //Continue to read until you reach end of file
                while (line != null && !sr.EndOfStream)
                {

                    //this.listBox1.Items.Add(line);
                    //Read the next line
                    //line = sr.ReadLine();
                    if (line.ToLower().Contains(searchText.Text.ToLower()))
                    {
                        error_Box.Text = ("yes");
                        error_Box.BackColor = System.Drawing.Color.Red;
                        break;
                    }
                    line = sr.ReadLine();
                }
                error_Box.Text = error_Box.Text == "" ? "no" : error_Box.Text;
                if (error_Box.Text == "no")
                { error_Box.BackColor = System.Drawing.Color.Green; }
                //close the file
                sr.Close();
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message.ToString());
            }
            finally
            {
                //close the file
                sr.Close();
            }
        }
        private void InsertvalueBP1XGBAP1662()
        {
            // int result = -1;
            String Connection = ConfigurationManager.ConnectionStrings["Dbconnection"].ConnectionString;
            SqlConnection conn = new SqlConnection(Connection);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection= conn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "usp_BP_InsertBP1XGBAP1662";
 
            cmd.Parameters.Add("@CPU", SqlDbType.Int).Value = Convert.ToInt32(CPULabel.Text);
            cmd.Parameters.Add("@CPUMemory", SqlDbType.NVarChar).Value = AvailLabel.Text;
            cmd.Parameters.Add("@ServiceName", SqlDbType.VarChar).Value = ServiceList.Text;
            cmd.Parameters.Add("@ServiceDisplayName", SqlDbType.VarChar).Value = ServiceDisplayName.Text;
            cmd.Parameters.Add("@ServiceStatus", SqlDbType.VarChar).Value = ServiceStatus.Text;
            cmd.Parameters.Add("@LogError", SqlDbType.VarChar).Value = error_Box.Text;          
            cmd.Parameters.Add("Date", SqlDbType.DateTime).Value = DateTime.Now.ToString("MM/dd/yy H:mm");
            //cmd.Parameters.Add("@returnpara", SqlDbType.Int).Direction = ParameterDirection.Output;
            conn.Open();
            cmd.ExecuteNonQuery();
           //result = Convert.ToInt32(cmd.Parameters["@ReturnPara"]);
            conn.Close();

        }
        private void LPIerrorlog()
        {
            DateTime inputDate = DateTime.Now;

            var d = inputDate;
            CultureInfo cul = CultureInfo.CurrentCulture;

            var firstDayWeek = cul.Calendar.GetWeekOfYear(
            d,
            CalendarWeekRule.FirstDay,
            DayOfWeek.Monday);

            int weekNum = cul.Calendar.GetWeekOfYear(
            d,
            CalendarWeekRule.FirstDay,
            DayOfWeek.Monday);

            StreamReader sr = new StreamReader(ConfigurationManager.AppSettings["FILEPATH"+weekNum]);
            string line = string.Empty;
            error_Box.Text = "";
            try
            {
                //Read the first line of text
                line = sr.ReadLine();

                //Continue to read until you reach end of file
                while (line != null && !sr.EndOfStream)
                {

                    //this.listBox1.Items.Add(line);
                    //Read the next line
                    //line = sr.ReadLine();
                    if (line.ToLower().Contains(searchText.Text.ToLower()))
                    {
                        error_Box.Text = ("yes");
                        error_Box.BackColor = System.Drawing.Color.Red;
                        break;
                    }
                    line = sr.ReadLine();
                }
                error_Box.Text = error_Box.Text == "" ? "no" : error_Box.Text;
                if (error_Box.Text == "no")
                { error_Box.BackColor = System.Drawing.Color.Green; }
                //close the file
                sr.Close();
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message.ToString());
            }
            finally
            {
                //close the file
                sr.Close();
            }
        }
        private void InsertvalueBP1XGBAP1599()
        {
            // int result = -1;
            String Connection = ConfigurationManager.ConnectionStrings["Dbconnection"].ConnectionString;
            SqlConnection conn = new SqlConnection(Connection);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "usp_BP_InsertBP1XGBAP1599";

            cmd.Parameters.Add("@CPU", SqlDbType.Int).Value = Convert.ToInt32(CPULabel.Text);
            cmd.Parameters.Add("@CPUMemory", SqlDbType.NVarChar).Value = AvailLabel.Text;
            cmd.Parameters.Add("@LogError", SqlDbType.VarChar).Value = error_Box.Text;
            cmd.Parameters.Add("Date", SqlDbType.DateTime).Value = DateTime.Now.ToString("MM/dd/yy H:mm");
            //cmd.Parameters.Add("@returnpara", SqlDbType.Int).Direction = ParameterDirection.Output;
            conn.Open();
            cmd.ExecuteNonQuery();
            //result = Convert.ToInt32(cmd.Parameters["@ReturnPara"]);
            conn.Close();

        }
        private void InsertvalueBP1XGBII078()
        {
            // int result = -1;
            String Connection = ConfigurationManager.ConnectionStrings["Dbconnection"].ConnectionString;
            SqlConnection conn = new SqlConnection(Connection);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "usp_BP_InsertBP1XGBII078";

            cmd.Parameters.Add("@CPU", SqlDbType.Int).Value = Convert.ToInt32(CPULabel.Text);
            cmd.Parameters.Add("@CPUMemory", SqlDbType.NVarChar).Value = AvailLabel.Text;
            cmd.Parameters.Add("Date", SqlDbType.DateTime).Value = DateTime.Now.ToString("MM/dd/yy H:mm");
            //cmd.Parameters.Add("@returnpara", SqlDbType.Int).Direction = ParameterDirection.Output;
            conn.Open();
            cmd.ExecuteNonQuery();
            //result = Convert.ToInt32(cmd.Parameters["@ReturnPara"]);
            conn.Close();

        }
        private void InsertvalueBP1XGBDB267()
        {
            {
                string fileName = "ChemmateTest";
                {
                    string a = ConfigurationManager.AppSettings["ChemmatePath"] + fileName;
                    if (File.Exists(a))
                    {
                        long size = (new System.IO.FileInfo(a).Length) / (1024 * 1024);
                        BackupSize.Text = Convert.ToString(size) + "MB";

                    }
                    else
                    {
                        BackupSize.Text = "nobackup";
                    }
                }
                // int result = -1;
                String Connection = ConfigurationManager.ConnectionStrings["Dbconnection"].ConnectionString;
                SqlConnection conn = new SqlConnection(Connection);
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_BP_InsertBP1XGBDB267";

                cmd.Parameters.Add("@CPU", SqlDbType.Int).Value = Convert.ToInt32(CPULabel.Text);
                cmd.Parameters.Add("@CPUMemory", SqlDbType.NVarChar).Value = AvailLabel.Text;
                cmd.Parameters.Add("@BackupName", SqlDbType.NVarChar).Value = "ChemmateTest";
                cmd.Parameters.Add("@BackupSize", SqlDbType.NVarChar).Value = BackupSize.Text;
                cmd.Parameters.Add("Date", SqlDbType.DateTime).Value = DateTime.Now.ToString("MM/dd/yy H:mm");
                //cmd.Parameters.Add("@returnpara", SqlDbType.Int).Direction = ParameterDirection.Output;
                conn.Open();
                cmd.ExecuteNonQuery();
                //result = Convert.ToInt32(cmd.Parameters["@ReturnPara"]);
                conn.Close();

            }
        }
            private void InsertvalueBP1XGBDB268()
        {
            string fileName = "CPW";
            {
                string a = ConfigurationManager.AppSettings["ChemmatePath"] + fileName;
                if (File.Exists(a))
                {
                    long size = (new System.IO.FileInfo(a).Length) / (1024 * 1024);
                    BackupSize.Text = Convert.ToString(size) + "MB";

                }
                else
                {
                    BackupSize.Text = "nobackup";
                }
            }
            // int result = -1;
            String Connection = ConfigurationManager.ConnectionStrings["Dbconnection"].ConnectionString;
            SqlConnection conn = new SqlConnection(Connection);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "usp_BP_InsertBP1XGBDB268";

            cmd.Parameters.Add("@CPU", SqlDbType.Int).Value = Convert.ToInt32(CPULabel.Text);
            cmd.Parameters.Add("@CPUMemory", SqlDbType.NVarChar).Value = AvailLabel.Text;
            cmd.Parameters.Add("@BackupName", SqlDbType.NVarChar).Value = "CPW";
            cmd.Parameters.Add("@BackupSize", SqlDbType.NVarChar).Value = BackupSize.Text;
            cmd.Parameters.Add("Date", SqlDbType.DateTime).Value = DateTime.Now.ToString("MM/dd/yy H:mm");
            //cmd.Parameters.Add("@returnpara", SqlDbType.Int).Direction = ParameterDirection.Output;
            conn.Open();
            cmd.ExecuteNonQuery();
            //result = Convert.ToInt32(cmd.Parameters["@ReturnPara"]);
            conn.Close();

        }





        private void BP1XGBAP1662()
        {
            CPU();
            myDrive();
            Service();
            errorLog();
            InsertvalueBP1XGBAP1662();

        }

            private void BP1XGBAP1599()
        {
            CPU();
            myDrive();
            LPIerrorlog();
            InsertvalueBP1XGBAP1599();
        }

        private void BP1XGBII078()
        {
            CPU();
            myDrive();
            InsertvalueBP1XGBII078();
        }

        private void BP1XGBDB267()
        {
            CPU();
            myDrive();
            sqlJob();
            
            InsertvalueBP1XGBDB267();
        }

        private void BP1XGBDB268()
        {
            CPU();
            myDrive();
            sqlJob();
            InsertvalueBP1XGBDB268();


        }


}

}


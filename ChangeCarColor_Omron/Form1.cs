using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HslCommunication.Profinet.Omron;
using HslCommunication.Profinet.Melsec;
using HslCommunication;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Data.SqlClient;

namespace ChangeCarColor_Omron
{
    public partial class Form1 : Form
    {
        public MelsecMcNet omronFinsNet = null;
        public bool b_ReadPLCData = false; //讀PLC資料 開關boolean
        string tmpNIMSBOTagData = "";
        string tmpLINEINTagData = "";
        string tmpNIMSWITagData = "";
        string tmpNIMSBITagData = "";
        string tmpC71WITagData = "";
        string tmpNIMSBO2TagData = "";
        string _connectionString =
"Persist Security Info=False;User ID=rco;Password=$rcopwd;Initial Catalog=YLRCO;Server=YLRCOS04;TrustServerCertificate=True;Encrypt=True;";
        public Form1()
        {
            InitializeComponent();

            omronFinsNet = new MelsecMcNet("10.202.51.5", 5000); //請輸入 PLC IP位址及Port
            omronFinsNet.ConnectTimeOut = 2000; // 網路连接的超时时间
            omronFinsNet.NetworkNumber = 0x00;  // 網路号
            omronFinsNet.NetworkStationNumber = 0x00; // 網路站号

            omronFinsNet.ConnectTimeOut = 500; //(固定)
            omronFinsNet.ReceiveTimeOut = 500; //(固定)
            OperateResult connect = omronFinsNet.ConnectServer();
            Thread.Sleep(1000); //Thread.Sleep 函數來使程式等待一段時間
            if (connect.IsSuccess) 
            { 
                pictureBox1.Image = Properties.Resources.Green; // PLC通訊成功
                //Task ReadPLCData = Task.Run(() => SyncPLCData());
                //b_ReadPLCData = true;
            }
            else
            { 
                pictureBox1.Image = Properties.Resources.Red; // PLC通訊失敗
            }   
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Interval = 1000;//查詢PLC資料頻率，單位ms
            timer2.Interval = 1000;//檢察PLC連線頻率，單位ms (*20)
        }

        int i = 0;

        public void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox14.BackColor = Color.Green;

            PLC_value_Get(); //讀取PLC車色數據，單次批量讀取

            pictureBox14.BackColor = Color.Red;
        }
        private void PLC_value_Get() //讀取PLC車色數據，單次批量讀取
        {

            try
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();//計時開始


                textBox2.Text = Convert.ToString(tmpNIMSBOTagData);
                textBox3.Text = Convert.ToString(tmpLINEINTagData);
                textBox4.Text = Convert.ToString(tmpNIMSWITagData);
                textBox5.Text = Convert.ToString(tmpNIMSBITagData);
                textBox6.Text = Convert.ToString(tmpC71WITagData);
                textBox7.Text = Convert.ToString(tmpNIMSBO2TagData);
                // OperateResult<byte[]> read = omronFinsNet.Read("D11120", 2); //PLC讀取起始位及讀取數量
                //{
                //    if (read.IsSuccess)
                //    {
                //        UInt16 D11100 = omronFinsNet.ByteTransform.TransUInt16(read.Content, 0);
                //        //        UInt16 D11101 = omronFinsNet.ByteTransform.TransUInt16(read.Content, 2);
                //        //        UInt16 D11102 = omronFinsNet.ByteTransform.TransUInt16(read.Content, 4);
                //        //        UInt16 D11103 = omronFinsNet.ByteTransform.TransUInt16(read.Content, 6);
                //        //        UInt16 D11104 = omronFinsNet.ByteTransform.TransUInt16(read.Content, 8);
                //        //        UInt16 D11105 = omronFinsNet.ByteTransform.TransUInt16(read.Content, 10);
                //        //        UInt16 D11106 = omronFinsNet.ByteTransform.TransUInt16(read.Content, 12);
                //        //        UInt16 D11107 = omronFinsNet.ByteTransform.TransUInt16(read.Content, 14);
                //        //        UInt16 D11108 = omronFinsNet.ByteTransform.TransUInt16(read.Content, 16);
                //        //        UInt16 D11109 = omronFinsNet.ByteTransform.TransUInt16(read.Content, 18);

                //        textBox2.Text = Convert.ToString(D11100);
                //        //        textBox3.Text = Convert.ToString(D11101);
                //        //        textBox4.Text = Convert.ToString(D11102);
                //        //        textBox5.Text = Convert.ToString(D11103);
                //        //        textBox6.Text = Convert.ToString(D11104);
                //        //        textBox7.Text = Convert.ToString(D11105);
                //        //        textBox8.Text = Convert.ToString(D11106);
                //        //        textBox9.Text = Convert.ToString(D11107);
                //        //        textBox10.Text = Convert.ToString(D11108);
                //        //        textBox11.Text = Convert.ToString(D11109);
                //        //        if (D11100 == 1) backgroundWorker1.RunWorkerAsync();// D11100位置為 "1" 時，查詢ALC。
                //        //        if (D11101 == 1) backgroundWorker2.RunWorkerAsync();// D11101位置為 "1" 時，寫入SQL。
                //    }
                //    else
                //        StreamWriterMethod("PLC資料讀取失敗");
                //}
                stopWatch.Stop();//計時結束
                TimeSpan s = stopWatch.Elapsed;//時間轉換
                double t = s.TotalMilliseconds;//時間轉換
                textBox1.Text = Convert.ToString(t);//時間顯示
            }
            catch (Exception ex) { StreamWriterMethod("PLC_value_Get Error : " + ex.ToString()); }
        }
        public void timer2_Tick(object sender, EventArgs e)
        {
            i++;
            if (i == 20)//20次查詢PLC連線撞況
            {
                PLC_Connection(); //PLC連線狀態顯示
                i = 0;
            }
        }
        private void PLC_Connection() //判斷PLC連線訊號正異常
        {
            OperateResult<byte[]> read = omronFinsNet.Read("D0", 1);
            {
                if (read.IsSuccess)
                {
                    pictureBox1.Image = Properties.Resources.Green; // success
                    StreamWriterMethod("PLC連線正常");
                    timer1.Enabled = true;
                }
                else
                {
                    pictureBox1.Image = Properties.Resources.Red; // failed
                    StreamWriterMethod("PLC連線中斷");
                    timer1.Enabled = false;
                }
            }
        }
        private void button000_Click(object sender, EventArgs e)//啟動PLC連線
        {
            timer2.Enabled = true;          
        }
        private void button00_Click(object sender, EventArgs e)
        {
            omronFinsNet.Write("D11100", 0);//對特定PLC位置寫入資料
        }

        private void button01_Click(object sender, EventArgs e)
        {
            omronFinsNet.Write("D500", 1);//對特定PLC位置寫入資料
        }
       
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) //背景執行緒
        {
           // Queryfromalc("123");//ALC查詢條件
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            //Updatetosql("6KT","B17");//寫入SQL資料
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        ALCDateDataContext alc = new ALCDateDataContext();
        DataBaseDataContext db1 = new DataBaseDataContext();      

        string[] Queryfromalc(string tagno)//查詢SQL
        {
            string[] backdata = new string [2];
            try
            {
                
                var result1 = from data in alc.WBS_link_tag_view //查詢列表名稱
                              where data.tag_no == tagno //查詢條件
                              select data; //顯示全部

                if (result1.ToArray().Length != 0) //查詢有資料條件下 
                {
                    foreach (var s in result1)
                    {
                        backdata[0] = s.colr;
                        backdata[1] = s.modl.Substring(0,3);
                    }
                }
            }
            catch(Exception ex) { StreamWriterMethod("Queryfromsql Error : " + ex.ToString()); }
            return backdata;
        }
        void Updatetosql(string tagno,string Color) //寫入SQL
        {
            try
            {     
                //寫入內容
                var newWBSList = new WBSList
                {
                    tag_no = tagno,
                    Color = Color
                };

                db1.WBSList.InsertOnSubmit(newWBSList);
                db1.SubmitChanges();
                //LOG顯示寫入內容
                StreamWriterMethod("Updatetosql : " + tagno + "," + Color);
            }
            catch (Exception ex) { StreamWriterMethod("Updatetosql Error : " + ex.ToString()); }
        }
        private async void SyncPLCData()
        {
            while (b_ReadPLCData)
            {
                try
                {
                    tmpNIMSBOTagData = omronFinsNet.ReadString("D322", 2).Content;
                    if (tmpNIMSBOTagData != null)
                    {
                        if (!tmpNIMSBOTagData.Trim().Equals("") && !tmpNIMSBOTagData.Trim().Equals("$TMO"))
                        {
                            WriteTagTable(tmpNIMSBOTagData, "BSI02");
                        }
                    }

                    tmpLINEINTagData = omronFinsNet.ReadString("D302", 2).Content;
                    if (tmpLINEINTagData != null)
                    {
                        if (!tmpLINEINTagData.Trim().Equals("") && !tmpLINEINTagData.Trim().Equals("$TMO"))
                        {
                            WriteTagTable(tmpLINEINTagData, "BSI00");
                        }
                    }

                    tmpNIMSWITagData = omronFinsNet.ReadString("D182", 2).Content;
                    if (tmpNIMSWITagData != null)
                    {
                        if (!tmpNIMSWITagData.Trim().Equals("") && !tmpNIMSWITagData.Trim().Equals("$TMO"))
                        {
                            WriteTagTable(tmpNIMSWITagData, "BSE01");
                        }
                    }

                    tmpNIMSBITagData = omronFinsNet.ReadString("D282", 2).Content;
                    if (tmpNIMSBITagData != null)
                    {
                        if (!tmpNIMSBITagData.Trim().Equals("") && !tmpNIMSBITagData.Trim().Equals("$TMO"))
                        {
                            WriteTagTable(tmpNIMSBITagData, "BSE04");
                        }
                    }

                    tmpC71WITagData = omronFinsNet.ReadString("D122", 2).Content;
                    if (tmpC71WITagData != null)
                    {
                        if (!tmpC71WITagData.Trim().Equals("") && !tmpC71WITagData.Trim().Equals("$TMO"))
                        {
                            WriteTagTable(tmpC71WITagData, "BSB02");
                        }
                    }
                    tmpNIMSBO2TagData = omronFinsNet.ReadString("D422", 2).Content;
                    if (tmpNIMSBO2TagData != null)
                    {
                        if (!tmpNIMSBO2TagData.Trim().Equals("") && !tmpNIMSBO2TagData.Trim().Equals("$TMO"))
                        {
                            WriteTagTable(tmpNIMSBO2TagData, "BSI02_2");
                        }
                    }

                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
                await Task.Delay(200);


            }
        }
        private void WriteTagTable(string tag,string station)
        {
            try
            {
                string query = string.Format("IF NOT EXISTS(SELECT * FROM C_Sharp_Tag WHERE tag_no = '" + tag + "' and station = '" + station + "')" +
                "BEGIN INSERT INTO C_Sharp_Tag(timestamp,station, tag_no)" +
                "VALUES(@P1,@P2,@P3) END");
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@P1", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                    cmd.Parameters.AddWithValue("@P2", station);
                    cmd.Parameters.AddWithValue("@P3", tag);
                    //cmd.Parameters.AddWithValue("@P4", tag);
                    //cmd.Parameters.AddWithValue("@P5", station);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public void StreamWriterMethod(string str) //LOG紀錄
        {
            string filepath = Environment.CurrentDirectory + "\\Logfile\\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            using (StreamWriter sw = File.AppendText(filepath)) //追加寫法
            {
                sw.WriteLine(DateTime.Now.ToString() + " - " + str);
            }
            Console.WriteLine(str);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            b_ReadPLCData = false;
        }
    }
}

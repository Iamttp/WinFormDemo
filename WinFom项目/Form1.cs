using Microsoft.Win32;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;



namespace WinFom项目
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;//设置该属性 为false
        }

        bool isOpened = false;//串口状态标志
        /// <summary>
        /// 连接串口
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            if (!isOpened)
            {
                try
                {
                    serialPort.PortName = cmbPort.Text;
                    serialPort.BaudRate = Convert.ToInt32(cmbBaud.Text, 10);
                    serialPort.Open();     //打开串口
                    button1.Text = "关闭串口";
                    cmbPort.Enabled = false;//关闭使能
                    cmbBaud.Enabled = false;
                    isOpened = true;
                    serialPort.DataReceived += new SerialDataReceivedEventHandler(post_DataReceived);//串口接收处理函数
                }
                catch
                {
                    MessageBox.Show("串口打开失败！");
                }
            }
            else
            {
                try
                {
                    serialPort.Close();     //关闭串口
                    button1.Text = "打开串口";
                    cmbPort.Enabled = true;//打开使能
                    cmbBaud.Enabled = true;
                    isOpened = false;
                }
                catch
                {
                    MessageBox.Show("串口关闭失败！");
                }
            }
        }

        /// <summary>
        /// 接收质量数据中断
        /// </summary>
        private void post_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string str = serialPort.ReadExisting();//字符串方式读
            label4.Text = "";//先清除上一次的数据
            label4.Text += str;
        }

        /// <summary>
        /// 初始化，设置COM
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            RegistryKey keyCom = Registry.LocalMachine.OpenSubKey("Hardware\\DeviceMap\\SerialComm");
            if (keyCom != null)
            {
                string[] sSubKeys = keyCom.GetValueNames();
                cmbPort.Items.Clear();
                foreach (string sName in sSubKeys)
                {
                    string sValue = (string)keyCom.GetValue(sName);
                    cmbPort.Items.Add(sValue);
                }
                if (cmbPort.Items.Count > 0)
                    cmbPort.SelectedIndex = 0;
            }
            cmbBaud.Text = "115200";

            try
            {
                dst = Cv2.ImRead(@foldPath + @"\mydst.jpg");
            }
            catch (Exception)
            {
                MessageBox.Show(@"请在.exe所在文件夹(" + @foldPath + @")放置名为mydst.jpg的目标文件");
            }
            try
            {
                v = new VideoCapture(1);
            }
            catch (Exception)
            {
                MessageBox.Show("请打开摄像头呀");
            }
            if (!v.IsOpened())
            {
                MessageBox.Show("请打开摄像头");
            }
        }





        private string foldPath = System.Windows.Forms.Application.StartupPath;
        VideoCapture v;
        Mat m, dst;
        /// <summary>
        /// 显示
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            if (!v.IsOpened())
            {
                MessageBox.Show("请打开摄像头");
                return;
            }
            if (timer1.Enabled)
            {
                button2.Text = "自动显示";
                timer1.Stop();
            }
            m = v.RetrieveMat();
            pictureBoxIpl1.ImageIpl = m;
            Cv2.ImWrite(@foldPath + @"\src.jpg", m);
        }

        /// <summary>
        /// 查看比较结果
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            if (!v.IsOpened())
            {
                MessageBox.Show("请打开摄像头");
                return;
            }
            if (timer1.Enabled)
            {
                MessageBox.Show("请先点击读取图像按钮");
                return;
            }
            double res = 0;
            Cv2.ImWrite(@foldPath + @"\src.jpg", m);
            try
            {
                res = DLLtest.Program.ImgCmp(
                @foldPath + @"\src.jpg",
                @foldPath + @"\mydst.jpg",
                @foldPath + @"\Gray.jpg",
                @foldPath + @"\Dst.jpg"
                );
                Cv2.ImShow("灰度比较结果", Cv2.ImRead(@foldPath + @"\Gray.jpg"));
                Cv2.ImShow("比较结果", Cv2.ImRead(@foldPath + @"\Dst.jpg"));
            }
            catch (Exception mye4)
            {
                MessageBox.Show(mye4.Message);
            }
            finally
            {
                richTextBox1.Text = res.ToString();
            }
        }

        /// <summary>
        /// 自动显示
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                button2.Text = "自动显示";
                timer1.Stop();
            }
            else
            {
                button2.Text = "停止自动显示";
                timer1.Start();
            }
        }

        private void 设置ToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 显示图像
        /// </summary>
        private void timer1_Tick(object sender, EventArgs e)
        {
            using (m = v.RetrieveMat())
            {
                try
                {
                    pictureBoxIpl1.ImageIpl = m;
                }
                catch (Exception)
                {
                    timer1.Stop();
                    MessageBox.Show("摄像头掉线");
                    System.Environment.Exit(0);
                }
            }
        }

        /*自动检测COM*/
        //private void timer2_Tick(object sender, EventArgs e)
        //{
        //    RegistryKey keyCom = Registry.LocalMachine.OpenSubKey("Hardware\\DeviceMap\\SerialComm");
        //    if (keyCom != null)
        //    {
        //        string[] sSubKeys = keyCom.GetValueNames();
        //        cmbPort.Items.Clear();
        //        foreach (string sName in sSubKeys)
        //        {
        //            string sValue = (string)keyCom.GetValue(sName);
        //            cmbPort.Items.Add(sValue);
        //        }
        //        if (cmbPort.Items.Count > 0)
        //            cmbPort.SelectedIndex = 0;
        //    }
        //    cmbBaud.Text = "115200";
        //}
    }
}

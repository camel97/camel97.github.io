using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private const int Port = 888;                       //指定端口号
        private const int serPort = 999;
        private Thread td;                                  //声明监听线程
        private TcpListener tcpListener;                    //声明监听对象
        private static string message = "";                 //定义发送和接收的消息
        IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());      //获取本地主机ip

        private void Form1_Load(object sender,EventArgs e)
        {
            td = new Thread(new ThreadStart(this.StartListen));//启动侦听线程
            td.Start();                                             //启动计时器
            timer1.Start();
        }
        private void StartListen()
        {
            message = "";
            tcpListener = new TcpListener(Port);                //创建侦听对象
            tcpListener.Start();
            while (true)
            {
                try
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    byte[] mbyte = new byte[1024];
                    NetworkStream ntwstream = client.GetStream();
                    int i = ntwstream.Read(mbyte, 0, mbyte.Length);
                    message = Encoding.Default.GetString(mbyte, 0, i);//将数据流中的信息保存在字符串 message 中
                }
                catch                                   //这里没有做异常处理。因为在测试的时候给了异常处理有时候会崩溃。。
                {

                }
            }
        }
        /*发送信息按钮*/
        private void button1_Click(object sender,EventArgs e)
        {
            try
            {     
                //设置消息格式           
                string strmsg = "" + myName.Text + "(" + ips[1].ToString() + ")" + DateTime.Now.ToLongTimeString() + "\n" + " " + this.rtbSend.Text + "\n";

                TcpClient client = new TcpClient(txtIP.Text, Port);      //创建tcpclient对象
                NetworkStream ntwstream = client.GetStream();                   //创建网络流
                StreamWriter wstream = new StreamWriter(ntwstream, Encoding.Default);
                wstream.Write(strmsg);                                          //将消息写入网络流

                wstream.Flush();
                wstream.Close();
                client.Close();                                                 //关闭服务，释放对象

                rtbSend.Clear();                                                //用户界面显示处理

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /*连接服务器按钮,发送自己的ip*/
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                TcpClient client = new TcpClient(txtIP.Text, serPort);//请求连接
                if (client != null)
                {
                    NetworkStream ntwstream = client.GetStream();                   //创建网络流
                    StreamWriter wstream = new StreamWriter(ntwstream, Encoding.Default);

                    string hostIP;
                    foreach(IPAddress hostIPAddress in ips)         //没找到 查找自己准确 ip 的方法，IPaddress 会返回一堆地址
                    {
                        hostIP = hostIPAddress.ToString();
                        if (Regex.IsMatch(hostIP, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$"))//这里用了正则主要是筛选成正确的 ip
                        {
                            if (Regex.IsMatch(hostIP, "222")) { wstream.Write(hostIP); }
                        }                                  //找 222 开头的字符串作为ip这里有很大的局限性。因为室友的 ip 都是 222.XXXX 
                    }
                    wstream.Flush();
                    wstream.Close();
                    MessageBox.Show("连接成功");
                    client.Close();
                }
            }
            catch
            { 
                MessageBox.Show("连接失败请尝试在其他时间连接");
            }

        }

        //计时器tick事件
        private void timer1_Tick(object sender,EventArgs e)
        {
            if (message != "")
            {
                rtbContent.AppendText(message);         //侦听到消息后显示内容
                rtbContent.ScrollToCaret();             //自动滚屏
                message = "";
            }
        }
        /*关闭窗口。停止所有服务*/
        private void Form1_FormClosed(object sender,FormClosedEventArgs e)
        {
            if (this.tcpListener != null) { tcpListener.Stop(); }           //停止侦听
            if (td != null)
            {
                if (td.ThreadState == ThreadState.Running) { td.Abort(); }      //终止线程
            }
        }

    }
}

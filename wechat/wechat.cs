using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace WindowsFormsApplication4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private const int Port = 888;                       //指定端口号
        private Thread td;                                  //声明监听线程
        private TcpListener tcpListener;                           //声明监听对象
        private static string message = "";                 //定义发送和接收的消息

        private void Form1_Load(object sender,EventArgs e)  //窗口加载
        {
            td = new Thread(new ThreadStart(this.StartListen)); //创建监听线程指定方法
            td.Start();                                         //线程启动
            timer1.Start();                                     //计时器启动
        }
        private void StartListen()                              //线程监听方法
        {
            message = "";
            tcpListener = new TcpListener(Port);
            tcpListener.Start();
            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();      //接受连接请求
                byte[] mbyte = new byte[1024];                          //建立缓存
                NetworkStream ntwstream = client.GetStream();          //从连接获取数据流
                int i = ntwstream.Read(mbyte, 0, mbyte.Length);         //将数据流写入缓存
                message = Encoding.Default.GetString(mbyte, 0, i);      //记录发送的消息

            }
        }
        /*指定发送按钮的功能*/
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());      //获取本地主机ip
                
                string strmsg = "" + DateTime.Now.ToLongTimeString() + "\n" + " " + this.rtbSend.Text + "\n";
                TcpClient client = new TcpClient(txtIP.Text, Port);      //创建tcpclient对象
                NetworkStream ntwstream = client.GetStream();                   //创建网络流
                StreamWriter wstream = new StreamWriter(ntwstream, Encoding.Default);
                wstream.Write(strmsg);                                          //将消息写入网络流

                wstream.Flush();
                wstream.Close();
                client.Close();                                                 //关闭服务，释放对象

                rtbContent.AppendText(strmsg);
                rtbContent.ScrollToCaret();
                rtbSend.Clear();                                                //用户界面显示处理

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /*启动计时器，判断当前是否有消息传输*/
        private void timer1_Tick(object sender,EventArgs e)
        {
            if(message != "")
            {
                rtbContent.AppendText(message);
                rtbContent.ScrollToCaret();
                message = "";
            }
        }
        /*关闭窗体，停止所有服务*/
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.tcpListener != null) { tcpListener.Stop(); }
            if (td != null)
            {
                if (td.ThreadState == ThreadState.Running) { td.Abort(); }
            }
        }
     }
}

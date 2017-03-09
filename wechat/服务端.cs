using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private const int MAXCONNECT = 50;
        private const int Port = 888;                       //指定端口号
        private const int ipPort = 999;                     //指定监听 ip 地址的端口号
        private Thread td;                                  //声明监听线程
        private Thread connect;                             //声明连接线程
        private TcpListener tcpListener;                    //声明监听对象
        private TcpListener tcpListenerIp;                  //声明监听 ip 对象
        private static string message = "";                 //定义发送和接收的消息
        string[] ips = new string[MAXCONNECT];              //定义存储各个成员ip地址的数组
        private static string eachIP = "";                  //定义接收每个成员的ip
        IPAddress[] hostname = Dns.GetHostAddresses(Dns.GetHostName());




        private void Form1_Load(object sender, EventArgs e)  //窗口加载
        {
            td = new Thread(new ThreadStart(this.StartListen)); //创建监听线程指定方法
            td.Start();                                         //线程启动
            connect = new Thread(new ThreadStart(this.getIP)); //创建连接线程指定方法
            connect.Start();                                     //线程启动
            timer1.Start();                                     //计时器启动
            timer2.Start();
        }
        private void StartListen()                              //线程监听方法
        {
            message = "";
            tcpListener = new TcpListener(Port);
            tcpListener.Start();
            while (true)
            {
                try
                {
                    TcpClient client = tcpListener.AcceptTcpClient();      //接受连接请求
                    byte[] mbyte = new byte[1024];                          //建立缓存
                    NetworkStream ntwstream = client.GetStream();          //从连接获取数据流
                    int i = ntwstream.Read(mbyte, 0, mbyte.Length);         //将数据流写入缓存
                    message = Encoding.Default.GetString(mbyte, 0, i);      //记录收到的消息
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                 }
            }
        }
        /*定义连接时获取所有客户端的ip*/
        private void getIP()
        {
            eachIP = "";
            tcpListenerIp = new TcpListener(ipPort);
            tcpListenerIp.Start();
            while (true)
            {
                try
                {
                    TcpClient client = tcpListenerIp.AcceptTcpClient();             //建立连接请求
                    byte[] mbyte = new byte[1024];                                  //建立缓存
                    NetworkStream ntwstream = client.GetStream();                   //写入缓存
                    int i = ntwstream.Read(mbyte, 0, mbyte.Length);
                    eachIP = Encoding.Default.GetString(mbyte, 0, i);               //得到每个客户端的 ip
                    ntwstream.Flush();
                    ntwstream.Close();
                    client.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }



        }
        /*启动计时器，判断当前是否有消息传输*/
        private void timer1_Tick(object sender, EventArgs e)
        {
            if(eachIP != "")                                            //将客户端的 ip 添加到 ips 组里
            {
                if (!File.Exists(eachIP+".txt")) { using (File.Create(eachIP + ".txt")) ;  }
                for (int j = 0; j <= MAXCONNECT; j++)
                {
                    if (ips[j] == null)
                    {
                        ips[j] = eachIP;
                        break;
                    }
                }
                

                connectIP.AppendText(eachIP+"\n");              //右边方框显示当前连接的所有客户端的  ip
                eachIP = "";
            }
            if (message != "")
            {
                /*发送消息给每一个客户端*/
                
                foreach (string ip in ips)              //从 ips 组中取到所有的客户端 ip
                {
                    try
                    {
                        TcpClient client = new TcpClient(ip, Port);      //创建tcpclient对象
                        NetworkStream ntwstream = client.GetStream();                   //创建网络流
                        StreamWriter wstream = new StreamWriter(ntwstream, Encoding.Default);
                        wstream.Write(message);                                          //将消息写入网络流

                        wstream.Flush();
                        wstream.Close();
                        client.Close();
                    }
                    catch (Exception)                                       //在这里做了异常处理，在截取异常之后，会出现服务端暂时卡死，同时目录下新建了一个
                                                                            //  .txt 文件自动重复多次写入聊天记录。弄不懂。
                    {                        
                        try
                        {
                            using (StreamWriter swriter = new StreamWriter(ip+".txt", true)) { swriter.WriteLine(message); }//断线时将消息保存在文件中
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }                    
                    }
                }                               
                chatContent.AppendText(message);
                chatContent.ScrollToCaret();
                message = "";
            }
        }
       


        /*计时器用来保存聊天记录*/
        private void timer2_Tick(object sender,EventArgs e)    //每一分钟将聊天框的内容保存在文本中同时清屏
        {
            //string chatRecords = chatContent.Text;
            try
                {
                    if (!File.Exists("Log.txt"))                //创建聊天记录的保存文本
                    {                   
                    using (File.Create("Log.txt")) ;
                    }
                    using (StreamWriter swriter = new StreamWriter("Log.txt", true)) { swriter.WriteLine(chatContent.Text); }//写入聊天记录
                    chatContent.Clear();                        //清屏

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                
        }


        /*关闭窗体，停止所有服务*/
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.tcpListener != null) { tcpListener.Stop(); }
            if (this.tcpListenerIp != null) { tcpListenerIp.Stop(); }       //停止侦听
            if (td != null)                                             
            {
                if (td.ThreadState == ThreadState.Running) { td.Abort(); }
            }
            if (connect != null)
            {
                if (connect.ThreadState == ThreadState.Running) { connect.Abort(); }
            }                                                                           //关闭线程
        }
    }
}

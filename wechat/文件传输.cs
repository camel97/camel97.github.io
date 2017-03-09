using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace ConsoleApplication1
{
    class Program
    {
        private static string oldFile = "Log.txt";          //需要发送的文件位置
        private static string newFile = "d:\\new.txt";          //接收的文件位置
        private static void fileSend()
        {

            TcpClient client = new TcpClient("222.20.24.53",888);       //请求连接
            NetworkStream ntstream = client.GetStream();                //建立网络流
            StreamWriter swriter = new StreamWriter(ntstream, Encoding.Default);
            
            byte[] mbyte = new byte[1024];                              //建立缓冲字节流
            int copied = 0;                                     //int 型数据，用来判断文件传输一部分之后的剩余大小
           
            
            FileStream fromStream = new FileStream(oldFile, FileMode.OpenOrCreate, FileAccess.Read);//文件流
            
            BinaryReader breader = new BinaryReader(fromStream);        //二进制读取
            
            while(copied <= (int)fromStream.Length - 1024)
            {
                //Console.WriteLine("ok");
                breader.Read(mbyte,0,1024);                 //将文件内容读取到缓冲
                
                string str1 = System.Text.Encoding.Default.GetString(mbyte);//将字节流转换成字符串
                Console.WriteLine("send:{0}", mbyte.Length);
                //Console.Write(str1);
                try
                {
                    swriter.Write(str1);                            //将字符串写入网络流
                    swriter.Flush();                                //清空网络流缓存
                    Thread.Sleep(2000);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                copied += 1024;                                             //copied 自加1024 记录当前已经发送的字节数
            }
            int left = (int)fromStream.Length - copied;                         //文件尚未发送且不足 1024 字节的部分
            byte[] strbyte = new byte[left];
            breader.Read(strbyte, 0, left);                                         //读取
            Console.WriteLine("send:{0}", strbyte.Length);
            // breader.Read(mbyte, 0, left);
            string str = System.Text.Encoding.Default.GetString(strbyte);
            //Console.Write(str);
            try
            {
                swriter.Write(str);                                         //传输字符串
                swriter.Flush();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            breader.Close(); 
            fromStream.Close();
            ntstream.Close();
            client.Close();                                 //关闭服务
        }

        private static void receTXT()
        {
            TcpListener tcplisten = new TcpListener(888);   //侦听本地端口
            tcplisten.Start();          
            
            FileStream ToStream = new FileStream(newFile, FileMode.OpenOrCreate, FileAccess.Write);//文件流
            BinaryWriter bwriter = new BinaryWriter(ToStream);      //二进制写入
            TcpClient client = tcplisten.AcceptTcpClient();      //接受连接请求 
            NetworkStream ntwstream = client.GetStream();          //从连接获取数据流

            while (true)            
            {
                byte[] mbyte = new byte[1024];
                int i = ntwstream.Read(mbyte, 0, mbyte.Length);     
                if (i >= 1)
                {
                    string message = Encoding.Default.GetString(mbyte, 0, i);
                    //Console.Write(message);
                    byte[] strbyte = System.Text.Encoding.Default.GetBytes(message);
                    ntwstream.Read(strbyte, 0, strbyte.Length);         //将数据流写入缓存
                    Console.WriteLine(strbyte.Length);
                    //string str = System.Text.Encoding.Default.GetString(mbyte);
                    //Console.WriteLine(str);
                    bwriter.Write(strbyte, 0, strbyte.Length);//讲缓存内容写入文件
                }
                else { break; }                     //当文件接收完毕后退出循环

                //bwriter.Write(mbyte,0,1024);              //缓存写入文件                               
                bwriter.Flush();                            
            }
            bwriter.Close();
            ToStream.Close();
            client.Close();             //关闭服务

        }

        static void Main(string[] args)
        {
            Thread td = new Thread(new ThreadStart(receTXT));
            td.Start();
            Thread sd = new Thread(new ThreadStart(fileSend));
            sd.Start();                             //建立线程开始工作
          
            Console.Read();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using _11111.Logic;
using _11111.Manager;


namespace _11111.Network
{
    //主服务
    public class MainService
    {
        public int 上传不成功吗 = 0;

        Main_Windows windows = new Main_Windows();

        Thread threadWatch = null; // 负责监听客户端连接请求的 线程； 
        Socket socketWatch = null;

        Dictionary<string, UserInfoManager> userInfo = new Dictionary<string, UserInfoManager>();

        private NetworkStream outStream = null;
        private MemoryStream memStream; //二进制(字节)缓存留
        private BinaryReader reader;

        private const int MAX_READ = 1024 * 512 * 4; //数据包大小限制
        private const int SIZE_PACK_HEAD = 8; //头包大小

        Dictionary<int, NiuNiu> nius = new Dictionary<int, NiuNiu>(); //牛牛房间数量
        Dictionary<string, int> roomKey = new Dictionary<string, int>(); //ip对应房间号

        public void StartUp(string ip, string port)
        {
            memStream = new MemoryStream();
            reader = new BinaryReader(memStream);//初始化流
            socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress address = IPAddress.Parse(ip);
            IPEndPoint endPoint = new IPEndPoint(address, int.Parse(port));
            try
            {
                socketWatch.Bind(endPoint);
            }
            catch (SocketException se)
            {
                MessageBox.Show("异常：" + se.Message);
                return;
            }
            // 设置监听队列的长度； 
            socketWatch.Listen(10);
            threadWatch = new Thread(WatchConnecting);
            threadWatch.IsBackground = true;
            threadWatch.Start();
            windows.ShowMsg("服务器启动监听成功！");
        }
        /// <summary> 
        /// 监听客户端请求的方法； 
        /// </summary> 
        void WatchConnecting()
        {
            while (true)  // 持续不断的监听客户端的连接请求； 
            {
                // 开始监听客户端连接请求，Accept方法会阻断当前的线程； 
                Socket sokConnection = socketWatch.Accept(); // 一旦监听到一个客户端的请求，就返回一个与该客户端通信的 套接字； 
                string ip = sokConnection.RemoteEndPoint.ToString();
                windows.ShowMsg("客户端连接成功！");
                Thread thr = new Thread(RecMsg);
                thr.IsBackground = true;
                thr.Start(sokConnection);
                windows.ShowIP(ip);
                //userInfo.Add(ip, UserInfoInit(ip, sokConnection));
            }
        }
        void RecMsg(object sokConnectionparn)
        {
            Socket sokClient = sokConnectionparn as Socket;
            while (true)
            {
                // 定义一个2M的缓存区； 
                byte[] arrMsgRec = new byte[1024 * 1024 * 2];
                // 将接受到的数据存入到输入  arrMsgRec中； 
                int length = -1;
                try
                {
                    length = sokClient.Receive(arrMsgRec); // 接收数据，并返回数据的长度； 
                }
                catch (SocketException se)
                {
                    windows.ShowMsg("异常：" + se.Message);
                    windows.RemoveClientLink(sokClient.RemoteEndPoint.ToString());
                    break;
                }
                catch (Exception e)
                {
                    windows.ShowMsg("异常：" + e.Message);
                    windows.RemoveClientLink(sokClient.RemoteEndPoint.ToString());
                    break;
                }
                if (length >= 1)
                {
                    string ip = sokClient.RemoteEndPoint.ToString();
                    string strMsg = Encoding.UTF8.GetString(arrMsgRec, 0, length);// 将接受到的字节数据转化成字符串； 
                    windows.ShowMsg(ip + ":" + strMsg);
                    //ReceiveMsg(arrMsgRec, length, ip);
                }
                else //接收的长度异常，断开客户端连接
                {
                    windows.ShowMsg("数据长度小于1，解析异常:" + length);
                    windows.RemoveClientLink(sokClient.RemoteEndPoint.ToString());
                    break;
                }
            }
        }
    }
}

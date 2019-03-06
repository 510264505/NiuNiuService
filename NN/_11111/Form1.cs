//[csharp] view plain copy print?
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;  // IP，IPAddress, IPEndPoint，端口等； 
using System.Threading;
using System.IO;
using LitJson;
using _11111.Manager;
using _11111.Logic;
using _11111.Network;


namespace _11111
{
    public class ClientDataItem
    {
        public ushort main_CMD;
        public ushort sub_CMD;
        public byte[] data;
        public string ip;
    }
    public partial class Main_Windows : Form
    {
        public Main_Windows()
        {
            
            InitializeComponent();
            TextBox.CheckForIllegalCrossThreadCalls = false;
        }

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

        private void btnBeginListen_Click(object sender, EventArgs e)
        {
            memStream = new MemoryStream();
            reader = new BinaryReader(memStream);//初始化流
            
            ////获取本机局域网IP
            //IPHostEntry host;
            //string localIP = "?";
            //host = Dns.GetHostEntry(Dns.GetHostName());
            //foreach (IPAddress ip in host.AddressList)
            //{
            //    if (ip.AddressFamily.ToString() == "InterNetwork")
            //    {
            //        localIP = ip.ToString();
            //        break;
            //    }
            //}
            // 创建负责监听的套接字，注意其中的参数； 
            socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            txtIp.Text = "127.0.0.1";
            txtPort.Text = "8080";
            // 获得文本框中的IP对象； 
            IPAddress address = IPAddress.Parse(txtIp.Text.Trim()); 
            //IPAddress address = IPAddress.Parse(localIP);
            // 创建包含ip和端口号的网络节点对象； 
            IPEndPoint endPoint = new IPEndPoint(address, int.Parse(txtPort.Text.Trim()));
            try
            {
                // 将负责监听的套接字绑定到唯一的ip和端口上； 
                socketWatch.Bind(endPoint);
            }
            catch (SocketException se)
            {
                MessageBox.Show("异常：" + se.Message);
                return;
            }
            // 设置监听队列的长度； 
            socketWatch.Listen(10);
            //创建负责监听的线程； 
            threadWatch = new Thread(WatchConnecting);
            threadWatch.IsBackground = true;
            threadWatch.Start();
            ShowMsg("服务器启动监听成功！");
            //} 
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
                ShowMsg("客户端连接成功！");
                Thread thr = new Thread(RecMsg);
                thr.IsBackground = true;
                thr.Start(sokConnection);
                lbOnline.Items.Add(ip);
                userInfo.Add(ip, UserInfoInit(ip, sokConnection));
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
                    ShowMsg("异常：" + se.Message);
                    RemoveClientLink(sokClient.RemoteEndPoint.ToString());
                    break;
                }
                catch (Exception e)
                {
                    ShowMsg("异常：" + e.Message);
                    RemoveClientLink(sokClient.RemoteEndPoint.ToString());
                    break;
                }
                if (length >= 1)
                {
                    string ip = sokClient.RemoteEndPoint.ToString();
                    string strMsg = Encoding.UTF8.GetString(arrMsgRec, 0, length);// 将接受到的字节数据转化成字符串； 
                    ShowMsg(ip + ":" + strMsg);
                    ReceiveMsg(arrMsgRec, length, ip);
                }
                else //接收的长度异常，断开客户端连接
                {
                    ShowMsg("数据长度小于1，解析异常:" + length);
                    RemoveClientLink(sokClient.RemoteEndPoint.ToString());
                    break;
                }
            }
        }
        public void RemoveClientLink(string ip)
        {
            userInfo.Remove(ip);
            lbOnline.Items.Remove(ip);
        }
        int index = 1;
        void InitUser(UserInfoManager user)
        {
            user.account = index.ToString();
            user.password = index.ToString();
            user.nick = "昵称" + index.ToString();
            user.userId = "userId" + index.ToString();
            user.status = 0;
            user.diamond = 5 + index;
            user.sex = index;
            index++;
        }
        UserInfoManager UserInfoInit(string ip, Socket socket)
        {
            UserInfoManager user = new UserInfoManager
            {
                ip = ip,
                socket = socket,
            };
            InitUser(user);
            return user;
        }
        public void ShowIP(string ip)
        {
            lbOnline.Items.Add(ip);
        }
        public void ShowMsg(string str)
        {
            txtMsg.AppendText(str + "\r\n");
        }

        // 发送消息  按钮
        private void btnSend_Click(object sender, EventArgs e)
        {
            string strMsg = "服务器" + "\r\n" + "   -->" + txtMsgSend.Text.Trim() + "\r\n";
            byte[] arrMsg = Encoding.UTF8.GetBytes(strMsg); // 将要发送的字符串转换成Utf-8字节数组； 
            byte[] arrSendMsg = new byte[arrMsg.Length + 1];
            arrSendMsg[0] = 0; // 表示发送的是消息数据 
            Buffer.BlockCopy(arrMsg, 0, arrSendMsg, 1, arrMsg.Length);
            string strKey = "";
            strKey = lbOnline.Text.Trim();
            if (string.IsNullOrEmpty(strKey))   // 判断是不是选择了发送的对象； 
            {
                MessageBox.Show("请选择你要发送的好友！！！");
            }
            else
            {
                userInfo[strKey].socket.Send(arrSendMsg);// 解决了 sokConnection是局部变量，不能再本函数中引用的问题； 
                ShowMsg(strMsg);
                txtMsgSend.Clear();
            }
        }

        /// <summary> 
        /// 群发消息 
        /// </summary> 
        /// <param name="sender"></param> 
        /// <param name="e">消息</param>      按钮
        private void btnSendToAll_Click(object sender, EventArgs e)  //（群发在客户端没有显示）
        {
            string strMsg = "服务器" + "\r\n" + "   -->" + txtMsgSend.Text.Trim() + "\r\n";
            byte[] arrMsg = Encoding.UTF8.GetBytes(strMsg);// 将要发送的字符串转换成Utf-8字节数组；
            //<span style="font-size:14px;"> 
            byte[] arrSendMsg = new byte[arrMsg.Length + 1]; // 上次写的时候把这一段给弄掉了，实在是抱歉哈~ 用来标识发送是数据而不是文件，如果没有这一段的客户端就接收不到消息了~~~ 
            arrSendMsg[0] = 0; // 表示发送的是消息数据 
            Buffer.BlockCopy(arrMsg, 0, arrSendMsg, 1, arrMsg.Length);
            //</span> 
            //[csharp] view plain copy print?

            foreach (UserInfoManager s in userInfo.Values)  //这里已经发出去
            {
                s.socket.Send(arrSendMsg);
            }
            ShowMsg(strMsg);
            txtMsgSend.Clear();
            ShowMsg(" 群发完毕～～～");
        }

        // 选择要发送的文件  按钮
        private void btnSelectFile_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = "D:\\";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtSelectFile.Text = ofd.FileName;
            }
        }

        // 文件的发送  按钮
        private void btnSendFile_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSelectFile.Text))
            {
                MessageBox.Show("请选择你要发送的文件！！！");
            }
            else
            {
                // 用文件流打开用户要发送的文件； 
                using (FileStream fs = new FileStream(txtSelectFile.Text, FileMode.Open))
                {
                    string fileName = Path.GetFileName(txtSelectFile.Text);
                    string fileExtension = Path.GetExtension(txtSelectFile.Text);
                    string strMsg = "我给你发送的文件为： " + fileName + fileExtension + "\r\n";
                    byte[] arrMsg = Encoding.UTF8.GetBytes(strMsg); // 将要发送的字符串转换成Utf-8字节数组； 
                    byte[] arrSendMsg = new byte[arrMsg.Length + 1];
                    arrSendMsg[0] = 0; // 表示发送的是消息数据 
                    Buffer.BlockCopy(arrMsg, 0, arrSendMsg, 1, arrMsg.Length);
                    string strKey = "";
                    strKey = lbOnline.Text.Trim();
                    if (string.IsNullOrEmpty(strKey))   // 判断是不是选择了发送的对象； 
                    {
                        MessageBox.Show("请选择你要发送的好友！！！");
                    }
                    else
                    {
                        userInfo[strKey].socket.Send(arrSendMsg);// 解决了 sokConnection是局部变量，不能再本函数中引用的问题； 
                        byte[] arrFile = new byte[1024 * 1024 * 2];
                        int length = fs.Read(arrFile, 0, arrFile.Length);  // 将文件中的数据读到arrFile数组中； 
                        byte[] arrFileSend = new byte[length + 1];
                        arrFileSend[0] = 1; // 用来表示发送的是文件数据； 
                        Buffer.BlockCopy(arrFile, 0, arrFileSend, 1, length);
                        // 还有一个 CopyTo的方法，但是在这里不适合； 当然还可以用for循环自己转化； 
                        //  sockClient.Send(arrFileSend);// 发送数据到服务端； 
                        userInfo[strKey].socket.Send(arrFileSend);// 解决了 sokConnection是局部变量，不能再本函数中引用的问题； 
                        txtSelectFile.Clear();
                    }
                }
            }
            txtSelectFile.Clear();
        }



        //------------------------------------游戏逻辑函数----------------------------
        ////大厅数据
        //public delegate void LobbyDeleageServer(ClientDataItem item);
        //public event LobbyDeleageServer OnLobbyDeleageServer;
        ////游戏逻辑
        //public delegate void GameLogicDeleage(ClientDataItem item);
        //public event GameLogicDeleage OnGameLogicDeleage;
        
        private void ReceiveMsg(byte[] bytes, int length, string ip)
        {
            memStream.Seek(0, SeekOrigin.End);
            memStream.Write(bytes, 0, length); //写入二进制流
            memStream.Seek(0, SeekOrigin.Begin); //设置起始位置
            while (RemainingBytes() > 8)
            {
                int messageLen = LogicDataLen(reader.ReadBytes(4)); // + SIZE_PACK_HEAD
                memStream.Position = memStream.Position - 4;
                //Debug.Log("当前位置:" + memStream.Position + " 最大长度:" + memStream.Length + " 数据长度:" + messageLen);
                if (RemainingBytes() >= messageLen)
                {
                    MemoryStream ms = new MemoryStream();
                    BinaryWriter writer = new BinaryWriter(ms);
                    writer.Write(reader.ReadBytes(messageLen));
                    ms.Seek(0, SeekOrigin.Begin);
                    OnReceiveMessage(ms, messageLen - SIZE_PACK_HEAD, ip);
                }
                else //备份位置两字节
                {
                    //Debug.Log("跳出循环");
                    break;
                }
            }
            //剩余的数据
            byte[] lefrover = reader.ReadBytes((int)RemainingBytes());
            memStream.SetLength(0); //设置流的长度
            memStream.Write(lefrover, 0, lefrover.Length); //讲数组写入二进制流
        }
        /// <summary>
        /// 剩余的字节，长度减去当前位置
        /// </summary>
        private long RemainingBytes()
        {
            return memStream.Length - memStream.Position;
        }
        /// <summary>
        /// 返回数据长度
        /// </summary>
        private int LogicDataLen(byte[] bytes)
        {
            try
            {
                byte[] b = new byte[4];
                Buffer.BlockCopy(bytes, 0, b, 0, b.Length);//(int)index + 4
                string str = Encoding.UTF8.GetString(b);
                short len = Convert.ToInt16(str, 16);
                return len;
            }
            catch (Exception e) //数据出错重连
            {
                ShowMsg("数据异常长度：" + bytes.Length + " \n数据解析错误：" + e.Message);
                return 0;
            }
        }
        /// <summary>
        /// 获取命令标识
        /// </summary>
        private ushort GetCMD(byte[] bytes)
        {
            try
            {
                return Convert.ToUInt16(Encoding.UTF8.GetString(bytes), 16);
            }
            catch (Exception e)
            {
                ShowMsg("转换异常：" + e.Message);
                return 0;
            }
        }
        /// <summary>
        /// 接收到消息
        /// </summary>
        private void OnReceiveMessage(MemoryStream ms, int length, string ip)
        {
            byte[] data = ms.ToArray();
            byte[] mMain = new byte[2];
            byte[] mSub = new byte[2];
            byte[] mData = new byte[length];
            Array.Copy(data, 4, mMain, 0, mMain.Length);
            Array.Copy(data, 6, mSub, 0, mSub.Length);
            Array.Copy(data, SIZE_PACK_HEAD, mData, 0, length);

            ClientDataItem item = new ClientDataItem();
            item.main_CMD = GetCMD(mMain);
            item.sub_CMD = GetCMD(mSub);
            item.data = mData;
            item.ip = ip;
            //string str = Encoding.UTF8.GetString(ms.ToArray());
            //Debug.Log("1---接收到的数据:" + str);
            //AddEvent(allData);
            HandleData(item);//每个客户端有一个标识
        }
        /// <summary>
        /// 处理分配数据------------------------------通过IP Key对象调用各自委托--------------------
        /// </summary>
        private void HandleData(ClientDataItem item)
        {
            JsonData js = JsonMapper.ToObject(ToolManager.GetString(item.data));
            switch (item.main_CMD)
            {
                case 1:
                    int number = RoomNumber(); //房间号
                    switch (item.sub_CMD)
                    {
                        case 1: //登录
                            break;
                        case 2: //注册
                            break;
                        case 3: //根据其中一人创建了房间，创建对象
                            while (true)
                            {
                                number = RoomNumber();
                                if (!nius.ContainsKey(number))
                                {
                                    roomKey.Add(item.ip, number);
                                    nius.Add(number, new NiuNiu(item));
                                    nius[number].AddPlayerInfo(userInfo[item.ip]);
                                    ReturnRoomInfo(number, "创建房间成功", true, item);
                                    nius[number].SendAllPlayerInfo();
                                    break;
                                }
                            }
                            break;
                        case 4: // 后面多加一些参数，房间规则等等
                            number = int.Parse(js["roomNumber"].ToString());
                            if (nius.ContainsKey(number))
                            {
                                nius[number].AddPlayerInfo(userInfo[item.ip]);
                                ReturnRoomInfo(number, "加入房间成功", true, item);
                                nius[number].SendAllPlayerInfo();
                            }
                            else
                            {
                                ReturnRoomInfo(number, "房间号无效", false, item);
                            }
                            break;
                    }
                    //OnLobbyDeleageServer?.Invoke(item);// 处理大厅信息
                    break;
                case 100:
                    number = int.Parse(js["roomNumber"].ToString());
                    nius[number].ReceiveGameData(item);

                    //OnGameLogicDeleage?.Invoke(item);// 处理游戏消息，不做委托有可能触发到所有对象
                    break;
                default:
                    var k = js.Keys.GetEnumerator();
                    while (k.MoveNext())
                    {
                        ShowMsg(js[k.Current].ToString());
                    }
                    SendMessage(item);
                    break;
            }
        }
        
        /// <summary>
        /// 发送数据
        /// </summary>
        internal void SendMessage(ClientDataItem item)
        {
            if (item != null)
            {
                userInfo[item.ip].socket.Send(ToolManager.ObjectToBytes(item.main_CMD, item.sub_CMD, item.data));
            }
            //MessageBox.Show("请选择你要发送的好友！！！");
        }

        /// <summary>
        /// 确认登录
        /// </summary>
        private void ComfirmLogin(string account, string password)
        {
            
        }
        private byte[] LoginSuccess(string account, string paasword)
        {
            if (userInfo.ContainsKey(account))
            {
                return Encoding.UTF8.GetBytes(JsonMapper.ToJson(userInfo[account]));
            }
            return null;
        }
        /// <summary>
        /// 返回房间信息
        /// </summary>
        private void ReturnRoomInfo(int roomNumber, string code, bool isSuccess, ClientDataItem item)
        {
            JsonData js = new JsonData
            {
                ["roomNumber"] = roomNumber,
                ["code"] = code,
                ["isSuccess"] = isSuccess,
            };
            item.data = ToolManager.GetBytes(JsonMapper.ToJson(js));
            SendMessage(item);
        }
        /// <summary>
        /// 随机房间号
        /// </summary>
        private int RoomNumber()
        {
            Random random = new Random();
            return random.Next(100000, 999999);
        }
    }
}
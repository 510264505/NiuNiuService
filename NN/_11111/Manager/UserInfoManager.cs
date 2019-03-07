using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _11111.Manager
{
    public class UserInfoManager
    {
        public Socket socket;
        public string ip;
        public string nick;
        public string account;
        public string password;
        public string userId;
        public ushort status;
        public int diamond; //钻石
        public int coin; //金币
        public int sex;
        public NiuNiuPlayerInfo info;


    }
    /// <summary>
    /// 玩家信息类
    /// </summary>
    public class NiuNiuPlayerInfo
    {
        public byte chairId; //椅子号
        public List<byte> card;
        // 第一个是牛类型，第二个是牛几，第三个是最大花色
        public int[] cowType; //0无牛，(10-19，个位数是牛几)有牛，2五花牛，3炸弹牛，4五小牛
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using _11111.Manager;
using LitJson;

namespace _11111.Logic
{
    class NiuNiu
    {
        public string gameRule; //房间游戏规则
        private byte peopleQuantity; //人数
        private bool[] isChair;
        private byte cardTypeCount = 0; //牌类型计数
        private Dictionary<string, UserInfoManager> playerInfo = new Dictionary<string, UserInfoManager>(); //所有在这个房间玩家的信息
        public NiuNiu(ClientDataItem item)
        {
            JsonData js = JsonMapper.ToObject(ToolManager.GetString(item.data));
            peopleQuantity = byte.Parse(js["people"].ToString());
            isChair = new bool[peopleQuantity];
            for (int i = 0; i < isChair.Length; i++)
            {
                isChair[i] = false;
            }
        }
        /// <summary>
        /// 添加玩家信息
        /// </summary>
        public void AddPlayerInfo(UserInfoManager user)
        {
            NiuNiuPlayerInfo p = new NiuNiuPlayerInfo();
            for (byte i = 0; i < isChair.Length; i++) //椅子号
            {
                if (!isChair[i])
                {
                    p.chairId = i;
                }
            }
            user.info = p;
            playerInfo.Add(user.ip, user); //添加
        }
        /// <summary>
        /// 移除玩家
        /// </summary>
        public void RemovePlayerInfo(string ip)
        {
            isChair[playerInfo[ip].info.chairId] = false;
            playerInfo.Remove(ip);
            
        }
        
        enum GameState
        {
            None,
            Up,
            Down,
            Ready,
            Play,
            Leave,
            Look,
        }
        public void Init()
        {
            //Main_server.OnLobbyDeleageServer += ReceiveGameData;
        }
        /// <summary>
        /// 发送所有玩家消息
        /// </summary>
        public void SendAllPlayerInfo()
        {
            JsonData js = new JsonData();
            js["player"] = new JsonData();
            var player = playerInfo.GetEnumerator();
            while (player.MoveNext())
            {
                JsonData j = new JsonData();
                j["chairId"] = player.Current.Value.info.chairId;
                j["nick"] = player.Current.Value.nick;
                j["userId"] = player.Current.Value.userId;
                j["status"] = player.Current.Value.status;
                j["coin"] = player.Current.Value.coin;
                j["sex"] = player.Current.Value.sex;
                js["player"].Add(j);
            }
            ClientDataItem c = new ClientDataItem();
            c.main_CMD = 100;
            c.sub_CMD = 1;
            c.data = ToolManager.GetBytes(JsonMapper.ToJson(js));
            GroupSendMessage(c);
        }
        /// <summary>
        /// 接收消息
        /// </summary>
        public void ReceiveGameData(ClientDataItem item)
        {
            JsonData js = JsonMapper.ToObject(ToolManager.GetString(item.data));
            switch (item.sub_CMD)
            {
                case 1: //接收玩家状态信息，并判断所有玩家准备后，发牌
                    playerInfo[item.ip].status = ushort.Parse(js["status"].ToString());
                    byte count = 0;
                    var e = playerInfo.GetEnumerator();
                    while (e.MoveNext())
                    {
                        if (e.Current.Value.status == (ushort)GameState.Ready)
                        {
                            count++;
                        }
                    }
                    if (count == peopleQuantity) //所有人准备 发牌
                    {
                        cardTypeCount = 0;
                        SendCard(count, item);
                    }
                    break;
                case 2: //接收客户端发过来的牌型
                    cardTypeCount++;
                    playerInfo[item.ip].info.card.Clear();
                    JsonData cardType = js["cardType"];
                    for (int i = 0; i < cardType.Count; i++)
                    {
                        playerInfo[item.ip].info.card.Add(byte.Parse(cardType[i].ToString()));
                    }
                    if (cardTypeCount == peopleQuantity) //所有人牌型已经摆好，计算牌型大小得分并群发返回
                    {
                        CalculCardBigSmall();
                    }
                    break;
                case 11:
                    break;
            }
        }
        /// <summary>
        /// 单发消息
        /// </summary>
        internal void SendMessage(ClientDataItem item)
        {
            playerInfo[item.ip].socket.Send(ToolManager.ObjectToBytes(item.main_CMD, item.sub_CMD, item.data));
        }
        /// <summary>
        /// 群发消息
        /// </summary>
        internal void GroupSendMessage(ClientDataItem item)
        {
            byte[] bytes = ToolManager.ObjectToBytes(item.main_CMD, item.sub_CMD, item.data);
            var player = playerInfo.GetEnumerator();
            while (player.MoveNext())
            {
                player.Current.Value.socket.Send(bytes);
            }
        }
        /// <summary>
        /// 计算牌型大小
        /// </summary>
        private void CalculCardBigSmall()
        {
            var p = playerInfo.GetEnumerator();
            JsonData js = new JsonData();
            while (p.MoveNext())
            {
                JsonData j = new JsonData();
                p.Current.Value.info.cowType = MostMaxCard(p.Current.Value.info.card);
                j["chairId"] = p.Current.Value.info.chairId;
                j["nick"] = p.Current.Value.nick;
                j["cowType"] = new JsonData();
                j["card"] = new JsonData();
                for (int i = 0; i < p.Current.Value.info.cowType.Length; i++)
                {
                    j["cowType"].Add(p.Current.Value.info.cowType[i]);
                }
                for (int i = 0; i < p.Current.Value.info.card.Count; i++)
                {
                    j["card"].Add(p.Current.Value.info.card[i]);
                }
                js.Add(j);
            }
            ClientDataItem item = new ClientDataItem();
            item.main_CMD = 100;
            item.sub_CMD = 2;
            item.data = ToolManager.GetBytes(JsonMapper.ToJson(js));
            GroupSendMessage(item);
        }
        /// <summary>
        /// 取最大的牛牌型，并取出同牌型最大的那张比拼
        /// </summary>
        private int[] MostMaxCard(List<byte> card)
        {
            int oneColumn = 13; //一列，一种花色
            int bigTan = 11; //大于十
            int fiveSmallSum = 0;
            int bombCount = 0;
            int fiveFlowerCount = 0;
            int haveSum = 0;
            int haveNumber = 0;
            int notMax = 0;
            int notIndex = 0;
            int oneMax = card[card.Count - 1]; //直接取最后一张
            int twoMax = 0;
            Dictionary<int, bool> dic = new Dictionary<int, bool>();
            for (byte i = 0; i < card.Count; i++)
            {
                if (card[i] % oneColumn != 0) //五小牛
                {
                    fiveSmallSum += card[i] % oneColumn;
                }
                else
                {
                    fiveSmallSum += card[i] % oneColumn + oneColumn;
                }
                if (!dic.ContainsKey(card[i] % oneColumn)) //炸弹牛
                {
                    dic.Add(card[i] % oneColumn, true);
                }
                else
                {
                    bombCount++;
                }
                if (card[i] % oneColumn >= bigTan || card[i] % oneColumn == 0) //五花牛
                {
                    fiveFlowerCount++;
                }
                if (i <= 2) //普通牛
                {
                    haveSum += card[i];
                }
                else
                {
                    if (haveNumber < card[i] % 13) //取最大的，第二次比不过时，不用从新赋值
                    {
                        twoMax = card[i];
                    }
                    haveNumber += card[i] % oneColumn;
                }
                if (card[i] % oneColumn > notMax) //无牛
                {
                    notIndex = i;
                    notMax = card[i];
                }
            }
            if (fiveSmallSum < 10)
            {
                return new int[3] { 4, fiveSmallSum, notIndex };
            }
            else if (bombCount >= 3)
            {
                return new int[3] { 3, bombCount , oneMax };
            }
            else if (fiveFlowerCount == 5)
            {
                return new int[3] { 2, fiveFlowerCount, notMax };
            }
            else if (haveSum == 10)
            {
                return new int[3] { 1, haveNumber % 10, twoMax };
            }
            else
            {
                return new int[3] { 0, notMax, notMax };
            }
        }
        /// <summary>
        /// 五小牛
        /// </summary>
        private int[] FiveSmallCow(List<byte> card)
        {
            int sum = 0;
            int max = 0;
            for (byte i = 0; i < card.Count; i++)
            {
                if (card[i] % 13 != 0) //五小牛
                {
                    sum += card[i] % 13;
                }
                else
                {
                    sum += card[i] % 13 + 13;
                }
                if (card[i] % 13 > max) //无牛
                {
                    max = i;
                }
            }
            return sum < 10 ? new int[] { 4, sum, max } : null;
        }
        /// <summary>
        /// 炸弹牛
        /// </summary>
        private int[] BombCow(List<byte> card)
        {
            int count = 0;
            Dictionary<int, bool> dic = new Dictionary<int, bool>();
            for (byte i = 0; i < card.Count; i++)
            {
                if (!dic.ContainsKey(card[i] % 13))
                {
                    dic.Add(card[i] % 13, true);
                }
                else
                {
                    count++;
                }
            }
            return count >= 3 ? new int[3] { 3, count, card[card.Count - 1] } : null;
        }
        /// <summary>
        /// 五花牛
        /// </summary>
        private int[] FiveFlowerCow(List<byte> card)
        {
            int count = 0;
            int max = 0;
            for (int i = 0; i < card.Count; i++)
            {
                if (card[i] % 13 >= 11 || card[i] % 13 == 0)
                {
                    count++;
                }
                if (card[i] % 13 > max) //无牛
                {
                    max = card[i];
                }
            }
            return count >= 5 ? new int[3] { 2, count, max } : null;
        }
        /// <summary>
        /// 有牛
        /// </summary>
        private int[] HaveCow(List<byte> card)
        {
            int sum = 0;
            int number = 0;
            int max = 0;
            for (int i = 0; i < card.Count; i++)
            {
                if (i <= 2) //普通牛
                {
                    sum += card[i];
                }
                else
                {
                    if (number < card[i] % 13) //取最大的，第二次比不过时，不用从新赋值
                    {
                        max = card[i];
                    }
                    number += card[i] % 13;
                }
            }
            return sum == 10 ? new int[3] { 2, sum, max } : null;
        }
        /// <summary>
        /// 散牌中的最大值
        /// </summary>
        private int[] NotCow(List<byte> card)
        {
            int max = 0;
            int index = 0;
            for (byte i = 0; i < card.Count; i++)
            {
                if (card[i] % 13 > max)
                {
                    index = i;
                    max = card[i];
                }
            }
            return new int[3] { 0, max, max };
        }
        #region 发牌
        /// <summary>
        /// 发牌，随机发，每人直接发五张
        /// </summary>
        /// 54张扑克随机抽五张
        private void SendCard(byte people, ClientDataItem item)
        {
            JsonData js = new JsonData();
            List<List<byte>> player = new List<List<byte>>();
            List<byte> isExist = new List<byte>();
            for (int i = 0; i < people; i++)
            {
                player.Add(Allocation(isExist));
            }
            var p = playerInfo.GetEnumerator();
            while (p.MoveNext())
            {
                js["chairId"] = p.Current.Value.info.chairId;
                js["card"] = new JsonData();
                js["card"].Add(player[p.Current.Value.info.chairId]);
                item.ip = p.Current.Value.ip;
                item.sub_CMD = 2;
                item.data = ToolManager.GetBytes(JsonMapper.ToJson(js));
                SendMessage(item);
            }
        }
        /// <summary>
        /// 分配牌
        /// </summary>
        private List<byte> Allocation(List<byte> isExist)
        {
            List<byte> player = new List<byte>();
            Random random = new Random();
            byte n = 0;
            for (int i = 0; i < 5; i++)
            {
                n = (byte)random.Next(1, 53);
                if (!player.Contains(n) && !isExist.Contains(n))
                {
                    player.Add(n);
                    isExist.Add(n);
                }
            }
            return player;
        }
        /// <summary>
        /// 所有有牛组合
        /// </summary>
        private string IsHaveCow(List<byte> player)
        {
            //先计算是否五小牛
            int sum = 0;
            for (byte i = 0; i < player.Count; i++)
            {
                sum += player[i];
            }
            if (sum < 10)
            {
                return "五小牛";
            }
            List<int> sumGroup = new List<int>();
            List<List<byte>> allCowGroup = new List<List<byte>>();
            List<List<byte>> allGroup = new List<List<byte>>();
            allGroup = GetAllCombination(player, allGroup, new List<byte>(), 0);
            for (int i = 0; i < allGroup.Count; i++)
            {
                sum = 0;
                for (int j = 0; j < allGroup[i].Count; j++)
                {
                    sum += allGroup[i][j] % 13;
                }
                if (sum % 10 == 0) //有牛，还有个排序，取最小花色(数字)的牛
                {
                    sumGroup.Add(sum);
                    allCowGroup.Add(allGroup[i]);
                }
            }
            allGroup.Clear();
            //int index = 0;    //取最小数字的牛
            //int min = sumGroup[index];
            //for (int i = 0; i < sumGroup.Count; i++)
            //{
            //    if (min > sumGroup[i])
            //    {
            //        index = i;
            //        min = sumGroup[i];
            //    }
            //}
            return "有牛";
        }
        /// <summary>
        /// 所有牌型组合
        /// </summary>
        private List<List<byte>> GetAllCombination(List<byte> player, List<List<byte>> li, List<byte> l, int index)
        {
            if (l.Count == 3)
            {
                List<byte> temp = new List<byte>();
                for (byte i = 0; i < l.Count; i++)
                {
                    temp.Add(l[i]);
                }
                li.Add(temp);
            }
            for (int i = index; i < player.Count; i++)
            {
                if (l.Count < 3) //先执行完第一、二行
                {
                    l.Add(player[i]);
                    GetAllCombination(player, li, l, i + 1);
                    l.RemoveAt(l.Count - 1); //直到条件不满足，并且有返回值得时候才从这里继续开始
                }
            }
            return li;
        }
        #endregion
        /// <summary>
        /// 计算阶乘
        /// </summary>
        private int Factorial(int n)
        {
            if (n == 0)
            {
                return 0;
            }
            if (n == 1)
            {
                return n;
            }
            return n * Factorial(n - 1);
        }
    }
}

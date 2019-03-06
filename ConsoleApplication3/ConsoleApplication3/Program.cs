using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using HtmlAgilityPack;
using System.Xml;
using System.Drawing;
using BallotAiying2;
using LitJson;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.IO.Compression;


namespace ConsoleApplication3
{
    class Program
    {
        static CookieContainer cookie = new CookieContainer();
        static string strCookie = "";
        static void Main(string[] args)
        {

            //            string line = "<span class=\"current\">1</span>"+
            //"<a href=\"?"+
            //"date=2018-12-27&"+
            //"page=2"+
            //"\">2</a>"+
            //"<a href=\"?"+
            //"date=2018-12-27&"+
            //"page=3"+
            //"\">3</a>"+
            //"<a class=\"next\""+
            //"			href=\"?"+
            //"date=2018-12-27&"+
            //"page=3"+
            //"\""+
            //"	>";
            //            Console.WriteLine(line);
            //            Regex reg = new Regex("<a href=\"(?<value>.*?)\">");
            //            //例如我想提取记录中的NAME值
            //            Match match = reg.Match(line);
            //            string value = match.Groups["value"].Value;
            //            //Console.WriteLine("value的值为：{0}", value);

            //            //MatchCollection m = reg.Matches(line);
            //            //foreach (Match item in m)
            //            //{
            //            //    Console.WriteLine(item.Groups["value"].Value);
            //            //}

            //            StringBuilder str = new StringBuilder();
            //            str.Append("LH_L");
            //            str.Insert(2, "1");
            //            str.Remove(0, str.Length);
            //            Console.WriteLine(str);



            //List<int> isExit = new List<int>();
            //List<List<int>> player = new List<List<int>>();
            //for (int i = 0; i < 5; i++)
            //{
            //    player.Add(SendCard(isExit));
            //}


            //GetHttp("http://120.24.56.48:8889/api/auth/login?username=13590372650&password=wjkj110");
            //GetHttp("https://chwm07.com/ssid1?url=/");

            //HttpGet("https://chwm07.com/ssid1?url=/", "");
            //HttpGet("https://chwm07.com/code?_=1544086633939", "");

            //HttpGet("https://chwm07.com/ssid1?url=/", "","");

            //SendDataByGet("https://chwm07.com/ssid1?url=/", "", ref cookie);
            //SendDataByGet("https://chwm07.com/code?_=" + GetTimeStamp(), "", ref cookie);

            //Console.WriteLine(GetCookies("https://chwm07.com/code?_=1544086633939"));

            //List<List<int>> li = new List<List<int>>();
            //List<int> player = new List<int>();
            //for (int i = 0; i < 5; i++)
            //{
            //    player.Add(i + 1);
            //}
            //li = GetAll(player, li, 0, new List<int>());


            //Console.WriteLine(StringToDateTime("1545877290000") + " 封盘");
            //Console.WriteLine(StringToDateTime("1545877239000") + " 当前");
            //Console.WriteLine(StringToDateTime("1545840000000") + " 未知");
            //Console.WriteLine(StringToDateTime("1545877305000") + " 开奖");
            //Console.WriteLine(StringToDateTime("1545877230000") + " 下注");
            //Console.WriteLine(GetTimeStamp());

            //DateTime dateTime = StringToDateTime("1545877239000");
            //Console.WriteLine(dateTime.Second); //秒 返回整形，先比较分，在比较秒，并得到当前倒计时秒数
            //Console.WriteLine(dateTime.Minute); //分

            //GetCookie("https://chwm07.com/ssid1?url=/mobile/login");


            


            Console.ReadLine();
        }
        

        static DateTime StringToDateTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }

        public static void GetCookie(string url)
        {
            CookieContainer cookies = new CookieContainer();
            //url = "http://120.24.56.48:8889/api/auth/login?username=13590372650&password=wjkj110";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 20 * 1000; //连接超时  
            request.Accept = "*/*";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0;)";
            request.CookieContainer = new CookieContainer(); //暂存到新实例</span>  
            request.AllowAutoRedirect = false;
            request.GetResponse().Close();
            cookies = request.CookieContainer; //保存cookies</span>  
            string cookiesstr = request.CookieContainer.GetCookieHeader(request.RequestUri); //把cookies转换成字符串  
            Console.WriteLine("获取的Cookie:" + cookiesstr);
        }
        public static string GetHttp(string url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0; QQWubi 133; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; CIBA; InfoPath.2)";
            //httpWebRequest.ContentType = "application/json";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Method = "GET";
            httpWebRequest.Timeout = 20000;
            httpWebRequest.CookieContainer = new CookieContainer();
            httpWebRequest.AllowAutoRedirect = false; //取消重定向
            

            //byte[] btBodys = Encoding.UTF8.GetBytes(body);
            //httpWebRequest.ContentLength = btBodys.Length;
            //httpWebRequest.GetRequestStream().Write(btBodys, 0, btBodys.Length);

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
            string responseContent = streamReader.ReadToEnd();

            string cookie = httpWebRequest.CookieContainer.GetCookieHeader(httpWebRequest.RequestUri);
            Console.WriteLine("cookie:" + cookie);

            httpWebResponse.Close();
            streamReader.Close();

            return responseContent; //返回网页源代码
        }

        private static string HttpPost(string Url, string postDataStr, ref CookieContainer cookie)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            byte[] postData = Encoding.UTF8.GetBytes(postDataStr);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            request.CookieContainer = cookie;
            request.AllowAutoRedirect = false; //取消重定向
            Stream myRequestStream = request.GetRequestStream();
            myRequestStream.Write(postData, 0, postData.Length);
            myRequestStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //response.Cookies = cookie.GetCookies(request.RequestUri);
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString; //POST接收到新网页源代码
        }
        private static string HttpGet(string url, string postDataStr, string coss)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.Timeout = 20000; //连接超时  
            //request.ContentType = "text/html;charset=UTF-8";
            request.Accept = "*/*";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.87 Safari/537.36";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = cookie;
            request.Headers.Add("Cookie", strCookie + coss);
            request.AllowAutoRedirect = false; //取消重定向

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //response.Cookies = request.CookieContainer.GetCookies(response.ResponseUri);
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();

            //Console.WriteLine(retString);

            strCookie = request.CookieContainer.GetCookieHeader(request.RequestUri);
            Console.WriteLine("1.cookie:" + strCookie + coss);

            myStreamReader.Close();
            myResponseStream.Close();


            url = "https://chwm07.com/" + GetTimeStamp();
            Console.WriteLine("地址:" + url);
            request = (HttpWebRequest)WebRequest.Create(url);
            //request.Accept = "image/webp,image/apng,image/*,*/*;q=0.8";
            request.Method = "GET";
            request.Timeout = 20000; //连接超时  
            //request.ContentType = "text/html;charset=UTF-8";
            //request.Host = "www.chwm07.com";
            //request.Referer = "https://chwm07.com/main";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.87 Safari/537.36";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = cookie;
            request.Headers.Add("Cookie", strCookie);
            //request.Headers.Add("Accept-Encoding", "gzip,deflate,br");
            //request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");

            //request.AllowAutoRedirect = false; //取消重定向
            //request.MaximumAutomaticRedirections = 2; //最大重定向次数
            HttpWebResponse myHttpWebResponse = (HttpWebResponse)request.GetResponse();
            Stream stream = myHttpWebResponse.GetResponseStream();
            StreamReader sr = new StreamReader(stream, Encoding.UTF8);
            string strWebData = sr.ReadToEnd();
            Console.WriteLine(strWebData);

            //strCookie = request.CookieContainer.GetCookieHeader(request.RequestUri);
            //Console.WriteLine("2.cookie:" + strCookie);

            return retString;
        }



        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        private static string GetTimeStamp()
        {
            long ts = ConvertDateTimeToInt(DateTime.Now);
            return ts.ToString();
        }
        /// <summary>  
        /// 将c# DateTime时间格式转换为Unix时间戳格式  
        /// </summary>  
        /// <param name="time">时间</param>  
        /// <returns>long</returns>  
        private static long ConvertDateTimeToInt(DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }



        public static string SendDataByPost(string url, string postData, ref CookieContainer cookie)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (cookie.Count == 0)
            {
                request.CookieContainer = new CookieContainer();
                cookie = request.CookieContainer;
            }
            else
            {
                request.CookieContainer = cookie;
            }
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            Stream myRequestStream = request.GetRequestStream();
            StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.UTF8);
            myStreamWriter.Write(postData);
            myStreamWriter.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();

            return retString;
        }
        public static string SendDataByGet(string url, string postData, ref CookieContainer cookie)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + (postData == "" ? "" : "?") + postData);
            if (cookie.Count == 0)
            {
                request.CookieContainer = new CookieContainer();
                cookie = request.CookieContainer;
            }
            else
            {
                request.CookieContainer = cookie;
            }
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.87 Safari/537.36";
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.AllowAutoRedirect = false;
            //request.MaximumAutomaticRedirections = 2;
            //request.Referer = url;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            request.Headers.Add("Accept-Encoding", "gzip,deflate,br");
            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");



            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            strCookie = request.CookieContainer.GetCookieHeader(request.RequestUri);
            Console.WriteLine("2.cookie:" + strCookie);

            return retString;
        }
    }
}

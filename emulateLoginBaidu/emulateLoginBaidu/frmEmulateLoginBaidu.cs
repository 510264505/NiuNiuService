using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace emulateLoginBaidu
{
    public partial class frmEmulateLoginBaidu : Form
    {
        CookieCollection curCookies = null;

        bool gotCookieBaiduid, extractTokenValueOK, loginBaiduOk;

        public frmEmulateLoginBaidu()
        {
            InitializeComponent();
        }

        private void frmEmulateLoginBaidu_Load(object sender, EventArgs e)
        {
            //init
            curCookies = new CookieCollection();
            gotCookieBaiduid = false;
            extractTokenValueOK = false;
            loginBaiduOk = false;
        }

        /******************************************************************************
        functions in crifanLib.cs
        *******************************************************************************/

        //quote the input dict values
        //note: the return result for first para no '&'
        public string quoteParas(Dictionary<string, string> paras)
        {
            string quotedParas = "";
            bool isFirst = true;
            string val = "";
            foreach (string para in paras.Keys)
            {
                if (paras.TryGetValue(para, out val))
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        quotedParas += para + "=" + HttpUtility.UrlPathEncode(val);
                    }
                    else
                    {
                        quotedParas += "&" + para + "=" + HttpUtility.UrlPathEncode(val);
                    }
                }
                else
                {
                    break;
                }
            }

            return quotedParas;
        }

        /******************************************************************************
        Demo emulate login baidu related functions
        *******************************************************************************/

        private void btnGetBaiduid_Click(object sender, EventArgs e)
        {
            //http://www.baidu.com/
            string baiduMainUrl = txbBaiduMainUrl.Text;
            //generate http request
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(baiduMainUrl);

            //add follow code to handle cookies
            req.CookieContainer = new CookieContainer();
            req.CookieContainer.Add(curCookies);

            req.Method = "GET";
            //use request to get response
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            txbGotBaiduid.Text = "";
            foreach (Cookie ck in resp.Cookies)
            {
                txbGotBaiduid.Text += "[" + ck.Name + "]=" + ck.Value;
                if (ck.Name == "BAIDUID")
                {
                    gotCookieBaiduid = true;
                }
            }

            if (gotCookieBaiduid)
            {
                //store cookies
                curCookies = resp.Cookies;
            }
            else 
            {
                MessageBox.Show("错误：没有找到cookie BAIDUID ！");
            }
        }

        private void btnGetToken_Click(object sender, EventArgs e)
        {
            if (gotCookieBaiduid)
            {
                string getapiUrl = "https://passport.baidu.com/v2/api/?getapi&class=login&tpl=mn&tangram=true";
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(getapiUrl);

                //add previously got cookies
                req.CookieContainer = new CookieContainer();
                req.CookieContainer.Add(curCookies);

                req.Method = "GET";
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                StreamReader sr = new StreamReader(resp.GetResponseStream());
                string respHtml = sr.ReadToEnd();

                //bdPass.api.params.login_token='5ab690978812b0e7fbbe1bfc267b90b3';
                string tokenValP = @"bdPass\.api\.params\.login_token='(?<tokenVal>\w+)';";
                Match foundTokenVal = (new Regex(tokenValP)).Match(respHtml);
                if (foundTokenVal.Success)
                {
                    //extracted the token value
                    txbExtractedTokenVal.Text = foundTokenVal.Groups["tokenVal"].Value;
                    extractTokenValueOK = true;
                }
                else
                {
                    txbExtractedTokenVal.Text = "错误：没有找到token的值！";
                }

            }
            else 
            {
                MessageBox.Show("错误：之前没有正确获得Cookie：BAIDUID ！");
            }
        }

        private void btnEmulateLoginBaidu_Click(object sender, EventArgs e)
        {
            if (gotCookieBaiduid && extractTokenValueOK)
            {
                string staticpage = "http://www.baidu.com/cache/user/html/jump.html";
                
                //init post dict info
                Dictionary<string, string> postDict = new Dictionary<string, string>();
                //postDict.Add("ppui_logintime", "");
                postDict.Add("charset", "utf-8");
                //postDict.Add("codestring", "");
                postDict.Add("token", txbExtractedTokenVal.Text);
                postDict.Add("isPhone", "false");
                postDict.Add("index", "0");
                //postDict.Add("u", "");
                //postDict.Add("safeflg", "0");
                postDict.Add("staticpage", staticpage);
                postDict.Add("loginType", "1");
                postDict.Add("tpl", "mn");
                postDict.Add("callback", "parent.bdPass.api.login._postCallback");
                postDict.Add("username", txbBaiduUsername.Text);
                postDict.Add("password", txbBaiduPassword.Text);
                //postDict.Add("verifycode", "");
                postDict.Add("mem_pass", "on");

                string baiduMainLoginUrl = "https://passport.baidu.com/v2/api/?login";
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(baiduMainLoginUrl);
                //add cookie
                req.CookieContainer = new CookieContainer();
                req.CookieContainer.Add(curCookies);
                //set to POST
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                //prepare post data
                string postDataStr = quoteParas(postDict);
                byte[] postBytes = Encoding.UTF8.GetBytes(postDataStr);
                req.ContentLength = postBytes.Length;
                //send post data
                Stream postDataStream = req.GetRequestStream();
                postDataStream.Write(postBytes, 0, postBytes.Length);
                postDataStream.Close();
                //got response
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                //got returned html
                StreamReader sr = new StreamReader(resp.GetResponseStream());
                string loginBaiduRespHtml = sr.ReadToEnd();

                //check whether got all expected cookies
                Dictionary<string, bool> cookieCheckDict = new Dictionary<string, bool>();
                string[] cookiesNameList = {"BDUSS", "PTOKEN", "STOKEN", "SAVEUSERID"};
                foreach (String cookieToCheck in cookiesNameList)
                {
                    cookieCheckDict.Add(cookieToCheck, false); 
                }

                foreach (Cookie singleCookie in resp.Cookies)
                {
                    if (cookieCheckDict.ContainsKey(singleCookie.Name))
                    {
                        cookieCheckDict[singleCookie.Name] = true;
                    }
                }

                bool allCookiesFound = true;
                foreach (bool foundCurCookie in cookieCheckDict.Values)
                {
                    allCookiesFound = allCookiesFound && foundCurCookie; 
                }


                loginBaiduOk = allCookiesFound;
                if (loginBaiduOk)
                {
                    txbEmulateLoginResult.Text = "成功模拟登陆百度首页！";
                }
                else
                {
                    txbEmulateLoginResult.Text = "模拟登陆百度首页 失败！";
                    txbEmulateLoginResult.Text += Environment.NewLine + "所返回的Header信息为：";
                    txbEmulateLoginResult.Text += Environment.NewLine + resp.Headers.ToString();
                    txbEmulateLoginResult.Text += Environment.NewLine + Environment.NewLine;
                    txbEmulateLoginResult.Text += Environment.NewLine + "所返回的HTML源码为：";
                    txbEmulateLoginResult.Text += Environment.NewLine + loginBaiduRespHtml;
                }
            }
            else
            {
                MessageBox.Show("错误：没有正确获得Cookie BAIDUID 和/或 没有正确提取出token值！");
            }
        }

        private void lklEmulateLoginTutorialUrl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string emulateLoginTutorialUrl = "http://www.crifan.com/emulate_login_website_using_csharp";
            System.Diagnostics.Process.Start(emulateLoginTutorialUrl);
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            curCookies = new CookieCollection();
            gotCookieBaiduid = false;
            extractTokenValueOK = false;
            loginBaiduOk = false;

            txbGotBaiduid.Text = "";
            txbExtractedTokenVal.Text = "";

            txbBaiduUsername.Text = "";
            txbBaiduPassword.Text = "";
            txbEmulateLoginResult.Text = "";
        }
    }
}

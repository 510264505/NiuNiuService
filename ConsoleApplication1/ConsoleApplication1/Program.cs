using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LitJson;
using System.IO;
using System.Security.Cryptography;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            JsonData js = new JsonData();
            js["cmd"] = "Heartbeat";
            js["11"] = false;
            js["22"] = new JsonData();
            js["22"]["33"] = 2233;
            js["44"] = null;

            js["33"] = new JsonData();
            js["33"].Add(new JsonData());
            js["33"].Add(2);
            js["33"].Add(5);
            js["33"].Add("8");


            string str = JsonMapper.ToJson(js);
            Console.WriteLine("Json : " + str);
            //Bianli(str);
            if (js["44"] == null)
            {
                Console.WriteLine("?????");
            }
            else if (js["44"].IsObject)
            {
                Console.WriteLine("!!!!!!!!");
            }
            



            Console.WriteLine();
            object a = AAA(js["11"].ToString());
            Console.WriteLine(a);



            byte[] b = Encoding.UTF8.GetBytes(str);


            Console.WriteLine(b.Length);

            byte[] b1 = Len(19);
            for (int i = 0; i < b1.Length; i++)
            {
                Console.Write(" b1:" + b1[i]);
            }
            byte[] b2 = new byte[5] { 12, 0, 0, 0, 0 };
            Console.WriteLine();
            Console.WriteLine("长度:" + BitConverter.ToInt32(b2, 0) + " 数组长度:" + b2.Length);



            

            byte[] bb = new byte[4] { 34, 58, 49, 44 };
            MemoryStream ms = new MemoryStream();
            BinaryReader read = new BinaryReader(ms);
            ms.Seek(0, SeekOrigin.End);
            ms.Write(bb, 0, bb.Length);
            ms.Seek(0, SeekOrigin.Begin);

            byte[] bb1 = read.ReadBytes(2);
            for (int i = 0; i < bb1.Length; i++)
            {
                Console.WriteLine("bb1 "+bb1[i]);
            }
            //ms.SetLength(0);
            //ms.Write(bb, 0, bb.Length);
            //ms.Seek(0, SeekOrigin.Begin);
            bb1 = ms.ToArray();
            for (int i = 0; i < bb1.Length; i++)
            {
                Console.WriteLine("bb1???" + bb1[i]);
            }

            JsonData li = new JsonData();
            li["0"] = "1";
            Console.WriteLine("int key to JsonStr:" + JsonMapper.ToJson(li));

            


            Console.ReadLine();
        }

        
        static void Bianli(string str)
        {
            JsonData jj1 = JsonMapper.ToObject(str);
            IEnumerator<string> key = jj1.Keys.GetEnumerator();
            while (key.MoveNext())
            {
                Console.WriteLine("jj1 :  " + key.Current + " value: " + jj1[key.Current]);
                if (jj1[key.Current].IsArray)
                {
                    BArray(jj1[key.Current], key.Current);
                }
            }
        }
        static void BArray(JsonData jj1, string key)
        {
            for (int i = 0; i < jj1.Count; i++)
            {
                Console.Write("  json array:" + jj1[i]);
            }
        }
        static object AAA(string str)
        {
            try
            {
                return Convert.ToBoolean(str);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }
        static byte[] Len(int len)
        {
            string str = len.ToString();
            byte[] b = new byte[4];
            for (int i = 1; i <= str.Length; i++)
            {
                b[b.Length - i] = byte.Parse(str.Substring(str.Length - i, 1));
            }
            return b;
        }
    }
}

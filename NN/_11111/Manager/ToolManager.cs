using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _11111.Manager
{
    class ToolManager
    {
        public static byte[] ObjectToBytes(int mainCMD, int subCMD, string str)
        {
            List<byte> data = new List<byte>();
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            data.AddRange(TanToSixteen(bytes.Length + 8, "X4"));
            data.AddRange(TanToSixteen(mainCMD, "X2"));
            data.AddRange(TanToSixteen(subCMD, "X2"));
            data.AddRange(Encoding.UTF8.GetBytes(str));
            return data.ToArray();
        }
        public static byte[] ObjectToBytes(int mainCMD, int subCMD, byte[] bytes)
        {
            List<byte> data = new List<byte>();
            data.AddRange(TanToSixteen(bytes.Length + 8, "X4"));
            data.AddRange(TanToSixteen(mainCMD, "X2"));
            data.AddRange(TanToSixteen(subCMD, "X2"));
            data.AddRange(bytes);
            return data.ToArray();
        }
        private static byte[] TanToSixteen(int num, string length)
        {
            return Encoding.UTF8.GetBytes(num.ToString(length));
        }
        public static string GetString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
        public static byte[] GetBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
    }
}

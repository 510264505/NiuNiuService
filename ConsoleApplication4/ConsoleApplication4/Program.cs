using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication4
{
    class Program
    {
        static IntPtr hProcess;
        static void Main(string[] args)
        {

            int pid = GetPidByProcessName("TestChangeMemory");
            Console.WriteLine(pid);
            hProcess = FindWindow("Form1");
            Console.WriteLine(hProcess);
            int dataAddress = ReadMemoryValue(0x0015E944, "TestChangeMemory");
            Console.WriteLine(dataAddress);




            Console.ReadLine();
        }
        /// <summary>
        /// 根据进程句柄读入该进程的某个内存空间
        /// </summary>
        /// <param name="lpProcess">进程句柄</param>
        /// <param name="lpBaseAddress">内存读取的起始地址</param>
        /// <param name="lpBuffer">写入地址</param>
        /// <param name="nSize">写入字节数</param>
        /// <param name="BytesRead">实际传递的字节数</param>
        /// <returns>读取结果</returns>
        [DllImport("kernel32.dll ")]
        static extern bool ReadProcessMemory(IntPtr lpProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int nSize, IntPtr BytesRead);
        /// <summary>
        /// 打开一个已存在的进程对象，并返回进程的句柄
        /// </summary>
        /// <param name="iAccess">渴望得到的访问权限</param>
        /// <param name="Handle">是否继承句柄</param>
        /// <param name="ProcessID">进程PID</param>
        /// <returns>进程的句柄</returns>
        [DllImportAttribute("kernel32.dll", EntryPoint = "OpenProcess")]
        public static extern IntPtr OpenProcess(int iAccess,bool Handle,int ProcessID);
        public static int GetPidByProcessName(string processName)
        {
            Process[] arrayProcess = Process.GetProcessesByName(processName);
            foreach (Process pro in arrayProcess)
            {
                return pro.Id;
            }
            return -1;
        }
        /// <summary>
        /// 根据窗口标题查找窗口句柄
        /// </summary>
        /// <param name="title">窗口标题</param>
        /// <returns></returns>
        public static IntPtr FindWindow(string title)
        {
            Process[] ps = Process.GetProcesses();
            foreach (Process p in ps)
            {
                if (p.MainWindowTitle.IndexOf(title) != -1)
                {
                    return p.MainWindowHandle;
                }
            }
            return IntPtr.Zero;
        }
        /// <summary>
        /// 写入某一进程的内存区域
        /// </summary>
        /// <param name="lpProcess">进程句柄</param>
        /// <param name="lpBaseAddress">写入的内存首地址</param>
        /// <param name="lpBuffer">写入数据的指针</param>
        /// <param name="nSize">写入字节数</param>
        /// <param name="BytesWrite">实际写入字节数的指针</param>
        /// <returns>大于0代表成功</returns>
        [DllImportAttribute("kernel32.dll", EntryPoint = "WriteProcessMemory")]
        public static extern bool WriteProcessMemory(IntPtr lpProcess, IntPtr lpBaseAddress, int[] lpBuffer, int nSize, IntPtr BytesWrite);

        [DllImport("kernel32.dll ")]
        static extern int VirtualQueryEx(IntPtr hProcess1, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer,int dwLength);
        [DllImport("kernel32.dll")]
        private static extern void CloseHandle(IntPtr hObject);




        //接收内存信息的结构体
        public struct MEMORY_BASIC_INFORMATION
        {
            //区域基地址
            public int BaseAddress;
            //分配基地址
            public int AllocationBase;
            //区域被初次保留时赋予的保护属性
            public int AllocationProtect;
            //区域大小
            public int RegionSize;
            //状态
            public int State;
            //保护属性
            public int Protect;
            //类型
            public int lType;
        }
        //读取内存中的值
        public static int ReadMemoryValue(int baseAddress, string processName)
        {
            try
            {
                byte[] buffer = new byte[4];
                //获取缓冲区地址
                IntPtr byteAddress = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
                //打开一个已存在的进程对象  0x1F0FFF 最高权限
                IntPtr hProcess = OpenProcess(0x1F0FFF, false, GetPidByProcessName(processName));
                //将制定内存中的值读入缓冲区
                ReadProcessMemory(hProcess, (IntPtr)baseAddress, byteAddress, 4, IntPtr.Zero);
                //关闭操作
                CloseHandle(hProcess);
                //从非托管内存中读取一个 32 位带符号整数。
                return Marshal.ReadInt32(byteAddress);
            }
            catch
            {
                return 0;
            }
        }
        
    }
}

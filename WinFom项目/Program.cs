using OpenCvSharp;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DLLtest
{
    class Program
    {
        //[DllImport(@"C:\Users\ttp\Desktop\WinFom项目 1.3修改\x64\Debug\DllCPP.dll")]
        //[DllImport(@"C:\Users\ttp\Desktop\WinFom项目 1.3gai\WinFom项目\bin\Debug\DllCPP.dll")]
        [DllImport(@"DllCPP.dll")]
        public static extern double ImgCmp(string srcPath, string dstPath, string saveGrayPath, string saveDstPath, int myX = 300, int myY = 486);
    }
}


namespace WinFom项目
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}

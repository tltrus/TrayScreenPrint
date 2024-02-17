using System;

using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace KeyboardHook
{
    static class Program
    {
        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        private const int HIDE = 0;
        private const int SHOW = 9;
        private const int MF_BYCOMMAND = 0;
        private const int SC_CLOSE = 0xF060;
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        private static IntPtr ConsoleWindow = GetConsoleWindow();
        private static NotifyIcon TrayIcon;

        private static string icon = @"icons/icon.ico";


        static void ToggleWindow(bool visible) => ShowWindow(ConsoleWindow, visible ? SHOW : HIDE);

        private static void TrayIcon_Click(object sender, EventArgs e)
        {
            var ee = (MouseEventArgs)e;
            if(ee.Button == MouseButtons.Right)
            {
                NotifyIcon ni = (NotifyIcon)sender;
                ni.BalloonTipText = "Bye...";
                ni.BalloonTipIcon = ToolTipIcon.Info;
                ni.Visible = true;
                ni.ShowBalloonTip(500);

                Thread.Sleep(500);  // Delay before app exit
                ni.Visible = false;
                Environment.Exit(0); // app exit
            }

        }

        [STAThread]
        static void Main(String[] args)
        {
            System.Threading.Timer t = new System.Threading.Timer(TimerCallback, null, 0, 500);

            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);
            ToggleWindow(false);
            TrayIcon = new NotifyIcon();
            TrayIcon.Icon = new Icon(icon);
            TrayIcon.Visible = true;
            TrayIcon.Click += TrayIcon_Click;

            Application.Run();
        }

        static void TimerCallback(object o)
        {
            for (int i = 0; i < 255; i++)
            {
                int state = GetAsyncKeyState(i);
                if (state != 0)
                {
                    if (((Keys)i) == Keys.PrintScreen)
                    {
                        Graphics graph = null;
                        var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                        
                        // Dir create with date name
                        var date = DateTime.Now.ToString("yyyy.MM.dd");
                        if (!Directory.Exists(date)) Directory.CreateDirectory(date); // if directory is not presence, create it
                        // ****************************************************

                        var filedate = DateTime.Now.ToString("yyyyMMdd_H.mm.ss");
                        graph = Graphics.FromImage(bmp);
                        graph.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                        bmp.Save(date + "/" + filedate + ".png");

                        // show balloon
                        NotifyIcon ni = TrayIcon;
                        ni.BalloonTipText = "Saved";
                        ni.BalloonTipIcon = ToolTipIcon.Info;
                        ni.ShowBalloonTip(100);

                        // trick for remove balloon after 3 sec
                        var date11 = DateTime.Now;
                        while (DateTime.Now < date11.AddSeconds(3))
                        { }

                        ni.Visible = false;
                        ni.Visible = true;
                    }
                }
            }

            // Force a garbage collection to occur for this demo.
            GC.Collect();
        }
    }
}

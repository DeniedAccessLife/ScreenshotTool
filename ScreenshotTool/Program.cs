using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

class Program
{
    [DllImport("user32.dll")]
    static extern bool SetProcessDPIAware();

    [DllImport("user32.dll")]
    static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;

    private const int SWP_NOSIZE = 0x0001;
    private const int SWP_NOZORDER = 0x0004;
    private const int SWP_SHOWWINDOW = 0x0040;

    [STAThread]
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.Unicode;

        Console.Write("process> ");
        string input = Console.ReadLine();
        Console.Clear();

        Process[] processes = Process.GetProcessesByName(input);

        if (processes.Length == 0)
        {
            Console.WriteLine("Process not found!");
            Console.ReadKey();
            return;
        }

        Process process = processes[0];
        IntPtr handle = process.MainWindowHandle;

        if (handle == IntPtr.Zero)
        {
            Console.WriteLine("Process window not found!");
            Console.ReadKey();
            return;
        }

        SetProcessDPIAware();
        Console.CursorVisible = false;
        Console.WriteLine("Screenshot countdown...");

        for (int i = 3; i > 0; i--)
        {
            Console.Write("\r{0} ", i);
            Thread.Sleep(1000);
        }

        Console.Write("\rCapturing screenshot.");

        SetForegroundWindow(handle);

        GetWindowRect(handle, out RECT windowRect);
        Rectangle screenBounds = Screen.FromHandle(handle).Bounds;
        Point centerPoint = new Point(screenBounds.Left + screenBounds.Width / 2 - (windowRect.Right - windowRect.Left) / 2, screenBounds.Top + screenBounds.Height / 2 - (windowRect.Bottom - windowRect.Top) / 2);
        SetWindowPos(handle, IntPtr.Zero, centerPoint.X, centerPoint.Y, 0, 0, SWP_NOZORDER | SWP_NOSIZE | SWP_SHOWWINDOW);

        IntPtr console = GetConsoleWindow();
        IntPtr taskbar = FindWindow("Shell_TrayWnd", null);
        IntPtr progman = FindWindow("Progman", null);
        progman = GetWindow(progman, 5);

        ShowWindow(progman, SW_HIDE);
        ShowWindow(taskbar, SW_HIDE);
        ShowWindow(console, SW_HIDE);

        Bitmap screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        Graphics graphics = Graphics.FromImage(screenshot);
        graphics.CopyFromScreen(0, 0, 0, 0, screenshot.Size);
        screenshot.Save(@"view.png", ImageFormat.Png);

        Clipboard.SetImage(screenshot);

        ShowWindow(taskbar, SW_SHOW);
        ShowWindow(console, SW_SHOW);
        ShowWindow(progman, SW_SHOW);

        Console.WriteLine(Environment.NewLine);
        Console.Write("Screenshot made successfully ʕっ•ᴥ•ʔっ");
        Console.ReadKey();
    }
}
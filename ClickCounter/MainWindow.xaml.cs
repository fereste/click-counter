using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace ClickCounter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

        static bool recording = false;
        static int leftButtonClicks = 0;
        static int rightButtonClicks = 0;
        static int middleButtonClicks = 0;
        static int wheelClicks = 0;
        static int keysUp = 0;
        static MainWindow staticThis = null;

        const int WH_KEYBOARD_LL = 13;
        const int WH_MOUSE_LL = 14;
        const int WM_KEYUP = 257;
        const int WM_LBUTTON_UP = 514;
        const int WM_RBUTTON_UP = 517;
        const int WM_MBUTTON_UP = 520;
        const int WM_WHEEL = 522;

        private LowLevelProc mouseCallback = MouseCallback;
        private LowLevelProc keyboardCallback = KeyboardCallback;
        private static IntPtr mouseHook = IntPtr.Zero;
        private static IntPtr keyboardHook = IntPtr.Zero;

        public MainWindow()
        {
            InitializeComponent();
            staticThis = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetHook();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Unhook();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (recording)
            {
                StopRecording(this);
            }
            else
            {
                StartRecording(this);
            }
        }

        private static void ResetCounters()
        {
            leftButtonClicks = 0;
            rightButtonClicks = 0;
            middleButtonClicks = 0;
            wheelClicks = 0;
            keysUp = 0;
        }

        public void SetHook()
        {
            IntPtr hInstance = LoadLibrary("User32");
            mouseHook = SetWindowsHookEx(WH_MOUSE_LL, mouseCallback, hInstance, 0);
            keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, keyboardCallback, hInstance, 0);
        }

        public static void Unhook()
        {
            UnhookWindowsHookEx(mouseHook);
            UnhookWindowsHookEx(keyboardHook);
        }

        public static IntPtr MouseCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            //Debug.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToString("HH:mm:ss.fff"), wParam));

            if (recording)
            {
                if (wParam == (IntPtr)WM_LBUTTON_UP)
                {
                    leftButtonClicks++;
                }
                else if (wParam == (IntPtr)WM_RBUTTON_UP)
                {
                    rightButtonClicks++;
                }
                else if (wParam == (IntPtr)WM_MBUTTON_UP)
                {
                    middleButtonClicks++;
                }
                else if (wParam == (IntPtr)WM_WHEEL)
                {
                    wheelClicks++;
                }
            }

            return CallNextHookEx(mouseHook, code, (int)wParam, lParam);
        }

        public static IntPtr KeyboardCallback(int code, IntPtr wParam, IntPtr lParam)
        {

            if (wParam == (IntPtr)WM_KEYUP)
            {
                keysUp++;

                int keyCode = Marshal.ReadInt32(lParam);
                Debug.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToString("HH:mm:ss.fff"), keyCode));

                if (keyCode == 113)
                {
                    StartRecording(staticThis);
                }
                else if (keyCode == 114)
                {
                    StopRecording(staticThis);
                }
            }

            return CallNextHookEx(mouseHook, code, (int)wParam, lParam);
        }

        private static void StopRecording(MainWindow staticThis)
        {
            if (recording)
            {
                staticThis.button1.Content = "Empezar";

                staticThis.textBox1.AppendText(string.Format("\r\nCuenta de clicks:\r\n" +
                    "{0}\r\n" +
                    "Clicks izquierdos: {1}\r\n" +
                    "Clicks derechos: {2}\r\n" +
                    "Clicks del medio: {3}\r\n" +
                    "Ticks ruedita: {4}\r\n" +
                    "Total clicks (I+D): {5}\r\n" +
                    "Total de teclas: {6}\r\n",
                    DateTime.Now.ToLongTimeString(),
                    leftButtonClicks,
                    rightButtonClicks,
                    middleButtonClicks,
                    wheelClicks,
                    leftButtonClicks + rightButtonClicks,
                    keysUp));

                staticThis.textBox1.ScrollToEnd();
                recording = false;
            }
        }

        private static void StartRecording(MainWindow staticThis)
        {
            if (!recording)
            {
                ResetCounters();
                staticThis.button1.Content = "Parar";
                recording = true;
            }
        }
    }
}

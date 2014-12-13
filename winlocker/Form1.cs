using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accord.Vision.Detection;
using Accord.Vision.Detection.Cascades;
using AForge.Video;
using AForge.Video.DirectShow;

namespace winlocker
{
    public partial class Form1 : Form
    {
        int _qcount = 0;
        VideoCaptureDevice _videoSource;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
            objKeyboardProcess = new LowLevelKeyboardProc(captureKey);
            ptrHook = SetWindowsHookEx(13, objKeyboardProcess, GetModuleHandle(objCurrentModule.ModuleName), 0);

            if (Screen.AllScreens.Count() > 1)
                show2();
            this.Focus();
        }

        private void show2()
        {
            Screen[] sc;
            sc = Screen.AllScreens;
            //get all the screen width and heights 
            Form2 f = new Form2();
            f.FormBorderStyle = FormBorderStyle.None;
            f.Left = sc[0].Bounds.Width;
            f.Top = sc[0].Bounds.Height;
            f.StartPosition = FormStartPosition.Manual;
            f.Location = sc[0].Bounds.Location;
            Point p = new Point(sc[0].Bounds.Location.X, sc[0].Bounds.Location.Y);
            f.Location = p;
            f.WindowState = FormWindowState.Maximized;
            f.Show();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            var events = e;

            if (e.Alt == true)
                e.Handled = true;

            if (e.KeyValue == 8)
            {
                if (_qcount > 4)
                    Close();
            }
            if (e.KeyValue == 81)
            {
                _qcount++;
            }

        }

        /* Code to Disable WinKey, Alt+Tab, Ctrl+Esc Starts Here */


        // Structure contain information about low-level keyboard input event 
        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public Keys key;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr extra;
        }
        //System level functions to be used for hook and unhook keyboard input  
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc callback, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool LockWorkStation();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wp, IntPtr lp);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern short GetAsyncKeyState(Keys key);
        //Declaring Global objects     
        private IntPtr ptrHook;
        private LowLevelKeyboardProc objKeyboardProcess;

        private IntPtr captureKey(int nCode, IntPtr wp, IntPtr lp)
        {
            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT objKeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lp, typeof(KBDLLHOOKSTRUCT));

                // Disabling Windows keys 

                if (objKeyInfo.key == Keys.RWin || 
                    objKeyInfo.key == Keys.LWin || 
                    objKeyInfo.key == Keys.Tab && HasAltModifier(objKeyInfo.flags) || 
                    objKeyInfo.key == Keys.Escape && (ModifierKeys & Keys.Control) == Keys.Control || 
                    objKeyInfo.key == Keys.Escape && HasAltModifier(objKeyInfo.flags))
                {
                    return (IntPtr)1; // if 0 is returned then All the above keys will be enabled
                }
            }
            return CallNextHookEx(ptrHook, nCode, wp, lp);
        }

        bool HasAltModifier(int flags)
        {
            return (flags & 0x20) == 0x20;
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            getPic();
        }

        private void getPic()
        {
            var videoDevs = new FilterInfoCollection(
                FilterCategory.VideoInputDevice);
            if (videoDevs.Count>0)
            {
                _videoSource = new VideoCaptureDevice(videoDevs[0].MonikerString);
                _videoSource.NewFrame += new NewFrameEventHandler(newFrame);
                _videoSource.Start();
            }
            if (!LockWorkStation())
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        private void newFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var bitmap = (Bitmap)eventArgs.Frame.Clone();

            // Is there a face?
            var detector = new HaarObjectDetector(new FaceHaarCascade(),30);

            detector.SearchMode = ObjectDetectorSearchMode.Single;
            detector.ScalingFactor = 1.5F;
            detector.ScalingMode = ObjectDetectorScalingMode.GreaterToSmaller;
            detector.UseParallelProcessing = true;

            var faceObjects = detector.ProcessFrame(bitmap);
            if (faceObjects.Count() > 0)
            {
                var g  = Graphics.FromImage(bitmap);
                foreach (var face in faceObjects)
                    g.DrawRectangle(Pens.DeepSkyBlue, face);
                g.Dispose();
            }

            var tempFilename = Path.GetRandomFileName();
            bitmap.Save(string.Format(@"c:\temp\{0}",tempFilename+".png"), System.Drawing.Imaging.ImageFormat.Png);
            if (_videoSource != null)
                _videoSource.SignalToStop();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_videoSource != null)
                _videoSource.SignalToStop();
        }
        /* Code to Disable WinKey, Alt+Tab, Ctrl+Esc Ends Here */

    }
}

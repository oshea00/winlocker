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
    public partial class Form2 : Form
    {
        int _qcount = 0;
        int _rcount = 0;
        VideoCaptureDevice _videoSource;

        public Form2()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool LockWorkStation();

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

            var tempFilename = Path.GetRandomFileName();
            bitmap.Save(string.Format(@"c:\temp\{0}",tempFilename+".png"), System.Drawing.Imaging.ImageFormat.Png);
            if (_videoSource != null)
                _videoSource.SignalToStop();
        }

        private void Form2_MouseClick_1(object sender, MouseEventArgs e)
        {
            getPic();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_videoSource != null)
                _videoSource.SignalToStop();
        }
        /* Code to Disable WinKey, Alt+Tab, Ctrl+Esc Ends Here */

    }
}

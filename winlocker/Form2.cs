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

        public Form1 form1 { get; set; }

        public Form2()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Form2_MouseClick_1(object sender, MouseEventArgs e)
        {
            form1.getPic();
        }
    }
}

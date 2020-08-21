using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();

            mapControl1.TileServer = new MapControl.OpenTopoMapServer();
            mapControl1.ZoomLevel = 10;
            mapControl1.CenterLon = 44.0;
            mapControl1.CenterLat = 56.3333333;
        }

        private void mapControl1_MouseMove(object sender, MouseEventArgs e)
        {
            this.Text = $"{mapControl1.MouseLon} / {mapControl1.MouseLat}";
        }

        private void mapControl1_Paint(object sender, PaintEventArgs e)
        {
            var p = mapControl1.GetProjection(44.0, 56.3333);
           
            p.X = p.X % mapControl1.FullMapSizeInPixels;
            do
            {
                e.Graphics.FillEllipse(Brushes.Red, p.X - 1, p.Y - 1, 3, 3);
                p.X += mapControl1.FullMapSizeInPixels;
            }
            while (p.X >= 0 && p.X <= Width);
        }
    }
}

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

            ITileServer[] tileServers = new ITileServer[]
            {
                new OpenStreetMapTileServer(),
                new StamenTerrainTileServer(),
                new OpenTopoMapServer(),
            };

            cmbTileServers.Items.AddRange(tileServers);
            cmbTileServers.SelectedIndex = 0;
            
            mapControl.ZoomLevel = 5;
            mapControl.CenterLon = 44.0;
            mapControl.CenterLat = 56.3333333;
        }

        private void mapControl_MouseMove(object sender, MouseEventArgs e)
        {
            this.Text = $"{mapControl.MouseLon} / {mapControl.MouseLat}";
        }

        private void mapControl_Paint(object sender, PaintEventArgs e)
        {
            var p = mapControl.GetProjection(44.0, 56.3333);
           
            p.X = p.X % mapControl.FullMapSizeInPixels;
            do
            {
                e.Graphics.FillEllipse(Brushes.Red, p.X - 1, p.Y - 1, 3, 3);
                p.X += mapControl.FullMapSizeInPixels;
            }
            while (p.X >= 0 && p.X <= Width);
        }

        private void btnClearCache_Click(object sender, EventArgs e)
        {
            mapControl.ClearCache();
            ActiveControl = mapControl;
        }

        private void cmbTileServers_SelectedIndexChanged(object sender, EventArgs e)
        {
            mapControl.TileServer = cmbTileServers.SelectedItem as ITileServer;
            ActiveControl = mapControl;
        }

        private void mapControl1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void mapControl1_MouseMove(object sender, MouseEventArgs e)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoApp
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
                new OfflineTileServer(),
            };

            cmbTileServers.Items.AddRange(tileServers);
            cmbTileServers.SelectedIndex = 0;
            
            mapControl.ZoomLevel = 5;
            mapControl.Center = new GeoPoint(44.0f, 56.33333f);           
            mapControl.Markers.Add(new Marker(new GeoPoint(44.0f, 56.33333f), "Test", Brushes.Black, Brushes.Red));
            
            var centralLine = new Track(new Pen(Brushes.Red));
            centralLine.AddRange(ReadPointsFromResource("CentralLine"));

            var penumbraLimit = new Track(new Pen(Brushes.Red));
            penumbraLimit.AddRange(ReadPointsFromResource("PenumbraLimit"));

            var riseSetCurves = new Track(new Pen(Brushes.Orange));
            riseSetCurves.AddRange(ReadPointsFromResource("RiseSetCurves"));

            var shadowPath = new List<GeoPoint>();
            shadowPath.AddRange(ReadPointsFromResource("ShadowPath"));
            
            mapControl.Tracks.Add(centralLine);
            mapControl.Tracks.Add(riseSetCurves);
            mapControl.Tracks.Add(penumbraLimit);
            mapControl.Polygons.Add(shadowPath);
        }

        private ICollection<GeoPoint> ReadPointsFromResource(string resourceName)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"DemoApp.MapObjects.{resourceName}.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                var stringPoints = reader.ReadToEnd().Split('\n').Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Split(','));
                List<GeoPoint> points = new List<GeoPoint>();
                foreach (var sp in stringPoints)
                {
                    float lon = float.Parse(sp[1], CultureInfo.InvariantCulture);
                    float lat = float.Parse(sp[0], CultureInfo.InvariantCulture);
                    points.Add(new GeoPoint(lon, lat));
                }
                return points;
            }

        }

        private void mapControl_MouseMove(object sender, MouseEventArgs e)
        {
            GeoPoint g = mapControl.Mouse;
            this.Text = $"Longitude = {g.Longitude} / Latitude = {g.Latitude} / Zoom = {mapControl.ZoomLevel}";
        }

        private void mapControl_Paint(object sender, PaintEventArgs e)
        {
           
        }

        private void btnClearCache_Click(object sender, EventArgs e)
        {
            mapControl.ClearCache(true);
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

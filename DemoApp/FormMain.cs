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
        public class Sample
        {
            public string Title { get; private set; }
            public Action InitAction { get; private set; }

            public Sample(string title, Action initAction)
            {
                Title = title;
                InitAction = initAction;
            }
        }

        private void Sample1()
        {           
            var centralLine = new Track(new TrackStyle(new Pen(Color.Red) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot }));
            centralLine.AddRange(ReadPointsFromResource("CentralLine.txt"));

            var penumbraLimit = new Track(new TrackStyle(new Pen(Color.Orange, 2)));
            penumbraLimit.AddRange(ReadPointsFromResource("PenumbraLimit.txt"));

            var riseSetCurves = new Track(new TrackStyle(new Pen(Color.Orange, 2)));
            riseSetCurves.AddRange(ReadPointsFromResource("RiseSetCurves.txt"));

            var shadowPath = new Polygon(new PolygonStyle(new SolidBrush(Color.FromArgb(100, Color.Black)), Pens.Black));
            shadowPath.AddRange(ReadPointsFromResource("ShadowPath.txt"));

            mapControl.Tracks.Add(centralLine);
            mapControl.Tracks.Add(riseSetCurves);
            mapControl.Tracks.Add(penumbraLimit);
            mapControl.Polygons.Add(shadowPath);
        }

        private void Sample2()
        {
            var magellanTraveling = new Track(new TrackStyle(Pens.Blue));
            magellanTraveling.AddRange(ReadPointsFromResource("MagellanExpedition.csv", reversed: true));
            mapControl.Tracks.Add(magellanTraveling);
        }

        private void Sample3()
        {
            var cities = ReadCities();
            foreach (var city in cities)
            {
                mapControl.Markers.Add(city);
            }
        }

        public FormMain()
        {
            InitializeComponent();

            cmbExample.Items.AddRange(new Sample[]
            {
                new Sample("Solar Eclipse Map 11 Aug 1999", Sample1),
                new Sample("Magellan's Circumnavigation Map ", Sample2),
                new Sample("World Greatest Cities", Sample3)
            });

            ITileServer[] tileServers = new ITileServer[]
            {               
                new OpenStreetMapTileServer("DemoApp for WinFormsMapControl 1.0 contact example@example.com"),
                new StamenTerrainTileServer(),
                new OpenTopoMapServer(),
                new OfflineTileServer(),
            };

            cmbTileServers.Items.AddRange(tileServers);
            cmbTileServers.SelectedIndex = 0;


            cmbExample.SelectedIndex = 0;

            //mapControl.ZoomLevel = 0;
            //mapControl.Center = new GeoPoint(44.0f, 56.33333f);           
            //mapControl.Markers.Add(new Marker(new GeoPoint(44.0f, 56.33333f), "Test"));
        }

        private ICollection<Marker> ReadCities()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"DemoApp.MapObjects.Cities.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                var stringCities = reader.ReadToEnd().Split('\n').Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Split(','));
                List<Marker> markers = new List<Marker>();
                foreach (var sp in stringCities)
                {
                    string name = sp[0];
                    long population = long.Parse(sp[3]);                  
                    float lon = float.Parse(sp[1], CultureInfo.InvariantCulture);
                    float lat = float.Parse(sp[2], CultureInfo.InvariantCulture);
                    markers.Add(new Marker(new GeoPoint(lon, lat), mapControl.DefaultMarkerStyle, name) { Data = population });                    
                }

                return markers;
            }
        }

        private ICollection<GeoPoint> ReadPointsFromResource(string resourceName, bool reversed = false)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"DemoApp.MapObjects.{resourceName}"))
            using (StreamReader reader = new StreamReader(stream))
            {
                var stringPoints = reader.ReadToEnd().Split('\n').Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Split(','));
                List<GeoPoint> points = new List<GeoPoint>();
                foreach (var sp in stringPoints)
                {
                    float lon = float.Parse(sp[reversed ? 0 : 1], CultureInfo.InvariantCulture);
                    float lat = float.Parse(sp[reversed ? 1 : 0], CultureInfo.InvariantCulture);
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

        private void cmbExample_SelectedIndexChanged(object sender, EventArgs e)
        {
            mapControl.ClearOverlays();
            var sample = cmbExample.SelectedItem as Sample;
            sample.InitAction();
        }

        private void mapControl_DrawMarker(object sender, DrawMarkerEventArgs e)
        {
            
            e.Graphics.FillEllipse(Brushes.Red, e.Point.X - 20, e.Point.Y - 20, 40, 40);
            e.Handled = true;
            e.Graphics.DrawString(e.Marker.Label, e.Marker.Style.LabelFont, e.Marker.Style.LabelBrush, e.Point.X + 20 * 0.35f, e.Point.Y + 20 * 0.35f);
        }
    }
}

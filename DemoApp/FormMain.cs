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
        private Image imageMarker = Image.FromStream(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream($"DemoApp.Marker.png"));

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
            var magellanTraveling = new Track(TrackStyle.Default);
            magellanTraveling.AddRange(ReadPointsFromResource("MagellanExpedition.txt"));
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
           
            mapControl.CacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MapControl");
            
            cmbExample.Items.AddRange(new Sample[]
            {
                new Sample("Map of Solar Eclipse 11 Aug 1999", Sample1),
                new Sample("Magellan's Circumnavigation Map", Sample2),
                new Sample("World Greatest Cities", Sample3)
            });

            ITileServer[] tileServers = new ITileServer[]
            {               
                new OpenStreetMapTileServer(userAgent: "DemoApp for WinFormsMapControl 1.0 contact example@example.com"),
                new StamenTerrainTileServer(),
                new OpenTopoMapServer(),
                new OfflineTileServer(),
            };

            cmbTileServers.Items.AddRange(tileServers);
            cmbTileServers.SelectedIndex = 0;
            cmbExample.SelectedIndex = 0;
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
                    float lon = float.Parse(sp[1], CultureInfo.InvariantCulture);
                    float lat = float.Parse(sp[2], CultureInfo.InvariantCulture);
                    long population = long.Parse(sp[3]);
                    markers.Add(new Marker(new GeoPoint(lon, lat), MarkerStyle.Default, name) { Data = population });                    
                }

                return markers;
            }
        }

        private ICollection<GeoPoint> ReadPointsFromResource(string resourceName)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"DemoApp.MapObjects.{resourceName}"))
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

        private void UpdateWindowTitle()
        {
            GeoPoint g = mapControl.Mouse;
            this.Text = $"Longitude = {DegreeToString(g.Longitude, "W", "E")} / Latitude = {DegreeToString(g.Latitude, "S", "N")} / Zoom = {mapControl.ZoomLevel}";
        }

        private void mapControl_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateWindowTitle();
        }

        private void mapControl_MouseWheel(object sender, MouseEventArgs e)
        {
            UpdateWindowTitle();
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

        private void cmbExample_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActiveControl = mapControl;
            mapControl.ClearAll();
            var sample = cmbExample.SelectedItem as Sample;
            sample.InitAction();
        }

        private void mapControl_DrawMarker(object sender, DrawMarkerEventArgs e)
        {
            e.Handled = true;
            e.Graphics.DrawImage(imageMarker, new Rectangle( (int)e.Point.X - 12, (int)e.Point.Y - 24, 24, 24 ));
            if (mapControl.ZoomLevel >= 5)
            {
                e.Graphics.DrawString(e.Marker.Label, SystemFonts.DefaultFont, Brushes.Red, new PointF(e.Point.X, e.Point.Y + 5), new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near });
            }
        }

        private void mapControl_DoubleClick(object sender, EventArgs e)
        {
            var coord = mapControl.Mouse;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Latitude: {DegreeToString(coord.Latitude, "S", "N")}");
            sb.AppendLine($"Longitude: {DegreeToString(coord.Longitude, "W", "E")}");
            MessageBox.Show(sb.ToString(), "Info");
        }

        private string DegreeToString(double coordinate, string negativeSym, string positiveSym)
        {
            string sym = coordinate < 0d ? negativeSym : positiveSym;
            coordinate = Math.Abs(coordinate);
            double d = Math.Floor(coordinate);
            coordinate -= d;
            coordinate *= 60;
            double m = Math.Floor(coordinate);
            coordinate -= m;
            coordinate *= 60;
            double s = coordinate;
            string dd = d.ToString();
            string mm = m.ToString().PadLeft(2, '0');
            string ss = s.ToString("00.00", CultureInfo.InvariantCulture);
            return $"{dd}° {mm}' {ss}\" {sym}";
        }
    }
}

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
using System.Windows.Forms.Maps.Layers;

namespace DemoApp
{
    public partial class FormMain : Form
    {
        private Image imageMarker = Image.FromStream(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream($"DemoApp.Marker.png"));

        private MarkerLayer markerLayer = new MarkerLayer(3);
        private TrackLayer trackLayer = new TrackLayer(2);
        private PolygonLayer polygonLayer = new PolygonLayer(1);
        private EllipseLayer ellipseLayer = new EllipseLayer(4);

        private LayerGroup sample1LayerGroup = new LayerGroup();
        private LayerGroup sample2LayerGroup = new LayerGroup();
        private LayerGroup sample3LayerGroup = new LayerGroup();
        private LayerGroup sample4LayerGroup = new LayerGroup();

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

        private void Sample0()
        {
            trackLayer.Tracks.Clear();
            markerLayer.Markers.Clear();
            polygonLayer.Polygons.Clear();
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

            trackLayer.Tracks.Clear();
            trackLayer.Tracks.Add(centralLine);
            trackLayer.Tracks.Add(riseSetCurves);
            trackLayer.Tracks.Add(penumbraLimit);

            polygonLayer.Polygons.Clear();
            polygonLayer.Polygons.Add(shadowPath);

            mapControl.Layers.Clear();
            mapControl.Layers.Add(sample1LayerGroup);
            mapControl.Invalidate();
        }

        private void Sample2()
        {
            var magellanTraveling = new Track(TrackStyle.Default);
            magellanTraveling.AddRange(ReadPointsFromResource("MagellanExpedition.txt"));

            trackLayer.Tracks.Clear();
            trackLayer.Tracks.Add(magellanTraveling);

            mapControl.Layers.Clear();
            mapControl.Layers.Add(sample2LayerGroup);
            mapControl.Invalidate();
        }

        private void Sample3()
        {
            var cities = ReadCities();

            markerLayer.Markers.Clear();
            foreach (var city in cities)
            {
                markerLayer.Markers.Add(city);
            }

            mapControl.Layers.Clear();
            mapControl.Layers.Add(sample3LayerGroup);
            mapControl.Invalidate();
        }

        private void Sample4()
        {
            ellipseLayer.Ellipses.Clear();

            ellipseLayer.Ellipses.Add(new Ellipse(new GeoPoint(13.376935f, 52.516181f), new EllipseStyle(50, 50, new SolidBrush(Color.FromArgb(80, Color.Blue)), Pens.Blue, EllipseStyle.Unit.METERS)));
            ellipseLayer.Ellipses.Add(new Ellipse(new GeoPoint(12.482932f, 41.89332f), new EllipseStyle(50, 50, new SolidBrush(Color.FromArgb(80, Color.Blue)), Pens.Blue, EllipseStyle.Unit.METERS)));
            ellipseLayer.Ellipses.Add(new Ellipse(new GeoPoint(-21.942237f, 64.145981f), new EllipseStyle(50, 50, new SolidBrush(Color.FromArgb(80, Color.Blue)), Pens.Blue, EllipseStyle.Unit.METERS)));
            ellipseLayer.Ellipses.Add(new Ellipse(new GeoPoint(-118.242766f, 34.053691f), new EllipseStyle(50, 50, new SolidBrush(Color.FromArgb(80, Color.Blue)), Pens.Blue, EllipseStyle.Unit.METERS)));

            mapControl.Layers.Clear();
            mapControl.Layers.Add(sample4LayerGroup);
            mapControl.Invalidate();
        }

        public FormMain()
        {
            InitializeComponent();
           
            mapControl.CacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MapControl");

            // group layers together
            sample1LayerGroup.Layers.Add(trackLayer);
            sample1LayerGroup.Layers.Add(polygonLayer);

            sample2LayerGroup.Layers.Add(trackLayer);

            sample3LayerGroup.Layers.Add(markerLayer);

            sample4LayerGroup.Layers.Add(ellipseLayer);
            
            cmbExample.Items.AddRange(new Sample[]
            {
                new Sample("Empty Map", Sample0),
                new Sample("Map of Solar Eclipse 11 Aug 1999", Sample1),
                new Sample("Magellan's Circumnavigation Map", Sample2),
                new Sample("World Greatest Cities", Sample3),
                new Sample("Some Ellipses with 50m Diameter", Sample4)
            });

            ITileServer[] tileServers = new ITileServer[]
            {               
                new OpenStreetMapTileServer(userAgent: "DemoApp for WinFormsMapControl 1.0 contact example@example.com"),
                new StamenTerrainTileServer(),
                new OpenTopoMapServer(),
                new OfflineTileServer(),
                new BingMapsAerialTileServer(),
                new BingMapsRoadsTileServer(),
                new BingMapsHybridTileServer(),
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
            this.Text = $"Mouse = {g} / Zoom = {mapControl.ZoomLevel} / Bounding Box = TL:{mapControl.TopLeft}, TR:{mapControl.TopRight}, BR:{mapControl.BottomRight}, BL:{mapControl.BottomLeft}";
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

        private void cbxMarkerLayer_CheckedChanged(object sender, EventArgs e)
        {
            markerLayer.Visible = cbxMarkerLayer.Checked;
            mapControl.Invalidate();
        }

        private void cbxTrackLayer_CheckedChanged(object sender, EventArgs e)
        {
            trackLayer.Visible = cbxTrackLayer.Checked;
            mapControl.Invalidate();
        }

        private void cbxPolygonLayer_CheckedChanged(object sender, EventArgs e)
        {
            polygonLayer.Visible = cbxPolygonLayer.Checked;
            mapControl.Invalidate();
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
            sb.AppendLine($"Location: {coord}");
            MessageBox.Show(sb.ToString(), "Info");
        }
    }
}

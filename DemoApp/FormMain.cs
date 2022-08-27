using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Maps.Common;
using System.Windows.Forms.Maps.Elements;
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
        private EllipseLayer dynamicEllipseLayer = new EllipseLayer(0);

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
            trackLayer.Clear();
            markerLayer.Clear();
            polygonLayer.Clear();
            ellipseLayer.Clear();

            dynamicEllipseLayer.Clear();
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

            trackLayer.Clear();
            trackLayer.AddTrack(centralLine);
            trackLayer.AddTrack(riseSetCurves);
            trackLayer.AddTrack(penumbraLimit);

            polygonLayer.Clear();
            polygonLayer.AddPolygon(shadowPath);

            sample1LayerGroup.Visible = true;
            sample2LayerGroup.Visible = false;
            sample3LayerGroup.Visible = false;
            sample4LayerGroup.Visible = false;
        }

        private void Sample2()
        {
            var magellanTraveling = new Track(TrackStyle.Default);
            magellanTraveling.AddRange(ReadPointsFromResource("MagellanExpedition.txt"));

            trackLayer.Clear();
            trackLayer.AddTrack(magellanTraveling);

            sample1LayerGroup.Visible = false;
            sample2LayerGroup.Visible = true;
            sample3LayerGroup.Visible = false;
            sample4LayerGroup.Visible = false;
        }

        private void Sample3()
        {
            var cities = ReadCities();

            markerLayer.Clear();
            foreach (var city in cities)
            {
                markerLayer.AddMarker(city);
            }

            sample1LayerGroup.Visible = false;
            sample2LayerGroup.Visible = false;
            sample3LayerGroup.Visible = true;
            sample4LayerGroup.Visible = false;
        }

        private void Sample4()
        {
            ellipseLayer.Clear();

            ellipseLayer.AddEllipse(new Ellipse(new GeoPoint(13.376935f, 52.516181f), new EllipseStyle(50, 50, new SolidBrush(Color.FromArgb(80, Color.Blue)), Pens.Blue, EllipseStyle.Unit.METERS)));
            ellipseLayer.AddEllipse(new Ellipse(new GeoPoint(8.702953f, 48.890885f), new EllipseStyle(50, 50, new SolidBrush(Color.FromArgb(80, Color.Blue)), Pens.Blue, EllipseStyle.Unit.METERS)));
            ellipseLayer.AddEllipse(new Ellipse(new GeoPoint(8.682092f, 50.110644f), new EllipseStyle(50, 50, new SolidBrush(Color.FromArgb(80, Color.Blue)), Pens.Blue, EllipseStyle.Unit.METERS)));
            ellipseLayer.AddEllipse(new Ellipse(new GeoPoint(8.043878f, 52.846134f), new EllipseStyle(50, 50, new SolidBrush(Color.FromArgb(80, Color.Blue)), Pens.Blue, EllipseStyle.Unit.METERS)));

            sample1LayerGroup.Visible = false;
            sample2LayerGroup.Visible = false;
            sample3LayerGroup.Visible = false;
            sample4LayerGroup.Visible = true;
        }

        private void Sample5()
        {
            mapControl.ClearElements();
            mapControl.AddMarker(new Marker(new GeoPoint(0, 0)));

            mapControl.Center = new GeoPoint(0, 0);
            mapControl.ZoomLevel = 15;
        }

        public FormMain()
        {
            InitializeComponent();
           
            mapControl.CacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MapControl");

            // group layers together
            sample1LayerGroup.AddLayer(trackLayer);
            sample1LayerGroup.AddLayer(polygonLayer);

            sample2LayerGroup.AddLayer(trackLayer);

            sample3LayerGroup.AddLayer(markerLayer);

            sample4LayerGroup.AddLayer(ellipseLayer);

            // add layers to map
            mapControl.AddLayer(sample1LayerGroup);
            mapControl.AddLayer(sample2LayerGroup);
            mapControl.AddLayer(sample3LayerGroup);
            mapControl.AddLayer(sample4LayerGroup);
            mapControl.AddLayer(dynamicEllipseLayer);
            
            cmbExample.Items.AddRange(new Sample[]
            {
                new Sample("Empty Map", Sample0),
                new Sample("Map of Solar Eclipse 11 Aug 1999", Sample1),
                new Sample("Magellan's Circumnavigation Map", Sample2),
                new Sample("World Greatest Cities", Sample3),
                new Sample("Some Ellipses with 50m Diameter", Sample4),
                new Sample("Center Marker directly in map", Sample5),
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
                    markers.Add(new Marker(new GeoPoint(lon, lat), new MarkerStyle(imageMarker), name) { Data = population });                    
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
            mapControl.ClearElements();
            var sample = cmbExample.SelectedItem as Sample;
            sample.InitAction();
        }

        private void cbxMarkerLayer_CheckedChanged(object sender, EventArgs e)
        {
            markerLayer.Visible = cbxMarkerLayer.Checked;
        }

        private void cbxTrackLayer_CheckedChanged(object sender, EventArgs e)
        {
            trackLayer.Visible = cbxTrackLayer.Checked;
        }

        private void cbxPolygonLayer_CheckedChanged(object sender, EventArgs e)
        {
            polygonLayer.Visible = cbxPolygonLayer.Checked;
        }

        private void cbxEllipseLayer_CheckedChanged(object sender, EventArgs e)
        {
            ellipseLayer.Visible = cbxEllipseLayer.Checked;
        }

        private void cbxDynamicEllipseLayer_CheckedChanged(object sender, EventArgs e)
        {
            dynamicEllipseLayer.Visible = cbxDynamicEllipseLayer.Checked;
        }

        private void btnAddDynamicEllipse_Click(object sender, EventArgs e)
        {
            dynamicEllipseLayer.AddEllipse(new Ellipse(new GeoPoint(13.376935f, 52.516181f), new EllipseStyle(500, 300, new SolidBrush(Color.FromArgb(80, Color.Red)), Pens.Red, EllipseStyle.Unit.METERS)));
        }

        private void btnZoomPolygonLayer_Click(object sender, EventArgs e)
        {
            mapControl.ZoomTo(polygonLayer);
        }

        private void btnZoomEllipseLayer_Click(object sender, EventArgs e)
        {
            mapControl.ZoomTo(ellipseLayer);
        }

        private void btnZoomSampleLayerGroup1_Click(object sender, EventArgs e)
        {
            mapControl.ZoomTo(sample1LayerGroup);
        }

        private void btnZoomBerlin_Click(object sender, EventArgs e)
        { 
            mapControl.ZoomLevel = 10;
            mapControl.Center = new GeoPoint(13.376935f, 52.516181f);
        }

        private void mapControl_DrawMarker(object sender, DrawMarkerEventArgs e)
        {
            //e.Handled = true;
        }

        private void mapControl_DoubleClick(object sender, EventArgs e)
        {
            var coord = mapControl.Mouse;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Location: {coord}");
            MessageBox.Show(sb.ToString(), "Info");
        }

        private void mapControl_ElementClick(object sender, MapControlElementEventArgs e)
        {
            Debug.WriteLine(e.Element.GetType().Name + " clicked!");
        }

        private void mapControl_ElementEnter(object sender, MapControlElementEventArgs e)
        {
            Debug.WriteLine(e.Element.GetType().Name + " entered!");
        }

        private void mapControl_ElementLeave(object sender, MapControlElementEventArgs e)
        {
            Debug.WriteLine(e.Element.GetType().Name + " leaved!");
        }
    }
}

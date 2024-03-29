﻿using System;
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

        private void Sample0()
        {

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

            string userAgent = "DemoApp for WinFormsMapControl 1.0 contact example@example.com";

            mapControl.Layers.Add(new Layer() { TileServer = new YandexSatelliteMapsTileServer(userAgent), ZIndex = 0, Opacity = 1f });

            mapControl.CacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MapControl");

            cmbExample.Items.AddRange(new Sample[]
            {
                new Sample("Empty Map", Sample0),
                new Sample("Map of Solar Eclipse 11 Aug 1999", Sample1),
                new Sample("Magellan's Circumnavigation Map", Sample2),
                new Sample("World Greatest Cities", Sample3)
            });

            ITileServer[] tileServers = new ITileServer[]
            {
                new OpenTopoMapServer(),
                new OpenStreetMapTileServer(userAgent),
                new StamenTerrainTileServer(),
                new OfflineTileServer(),
                new BingMapsAerialTileServer(),
                new BingMapsRoadsTileServer(),
                new BingMapsHybridTileServer(),
                new GoogleMapsSatelliteTileServer(userAgent),
                new GoogleMapsRoadmapTileServer(userAgent),
                new GoogleMapsHybridTileServer(userAgent),
                new YandexSatelliteMapsTileServer(userAgent),
                new YandexRoadMapsTileServer(userAgent),
                new EsriSatelliteMapsTileServer(userAgent),
                new DoubleGisTileServer(userAgent),
                new WikimapiaTileServer(userAgent),
            };

            object[] overlays = new object[]
            {
                "None",
                new YandexRoadsOverlayTileServer(userAgent)
            };

            cmbTileServers.Items.AddRange(tileServers);
            cmbTileServers.SelectedIndex = 0;

            cmbOverlay.Items.AddRange(overlays);
            cmbOverlay.SelectedIndex = 0;

            cmbExample.SelectedIndex = 0;

            mapControl.ZoomLevel = 12;
            mapControl.Center = new GeoPoint(44.0f, 56.333f);
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
            var tileServer = cmbTileServers.SelectedItem as ITileServer;
            mapControl.Layers[0] = new Layer() { Opacity = 1, TileServer = tileServer, ZIndex = 1 };
            
            ActiveControl = mapControl;
        }

        private void cmbOverlay_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tileServer = cmbOverlay.SelectedItem as ITileServer;
            if (mapControl.Layers.Count == 1)
            {
                if (cmbOverlay.SelectedItem is ITileServer)
                {
                    mapControl.Layers.Add(new Layer() { TileServer = tileServer, ZIndex = 1, Opacity = 1 });
                }
            }
            else if (mapControl.Layers.Count == 2)
            {
                if (cmbOverlay.SelectedItem is string)
                {
                    mapControl.Layers.RemoveAt(1);
                }
                else
                {
                    mapControl.Layers[1] = new Layer() { Opacity = 1, TileServer = tileServer, ZIndex = 1 };
                }
            }

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
            sb.AppendLine($"Location: {coord}");
            MessageBox.Show(sb.ToString(), "Info");
        }
    }
}

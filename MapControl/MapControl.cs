using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Windows.Forms
{

    public class DrawMarkerEventArgs : HandledEventArgs
    {
        public Marker Marker { get; internal set; }
        public Graphics Graphics { get; internal set; }
        public PointF Point { get; internal set; }
        internal DrawMarkerEventArgs() { }
    }

    public class DrawTrackSegmentArgs : HandledEventArgs
    {
        public Track Track { get; internal set; }
        public Graphics Graphics { get; internal set; }
        public PointF Point1 { get; internal set; }
        public PointF Point2 { get; internal set; }
        internal DrawTrackSegmentArgs() { }
    }

    /// <summary>
    /// Map control for displaying online and offline maps.
    /// </summary>
    [DesignerCategory("code")]
    public partial class MapControl : Control
    {
        /// <summary>
        /// Tile size, in pixels
        /// </summary>
        private const int TILE_SIZE = 256;

        /// <summary>
        /// First tile offset
        /// </summary>
        private Point _Offset = new Point();

        /// <summary>
        /// Flag indicating thar mouse is captures
        /// </summary>
        private bool _MouseCaptured = false;

        /// <summary>
        /// Last known mouse position
        /// </summary>
        private Point _LastMouse = new Point();

        /// <summary>
        /// Cache used to store tile images
        /// </summary>
        private ConcurrentBag<CachedImage> _Cache = new ConcurrentBag<CachedImage>();

        /// <summary>
        /// Link label displaying in the bottom right corner of the map with attribution text
        /// </summary>
        private HtmlLinkLabel _LinkLabel;

        /// <summary>
        /// Gets size of map in tiles
        /// </summary>
        private int FullMapSizeInTiles => 1 << ZoomLevel;

        /// <summary>
        /// Gets maps size in pixels
        /// </summary>
        private long FullMapSizeInPixels => FullMapSizeInTiles * TILE_SIZE;

        /// <summary>
        /// Backing field for <see cref="ZoomLevel"/> property
        /// </summary>
        private int _ZoomLevel = 0;

        /// <summary>
        /// Map zoom level
        /// </summary>
        [Description("Map zoom level"), Category("Behavior")]
        public int ZoomLevel
        {
            get => _ZoomLevel;
            set
            {
                if (value < 0 || value > 19)
                    throw new ArgumentException($"{value} is an incorrect value for {nameof(ZoomLevel)} property. Value should be in range from 0 to 19.");

                SetZoomLevel(value, new Point(Width / 2, Height / 2));
            }
        }

        /// <summary>
        /// Backing field for <see cref="CacheFolder"/> property
        /// </summary>
        private string _CacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MapControl");
        
        /// <summary>
        /// Path to tile cache folder
        /// </summary>
        [Browsable(false)]
        public string CacheFolder
        {
            get => _CacheFolder;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException($"{nameof(CacheFolder)} property value should not be empty.");

                if (value.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                    throw new ArgumentException($"{value} is an incorrect value for {nameof(CacheFolder)} property.");

                _CacheFolder = value;
                SetTileServerCacheFolder();
            }
        }

        /// <summary>
        /// Minimal zoom level backing field
        /// </summary>
        private int _MinZoomLevel = 0;

        /// <summary>
        /// Minimal zoom level
        /// </summary>
        [Description("Minimal zoom level"), Category("Behavior")]
        public int MinZoomLevel
        {
            get => _MinZoomLevel;
            set
            {
                if (value < 0 || value > 19)
                    throw new ArgumentException($"{value} is an incorrect value for {nameof(MinZoomLevel)} property. Value should be in range from 0 to 19.");

                _MinZoomLevel = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Maximal zoom level backing field
        /// </summary>
        private int _MaxZoomLevel = 19;

        /// <summary>
        /// Maximal zoom level
        /// </summary>
        [Description("Maximal zoom level"), Category("Behavior")]
        public int MaxZoomLevel
        {
            get => _MaxZoomLevel;
            set
            {
                if (value < 0 || value > 19)
                    throw new ArgumentException($"{value} is an incorrect value for {nameof(MaxZoomLevel)} property. Value should be in range from 0 to 19.");

                _MaxZoomLevel = value;
                Invalidate();
            }
        }
       
        /// <summary>
        /// Backing field for <see cref="TileServer"/> property
        /// </summary>
        private ITileServer _TileServer;

        /// <summary>
        /// Map tile server
        /// </summary>
        [Browsable(false)]
        public ITileServer TileServer
        {
            get => _TileServer;
            set
            {
                if (_TileServer != null)
                {
                    _TileServer.InvalidateRequired -= Invalidate;
                }

                _TileServer = value;
                _LinkLabel.Links.Clear();
                _LinkLabel.Visible = false;

                if (value != null)
                {
                    _TileServer.InvalidateRequired += Invalidate;

                    SetTileServerCacheFolder();
                    _Cache = new ConcurrentBag<CachedImage>();

                    if (_TileServer.AttributionText != null)
                    {
                        _LinkLabel.Text = _TileServer.AttributionText;
                        _LinkLabel.Visible = true;
                        OnSizeChanged(new EventArgs());
                    }

                    if (ZoomLevel > TileServer.MaxZoomLevel)
                        ZoomLevel = TileServer.MaxZoomLevel;

                    if (ZoomLevel < TileServer.MinZoomLevel)
                        ZoomLevel = TileServer.MinZoomLevel;
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets geographical coordinates of map center
        /// </summary>
        [Browsable(false)]
        public GeoPoint Center
        {
            get
            {
                float x = ArrangeTileNumber(-(_Offset.X - Width / 2) / TILE_SIZE);
                float y = -(_Offset.Y - Height / 2) / TILE_SIZE;
                return TileToWorldPos(x, y);
            }
            set
            {
                var center = WorldToTilePos(value);
                _Offset.X = -(int)(center.X * TILE_SIZE) + Width / 2;
                _Offset.Y = -(int)(center.Y * TILE_SIZE) + Height / 2;
                _Offset.X = (int)(_Offset.X % FullMapSizeInPixels);

                Invalidate();
            }
        }


        /// <summary>
        /// Gets or sets geographical coordinates of current position of mouse
        /// </summary>
        [Browsable(false)]
        public GeoPoint Mouse
        {
            get
            {
                float x = ArrangeTileNumber(-(float)(_Offset.X - _LastMouse.X) / TILE_SIZE);
                float y = -(float)(_Offset.Y - _LastMouse.Y) / TILE_SIZE;
                return TileToWorldPos(x, y);
            }
        }

        /// <summary>
        /// Gets collection of markers to be displayed on the map
        /// </summary>
        [Browsable(false)]
        public ICollection<Marker> Markers { get; } = new List<Marker>();

        /// <summary>
        /// Gets collection of tracks to be displayed on the map
        /// </summary>
        [Browsable(false)]
        public ICollection<Track> Tracks { get; } = new List<Track>();

        /// <summary>
        /// Gets collection of polygons to be displayed on the map
        /// </summary>
        [Browsable(false)]
        public ICollection<Polygon> Polygons { get; } = new List<Polygon>();

        /// <summary>
        /// Default style for map markers
        /// </summary>
        [Browsable(false)]
        public MarkerStyle DefaultMarkerStyle { get; set; } = new MarkerStyle(Brushes.Red, 3, Brushes.Black, SystemFonts.DefaultFont);

        /// <summary>
        /// Default style for tracks
        /// </summary>
        [Browsable(false)]
        public TrackStyle DefaultTrackStyle { get; set; } = new TrackStyle(Pens.Red);

        /// <summary>
        /// Default style for polygons
        /// </summary>
        [Browsable(false)]
        public PolygonStyle DefaultPolygonStyle { get; set; } = new PolygonStyle(new SolidBrush(Color.FromArgb(100, Color.Black)), new Pen(Brushes.Blue, 3) { DashStyle = DashStyle.Dot });

        /// <summary>
        /// Raised when marker is drawn on the map
        /// </summary>
        public event EventHandler<DrawMarkerEventArgs> DrawMarker;

        /// <summary>
        /// Raised when track segment is drawn on the map
        /// </summary>
        public event EventHandler<DrawTrackSegmentArgs> DrawTrackSegment;

        /// <summary>
        /// Creates new <see cref="MapControl"/> control.
        /// </summary>
        public MapControl()
        {
            InitializeComponent();
            DoubleBuffered = true;
            Cursor = Cursors.Cross;

            _LinkLabel = new HtmlLinkLabel() { Text = "", BackColor = Color.FromArgb(100, BackColor) };
            _LinkLabel.AutoSize = true;
            _LinkLabel.ForeColor = ForeColor;

            _LinkLabel.Margin = new Padding(2);
            _LinkLabel.LinkClicked += _LinkLabel_LinkClicked;

            Controls.Add(_LinkLabel);

            
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            Center = new GeoPoint(0, 0);
        }

        private void _LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString());
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (!DesignMode)
            {
                pe.Graphics.SmoothingMode = SmoothingMode.None;
                DrawTiles(pe.Graphics);
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                DrawPolygons(pe.Graphics);
                DrawTracks(pe.Graphics);
                DrawMarkers(pe.Graphics);
            }

            base.OnPaint(pe);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            _LinkLabel.Left = Width - _LinkLabel.Width;
            _LinkLabel.Top = Height - _LinkLabel.Height;

            base.OnSizeChanged(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _MouseCaptured = true;
                _LastMouse.X = e.X;
                _LastMouse.Y = e.Y;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _MouseCaptured = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_MouseCaptured)
            {
                _Offset.X += (e.X - _LastMouse.X);
                _Offset.Y += (e.Y - _LastMouse.Y);

                _Offset.X = (int)(_Offset.X % FullMapSizeInPixels);

                
                if (_Offset.Y < -(int)FullMapSizeInPixels)
                    _Offset.Y = -(int)FullMapSizeInPixels;

                if (_Offset.Y > Height)
                    _Offset.Y = Height;

                Invalidate();
            }

            _LastMouse.X = e.X;
            _LastMouse.Y = e.Y;

            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int z = ZoomLevel;

            if (e.Delta > 0)
                z = ZoomLevel + 1;
            else if (e.Delta < 0)
                z = ZoomLevel - 1;

            SetZoomLevel(z, new Point(e.X, e.Y));

            base.OnMouseWheel(e);
        }

        private void SetTileServerCacheFolder()
        {
            if (_TileServer is WebTileServer webTileServer)
            {
                webTileServer.CacheFolder = Path.Combine(_CacheFolder, _TileServer.GetType().Name);
            }
        }

        private float ArrangeTileNumber(float n)
        {
            int size = FullMapSizeInTiles;
            return (n %= size) >= 0 ? n : (n + size);
        }

        private void DrawTiles(Graphics g)
        {
            // indices of first visible tile
            int fromX = (int)Math.Floor(-(float)_Offset.X / TILE_SIZE);
            int fromY = (int)Math.Floor(-(float)_Offset.Y / TILE_SIZE);

            // count of visible tiles (vertically and horizontally)
            int tilesByWidth = (int)Math.Ceiling((float)Width / TILE_SIZE);
            int tilesByHeight = (int)Math.Ceiling((float)Height / TILE_SIZE);

            int toX = fromX + tilesByWidth;
            int toY = fromY + tilesByHeight;

            foreach (var c in _Cache)
            {
                c.Used = false;
            }

            for (int x = fromX; x <= toX; x++)
            {
                for (int y = fromY; y <= toY; y++)
                {
                    int x_ = (int)ArrangeTileNumber(x);
                    if (y >= 0 && y < FullMapSizeInTiles)
                    {
                        Image tile = GetTile(x_, y, ZoomLevel);
                        if (tile != null)
                        {
                            DrawTile(g, x, y, tile);
                        }
                    }
                }
            }

            // Dispose images that were not used 
            _Cache.Where(c => !c.Used).ToList().ForEach(c => c.Image.Dispose());

            // Update cache, leave only used images
            _Cache = new ConcurrentBag<CachedImage>(_Cache.Where(c => c.Used));
        }

        private void DrawMarkers(Graphics gr)
        {
            foreach (Marker m in Markers)
            {
                var p = Project(m.Point);

                var labelFont = m.Style != null ? m.Style.LabelFont : DefaultMarkerStyle.LabelFont;
                var labelBrush = m.Style != null ? m.Style.LabelBrush : DefaultMarkerStyle.LabelBrush;
                var markerWidth = m.Style != null ? m.Style.MarkerWidth : DefaultMarkerStyle.MarkerWidth;
                var markerBrush = m.Style != null ? m.Style.MarkerBrush : DefaultMarkerStyle.MarkerBrush;

                Draw(gr, () =>
                {
                    if (gr.IsVisible(p))
                    {
                        var eventArgs = new DrawMarkerEventArgs()
                        {
                            Graphics = gr,
                            Marker = m,
                            Point = p
                        };
                        DrawMarker?.Invoke(this, eventArgs);
                        if (!eventArgs.Handled)
                        {
                            if (markerBrush != null)
                            {
                                gr.FillEllipse(markerBrush, p.X - markerWidth / 2, p.Y - markerWidth / 2, markerWidth, markerWidth);
                            }

                            if (labelFont != null && labelBrush != null)
                            {
                                gr.DrawString(m.Label, labelFont, labelBrush, new PointF(p.X + markerWidth * 0.35f, p.Y + markerWidth * 0.35f));
                            }
                        }                            
                    }
                });
            }
        }

        private void DrawTracks(Graphics gr)
        {
            foreach (var track in Tracks)
            {
                PointF[] points = new PointF[track.Count];

                for (int i = 0; i < track.Count; i++)
                {
                    GeoPoint g = track.ElementAt(i);
                    PointF p = Project(g);
                    if (i > 0)
                    {
                        p = points[i - 1].Nearest(p, new PointF(p.X - FullMapSizeInPixels, p.Y), new PointF(p.X + FullMapSizeInPixels, p.Y));
                    }
                    points[i] = p;
                }

                Pen pen = track.Style != null ? track.Style.Pen : DefaultTrackStyle.Pen;
                Draw(gr, () => gr.DrawPolyline(pen, points));
            }
        }

        private void DrawPolygons(Graphics gr)
        {
            foreach (var polygon in Polygons)
            {
                PointF p0 = PointF.Empty;
                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.StartFigure();

                    for (int i = 0; i < polygon.Count; i++)
                    {
                        GeoPoint g = polygon.ElementAt(i);
                        PointF p = Project(g);
                        if (i > 0)
                        {
                            p = p0.Nearest(p, new PointF(p.X - FullMapSizeInPixels, p.Y), new PointF(p.X + FullMapSizeInPixels, p.Y));
                            gp.AddLine(p0, p);
                        }
                        p0 = p;
                    }

                    Brush brush = polygon.Style != null ? polygon.Style.Brush : DefaultPolygonStyle.Brush;
                    Pen pen = polygon.Style != null ? polygon.Style.Pen : DefaultPolygonStyle.Pen;

                    SolidBrush br = new SolidBrush(Color.FromArgb(100, Color.Black));

                    Draw(gr, () => gr.DrawGraphicsPath(gp, brush, pen));
                }
            }
        }

        private void Draw(Graphics gr, Action draw)
        {
            int count = (int)Math.Ceiling((double)Width / FullMapSizeInPixels) + 1;
            for (int i = -count; i < count; i++)
            {
                var state = gr.Save();
                gr.TranslateTransform(i * FullMapSizeInPixels, 0);
                draw();
                gr.Restore(state);
            }
        }

        private void SetZoomLevel(int z, Point e)
        {
            int max = TileServer != null ? Math.Min(MaxZoomLevel, TileServer.MaxZoomLevel) : MaxZoomLevel;
            int min = TileServer != null ? Math.Max(MinZoomLevel, TileServer.MinZoomLevel) : MinZoomLevel;

            if (z < min) z = min;
            if (z > max) z = max;

            if (z != ZoomLevel)
            {
                double factor = Math.Pow(2, z - ZoomLevel);
                _Offset.X = (int)((_Offset.X - e.X) * factor) + e.X;
                _Offset.Y = (int)((_Offset.Y - e.Y) * factor) + e.Y;

                _ZoomLevel = z;

                _Offset.X = (int)(_Offset.X % FullMapSizeInPixels);

                Invalidate();
            }
        }

        private void DrawTile(Graphics g, int x, int y, Image image)
        {
            Point p = new Point();
            p.X = _Offset.X + x * TILE_SIZE;
            p.Y = _Offset.Y + y * TILE_SIZE;
            g.DrawImageUnscaled(image, p);
        }

        private Image GetTile(int x, int y, int z)
        {
            try
            {
                CachedImage cached = _Cache.FirstOrDefault(c => c.X == x && c.Y == y && c.Z == z);
                if (cached != null)
                {
                    cached.Used = true;
                    return cached.Image;
                }
                else
                {
                    Image image = _TileServer.GetTile(x, y, z);
                    if (image != null)
                    {
                        cached = new CachedImage() { X = x, Y = y, Z = z, Image = image, Used = true };
                        _Cache.Add(cached);
                    }
                    return image;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets projection of geographical coordinates onto the map
        /// </summary>
        /// <param name="g">Point with geographical coordinates</param>
        /// <returns><see cref="PointF"/> object representing projection of the specified geographical coordinates on the map.</returns>
        public PointF Project(GeoPoint g)
        {
            var p = WorldToTilePos(g);
            return new PointF(p.X * TILE_SIZE + _Offset.X, p.Y * TILE_SIZE + _Offset.Y);
        }

        
        public PointF WorldToTilePos(GeoPoint g)
        {
            PointF p = new Point();
            p.X = (float)((g.Longitude + 180.0) / 360.0 * (1 << ZoomLevel));
            p.Y = (float)((1.0 - Math.Log(Math.Tan(g.Latitude * Math.PI / 180.0) +
                1.0 / Math.Cos(g.Latitude * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << ZoomLevel));

            return p;
        }

        public GeoPoint TileToWorldPos(double tile_x, double tile_y)
        {
            GeoPoint g = new GeoPoint();
            double n = Math.PI - ((2.0 * Math.PI * tile_y) / Math.Pow(2.0, ZoomLevel));

            g.Longitude = (float)((tile_x / Math.Pow(2.0, ZoomLevel) * 360.0) - 180.0);
            g.Latitude = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));

            return g;
        }

        public void ClearCache(bool allTileServers = false)
        {
            _Cache = new ConcurrentBag<CachedImage>();
            if (TileServer != null)
            {
                string cacheFolder = allTileServers ? CacheFolder : Path.Combine(CacheFolder, TileServer.GetType().Name);

                if (Directory.Exists(cacheFolder))
                {
                    try
                    {
                        Directory.Delete(cacheFolder, true);
                    }
                    catch { }
                }
            }
            Invalidate();
        }

        public void ClearOverlays()
        {
            Markers.Clear();
            Tracks.Clear();
            Polygons.Clear();
            Invalidate();
        }
    }
}

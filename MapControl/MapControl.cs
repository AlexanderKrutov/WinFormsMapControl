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
        /// Map zoom level backing field
        /// </summary>
        private int _ZoomLevel = 0;

        /// <summary>
        /// Map zoom level
        /// </summary>
        public int ZoomLevel
        {
            get => _ZoomLevel;
            set
            {
                if (value < 0 || value > TileServer?.MaxZoomLevel)
                    throw new ArgumentException($"{value} is an incorrect value for {nameof(ZoomLevel)} property.");

                _ZoomLevel = value;
                Invalidate();
            }
        }


        private string _CacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MapControl");
        public string CacheFolder
        {
            get => _CacheFolder;
            set 
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException($"{nameof(CacheFolder)} property value should not be empty.");

                if (value.IndexOfAny(Path.GetInvalidPathChars()) == -1)
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
        public int MinZoomLevel
        {
            get => _MinZoomLevel;
            set
            {
                if (value < 0 || value < TileServer?.MinZoomLevel)
                    throw new ArgumentException($"{value} is an incorrect value for {nameof(MinZoomLevel)} property.");

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
        public int MaxZoomLevel
        {
            get => _MaxZoomLevel;
            set
            {
                if (value < 0 || value > TileServer?.MaxZoomLevel)
                    throw new ArgumentException($"{value} is an incorrect value for {nameof(MaxZoomLevel)} property.");

                _MaxZoomLevel = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets size of map in tiles
        /// </summary>
        public int FullMapSizeInTiles => 1 << ZoomLevel;

        /// <summary>
        /// Gets maps size in pixels
        /// </summary>
        public long FullMapSizeInPixels => FullMapSizeInTiles * TILE_SIZE;

        private bool _MouseCaptured = false;

        private Point _LastMouse = new Point();
      
        private ConcurrentBag<CachedImage> _Cache = new ConcurrentBag<CachedImage>();

        private ITileServer _TileServer;

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
                    MaxZoomLevel = Math.Min(MaxZoomLevel, _TileServer.MaxZoomLevel);
                    MinZoomLevel = Math.Max(MinZoomLevel, _TileServer.MinZoomLevel);

                    SetTileServerCacheFolder();
                    _Cache = new ConcurrentBag<CachedImage>();

                    if (_TileServer.AttributionText != null)
                    {
                        _LinkLabel.Text = _TileServer.AttributionText;
                        _LinkLabel.Visible = true;
                        OnSizeChanged(new EventArgs());
                    }
                }
            }
        }

        private void SetTileServerCacheFolder()
        {
            if (_TileServer is WebTileServer webTileServer)
            {
                webTileServer.CacheFolder = Path.Combine(_CacheFolder, _TileServer.GetType().Name);
            }
        }

        private HtmlLinkLabel _LinkLabel;

        /// <summary>
        /// Gets or sets geographical coordinates of map center
        /// </summary>
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
        public GeoPoint Mouse
        {
            get
            {
                float x = ArrangeTileNumber(-(float)(_Offset.X - _LastMouse.X) / TILE_SIZE);
                float y = -(float)(_Offset.Y - _LastMouse.Y) / TILE_SIZE;
                return TileToWorldPos(x, y);
            }
        }        

        public ICollection<Marker> Markers { get; } = new List<Marker>();
        public ICollection<Track> Tracks { get; } = new List<Track>();
        public ICollection<ICollection<GeoPoint>> Polygons { get; } = new List<ICollection<GeoPoint>>();

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

        private void _LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString());
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

                Invalidate();
            }

            _LastMouse.X = e.X;
            _LastMouse.Y = e.Y;

            base.OnMouseMove(e);
        }

        private float ArrangeTileNumber(float n)
        {
            int size = FullMapSizeInTiles;
            return (n %= size) >= 0 ? n : (n + size);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (!DesignMode)
            {
                pe.Graphics.SmoothingMode = SmoothingMode.None;
                DrawTiles(pe.Graphics);
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                DrawMarkers(pe.Graphics);
                DrawTracks(pe.Graphics);
                DrawPolygons(pe.Graphics);
            }

            base.OnPaint(pe);
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
                var p = GetProjection(m.Point);
                Draw(gr, () => gr.FillEllipse(m.MarkerBrush ?? new SolidBrush(ForeColor), p.X - 1, p.Y - 1, 3, 3));
                if (m.Label != null)
                {
                    Draw(gr, () => gr.DrawString(m.Label, m.Font ?? Font, m.LabelBrush ?? new SolidBrush(ForeColor), new PointF(p.X + m.MarkerWidth, p.Y + m.MarkerWidth)));
                }
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
                    PointF p = GetProjection(g);
                    if (i > 0)
                    {
                        p = NearestPoint(points[i - 1], p, new PointF(p.X - FullMapSizeInPixels, p.Y), new PointF(p.X + FullMapSizeInPixels, p.Y)); 
                    }
                    points[i] = p;
                }

                Draw(gr, () => gr.DrawLines(track.Pen, points));
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
                        PointF p = GetProjection(g);
                        if (i > 0)
                        {
                            p = NearestPoint(p0, p, new PointF(p.X - FullMapSizeInPixels, p.Y), new PointF(p.X + FullMapSizeInPixels, p.Y));
                            gp.AddLine(p0, p);                            
                        }
                        p0 = p;
                    }

                    SolidBrush br = new SolidBrush(Color.FromArgb(100, Color.Black));
                    Draw(gr, () => gr.FillPath(br, gp));
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

        private PointF NearestPoint(PointF point, params PointF[] points)
        {
            return points.OrderBy(p => ((p.X - point.X) * (p.X - point.X) + (p.Y - point.Y) * (p.Y - point.Y))).First();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int z = ZoomLevel;

            if (e.Delta > 0)
                z = ZoomLevel + 1;
            else if (e.Delta < 0)
                z = ZoomLevel - 1;

            if (z < MinZoomLevel) z = MinZoomLevel;
            if (z > MaxZoomLevel) z = MaxZoomLevel;

            if (z != ZoomLevel)
            {
                double factor = Math.Pow(2, z - ZoomLevel);
                _Offset.X = (int)((_Offset.X - e.X) * factor) + e.X;
                _Offset.Y = (int)((_Offset.Y - e.Y) * factor) + e.Y;

                ZoomLevel = z;

                _Offset.X = (int)(_Offset.X % FullMapSizeInPixels);

                Invalidate();
            }

            base.OnMouseWheel(e);
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
        /// <param name="lon">Longitude, in degrees, positive East</param>
        /// <param name="lat">Latitude, in degrees</param>
        /// <returns></returns>
        public PointF GetProjection(GeoPoint g)
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

      
    }
}

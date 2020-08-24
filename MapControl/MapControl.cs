using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
                if (value < 0 || value > TileServer?.MinZoomLevel)
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
                    ClearCache();

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

        public double CenterLon
        {
            get
            {
                float x = ArrangeTileNumber(-(_Offset.X - Width / 2) / TILE_SIZE);
                float y = -(_Offset.Y - Height / 2) / TILE_SIZE;
                return TileToWorldPos(x, y).X;
            }
            set
            {
                var center = WorldToTilePos(value, CenterLat);
                _Offset.X = -(int)(center.X * TILE_SIZE) + Width / 2;
                _Offset.Y = -(int)(center.Y * TILE_SIZE) + Height / 2;
                Invalidate();
            }
        }

        public double CenterLat
        {
            get
            {
                float x = ArrangeTileNumber(-(_Offset.X - Width / 2) / TILE_SIZE);
                float y = -(_Offset.Y - Height / 2) / TILE_SIZE;
                return TileToWorldPos(x, y).Y;
            }
            set
            {
                var center = WorldToTilePos(CenterLon, value);
                _Offset.X = -(int)(center.X * TILE_SIZE) + Width / 2;
                _Offset.Y = -(int)(center.Y * TILE_SIZE) + Height / 2;
                Invalidate();
            }
        }

        public double MouseLat
        {
            get
            {
                float x = ArrangeTileNumber(-(float)(_Offset.X - _LastMouse.X) / TILE_SIZE);
                float y = -(float)(_Offset.Y - _LastMouse.Y) / TILE_SIZE;
                return TileToWorldPos(x, y).Y;
            }
        }

        public double MouseLon
        {
            get
            {
                float x = ArrangeTileNumber(-(float)(_Offset.X - _LastMouse.X) / TILE_SIZE);
                float y = -(float)(_Offset.Y - _LastMouse.Y) / TILE_SIZE;
                return TileToWorldPos(x, y).X;
            }
        }

        public ICollection<PointF> Points { get; } = new List<PointF>();

        public ICollection<ICollection<PointF>> Tracks { get; } = new List<ICollection<PointF>>();

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
                DrawTiles(pe.Graphics);
                DrawPoints(pe.Graphics);
                DrawTracks(pe.Graphics);
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

        private void DrawPoints(Graphics g)
        {
            foreach (PointF point in Points)
            {
                var p = GetProjection(point.X, point.Y);
                p.X = p.X % FullMapSizeInPixels;
                do
                {
                    g.FillEllipse(Brushes.Red, p.X - 1, p.Y - 1, 3, 3);
                    p.X += FullMapSizeInPixels;
                }
                while (p.X >= 0 && p.X <= Width);
            }
        }

        private void DrawTracks(Graphics g)
        {
            foreach (var track in Tracks)
            {
                for (int i = 0; i < track.Count - 1; i++)
                {
                    PointF point0 = track.ElementAt(i);
                    var p0 = GetProjection(point0.X, point0.Y);

                    PointF point1 = track.ElementAt(i + 1);
                    var p1 = GetProjection(point1.X, point1.Y);

                    var points = SegmentScreenIntersection(p0, p1);

                    if (points.Length == 2)
                    {
                        g.DrawLine(Pens.Red, points[0], points[1]);
                    }
                }                
            }
        }

        private PointF[] SegmentScreenIntersection(PointF p1, PointF p2)
        {
            List<PointF> crosses = new List<PointF>();

            if (IsVisible(p1))
            {
                crosses.Add(p1);
            }

            if (IsVisible(p2))
            {
                crosses.Add(p2);
            }

            if (crosses.Count != 2)
            {
                crosses.AddRange(EdgeCrosspoints(p1, p2));
            }

            return crosses.ToArray();
        }

        private PointF[] EdgeCrosspoints(PointF p1, PointF p2)
        {
            int width = Width;
            int height = Height;

            PointF p00 = new PointF(0, 0);
            PointF pW0 = new PointF(width, 0);
            PointF pWH = new PointF(width, height);
            PointF p0H = new PointF(0, height);

            List<PointF?> crossPoints = new List<PointF?>();

            // top edge
            crossPoints.Add(SegmentsIntersection(p1, p2, p00, pW0));

            // right edge
            crossPoints.Add(SegmentsIntersection(p1, p2, pW0, pWH));

            // bottom edge
            crossPoints.Add(SegmentsIntersection(p1, p2, pWH, p0H));

            // left edge
            crossPoints.Add(SegmentsIntersection(p1, p2, p0H, p00));

            return crossPoints.Where(p => p != null).Cast<PointF>().ToArray();
        }

        private static PointF? SegmentsIntersection(PointF p1, PointF p2, PointF p3, PointF p4)
        {
            float v1 = VectorMult(p4.X - p3.X, p4.Y - p3.Y, p1.X - p3.X, p1.Y - p3.Y);
            float v2 = VectorMult(p4.X - p3.X, p4.Y - p3.Y, p2.X - p3.X, p2.Y - p3.Y);
            float v3 = VectorMult(p2.X - p1.X, p2.Y - p1.Y, p3.X - p1.X, p3.Y - p1.Y);
            float v4 = VectorMult(p2.X - p1.X, p2.Y - p1.Y, p4.X - p1.X, p4.Y - p1.Y);

            if ((v1 * v2) < 0 && (v3 * v4) < 0)
            {
                float a1 = 0, b1 = 0, c1 = 0;
                LineEquation(p1, p2, ref a1, ref b1, ref c1);

                float a2 = 0, b2 = 0, c2 = 0;
                LineEquation(p3, p4, ref a2, ref b2, ref c2);

                double d = a1 * b2 - b1 * a2;

                double dx = -c1 * b2 + b1 * c2;
                double dy = -a1 * c2 + c1 * a2;

                return new PointF((float)(dx / d), (float)(dy / d));
            }

            return null;
        }

        private static float VectorMult(float ax, float ay, float bx, float by)
        {
            return ax * by - bx * ay;
        }

        private static void LineEquation(PointF p1, PointF p2, ref float A, ref float B, ref float C)
        {
            A = p2.Y - p1.Y;
            B = p1.X - p2.X;
            C = -p1.X * (p2.Y - p1.Y) + p1.Y * (p2.X - p1.X);
        }

        private bool IsVisible(PointF p)
        {
            return p.X >= 0 && p.X <= Width && p.Y >= 0 && p.Y <= Height;
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
        public PointF GetProjection(double lon, double lat)
        {
            var p = WorldToTilePos(lon, lat);
            return new PointF(p.X * TILE_SIZE + _Offset.X, p.Y * TILE_SIZE + _Offset.Y);
        }

        public PointF WorldToTilePos(double lon, double lat)
        {
            PointF p = new Point();
            p.X = (float)((lon + 180.0) / 360.0 * (1 << ZoomLevel));
            p.Y = (float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
                1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << ZoomLevel));

            return p;
        }

        public PointF TileToWorldPos(double tile_x, double tile_y)
        {
            PointF p = new Point();
            double n = Math.PI - ((2.0 * Math.PI * tile_y) / Math.Pow(2.0, ZoomLevel));

            p.X = (float)((tile_x / Math.Pow(2.0, ZoomLevel) * 360.0) - 180.0);
            p.Y = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));

            return p;
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

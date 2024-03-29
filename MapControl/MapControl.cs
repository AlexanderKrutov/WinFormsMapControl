﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;

namespace System.Windows.Forms
{
    /// <summary>
    /// Map control for displaying online and offline maps.
    /// </summary>
    [DesignerCategory("code")]
    public partial class MapControl : Control
    {
        /// <summary>
        /// Tile size, in pixels.
        /// </summary>
        private const int TILE_SIZE = 256;

        /// <summary>
        /// Flag indicating thar mouse is captured.
        /// </summary>
        private bool _MouseCaptured = false;

        /// <summary>
        /// Last known mouse position.
        /// </summary>
        private Point _LastMouse = new Point();

        /// <summary>
        /// Cache used to store tile images in memory.
        /// </summary>
        private ConcurrentBag<Tile> _Cache = new ConcurrentBag<Tile>();

        /// <summary>
        /// Pool of tiles to be requested from the server.
        /// </summary>
        private ConcurrentBag<Tile> _RequestPool = new ConcurrentBag<Tile>();

        /// <summary>
        /// Worker threads for processing tile requests to the server.
        /// </summary>
        private Thread[] _Workers = new Thread[10];

        /// <summary>
        /// Event handle to stop/resume requests processing.
        /// </summary>
        private EventWaitHandle _WorkerWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        /// <summary>
        /// String format to draw text aligned to center.
        /// </summary>
        private readonly StringFormat _AlignCenterStringFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        /// <summary>
        /// Link label displayed in the bottom right corner of the map with attribution text.
        /// </summary>
        private HtmlLinkLabel _LinkLabel;

        /// <summary>
        /// Gets size of map in tiles.
        /// </summary>
        private int FullMapSizeInTiles => 1 << ZoomLevel;

        /// <summary>
        /// Gets maps size in pixels.
        /// </summary>
        private int FullMapSizeInPixels => FullMapSizeInTiles * TILE_SIZE;

        /// <summary>
        /// Backing field for <see cref="ZoomLevel"/> property.
        /// </summary>
        private int _ZoomLevel = 0;

        /// <summary>
        /// Map zoom level.
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
                CenterChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Backing field for <see cref="CacheFolder"/> property.
        /// </summary>
        private string _CacheFolder = null;

        /// <summary>
        /// Path to tile cache folder. Should be set if tile server supports file system caching.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
            }
        }

        /// <summary>
        /// Backing field for <see cref="MinZoomLevel"/> property.
        /// </summary>
        private int _MinZoomLevel = 0;

        /// <summary>
        /// Gets or sets minimal zoom level.
        /// </summary>
        [Description("Minimal zoom level"), Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        /// Backing field for <see cref="MaxZoomLevel"/> property.
        /// </summary>
        private int _MaxZoomLevel = 19;

        /// <summary>
        /// Gets or sets maximal zoom level.
        /// </summary>
        [Description("Maximal zoom level"), Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        /// Gets collection of Layers of the map control.
        /// Each layer can have own tile server, opacity, and Z-index.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ILayersCollection Layers { get; } = new LayersCollection();

        private void UpdateOffsets()
        {
            var c = Center;

            foreach (var layer in Layers)
            {
                var center = layer.TileServer.Projection.WorldToTilePos(c, ZoomLevel);
                Debug.WriteLine($"Center = {c}");

                layer.Offset.X = -(int)(center.X * TILE_SIZE) + Width / 2;
                layer.Offset.Y = -(int)(center.Y * TILE_SIZE) + Height / 2;
                layer.Offset.X = (int)(layer.Offset.X % FullMapSizeInPixels);
            }
        }

        private void UpdateLayers()
        {
            Interlocked.Exchange(ref _Cache, new ConcurrentBag<Tile>());

            // TODO: change attrubution text
            //if (_TileServer.AttributionText != null)
            //{
            //    _LinkLabel.Text = _TileServer.AttributionText;
            //    _LinkLabel.Visible = true;
            //    OnSizeChanged(new EventArgs());
            //}

            int min = Layers.Max(lay => lay.TileServer.MinZoomLevel);
            int max = Layers.Min(lay => lay.TileServer.MaxZoomLevel);

            _MaxZoomLevel = max;
            _MinZoomLevel = min;

            if (_ZoomLevel > max)
                _ZoomLevel = max;

            if (_ZoomLevel < min)
                _ZoomLevel = min;

            // TODO: update layer's offets

            UpdateOffsets();

            Invalidate();
            
            // TODO: notify layers changed
            //TileServerChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets or sets geographical coordinates of the map center.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GeoPoint Center
        {
            get
            {
                if (Layers.Any())
                {
                    Layer layer = Layers.ElementAt(0);
                    float x = NormalizeTileNumber(-(float)(layer.Offset.X - Width / 2) / TILE_SIZE);
                    float y = -(float)(layer.Offset.Y - Height / 2) / TILE_SIZE;
                    return layer.TileServer.Projection.TileToWorldPos(x, y, ZoomLevel);
                }
                else
                {
                    return GeoPoint.Empty;
                }
            }
            set
            {
                if (Layers.Any())
                {
                    foreach (var layer in Layers)
                    {
                        var center = layer.TileServer.Projection.WorldToTilePos(value, ZoomLevel);
                        layer.Offset.X = -(int)(center.X * TILE_SIZE) + Width / 2;
                        layer.Offset.Y = -(int)(center.Y * TILE_SIZE) + Height / 2;
                        layer.Offset.X = (int)(layer.Offset.X % FullMapSizeInPixels);
                    }
                    Invalidate();
                    CenterChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets geographical coordinates of the current position of mouse.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GeoPoint Mouse
        {
            get
            {
                float x = NormalizeTileNumber(-(float)(Layers.ElementAt(0).Offset.X - _LastMouse.X) / TILE_SIZE);
                float y = -(float)(Layers.ElementAt(0).Offset.Y - _LastMouse.Y) / TILE_SIZE;
                return Layers.ElementAt(0).TileServer.Projection.TileToWorldPos(x, y, ZoomLevel);
            }
        }

        /// <summary>
        /// Gets geographical coordinates of the top left point of the map
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GeoPoint TopLeft
        {
            get
            {
                float x = NormalizeTileNumber(-(float)(Layers.ElementAt(0).Offset.X) / TILE_SIZE);
                float y = -(float)(Layers.ElementAt(0).Offset.Y) / TILE_SIZE;
                return Layers.ElementAt(0).TileServer.Projection.TileToWorldPos(x, y, ZoomLevel);
            }
        }

        /// <summary>
        /// Gets geographical coordinates of the top right point of the map
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GeoPoint TopRight
        {
            get
            {
                float x = NormalizeTileNumber(-(float)(Layers.ElementAt(0).Offset.X - Width) / TILE_SIZE);
                float y = -(float)(Layers.ElementAt(0).Offset.Y) / TILE_SIZE;
                return Layers.ElementAt(0).TileServer.Projection.TileToWorldPos(x, y, ZoomLevel);
            }
        }

        /// <summary>
        /// Gets geographical coordinates of the bottom left point of the map
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GeoPoint BottomLeft
        {
            get
            {
                float x = NormalizeTileNumber(-(float)(Layers.ElementAt(0).Offset.X) / TILE_SIZE);
                float y = -(float)(Layers.ElementAt(0).Offset.Y - Height) / TILE_SIZE;
                return Layers.ElementAt(0).TileServer.Projection.TileToWorldPos(x, y, ZoomLevel);
            }
        }

        /// <summary>
        /// Gets geographical coordinates of the bottom right point of the map
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GeoPoint BottomRight
        {
            get
            {
                float x = NormalizeTileNumber(-(float)(Layers.ElementAt(0).Offset.X - Width) / TILE_SIZE);
                float y = -(float)(Layers.ElementAt(0).Offset.Y - Height) / TILE_SIZE;
                return Layers.ElementAt(0).TileServer.Projection.TileToWorldPos(x, y, ZoomLevel);
            }
        }

        /// <summary>
        /// Gets collection of markers to be displayed on the map.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICollection<Marker> Markers { get; } = new List<Marker>();

        /// <summary>
        /// Gets collection of tracks to be displayed on the map.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICollection<Track> Tracks { get; } = new List<Track>();

        /// <summary>
        /// Gets collection of polygons to be displayed on the map.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICollection<Polygon> Polygons { get; } = new List<Polygon>();

        /// <summary>
        /// Backing field for <see cref="FitToBounds"/> property.
        /// </summary>
        private bool _FitToBounds = true;

        /// <summary>
        /// Gets or sets value indicating should the map fit to vertical bounds of the control or not.
        /// </summary>
        [Description("Value indicating should the map fit to vertical bounds of the control or not."), Category("Behavior")]
        public bool FitToBounds
        {
            get => _FitToBounds;
            set
            {
                _FitToBounds = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets color used to draw error messages.
        /// </summary>
        [Description("Color used to draw error messages."), Category("Appearance")]
        public Color ErrorColor { get; set; } = Color.Red;

        /// <summary>
        /// Gets or sets text to be displayed instead of tile when it is being downloaded.
        /// </summary>
        [Description("Text to be displayed instead of tile when it is being downloaded."), Category("Appearance")]
        public string ThumbnailText { get; set; } = "Downloading...";

        /// <summary>
        /// Gets or sets color of the text to be displayed instead of tile when it is being downloaded.
        /// </summary>
        [Description("Color of the tile thumbnail text."), Category("Appearance")]
        public Color ThumbnailForeColor { get; set; } = Color.FromArgb(0xB0, 0xB0, 0xB0);

        /// <summary>
        /// Gets or sets backgound of the thumbnail to be displayed when a tile is being downloaded.
        /// </summary>
        [Description("Color of the tile thumbnail background."), Category("Appearance")]
        public Color ThumbnailBackColor { get; set; } = Color.FromArgb(0xE0, 0xE0, 0xE0);

        /// <summary>
        /// Gets or sets flag indicating show thumbnails while downloading tile images or not.
        /// </summary>
        [Description("Show thumbnails while downloading tile images."), Category("Behavior")]
        public bool ShowThumbnails { get; set; } = true;

        /// <summary>
        /// Gets or sets the foreground color of the map.
        /// </summary>
        [Description("Gets or sets the foreground color of the map."), Category("Appearance")]
        public override Color ForeColor
        {
            get => base.ForeColor;
            set
            {
                base.ForeColor = value;
                _LinkLabel.ForeColor = value;
            }
        }

        /// <summary>
        /// Image attributes to be applied to a tile image when drawing.
        /// </summary>
        [Description("Image attributes to be applied to a tile image when drawing."), Category("Appearance")]
        public ImageAttributes TileImageAttributes { get; set; } = null;

        /// <summary>
        /// Raised when marker is drawn on the map.
        /// </summary>
        public event EventHandler<DrawMarkerEventArgs> DrawMarker;

        /// <summary>
        /// Raised when track is drawn on the map.
        /// </summary>
        public event EventHandler<DrawTrackEventArgs> DrawTrack;

        /// <summary>
        /// Raised when polygon is drawn on the map.
        /// </summary>
        public event EventHandler<DrawPolygonEventArgs> DrawPolygon;

        /// <summary>
        /// Raised when <see cref="Center"/> property value is changed.
        /// </summary>
        public event EventHandler<EventArgs> CenterChanged;

        /// <summary>
        /// Raised when <see cref="Mouse"/> property value is changed.
        /// </summary>
        public event EventHandler<EventArgs> MouseChanged;

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

            // Intialize worker, if not yet initialized
            for (int w = 0; w < _Workers.Length; w++)
            {
                _Workers[w] = new Thread(new ThreadStart(ProcessRequests));
                _Workers[w].Name = $"Request worker #{w + 1}";
                _Workers[w].IsBackground = true;
                _Workers[w].Priority = ThreadPriority.Highest;
                _Workers[w].Start();
            }

            Layers.LayersCollectionBeforeChange += OnBeforeChangeLayersCollection;
            Layers.LayersCollectionAfterChange += OnAfterChangeLayersCollection;
        }

        protected GeoPoint _CenterKeeper;

        protected void OnBeforeChangeLayersCollection(object sender, EventArgs eventArgs)
        {
            _CenterKeeper = Center;
        }

        private void OnAfterChangeLayersCollection(object sender, EventArgs eventArgs)
        {
            Center = _CenterKeeper;
        }

        /// <summary>
        /// Handles clicks on LinkLabel links
        /// </summary>
        private void _LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString());
        }

        /// <summary>
        /// Does painting of the map.
        /// </summary>
        /// <param name="pe">Paint event args.</param>
        protected override void OnPaint(PaintEventArgs pe)
        {
            bool drawContent = true;

            if (!DesignMode)
            {
                if (CacheFolder == null && Layers.Any(lay => lay.TileServer is IFileCacheTileServer))
                {
                    drawContent = false;
                    DrawErrorString(pe.Graphics, $"{nameof(CacheFolder)} property value is not set.\nIt should be specified if you are using tile server which supports file system cache.");
                }
                else if (!Layers.Any())
                {
                    drawContent = false;
                    DrawErrorString(pe.Graphics, $"{nameof(Layers)} collection value is empty.\nPlease add at least one layer to the map control and set {nameof(Layer.TileServer)} property of the layer to obtain map images before using the map control.");
                }
            }
            else
            {
                drawContent = false;
                pe.Graphics.DrawString("Map is unavailable in design mode.", Font, new SolidBrush(ForeColor), new Point(Width / 2, Height / 2), _AlignCenterStringFormat);
            }

            if (drawContent)
            {
                pe.Graphics.SmoothingMode = SmoothingMode.None;
                DrawTiles(pe.Graphics);
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                DrawPolygons(pe.Graphics);
                DrawTracks(pe.Graphics);
                DrawMarkers(pe.Graphics);
            }
        }

        /// <summary>
        /// Called when control size is changed.
        /// </summary>
        /// <param name="e">Event args.</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            _LinkLabel.Left = Width - _LinkLabel.Width;
            _LinkLabel.Top = Height - _LinkLabel.Height;
           
            AdjustMapBounds();
            Invalidate();
            CenterChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when mouse is down.
        /// </summary>
        /// <param name="e">Event args.</param>
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

        /// <summary>
        /// Called when mouse is up.
        /// </summary>
        /// <param name="e">Event args.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            _MouseCaptured = false;
            base.OnMouseUp(e);
        }

        /// <summary>
        /// Called when mouse is moving.
        /// </summary>
        /// <param name="e">Event args.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_MouseCaptured)
            {
                foreach (var layer in Layers)
                {
                    layer.Offset.X += (e.X - _LastMouse.X);
                    layer.Offset.Y += (e.Y - _LastMouse.Y);

                    layer.Offset.X = (int)(layer.Offset.X % FullMapSizeInPixels);

                    if (layer.Offset.Y < -(int)FullMapSizeInPixels)
                        layer.Offset.Y = -(int)FullMapSizeInPixels;

                    if (layer.Offset.Y > Height)
                        layer.Offset.Y = Height;
                }

                AdjustMapBounds();
                Invalidate();

                CenterChanged?.Invoke(this, EventArgs.Empty);
            }

            _LastMouse.X = e.X;
            _LastMouse.Y = e.Y;

            MouseChanged?.Invoke(this, EventArgs.Empty);

            base.OnMouseMove(e);
        }

        /// <summary>
        /// Called when mouse is wheeling.
        /// </summary>
        /// <param name="e">Event args.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int z = ZoomLevel;

            if (e.Delta >= 120)
                z = ZoomLevel + 1;
            else if (e.Delta <= -120)
                z = ZoomLevel - 1;

            SetZoomLevel(z, new Point(e.X, e.Y));

            AdjustMapBounds();
            base.OnMouseWheel(e);
            CenterChanged?.Invoke(this, EventArgs.Empty);          
        }

        /// <summary>
        /// Adjusts map bounds if required.
        /// </summary>
        private void AdjustMapBounds()
        {
            if (FitToBounds)
            {
                foreach (var layer in Layers)
                {
                    if (FullMapSizeInPixels > Height)
                    {
                        if (layer.Offset.Y > 0) layer.Offset.Y = 0;
                        if (layer.Offset.Y + FullMapSizeInPixels < Height) layer.Offset.Y = Height - FullMapSizeInPixels;
                    }
                    else
                    {
                        if (layer.Offset.Y > Height - FullMapSizeInPixels) layer.Offset.Y = Height - FullMapSizeInPixels;
                        if (layer.Offset.Y < 0) layer.Offset.Y = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Normalizes tile number to fit value from 0 to FullMapSizeInTiles.
        /// </summary>
        /// <param name="n">Tile number, with fractions.</param>
        /// <returns>Tile number in range from 0 to FullMapSizeInTiles.</returns>
        private float NormalizeTileNumber(float n)
        {
            int size = FullMapSizeInTiles;
            return (n %= size) >= 0 ? n : (n + size);
        }

        /// <summary>
        /// Draws error string on the map.
        /// </summary>
        /// <param name="gr">Graphics instance to draw on.</param>
        /// <param name="text">Error string to be drawn.</param>
        private void DrawErrorString(Graphics gr, string text)
        {
            gr.DrawString(text, Font, new SolidBrush(ErrorColor), new Point(Width / 2, Height / 2), _AlignCenterStringFormat);
        }

        /// <summary>
        /// Draws map tiles.
        /// </summary>
        /// <param name="g">Graphics instance to draw on.</param>
        private void DrawTiles(Graphics g)
        {
            // count of visible tiles (vertically and horizontally)
            int tilesByWidth = (int)Math.Ceiling((float)Width / TILE_SIZE);
            int tilesByHeight = (int)Math.Ceiling((float)Height / TILE_SIZE);

            // flush used flag for all memory-cached tiles
            foreach (var c in _Cache)
            {
                c.Used = false;
            }

            foreach (var layer in Layers.OrderBy(lay => lay.ZIndex))
            {
                // indices of first visible tile
                int fromX = (int)Math.Floor(-(float)layer.Offset.X / TILE_SIZE);
                int fromY = (int)Math.Floor(-(float)layer.Offset.Y / TILE_SIZE);

                // indices of last visible tile
                int toX = fromX + tilesByWidth;
                int toY = fromY + tilesByHeight;

                for (int x = fromX; x <= toX; x++)
                {
                    for (int y = fromY; y <= toY; y++)
                    {
                        int x_ = (int)NormalizeTileNumber(x);
                        if (y >= 0 && y < FullMapSizeInTiles)
                        {
                            Tile tile = GetTile(layer, x_, y, ZoomLevel);

                            // tile for current zoom and position found
                            if (tile != null)
                            {
                                if (tile.Image != null)
                                {
                                    tile.Used = true;
                                    DrawTile(g, layer, x, y, tile.Image);
                                }
                                else
                                {
                                    tile.Used = true;
                                    DrawThumbnail(g, layer, x, y, tile.ErrorMessage, true);
                                }
                            }
                            // tile not found, do some magic
                            else
                            {
                                // draw thumbnail first
                                DrawThumbnail(g, layer, x, y, ThumbnailText, false);

                                // try to find out tile with less zoom level, and draw scaled part of that tile

                                int z = 1;
                                while (ZoomLevel - z >= 0)
                                {
                                    // fraction of a tile to be drawn (1/2, 1/4 and etc.)
                                    int f = 1 << z;

                                    //  try to get tile with less zoom level from cache
                                    tile = GetTile(layer, x_ / f, y / f, ZoomLevel - z, fromCacheOnly: true);

                                    // if tile found, draw part of it
                                    if (tile != null && tile.Image != null)
                                    {
                                        tile.Used = true;
                                        DrawTilePart(g, layer, x, y, x_ % f, y % f, f, tile.Image);
                                        break;
                                    }

                                    // move up to less zoom level
                                    z++;
                                }
                            }
                        }
                    }
                }
            }

            // Dispose images that were not used 
            _Cache.Where(c => !c.Used).ToList().ForEach(c => c.Image?.Dispose());

            // Update cache, leave only used images
            //_Cache = new ConcurrentBag<Tile>(_Cache.Where(c => c.Used));

            Interlocked.Exchange(ref _Cache, new ConcurrentBag<Tile>(_Cache.Where(c => c.Used)));

        }

        /// <summary>
        /// Draws markers on the map
        /// </summary>
        /// <param name="gr">Graphics instance to draw on.</param>
        private void DrawMarkers(Graphics gr)
        {
            foreach (Marker m in Markers)
            {
                var p = Project(m.Point);
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
                            if (m.Style.MarkerBrush != null)
                            {
                                gr.FillEllipse(m.Style.MarkerBrush, p.X - m.Style.MarkerWidth / 2, p.Y - m.Style.MarkerWidth / 2, m.Style.MarkerWidth, m.Style.MarkerWidth);
                            }
                            if (m.Style.MarkerPen != null)
                            {
                                gr.DrawEllipse(m.Style.MarkerPen, p.X - m.Style.MarkerWidth / 2, p.Y - m.Style.MarkerWidth / 2, m.Style.MarkerWidth, m.Style.MarkerWidth);
                            }
                            if (m.Style.LabelFont != null && m.Style.LabelBrush != null && m.Style.LabelFormat != null)
                            {
                                gr.DrawString(m.Label, m.Style.LabelFont, m.Style.LabelBrush, new PointF(p.X + m.Style.MarkerWidth * 0.35f, p.Y + m.Style.MarkerWidth * 0.35f), m.Style.LabelFormat);
                            }
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Draws tracks on the map
        /// </summary>
        /// <param name="gr">Graphics instance to draw on.</param>
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

                var eventArgs = new DrawTrackEventArgs()
                {
                    Graphics = gr,
                    Track = track,
                    Points = points
                };

                DrawTrack?.Invoke(this, eventArgs);
                if (!eventArgs.Handled)
                {
                    if (ZoomLevel < 3)
                    {
                        if (track.Style.Pen != null)
                        {
                            Draw(gr, () => gr.DrawLines(track.Style.Pen, points));
                        }
                    }
                    else
                    {
                        Draw(gr, () => gr.DrawPolyline(track.Style.Pen, points));
                    }
                }
            }
        }

        /// <summary>
        /// Draws polygons on the map
        /// </summary>
        /// <param name="gr">Graphics instance to draw on.</param>
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

                    gp.CloseFigure();

                    var eventArgs = new DrawPolygonEventArgs()
                    {
                        Graphics = gr,
                        Polygon = polygon,
                        Path = gp
                    };

                    DrawPolygon?.Invoke(this, eventArgs);
                    if (!eventArgs.Handled)
                    {
                        if (ZoomLevel < 3)
                        {
                            Draw(gr, () =>
                            {
                                if (polygon.Style.Brush != null)
                                {
                                    gr.FillPath(polygon.Style.Brush, gp);
                                }
                                if (polygon.Style.Pen != null)
                                {
                                    gr.DrawPath(polygon.Style.Pen, gp);
                                }
                            });
                        }
                        else
                        {
                            Draw(gr, () => gr.DrawGraphicsPath(gp, polygon.Style.Brush, polygon.Style.Pen));
                        }
                    }                    
                }
            }
        }

        private ImageAttributes GetImageAttributes(float opacity)
        {
            var attrs = (ImageAttributes)TileImageAttributes?.Clone() ?? new ImageAttributes();

            // create a color matrix object  
            ColorMatrix matrix = new ColorMatrix();

            // set the opacity  
            matrix.Matrix33 = opacity;

            // set the color( opacity) of the image  
            attrs.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            return attrs;
        }

        /// <summary>
        /// Draws part of a tile.
        /// This method is needed to draw portion of a tile with highest zoom level if a tile with smallest zoom is not ready yet.
        /// </summary>
        /// <param name="gr">Graphics instance to draw on.</param>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index of the tile.</param>
        /// <param name="xRemainder">X-index of a tile portion to be drawn.</param>
        /// <param name="yRemainder">Y-index of a tile portion to be drawn.</param>
        /// <param name="frac">Portion of a tile to be drawn, 2 means 1/2, 4 means 1/4 etc.</param>
        /// <param name="image">Full tile image.</param>
        private void DrawTilePart(Graphics gr, Layer layer, int x, int y, int xRemainder, int yRemainder, int frac, Image image)
        {
            // coordinates of a tile on the map
            Point p = new Point();
            p.X = layer.Offset.X + x * TILE_SIZE;
            p.Y = layer.Offset.Y + y * TILE_SIZE;

            // Calc source portion of the tile
            Rectangle srcRect = new Rectangle(TILE_SIZE / frac * xRemainder, TILE_SIZE / frac * yRemainder, TILE_SIZE / frac, TILE_SIZE / frac);

            // Destination rectangle
            Rectangle destRect = new Rectangle(p.X - frac, p.Y - frac, TILE_SIZE + 2 * frac, TILE_SIZE + 2 * frac);

            var state = gr.Save();            
            gr.SmoothingMode = SmoothingMode.HighSpeed;
            gr.InterpolationMode = InterpolationMode.NearestNeighbor;
            gr.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            gr.CompositingQuality = CompositingQuality.HighSpeed;
            gr.DrawImage(image, destRect, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, GraphicsUnit.Pixel, GetImageAttributes(layer.Opacity));
            gr.Restore(state);
        }

        /// <summary>
        /// Draws a tile on the map.
        /// </summary>
        /// <param name="gr">Graphics instance to draw on.</param>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index of the tile.</param>
        /// <param name="image">Tile image.</param>
        private void DrawTile(Graphics gr, Layer layer, int x, int y, Image image)
        {
            Point p = new Point();
            p.X = layer.Offset.X + x * TILE_SIZE;
            p.Y = layer.Offset.Y + y * TILE_SIZE;

            gr.DrawImage(image, new Rectangle(p, new Drawing.Size(TILE_SIZE, TILE_SIZE)), 0, 0, TILE_SIZE, TILE_SIZE, GraphicsUnit.Pixel, GetImageAttributes(layer.Opacity));
        }

        /// <summary>
        /// Draws thumbnail frame and text instead of a tile.
        /// </summary>
        /// <param name="gr">Graphics instance to draw on.</param>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index of the tile.</param>
        /// <param name="message">Message to be displayed instead of the tile.</param>
        /// <param name="isError">Flag indicating the message should be displayed with error color.</param>
        private void DrawThumbnail(Graphics gr, Layer layer, int x, int y, string message, bool isError)
        {
            if (ShowThumbnails || isError)
            {
                Point p = new Point();
                p.X = layer.Offset.X + x * TILE_SIZE;
                p.Y = layer.Offset.Y + y * TILE_SIZE;
                Rectangle rectangle = new Rectangle(p.X, p.Y, TILE_SIZE, TILE_SIZE);
                gr.FillRectangle(new SolidBrush(ThumbnailBackColor), rectangle);
                gr.DrawRectangle(new Pen(ThumbnailForeColor) { DashStyle = DashStyle.Dot }, rectangle);
                gr.DrawString(message, Font, new SolidBrush(isError ? ErrorColor : ThumbnailForeColor), rectangle, _AlignCenterStringFormat);
            }
        }

        /// <summary>
        /// Does the draw action.
        /// The method is needed for repeating drawing because map is infinite in longitude.
        /// </summary>
        /// <param name="gr">Graphics instance to draw on.</param>
        /// <param name="draw">Draw action to perform several times for all visible width of the map.</param>
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

        /// <summary>
        /// Sets zoom level with specifying central point to zoom in/out.
        /// </summary>
        /// <param name="z">Zoom level to be set.</param>
        /// <param name="p">Central point to zoom in/out.</param>
        private void SetZoomLevel(int z, Point p)
        {
            int max = Layers.Any() ? Math.Min(MaxZoomLevel, Layers.Min(lay => lay.TileServer.MaxZoomLevel)) : MaxZoomLevel;
            int min = Layers.Any() ? Math.Max(MinZoomLevel, Layers.Max(lay => lay.TileServer.MinZoomLevel)) : MinZoomLevel;

            if (z < min) z = min;
            if (z > max) z = max;

            if (z != _ZoomLevel)
            {
                double factor = Math.Pow(2, z - _ZoomLevel);
                _ZoomLevel = z;

                foreach (var layer in Layers)
                {
                    layer.Offset.X = (int)((layer.Offset.X - p.X) * factor) + p.X;
                    layer.Offset.Y = (int)((layer.Offset.Y - p.Y) * factor) + p.Y;
                    layer.Offset.X = (int)(layer.Offset.X % FullMapSizeInPixels);
                }

                UpdateOffsets();

                Invalidate();

                //ZoomLevelChaged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets tile image by X and Y indices and zoom level.
        /// </summary>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index of the tile.</param>
        /// <param name="z">Zoom level.</param>
        /// <param name="fromCacheOnly">Flag indicating the tile can be fetched from cache only (server request is not allowed).</param>
        /// <returns><see cref="Tile"/> instance.</returns>
        private Tile GetTile(Layer layer, int x, int y, int z, bool fromCacheOnly = false)
        {
            try
            {
                Tile tile;

                // try to get tile from memory cache
                tile = _Cache.FirstOrDefault(t => t.TileServer == layer.TileServer.GetType().Name && t.Z == z && t.X == x && t.Y == y);
                if (tile != null)
                {
                    return tile;
                }

                // try to get tile from file system
                if (layer.TileServer is IFileCacheTileServer fcTileServer)
                {
                    string localPath = Path.Combine(CacheFolder, layer.TileServer.GetType().Name, $"{z}", $"{x}", $"{y}.tile");
                    if (File.Exists(localPath))
                    {
                        var fileInfo = new FileInfo(localPath);
                        if (fileInfo.Length > 0 && fileInfo.LastWriteTime + fcTileServer.TileExpirationPeriod >= DateTime.Now)
                        {
                            Image image = Image.FromFile(localPath);
                            tile = new Tile(image, x, y, z, layer.TileServer.GetType().Name);
                            _Cache.Add(tile);
                            return tile;
                        }
                    }
                }
                
                // request tile from the server 
                if (!fromCacheOnly)
                {
                    RequestTile(layer, x, y, z);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Does a tile request to the tile server
        /// </summary>
        /// <param name="x">X-index of the tile to be requested.</param>
        /// <param name="y">Y-index of the tile to be requested.</param>
        /// <param name="z">Zoom level</param>
        private void RequestTile(Layer layer, int x, int y, int z)
        {
            // Check the tile is already requested
            string tileServer = layer.TileServer.GetType().Name;
            if (!_RequestPool.Any(t => t.TileServer == tileServer && t.Z == z && t.X == x && t.Y == y))
            {
                _RequestPool.Add(new Tile(x, y, z, tileServer));
                _WorkerWaitHandle.Set();
            }
        }

        /// <summary>
        /// Background worker function. 
        /// Processes tiles requests if requests pool is not empty, 
        /// than stops execution until the pool gets a new image request.
        /// Breaks execution on disposing.
        /// </summary>
        private void ProcessRequests()
        {
            while (!IsDisposed)
            {
                // try to process all tile requests till pool is not empty
                while (_RequestPool.TryTake(out Tile tile))
                {
                    Console.WriteLine($"{Thread.CurrentThread.Name} processing...");

                    var layer = Layers.FirstOrDefault(lay => lay.TileServer.GetType().Name == tile.TileServer);

                    try
                    {
                        // ignore pooled items with different zoom level and another tile server
                        if (layer != null && tile.Z == ZoomLevel)
                        {
                            tile.Image = layer.TileServer.GetTile(tile.X, tile.Y, tile.Z);
                            tile.Used = true;


                        }
                    }
                    catch (Exception ex)
                    {
                        // keep error text to be displayed instead of the tile
                        tile.ErrorMessage = ex.Message;
                    }
                    finally
                    {
                        // if we have obtained image from the server, save it in file system (if server supports file system cache)
                        if (layer != null && layer.TileServer is IFileCacheTileServer && tile.Image != null)
                        {
                            string localPath = Path.Combine(CacheFolder, tile.TileServer, $"{tile.Z}", $"{tile.X}", $"{tile.Y}.tile");
                            try
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(localPath));


                                tile.Image.Save(localPath);
                                Debug.WriteLine($"saved {localPath}");
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Unable to save tile image {localPath}. Reason: {ex.Message}");
                            }
                        }

                        // add tile to the memory cache
                        if (tile.Image != null || tile.ErrorMessage != null)
                        {
                            _Cache.Add(tile);
                        }

                        Invalidate();
                        //_RepaingWaitHandle.Set();
                    }
                }

                _WorkerWaitHandle.WaitOne();
            };
        }

        /// <summary>
        /// Gets projection of geographical coordinates onto the map.
        /// </summary>
        /// <param name="g">Point with geographical coordinates.</param>
        /// <returns><see cref="PointF"/> object representing projection of the specified geographical coordinates on the map.</returns>
        private PointF Project(GeoPoint g)
        {
            var p = Layers.ElementAt(0).TileServer.Projection.WorldToTilePos(g, ZoomLevel);
            return new PointF(p.X * TILE_SIZE + Layers.ElementAt(0).Offset.X, p.Y * TILE_SIZE + Layers.ElementAt(0).Offset.Y);
        }

        /// <summary>
        /// Clears map cache. If <paramref name="allTileServers"/> flag is set to true, cache for all tile servers will be cleared.
        /// If no, only current tile server cache will be cleared.
        /// </summary>
        /// <param name="allTileServers">If flag is set to true, cache for all tile servers will be cleared.</param>
        public void ClearCache(bool allTileServers = false)
        {
            Interlocked.Exchange(ref _Cache, new ConcurrentBag<Tile>());
            Interlocked.Exchange(ref _RequestPool, new ConcurrentBag<Tile>());

            foreach (var layer in Layers)
            {                
                string cacheFolder = allTileServers ? CacheFolder : Path.Combine(CacheFolder, layer.TileServer.GetType().Name);
                if (Directory.Exists(cacheFolder))
                {
                    if (allTileServers)
                    {
                        var subdirs = Directory.EnumerateDirectories(cacheFolder);
                        foreach (string dir in subdirs)
                        {
                            try
                            {
                                Directory.Delete(dir, true);
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        try
                        {
                            Directory.Delete(cacheFolder, true);
                        }
                        catch { }
                    }
                }
            }
            Invalidate();
        }

        /// <summary>
        /// Removes all markers, tracks and polygons from the map.
        /// </summary>
        public void ClearAll()
        {
            Markers.Clear();
            Tracks.Clear();
            Polygons.Clear();
            Invalidate();
        }
    }
}

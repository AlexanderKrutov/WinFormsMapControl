using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms.Maps.Common;
using System.Windows.Forms.Maps.Elements;
using System.Windows.Forms.Maps.Layers;

namespace System.Windows.Forms
{
    /// <summary>
    /// Map control for displaying online and offline maps.
    /// </summary>
    [DesignerCategory("code")]
    public partial class MapControl : Control
    {
        #region Private Properties

        /// <summary>
        /// Tile size, in pixels.
        /// </summary>
        private const int TILE_SIZE = 256;

        /// <summary>
        /// Layer for markers added directly to the map.
        /// </summary>
        private MarkerLayer _BaseMarkerLayer = new MarkerLayer(Int32.MaxValue) { Name = "BaseMarkerLayer" };

        /// <summary>
        /// Layer for tracks added directly to the map.
        /// </summary>
        private TrackLayer _BaseTrackLayer = new TrackLayer(Int32.MaxValue) { Name = "BaseTrackLayer" };

        /// <summary>
        /// Layer for polgons added directly to the map.
        /// </summary>
        private PolygonLayer _BasePolygonLayer = new PolygonLayer(Int32.MaxValue) { Name = "BasePolygonLayer" };

        /// <summary>
        /// Layer for ellipses added directly to the map.
        /// </summary>
        private EllipseLayer _BaseEllipseLayer = new EllipseLayer(Int32.MaxValue) { Name = "BaseEllipseLayer" };

        /// <summary>
        /// First tile offset.
        /// </summary>
        private Point _Offset = new Point();

        /// <summary>
        /// Flag indicating thar mouse is captured.
        /// </summary>
        private bool _MouseCaptured = false;

        /// <summary>
        /// Last element used to send an enter, leave or click event.
        /// </summary>
        private IElement _LastElement = null;

        /// <summary>
        /// Last layer in which an element for enter, leave or click event has been found.
        /// </summary>
        private Layer _LastLayer = null;

        /// <summary>
        /// Last known mouse position.
        /// </summary>
        private Point _LastMouse = new Point();

        /// <summary>
        /// Last known center point before resizing.
        /// </summary>
        private GeoPoint _LastCenter = new GeoPoint();

        /// <summary>
        /// Cache used to store tile images in memory.
        /// </summary>
        private ConcurrentBag<Tile> _Cache = new ConcurrentBag<Tile>();

        /// <summary>
        /// Pool of tiles to be requested from the server.
        /// </summary>
        private ConcurrentBag<Tile> _RequestPool = new ConcurrentBag<Tile>();

        /// <summary>
        /// Worker thread to process tile requests to the server.
        /// </summary>
        private Thread _Worker = null;

        /// <summary>
        /// Event handle to stop/resume requests processing.
        /// </summary>
        private EventWaitHandle _WorkerWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

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

        #endregion

        #region Public Properties

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
        /// Backing field for <see cref="TileServer"/> property.
        /// </summary>
        private ITileServer _TileServer;

        /// <summary>
        /// Gets or sets tile server instance used to obtain map tiles.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ITileServer TileServer
        {
            get => _TileServer;
            set
            {
                _TileServer = value;
                _LinkLabel.Links.Clear();
                _LinkLabel.Visible = false;

                if (value != null)
                {
                    _Cache = new ConcurrentBag<Tile>();

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
                TileServerChanged?.Invoke(this, EventArgs.Empty);
            }
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
                float x = NormalizeTileNumber(-(float)(_Offset.X - Width / 2) / TILE_SIZE);
                float y = -(float)(_Offset.Y - Height / 2) / TILE_SIZE;
                return TileToWorldPos(x, y);
            }
            set
            {
                _LastCenter = value;

                var center = WorldToTilePos(value);
                _Offset.X = -(int)(center.X * TILE_SIZE) + Width / 2;
                _Offset.Y = -(int)(center.Y * TILE_SIZE) + Height / 2;
                _Offset.X = (int)(_Offset.X % FullMapSizeInPixels);
                Invalidate();
                CenterChanged?.Invoke(this, EventArgs.Empty);
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
                float x = NormalizeTileNumber(-(float)(_Offset.X - _LastMouse.X) / TILE_SIZE);
                float y = -(float)(_Offset.Y - _LastMouse.Y) / TILE_SIZE;
                return TileToWorldPos(x, y);
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
                float x = NormalizeTileNumber(-(float)(_Offset.X) / TILE_SIZE);
                float y = -(float)(_Offset.Y) / TILE_SIZE;
                return TileToWorldPos(x, y);
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
                float x = NormalizeTileNumber(-(float)(_Offset.X - Width) / TILE_SIZE);
                float y = -(float)(_Offset.Y) / TILE_SIZE;
                return TileToWorldPos(x, y);
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
                float x = NormalizeTileNumber(-(float)(_Offset.X) / TILE_SIZE);
                float y = -(float)(_Offset.Y - Height) / TILE_SIZE;
                return TileToWorldPos(x, y);
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
                float x = NormalizeTileNumber(-(float)(_Offset.X - Width) / TILE_SIZE);
                float y = -(float)(_Offset.Y - Height) / TILE_SIZE;
                return TileToWorldPos(x, y);
            }
        }

        /// <summary>
        /// Gets collection of layers to be displayed on the map.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICollection<Layer> Layers { get; } = new List<Layer>();

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

        #endregion

        #region Public Events

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
        /// Raised when ellipse is drawn on the map.
        /// </summary>
        public event EventHandler<DrawEllipseEventArgs> DrawEllipse;

        /// <summary>
        /// Raised when <see cref="IElement"/> of an layer has been clicked.
        /// </summary>
        public event EventHandler<MapControlElementEventArgs> ElementClick;

        /// <summary>
        /// Raised when <see cref="IElement"/> of an layer is entered.
        /// </summary>
        public event EventHandler<MapControlElementEventArgs> ElementEnter;

        /// <summary>
        /// Raised when <see cref="IElement"/> of an layer is leaved.
        /// </summary>
        public event EventHandler<MapControlElementEventArgs> ElementLeave;

        /// <summary>
        /// Raised when <see cref="Center"/> property value is changed.
        /// </summary>
        public event EventHandler<EventArgs> CenterChanged;

        /// <summary>
        /// Raised when <see cref="Mouse"/> property value is changed.
        /// </summary>
        public event EventHandler<EventArgs> MouseChanged;

        /// <summary>
        /// Raised when <see cref="TileServer"/> property value is changed.
        /// </summary>
        public event EventHandler<EventArgs> TileServerChanged;

        #endregion

        #region Public Methods

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

            // add base layers
            AddLayer(_BaseMarkerLayer);
            AddLayer(_BaseTrackLayer);
            AddLayer(_BasePolygonLayer);
            AddLayer(_BaseEllipseLayer);
        }

        /// <summary>
        /// Adds a layer to the map control and invalidates map.
        /// </summary>
        /// <param name="layer">Layer to add to the map.</param>
        public void AddLayer(Layer layer)
        {
            layer.LayerPropertyChanged += LayerPropertyChanged;

            Layers.Add(layer);
            Invalidate();
        }

        /// <summary>
        /// Adds a marker to the map control and invalidates map.
        /// </summary>
        /// <param name="marker">Marker to add to the map.</param>
        public void AddMarker(Marker marker)
        {
            _BaseMarkerLayer.AddMarker(marker);
            
            // Markers.Add(marker);
            // Invalidate();
        }

        /// <summary>
        /// Adds a track to the map control and invalidates map.
        /// </summary>
        /// <param name="track">Track to add to the map.</param>
        public void AddTrack(Track track)
        {
            _BaseTrackLayer.AddTrack(track);
            
            // Tracks.Add(track);
            Invalidate();
        }

        /// <summary>
        /// Adds a polygon to the map control and invalidates map.
        /// </summary>
        /// <param name="polygon">Polygon to add to the map.</param>
        public void AddPolygon(Polygon polygon)
        {
            _BasePolygonLayer.AddPolygon(polygon);
            
            // Polygons.Add(polygon);
            Invalidate();
        }

        /// <summary>
        /// Adds an ellipse to the map control and invalidates map.
        /// </summary>
        /// <param name="ellipse">Ellipse to add to the map.</param>
        public void AddEllipse(Ellipse ellipse)
        {
            _BaseEllipseLayer.AddEllipse(ellipse);
            
            // Ellipses.Add(ellipse);
            Invalidate();
        }

        /// <summary>
        /// Removes all layers from the map.
        /// </summary>
        public void ClearLayers()
        {
            Layers.Clear();
            Invalidate();
        }

        /// <summary>
        /// Removes all markers, tracks, polygons and ellipses from the map.
        /// </summary>
        public void ClearElements()
        {
            _BaseMarkerLayer.Clear();
            _BaseTrackLayer.Clear();
            _BasePolygonLayer.Clear();
            _BaseEllipseLayer.Clear();
            
            /*Markers.Clear();
            Tracks.Clear();
            Polygons.Clear();
            Ellipses.Clear();*/
            Invalidate();
        }

        /// <summary>
        /// Clears map cache. If <paramref name="allTileServers"/> flag is set to true, cache for all tile servers will be cleared.
        /// If no, only current tile server cache will be cleared.
        /// </summary>
        /// <param name="allTileServers">If flag is set to true, cache for all tile servers will be cleared.</param>
        public void ClearCache(bool allTileServers = false)
        {
            _Cache = new ConcurrentBag<Tile>();
            _RequestPool = new ConcurrentBag<Tile>();

            if (TileServer != null)
            {
                string cacheFolder = allTileServers ? CacheFolder : Path.Combine(CacheFolder, TileServer.GetType().Name);
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
        /// Zooms map control to a certain layer.
        /// </summary>
        /// <param name="layer">Layer to zoom to.</param>
        public void ZoomTo(Layer layer)
        {
            GeoPoint[] layerBounds = MeasureLayer(layer);

            if (layerBounds != null)
            {
                // determine and set center
                double centerLongitude = layerBounds[0].Longitude + (layerBounds[3].Longitude - layerBounds[0].Longitude) / 2.0;
                double centerLatitude = layerBounds[0].Latitude + (layerBounds[3].Latitude - layerBounds[0].Latitude) / 2.0;

                Center = new GeoPoint((float)centerLongitude, (float)centerLatitude);

                // find required zoom level iteratively
                ZoomLevel = 19;
                while(ZoomLevel > 1)
                {
                    bool topLeftFits = (TopLeft.Longitude <= layerBounds[0].Longitude || (_Offset.X > 0 && TopLeft.Longitude >= layerBounds[0].Longitude)) && TopLeft.Latitude >= layerBounds[0].Latitude;
                    bool bottomRightFits = (BottomRight.Longitude >= layerBounds[3].Longitude || (_Offset.X > 0 && BottomRight.Longitude <= layerBounds[3].Longitude)) && BottomRight.Latitude <= layerBounds[3].Latitude;

                    if (topLeftFits && bottomRightFits)
                    {
                        break;
                    }
                    else
                    {
                        ZoomLevel = ZoomLevel - 1;
                    }
                }
            }
        }

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// Called on creating control.
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            Center = new GeoPoint(0, 0);
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
                if (CacheFolder == null && TileServer is IFileCacheTileServer)
                {
                    drawContent = false;
                    DrawErrorString(pe.Graphics, $"{nameof(CacheFolder)} property value is not set.\nIt should be specified if you are using tile server which supports file system cache.");
                }
                else if (TileServer == null)
                {
                    drawContent = false;
                    DrawErrorString(pe.Graphics, $"{nameof(TileServer)} property value is not set.\nPlease specify tile server instance to obtain map images before using the map control.");
                }
            }

            if (drawContent)
            {
                pe.Graphics.SmoothingMode = SmoothingMode.None;
                DrawTiles(pe.Graphics);
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                DrawLayers(pe.Graphics);
            }

            base.OnPaint(pe);
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

            Center = _LastCenter;

            AdjustMapBounds();
            Invalidate();
            //CenterChanged?.Invoke(this, EventArgs.Empty);
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
            if (_MouseCaptured)
            {
                foreach (Layer layer in Layers.OrderByDescending(l => l.Level))
                {
                    if (_LastLayer != null && _LastLayer.Equals(layer) && layer.Clickable)
                    {
                        ElementClick?.Invoke(this, new MapControlElementEventArgs()
                        {
                            X = e.X,
                            Y = e.Y,
                            Layer = _LastLayer,
                            Element = _LastElement
                        });

                        break;
                    }
                }
            }

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
                _Offset.X += (e.X - _LastMouse.X);
                _Offset.Y += (e.Y - _LastMouse.Y);

                _Offset.X = (int)(_Offset.X % FullMapSizeInPixels);

                if (_Offset.Y < -(int)FullMapSizeInPixels)
                    _Offset.Y = -(int)FullMapSizeInPixels;

                if (_Offset.Y > Height)
                    _Offset.Y = Height;

                _LastCenter = Center;

                AdjustMapBounds();
                Invalidate();
                CenterChanged?.Invoke(this, EventArgs.Empty);
            }

            _LastMouse.X = e.X;
            _LastMouse.Y = e.Y;

            foreach(Layer layer in Layers.OrderByDescending(l => l.Level))
            {
                if (layer.Visible && (layer.Hoverable || layer.Clickable))
                {
                    IElement touchedElement = FindTouchedElement(layer, e.X, e.Y);

                    if (touchedElement != null && _LastElement == null)
                    {
                        if (layer.Hoverable)
                        {
                            ElementEnter?.Invoke(this, new MapControlElementEventArgs()
                            {
                                X = e.X,
                                Y = e.Y,
                                Layer = layer,
                                Element = touchedElement
                            });
                        }

                        _LastElement = touchedElement;
                        _LastLayer = layer;

                        break;
                    }
                    else if (touchedElement == null && _LastElement != null && _LastLayer.Equals(layer))
                    {
                        if (layer.Hoverable)
                        {
                            ElementLeave?.Invoke(this, new MapControlElementEventArgs()
                            {
                                X = e.X,
                                Y = e.Y,
                                Layer = _LastLayer,
                                Element = _LastElement
                            });
                        }

                        _LastElement = null;
                        _LastLayer = null;
                    }
                }
            }

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

        #endregion

        #region Private Draw Methods

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
            // indices of first visible tile
            int fromX = (int)Math.Floor(-(float)_Offset.X / TILE_SIZE);
            int fromY = (int)Math.Floor(-(float)_Offset.Y / TILE_SIZE);

            // count of visible tiles (vertically and horizontally)
            int tilesByWidth = (int)Math.Ceiling((float)Width / TILE_SIZE);
            int tilesByHeight = (int)Math.Ceiling((float)Height / TILE_SIZE);

            // indices of last visible tile
            int toX = fromX + tilesByWidth;
            int toY = fromY + tilesByHeight;

            // flush used flag for all memory-cached tiles
            foreach (var c in _Cache)
            {
                c.Used = false;
            }

            for (int x = fromX; x <= toX; x++)
            {
                for (int y = fromY; y <= toY; y++)
                {
                    int x_ = (int)NormalizeTileNumber(x);
                    if (y >= 0 && y < FullMapSizeInTiles)
                    {
                        Tile tile = GetTile(x_, y, ZoomLevel);

                        // tile for current zoom and position found
                        if (tile != null)
                        {
                            if (tile.Image != null)
                            {
                                tile.Used = true;
                                DrawTile(g, x, y, tile.Image);
                            }
                            else
                            {
                                tile.Used = true;
                                DrawThumbnail(g, x, y, tile.ErrorMessage, true);
                            }
                        }
                        // tile not found, do some magic
                        else
                        {
                            // draw thumbnail first
                            DrawThumbnail(g, x, y, ThumbnailText, false);

                            // try to find out tile with less zoom level, and draw scaled part of that tile

                            int z = 1;
                            while (ZoomLevel - z >= 0)
                            {
                                // fraction of a tile to be drawn (1/2, 1/4 and etc.)
                                int f = 1 << z;

                                //  try to get tile with less zoom level from cache
                                tile = GetTile(x_ / f, y / f, ZoomLevel - z, fromCacheOnly: true);

                                // if tile found, draw part of it
                                if (tile != null && tile.Image != null)
                                {
                                    tile.Used = true;
                                    DrawTilePart(g, x, y, x_ % f, y % f, f, tile.Image);
                                    break;
                                }

                                // move up to less zoom level
                                z++;
                            }
                        }
                    }
                }
            }

            // Dispose images that were not used 
            _Cache.Where(c => !c.Used).ToList().ForEach(c => c.Image?.Dispose());

            // Update cache, leave only used images
            _Cache = new ConcurrentBag<Tile>(_Cache.Where(c => c.Used));
        }

        /// <summary>
        /// Draw layers
        /// </summary>
        /// <param name="gr">Graphics instance to draw on.</param>
        private void DrawLayers(Graphics gr)
        {
            List<Layer> drawLayerList = new List<Layer>(Layers).OrderBy(l => l.Level).ToList();

            foreach (Layer layer in drawLayerList)
            {
                DrawSingleLayer(layer, gr);
            }
        }

        /// <summary>
        /// Draw a single layer
        /// </summary>
        /// <param name="layer">Layer to draw.</param>
        /// <param name="gr">Graphics instance to draw on.</param>
        private void DrawSingleLayer(Layer layer, Graphics gr)
        {
            if (layer.Visible)
            {
                if (layer is LayerGroup)
                {
                    foreach (Layer l in ((LayerGroup)layer).Layers)
                    {
                        DrawSingleLayer(l, gr);
                    }
                }
                else if (layer is MarkerLayer)
                {
                    foreach (Marker marker in ((MarkerLayer)layer).Markers)
                    {
                        DrawSingleMarker(marker, gr);
                    }
                }
                else if (layer is TrackLayer)
                {
                    foreach (Track track in ((TrackLayer)layer).Tracks)
                    {
                        DrawSingleTrack(track, gr);
                    }
                }
                else if (layer is PolygonLayer)
                {
                    foreach (Polygon polygon in ((PolygonLayer)layer).Polygons)
                    {
                        DrawSinglePolygon(polygon, gr);
                    }
                }
                else if (layer is EllipseLayer)
                {
                    foreach (Ellipse ellipse in ((EllipseLayer)layer).Ellipses)
                    {
                        DrawSingleEllipse(ellipse, gr);
                    }
                }
            }
        }

        /// <summary>
        /// Draw a single marker
        /// </summary>
        /// <param name="marker">Marker to draw.</param>
        /// <param name="gr">Graphics instance to draw on.</param>
        private void DrawSingleMarker(Marker marker, Graphics gr)
        {
            var p = Project(marker.Point);
            if (p.X > -marker.Style.MarkerWidth && p.X < this.ClientSize.Width + marker.Style.MarkerWidth && p.Y > -marker.Style.MarkerWidth && p.Y < this.ClientSize.Height + marker.Style.MarkerWidth)
            {
                Draw(gr, () =>
                {
                    var eventArgs = new DrawMarkerEventArgs()
                    {
                        Graphics = gr,
                        Marker = marker,
                        Point = p
                    };

                    DrawMarker?.Invoke(this, eventArgs);
                    if (!eventArgs.Handled)
                    {
                        if (marker.Style.MarkerImage != null)
                        {
                            gr.DrawImage(marker.Style.MarkerImage, new Rectangle((int)p.X - marker.Style.MarkerImage.Width / 2, (int)p.Y - marker.Style.MarkerImage.Height / 2, marker.Style.MarkerImage.Width, marker.Style.MarkerImage.Height));
                        }
                        else
                        {
                            if (marker.Style.MarkerBrush != null)
                            {
                                gr.FillEllipse(marker.Style.MarkerBrush, p.X - marker.Style.MarkerWidth / 2, p.Y - marker.Style.MarkerWidth / 2, marker.Style.MarkerWidth, marker.Style.MarkerWidth);
                            }
                            if (marker.Style.MarkerPen != null)
                            {
                                gr.DrawEllipse(marker.Style.MarkerPen, p.X - marker.Style.MarkerWidth / 2, p.Y - marker.Style.MarkerWidth / 2, marker.Style.MarkerWidth, marker.Style.MarkerWidth);
                            }
                            if (marker.Style.LabelFont != null && marker.Style.LabelBrush != null && marker.Style.LabelFormat != null)
                            {
                                gr.DrawString(marker.Label, marker.Style.LabelFont, marker.Style.LabelBrush, new PointF(p.X, p.Y + 5), new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near });
                            }
                        }
                    }
                });
            } 
        }

        /// <summary>
        /// Draw a single track
        /// </summary>
        /// <param name="track">Track to draw.</param>
        /// <param name="gr">Graphics instance to draw on.</param>
        private void DrawSingleTrack(Track track, Graphics gr)
        {
            PointF[] points = new PointF[track.Count];
            bool atLeastOnePointVisible = false;

            for (int i = 0; i < track.Count; i++)
            {
                GeoPoint g = track.ElementAt(i);
                PointF p = Project(g);

                if (atLeastOnePointVisible || (p.X > 0 && p.X < this.ClientSize.Width && p.Y > 0 && p.Y < this.ClientSize.Height))
                {
                    atLeastOnePointVisible = true;
                }

                if (i > 0)
                {
                    p = points[i - 1].Nearest(p, new PointF(p.X - FullMapSizeInPixels, p.Y), new PointF(p.X + FullMapSizeInPixels, p.Y));
                }

                points[i] = p;
            }

            if (atLeastOnePointVisible)
            {
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

                    if (track.Style.DirectionIndicatorPen != null && ZoomLevel >= track.Style.DirectionIndicatorMinimumZoomLevel)
                    {
                        float directionIndicatorSize = track.Style.DirectionIndicatorPen.Width;
                        for (int a = 0; a < points.Length - 1; a++)
                        {
                            PointF pointA = points[a];
                            PointF pointB = points[a + 1];

                            if (pointA.X != pointB.X || pointA.Y != pointB.Y)
                            {
                                PointF directionIndicatorCenter = new PointF(
                                    (pointB.X + pointA.X) / 2.0f,
                                    (pointB.Y + pointA.Y) / 2.0f
                                );

                                float directionIndicatorAngle = (float)(Math.Atan2(pointA.Y - pointB.Y, pointA.X - pointB.X) * 180.0 / Math.PI);// - 90.0f;

                                Draw(gr, () =>
                                {
                                    gr.TranslateTransform(directionIndicatorCenter.X, directionIndicatorCenter.Y);
                                    gr.RotateTransform(directionIndicatorAngle);
                                    gr.TranslateTransform(-directionIndicatorCenter.X, -directionIndicatorCenter.Y);

                                    gr.FillPolygon(new SolidBrush(track.Style.DirectionIndicatorPen.Color), new PointF[]
                                    {
                                        new PointF(directionIndicatorCenter.X, directionIndicatorCenter.Y + directionIndicatorSize),
                                        new PointF(directionIndicatorCenter.X + directionIndicatorSize * 2.5f, directionIndicatorCenter.Y),
                                        new PointF(directionIndicatorCenter.X, directionIndicatorCenter.Y - directionIndicatorSize)
                                    });

                                    gr.ResetTransform();
                                });
                            }
                        }
                    }
                }
            } 
        }

        /// <summary>
        /// Draw a single polygon.
        /// </summary>
        /// <param name="polygon">Polygon to draw.</param>
        /// <param name="gr">Graphics instance to draw on.</param>
        private void DrawSinglePolygon(Polygon polygon, Graphics gr)
        {
            PointF p0 = PointF.Empty;
            using (GraphicsPath gp = new GraphicsPath())
            {
                bool atLeastOnePointVisible = false;
                
                gp.StartFigure();
                for (int i = 0; i < polygon.Count; i++)
                {
                    GeoPoint g = polygon.ElementAt(i);
                    PointF p = Project(g);

                    if (atLeastOnePointVisible || (p.X > 0 && p.X < this.ClientSize.Width && p.Y > 0 && p.Y < this.ClientSize.Height))
                    {
                        atLeastOnePointVisible = true;
                    }

                    if (i > 0)
                    {
                        p = p0.Nearest(p, new PointF(p.X - FullMapSizeInPixels, p.Y), new PointF(p.X + FullMapSizeInPixels, p.Y));
                        gp.AddLine(p0, p);
                    }

                    p0 = p;
                }
                gp.CloseFigure();

                if (atLeastOnePointVisible)
                {
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

        /// <summary>
        /// Draw a single ellipse.
        /// </summary>
        /// <param name="ellipse">Ellipse to draw.</param>
        /// <param name="gr">Graphics instance to draw on.</param>
        private void DrawSingleEllipse(Ellipse ellipse, Graphics gr)
        {
            var p = Project(ellipse.Point);

            float ellipseWidth = 0.0f;
            float ellipseHeight = 0.0f;

            if (ellipse.Style.EllipseUnit == EllipseStyle.Unit.PIXELS)
            {
                ellipseWidth = ellipse.Style.EllipseWidth;
                ellipseHeight = ellipse.Style.EllipseHeight;
            }
            else if (ellipse.Style.EllipseUnit == EllipseStyle.Unit.METERS)
            {
                double pixelPerMeter = MetersToPixels(ellipse.Point.Latitude, ZoomLevel);

                ellipseWidth = (float)(ellipse.Style.EllipseWidth / pixelPerMeter);
                ellipseHeight = (float)(ellipse.Style.EllipseHeight / pixelPerMeter);
            }
            else if (ellipse.Style.EllipseUnit == EllipseStyle.Unit.YARDS)
            {
                double pixelPerYard = YardsToPixels(ellipse.Point.Latitude, ZoomLevel);

                ellipseWidth = (float)(ellipse.Style.EllipseWidth / pixelPerYard);
                ellipseHeight = (float)(ellipse.Style.EllipseHeight / pixelPerYard);
            }

            if (p.X > -ellipseWidth && p.X < this.ClientSize.Width + ellipseWidth && p.Y > -ellipseHeight && p.Y < this.ClientSize.Height + ellipseHeight)
            {
                Draw(gr, () =>
                {
                    var eventArgs = new DrawEllipseEventArgs()
                    {
                        Graphics = gr,
                        Ellipse = ellipse,
                        Point = p
                    };

                    DrawEllipse?.Invoke(this, eventArgs);
                    if (!eventArgs.Handled)
                    {
                        if (ellipse.Style.EllipseBrush != null)
                        {
                            gr.FillEllipse(ellipse.Style.EllipseBrush, p.X - ellipseWidth / 2, p.Y - ellipseHeight / 2, ellipseWidth, ellipseHeight);
                        }
                        if (ellipse.Style.EllipsePen != null)
                        {
                            gr.DrawEllipse(ellipse.Style.EllipsePen, p.X - ellipseWidth / 2, p.Y - ellipseHeight / 2, ellipseWidth, ellipseHeight);
                        }
                    }
                });
            }
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
        private void DrawTilePart(Graphics gr, int x, int y, int xRemainder, int yRemainder, int frac, Image image)
        {
            // coordinates of a tile on the map
            Point p = new Point();
            p.X = _Offset.X + x * TILE_SIZE;
            p.Y = _Offset.Y + y * TILE_SIZE;

            // Calc source portion of the tile
            Rectangle srcRect = new Rectangle(TILE_SIZE / frac * xRemainder, TILE_SIZE / frac * yRemainder, TILE_SIZE / frac, TILE_SIZE / frac);

            // Destination rectangle
            Rectangle destRect = new Rectangle(p.X - frac, p.Y - frac, TILE_SIZE + 2 * frac, TILE_SIZE + 2 * frac);

            var state = gr.Save();
            gr.SmoothingMode = SmoothingMode.HighSpeed;
            gr.InterpolationMode = InterpolationMode.NearestNeighbor;
            gr.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            gr.CompositingQuality = CompositingQuality.HighSpeed;
            gr.DrawImage(image, destRect, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, GraphicsUnit.Pixel, TileImageAttributes);
            gr.Restore(state);
        }

        /// <summary>
        /// Draws a tile on the map.
        /// </summary>
        /// <param name="gr">Graphics instance to draw on.</param>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index of the tile.</param>
        /// <param name="image">Tile image.</param>
        private void DrawTile(Graphics gr, int x, int y, Image image)
        {
            Point p = new Point();
            p.X = _Offset.X + x * TILE_SIZE;
            p.Y = _Offset.Y + y * TILE_SIZE;

            gr.DrawImage(image, new Rectangle(p, new Drawing.Size(TILE_SIZE, TILE_SIZE)), 0, 0, TILE_SIZE, TILE_SIZE, GraphicsUnit.Pixel, TileImageAttributes);
        }

        /// <summary>
        /// Draws thumbnail frame and text instead of a tile.
        /// </summary>
        /// <param name="gr">Graphics instance to draw on.</param>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index of the tile.</param>
        /// <param name="message">Message to be displayed instead of the tile.</param>
        /// <param name="isError">Flag indicating the message should be displayed with error color.</param>
        private void DrawThumbnail(Graphics gr, int x, int y, string message, bool isError)
        {
            if (ShowThumbnails || isError)
            {
                Point p = new Point();
                p.X = _Offset.X + x * TILE_SIZE;
                p.Y = _Offset.Y + y * TILE_SIZE;
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

        #endregion

        #region Private Mapping Methods

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
        /// Returns the number of pixels per meter according to the reference latitude and zoom level.
        /// </summary>
        /// <param name="latitude">Latitude for which the distance should be calculated</param>
        /// <param name="zoomLevel">Current zoomlevel</param>
        /// <returns>Number of pixels representing one meter.</returns>
        public double MetersToPixels(double latitude, int zoomLevel)
        {
            var earthCircumference = 40075016.69;
            var worldSize = TILE_SIZE * Math.Pow(2, zoomLevel);

            var latitudeRadians = latitude * (Math.PI / 180);

            return earthCircumference * Math.Cos(latitudeRadians) / worldSize;
        }

        /// <summary>
        /// Returns the number of pixels per yard according to the reference latitude and zoom level.
        /// </summary>
        /// <param name="latitude">Latitude for which the distance should be calculated</param>
        /// <param name="zoomLevel">Current zoomlevel</param>
        /// <returns>Number of pixels representing one yard.</returns>
        public double YardsToPixels(double latitude, int zoomLevel)
        {
            var earthCircumference = 52626768;
            var worldSize = TILE_SIZE * Math.Pow(2, zoomLevel);

            var latitudeRadians = latitude * (Math.PI / 180);

            return earthCircumference * Math.Cos(latitudeRadians) / worldSize;
        }

        /// <summary>
        /// Gets projection of geographical coordinates onto the map.
        /// </summary>
        /// <param name="g">Point with geographical coordinates.</param>
        /// <returns><see cref="PointF"/> object representing projection of the specified geographical coordinates on the map.</returns>
        public PointF Project(GeoPoint g)
        {
            var p = WorldToTilePos(g);
            return new PointF(p.X * TILE_SIZE + _Offset.X, p.Y * TILE_SIZE + _Offset.Y);
        }

        /// <summary>
        /// Converts geographical coordinates to tile indices with fractions.
        /// </summary>
        /// <param name="g">Point with geographical coordinates.</param>
        /// <returns>Point representing X/Y indices of the specified geographical coordinates in Slippy map scheme.</returns>
        public PointF WorldToTilePos(GeoPoint g)
        {
            PointF p = new Point();
            p.X = (float)((g.Longitude + 180.0) / 360.0 * (1 << ZoomLevel));
            p.Y = (float)((1.0 - Math.Log(Math.Tan(g.Latitude * Math.PI / 180.0) +
                1.0 / Math.Cos(g.Latitude * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << ZoomLevel));

            return p;
        }

        /// <summary>
        /// Converts tile indices to geographical coordinates.
        /// </summary>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index of the tile.</param>
        /// <returns>Point representing geographical coordinates.</returns>
        public GeoPoint TileToWorldPos(double x, double y)
        {
            GeoPoint g = new GeoPoint();
            double n = Math.PI - ((2.0 * Math.PI * y) / Math.Pow(2.0, ZoomLevel));
            g.Longitude = (float)((x / Math.Pow(2.0, ZoomLevel) * 360.0) - 180.0);
            g.Latitude = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));
            return g;
        }

        #endregion

        #region Private Support Methods

        /// <summary>
        /// Adjusts map bounds if required.
        /// </summary>
        private void AdjustMapBounds()
        {
            if (FitToBounds)
            {
                if (FullMapSizeInPixels > Height)
                {
                    if (_Offset.Y > 0) _Offset.Y = 0;
                    if (_Offset.Y + FullMapSizeInPixels < Height) _Offset.Y = Height - FullMapSizeInPixels;
                }
                else
                {
                    if (_Offset.Y > Height - FullMapSizeInPixels) _Offset.Y = Height - FullMapSizeInPixels;
                    if (_Offset.Y < 0) _Offset.Y = 0;
                }
            }
        }

        /// <summary>
        /// Sets zoom level with specifying central point to zoom in/out.
        /// </summary>
        /// <param name="z">Zoom level to be set.</param>
        /// <param name="p">Central point to zoom in/out.</param>
        private void SetZoomLevel(int z, Point p)
        {
            int max = TileServer != null ? Math.Min(MaxZoomLevel, TileServer.MaxZoomLevel) : MaxZoomLevel;
            int min = TileServer != null ? Math.Max(MinZoomLevel, TileServer.MinZoomLevel) : MinZoomLevel;

            if (z < min) z = min;
            if (z > max) z = max;

            if (z != ZoomLevel)
            {
                double factor = Math.Pow(2, z - ZoomLevel);
                _Offset.X = (int)((_Offset.X - p.X) * factor) + p.X;
                _Offset.Y = (int)((_Offset.Y - p.Y) * factor) + p.Y;

                _ZoomLevel = z;

                _Offset.X = (int)(_Offset.X % FullMapSizeInPixels);

                Invalidate();
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
        private Tile GetTile(int x, int y, int z, bool fromCacheOnly = false)
        {
            try
            {
                Tile tile;

                // try to get tile from memory cache
                tile = _Cache.FirstOrDefault(t => t.X == x && t.Y == y && t.Z == z && t.TileServer == TileServer.GetType().Name);
                if (tile != null)
                {
                    return tile;
                }

                // try to get tile from file system
                if (TileServer is IFileCacheTileServer fcTileServer)
                {
                    string localPath = Path.Combine(CacheFolder, TileServer.GetType().Name, $"{z}", $"{x}", $"{y}.tile");
                    if (File.Exists(localPath))
                    {
                        var fileInfo = new FileInfo(localPath);
                        if (fileInfo.Length > 0 && fileInfo.LastWriteTime + fcTileServer.TileExpirationPeriod >= DateTime.Now)
                        {
                            Image image = Image.FromFile(localPath);
                            tile = new Tile(image, x, y, z, TileServer.GetType().Name);
                            _Cache.Add(tile);
                            return tile;
                        }
                    }
                }

                // get tile from the server 
                if (!fromCacheOnly)
                {
                    RequestTile(x, y, z);
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
        private void RequestTile(int x, int y, int z)
        {
            // Intialize worker, if not yet initialized
            if (_Worker == null)
            {
                _Worker = new Thread(new ThreadStart(ProcessRequests));
                _Worker.IsBackground = true;
                _Worker.Start();
            }

            // Check the tile is already requested
            if (!_RequestPool.Any(t => t.X == x && t.Y == y && t.Z == z))
            {
                _RequestPool.Add(new Tile(x, y, z, TileServer.GetType().Name));
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
                if (_RequestPool.TryPeek(out Tile tile))
                {
                    try
                    {
                        // ignore pooled items with different zoom level and another tile server
                        if (tile.TileServer == TileServer.GetType().Name && tile.Z == ZoomLevel)
                        {
                            tile.Image = TileServer.GetTile(tile.X, tile.Y, tile.Z);
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
                        if (TileServer is IFileCacheTileServer && tile.Image != null)
                        {
                            string localPath = Path.Combine(CacheFolder, tile.TileServer, $"{tile.Z}", $"{tile.X}", $"{tile.Y}.tile");
                            try
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(localPath));
                                tile.Image.Save(localPath);
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

                        // remove the tile from requests pool
                        _RequestPool.TryTake(out tile);

                        // redraw the map
                        Invalidate();
                    }
                }
                else
                {
                    _WorkerWaitHandle.WaitOne();
                }
            };
        }

        /// <summary>
        /// Returns the bounds of a layer.
        /// </summary>
        /// <param name="layer">Layer to measure.</param>
        /// <returns>Bounds of a layer, null if layer doesn't contain objects.</returns>
        private GeoPoint[] MeasureLayer(Layer layer)
        {
            GeoPoint[] resultPoints = null;

            double lowestLongitude = 180.0;
            double highestLongitude = -180.0;
            double lowestLatitude = 90.0;
            double highestLatitude = -90.0;

            bool layerContainsObjects = false;

            if (layer is LayerGroup)
            {              
                foreach(Layer l in ((LayerGroup)layer).Layers)
                {
                    GeoPoint[] layerBounds = MeasureLayer(l);

                    if (layerBounds != null)
                    {
                        if (layerBounds[0].Longitude < lowestLongitude)
                        {
                            lowestLongitude = layerBounds[0].Longitude;
                        }

                        if (layerBounds[3].Longitude > highestLongitude)
                        {
                            highestLongitude = layerBounds[1].Longitude;
                        }

                        if (layerBounds[3].Latitude < lowestLatitude)
                        {
                            lowestLatitude = layerBounds[3].Latitude;
                        }

                        if (layerBounds[0].Latitude > highestLatitude)
                        {
                            highestLatitude = layerBounds[0].Latitude;
                        }

                        layerContainsObjects = true;
                    }
                }
            }
            else if (layer is MarkerLayer)
            {
                foreach (Marker marker in ((MarkerLayer)layer).Markers)
                {
                    if (marker.Point.Longitude < lowestLongitude)
                    {
                        lowestLongitude = marker.Point.Longitude;
                    }
                    
                    if (marker.Point.Longitude > highestLongitude)
                    {
                        highestLongitude = marker.Point.Longitude;
                    }

                    if (marker.Point.Latitude < lowestLatitude)
                    {
                        lowestLatitude = marker.Point.Latitude;
                    }
                    
                    if (marker.Point.Latitude > highestLatitude)
                    {
                        highestLatitude = marker.Point.Latitude;
                    }

                    layerContainsObjects = true;
                }
            }
            else if (layer is TrackLayer)
            {
                foreach (Track track in ((TrackLayer)layer).Tracks)
                {
                    foreach (GeoPoint point in track)
                    {
                        if (point.Longitude < lowestLongitude)
                        {
                            lowestLongitude = point.Longitude;
                        }
                        
                        if (point.Longitude > highestLongitude)
                        {
                            highestLongitude = point.Longitude;
                        }

                        if (point.Latitude < lowestLatitude)
                        {
                            lowestLatitude = point.Latitude;
                        }
                        
                        if (point.Latitude > highestLatitude)
                        {
                            highestLatitude = point.Latitude;
                        }

                        layerContainsObjects = true;
                    }
                }
            }
            else if (layer is PolygonLayer)
            {
                foreach (Polygon polygon in ((PolygonLayer)layer).Polygons)
                {
                    foreach (GeoPoint point in polygon)
                    {
                        if (point.Longitude < lowestLongitude)
                        {
                            lowestLongitude = point.Longitude;
                        }
                        
                        if (point.Longitude > highestLongitude)
                        {
                            highestLongitude = point.Longitude;
                        }

                        if (point.Latitude < lowestLatitude)
                        {
                            lowestLatitude = point.Latitude;
                        }
                        
                        if (point.Latitude > highestLatitude)
                        {
                            highestLatitude = point.Latitude;
                        }

                        layerContainsObjects = true;
                    }
                }
            }
            else if (layer is EllipseLayer)
            {
                foreach (Ellipse ellipse in ((EllipseLayer)layer).Ellipses)
                {
                    if (ellipse.Point.Longitude < lowestLongitude)
                    {
                        lowestLongitude = ellipse.Point.Longitude;
                    }
                    
                    if (ellipse.Point.Longitude > highestLongitude)
                    {
                        highestLongitude = ellipse.Point.Longitude;
                    }

                    if (ellipse.Point.Latitude < lowestLatitude)
                    {
                        lowestLatitude = ellipse.Point.Latitude;
                    }
                    
                    if (ellipse.Point.Latitude > highestLatitude)
                    {
                        highestLatitude = ellipse.Point.Latitude;
                    }

                    layerContainsObjects = true;
                }
            }

            if (layerContainsObjects)
            {
                resultPoints = new GeoPoint[4];

                resultPoints[0] = new GeoPoint((float)lowestLongitude, (float)highestLatitude);     // TOP LEFT
                resultPoints[1] = new GeoPoint((float)highestLongitude, (float)highestLatitude);    // TOP RIGHT
                resultPoints[2] = new GeoPoint((float)lowestLongitude, (float)lowestLatitude);      // BOTTOM LEFT
                resultPoints[3] = new GeoPoint((float)highestLongitude, (float)lowestLatitude);     // BOTTOM RIGHT
            }

            return resultPoints;
        }

        /// <summary>
        /// Iterates over all elements in layers and returns the first matching element of the top most layer.
        /// </summary>
        /// <param name="position">Cursor positon</param>
        /// <returns>The first matching element, null if there was no element found</returns>
        private IElement FindTouchedElement(Layer layer, double x, double y)
        {
            PointF screenPoint = new PointF((float) x, (float) y);
            
            if (layer is LayerGroup)
            {
                foreach (Layer l in ((LayerGroup)layer).Layers.OrderByDescending(l => l.Level))
                {
                    IElement touchedElement = FindTouchedElement(l, x, y);

                    if (touchedElement != null)
                    {
                        return touchedElement;
                    }
                }
            }
            else if (layer is MarkerLayer)
            {
                foreach (Marker marker in ((MarkerLayer)layer).Markers)
                {
                    PointF point = Project(marker.Point);
                    double distance = Math.Sqrt(Math.Pow(point.X - screenPoint.X, 2) + Math.Pow(point.Y - screenPoint.Y, 2));

                    if (distance <= marker.Style.MarkerWidth / 2)
                    {
                        return marker;
                    }
                }
            }
            else if (layer is TrackLayer)
            {
                foreach (Track track in ((TrackLayer)layer).Tracks)
                {
                    foreach(GeoPoint trackPoint in track)
                    {
                        PointF point = Project(trackPoint);
                        double distance = Math.Sqrt(Math.Pow(point.X - screenPoint.X, 2) + Math.Pow(point.Y - screenPoint.Y, 2));

                        if (distance <= 5)
                        {
                            return track;
                        }
                    }
                    
                }
            }
            else if (layer is PolygonLayer)
            {
                foreach (Polygon polygon in ((PolygonLayer)layer).Polygons)
                {
                    // find outer bounds of the polygon
                    double minX = polygon[0].Latitude;
                    double maxX = polygon[0].Latitude;
                    double minY = polygon[0].Longitude;
                    double maxY = polygon[0].Longitude;
                    for (int i = 1; i < polygon.Count; i++)
                    {
                        PointF q = Project(polygon[i]);
                        minX = Math.Min(q.X, minX);
                        maxX = Math.Max(q.X, maxX);
                        minY = Math.Min(q.Y, minY);
                        maxY = Math.Max(q.Y, maxY);
                    }

                    // if screen point is outside these bounds, it cannot be in polygon
                    // -> continue with next one
                    if (screenPoint.X < minX || screenPoint.X > maxX || screenPoint.Y < minY || screenPoint.Y > maxY)
                    {
                        continue;
                    }

                    // check whether screen point is in the area drawn by the polygon
                    bool inside = false;
                    for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
                    {
                        PointF pointI = Project(polygon[i]);
                        PointF pointJ = Project(polygon[j]);
                        
                        if ((pointI.Y > screenPoint.Y) != (pointJ.Y > screenPoint.Y) && screenPoint.X < (pointJ.X - pointI.X) * (screenPoint.Y - pointI.Y) / (pointJ.Y - pointI.Y) + pointI.X)
                        {
                            inside = !inside;
                        }
                    }

                    // if inside == true -> screen point is in polygon
                    if (inside)
                    {
                        return polygon;
                    }
                }
            }
            else if (layer is EllipseLayer)
            {
                foreach (Ellipse ellipse in ((EllipseLayer)layer).Ellipses)
                {
                    PointF ellipsePoint = Project(ellipse.Point);
                    float ellipseWidth = 0.0f;
                    float ellipseHeight = 0.0f;

                    if (ellipse.Style.EllipseUnit == EllipseStyle.Unit.PIXELS)
                    {
                        ellipseWidth = ellipse.Style.EllipseWidth;
                        ellipseHeight = ellipse.Style.EllipseHeight;
                    }
                    else if (ellipse.Style.EllipseUnit == EllipseStyle.Unit.METERS)
                    {
                        double pixelPerMeter = MetersToPixels(ellipse.Point.Latitude, ZoomLevel);

                        ellipseWidth = (float)(ellipse.Style.EllipseWidth / pixelPerMeter);
                        ellipseHeight = (float)(ellipse.Style.EllipseHeight / pixelPerMeter);
                    }
                    else if (ellipse.Style.EllipseUnit == EllipseStyle.Unit.YARDS)
                    {
                        double pixelPerYard = YardsToPixels(ellipse.Point.Latitude, ZoomLevel);

                        ellipseWidth = (float)(ellipse.Style.EllipseWidth / pixelPerYard);
                        ellipseHeight = (float)(ellipse.Style.EllipseHeight / pixelPerYard);
                    }

                    if (ellipseWidth >= 2 && ellipseHeight >= 2)
                    {
                        double inside = (Math.Pow(screenPoint.X - ellipsePoint.X, 2) /
                                        Math.Pow(ellipseWidth / 2, 2)) +
                                        (Math.Pow(screenPoint.Y - ellipsePoint.Y, 2) /
                                        Math.Pow(ellipseHeight / 2, 2));

                        if (inside <= 1)
                        {
                            return ellipse;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Handles event when a property of a layer is changed.
        /// </summary>
        /// <param name="sender">Layer which raised the event.</param>
        /// <param name="args">Event arguments.</param>
        private void LayerPropertyChanged(object sender, EventArgs args)
        {
            Invalidate();
        }

        /// <summary>
        /// Handles clicks on LinkLabel links
        /// </summary>
        private void _LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString());
        }

        #endregion
    }
}

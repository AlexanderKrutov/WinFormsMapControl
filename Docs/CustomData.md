# Displaying Custom Data
There are three different types of objects used to display data on the map control:
* `Marker` - for displaying single-point objects;
* `Track` - for displaying multiline objects (set of connected points);
* `Polygon` - for displaying area objects.

Map control has correspoding properties to add these objects, i.e.:

```csharp
public ICollection<Marker> Markers { get; }
```
```csharp
public ICollection<Track> Tracks { get; }
```
```csharp
public ICollection<Polygon> Polygons { get; }
```

## Markers

By default, markers are drawn as filled circles with specified diameter in pixels, and with label.
To add new marker to the map:

```csharp
// Create marker's location point
var point = new GeoPoint(44.0, 56.3333);

// Create marker instance: specify location on the map, drawing style, and label
var marker = new Marker(point, MarkerStyle.Default, "Some label");

// Add marker to the map
mapControl.Markers.Add(marker);
```

You can customize the marker drawing style by:

```csharp
// Set marker diameter (3), fill brush (red), outline pen (blue), label brush (black), label font (default system), label formatting (generic default)
var style = new MarkerStyle(3, Brushes.Red, Pens.Blue, Brushes.Black, SystemFonts.DefaultFont, StringFormat.GenericDefault);

// Create marker with custom style
var marker = new Marker(point, style, "Some label");
```

Also you can override drawing the marker with handling `DrawMarker` event.
For example, to draw a 24x24 image instead of marker circle, do the following:

```csharp
// Subscribe to DrawMarker event
mapControl.DrawMarker += new EventHandler<DrawMarkerEventArgs>(mapControl_DrawMarker);

// Create marker image
Image imageMarker = Image.FromFile("Path/to/image.png");

// Event handler
private void mapControl_DrawMarker(object sender, DrawMarkerEventArgs e)
{
    // Important: set flag to override drawing
    e.Handled = true;

    // Draw image
    e.Graphics.DrawImage(imageMarker, new Rectangle((int)e.Point.X - 12, (int)e.Point.Y - 24, 24, 24));
    
    // And label
    e.Graphics.DrawString(e.Marker.Label, SystemFonts.DefaultFont, Brushes.Red, new PointF(e.Point.X, e.Point.Y + 5), new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near });
}

```
You can use different images for markers. To distinguish markers, use `Data` property of the `Marker` class. It can be any object you want, for example, marker unique identifier.

## Tracks
To add a track to the map, do the following:

```csharp
// Initializing collection of points,
// filling points with data omitted
var points = new List<GeoPoint>();

// Creating a track with default style
var track = new Track(TrackStyle.Default);

// Add points to the track
track.AddRange(points);

// Add track to the map
mapControl.Tracks.Add(track);
```

Track style is also customizable:
```csharp
// Define custom track style
var style = new TrackStyle(new Pen(Color.Blue) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash });

// Assign style to the track
var track = new Track(style);
```

## Polygons
Working with polygons is similar to deal with tracks.
To create a new area object to the map, do something like:

```csharp
// Default-styled polygon
var polygon = new Polygon(PolygonStyle.Default);

// Add points
polygon.AddRange(points);

// Add it to the map
mapControl.Polygons.Add(polygon);
```

Again, if you need to customize style of the polygon:
```csharp
// Brush to fill polygon area
var brush = Brushes.Gray;

// Pen to draw polygon outline
var pen = Pens.Black;

// Custom style
var style = new PolygonStyle(brush, pen);

// Make styled polygon
var polygon = new Polygon(style);
```

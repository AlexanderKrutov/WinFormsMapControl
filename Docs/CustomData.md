# Displaying Custom Data
There are four different types of objects used to display data on the map control:
* `Marker` - for displaying single-point objects;
* `Track` - for displaying multiline objects (set of connected points);
* `Polygon` - for displaying area objects.
* `Ellipse` - for displaying an ellipse around a center point with size in meters, yards or pixels.

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
```csharp
public ICollection<Ellipse> Ellipses { get; }
```

To redraw the map after adding an element automatically use the `Add` methods like this:

```csharp
mapControl.AddMarker(new Marker(...));
```
```csharp
mapControl.AddTrack(new Track(...));
```
```csharp
mapControl.AddPolygon(new Polygon(...));
```
```csharp
mapControl.AddEllipse(new Ellipse(...));
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
mapControl.AddMarker(marker);
```

You can customize the marker drawing style by:

```csharp
// Set marker diameter (3), fill brush (red), outline pen (blue), label brush (black), label font (default system), label formatting (generic default)
var style = new MarkerStyle(3, Brushes.Red, Pens.Blue, Brushes.Black, SystemFonts.DefaultFont, StringFormat.GenericDefault);

// Create marker with custom style
var marker = new Marker(point, style, "Some label");
```

To draw an image instead of a point for a marker, use the image in marker style:

```csharp
// Create marker image
Image imageMarker = Image.FromFile("Path/to/image.png");

var style = new MarkerStyle(imageMarker);

var marker = new Marker(point, style, "Some label");

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
mapControl.AddTrack(track);
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
mapControl.AddPolygon(polygon);
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
## Ellipses

Default size unit for ellipses are pixels. You can add an ellipse to your map using the following code:

```csharp
// Create ellipses's location point
var point = new GeoPoint(44.0, 56.3333);

// Create ellpise instance: specify location on the map and drawing style
var ellipse = new Ellipse(point, EllipseStyle.Default);

// Add marker to the map
mapControl.AddEllipse(ellipse);
```

Ellipse style is also customizable:
```csharp
// Define custom ellipse style
var style = new EllipseStyle(500, 500, Brush.Blue, Pen.Blue, EllipseStyle.Unit.METERS);

// Assign style to the ellipse
var ellipse = new Ellipse(style);
```
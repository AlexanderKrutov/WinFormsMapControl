# Adding own tile server

You can add custom tile server by two ways.

**If your tile server is a web source**, you should to inherit from base abstract class `WebTileServer`.
Here is sample implementation of a tile server located by addess http://map.example.com

```csharp
public class SampleTileServer : WebTileServer
{
    // This is just a displayable name of the server
    public override string Name => "Sample Tile Server";

    // This text will be displayed in the bottom right corner of the map control.
    // Most of tile servers require attribution text to be specified.
    // Text can contain HTML links, they will be clickable.
    // If no attribution text is required, just return null.
    public override string AttributionText => "<a href='http://map.example.com/'>Map attribution text</a>";

    // The method should return valid URL string by x, y, and z arguments.
    public override Uri GetTileUri(int x, int y, int z)
    {
        return new Uri($"http://map.example.com/{z}/{x}/{y}.png");
    }

    // Optional: Maximal zoom level. 
    // You can override it to limit maximal value with smallest value than default.
    public override int MaxZoomLevel => 13;

    // Optional: Minimal zoom level. 
    // You can override it to limit minimal value with largest value than default.
    public override int MinZoomLevel => 1;
    
    // Optional: User-Agent string
    // Some tile servers require User-Agent string to identify your application.
    public override string UserAgent { get; set; } = "My Application";
}

```

**If your map tiles are offline**, for example embedded in your app or located in a database, you should to implement interface `ITileServer`.
Here is a sample implentation of a tile server with database source.

```csharp
public class DatabaseTileServer : ITileServer
{
    // This is just a displayable name of the server
    public string Name => "Sample Tile Server";

    // Assume attribution is not needed
    public string AttributionText => null;

    // Minimal zoom level
    public int MinZoomLevel => 0;

    // Maximal zoom level
    public int MaxZoomLevel => 10;

    // Getting tile
    public Image GetTile(int x, int y, int z)
    {
        // Logic of getting tile by X, Y and Z from DB is omitted         
        Image image = ...
        return image;
    }
}
```
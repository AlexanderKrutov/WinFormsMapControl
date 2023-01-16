# Documentation

## Quick start
To add map control to your Windows Forms application, install the package and just drag and drop the control from the Visual Studio Toolbox to a Form.

```csharp
// Somewhere in Form Designer
MapControl mapControl = new MapControl();
```

Before using the control, you need to specify values for the following control properties:

* `TileServer` - it defines tiles source used for map control;
* `CacheFolder` - should be specified if you are using web-based tile server. 


### For the offline (embedded) maps:
```csharp
mapControl.TileServer = new OfflineTileServer();
```

### For web-based tile servers:

```csharp
// If tile server supports file system caching, 
// you should specify path to the cache folder.
// Web-based tile servers strongly require file system caching
// to prevent highload of the server.
mapControl.CacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MapControl");

// Use tiles from OpenTopoMap (https://opentopomap.org/)
mapControl.TileServer = new OpenTopoMapServer();
```

That's it! Run the app to see the map control in action.

## Customization

You may also need to change values of following properties to customize the map control before using:

* `MinZoomLevel` - minimal zoom value of the map;
* `MinZoomLevel` - maximal zoom value of the map;
* `ZoomLevel` - current zoom level;
* `FitToBounds` - set to `true` if you want the map will not slide over the vertical bounds of the control;
* `BackColor` - to change the color of the control itself;
* `ErrorColor` - color used to draw error messages, for example when a tile was not found;
* `ShowThumbnails` - if you are want to display thumbnails while loading map tiles;
* `ThumbnailText` - text to be displayed when a tile is downloading from the server;
* `ThumbnailForeColor` - thumbnail text color;
* `ThumbnailBackColor` - background of the tile thumbnail;

Other properties are also available, see documenation comments.

## Custom data
[See here](/Docs/CustomData.md) how to add markers, tracks, polygons and ellipses on the map.

## Layers
[See here](/Docs/Layer.md) how to use layers in the map.

## Events
[See how](/Docs/Event.md) to react to basic events on elements.

## Custom tile servers
[Instruction](/Docs/TileServer.md) how to create own tile server or how to use offline map sources [offline map sources](/Docs/OfflineMaps.md)
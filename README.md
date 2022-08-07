# WinFormsMapControl
Windows Forms control for displaying online and offline maps.

![Demo](/Docs/Demo.png)

## Installation [![NuGet Status](http://img.shields.io/nuget/v/WinFormsMapControl.svg?style=flat)](https://www.nuget.org/packages/WinFormsMapControl/)
```
PM> Install-Package WinFormsMapControl
```

## Features

* Different map sources:
  * Offline maps (embedded tiles from [MapTiler](https://www.maptiler.com/copyright/))
  * [OpenStreetMap](https://www.openstreetmap.org/)
  * [OpenTopoMap](https://opentopomap.org/)
  * [Stamen Terrain](http://maps.stamen.com/terrain/)
* Easy to add new tile sources
* Infinite map (endless in longitude)
* File system and in-memory tile cache
* Custom data over the map:
  * Markers
  * Tracks
  * Polygons
  * Ellipses (with size in meters, yards or pixels)
* Layers for grouping data together

## How to use it?
[See documentation](/Docs/Index.md)

## License
MapControl is licensed under [MIT license](LICENSE).
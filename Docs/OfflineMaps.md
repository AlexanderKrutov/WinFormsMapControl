# Using Offline Maps

You can use offline tiles located in a ZIP archive by using the class `ZipTileServer` as follows

```csharp
ZipTileServer tileServer = new ZipTileServer("your/path/to/zip/file.zip")
```

or

```csharp
ZipTileServer tileServer = new ZipTileServer("your/path/to/zip/file.zip", "myFancyMapStyle")
```

Only requirement is that you have your tiles located in a ZIP archive with following structure:

```
/file.zip
	./default/
		./info.txt
		./0/0/0.jpg
		./1/1/0.jpg
		...
	./myFancyMapStyle/
		./info.txt
		./0/0/0.jpg
		./1/1/0.jpg
		...
```

By using this structure, several map styles can be obtained using one single ZIP archive. The `info.txt` file contains three lines with min zoom level (1st line), max zoom  level (2nd line) and image file extension (3rd line). The `ZipTileServer` uses this file to determine a valid map data archive and auto-detect required meta information. You can find a sample python script creating such ZIP archives in [this gist](https://gist.github.com/sebastianknopf/d1d1819181240e17b6c16ba44f3d6c11).

Please note that offline maps like this have a very high storage consumtion increased with each zoom level. The higher the zoom level contained in the archive, the higher the required storage. Use [this tool of Geofabrik](https://tools.geofabrik.de/calc/#type=geofabrik_standard&bbox=8.5429,48.349,9.8395,49.1504) to see how many storage you would need to render your desired map.
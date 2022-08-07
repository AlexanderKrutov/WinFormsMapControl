# Using Layers and LayerGroups
Layers enable you to group elements like markers, polygons (...) in logical units. Advantages of using layers:
* Group elements in logical units 
* Show / hide a layer in map control
* Order different layers by their z-index
* Zoom map to fit data of a whole layer

## Layers
Each layer requires a z-index in constructor. A higher z-index will show a layer before other layers with a lower z-index.
To create a new layer on the map:

```csharp
// create new layer instance
var layer = MarkerLayer(0);

// Add layer to the map
mapControl.Layers.Add(layer);

// ... or
mapControl.AddLayer(layer);

// ...

// Add a new marker to the layer which will be displayed on the map
layer.AddMarker(new Marker(...));

// ... 

// Clear the whole layer
layer.Clear();
```

Note that elements which are added to the map control directly without a layer will displayed always in front of all layers!

To change visibility or z-index of a layer in the map, simply change the corresponding property:

```csharp
// Change visibility
layer.Visible = false;
layer.Visible = true;

// Change z-index (level)
layer.Level = 0;
layer.Level = 1;
// ...
```

## LayerGroups
Even layers can be grouped together in layers by using a `LayerGroup`. Layer groups do not require an z-index, but can be used with one. Layer groups without having a z-index specified will be drawn in the order that they were added to the layer list of the map. To create a new layer group:

```csharp
// Create layer group
var layerGroup = new LayerGroup();

// Add layer group to map
mapControl.AddLayer(layerGroup);

// Add layers to layer group 
layerGroup.AddLayer(layer1);
layerGroup.AddLayer(layer2);
```
Everything other works like with an ordinary layer.

## Zoom to layers
In some cases you might want to zoom the map control to a set of elements. To achieve this, group them together in a layer, add the layer to the map and then use:

```csharp
mapControl.ZoomTo(groupLayer);
```

to let the map display the elements of the layer.
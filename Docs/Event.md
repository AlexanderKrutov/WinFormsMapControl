# React to events
By using events you can add a tooltip when a polygon is entered by mouse or show a message box when a marker is clicked. 

_Note: Events work only for elements added into layers! Elementes added to map directly will not raise events!_

Also, elements are considered in the order they're displayed in the map. That means if a marker is displayed on top of a polygon, only the marker will trigger an event!

## Layers
Each layer requires a z-index in constructor. A higher z-index will show a layer before other layers with a lower z-index.
To create a new layer on the map:

```csharp
// add enter event handler
mapControl.ElementEnter += new EventHandler<DrawMarkerEventArgs>(mapControl_ElementEnter);

// enter event handler
private void mapControl_ElementEnter(object sender, MapControlElementEventArgs e)
{
	// access element which has been entered ...	
	Polygon polygon = e.Element as Polygon;

	// do your stuff here ...
}

// add click event handler
mapControl.ElementClick += new EventHandler<DrawMarkerEventArgs>(mapControl_ElementClick);

// click event handler
private void mapControl_ElementClick(object sender, MapControlElementEventArgs e)
{
	// access element which has been clicked ...	
	Polygon polygon = e.Element as Polygon;

	// do your stuff here ...
}

// add click event handler
mapControl.ElementLeave += new EventHandler<DrawMarkerEventArgs>(mapControl_ElementLeave);

// click event handler
private void mapControl_ElementLeave(object sender, MapControlElementEventArgs e)
{
	// access element which has been leaved ...	
	Polygon polygon = e.Element as Polygon;

	// do your stuff here ...
}

```

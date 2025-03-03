using Mapsui;
using Mapsui.Layers;
using ReactiveUI;

namespace Voltflow.ViewModels.Pages.Map.SidePanels;


//This has to be abstract because of view locator
/// <summary>
/// General class for map view side panels
/// </summary>
public abstract class MapSidePanelBase(MemoryLayer layer, IScreen screen) : ViewModelBase(screen)
{
    protected MemoryLayer _pointsLayer = layer;

    public abstract void MapClicked(MapInfoEventArgs e);
}

using Mapsui;
using Mapsui.Layers;
using ReactiveUI;

namespace Voltflow.ViewModels.Pages.Map.SidePanels;


//This has to be abstract because of view locator
/// <summary>
/// General class for map view side panels
/// </summary>
public abstract class MapSidePanelBase : ViewModelBase
{
    protected MemoryLayer _pointsLayer;

    protected MapSidePanelBase(MemoryLayer layer, IScreen screen) : base(screen)
    {
        _pointsLayer = layer;
    }

    public abstract void MapClicked(MapInfoEventArgs e);
}

using Mapsui.Extensions;
using Mapsui.Styles;

namespace Voltflow.Models;

public static class Marker
{
    public static readonly int Red = typeof(Marker).LoadBitmapId("Assets.red-marker.png");
    public static readonly int Green = typeof(Marker).LoadBitmapId("Assets.green-marker.png");
    public static readonly int Blue = typeof(Marker).LoadBitmapId("Assets.blue-marker.png");

    public static SymbolStyle Create(int markerId)
    {
        return new SymbolStyle
        {
            BitmapId = markerId,
            SymbolScale = 0.6,
            SymbolOffset = new RelativeOffset(0.0, 0.5)
        };
    }
}

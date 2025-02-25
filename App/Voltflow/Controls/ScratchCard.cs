using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia;
using System.Globalization;
using System.Diagnostics;

namespace Voltflow.Controls
{
    public class ScratchCard : Control
    {
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            Debug.WriteLine(e.GetPosition(this));   
        }

        public override void Render(DrawingContext context)
        {
            FormattedText X = new FormattedText("X", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 24, Brushes.Red);
            FormattedText O = new FormattedText("O", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 24, Brushes.Red);

            context.DrawText(X, new Point(0, 0));
            context.DrawText(O, new Point(0, 50));
        }
    }
}

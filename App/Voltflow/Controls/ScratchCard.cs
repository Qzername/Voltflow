using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia;
using System.Globalization;
using System;
using System.Collections.Generic;
using Avalonia.Data;

namespace Voltflow.Controls
{
    public class ScratchCard : Control
    {
        public static readonly AvaloniaProperty<bool> IsWonProperty = AvaloniaProperty.Register<ScratchCard, bool>(nameof(IsWon));
        public static readonly AvaloniaProperty<bool> IsDoneProperty = AvaloniaProperty.Register<ScratchCard, bool>(nameof(IsDone), defaultBindingMode: BindingMode.TwoWay);

        public bool IsWon
        {
            get => (bool)GetValue(IsWonProperty)!;
            set
            {
                SetValue(IsWonProperty, value);
                GenerateGrid();
            }
        }

        public bool IsDone
        {
            get => (bool)GetValue(IsDoneProperty)!;
            set => SetValue(IsDoneProperty, value);
        }

        bool[,] showGrid;
        bool[,] xGrid;

        public ScratchCard()
        {
            showGrid = new bool[3, 3];
            xGrid = new bool[3, 3];

            PropertyChanged += ScratchCard_PropertyChanged; 
            GenerateGrid();
        }

        private void ScratchCard_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == IsWonProperty)
                IsWon = (bool)e.NewValue!;
        }

        public override void Render(DrawingContext context)
        {
            FormattedText X = new FormattedText("X", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 24, Brushes.Red);

            bool cardCompleted = true;

            for (int i = 0; i < 9; i++)
            {
                var gridX = i % 3;
                var gridY = i/3;
                var pos = new Point(gridX * 50, gridY * 50);

                if (xGrid[gridX,gridY])
                    context.DrawText(X, pos);

                if (!showGrid[gridX,gridY])
                {
                    cardCompleted = false;
                    context.DrawRectangle(Brushes.Red, null, new Rect(pos, new Size(50, 50)));
                }
            }

            if(cardCompleted)
                IsDone = true;
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            var pos = e.GetPosition(this);

            var gridX = Convert.ToInt32(Math.Floor(pos.X / 50));
            var gridY = Convert.ToInt32(Math.Floor(pos.Y / 50));

            showGrid[gridX,gridY] = true;

            InvalidateVisual();
        }

        void GenerateGrid()
        {
            xGrid = new bool[3, 3];

            if (IsWon)
                GenerateWon();
            else
                GenerateLost();

            InvalidateVisual();
        }

        void GenerateWon()
        {
            var rng = new Random();

            var mode = rng.Next(0, 3);

            switch(mode)
            {
                //diagonally
                case 0:
                    var direction = rng.Next(0, 2);

                    if (direction == 0)
                    {
                        xGrid[0, 0] = true;
                        xGrid[1, 1] = true;
                        xGrid[2, 2] = true;
                    }
                    else
                    {
                        xGrid[2, 0] = true;
                        xGrid[1, 1] = true;
                        xGrid[0, 2] = true;
                    }
                    break;
                //column
                case 1: 
                    var column = rng.Next(0, 3);

                    xGrid[column,0] = true;
                    xGrid[column,1] = true;
                    xGrid[column,2] = true;
                    break;
                //row
                case 2:
                    var row = rng.Next(0, 3);

                    xGrid[0, row] = true;
                    xGrid[1, row] = true;
                    xGrid[2, row] = true;
                    break;
            }
        }

        void GenerateLost()
        {
            var rng = new Random();

            var firstRow = rng.Next(0, 3);
            var secondRow = rng.Next(0, 3);

            var thirdRowPossibilities = new List<int>() { 0, 1, 2 };

            if (firstRow == secondRow)
                thirdRowPossibilities.Remove(firstRow);
            else if (firstRow != 1 && secondRow == 1)
            {
                thirdRowPossibilities.Remove(0);
                thirdRowPossibilities.Remove(2);
            }

            var thirdRowId = rng.Next(0, thirdRowPossibilities.Count);
            var thirdRow = thirdRowPossibilities[0];

            xGrid[firstRow, 0] = true;
            xGrid[secondRow, 1] = true;
            xGrid[thirdRow, 2] = true;
        }
    }
}

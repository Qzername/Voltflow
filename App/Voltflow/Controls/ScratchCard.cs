using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;

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

        private readonly bool[,] _showGrid;
        private bool[,] _xGrid;

        public ScratchCard()
        {
            _showGrid = new bool[3, 3];
            _xGrid = new bool[3, 3];

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

            for (int i = 0; i < 9; i++)
            {
                var gridX = i % 3;
                var gridY = i / 3;
                var pos = new Point(gridX * 50, gridY * 50);

                if (_xGrid[gridX, gridY])
                    context.DrawText(X, pos);

                if (!_showGrid[gridX, gridY])
                    context.DrawRectangle(Brushes.Red, null, new Rect(pos, new Size(50, 50)));
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            var pos = e.GetPosition(this);

            var gridX = Convert.ToInt32(Math.Floor(pos.X / 50));
            var gridY = Convert.ToInt32(Math.Floor(pos.Y / 50));

            _showGrid[gridX, gridY] = true;

            bool cardCompleted = true;

            for (int i = 0; i < 9; i++)
            {
                gridX = i % 3;
                gridY = i / 3;

                if (!_showGrid[gridX, gridY])
                {
                    cardCompleted = false;
                }
            }

            if (cardCompleted)
                IsDone = true;

            InvalidateVisual();
        }

        void GenerateGrid()
        {
            _xGrid = new bool[3, 3];

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

            switch (mode)
            {
                //diagonally
                case 0:
                    var direction = rng.Next(0, 2);

                    if (direction == 0)
                    {
                        _xGrid[0, 0] = true;
                        _xGrid[1, 1] = true;
                        _xGrid[2, 2] = true;
                    }
                    else
                    {
                        _xGrid[2, 0] = true;
                        _xGrid[1, 1] = true;
                        _xGrid[0, 2] = true;
                    }
                    break;
                //column
                case 1:
                    var column = rng.Next(0, 3);

                    _xGrid[column, 0] = true;
                    _xGrid[column, 1] = true;
                    _xGrid[column, 2] = true;

                    break;
                //row
                case 2:
                    var row = rng.Next(0, 3);

                    _xGrid[0, row] = true;
                    _xGrid[1, row] = true;
                    _xGrid[2, row] = true;

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

            _xGrid[firstRow, 0] = true;
            _xGrid[secondRow, 1] = true;
            _xGrid[thirdRow, 2] = true;
        }
    }
}

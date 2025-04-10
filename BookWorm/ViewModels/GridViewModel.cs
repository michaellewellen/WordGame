using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BookWorm.ViewModels
{
    public class GridViewModel
    {
        public ObservableCollection<TileViewModel> Tiles { get; } = new();
        public int ColumnCount => 7;
        private TileViewModel?[][] _tileMap = new TileViewModel[7][];
        public int[] ColumnHeights => new int[] { 7, 8, 7, 8, 7, 8, 7 };
        private readonly Random _random = new();

        public GridViewModel()
        {
            InitializeGrid();
            LinkNeighbors();
        }

        private void InitializeGrid()
        {
            

            for (int col = 0; col < ColumnCount; col++)
            {
                _tileMap[col] = new TileViewModel[ColumnHeights[col]];
                for (int row = 0; row < ColumnHeights[col]; row++)
                {
                    char letter = GenerateRandomLetter();
                    var tile = new TileViewModel(col, row, letter);
                    _tileMap[col][row] = tile;
                    Tiles.Add(tile);
                }
            }
        }

        public void SetTileAt(int col, int row, TileViewModel tile)
        {
            _tileMap[col][row] = tile;
        }
        public void LinkNeighbors()
        {
            for (int col = 0;col < ColumnCount; col++)
            {
                for (int row = 0; row < _tileMap[col].Length; row++)
                {
                    var tile = _tileMap[col][row];
                    if (tile == null) continue;

                    // Vertical neighbors
                    TryAddNeighbor(tile, col, row - 1); // up
                    TryAddNeighbor(tile, col, row + 1); // down

                    bool isEven = col % 2 == 0;

                    if (isEven)
                    {
                        TryAddNeighbor(tile, col - 1, row);     // UL
                        TryAddNeighbor(tile, col - 1, row + 1); // LL
                        TryAddNeighbor(tile, col + 1, row);     // UR
                        TryAddNeighbor(tile, col + 1, row + 1); // LR
                    }
                    else
                    {
                        TryAddNeighbor(tile, col - 1, row - 1); // UL
                        TryAddNeighbor(tile, col - 1, row);     // LL
                        TryAddNeighbor(tile, col + 1, row - 1); // UR
                        TryAddNeighbor(tile, col + 1, row);     // LR
                    }
                }
            }
        }

        private void TryAddNeighbor(TileViewModel tile, int col, int row)
        {
            if (col >= 0 && col < ColumnCount &&
                row >= 0 && row < _tileMap[col].Length &&
                _tileMap[col][row] != null)
            {
                tile.Neighbors.Add(_tileMap[col][row]!);
            }
        }

        public char GenerateRandomLetter()
        {
            const string frequencyString = "eeeeeeeeeeeeeeaaaaaaaaiiiiiiooooooonnnnnnrrrrrrttttttlllllllssssssddduuummmhhhccfffppwwbbggyykvvxjqz";
            char result =  frequencyString[_random.Next(frequencyString.Length)];
            Debug.WriteLine($"Generated letter: {result}");
            return result;
        }
       
    }
}

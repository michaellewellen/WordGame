using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace WordSearchEngine
{
    public class WordSearchGenerator
    {
        private const int MaxGridSize = 20;
        private readonly Random _rand = new();

        public int OffsetX { get; private set; }
        public int OffsetY { get; private set; }

        public GridCell[,] Grid { get; private set; } = new GridCell[MaxGridSize, MaxGridSize];
        public List<WordEntry> PlacedWords { get; private set; } = new();

        private List<WordEntry> _wordList = new();

        private int minRow = MaxGridSize, maxRow = 0;
        private int minCol = MaxGridSize, maxCol = 0;

        public WordSearchGenerator() 
        {
            for (int r = 0; r < MaxGridSize; r++)
            {
                for (int c = 0; c < MaxGridSize; c++)
                {
                    Grid[r, c] = new GridCell { X = c, Y = r };
                }
            }
        }

        public void Generate(List<string> phrases)
        {
            _wordList = phrases
                .Select(p => new WordEntry(p, p.Replace(" ", "").ToUpper()))
                .OrderByDescending(w => w.GridText.Length)
                .ToList();

            foreach (var word in _wordList)
            {
                TryPlaceWord(word);
            }

            FillUnusedCells();
        }

        private void TryPlaceWord(WordEntry word)
        {
            var directions = Enum.GetValues(typeof(Direction)).Cast<Direction>().OrderBy(_ => _rand.Next()).ToList();

            // Try to anchor to an existing letter
            var anchorPoints = new List<(int x, int y, char letter)>();
            foreach (var placed in PlacedWords)
            {
                for (int i = 0; i < placed.GridText.Length; i++)
                {
                    char letter = placed.GridText[i];
                    for (int r = 0; r < MaxGridSize; r++)
                    {
                        for (int c = 0; c < MaxGridSize; c++)
                        {
                            if (Grid[r, c].Letter == letter)
                            {
                                anchorPoints.Add((c, r, letter));
                            }
                        }
                    }
                }
            }
            if (!anchorPoints.Any())
            {
                int centerX = MaxGridSize / 2;
                int centerY = MaxGridSize / 2;
                foreach (var dir in directions)
                {
                    if (TryPlaceAt(word,centerX,centerY,dir))
                        return;
                }
            }
            else
            {
                foreach (var anchor in anchorPoints.OrderBy(_ => _rand.Next()))
                {
                    for (int i = 0; i < word.GridText.Length; i++)
                    {
                        if (word.GridText[i] != anchor.letter) continue;

                        foreach (var dir in directions)
                        {
                            int startX = anchor.x - i * DeltaX(dir);
                            int startY = anchor.y - i * DeltaY(dir);
                            if (TryPlaceAt(word, startX, startY, dir)) return;
                        }
                    }
                }
            }

            for (int r = 0; r < MaxGridSize; r++)
            {
                for (int c = 0; c < MaxGridSize; c++)
                {
                    foreach (var dir in directions)
                    {
                        if (TryPlaceAt(word, r, c, dir)) return;
                    }
                }
            }
        }

        private bool TryPlaceAt(WordEntry word, int startX, int startY, Direction dir)
        {
            int dx = DeltaX(dir);
            int dy = DeltaY(dir);

            int x = startX;
            int y = startY;

            for (int i = 0; i < word.GridText.Length; i++)
            {
                if (x < 0 || x >= MaxGridSize || y < 0 || y >= MaxGridSize)
                    return false;

                var cell = Grid[y, x];
                if (cell.Letter !=' ' && cell.Letter != word.GridText[i])
                    return false;

                x += dx;
                y += dy;
            }

            x = startX;
            y = startY;
            for (int i = 0; i< word.GridText.Length; i++)
            {
                var cell = Grid[y, x];
                cell.Letter = word.GridText[i];
                cell.IsPartOfWord = true;

                minRow = Math.Min(minRow, y);
                maxRow = Math.Max(maxRow, y);
                minCol = Math.Min(minCol, x);
                maxCol = Math.Max(maxCol, x);

                x += dx;
                y += dy;
            }

            word.StartX = startX;
            word.StartY = startY;
            word.EndX = x - dx;
            word.EndY = y - dy;
            word.Direction = dir;

            PlacedWords.Add(word);
            return true;
        }

        private int DeltaX(Direction dir) => dir switch
        {
            Direction.Horizontal => 1,
            Direction.HoriontalBack => -1,
            Direction.Vertical => 0,
            Direction.VerticalUp => 0,
            Direction.DiagonalDownRight => 1,
            Direction.DiagonalUpLeft => -1,
            Direction.DiagonalUpRight => 1,
            Direction.DiagonalDownLeft => -1,
            _ => 0
        };

        private int DeltaY(Direction dir) => dir switch
        {
            Direction.Horizontal => 0,
            Direction.HoriontalBack => 0,
            Direction.Vertical => 1,
            Direction.VerticalUp => -1,
            Direction.DiagonalDownRight => 1,
            Direction.DiagonalUpLeft => -1,
            Direction.DiagonalUpRight => -1,
            Direction.DiagonalDownLeft => 1,
            _ => 0
        };

        private void FillUnusedCells()
        {
            var usedLetters = new HashSet<char>(PlacedWords.SelectMany(w => w.GridText));
            var letterList = usedLetters.ToList();

            for (int r = 0; r < MaxGridSize; r++)
            {
                for(int c = 0; c < MaxGridSize; c++)
                {
                    var cell = Grid[r, c];
                    if (cell.Letter == ' ')
                    {
                        cell.Letter = letterList[_rand.Next(letterList.Count)];
                    }
                }
            }
        }
        
        public GridCell[,] GetCroppedGrid()
        {
            int height = maxRow - minRow + 1;
            int width = maxCol - minCol + 1;
            var cropped = new GridCell[height, width];

            OffsetX = minCol;
            OffsetY = minRow;

            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    cropped[r, c] = Grid[minRow + r, minCol + c];
                }
            }
            return cropped;
        }

        public void PrintToConsole()
        {
            var cropped = GetCroppedGrid();
            int rows = cropped.GetLength(0);
            int cols = cropped.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Console.Write(cropped[r, c].Letter + " ");
                }
                Console.WriteLine();
            }

            Console.WriteLine("\nWords:");
            foreach (var word in PlacedWords)
            {
                Console.WriteLine($" - {word.DisplayText} ({word.StartX},{word.StartY}) - ({word.EndX},{word.EndY})");
            }

        }
    }
}

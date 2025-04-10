using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WordSearchEngine;
using System.Runtime.Intrinsics.Arm;
using Microsoft.VisualBasic;

namespace WordSearchEngine
{
    public class WordDisplayItem
    {
        public string Text { get; set; }    
        public bool Found { get; set; }
    }
    public partial class PuzzleWindow : Window
    {
        
        private GridCell[,] _grid;
        private List<WordEntry> _placedWords;

        private Point? _firstClick = null;
        private readonly List<WordDisplayItem> _wordItems = new();
        private readonly string _topic;
        public PuzzleWindow(List<string> words, string topic)
        {
            InitializeComponent();
            _topic = topic;
            foreach (var word in words.OrderBy(w => w))
            {
                _wordItems.Add(new WordDisplayItem {  Text = word, Found = false});
                DataContext = this;
                Loaded += PuzzleWindow_Loaded;
                            }
        }
        private void PuzzleWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            SizeToContent = SizeToContent.Manual;
            Width = 1000;
            Height = 800;
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Normal;
            ResizeMode = ResizeMode.NoResize;
            GeneratePuzzle();
        }
       

        public List<WordDisplayItem> WordItems => _wordItems;
        public string TopicTitle => _topic;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void GeneratePuzzle()
        {
            var cleanWords = _wordItems
                .Select(w => w.Text)
                .Where(word => word.All(char.IsLetter))
                .ToList();

            HighlightCanvas.Children.Clear();

            var generator = new WordSearchGenerator();
            generator.Generate(cleanWords);

            var placed = generator.PlacedWords.Select(p => p.DisplayText.ToLower()).ToHashSet();
            var unplaced = cleanWords.Where(w => !placed.Contains(w.ToLower()));

            _grid = generator.GetCroppedGrid();
            _placedWords = generator.PlacedWords;

            int offsetX = generator.OffsetX;
            int offsetY = generator.OffsetY;

            foreach (var word in _placedWords)
            {
                word.StartX -= offsetX;
                word.StartY -= offsetY;
                word.EndX -= offsetX;
                word.EndY -= offsetY;
            }

            int rows = _grid.GetLength(0);
            int cols = _grid.GetLength(1);

            PuzzleGrid.Rows = rows;
            PuzzleGrid.Columns = cols;
            PuzzleGrid.Children.Clear();

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var cell = _grid[r,c];
                    var border = new Border
                    {
                        BorderBrush = Brushes.Transparent,
                        BorderThickness = new Thickness(1),
                        Child = new TextBlock
                        {
                            Text = cell.Letter.ToString(),
                            FontSize = 20,
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.White,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            TextAlignment = TextAlignment.Center
                        }
                    };
                    border.MouseLeftButtonDown += Cell_Click;
                    PuzzleGrid.Children.Add(border);
                }
            }
            var placedWordSet = _placedWords.Select(w => w.DisplayText.ToLower()).ToHashSet();
            WordListDisplay.ItemsSource = _wordItems
                .Where(w => placedWordSet.Contains(w.Text.ToLower()))
                .OrderBy(w => w.Text)
                .ToList();
            PuzzleTItle.Text = _topic;
        }
        private void Cell_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            int index = PuzzleGrid.Children.IndexOf(border);
            int row = index / PuzzleGrid.Columns;
            int col = index % PuzzleGrid.Columns;
            
            if (_firstClick == null)
            {
                _firstClick = new Point(col, row);
                border.BorderBrush = Brushes.White;
                border.BorderThickness = new Thickness(3);
            }
            else
            {
                var second = new Point(col, row);
                var first = _firstClick.Value;

                _firstClick = null;
                ClearSelections();

                foreach (var word in _placedWords)
                {
                    if ((word.StartX == (int)first.X && word.StartY == (int) first.Y &&
                        word.EndX == (int)second.X && word.EndY == (int)second.Y)  ||
                        (word.EndX == (int)first.X && word.EndY == (int)first.Y &&
                        word.StartX == (int)second.X && word.StartY == (int)second.Y))
                    {
                        HighlightWord(word);
                        var match = _wordItems.FirstOrDefault(w => w.Text.Equals(word.DisplayText, StringComparison.OrdinalIgnoreCase));
                        if (match != null)
                        {
                            match.Found = true;
                        }
                        WordListDisplay.Items.Refresh();
                        break;
                    }
                }
            }
        }

        private void ClearSelections()
        {
            foreach (var child in PuzzleGrid.Children)
            {
                if (child is Border border)
                {
                    border.BorderBrush = Brushes.Transparent;
                    border.BorderThickness = new Thickness(1);
                }
            }
        }
        private void HighlightWord(WordEntry word)
        {
            int rows = _grid.GetLength(0);
            int cols = _grid.GetLength(1);

            int index1 = word.StartY * cols + word.StartX;
            int index2 = word.EndY * cols + word.EndX;

            if (index1 >= PuzzleGrid.Children.Count || index2 >= PuzzleGrid.Children.Count)
                return;

            var element1 = PuzzleGrid.Children[index1] as FrameworkElement;
            var element2 = PuzzleGrid.Children[index2] as FrameworkElement;

            Point p1 = element1.TransformToVisual(HighlightCanvas).Transform(new Point(element1.ActualWidth/2, element1.ActualHeight/2));
            Point p2 = element2.TransformToVisual(HighlightCanvas).Transform(new Point(element2.ActualWidth/2, element2.ActualHeight/2));
           
            double centerX = (p1.X + p2.X) / 2;
            double centerY = (p1.Y + p2.Y) / 2;

            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            

            double angle = Math.Atan2(dy,dx) * (180 / Math.PI);

            double cellHeight = element1.ActualHeight;
            double height = cellHeight * .7;

            var (fillBrush, strokeBrush) = PastelPalette.GetRandomBrushPair();
            
            var rect = new System.Windows.Shapes.Rectangle
            {
                Width = length + element1.ActualHeight,
                Height = height,
                RadiusX = height / 2,
                RadiusY = height / 2,
                Stroke = strokeBrush,
                StrokeThickness = 2,
                Fill = fillBrush
            };

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new RotateTransform(angle, rect.Width / 2, rect.Height / 2));
            transformGroup.Children.Add(new TranslateTransform(centerX - rect.Width/2, centerY - rect.Height / 2));
            rect.RenderTransform = transformGroup;

            HighlightCanvas.Children.Add(rect);
        }

        private void WordText_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock tb)
            {
                if (tb.Foreground is SolidColorBrush scb && scb.IsFrozen)
                    tb.Foreground = scb.Clone();
            }
        }
    }
}

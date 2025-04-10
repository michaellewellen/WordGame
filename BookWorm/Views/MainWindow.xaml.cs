using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using BookWorm.ViewModels;
using System.IO;
using BookWorm.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace BookWorm
{
    public partial class MainWindow : Window
    {
        private readonly GridViewModel _viewModel;
        private readonly List<TileViewModel> _selectedTiles = new();
        private bool _isDragging = false;
        private TileViewModel? _lastHovered = null;
        private const double Radius = 60;
        private readonly double HexHeight = Math.Sqrt(3) * Radius;
        private readonly double HorizontalStep = Radius * 1.5;
        private readonly double VerticalStep = Math.Sqrt(3) * Radius;
        private HashSet<string> _validWords = new();

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _viewModel = new GridViewModel();
            DataContext = _viewModel;
            Loaded += MainWindow_Loaded;
            TileCanvas.MouseLeftButtonUp += OnCanvasMouseUp;
            TileCanvas.MouseLeave += OnCanvasMouseLeave;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadWordList();
            RenderHexTiles();
        }

        private void LoadWordList()
        {
            try
            {
                var path = "Files/words_alpha.txt";
                _validWords = File.ReadAllLines(path)
                                .Select(w => w.Trim().ToLowerInvariant())
                                .Where(w => !string.IsNullOrWhiteSpace(w))
                                .ToHashSet();    
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load word list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void OnCanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _lastHovered = null;
            TileCanvas.ReleaseMouseCapture();
        }

        private void OnCanvasMouseLeave(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            _lastHovered = null;
            TileCanvas.ReleaseMouseCapture();
        }

        private void RenderHexTiles()
        {
            TileCanvas.Children.Clear();

            foreach (var tile in _viewModel.Tiles)
            {
                double centerX = Radius + tile.Column * HorizontalStep;
                double centerY = tile.Row * VerticalStep + HexHeight * 1.5;

                if (tile.Column % 2 != 0)
                {
                    centerY -= VerticalStep / 2;
                }

                Polygon hex = new Polygon
                {
                    Points = GetHexPoints(Radius),
                    Stroke = Brushes.SaddleBrown,
                    StrokeThickness = 3.5,
                    Fill = tile.IsSelected ? Brushes.LawnGreen : Brushes.Peru,
                    RenderTransform = new TranslateTransform(centerX, centerY),
                    Tag = tile,
                    Cursor = Cursors.Hand,
                    IsHitTestVisible = true
                };

                hex.MouseLeftButtonDown += OnTileMouseDown;
                hex.MouseEnter += OnTileMouseEnter;
                hex.MouseLeftButtonUp += OnTileMouseUp;

                TextBlock letter = new TextBlock
                {
                    Text = tile.Letter.ToString().ToUpper(),
                    Tag = tile,
                    FontSize = 36,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    FontFamily = new FontFamily("Helvetica"),
                    TextAlignment = TextAlignment.Center,
                    Width = Radius * 2,
                    TextWrapping = TextWrapping.NoWrap
                };

                letter.MouseLeftButtonDown += (s, e) => OnTileClicked(tile);

                Canvas.SetLeft(letter, centerX - Radius);
                Canvas.SetTop(letter, centerY - letter.FontSize / 2);

                TileCanvas.Children.Add(hex);
                TileCanvas.Children.Add(letter);

                double dotDistance = Radius * 0.7;
                double angleStep = 12 * (Math.PI / 180);
                double baseAngle = 25 * (Math.PI / 180);

                for (int i = 0; i < tile.DotCount; i++)
                {
                    double angle = baseAngle + i * angleStep;

                    double dotX = centerX + Math.Cos(angle) * dotDistance;
                    double dotY = centerY + Math.Sin(angle) * dotDistance;

                    Ellipse dot = new Ellipse
                    {
                        Width = 6,
                        Height = 6,
                        Tag = tile,
                        Fill = Brushes.SaddleBrown
                    };

                    Canvas.SetLeft(dot, dotX - 3);
                    Canvas.SetTop(dot, dotY - 3);
                    TileCanvas.Children.Add(dot);
                }
            }
        }

        private void OnTileMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is TileViewModel tile)
            {
                _isDragging = true;
                _lastHovered = tile;
                OnTileClicked(tile);

                TileCanvas.CaptureMouse();
            }
        }

        private void OnTileMouseEnter(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;
            if (sender is FrameworkElement element && element.Tag is TileViewModel tile)
            {
                if (_selectedTiles.Contains(tile))
                {
                    int index = _selectedTiles.IndexOf(tile);
                    for (int i = _selectedTiles.Count - 1; i > index; i--)
                    {
                        _selectedTiles[i].IsSelected = false;
                        _selectedTiles.RemoveAt(i);
                    }
                }
                else if (_selectedTiles.Count == 0 || _selectedTiles.Last().Neighbors.Contains(tile))
                {
                    tile.IsSelected = true;
                    _selectedTiles.Add(tile);
                }
                else
                {
                    foreach (var t in _selectedTiles)
                        t.IsSelected = false;
                    _selectedTiles.Clear();
                    tile.IsSelected = true;
                    _selectedTiles.Add(tile);
                }
                _lastHovered = tile;
                RenderHexTiles();
                CurrentWordDisplay.Text = string.Join("", _selectedTiles.Select(t => t.Letter));
            }
        }

        private void OnTileMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _lastHovered = null;
            TileCanvas.ReleaseMouseCapture();
        }
        private void OnTileClicked(TileViewModel clicked)
        {
            if (_selectedTiles.Contains(clicked))
            {
                int index = _selectedTiles.IndexOf(clicked);
                for (int i = _selectedTiles.Count - 1; i > index; i--)
                {
                    _selectedTiles[i].IsSelected = false;
                    _selectedTiles.RemoveAt(i);
                }
            }
            else if (_selectedTiles.Count == 0)
            {
                clicked.IsSelected = true;
                _selectedTiles.Add(clicked);
            }
            else
            {
                var last = _selectedTiles.Last();
                if (last.Neighbors.Contains(clicked))
                {
                    clicked.IsSelected = true;
                    _selectedTiles.Add(clicked);
                }
                else
                {
                    foreach (var tile in _selectedTiles)
                        tile.IsSelected = false;

                    _selectedTiles.Clear();
                    clicked.IsSelected = true;
                    _selectedTiles.Add(clicked);
                }

            }
            RenderHexTiles();
            CurrentWordDisplay.Text = string.Join("", _selectedTiles.Select(t => t.Letter));
            Debug.WriteLine($"Clicked: ({clicked.Column},{clicked.Row}) = {clicked.Letter}");
            foreach (var neighbor in clicked.Neighbors)
            {
                Debug.WriteLine($"  Neighbor: ({neighbor.Column},{neighbor.Row}) = {neighbor.Letter}");
            }
        }

        private PointCollection GetHexPoints(double radius)
        {
            double h = Math.Sqrt(3) * radius / 2;
            return new PointCollection
            {
                new Point(-radius,0),
                new Point(-radius/2, h),
                new Point(radius / 2, h),
                new Point(radius,0),
                new Point(radius/2,-h),
                new Point(-radius/2,-h)
            };
        }

        private async void SubmitWord_Click(object sender, RoutedEventArgs e)
        {
            string word = string.Join("", _selectedTiles.Select(t => t.Letter)).ToLowerInvariant();

            if (_validWords.Contains(word))
            {
               

                var affectedColumns = _selectedTiles.Select(t => t.Column).Distinct().ToList();

                await AnimateTileRemoval(_selectedTiles);

                // Remove selected tiles
                foreach (var tile in _selectedTiles)
                    _viewModel.Tiles.Remove(tile);

                await AnimateTileDrops(affectedColumns);
                
                // Remove each afected column 
                foreach (int col in affectedColumns)
                {
                    int columnHeight = _viewModel.ColumnHeights[col];
                    // Remaining tiles in column, ordered top to bottom
                    var remaining = _viewModel.Tiles
                        .Where(t => t.Column == col)
                        .OrderBy(t => t.Row)
                        .ToList();

                    var rebuilt = new List<TileViewModel>();

                    // New tiles at the top
                    for (int i = 0; i < remaining.Count; i++)
                    {
                        var newTile = new TileViewModel(col, columnHeight - remaining.Count + i, remaining[i].Letter);
                        rebuilt.Add(newTile);
                    }

                    int newTilesNeeded = columnHeight - rebuilt.Count;
                    
                    for (int i = 0; i < newTilesNeeded; i++)
                    {
                        char letter = _viewModel.GenerateRandomLetter();
                        var newTile = new TileViewModel(col, i, letter);
                        rebuilt.Insert(i, newTile); 
                    }

                    _viewModel.Tiles.RemoveWhere(t => t.Column == col);
                    foreach(var tile in rebuilt)
                    {
                        _viewModel.Tiles.Add(tile);
                        _viewModel.SetTileAt(tile.Column, tile.Row, tile);
                    }
                }

                _viewModel.LinkNeighbors();

                _selectedTiles.Clear();
                CurrentWordDisplay.Text = "";
            }
            else
            {
                MessageBox.Show($"'{word}' is Not a valid word.");
                foreach (var tile in _selectedTiles)
                    tile.IsSelected = false;
                _selectedTiles.Clear();
                CurrentWordDisplay.Text = "";
            }

            RenderHexTiles();
        }

        private async Task AnimateTileRemoval(List<TileViewModel> tiles)
        {
            foreach (var tile in tiles)
            {
                UIElement? uiElement = TileCanvas.Children
                    .OfType<FrameworkElement>()
                    .FirstOrDefault(e => e.Tag == tile);

                if (uiElement != null)
                {
                    var scale = new ScaleTransform(1, 1);
                    uiElement.RenderTransformOrigin = new Point(0.5, 0.5);
                    uiElement.RenderTransform = scale;

                    var shrink = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.1)))
                    {
                        FillBehavior = FillBehavior.Stop
                    };

                    scale.BeginAnimation(ScaleTransform.ScaleXProperty, shrink);
                    scale.BeginAnimation(ScaleTransform.ScaleYProperty, shrink);

                }

                await Task.Delay(100); // 0.15 per tile
            }
        }

        private async Task AnimateTileDrops(List<int> affectedColumns)
        {
            await Task.Delay(50); // slight pause after popping

            double dropDistance = 80;
            double duration = 100;

            foreach (int col in affectedColumns)
            {
                var columnTiles = _viewModel.Tiles
                    .Where(t => t.Column == col)
                    .OrderBy(t => t.Row)
                    .ToList();

                foreach (var tile in columnTiles)
                {
                    var elements = TileCanvas.Children
                        .OfType<FrameworkElement>()
                        .Where(e => e.Tag == tile)
                        .ToList();

                    foreach (var element in elements)
                    {
                        var tt = new TranslateTransform();
                        element.RenderTransform = tt;

                        var anim = new DoubleAnimation
                        {
                            From = -dropDistance,
                            To = 0,
                            Duration = TimeSpan.FromMilliseconds(duration),
                            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                        };

                        tt.BeginAnimation(TranslateTransform.YProperty, anim);
                        Debug.WriteLine($"Dropping element for tile ({tile.Column},{tile.Row})");
                    }

                    await Task.Delay(50); // Stagger animation
                }
            }
        }
    }
}

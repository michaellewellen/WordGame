using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wordle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GameViewModel _viewModel;
        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new GameViewModel();
            this.DataContext = _viewModel;

            currentGuess = new string[5] { "", "", "", "", "" };
            GenerateKeyboard();
           
        }

        private string[] currentGuess;
        private int currentIndex = 0;
        private void GenerateKeyboard()
        {
            string row1 = "QWERTYUIOP";
            string row2 = "ASDFGHJKL";
            string row3 = "ZXCVBNM";

            foreach (char c in row1)
                KeyboardRow1.Children.Add(CreateKeyButton(c.ToString()));
            foreach (char c in row2)
                KeyboardRow2.Children.Add(CreateKeyButton(c.ToString()));
            KeyboardRow3.Children.Add(CreateKeyButton("Enter", isSpecial: true));
            foreach (char c in row3)
                KeyboardRow3.Children.Add(CreateKeyButton(c.ToString()));
            KeyboardRow3.Children.Add(CreateKeyButton("←", isSpecial: true));
        }

        private Button CreateKeyButton(string key, bool isSpecial = false)
        {
            var btn = new Button
            {
                Content = key,
                Width = isSpecial ? 60 : 40,
                Height = 50,
                Margin = new Thickness(3),
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Tag = key,
                Background = Brushes.DimGray,
                Foreground = Brushes.White
            };
            btn.Click += KeyButton_Click;
            return btn;
            ;
        }

        private void KeyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string key)
            {
                if (key == "Enter")
                    _viewModel.SubmitGuess();
                else if (key == "←")
                    _viewModel.Backspace();
                else
                    _viewModel.AddLetter(key);
            }
        }        
    }
}
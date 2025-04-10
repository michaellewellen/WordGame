using System.Diagnostics;
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
using Word_Search;

namespace WordSearchEngine
{
    
    public partial class MainWindow : Window
    {
        private readonly WordListFetcher _fetcher = new();
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void GenerateWords_Click(object sender, RoutedEventArgs e)
        {
            var topic = TopicInput.Text.Trim();
            if (string.IsNullOrEmpty(topic))
                return;

            WordListDisplay.Items.Clear();
            var words = await _fetcher.GetPhrasesForTopicAsync(topic);

            foreach (var word in words)
                WordListDisplay.Items.Add(word);
        }

        private void OnMakePuzzle_Click(object sender, RoutedEventArgs e)
        {
            string topic = TopicInput.Text.Trim();
            var words = WordListDisplay.Items.Cast<string>().ToList();
            Debug.WriteLine("Preparing to launch PuzzleWindow with words:");
            foreach (var word in words)
                Debug.WriteLine(word);
            var puzzleWindow = new PuzzleWindow(words, topic);
            puzzleWindow.Show();
            this.Close();
        }
    }
}
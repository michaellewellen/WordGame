using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Wordle
{
    public class GameViewModel:INotifyPropertyChanged
    {
        public ObservableCollection<string> CurrentGuess { get; set; } = new ObservableCollection<string>(new string[5]);
        public ObservableCollection<ObservableCollection<Tile>> GuessRows { get; set; } = new();

        private WordListService _wordService;
        private string _secretWord;
        private int _currentRow = 0;
        
        public ObservableCollection<Tile> Tiles { get; set; } = new();
        // Constructor
        public GameViewModel()
        {
            _wordService = new WordListService();
            _secretWord = _wordService.GetRandomUnusedAnswerWord();
            for (int i = 0; i<30; i++)
            {
                Tiles.Add(new Tile());
            }

            CurrentGuess = new ObservableCollection<string>(new string[5]);
        }

        public void AddLetter(string letter)
        {
            for (int i = 0; i<5; i++)
            {
                if (string.IsNullOrEmpty(CurrentGuess[i]))
                {
                    CurrentGuess[i] = letter;
                    OnPropertyChanged(nameof(CurrentGuess));
                    break;
                }
            }
        }

        public void Backspace()
        {
            for (int i = 4; i >=0; i--)
            {
                if (!string.IsNullOrEmpty(CurrentGuess[i]))
                {
                    CurrentGuess[i] = "";
                    OnPropertyChanged(nameof(CurrentGuess));
                    break;
                }
            }
        }

        public void SubmitGuess()
        {
            var guess = string.Join("", CurrentGuess).ToLower();
            if(guess.Length == 5 && _wordService.IsValidWord(guess))
            {
                ColorGuess(guess);
                CurrentGuess = new ObservableCollection<string>(new string[5]);
                OnPropertyChanged(nameof(CurrentGuess));
                _currentRow++;
            }
            else
            {
                // Show invalid guess alert
            }
        }

        private void ColorGuess(string guess)
        {
            for (int i = 0; i < 5; i++)
            {
                var tile = Tiles[_currentRow * 5 + i];
                tile.Letter = guess[i].ToString().ToUpper();

                if (guess[i] == _secretWord[i])
                    tile.Color = "#6AAA64";
                else if (_secretWord.Contains(guess[i]))
                    tile.Color = "#C9B458";
                else
                    tile.Color = "#787C7E";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

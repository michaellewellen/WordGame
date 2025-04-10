using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BookWorm.ViewModels
{
    public class TileViewModel
    {
        private char _letter;
        private bool _isSelected;
        
        public int Column { get; }
        private int _row;
        public int DotCount { get; }
        public List<TileViewModel> Neighbors { get; } = new();

        public int Row
        {
            get => _row;
            set
            {
                if ( (_row != value))
                {
                    {
                        _row = value;
                        OnPropertyChanged();
                    }
                }
            }
        }
       
        public char Letter
        {
            get => _letter;
            set
            {
                if(_letter != value)
                {
                    _letter = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public TileViewModel(int column, int row, char letter)
        {
            Column = column;
            Row = row;
            Letter = letter;
            DotCount = CalculateDotCount(letter);
        }

        public int CalculateDotCount(char c)
        {
            c = char.ToUpper(c);
            return c switch
            {
                'A' or 'E' or 'I' or 'O' or 'U' => 0,
                'N' or 'R' or 'T' or 'L' or 'S' or 'D' or 'M' or 'H' or 'C' => 1,
                'B' or 'G' or 'F' or 'P' or 'K' or 'W' or 'Y' => 2,
                'J' or 'Q' or 'X' or 'Z' or 'V' => 3,
                _ => 1
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

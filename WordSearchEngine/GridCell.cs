using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordSearchEngine
{
    public class GridCell
    {
        public char Letter { get; set; } = ' ';
        public bool IsPartOfWord { get; set; } = false;
        public int X { get; set; }  
        public int Y { get; set; }  
    }
}

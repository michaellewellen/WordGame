using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace WordSearchEngine
{
    public record WordEntry(string DisplayText, string GridText)
    {
        public bool IsFound { get; set; }
        public int StartX {  get; set; }
        public int StartY { get; set; }
        public int EndX { get; set; }
        public int EndY { get; set; }
        public Direction Direction { get; set; }
    }
           
    
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWorm
{
    public static class ObservableCollectionExtensions
    {      
        public static void RemoveWhere<T>(this ObservableCollection<T> collection, Func<T, bool> predicate)
        {
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (predicate(collection[i]))
                    collection.RemoveAt(i);
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace TKLua
{
    [Serializable]
    public class CSLList<T> : List<T>
    {
        public CSLList() : base()
        {
            
        }
        public CSLList(IEnumerable<T> collection) : base(collection)
        {
            
        }
    }
}
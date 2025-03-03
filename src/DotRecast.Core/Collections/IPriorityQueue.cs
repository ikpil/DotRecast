using System;
using System.Collections.Generic;
using System.Text;

namespace DotRecast.Core.Collections
{
    public interface IPriorityQueue<TItem>
    {
        public void Push(TItem item);
        public TItem Pop();
        public TItem Peek();
        public bool Modify(TItem item);
        public void Clear();
        public int Count();
        public bool IsEmpty();
    }
}

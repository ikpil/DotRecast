/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023 Choi Ikpil ikpil@naver.com

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

using System;

namespace DotRecast.Core;

using System.Collections.Generic;

public class OrderedQueue<T>
{

    private readonly List<T> _items;
    private readonly Comparison<T> _comparison;

    public OrderedQueue(Comparison<T> comparison)
    {
        _items = new();
        _comparison = comparison;
    }

    public int count()
    {
        return _items.Count;
    }

    public void clear() {
        _items.Clear();
    }

    public T top()
    {
        return _items[0];
    }

    public T Dequeue()
    {
        var node = top();
        _items.Remove(node);
        return node;
    }

    public void Enqueue(T item) {
        _items.Add(item);
        _items.Sort(_comparison);
    }

    public void Remove(T item) {
        _items.Remove(item);
    }

    public bool isEmpty()
    {
        return 0 == _items.Count;
    }
}

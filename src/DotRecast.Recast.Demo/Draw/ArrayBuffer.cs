using System;
using DotRecast.Core;

namespace DotRecast.Recast.Demo.Draw;

public class ArrayBuffer<T>
{
    private int _size;
    private T[] _items;
    public int Count => _size;

    public ArrayBuffer() : this(512) { }

    public ArrayBuffer(int capcatiy)
    {
        if (capcatiy <= 0)
            throw new ArgumentOutOfRangeException();

        _size = 0;
        _items = new T[capcatiy];
    }

    public void Add(T item)
    {
        if (_items.Length <= _size)
        {
            var temp = new T[(int)(_size * 1.5)];
            RcArrays.Copy(_items, 0, temp, 0, _items.Length);
            _items = temp;
        }

        _items[_size++] = item;
    }

    public void Clear()
    {
        _size = 0;
    }

    public Span<T> AsArray()
    {
        return _items.AsSpan(0, _size);
    }
}
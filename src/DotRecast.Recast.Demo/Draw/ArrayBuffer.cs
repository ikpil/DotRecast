using System;

namespace DotRecast.Recast.Demo.Draw;

public class ArrayBuffer<T>
{
    private int _size;
    private T[] _items;
    public int Count => _size;

    public ArrayBuffer()
    {
        _size = 0;
        _items = Array.Empty<T>();
    }

    public void Add(T item)
    {
        if (0 >= _items.Length)
        {
            _items = new T[256];
        }

        if (_items.Length <= _size)
        {
            var temp = new T[(int)(_size * 1.5)];
            Array.Copy(_items, 0, temp, 0, _items.Length);
            _items = temp;
        }

        _items[_size++] = item;
    }

    public void Clear()
    {
        _size = 0;
    }

    public T[] AsArray()
    {
        return _items;
    }
}
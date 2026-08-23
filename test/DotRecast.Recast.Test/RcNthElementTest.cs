using System;
using System.Collections.Generic;
using DotRecast.Recast.Geom;
using NUnit.Framework;

namespace DotRecast.Recast.Test;

public class RcNthElementTest
{
    private sealed class Item
    {
        public readonly int Key;
        public readonly string Id;

        public Item(int key, string id)
        {
            Key = key;
            Id = id;
        }
    }

    private sealed class ItemComparer : IComparer<Item>
    {
        public static readonly ItemComparer Shared = new ItemComparer();

        public int Compare(Item x, Item y)
        {
            return x.Key.CompareTo(y.Key);
        }
    }

    [Test]
    public void TestNthElementSimple()
    {
        int[] values = { 5, 3, 1, 4, 2 };

        RcNthElement.NthElement(values, 0, 2, values.Length, Comparer<int>.Default);

        int[] sorted = (int[])values.Clone();
        Array.Sort(sorted);
        Assert.That(values[2], Is.EqualTo(sorted[2]));
        AssertPartition(values, 2, Comparer<int>.Default);
    }

    [Test]
    public void TestNthElementRandom()
    {
        Random random = new Random(12345);
        for (int size = 1; size < 100; size++)
        {
            int[] values = new int[size];
            for (int i = 0; i < size; i++)
            {
                values[i] = random.Next(1000) - 500;
            }

            int[] sorted = (int[])values.Clone();
            Array.Sort(sorted);
            for (int nth = 0; nth < size; nth++)
            {
                int[] copy = (int[])values.Clone();
                RcNthElement.NthElement(copy, 0, nth, copy.Length, Comparer<int>.Default);

                Assert.That(copy[nth], Is.EqualTo(sorted[nth]),
                    $"nth element should match sorted value for size={size} nth={nth}");
                AssertPartition(copy, nth, Comparer<int>.Default);
            }
        }
    }

    [Test]
    public void TestNthElementWithDuplicatesAndComparator()
    {
        Item[] items =
        {
            new Item(2, "a"),
            new Item(1, "b"),
            new Item(2, "c"),
            new Item(3, "d"),
            new Item(1, "e"),
        };
        Item[] expected = (Item[])items.Clone();
        Array.Sort(expected, ItemComparer.Shared);

        RcNthElement.NthElement(items, 0, 2, items.Length, ItemComparer.Shared);

        Assert.That(items[2].Key, Is.EqualTo(expected[2].Key));
        AssertPartition(items, 2, ItemComparer.Shared);
    }

    private static void AssertPartition<T>(T[] values, int nth, IComparer<T> comparer)
    {
        for (int i = 0; i < nth; i++)
        {
            Assert.That(comparer.Compare(values[i], values[nth]), Is.LessThanOrEqualTo(0),
                "element before nth should be <= nth");
        }

        for (int i = nth + 1; i < values.Length; i++)
        {
            Assert.That(comparer.Compare(values[nth], values[i]), Is.LessThanOrEqualTo(0),
                "element after nth should be >= nth");
        }
    }
}

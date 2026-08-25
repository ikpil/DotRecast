using System;
using System.Collections.Generic;
using DotRecast.Core.Collections;
using NUnit.Framework;

namespace DotRecast.Core.Test;

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

    private sealed class CountingComparer : IComparer<int>
    {
        public int Count { get; private set; }

        public int Compare(int x, int y)
        {
            Count++;
            return x.CompareTo(y);
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
        AssertPartition(values, 0, 2, values.Length, Comparer<int>.Default);
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
                AssertPartition(copy, 0, nth, copy.Length, Comparer<int>.Default);
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
        AssertPartition(items, 0, 2, items.Length, ItemComparer.Shared);
    }

    [Test]
    public void TestNthElementWithNonZeroLowIndex()
    {
        int[] values = { 100, 90, 8, 2, 7, 1, 6, 3, 5, 4, -90 };
        int[] expected = (int[])values.Clone();
        const int low = 2;
        const int nth = 6;
        const int high = 10;
        Array.Sort(expected, low, high - low);

        RcNthElement.NthElement(values, low, nth, high, Comparer<int>.Default);

        Assert.That(values[nth], Is.EqualTo(expected[nth]));
        Assert.That(values[0], Is.EqualTo(expected[0]));
        Assert.That(values[1], Is.EqualTo(expected[1]));
        Assert.That(values[high], Is.EqualTo(expected[high]));
        AssertPartition(values, low, nth, high, Comparer<int>.Default);
    }

    [Test]
    public void TestNthElementRejectsInvalidArguments()
    {
        int[] values = { 3, 1, 2 };

        Assert.That(
            Assert.Throws<ArgumentNullException>((Action)(() => RcNthElement.NthElement<int>(null, 0, 0, 1, Comparer<int>.Default)))!.ParamName,
            Is.EqualTo("array"));
        Assert.That(
            Assert.Throws<ArgumentNullException>((Action)(() => RcNthElement.NthElement(values, 0, 0, 1, null)))!.ParamName,
            Is.EqualTo("comparer"));
        Assert.That(
            Assert.Throws<ArgumentOutOfRangeException>((Action)(() => RcNthElement.NthElement(values, -1, 0, 1, Comparer<int>.Default)))!.ParamName,
            Is.EqualTo("low"));
        Assert.That(
            Assert.Throws<ArgumentOutOfRangeException>((Action)(() => RcNthElement.NthElement(values, 0, 0, values.Length + 1, Comparer<int>.Default)))!.ParamName,
            Is.EqualTo("high"));
        Assert.That(
            Assert.Throws<ArgumentOutOfRangeException>((Action)(() => RcNthElement.NthElement(values, 2, 2, 1, Comparer<int>.Default)))!.ParamName,
            Is.EqualTo("high"));
        Assert.That(
            Assert.Throws<ArgumentOutOfRangeException>((Action)(() => RcNthElement.NthElement(values, 1, 0, 2, Comparer<int>.Default)))!.ParamName,
            Is.EqualTo("nth"));
        Assert.That(
            Assert.Throws<ArgumentOutOfRangeException>((Action)(() => RcNthElement.NthElement(values, 0, 2, 2, Comparer<int>.Default)))!.ParamName,
            Is.EqualTo("nth"));
    }

    [Test]
    public void TestNthElementLimitsWorkForMedianOfThreeKillerSequence()
    {
        const int size = 1024;
        int[] values = CreateMedianOfThreeKillerSequence(size);
        CountingComparer comparer = new CountingComparer();

        RcNthElement.NthElement(values, 0, size / 2, values.Length, comparer);

        Assert.That(values[size / 2], Is.EqualTo(size / 2));
        AssertPartition(values, 0, size / 2, values.Length, Comparer<int>.Default);
        Assert.That(comparer.Count, Is.LessThan(size * 64),
            "pathological partitions should fall back before comparison count becomes quadratic");
    }

    private static int[] CreateMedianOfThreeKillerSequence(int size)
    {
        int[] values = new int[size];
        int index = 0;
        int half = size / 2;
        for (int value = 1; value <= half; value += 2)
        {
            values[index++] = value - 1;
            values[index++] = half + value - 1;
        }

        for (int value = 2; value <= size; value += 2)
        {
            values[index++] = value - 1;
        }

        return values;
    }

    private static void AssertPartition<T>(T[] values, int low, int nth, int high, IComparer<T> comparer)
    {
        for (int i = low; i < nth; i++)
        {
            Assert.That(comparer.Compare(values[i], values[nth]), Is.LessThanOrEqualTo(0),
                "element before nth should be <= nth");
        }

        for (int i = nth + 1; i < high; i++)
        {
            Assert.That(comparer.Compare(values[nth], values[i]), Is.LessThanOrEqualTo(0),
                "element after nth should be >= nth");
        }
    }
}

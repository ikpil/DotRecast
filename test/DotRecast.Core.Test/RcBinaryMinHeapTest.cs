using System;
using System.Collections.Generic;
using System.Linq;
using DotRecast.Core.Collections;
using NUnit.Framework;

namespace DotRecast.Core.Test;

public class RcBinaryMinHeapTest
{
    // Reference type on purpose: the heap must track identity (SameAs),
    // exactly like DtNodeQueue tracks DtNode instances.
    private sealed class Node
    {
        public int Value;

        public Node(int value)
        {
            Value = value;
        }
    }

    private static readonly Comparison<Node> ByValue = (x, y) => x.Value.CompareTo(y.Value);

    private static RcBinaryMinHeap<Node> HeapOf(params int[] values)
    {
        var heap = new RcBinaryMinHeap<Node>(ByValue);
        foreach (var value in values)
        {
            heap.Push(new Node(value));
        }

        return heap;
    }

    // The single invariant a binary min-heap guarantees: every parent <= its children.
    private static void AssertHeapProperty(RcBinaryMinHeap<Node> heap)
    {
        var items = heap.ToArray();
        for (int child = 1; child < items.Length; ++child)
        {
            int parent = (child - 1) / 2;
            Assert.That(items[parent].Value, Is.LessThanOrEqualTo(items[child].Value),
                $"parent[{parent}]={items[parent].Value} > child[{child}]={items[child].Value}");
        }
    }

    private static void AssertPopsAllInSortedOrder(RcBinaryMinHeap<Node> heap, IReadOnlyCollection<Node> pushed)
    {
        var expected = pushed.Select(node => node.Value).OrderBy(value => value).ToList();

        var actual = new List<int>(pushed.Count);
        while (!heap.IsEmpty())
        {
            actual.Add(heap.Pop().Value);
        }

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Constructor_NonPositiveCapacity_Throws()
    {
        Assert.Throws<ArgumentException>((TestDelegate)(() => new RcBinaryMinHeap<Node>(0, ByValue)));
        Assert.Throws<ArgumentException>((TestDelegate)(() => new RcBinaryMinHeap<Node>(-1, ByValue)));
    }

    [Test]
    public void Constructor_NullComparison_Throws()
    {
        Assert.Throws<ArgumentNullException>((TestDelegate)(() => new RcBinaryMinHeap<Node>(null)));
        Assert.Throws<ArgumentNullException>((TestDelegate)(() => new RcBinaryMinHeap<Node>(16, null)));
    }

    [Test]
    public void Push_KeepsHeapPropertyAndCount()
    {
        var rand = new Random(12345);
        var heap = new RcBinaryMinHeap<Node>(ByValue);

        for (int i = 0; i < 256; ++i)
        {
            heap.Push(new Node(rand.Next(-1000, 1000)));

            Assert.That(heap.Count, Is.EqualTo(i + 1));
            AssertHeapProperty(heap);
        }
    }

    [Test]
    public void Pop_ReturnsElementsInAscendingOrder()
    {
        var rand = new Random(12345);

        // narrow value range - duplicate values must be handled too
        var pushed = Enumerable.Range(0, 500)
            .Select(_ => new Node(rand.Next(-100, 100)))
            .ToList();

        var heap = new RcBinaryMinHeap<Node>(ByValue);
        foreach (var node in pushed)
        {
            heap.Push(node);
        }

        AssertPopsAllInSortedOrder(heap, pushed);
        Assert.That(heap.IsEmpty(), Is.True);
    }

    [Test]
    public void Pop_EmptyHeap_Throws()
    {
        var heap = new RcBinaryMinHeap<Node>(ByValue);
        Assert.Throws<InvalidOperationException>((TestDelegate)(() => heap.Pop()));
    }

    [Test]
    public void Peek_ReturnsMinWithoutRemoving()
    {
        var heap = HeapOf(5, 3, 7, 2, 4);

        Assert.That(heap.Peek().Value, Is.EqualTo(2));
        Assert.That(heap.Count, Is.EqualTo(5));
    }

    [Test]
    public void Peek_EmptyHeap_Throws()
    {
        var heap = new RcBinaryMinHeap<Node>(ByValue);
        Assert.Throws<InvalidOperationException>((TestDelegate)(() => heap.Peek()));
    }

    [Test]
    public void Modify_DecreasedValue_MovesToTop()
    {
        var heap = new RcBinaryMinHeap<Node>(ByValue);
        var target = new Node(70);

        heap.Push(new Node(10));
        heap.Push(new Node(20));
        heap.Push(target);
        heap.Push(new Node(30));

        target.Value = 5;

        Assert.That(heap.Modify(target), Is.True);
        Assert.That(heap.Peek(), Is.SameAs(target));
        AssertHeapProperty(heap);
    }

    [Test]
    public void Modify_IncreasedValue_MovesDown()
    {
        var heap = new RcBinaryMinHeap<Node>(ByValue);
        var target = new Node(1);

        heap.Push(target);
        heap.Push(new Node(10));
        heap.Push(new Node(20));
        heap.Push(new Node(30));

        target.Value = 100;

        Assert.That(heap.Modify(target), Is.True);
        Assert.That(heap.Peek(), Is.Not.SameAs(target));
        AssertHeapProperty(heap);
    }

    [Test]
    public void Modify_NodeNotInHeap_ReturnsFalse()
    {
        var heap = HeapOf(1, 2, 3);

        Assert.That(heap.Modify(new Node(2)), Is.False);
        Assert.That(heap.Count, Is.EqualTo(3));
    }

    // Contract check: mutate one key, Modify() immediately, repeat.
    // This is the access pattern of an A* open list (DtNodeQueue).
    [Test]
    public void Modify_RepeatedSingleKeyUpdates_StayConsistent()
    {
        var rand = new Random(12345);
        var pushed = Enumerable.Range(0, 100)
            .Select(_ => new Node(rand.Next(-1000, 1000)))
            .ToList();

        var heap = new RcBinaryMinHeap<Node>(ByValue);
        foreach (var node in pushed)
        {
            heap.Push(node);
        }

        for (int i = 0; i < 1000; ++i)
        {
            var node = pushed[rand.Next(pushed.Count)];
            node.Value = rand.Next(-1000, 1000);

            Assert.That(heap.Modify(node), Is.True);
            AssertHeapProperty(heap);
        }

        AssertPopsAllInSortedOrder(heap, pushed);
    }

    [Test]
    public void CountClearIsEmpty_Lifecycle()
    {
        var heap = HeapOf(5, 3, 7);
        Assert.That(heap.Count, Is.EqualTo(3));
        Assert.That(heap.IsEmpty(), Is.False);

        heap.Pop();
        Assert.That(heap.Count, Is.EqualTo(2));

        heap.Clear();
        Assert.That(heap.Count, Is.EqualTo(0));
        Assert.That(heap.IsEmpty(), Is.True);
    }
}

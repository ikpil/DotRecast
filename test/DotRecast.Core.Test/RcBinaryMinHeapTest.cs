using DotRecast.Core.Collections;
using NUnit.Framework;

namespace DotRecast.Core.Test;

public class RcBinaryMinHeapTest
{
    private static readonly RcAtomicLong Gen = new();

    private class Node
    {
        public readonly long Id;
        public long Value;

        public Node(int value)
        {
            Id = Gen.IncrementAndGet();
            Value = value;
        }
    }

    [Test]
    public void TestPush()
    {
        var minHeap = new RcBinaryMinHeap<Node>((x, y) => x.Value.CompareTo(y.Value));

        minHeap.Push(new Node(5));
        minHeap.Push(new Node(3));
        minHeap.Push(new Node(7));
        minHeap.Push(new Node(2));
        minHeap.Push(new Node(4));

        // Push 후 힙의 속성을 검증
        AssertHeapProperty(minHeap.ToArray());
    }

    [Test]
    public void TestPop()
    {
        var minHeap = new RcBinaryMinHeap<Node>((x, y) => x.Value.CompareTo(y.Value));

        minHeap.Push(new Node(5));
        minHeap.Push(new Node(3));
        minHeap.Push(new Node(7));
        minHeap.Push(new Node(2));
        minHeap.Push(new Node(4));

        // Pop을 통해 최소 값부터 순서대로 제거하면서 검증
        Assert.That(minHeap.Pop().Value, Is.EqualTo(2));
        Assert.That(minHeap.Pop().Value, Is.EqualTo(3));
        Assert.That(minHeap.Pop().Value, Is.EqualTo(4));
        Assert.That(minHeap.Pop().Value, Is.EqualTo(5));
        Assert.That(minHeap.Pop().Value, Is.EqualTo(7));

        // 모든 요소를 Pop한 후에는 비어있어야 함
        Assert.That(minHeap.IsEmpty(), Is.True);
    }


    [Test]
    public void TestTop()
    {
        var minHeap = new RcBinaryMinHeap<Node>((x, y) => x.Value.CompareTo(y.Value));

        minHeap.Push(new Node(5));
        minHeap.Push(new Node(3));
        minHeap.Push(new Node(7));

        Assert.That(minHeap.Top().Value, Is.EqualTo(3));
        AssertHeapProperty(minHeap.ToArray());
    }

    [Test]
    public void TestModify()
    {
        var minHeap = new RcBinaryMinHeap<Node>((x, y) => x.Value.CompareTo(y.Value));

        var node7 = new Node(7);
        minHeap.Push(new Node(5));
        minHeap.Push(new Node(3));
        minHeap.Push(node7);
        minHeap.Push(new Node(2));
        minHeap.Push(new Node(4));

        node7.Value = 1;
        var result = minHeap.Modify(node7); // Modify value 7 to 1
        var result2 = minHeap.Modify(new Node(4));

        Assert.That(result, Is.EqualTo(true));
        Assert.That(result2, Is.EqualTo(false));
        Assert.That(minHeap.Top().Value, Is.EqualTo(1));
        AssertHeapProperty(minHeap.ToArray());
    }

    [Test]
    public void TestCount()
    {
        var minHeap = new RcBinaryMinHeap<Node>((x, y) => x.Value.CompareTo(y.Value));

        minHeap.Push(new Node(5));
        minHeap.Push(new Node(3));
        minHeap.Push(new Node(7));

        Assert.That(minHeap.Count, Is.EqualTo(3));

        minHeap.Pop();

        Assert.That(minHeap.Count, Is.EqualTo(2));

        minHeap.Clear();

        Assert.That(minHeap.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestIsEmpty()
    {
        var minHeap = new RcBinaryMinHeap<Node>((x, y) => x.Value.CompareTo(y.Value));

        Assert.That(minHeap.IsEmpty(), Is.True);

        minHeap.Push(new Node(5));

        Assert.That(minHeap.IsEmpty(), Is.False);

        minHeap.Pop();

        Assert.That(minHeap.IsEmpty(), Is.True);
    }

    private void AssertHeapProperty(Node[] array)
    {
        for (int i = 0; i < array.Length / 2; i++)
        {
            int leftChildIndex = 2 * i + 1;
            int rightChildIndex = 2 * i + 2;

            // 왼쪽 자식 노드가 있는지 확인하고 비교
            if (leftChildIndex < array.Length)
                Assert.That(array[i].Value, Is.LessThanOrEqualTo(array[leftChildIndex].Value));

            // 오른쪽 자식 노드가 있는지 확인하고 비교
            if (rightChildIndex < array.Length)
                Assert.That(array[i].Value, Is.LessThanOrEqualTo(array[rightChildIndex].Value));
        }
    }
}
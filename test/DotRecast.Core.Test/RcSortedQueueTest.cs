using System.Collections.Generic;
using DotRecast.Core.Collections;
using DotRecast.Core.Collections.Extensions;
using NUnit.Framework;

namespace DotRecast.Core.Test;

public class RcSortedQueueTest
{
    [Test]
    public void TestEnqueueAndDequeue()
    {
        var sortedQueue = new RcSortedQueue<int>((a, b) => a.CompareTo(b));

        var r = new RcRand();
        var expectedList = new List<int>();
        for (int i = 0; i < 999; ++i)
        {
            expectedList.Add(r.NextInt32() % 300); // allow duplication 
        }

        // ready
        foreach (var expected in expectedList)
        {
            sortedQueue.Enqueue(expected);
        }

        expectedList.Sort();

        // check count
        Assert.That(sortedQueue.Count(), Is.EqualTo(expectedList.Count));
        Assert.That(sortedQueue.IsEmpty(), Is.False);

        Assert.That(sortedQueue.ToList(), Is.EqualTo(expectedList));

        // check Peek and Dequeue
        for (int i = 0; i < expectedList.Count; ++i)
        {
            Assert.That(sortedQueue.Peek(), Is.EqualTo(expectedList[i]));
            Assert.That(sortedQueue.Count(), Is.EqualTo(expectedList.Count - i));

            Assert.That(sortedQueue.Dequeue(), Is.EqualTo(expectedList[i]));
            Assert.That(sortedQueue.Count(), Is.EqualTo(expectedList.Count - i - 1));
        }

        // check count
        Assert.That(sortedQueue.Count(), Is.EqualTo(0));
        Assert.That(sortedQueue.IsEmpty(), Is.True);
    }

    [Test]
    public void TestRemoveForValueType()
    {
        var sortedQueue = new RcSortedQueue<int>((a, b) => a.CompareTo(b));

        var r = new RcRand();
        var expectedList = new List<int>();
        for (int i = 0; i < 999; ++i)
        {
            expectedList.Add(r.NextInt32() % 300); // allow duplication 
        }

        // ready
        foreach (var expected in expectedList)
        {
            sortedQueue.Enqueue(expected);
        }

        expectedList.Shuffle();

        // check
        Assert.That(sortedQueue.Count(), Is.EqualTo(expectedList.Count));

        foreach (var expected in expectedList)
        {
            Assert.That(sortedQueue.Remove(expected), Is.True);
        }

        Assert.That(sortedQueue.IsEmpty(), Is.True);
    }

    [Test]
    public void TestRemoveForReferenceType()
    {
        var sortedQueue = new RcSortedQueue<RcAtomicLong>((a, b) => a.Read().CompareTo(b.Read()));

        var r = new RcRand();
        var expectedList = new List<RcAtomicLong>();
        for (int i = 0; i < 999; ++i)
        {
            expectedList.Add(new RcAtomicLong(r.NextInt32() % 300)); // allow duplication 
        }

        // ready
        foreach (var expected in expectedList)
        {
            sortedQueue.Enqueue(expected);
        }

        expectedList.Shuffle();

        // check
        Assert.That(sortedQueue.Count(), Is.EqualTo(expectedList.Count));

        foreach (var expected in expectedList)
        {
            Assert.That(sortedQueue.Remove(expected), Is.True);
        }

        Assert.That(sortedQueue.IsEmpty(), Is.True);
    }

}
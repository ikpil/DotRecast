using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Core.Collections;
using NUnit.Framework;

namespace DotRecast.Detour.Test;

public class DtNodeQueueTest
{
    private static List<DtNode> ShuffledNodes(int count)
    {
        var nodes = new List<DtNode>();
        for (int i = 0; i < count; ++i)
        {
            var node = new DtNode(i);
            node.total = i;
            nodes.Add(node);
        }

        nodes.Shuffle();
        return nodes;
    }

    [Test]
    public void TestPushAndPop()
    {
        var queue = new DtNodeQueue();

        // check count
        Assert.That(queue.Count(), Is.EqualTo(0));
        
        // null push
        queue.Push(null);
        Assert.That(queue.Count(), Is.EqualTo(0));

        // test push
        const int count = 1000;
        var expectedNodes = ShuffledNodes(count);
        foreach (var node in expectedNodes)
        {
            queue.Push(node);
        }

        Assert.That(queue.Count(), Is.EqualTo(count));

        // test pop
        expectedNodes.Sort(DtNode.ComparisonNodeTotal);
        foreach (var node in expectedNodes)
        {
            Assert.That(queue.Peek(), Is.SameAs(node));
            Assert.That(queue.Pop(), Is.SameAs(node));
        }

        Assert.That(queue.Count(), Is.EqualTo(0));
    }

    [Test]
    public void TestClear()
    {
        var queue = new DtNodeQueue();

        const int count = 555;
        var expectedNodes = ShuffledNodes(count);
        foreach (var node in expectedNodes)
        {
            queue.Push(node);
        }

        Assert.That(queue.Count(), Is.EqualTo(count));

        queue.Clear();
        Assert.That(queue.Count(), Is.EqualTo(0));
        Assert.That(queue.IsEmpty(), Is.True);
    }
    
    [Test]
    public void TestModify()
    {
        var queue = new DtNodeQueue();

        const int count = 5000;
        var expectedNodes = ShuffledNodes(count);
        
        foreach (var node in expectedNodes)
        {
            queue.Push(node);
        }

        // check modify
        queue.Modify(null);

        // change total
        var r = new RcRand();
        foreach (var node in expectedNodes)
        {
            node.total = r.NextInt32() % (count / 50); // duplication for test
        }

        // test modify
        foreach (var node in expectedNodes)
        {
            queue.Modify(node);
        }
        
        // check
        expectedNodes.Sort(DtNode.ComparisonNodeTotal);
        foreach (var node in expectedNodes)
        {
            Assert.That(queue.Pop(), Is.SameAs(node));
        }
    } 
}
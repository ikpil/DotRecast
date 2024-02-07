using System.Collections.Generic;
using System.Linq;
using DotRecast.Core.Collections;
using NUnit.Framework;

namespace DotRecast.Detour.Test;

public class DtNodeQueueTest
{
    [Test]
    public void TestDtNodeQueue()
    {
        var queue = new DtNodeQueue();

        // check count
        Assert.That(queue.Count(), Is.EqualTo(0));

        const int count = 1000;
        var nodes = new List<DtNode>();
        for (int i = 0; i < count; ++i)
        {
            var node = new DtNode(i);
            node.total = i;
            nodes.Add(node);
        }
        nodes.Shuffle();

        foreach (var node in nodes)
        {
            queue.Push(node);
        }

        Assert.That(queue.Count(), Is.EqualTo(count));
    }
}
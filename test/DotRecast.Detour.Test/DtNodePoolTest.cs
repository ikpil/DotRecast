using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;

namespace DotRecast.Detour.Test;

public class DtNodePoolTest
{
    [Test]
    public void TestGetNode()
    {
        var pool = new DtNodePool();

        var node1St = pool.GetNode(0);
        var node2St = pool.GetNode(0);
        Assert.That(node1St, Is.SameAs(node2St));

        node1St.state = 1;
        var node3St = pool.GetNode(0);
        Assert.That(node1St, Is.Not.SameAs(node3St));
    }

    [Test]
    public void TestFindNode()
    {
        var pool = new DtNodePool();

        var counts = ImmutableArray.Create(2, 3, 5);

        // get and create
        for (int i = 0; i < counts.Length; ++i)
        {
            var count = counts[i];
            for (int ii = 0; ii < count; ++ii)
            {
                var node = pool.GetNode(i);
                node.state = ii + 1;
            }
        }

        int sum = counts.Sum();
        Assert.That(sum, Is.EqualTo(10));

        // check GetNodeIdx GetNodeAtIdx
        for (int i = 0; i < sum; ++i)
        {
            var node = pool.GetNodeAtIdx(i);
            var nodeIdx = pool.GetNodeIdx(node);
            var nodeByIdx = pool.GetNodeAtIdx(nodeIdx);

            Assert.That(node, Is.SameAs(nodeByIdx));
            Assert.That(nodeIdx, Is.EqualTo(i));
        }

        // check count
        for (int i = 0; i < counts.Length; ++i)
        {
            var count = counts[i];
            var n = pool.FindNodes(i, out var nodes);
            Assert.That(n, Is.EqualTo(count));
            Assert.That(nodes, Has.Count.EqualTo(count));

            var node = pool.FindNode(i);
            Assert.That(nodes[0], Is.SameAs(node));

            var node2 = pool.FindNode(i);
            Assert.That(nodes[0], Is.SameAs(node2));
        }

        // check other count
        {
            var n = pool.FindNodes(4, out var nodes);
            Assert.That(n, Is.EqualTo(0));
            Assert.That(nodes, Is.Null);
        }

        var totalCount = pool.GetNodeCount();
        Assert.That(totalCount, Is.EqualTo(sum));

        pool.Clear();
        totalCount = pool.GetNodeCount();
        Assert.That(totalCount, Is.EqualTo(0));
    }
}
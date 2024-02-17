using System;
using DotRecast.Core.Buffers;
using NUnit.Framework;

namespace DotRecast.Core.Test;

// https://github.com/joaoportela/CircularBuffer-CSharp/blob/master/CircularBuffer.Tests/CircularBufferTests.cs
public class RcCyclicBufferTests
{
    [Test]
    public void RcCyclicBuffer_GetEnumeratorConstructorCapacity_ReturnsEmptyCollection()
    {
        var buffer = new RcCyclicBuffer<string>(5);
        Assert.That(buffer.ToArray(), Is.Empty);
    }

    [Test]
    public void RcCyclicBuffer_ConstructorSizeIndexAccess_CorrectContent()
    {
        var buffer = new RcCyclicBuffer<int>(5, new[] { 0, 1, 2, 3 });

        Assert.That(buffer.Capacity, Is.EqualTo(5));
        Assert.That(buffer.Size, Is.EqualTo(4));
        for (int i = 0; i < 4; i++)
        {
            Assert.That(buffer[i], Is.EqualTo(i));
        }
    }

    [Test]
    public void RcCyclicBuffer_Constructor_ExceptionWhenSourceIsLargerThanCapacity()
    {
        Assert.Throws<ArgumentException>(() => new RcCyclicBuffer<int>(3, new[] { 0, 1, 2, 3 }));
    }

    [Test]
    public void RcCyclicBuffer_GetEnumeratorConstructorDefinedArray_CorrectContent()
    {
        var buffer = new RcCyclicBuffer<int>(5, new[] { 0, 1, 2, 3 });

        int x = 0;
        buffer.ForEach(item =>
        {
            Assert.That(item, Is.EqualTo(x));
            x++;
        });
    }

    [Test]
    public void RcCyclicBuffer_PushBack_CorrectContent()
    {
        var buffer = new RcCyclicBuffer<int>(5);

        for (int i = 0; i < 5; i++)
        {
            buffer.PushBack(i);
        }

        Assert.That(buffer.Front(), Is.EqualTo(0));
        for (int i = 0; i < 5; i++)
        {
            Assert.That(buffer[i], Is.EqualTo(i));
        }
    }

    [Test]
    public void RcCyclicBuffer_PushBackOverflowingBuffer_CorrectContent()
    {
        var buffer = new RcCyclicBuffer<int>(5);

        for (int i = 0; i < 10; i++)
        {
            buffer.PushBack(i);
        }

        Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 5, 6, 7, 8, 9 }));
    }

    [Test]
    public void RcCyclicBuffer_GetEnumeratorOverflowedArray_CorrectContent()
    {
        var buffer = new RcCyclicBuffer<int>(5);

        for (int i = 0; i < 10; i++)
        {
            buffer.PushBack(i);
        }

        // buffer should have [5,6,7,8,9]
        int x = 5;
        buffer.ForEach(item =>
        {
            Assert.That(item, Is.EqualTo(x));
            x++;
        });
    }

    [Test]
    public void RcCyclicBuffer_ToArrayConstructorDefinedArray_CorrectContent()
    {
        var buffer = new RcCyclicBuffer<int>(5, new[] { 0, 1, 2, 3 });

        Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 0, 1, 2, 3 }));
    }

    [Test]
    public void RcCyclicBuffer_ToArrayOverflowedBuffer_CorrectContent()
    {
        var buffer = new RcCyclicBuffer<int>(5);

        for (int i = 0; i < 10; i++)
        {
            buffer.PushBack(i);
        }

        Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 5, 6, 7, 8, 9 }));
    }

    [Test]
    public void RcCyclicBuffer_PushFront_CorrectContent()
    {
        var buffer = new RcCyclicBuffer<int>(5);

        for (int i = 0; i < 5; i++)
        {
            buffer.PushFront(i);
        }

        Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 4, 3, 2, 1, 0 }));
    }

    [Test]
    public void RcCyclicBuffer_PushFrontAndOverflow_CorrectContent()
    {
        var buffer = new RcCyclicBuffer<int>(5);

        for (int i = 0; i < 10; i++)
        {
            buffer.PushFront(i);
        }

        Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 9, 8, 7, 6, 5 }));
    }

    [Test]
    public void RcCyclicBuffer_Front_CorrectItem()
    {
        var buffer = new RcCyclicBuffer<int>(5, new[] { 0, 1, 2, 3, 4 });

        Assert.That(buffer.Front(), Is.EqualTo(0));
    }

    [Test]
    public void RcCyclicBuffer_Back_CorrectItem()
    {
        var buffer = new RcCyclicBuffer<int>(5, new[] { 0, 1, 2, 3, 4 });
        Assert.That(buffer.Back(), Is.EqualTo(4));
    }

    [Test]
    public void RcCyclicBuffer_BackOfBufferOverflowByOne_CorrectItem()
    {
        var buffer = new RcCyclicBuffer<int>(5, new[] { 0, 1, 2, 3, 4 });
        buffer.PushBack(42);
        Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 1, 2, 3, 4, 42 }));
        Assert.That(buffer.Back(), Is.EqualTo(42));
    }

    [Test]
    public void RcCyclicBuffer_Front_EmptyBufferThrowsException()
    {
        var buffer = new RcCyclicBuffer<int>(5);

        Assert.Throws<InvalidOperationException>(() => buffer.Front());
    }

    [Test]
    public void RcCyclicBuffer_Back_EmptyBufferThrowsException()
    {
        var buffer = new RcCyclicBuffer<int>(5);
        Assert.Throws<InvalidOperationException>(() => buffer.Back());
    }

    [Test]
    public void RcCyclicBuffer_PopBack_RemovesBackElement()
    {
        var buffer = new RcCyclicBuffer<int>(5, new[] { 0, 1, 2, 3, 4 });

        Assert.That(buffer.Size, Is.EqualTo(5));

        buffer.PopBack();

        Assert.That(buffer.Size, Is.EqualTo(4));
        Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 0, 1, 2, 3 }));
    }

    [Test]
    public void RcCyclicBuffer_PopBackInOverflowBuffer_RemovesBackElement()
    {
        var buffer = new RcCyclicBuffer<int>(5, new[] { 0, 1, 2, 3, 4 });
        buffer.PushBack(5);

        Assert.That(buffer.Size, Is.EqualTo(5));
        Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));

        buffer.PopBack();

        Assert.That(buffer.Size, Is.EqualTo(4));
        Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 1, 2, 3, 4 }));
    }

    [Test]
    public void RcCyclicBuffer_PopFront_RemovesBackElement()
    {
        var buffer = new RcCyclicBuffer<int>(5, new[] { 0, 1, 2, 3, 4 });

        Assert.That(buffer.Size, Is.EqualTo(5));

        buffer.PopFront();

        Assert.That(buffer.Size, Is.EqualTo(4));
        Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 1, 2, 3, 4 }));
    }

    [Test]
    public void RcCyclicBuffer_PopFrontInOverflowBuffer_RemovesBackElement()
    {
        var buffer = new RcCyclicBuffer<int>(5, new[] { 0, 1, 2, 3, 4 });
        buffer.PushFront(5);

        Assert.That(buffer.Size, Is.EqualTo(5));
        Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 5, 0, 1, 2, 3 }));

        buffer.PopFront();

        Assert.That(buffer.Size, Is.EqualTo(4));
        Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 0, 1, 2, 3 }));
    }

    [Test]
    public void RcCyclicBuffer_SetIndex_ReplacesElement()
    {
        var buffer = new RcCyclicBuffer<int>(5, new[] { 0, 1, 2, 3, 4 });

        buffer[1] = 10;
        buffer[3] = 30;

        Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 0, 10, 2, 30, 4 }));
    }

    [Test]
    public void RcCyclicBuffer_WithDifferentSizeAndCapacity_BackReturnsLastArrayPosition()
    {
        // test to confirm this issue does not happen anymore:
        // https://github.com/joaoportela/RcCyclicBuffer-CSharp/issues/2

        var buffer = new RcCyclicBuffer<int>(5, new[] { 0, 1, 2, 3, 4 });

        buffer.PopFront(); // (make size and capacity different)

        Assert.That(buffer.Back(), Is.EqualTo(4));
    }

    [Test]
    public void RcCyclicBuffer_Clear_ClearsContent()
    {
        var buffer = new RcCyclicBuffer<int>(5, new[] { 4, 3, 2, 1, 0 });

        buffer.Clear();

        Assert.That(buffer.Size, Is.EqualTo(0));
        Assert.That(buffer.Capacity, Is.EqualTo(5));
        Assert.That(buffer.ToArray(), Is.EqualTo(new int[0]));
    }

    [Test]
    public void RcCyclicBuffer_Clear_WorksNormallyAfterClear()
    {
        var buffer = new RcCyclicBuffer<int>(5, new[] { 4, 3, 2, 1, 0 });

        buffer.Clear();
        for (int i = 0; i < 5; i++)
        {
            buffer.PushBack(i);
        }

        Assert.That(buffer.Front(), Is.EqualTo(0));
        for (int i = 0; i < 5; i++)
        {
            Assert.That(buffer[i], Is.EqualTo(i));
        }
    }
}
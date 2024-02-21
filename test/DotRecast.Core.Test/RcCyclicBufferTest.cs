using System;
using System.Collections.Generic;
using System.Linq;
using DotRecast.Core.Buffers;
using DotRecast.Core.Collections;
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
        foreach (var item in buffer)
        {
            Assert.That(item, Is.EqualTo(x));
            x++;
        }
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

    [Test]
    public void RcCyclicBuffer_RegularForEachWorks()
    {
        var refValues = new[] { 4, 3, 2, 1, 0 };
        var buffer = new RcCyclicBuffer<int>(5, refValues);

        var index = 0;
        foreach (var element in buffer)
        {
            Assert.That(element, Is.EqualTo(refValues[index++]));
        }
    }

    [Test]
    public void RcCyclicBuffer_EnumeratorWorks()
    {
        var refValues = new int[] { 4, 3, 2, 1, 0 };
        var buffer = new RcCyclicBuffer<int>(5, refValues);


        var index = 0;
        using var enumerator = buffer.GetEnumerator();
        enumerator.Reset();
        while (enumerator.MoveNext())
        {
            Assert.That(enumerator.Current, Is.EqualTo(refValues[index++]));
        }

        // Ensure Reset works properly
        index = 0;
        enumerator.Reset();
        while (enumerator.MoveNext())
        {
            Assert.That(enumerator.Current, Is.EqualTo(refValues[index++]));
        }
    }
    
    [Test]
    public void RcCyclicBuffers_Sum()
    {
        var refValues = Enumerable.Range(-100, 211).Select(x => (long)x).ToArray();
        var buffer = new RcCyclicBuffer<long>(refValues.Length, refValues);
        Assert.That(RcCyclicBuffers.Sum(buffer), Is.EqualTo(refValues.Sum()));
    }
    
    [Test]
    public void RcCyclicBuffers_Average()
    {
        var refValues = Enumerable.Range(-100, 211).Select(x => (long)x).ToArray();
        var buffer = new RcCyclicBuffer<long>(refValues.Length, refValues);
        Assert.That(RcCyclicBuffers.Average(buffer), Is.EqualTo(refValues.Average()));
    }

    [Test]
    public void RcCyclicBuffers_Min()
    {
        var refValues = Enumerable.Range(-100, 211).Select(x => (long)x).ToArray();
        var buffer = new RcCyclicBuffer<long>(refValues.Length, refValues);
        Assert.That(RcCyclicBuffers.Min(buffer), Is.EqualTo(refValues.Min()));
    }

    [Test]
    public void RcCyclicBuffers_Max()
    {
        var refValues = Enumerable.Range(-100, 211).Select(x => (long)x).ToArray();
        var buffer = new RcCyclicBuffer<long>(refValues.Length, refValues);
        Assert.That(RcCyclicBuffers.Max(buffer), Is.EqualTo(refValues.Max()));
    }
    
    [Test]
    public void RcCyclicBuffers_SumUnaligned()
    {
        var refValues = Enumerable.Range(-1, 3).Select(x => (long)x).ToArray();
        var buffer = new RcCyclicBuffer<long>(refValues.Length, refValues);
        Assert.That(RcCyclicBuffers.Sum(buffer), Is.EqualTo(refValues.Sum()));
    }
    
    [Test]
    public void RcCyclicBuffers_AverageUnaligned()
    {
        var refValues = Enumerable.Range(-1, 3).Select(x => (long)x).ToArray();
        var buffer = new RcCyclicBuffer<long>(refValues.Length, refValues);
        Assert.That(RcCyclicBuffers.Average(buffer), Is.EqualTo(refValues.Average()));
    }

    [Test]
    public void RcCyclicBuffers_MinUnaligned()
    {
        var refValues = Enumerable.Range(5, 3).Select(x => (long)x).ToArray();
        var buffer = new RcCyclicBuffer<long>(refValues.Length, refValues);
        Assert.That(RcCyclicBuffers.Min(buffer), Is.EqualTo(refValues.Min()));
    }

    [Test]
    public void RcCyclicBuffers_MaxUnaligned()
    {
        var refValues = Enumerable.Range(-5, 3).Select(x => (long)x).ToArray();
        var buffer = new RcCyclicBuffer<long>(refValues.Length, refValues);
        Assert.That(RcCyclicBuffers.Max(buffer), Is.EqualTo(refValues.Max()));
    }
    
    [Test]
    public void RcCyclicBuffers_SumDeleted()
    {
        var initialValues = Enumerable.Range(-100, 211).Select(x => (long)x).ToArray();
        var refValues = initialValues.Skip(1).SkipLast(1).ToArray();
        var buffer = new RcCyclicBuffer<long>(initialValues.Length, initialValues);
        buffer.PopBack();
        buffer.PopFront();
        
        Assert.That(RcCyclicBuffers.Sum(buffer), Is.EqualTo(refValues.Sum()));
    }
    
    [Test]
    public void RcCyclicBuffers_SumSplit()
    {
        var refValues = Enumerable.Range(-100, 211).Select(x => (long)x).ToArray();
        var buffer = new RcCyclicBuffer<long>(refValues.Length, refValues);
        buffer.PopFront();
        buffer.PushBack(refValues[0]);
        Assert.That(RcCyclicBuffers.Sum(buffer), Is.EqualTo(refValues.Sum()));
    }
    
    [Test]
    public void RcCyclicBuffers_AverageSplit()
    {
        var refValues = Enumerable.Range(-100, 211).Select(x => (long)x).ToArray();
        var buffer = new RcCyclicBuffer<long>(refValues.Length, refValues);
        buffer.PopFront();
        buffer.PushBack(refValues[0]);
        Assert.That(RcCyclicBuffers.Average(buffer), Is.EqualTo(refValues.Average()));
    }

    [Test]
    public void RcCyclicBuffers_MinSplit()
    {
        var refValues = Enumerable.Range(-100, 211).Select(x => (long)x).ToArray();
        var buffer = new RcCyclicBuffer<long>(refValues.Length, refValues);
        buffer.PopFront();
        buffer.PushBack(refValues[0]);
        Assert.That(RcCyclicBuffers.Min(buffer), Is.EqualTo(refValues.Min()));
    }

    [Test]
    public void RcCyclicBuffers_MaxSplit()
    {
        var refValues = Enumerable.Range(-100, 211).Select(x => (long)x).ToArray();
        var buffer = new RcCyclicBuffer<long>(refValues.Length, refValues);
        buffer.PopFront();
        buffer.PushBack(refValues[0]);
        Assert.That(RcCyclicBuffers.Max(buffer), Is.EqualTo(refValues.Max()));
    }
}
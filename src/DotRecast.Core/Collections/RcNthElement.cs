/*
recast4j copyright (c) 2021-2026 Piotr Piastucki piotr@recast4j.org
DotRecast Copyright (c) 2023-2026 Choi Ikpil ikpil@naver.com

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

using System.Collections.Generic;

namespace DotRecast.Core.Collections
{
    public static class RcNthElement
    {
        public static void NthElement<T>(T[] array, int low, int nth, int high, IComparer<T> comparer)
        {
            while (high - low > 3)
            {
                T midValue = MedianOfThree(
                    array[low],
                    array[low + ((high - low) >> 1)],
                    array[high - 1],
                    comparer);

                int first = low;
                int last = high;
                while (true)
                {
                    while (comparer.Compare(array[first], midValue) < 0)
                    {
                        first++;
                    }

                    do
                    {
                        last--;
                    } while (comparer.Compare(midValue, array[last]) < 0);

                    if (first >= last)
                    {
                        break;
                    }

                    (array[first], array[last]) = (array[last], array[first]);
                    first++;
                }

                if (first <= nth)
                {
                    low = first;
                }
                else
                {
                    high = first;
                }
            }

            InsertionSort(array, low, high, comparer);
        }

        private static void InsertionSort<T>(T[] array, int low, int high, IComparer<T> comparer)
        {
            for (int i = low + 1; i < high; i++)
            {
                T key = array[i];
                int j = i - 1;
                while (j >= low && comparer.Compare(array[j], key) > 0)
                {
                    array[j + 1] = array[j];
                    j--;
                }

                array[j + 1] = key;
            }
        }

        private static T MedianOfThree<T>(T a, T b, T c, IComparer<T> comparer)
        {
            if (comparer.Compare(a, b) < 0)
            {
                if (comparer.Compare(b, c) < 0)
                {
                    return b;
                }

                return comparer.Compare(a, c) < 0 ? c : a;
            }

            if (comparer.Compare(a, c) < 0)
            {
                return a;
            }

            return comparer.Compare(b, c) < 0 ? c : b;
        }
    }
}

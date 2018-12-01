using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Collections;
using NUnit.Framework;

namespace NativeArrays
{
    public class NativeArray1DTest
    {
        [Test]
        public void TestReadWriteArray()
        {
            int count = 256;
            var array = new NativeArray1D<int>(count, Allocator.Temp);
            for(int i = 0; i < count; i++)
            {
                array[i] = i * i;
                Assert.AreEqual(i * i, array[i]);
            }

            for(int i = 0; i < count; i++)
            {
                Assert.AreEqual(i * i, array[i]);
            }

            array.Clear();
            for(int i = 0; i < count; i++)
            {
                Assert.AreEqual(0, array[i]);
            }

            array.Dispose();
        }

        [Test]
        public void TestCopyFromArray()
        {
            int count = 256;
            var array = new NativeArray1D<int>(count, Allocator.Temp);
            var array2 = new NativeArray1D<int>(count * 2, Allocator.Temp);
            for(int i = 0; i < array2.Size; i++)
            {
                array2[i] = i * i;
                Assert.AreEqual(i * i, array2[i]);
            }

            for(int i = 0; i < array.Size; i++)
            {
                Assert.AreEqual(0, array[i]);
            }

            array.CopyFrom(array2, count);

            for(int i = 0; i < array.Size; i++)
            {
                Assert.AreEqual(i * i, array[i]);
            }

            array.Dispose();
        }
    }
}
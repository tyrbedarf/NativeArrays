using System;
using System.Collections.Generic;
using UnityEngine;

using Unity.Collections;
using NUnit.Framework;

namespace NativeArrays
{
    public class NativeArray3D
    {
        const int Width = 32;
        const int Height = 32;
        const int Depth = 32;

        [Test]
        public void TestReadWrite()
        {
            var subject = new NativeArray3D<int>(Width, Height, Depth, Allocator.Temp);
            Assert.AreEqual(subject.Width, Width);
            Assert.AreEqual(subject.Height, Height);
            Assert.AreEqual(subject.Depth, Depth);

            var value = 0;
            for(int x = 0; x < Width; x++)
            {
                for(int y = 0; y < Height; y++)
                {
                    for(int z = 0; z < Depth; z++)
                    {
                        subject[x, y, z] = value;
                        Assert.AreEqual(value, subject[x, y, z]);
                        value++;
                    }
                }
            }

            value = 0;
            for(int x = 0; x < Width; x++)
            {
                for(int y = 0; y < Height; y++)
                {
                    for(int z = 0; z < Depth; z++)
                    {
                        Assert.AreEqual(value, subject[x, y, z]);
                        value++;
                    }
                }
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Assert.Throws<IndexOutOfRangeException>(() => { subject[Width, Height + 1, Depth] = 10; });
            Assert.Throws<IndexOutOfRangeException>(() => { subject[Width + 1, Height, Depth] = 10; });
            Assert.Throws<IndexOutOfRangeException>(() => { subject[-1, Height, Depth] = 10; });
            Assert.Throws<IndexOutOfRangeException>(() => { subject[Width, -1, Depth] = 10; });
            Assert.Throws<IndexOutOfRangeException>(() => { subject[Width, Height, -1] = 10; });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var test = new NativeArray3D<int>(-1, 10, 10, Allocator.Temp);
                });

            Assert.Throws<ArgumentOutOfRangeException>(() => 
                {
                    var test = new NativeArray3D<int>(10, -10, 10, Allocator.Temp);
                });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var test = new NativeArray3D<int>(-1, 10, -1, Allocator.Temp);
                });

            Assert.Throws<ArgumentOutOfRangeException>(() => 
                {
                    var test = new NativeArray3D<int>(-10, -10, -10, Allocator.Temp);
                });

            Assert.Throws<ArgumentException>(() => 
                {
                    var test = new NativeArray3D<int>(Width, Height, Depth, Allocator.None);
                });
#endif

            subject.Dispose();
        }
    }
}

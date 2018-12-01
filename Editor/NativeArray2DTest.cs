using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Collections;
using NUnit.Framework;

namespace NativeArrays
{
    public class NativeArray2DTest
    {
        const int Width = 254;
        const int Height = 254;

        [Test]
        public void TestAsymArray()
        {
            const int width = 16;
            const int height =22;

            var subject = new NativeArray2D<int>(width, height, Allocator.Temp);
            Assert.AreEqual(subject.Width, width);
            Assert.AreEqual(subject.Height, height);

            var test = 0;
            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    subject[x, y] = test;
                    Assert.AreEqual(test, subject[x, y]);
                    test++;
                }
            }

            test = 0;
            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    Assert.AreEqual(test, subject[x, y]);
                    test++;
                }
            }

            subject.Dispose();
        }

        enum Side
        {
            Top,
            Front,
            Left,
            Right,
            Bottom,
            Back
        }

        [Test]
        public void TestEnumArray()
        {
            const int width = 16;
            const int height = 4;

            var subject = new NativeArray2D<Side>(width, height, Allocator.Temp);
            Assert.AreEqual(subject.Width, width);
            Assert.AreEqual(subject.Height, height);

            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    var test = (Side) ((x + y) % Enum.GetValues(typeof(Side)).Length);
                    subject[x, y] = test;
                    Assert.AreEqual(test, subject[x, y]);
                }
            }

            subject.Dispose();
        }

        [Test]
        public void TestReadWrite()
        {
            var subject = new NativeArray2D<int>(Width, Height, Allocator.Temp);
            Assert.AreEqual(subject.Width, Width);
            Assert.AreEqual(subject.Height, Height);

            var value = 0;
            for(int x = 0; x < Width; x++)
            {
                for(int y = 0; y < Height; y++)
                {
                    var test = value;
                    subject[x, y] = test;
                    Assert.AreEqual(test, subject[x, y]);

                    value++;
                }
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Assert.Throws<IndexOutOfRangeException>(() => { subject[Width, Height + 1] = 10; });
            Assert.Throws<IndexOutOfRangeException>(() => { subject[Width + 1, Height] = 10; });
            Assert.Throws<IndexOutOfRangeException>(() => { subject[-1, Height] = 10; });
            Assert.Throws<IndexOutOfRangeException>(() => { subject[Width, -1] = 10; });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var test = new NativeArray2D<int>(-1, 10, Allocator.Temp);
                });

            Assert.Throws<ArgumentOutOfRangeException>(() => 
                {
                    var test = new NativeArray2D<int>(10, -10, Allocator.Temp);
                });

            Assert.Throws<ArgumentException>(() => 
                {
                    var test = new NativeArray2D<int>(Width, Height, Allocator.None);
                });

            Assert.Throws<ArgumentException>(() => 
                {
                    var test = new NativeArray2D<NativeArray<int>>(Width, Height, Allocator.Persistent);
                });
#endif

            subject.Dispose();
        }
    }
}

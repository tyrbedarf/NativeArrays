using System.Diagnostics;
using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

namespace NativeArrays {
    [NativeContainer]
    [NativeContainerSupportsMinMaxWriteRestriction]
    [DebuggerDisplay("Width = {" + nameof(Width) + "} Height: {" + nameof(Height) + "} Depth { " + nameof(Depth) + "}")]
    [DebuggerTypeProxy(typeof(NativeArray3DDebugView<>))]
    public unsafe struct NativeArray3D<T> : IDisposable
        where T : struct
    {
        [NativeDisableUnsafePtrRestriction]
        private void* Buffer;

        private Allocator Allocator;
        public int Width    { get; private set; }
        public int Height   { get; private set; }
        public int Depth    { get; private set; }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle m_Safety;
        [NativeSetClassTypeToNullOnSchedule]
        internal DisposeSentinel m_DisposeSentinel;
#endif

        public NativeArray3D(int width, int height, int depth, Allocator allocator)
        {
            Height = height;
            Width = width;
            Depth = depth;

            var Length = Width * Height * Depth;
            long totalSize = UnsafeUtility.SizeOf<T>() * (long) (Length);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if(allocator <= Allocator.None)
            {
                throw new ArgumentException(
                    "Allocator must be Temp, TempJob or Persistent",
                    nameof(allocator));
            }

            if(Height < 0 || Width < 0 || Depth < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "Negative sizes are not allowed.",
                    nameof(allocator));
            }

            if(Length < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Length),
                    "Length must be >= 0");
            }

            if(!UnsafeUtility.IsBlittable<T>())
            {
                throw new ArgumentException(
                    string.Format(
                        "{0} used in NativeCustomArray<{0}> must be blittable",
                        typeof(T)));
            }
#endif

            Buffer = UnsafeUtility.Malloc(
                totalSize,
                UnsafeUtility.AlignOf<T>(),
                allocator);
            UnsafeUtility.MemClear(Buffer, totalSize);

            this.Allocator = allocator;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0, allocator);
#endif
        }

        private int GetIndex(int x, int y, int z)
        {
            return (z * this.Width * this.Height) + (y * this.Width) + x;
        }

        public T this[int x, int y, int z]
        {
            get
            {
                var index = GetIndex(x, y, z);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
                FailOutOfRangeError(x, y, z);
#endif
                return UnsafeUtility.ReadArrayElement<T>(Buffer, index);
            }

            set
            {
                var index = GetIndex(x, y, z);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
                FailOutOfRangeError(x, y, z);
#endif
                UnsafeUtility.WriteArrayElement(Buffer, index, value);
            }
        }

        public T[] ToArray()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif

            var size = Width * Height * Depth;
            T[] array = new T[size];
            for(int i = 0; i < size; i++)
            {
                array[i] = UnsafeUtility.ReadArrayElement<T>(Buffer, i);
            }
            return array;
        }

        public bool IsCreated
        {
            get
            {
                return Buffer != null;
            }
        }

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif

            UnsafeUtility.Free(Buffer, Allocator);
            Buffer = null;
            Width = 0;
            Height = 0;
            Depth = 0;
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private void FailOutOfRangeError(int x, int y, int z)
        {
            if(x >= Width || x < 0)
            {
                throw new IndexOutOfRangeException(
                    $"Width {x} is out of restricted IJobParallelFor range " +
                    $"[{Width} x {Height} x {Depth}] in ReadWriteBuffer.\n" +
                    "ReadWriteBuffers are restricted to only read & write the " +
                    "element at the job index. You can use double buffering " +
                    "strategies to avoid race conditions due to reading & " +
                    "writing in parallel to the same elements from a job.");
            }

            if(y >= Height || y < 0)
            {
                throw new IndexOutOfRangeException(
                    $"Height {y} is out of restricted IJobParallelFor range " +
                    $"[{Width} x {Height} x {Depth}] in ReadWriteBuffer.\n" +
                    "ReadWriteBuffers are restricted to only read & write the " +
                    "element at the job index. You can use double buffering " +
                    "strategies to avoid race conditions due to reading & " +
                    "writing in parallel to the same elements from a job.");
            }

            if(z >= Depth || z < 0)
            {
                throw new IndexOutOfRangeException(
                    $"Depth {z} is out of restricted IJobParallelFor range " +
                    $"[{Width} x {Height} x {Depth}] in ReadWriteBuffer.\n" +
                    "ReadWriteBuffers are restricted to only read & write the " +
                    "element at the job index. You can use double buffering " +
                    "strategies to avoid race conditions due to reading & " +
                    "writing in parallel to the same elements from a job.");
            }
        }
#endif
    }

    internal sealed class NativeArray3DDebugView<T>
        where T : struct
    {
        private NativeArray3D<T> m_Array;

        public NativeArray3DDebugView(NativeArray3D<T> array)
        {
            m_Array = array;
        }

        public T[] Items
        {
            get
            {
                return m_Array.ToArray();
            }
        }
    }
}
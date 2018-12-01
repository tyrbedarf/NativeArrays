using System.Diagnostics;
using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

namespace NativeArrays {
    [NativeContainer]
    [NativeContainerSupportsMinMaxWriteRestriction]
    [DebuggerDisplay("Width = {" + nameof(Width) + "} Height = {" + nameof(Height) + "}")]
    [DebuggerTypeProxy(typeof(NativeArray2DDebugView<>))]
    public unsafe struct NativeArray2D<T> : IDisposable
        where T : struct
    {
        [NativeDisableUnsafePtrRestriction]
        private void* Buffer;
        
        private Allocator Allocator;
        public int Width    { get; private set; }
        public int Height   { get; private set; }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle m_Safety;
        [NativeSetClassTypeToNullOnSchedule]
        internal DisposeSentinel m_DisposeSentinel;
#endif

        public NativeArray2D(int width, int height, Allocator allocator)
        {
            Width = width;
            Height = height;
            
            long totalSize = UnsafeUtility.SizeOf<T>() * (long) (Width * Height);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var Length = Width * Height;
            if(allocator <= Allocator.None)
            {
                throw new ArgumentException(
                    "Allocator must be Temp, TempJob or Persistent",
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

        public T this[int x, int y]
        {
            get
            {
                var index = (Height * x) + y;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
                FailWOutOfRange(x, y);
#endif
                return UnsafeUtility.ReadArrayElement<T>(Buffer, index);
            }

            set
            {
                var index = (Height * x) + y;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
                FailWOutOfRange(x, y);
#endif
                UnsafeUtility.WriteArrayElement(Buffer, index, value);
            }
        }

        /// <summary>
        /// Returns a the flat array to be view in the debugger.
        /// </summary>
        /// <returns></returns>
        public T[] ToArray()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
            var size = Width * Height;
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
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private void FailWOutOfRange(int width, int height)
        {
            if(width >= Width || width < 0)
            {
                throw new IndexOutOfRangeException(
                    $"Width '{width}' is out of restricted IJobParallelFor range " +
                    $"[{Width} x {Height}] in ReadWriteBuffer.\n" +
                    "ReadWriteBuffers are restricted to only read & write the " +
                    "element at the job index. You can use double buffering " +
                    "strategies to avoid race conditions due to reading & " +
                    "writing in parallel to the same elements from a job.");
            }

            if(height >= Height || height < 0)
            {
                throw new IndexOutOfRangeException(
                    $"Height '{height}' is out of restricted IJobParallelFor range " +
                    $"[{Width} x {Height}] in ReadWriteBuffer.\n" +
                    "ReadWriteBuffers are restricted to only read & write the " +
                    "element at the job index. You can use double buffering " +
                    "strategies to avoid race conditions due to reading & " +
                    "writing in parallel to the same elements from a job.");
            }
        }
#endif
    }

    internal sealed class NativeArray2DDebugView<T>
        where T : struct
    {
        private NativeArray2D<T> m_Array;

        public NativeArray2DDebugView(NativeArray2D<T> array)
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
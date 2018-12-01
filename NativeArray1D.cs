using System.Diagnostics;
using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

namespace NativeArrays {
    [NativeContainer]
    [NativeContainerSupportsMinMaxWriteRestriction]
    [DebuggerDisplay("Size = {" + nameof(Size) + "}")]
    [DebuggerTypeProxy(typeof(NativeArray3DDebugView<>))]
    public unsafe struct NativeArray1D<T> : IDisposable
        where T : struct
    {
        [NativeDisableUnsafePtrRestriction]
        private void* Buffer;

        private Allocator Allocator;
        public int Size    { get; private set; }
        private long TotalSize;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle m_Safety;
        [NativeSetClassTypeToNullOnSchedule]
        internal DisposeSentinel m_DisposeSentinel;
#endif

        public NativeArray1D(int size, Allocator allocator)
        {
            Size = size;
            TotalSize = UnsafeUtility.SizeOf<T>() * (long) (Size);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if(allocator <= Allocator.None)
            {
                throw new ArgumentException(
                    "Allocator must be Temp, TempJob or Persistent",
                    nameof(allocator));
            }

            if(Size < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Size),
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
                TotalSize,
                UnsafeUtility.AlignOf<T>(),
                allocator);

            this.Allocator = allocator;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0, allocator);
#endif
            // Clear the memory.
            Clear();
        }

        /**
         * Reset the array
         * 
         **/
        public void Clear()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
            UnsafeUtility.MemClear(Buffer, TotalSize);
        }

        public void CopyFrom(NativeArray1D<T> source, int count)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
            
            if(count >= source.Size)
            {
                throw new IndexOutOfRangeException();
            }
#endif
            var length = UnsafeUtility.SizeOf<T>() * (long) (count);
            UnsafeUtility.MemCpy(Buffer, source.Buffer, length);
        }

        public T this[int x]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
                FailOutOfRangeError(x);
#endif
                return UnsafeUtility.ReadArrayElement<T>(Buffer, x);
            }

            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
                FailOutOfRangeError(x);
#endif
                UnsafeUtility.WriteArrayElement(Buffer, x, value);
            }
        }

        public T[] ToArray()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif

            T[] array = new T[Size];
            for(int i = 0; i < Size; i++)
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
            Size = 0;
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private void FailOutOfRangeError(int x)
        {
            if(x >= Size || x < 0)
            {
                throw new IndexOutOfRangeException(
                    $"Width {x} is out of restricted IJobParallelFor range " +
                    $"[{Size}] in ReadWriteBuffer.\n" +
                    "ReadWriteBuffers are restricted to only read & write the " +
                    "element at the job index. You can use double buffering " +
                    "strategies to avoid race conditions due to reading & " +
                    "writing in parallel to the same elements from a job.");
            }
        }
#endif
    }

    internal sealed class NativeArray1DDebugView<T>
        where T : struct
    {
        private NativeArray1D<T> m_Array;

        public NativeArray1DDebugView(NativeArray1D<T> array)
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
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Foundation.OpenCL.Tests.Tensors
{
    public enum TensorLayout
    {
        LastIndexContiguous,            // row major for matrices - default
        FirstIndexContiguous,           // column major for matrices
    }

    public interface ITensorHandle : IDisposable
    {
        bool IsValid();
    }

    public unsafe class TensorMemoryManager : ITensorHandle
    {
        public Tensor<T> Create<T>(
            ReadOnlySpan<int> dimensions,
            ReadOnlySpan<int> strides,
            int size = 0,
            bool clear = true)
            where T : unmanaged
        {
            size = int.Max(size, RequiredSize(dimensions, strides));

            var rep = Allocate<T>(size);

            if (clear)
            {
                var span = new Span<T>(rep, size);
                span.Clear();           // Todo SIMD
            }

            return new Tensor<T>(
                rep, dimensions, strides, size, this);
        }

        private static int RequiredSize(
            ReadOnlySpan<int> dimensions,
            ReadOnlySpan<int> strides)
        {
            var size = 0;
            for (var i = 0; i < dimensions.Length; i++)
            {
                size = int.Max(strides[i] * dimensions[i], size);
            }
            return size;
        }

        public Tensor<T> Create<T>(
            ReadOnlySpan<int> dimensions,
            TensorLayout layout = TensorLayout.LastIndexContiguous,
            bool clear = true)
            where T : unmanaged
        {
            if (disposed) throw new InvalidOperationException("Already disposed");

            Span<int> strides = stackalloc int[dimensions.Length];

            var size = 1;
            if (layout == TensorLayout.LastIndexContiguous)
            {
                for (var i = dimensions.Length - 1; i >= 0; i--)
                {
                    strides[i] = size;
                    size *= dimensions[i];
                }
            }
            else
            {
                for (var i = 0; i < dimensions.Length; i++)
                {
                    strides[i] = size;
                    size *= dimensions[i];
                }
            }

            return Create<T>(dimensions, strides, size, clear);
        }

        public Tensor<T> CreateFromValues<T>(
            ReadOnlySpan<int> dimensions,
            params ReadOnlySpan<T> values)
            where T : unmanaged
        {
            var tensor = Create<T>(dimensions);
            values.CopyTo(tensor.AsSpan());
            return tensor;
        }

        public T* Allocate<T>(int size)
            where T : unmanaged
        {
            var rep = (T*)NativeMemory.AlignedAlloc((nuint)(size * sizeof(T)), (nuint)(128 * sizeof(T)));
            toDispose.Add((IntPtr)rep);
            return rep;
        }

        private bool disposed = false;
        private readonly List<IntPtr> toDispose = new ();

        public bool IsValid()
        {
            return !disposed;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            foreach (var ptr in toDispose)
            {
                NativeMemory.AlignedFree((void*) ptr);
            }

            GC.SuppressFinalize(this);
        }

        ~TensorMemoryManager()
        {
            Dispose();
        }
    }
}

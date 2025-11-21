using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Foundation.OpenCL.Tests.Tensors
{
    public unsafe interface ITensor<T>
        where T : unmanaged
    {
        T* Rep { get; }
        int Rank { get; }
        int[] Dimensions { get; }
        int[] Strides { get; }

        ref T this[params ReadOnlySpan<int> indices] { get; }
        int GetPosition(params ReadOnlySpan<int> indices);

        ITensorHandle Handle { get; }
    }

    public  readonly unsafe struct Tensor<T> : ITensor<T>
        where T : unmanaged
    {
        private readonly T* rep;
        private readonly int[] dimensions;
        private readonly int[] strides;
        private readonly int size;

        private readonly ITensorHandle handle;

        public Tensor(
            T* rep,
            ReadOnlySpan<int> dimensions,
            ReadOnlySpan<int> strides,
            int size,
            ITensorHandle handle,
            int offset = 0,
            bool allowUnaligned = false)
        {
            this.rep = rep + offset;
            this.handle = handle;
            this.dimensions = dimensions.ToArray();
            this.strides = strides.ToArray();
            this.size = TensorExtensions.RequiredSize(dimensions, strides);

            #region Debug Validation

            Check.That(dimensions.Length == strides.Length);
            Check.That(handle.IsValid());
            Check.That(rep != null);
            Check.That(offset >= 0);

            var dimensionsArray = Dimensions;
            var stridesArray = Strides;

            // Lazy validation for zero overhead in Release
            Check.That(() => dimensionsArray.All(d => d > 0) &&
                             stridesArray.All(s => s >= 0)); // Allow zero-strides for broadcasting

            Check.That(() => size - offset >= TensorExtensions.RequiredSize(dimensionsArray, stridesArray));

            // Optional: validate memory alignment for SIMD
            if (!allowUnaligned) Check.Aligned(rep, 128);

            #endregion
        }

        public T* Rep
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => rep;
        }

        public int Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => size;
        }

        public nuint ByteSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (nuint)(size * sizeof(T));
        }

        public ITensorHandle Handle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => handle;
        }

        public int Rank
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => dimensions.Length;
        }

        public int[] Dimensions
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => dimensions;
        }

        public int[] Strides
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => strides;
        }

        #region Individual Indexer (for slow reference code only)

        public ref T this[params ReadOnlySpan<int> indices]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref rep[GetPosition(indices)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPosition(params ReadOnlySpan<int> indices)
        {
            var position = 0;
            for (var k = 0; k < indices.Length; k++)
            {
                position += strides[k] * indices[k];
                Check.That(0 <= indices[k]);
                Check.That(indices[k] < Dimensions[k]);
            }

            Check.That(position < Size);

            return position;
        }

        #endregion
    }

    public static unsafe class TensorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this Tensor<T> tensor)
            where T : unmanaged
        {
            return new Span<T>(tensor.Rep, tensor.Size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor<T> Flat<T>(this Tensor<T> tensor)
            where T : unmanaged
        {
            return new Tensor<T>(tensor.Rep, [tensor.Size], [1], tensor.Size, tensor.Handle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RequiredSize<T>(this Tensor<T> tensor)
            where T : unmanaged
        {
            return RequiredSize(tensor.Dimensions, tensor.Strides);
        }

        public static int RequiredSize(ReadOnlySpan<int> dimensions, ReadOnlySpan<int> strides)
        {
            var maxIndex = 0;

            for (var i = 0; i < dimensions.Length; i++)
            {
                maxIndex += (dimensions[i] - 1) * strides[i];
            }

            return maxIndex + 1;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static bool IsContiguous<T>(this Tensor<T> tensor)
        //    where T : unmanaged
        //{
        //    return tensor.Strides.Any(s => s == 1);
        //}

        //public static Span<T> DimensionAsSpan<T>(this Tensor<T> tensor, int dim)
        //    where T : unmanaged
        //{
        //    return tensor.Strides[i] == dim;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsContiguous<T>(this Tensor<T> tensor, int dim)
            where T : unmanaged
        {
            return tensor.Strides[dim] == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor<T> Permute<T>(this Tensor<T> tensor, params int[] permutation)
            where T : unmanaged
        {
            return new Tensor<T>(tensor.Rep,
                [.. permutation.Select(i => tensor.Dimensions[i])],
                [.. permutation.Select(i => tensor.Strides[i])],
                tensor.Size,
                tensor.Handle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPosition<T>(this Tensor<T> tensor, params ReadOnlySpan<int> indices)
            where T : unmanaged
        {
            var position = 0;
            for (var k = 0; k < indices.Length; k++)
                position += tensor.Strides[k] * indices[k];
            return position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tensor<T> Slice<T>(
            this Tensor<T> tensor,
            ReadOnlySpan<int> dimensions,
            ReadOnlySpan<int> strides,
            int offset = 0)
            where T : unmanaged
        {
            return new Tensor<T>(
                tensor.Rep,
                dimensions,
                strides,
                tensor.Size,
                tensor.Handle,
                offset);
        }
    }
}

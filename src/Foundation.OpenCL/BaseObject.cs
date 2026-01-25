using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Foundation.OpenCL
{
    public readonly struct Handle<T>
    {
        private readonly nint rep;

        public nint Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => rep;
        }

        public override string ToString() => $"Handle: 0x{rep:X}";

        public static Handle<T> Null => new();
    }

    public interface IReify<out TSelf, TTag>
    {
        public static abstract TSelf Reify(Handle<TTag> handle);
    }

    public interface IReify<T> : IReify<T, T>
    {

    }

    public abstract class BaseObject<TSelf, TTag>(Handle<TTag> handle)
        : IDisposable
        where TSelf : BaseObject<TSelf, TTag>, IReify<TSelf, TTag>
    {
        private Handle<TTag> handle = handle;

        public Handle<TTag> Handle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => handle;
        }

        public override string ToString() => $"OpenCL {typeof(TSelf).Name} ({Handle})";

        public TSelf Retain()
        {
            var tmpHandle = Handle;
            RetainHook();
            return TSelf.Reify(tmpHandle);
        }

        // The internal managed reference count, initialized to 1 for the object's creator.
        // This controls when the native resource (handle) is released.
        private int localReferenceCount = 1;

        /// <summary>
        /// Decrements the managed reference count. If the count reaches zero, the native
        /// OpenCL resource is released.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            // Decrement the managed reference count. If the result is greater than 0,
            // another managed owner still holds a reference, so we stop here.
            if (Interlocked.CompareExchange(ref localReferenceCount, 0, 1) == 1)
            {
                // 1. Fire disposal event
                if (disposing)
                {
                    var tmp = OnDispose;
                    if (tmp != null) try { tmp(); } catch { }
                }

                var tmpHandle = Handle;                     // 2. Capture the current handle
                handle = Handle<TTag>.Null;                 // 3. Immediately invalidate the instance handle
                Interlocked.MemoryBarrier();                // 4. Memory barrier
                ReleaseHook(tmpHandle);                     // 5. Release the native resource using the captured handle

                // 5. Prevent finalizer from running, as cleanup was performed.
                GC.SuppressFinalize(this);

                return;
            }

            if (disposing) throw new ObjectDisposedException(nameof(TSelf));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static unsafe TP* NullIfEmpty<TP, TS>(Span<TS> span, TP* ptr)
            where TP : unmanaged
            => span.IsEmpty ? null : ptr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static unsafe TP* NullIfEmpty<TP, TS>(ReadOnlySpan<TS> span, TP* ptr)
            where TP : unmanaged
            => span.IsEmpty ? null : ptr;

        public event Action? OnDispose;

        protected abstract void RetainHook();                                   // calls the Native API
        protected abstract void ReleaseHook(Handle<TTag> tmpHandle);            // calls the Native API

        ~BaseObject()
        {
            Dispose(false);
        }
    }

    public abstract class BaseObject<TSelf>(Handle<TSelf> handle)
        : BaseObject<TSelf, TSelf>(handle)
        where TSelf : BaseObject<TSelf>, IReify<TSelf>
    {

    }


    public abstract class InformationNode<TSelf, TInfo>(Handle<TSelf> handle)
        : InformationNode<TSelf, TSelf, TInfo>(handle)
        where TSelf : InformationNode<TSelf, TInfo>, IReify<TSelf>
        where TInfo : Enum
    {

    }

    public abstract unsafe class InformationNode<TSelf, TTag, TInfo>(Handle<TTag> handle)
        : BaseObject<TSelf, TTag>(handle)
        where TSelf: InformationNode<TSelf, TTag, TInfo>, IReify<TSelf, TTag>
        where TInfo : Enum
    {
        protected abstract void GetInfo(TInfo paramName,
            nuint paramValueSize,
            void* paramValue,
            out nuint paramValueSizeRet);

        public bool TryGetInfo<T>(TInfo paramName, out T value)
            where T : unmanaged
        {
            var val = value = default;
            GetInfo(paramName, (nuint)sizeof(T), &val, out var size);
            if ((int)size != sizeof(T)) return false;
            value = val;
            return true;
        }

        public T GetInfo<T>(TInfo paramName)
            where T : unmanaged
        {
            if (TryGetInfo(paramName, out T value)) return value;
            throw new InvalidOperationException();
        }

        public string GetStringInfo(TInfo paramName)
        {
            var length = GetInfoByteSize(paramName);
            var buffer = stackalloc byte[length];

            GetInfo(paramName, (nuint)length, buffer, out _);
            return Encoding.ASCII.GetString(new ReadOnlySpan<byte>(buffer,
                length - 1)); // C-strings have an extra zero in the end.
        }

        public int GetInfoByteSize(TInfo paramName)
        {
            GetInfo(paramName, 0, null, out var size);
            return (int)size;
        }

        public bool TryGetInfo<T>(TInfo paramName, Span<T> values)
            where T : unmanaged
        {
            fixed (T* ptr = values)
            {
                var bufferSize = (nuint)(sizeof(T) * values.Length);
                GetInfo(paramName, bufferSize, ptr, out var size);
                return size == bufferSize;
            }
        }
    }
}
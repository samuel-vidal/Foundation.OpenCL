using System;
using System.Text;

namespace Foundation.OpenCL
{
    #region Constants

    public enum PipeInfo
    {
        PacketSize = 0x1120,          // CL_PIPE_PACKET_SIZE
        MaxPackets = 0x1121,          // CL_PIPE_MAX_PACKETS
        Properties = 0x1122,          // CL_PIPE_PROPERTIES
    }

    #endregion

    public sealed unsafe class Pipe(Handle<MemoryObject> handle)
        : BaseMemoryObject<Pipe>(handle), IReify<Pipe, MemoryObject>
    {
        public int GetPacketSize() => GetInfo<int>(PipeInfo.PacketSize);
        public int GetMaxPackets() => GetInfo<int>(PipeInfo.MaxPackets);

        private void GetInfo(PipeInfo paramName, nuint paramValueSize, void* paramValue, out nuint paramValueSizeRet)
            => OpenCLNative.GetPipeInfo(Handle, paramName, paramValueSize, paramValue, out paramValueSizeRet).ThrowIfUnsuccessful();

        public bool TryGetInfo<T>(PipeInfo paramName, out T value)
            where T : unmanaged
        {
            var val = value = default;
            GetInfo(paramName, (nuint)sizeof(T), &val, out var size);
            if ((int)size != sizeof(T)) return false;
            value = val;
            return true;
        }

        public T GetInfo<T>(PipeInfo paramName)
            where T : unmanaged
        {
            if (TryGetInfo(paramName, out T value)) return value;
            throw new InvalidOperationException();
        }

        public string GetStringInfo(PipeInfo paramName)
        {
            var length = GetInfoByteSize(paramName);
            var buffer = stackalloc byte[length];

            GetInfo(paramName, (nuint)length, buffer, out _);
            return Encoding.ASCII.GetString(new ReadOnlySpan<byte>(buffer, length - 1));        // C-strings have an extra zero in the end.
        }

        public int GetInfoByteSize(PipeInfo paramName)
        {
            GetInfo(paramName, 0, null, out var size);
            return (int)size;
        }

        public bool TryGetInfo<T>(PipeInfo paramName, Span<T> values)
            where T : unmanaged
        {
            fixed (T* ptr = values)
            {
                var bufferSize = (nuint)(sizeof(T) * values.Length);
                GetInfo(paramName, bufferSize, ptr, out var size);
                return size == bufferSize;
            }
        }

        public static Pipe Reify(Handle<MemoryObject> handle) => new(handle);
    }
}

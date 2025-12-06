using System;

namespace Foundation.OpenCL.Extensions.Intel
{
    #region Constants

    public enum MemAllocFlag : ulong
    {
        None = 0x0,
        WriteCombined = 0x1,                   // CL_MEM_ALLOC_WRITE_COMBINED_INTEL
        InitialPlacementDevice = 0x2,          // CL_MEM_ALLOC_INITIAL_PLACEMENT_DEVICE_INTEL
        InitialPlacementHost = 0x4,            // CL_MEM_ALLOC_INITIAL_PLACEMENT_HOST_INTEL
    }

    public enum MemAllocInfo
    {
        Type = 0x419A,                    // CL_MEM_ALLOC_TYPE_INTEL
        BasePtr = 0x419B,                 // CL_MEM_ALLOC_BASE_PTR_INTEL
        Size = 0x419C,                    // CL_MEM_ALLOC_SIZE_INTEL
        Device = 0x419D,                  // CL_MEM_ALLOC_DEVICE_INTEL
        BufferLocation = 0x419E,          // CL_MEM_ALLOC_BUFFER_LOCATION_INTEL
    }

    public enum MemProperties : ulong
    {
        AllocFlags = 0x4195,                   // CL_MEM_ALLOC_FLAGS_INTEL
        Channel = 0x4213,                      // CL_MEM_CHANNEL_INTEL
        AllocBufferLocation = 0x419E,          // CL_MEM_ALLOC_BUFFER_LOCATION_INTEL
    }

    public enum UnifiedSharedMemoryType
    {
        Unknown = 0x4196,          // CL_MEM_TYPE_UNKNOWN_INTEL
        Host = 0x4197,             // CL_MEM_TYPE_HOST_INTEL
        Device = 0x4198,           // CL_MEM_TYPE_DEVICE_INTEL
        Shared = 0x4199,           // CL_MEM_TYPE_SHARED_INTEL
    }

    public enum MemAdvice
    {
        Default = 0x0,
        ReadMostly = 0x1,           // CL_MEM_ADVICE_READ_MOSTLY_INTEL
        PreferredLocationDevice = 0x2,  // CL_MEM_ADVICE_PREFERRED_LOCATION_DEVICE_INTEL
        PreferredLocationHost = 0x3,    // CL_MEM_ADVICE_PREFERRED_LOCATION_HOST_INTEL
        NonTemporal = 0x4,          // CL_MEM_ADVICE_NON_TEMPORAL_INTEL
        Cached = 0x5,               // CL_MEM_ADVICE_CACHED_INTEL
        Uncached = 0x6,             // CL_MEM_ADVICE_UNCACHED_INTEL
    }

    #endregion

    public readonly struct UsmConfig(MemProperties property, ulong value)
    {
        public MemProperties Property { get; } = property;
        public ulong Value { get; } = value;
    }

    public static unsafe class UnifiedSharedMemoryExtensions
    { 
        #region Context USM Allocation Methods

        public static void* AllocateHostMemory(
            this Context context,
            nuint size,
            uint alignment = 0,
            ReadOnlySpan<UsmConfig> properties = default)
        {
            var propArray = stackalloc ulong[properties.Length * 2 + 1];
            for (int i = 0, pos = 0; i < properties.Length; i++)
            {
                propArray[pos++] = (ulong)properties[i].Property;
                propArray[pos++] = properties[i].Value;
                propArray[pos] = 0;
            }

            var ptr = UnifiedSharedMemoryNative.HostMemAlloc(
                context.Handle, propArray, size, alignment, out var errorCode);

            errorCode.ThrowIfUnsuccessful();
            return ptr;
        }

        public static void* AllocateDeviceMemory(
            this Context context,
            Device device,
            nuint size,
            uint alignment = 0,
            ReadOnlySpan<UsmConfig> properties = default)
        {
            var propArray = stackalloc ulong[properties.Length * 2 + 1];
            for (int i = 0, pos = 0; i < properties.Length; i++)
            {
                propArray[pos++] = (ulong)properties[i].Property;
                propArray[pos++] = properties[i].Value;
                propArray[pos] = 0;
            }

            var ptr = UnifiedSharedMemoryNative.DeviceMemAlloc(
                context.Handle, device.Handle, propArray, size, alignment, out var errorCode);

            errorCode.ThrowIfUnsuccessful();
            return ptr;
        }

        public static void* AllocateSharedMemory(
            this Context context,
            Device device,
            nuint size,
            uint alignment = 0,
            ReadOnlySpan<UsmConfig> properties = default)
        {
            var propArray = stackalloc ulong[properties.Length * 2 + 1];
            for (int i = 0, pos = 0; i < properties.Length; i++)
            {
                propArray[pos++] = (ulong)properties[i].Property;
                propArray[pos++] = properties[i].Value;
                propArray[pos] = 0;
            }

            var ptr = UnifiedSharedMemoryNative.SharedMemAlloc(
                context.Handle, device.Handle, propArray, size, alignment, out var errorCode);

            errorCode.ThrowIfUnsuccessful();
            return ptr;
        }

        public static void FreeMemory(this Context context, void* ptr)
        {
            UnifiedSharedMemoryNative.MemFree(context.Handle, ptr)
                .ThrowIfUnsuccessful();
        }

        public static void FreeMemoryBlocking(this Context context, void* ptr)
        {
            UnifiedSharedMemoryNative.MemBlockingFree(context.Handle, ptr)
                .ThrowIfUnsuccessful();
        }

        public static T GetAllocInfo<T>(this Context context, void* ptr, MemAllocInfo paramName)
            where T : unmanaged
        {
            if (TryGetAllocInfo<T>(context, ptr, paramName, out var value))
                return value;
            throw new InvalidOperationException($"Failed to get alloc info {paramName}");
        }

        public static bool TryGetAllocInfo<T>(this Context context, void* ptr, MemAllocInfo paramName, out T value)
            where T : unmanaged
        {
            var val = value = default;
            UnifiedSharedMemoryNative.GetMemAllocInfo(
                context.Handle, ptr, paramName, (nuint)sizeof(T), &val, out var size)
                .ThrowIfUnsuccessful();

            value = val;
            return (int)size == sizeof(T);
        }

        public static UnifiedSharedMemoryType GetAllocationType(this Context context, void* ptr)
            => (UnifiedSharedMemoryType)context.GetAllocInfo<ulong>(ptr, MemAllocInfo.Type);

        #endregion

        #region Kernel USM Argument Methods

        public static void SetArgMemPointer(this Kernel kernel, int argIndex, void* ptr)
        {
            UnifiedSharedMemoryNative.SetKernelArgMemPointer(
                kernel.Handle, (uint)argIndex, ptr)
                .ThrowIfUnsuccessful();
        }

        #endregion

        #region Command Queue USM Operations

        public static Event EnqueueMemFill<T>(
            this CommandQueue queue,
            void* dstPtr,
            T pattern,
            nuint size,
            ReadOnlySpan<Event> waitEvents = default)
            where T : unmanaged
        {
            return queue.EnqueueMemFill(dstPtr, [pattern], size, waitEvents);
        }

        public static Event EnqueueMemFill<T>(
            this CommandQueue queue,
            void* dstPtr,
            ReadOnlySpan<T> pattern,
            nuint size,
            ReadOnlySpan<Event> waitEvents = default)
            where T : unmanaged
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++)
                eventHandles[i] = waitEvents[i].Handle;

            fixed (T* patternPtr = pattern)
            {
                UnifiedSharedMemoryNative.EnqueueMemFill(
                    queue.Handle, dstPtr, patternPtr,
                    (nuint)(sizeof(T) * pattern.Length), size,
                    (uint)waitEvents.Length, eventHandles, out var eventHandle)
                    .ThrowIfUnsuccessful();

                return Event.Reify(eventHandle);
            }
        }

        public static Event EnqueueMemcpy(
            this CommandQueue queue,
            bool blocking,
            void* dstPtr,
            void* srcPtr,
            nuint size,
            ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++)
                eventHandles[i] = waitEvents[i].Handle;

            UnifiedSharedMemoryNative.EnqueueMemcpy(
                queue.Handle, blocking, dstPtr, srcPtr, size,
                (uint)waitEvents.Length, eventHandles, out var eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        public static Event EnqueueMigrateMem(
            this CommandQueue queue,
            void* ptr,
            nuint size,
            MemMigrationFlags flags,
            ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++)
                eventHandles[i] = waitEvents[i].Handle;

            UnifiedSharedMemoryNative.EnqueueMigrateMem(
                queue.Handle, ptr, size, flags,
                (uint)waitEvents.Length, eventHandles, out var eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        public static Event EnqueueMemAdvise(
            this CommandQueue queue,
            void* ptr,
            nuint size,
            MemAdvice advice,
            ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++)
                eventHandles[i] = waitEvents[i].Handle;

            UnifiedSharedMemoryNative.EnqueueMemAdvise(
                queue.Handle, ptr, size, advice,
                (uint)waitEvents.Length, eventHandles, out var eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        #endregion

        #region Context Extensions

        public static bool IsDeviceMemory(this Context context, void* ptr)
            => context.GetAllocationType(ptr) == UnifiedSharedMemoryType.Device;

        public static bool IsHostMemory(this Context context, void* ptr)
            => context.GetAllocationType(ptr) == UnifiedSharedMemoryType.Host;

        public static bool IsSharedMemory(this Context context, void* ptr)
            => context.GetAllocationType(ptr) == UnifiedSharedMemoryType.Shared;

        public static void* GetAllocationBasePtr(this Context context, void* ptr)
            => (void*) context.GetAllocInfo<nint>(ptr, MemAllocInfo.BasePtr);

        public static nuint GetAllocationSize(this Context context, void* ptr)
            => context.GetAllocInfo<nuint>(ptr, MemAllocInfo.Size);

        public static Device? GetAllocationDevice(this Context context, void* ptr)
        {
            if (context.TryGetAllocInfo<Handle<Device>>(ptr, MemAllocInfo.Device, out var deviceHandle))
                return deviceHandle.Value != 0 ? Device.Reify(deviceHandle) : null;
            return null;
        }

        #endregion
    }
}

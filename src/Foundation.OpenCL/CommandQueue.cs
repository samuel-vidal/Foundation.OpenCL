using System;
using System.Runtime.InteropServices;

namespace Foundation.OpenCL
{
    #region Constants

    public enum CommandQueueInfo
    {
        Context = 0x1090,                  // CL_QUEUE_CONTEXT
        Device = 0x1091,                   // CL_QUEUE_DEVICE
        ReferenceCount = 0x1092,           // CL_QUEUE_REFERENCE_COUNT
        Properties = 0x1093,               // CL_QUEUE_PROPERTIES
        Size = 0x1094,                     // CL_QUEUE_SIZE
        DeviceDefault = 0x1095,            // CL_QUEUE_DEVICE_DEFAULT
        PropertiesArray = 0x1098,          // CL_QUEUE_PROPERTIES_ARRAY
    }

    public enum CommandQueueProperty : ulong
    {
        None = 0x0,
        OutOfOrderExecModeEnable = 0x1,                   // CL_QUEUE_OUT_OF_ORDER_EXEC_MODE_ENABLE
        ProfilingEnable = 0x2,                            // CL_QUEUE_PROFILING_ENABLE
        OnDevice = 0x4,                                   // CL_QUEUE_ON_DEVICE
        OnDeviceDefault = 0x8,                            // CL_QUEUE_ON_DEVICE_DEFAULT
        ThreadLocalExecEnableIntel = 0x80000000,          // CL_QUEUE_THREAD_LOCAL_EXEC_ENABLE_INTEL
        NoSyncOperationsIntel = 0x20000000,               // CL_QUEUE_NO_SYNC_OPERATIONS_INTEL
    }

    [Flags]
    public enum MemMigrationFlags : ulong
    {
        None = 0x0,
        Host = 0x1,                      // CL_MIGRATE_MEM_OBJECT_HOST
        ContentUndefined = 0x2,          // CL_MIGRATE_MEM_OBJECT_CONTENT_UNDEFINED
    }

    [Flags]
    public enum MapFlags : ulong
    {
        None = 0x0,
        Read = 0x1,                           // CL_MAP_READ
        Write = 0x2,                          // CL_MAP_WRITE
        WriteInvalidateRegion = 0x4,          // CL_MAP_WRITE_INVALIDATE_REGION
    }

    #endregion

    public readonly struct CommandQueueConfig(CommandQueueProperty property, ulong value)
    {
        public CommandQueueProperty Property { get; } = property;
        public ulong Value { get; } = value;
    }

    public sealed unsafe class CommandQueue(Handle<CommandQueue> handle)
        : BaseObject<CommandQueue, CommandQueueInfo>(handle), IReify<CommandQueue>
    {
        public void Flush() => OpenCLNative.Flush(Handle).ThrowIfUnsuccessful();

        public void Finish() => OpenCLNative.Finish(Handle).ThrowIfUnsuccessful();

        #region Buffer Operations

        public Event EnqueueReadBuffer(Buffer buffer, nuint offset, nuint size, void* hostPtr, ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            Handle<Event> eventHandle;
            OpenCLNative.EnqueueReadBuffer(
                    Handle, buffer.Handle, false, offset, size, hostPtr,
                    waitEvents.Length,
                    NullIfEmpty(waitEvents, eventHandles),
                    &eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        public void EnqueueReadBufferBlocking(Buffer buffer, nuint offset, nuint size, void* hostPtr, ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueReadBuffer(
                    Handle, buffer.Handle, true, offset, size, hostPtr,
                    waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), null)
                .ThrowIfUnsuccessful();
        }

        public Event EnqueueWriteBuffer(Buffer buffer, nuint offset, nuint size, void* hostPtr, ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;


            Handle<Event> eventHandle;
            OpenCLNative.EnqueueWriteBuffer(
                    Handle, buffer.Handle, false, offset, size, hostPtr,
                    waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), &eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        public void EnqueueWriteBufferBlocking(Buffer buffer, nuint offset, nuint size, void* hostPtr, ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueWriteBuffer(
                    Handle, buffer.Handle, true, offset, size, hostPtr,
                    waitEvents.Length, eventHandles, null)
                .ThrowIfUnsuccessful();
        }

        public Event EnqueueCopyBuffer(Buffer source, Buffer destination, nuint srcOffset, nuint dstOffset, nuint size, ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueCopyBuffer(
                Handle, source.Handle, destination.Handle, srcOffset, dstOffset, size,
                waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), out var eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        public Event EnqueueFillBuffer<T>(
            Buffer buffer,
            T pattern,
            nuint offset,
            nuint size,
            ReadOnlySpan<Event> waitEvents = default)
            where T : unmanaged
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

                OpenCLNative.EnqueueFillBuffer(
                    Handle, buffer.Handle, &pattern, (nuint)sizeof(T), offset, size,
                    waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), out var eventHandle)
                    .ThrowIfUnsuccessful();

                return Event.Reify(eventHandle);
        }

        #region Rectangular Buffer Operations

        public Event EnqueueReadBufferRect(
            Buffer memObject,
            ReadOnlySpan<nuint> targetOrigin, // host
            ReadOnlySpan<nuint> sourceOrigin, // buffer
            ReadOnlySpan<nuint> region,
            ReadOnlySpan<nuint> targetPitch,
            ReadOnlySpan<nuint> sourcePitch,
            void* hostPtr,
            ReadOnlySpan<Event> waitEvents = default)
        {
            return EnqueueReadBufferRectImplementation(memObject, false, targetOrigin, sourceOrigin, region,
                targetPitch, sourcePitch, hostPtr, waitEvents)!;
        }

        public void EnqueueReadBufferRectBlocking(
            Buffer memObject,
            ReadOnlySpan<nuint> targetOrigin, // host
            ReadOnlySpan<nuint> sourceOrigin, // buffer
            ReadOnlySpan<nuint> region,
            ReadOnlySpan<nuint> targetPitch,
            ReadOnlySpan<nuint> sourcePitch,
            void* hostPtr,
            ReadOnlySpan<Event> waitEvents = default)
        {
            EnqueueReadBufferRectImplementation(memObject, true, targetOrigin, sourceOrigin, region,
                targetPitch, sourcePitch, hostPtr, waitEvents);
        }

        private Event? EnqueueReadBufferRectImplementation(
            Buffer memObject,
            bool blocking,
            ReadOnlySpan<nuint> targetOrigin, // host
            ReadOnlySpan<nuint> sourceOrigin, // buffer
            ReadOnlySpan<nuint> region,
            ReadOnlySpan<nuint> targetPitch,
            ReadOnlySpan<nuint> sourcePitch,
            void* hostPtr,
            ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            if (targetOrigin.Length > 3) throw new ArgumentException("Length cannot be greater than 3.", nameof(targetOrigin));
            if (sourceOrigin.Length > 3) throw new ArgumentException("Length cannot be greater than 3.", nameof(sourceOrigin));
            if (region.Length > 3) throw new ArgumentException("Length cannot be greater than 3.", nameof(region));

            var bufferOriginPtr = stackalloc nuint[3];
            var hostOriginPtr = stackalloc nuint[3];
            var regionPtr = stackalloc nuint[3];
            nuint bufferRowPitch = 0;
            nuint bufferSlicePitch = 0;
            nuint hostRowPitch = 0;
            nuint hostSlicePitch = 0;

            for (var i = 0; i < 3; i++)
            {
                hostOriginPtr[i] = i < targetOrigin.Length ? targetOrigin[i] : 0;
                bufferOriginPtr[i] = i < sourceOrigin.Length ? sourceOrigin[i] : 0;
                regionPtr[i] = i < region.Length ? region[i] : 1;
            }

            if (targetPitch.Length > 0) hostRowPitch = targetPitch[0];
            if (targetPitch.Length > 1) hostSlicePitch = targetPitch[1];

            if (sourcePitch.Length > 0) bufferRowPitch = sourcePitch[0];
            if (sourcePitch.Length > 1) bufferSlicePitch = sourcePitch[1];

            var eventHandle = Handle<Event>.Null;
            OpenCLNative.EnqueueReadBufferRect(
                    Handle, memObject.Handle, blocking,
                    bufferOriginPtr, hostOriginPtr, regionPtr,
                    bufferRowPitch, bufferSlicePitch,
                    hostRowPitch, hostSlicePitch,
                    hostPtr,
                    waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), blocking ? null : &eventHandle)
                .ThrowIfUnsuccessful();

            return blocking ? null : Event.Reify(eventHandle);
        }

        public Event EnqueueWriteBufferRect(
            Buffer memObject,
            ReadOnlySpan<nuint> targetOrigin, // buffer
            ReadOnlySpan<nuint> sourceOrigin, // host
            ReadOnlySpan<nuint> region,
            ReadOnlySpan<nuint> targetPitches,
            ReadOnlySpan<nuint> sourcePitch,
            void* hostPtr,
            ReadOnlySpan<Event> waitEvents = default)
        {
            return EnqueueWriteBufferRectImplementation(memObject, false, targetOrigin, sourceOrigin, region,
                targetPitches, sourcePitch, hostPtr, waitEvents)!;
        }

        public void EnqueueWriteBufferRectBlocking(
            Buffer memObject,
            ReadOnlySpan<nuint> targetOrigin, // buffer
            ReadOnlySpan<nuint> sourceOrigin, // host
            ReadOnlySpan<nuint> region,
            ReadOnlySpan<nuint> targetPitch,
            ReadOnlySpan<nuint> sourcePitch,
            void* hostPtr,
            ReadOnlySpan<Event> waitEvents = default)
        {
            EnqueueWriteBufferRectImplementation(memObject, true, targetOrigin, sourceOrigin, region,
                targetPitch, sourcePitch, hostPtr, waitEvents);
        }

        private Event? EnqueueWriteBufferRectImplementation(
            Buffer memObject,
            bool blocking,
            ReadOnlySpan<nuint> targetOrigin, // buffer
            ReadOnlySpan<nuint> sourceOrigin, // host
            ReadOnlySpan<nuint> region,
            ReadOnlySpan<nuint> targetPitch,
            ReadOnlySpan<nuint> sourcePitch,
            void* hostPtr,
            ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            if (targetOrigin.Length > 3) throw new ArgumentException("Length cannot be greater than 3.", nameof(targetOrigin));
            if (sourceOrigin.Length > 3) throw new ArgumentException("Length cannot be greater than 3.", nameof(sourceOrigin));
            if (region.Length > 3) throw new ArgumentException("Length cannot be greater than 3.", nameof(region));

            var bufferOriginPtr = stackalloc nuint[3];
            var hostOriginPtr = stackalloc nuint[3];
            var regionPtr = stackalloc nuint[3];
            nuint bufferRowPitch = 0;
            nuint bufferSlicePitch = 0;
            nuint hostRowPitch = 0;
            nuint hostSlicePitch = 0;

            for (var i = 0; i < 3; i++)
            {
                bufferOriginPtr[i] = i < targetOrigin.Length ? targetOrigin[i] : 0;
                hostOriginPtr[i] = i < sourceOrigin.Length ? sourceOrigin[i] : 0;
                regionPtr[i] = i < region.Length ? region[i] : 1;
            }

            if (targetPitch.Length > 0) bufferRowPitch = targetPitch[0];
            if (targetPitch.Length > 1) bufferSlicePitch = targetPitch[1];

            if (sourcePitch.Length > 0) hostRowPitch = sourcePitch[0];
            if (sourcePitch.Length > 1) hostSlicePitch = sourcePitch[1];

            var eventHandle = Handle<Event>.Null;
            OpenCLNative.EnqueueWriteBufferRect(
                    Handle, memObject.Handle, blocking,
                    bufferOriginPtr, hostOriginPtr, regionPtr,
                    bufferRowPitch, bufferSlicePitch,
                    hostRowPitch, hostSlicePitch,
                    hostPtr,
                    waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), blocking ? null : &eventHandle)
                .ThrowIfUnsuccessful();

            return blocking ? null : Event.Reify(eventHandle);
        }

        public Event EnqueueCopyBufferRect(
            Buffer targetBuffer,
            Buffer sourceBuffer,
            ReadOnlySpan<nuint> targetOrigin,
            ReadOnlySpan<nuint> sourceOrigin,
            ReadOnlySpan<nuint> region,
            ReadOnlySpan<nuint> targetPitch,
            ReadOnlySpan<nuint> sourcePitch,
            ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            if (targetOrigin.Length > 3) throw new ArgumentException("Length cannot be greater than 3.", nameof(targetOrigin));
            if (sourceOrigin.Length > 3) throw new ArgumentException("Length cannot be greater than 3.", nameof(sourceOrigin));
            if (region.Length > 3) throw new ArgumentException("Length cannot be greater than 3.", nameof(region));

            var srcOriginPtr = stackalloc nuint[3];
            var dstOriginPtr = stackalloc nuint[3];
            var regionPtr = stackalloc nuint[3];
            nuint srcRowPitch = 0;
            nuint srcSlicePitch = 0;
            nuint dstRowPitch = 0;
            nuint dstSlicePitch = 0;

            for (var i = 0; i < 3; i++)
            {
                srcOriginPtr[i] = i < targetOrigin.Length ? targetOrigin[i] : 0;
                dstOriginPtr[i] = i < sourceOrigin.Length ? sourceOrigin[i] : 0;
                regionPtr[i] = i < region.Length ? region[i] : 1;
            }

            if (targetPitch.Length > 0) srcRowPitch = targetPitch[0];
            if (targetPitch.Length > 1) srcSlicePitch = targetPitch[1];

            if (sourcePitch.Length > 0) dstRowPitch = sourcePitch[0];
            if (sourcePitch.Length > 1) dstSlicePitch = sourcePitch[1];

            OpenCLNative.EnqueueCopyBufferRect(
                    Handle, sourceBuffer.Handle, targetBuffer.Handle,
                    srcOriginPtr,
                    dstOriginPtr,
                    regionPtr,
                    srcRowPitch,
                    srcSlicePitch,
                    dstRowPitch,
                    dstSlicePitch,
                    waitEvents.Length,
                    NullIfEmpty(waitEvents, eventHandles),
                    out var eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        #endregion

        #endregion

        #region Image Operations

        public Event EnqueueReadImage(Image image, nuint* origin, nuint* region, nuint rowPitch, nuint slicePitch, void* ptr, ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            Handle<Event> eventHandle;
            OpenCLNative.EnqueueReadImage(
                    Handle, image.Handle, false, origin, region, rowPitch, slicePitch, ptr,
                    waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), & eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        public void EnqueueReadImageBlocking(Image image, nuint* origin, nuint* region, nuint rowPitch, nuint slicePitch, void* ptr, ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueReadImage(
                    Handle, image.Handle, true, origin, region, rowPitch, slicePitch, ptr,
                    waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), null)
                .ThrowIfUnsuccessful();
        }

        public Event EnqueueWriteImage(Image image, nuint* origin, nuint* region, nuint rowPitch, nuint slicePitch, void* ptr, ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            Handle<Event> eventHandle;
            OpenCLNative.EnqueueWriteImage(
                    Handle, image.Handle, false, origin, region, rowPitch, slicePitch, ptr,
                    waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), &eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        public void EnqueueWriteImageBlocking(Image image, nuint* origin, nuint* region, nuint rowPitch, nuint slicePitch, void* ptr, ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueWriteImage(
                    Handle, image.Handle, true, origin, region, rowPitch, slicePitch, ptr,
                    waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), null)
                .ThrowIfUnsuccessful();
        }

        public Event EnqueueCopyImage(Image source, Image destination, nuint* srcOrigin, nuint* dstOrigin, nuint* region, ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueCopyImage(
                Handle, source.Handle, destination.Handle, srcOrigin, dstOrigin, region,
                waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), out var eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        public Event EnqueueFillImage(Image image, void* fillColor, nuint* origin, nuint* region, ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueFillImage(
                Handle, image.Handle, fillColor, origin, region,
                waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), out var eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        #endregion

        #region Mapping Operations

        public void* EnqueueMapBuffer(Buffer buffer, MapFlags flags, nuint offset, nuint size, out Event mapEvent, ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            Handle<Event> eventHandle;
            var ptr = OpenCLNative.EnqueueMapBuffer(
                Handle, buffer.Handle, false, flags, offset, size,
                waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), &eventHandle, out var errorCode);

            errorCode.ThrowIfUnsuccessful();
            mapEvent = Event.Reify(eventHandle);
            return ptr;
        }

        public void* EnqueueMapBufferBlocking(Buffer buffer, bool blocking, MapFlags flags, nuint offset, nuint size, ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            var ptr = OpenCLNative.EnqueueMapBuffer(
                Handle, buffer.Handle, blocking, flags, offset, size,
                waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), null, out var errorCode);

            errorCode.ThrowIfUnsuccessful();
            return ptr;
        }

        public void* EnqueueMapImage(Image image, MapFlags flags, nuint* origin, nuint* region, out nuint rowPitch, out nuint slicePitch, out Event mapEvent, ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            Handle<Event> eventHandle;
            var ptr = OpenCLNative.EnqueueMapImage(
                Handle, image.Handle, false, flags, origin, region,
                out rowPitch, out slicePitch, waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), &eventHandle, out var errorCode);

            errorCode.ThrowIfUnsuccessful();
            mapEvent = Event.Reify(eventHandle);
            return ptr;
        }

        public void* EnqueueMapImageBlocking(Image image, MapFlags flags, nuint* origin, nuint* region, out nuint rowPitch, out nuint slicePitch, ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            var ptr = OpenCLNative.EnqueueMapImage(
                Handle, image.Handle, true, flags, origin, region,
                out rowPitch, out slicePitch, waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), null, out var errorCode);

            errorCode.ThrowIfUnsuccessful();
            return ptr;
        }

        public Event EnqueueUnmapMemObject<TMemObject>(TMemObject memObject, void* mappedPtr, ReadOnlySpan<Event> waitEvents = default)
            where TMemObject : BaseMemoryObject<TMemObject>, IReify<TMemObject, MemoryObject>
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueUnmapMemObject(
                Handle, memObject.Handle, mappedPtr,
                waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), out var eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        #endregion

        #region Migration & SVM Operations

        public Event EnqueueMigrateMemObjects<TMemObject>(ReadOnlySpan<TMemObject> memObjects, MemMigrationFlags flags, ReadOnlySpan<Event> waitEvents = default)
            where TMemObject : BaseMemoryObject<TMemObject>, IReify<TMemObject, MemoryObject>
        {
            var memHandles = stackalloc Handle<MemoryObject>[memObjects.Length];
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];

            for (var i = 0; i < memObjects.Length; i++) memHandles[i] = memObjects[i].Handle;
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueMigrateMemObjects(
                Handle, memObjects.Length, memHandles, flags,
                waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), out var eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        #endregion

        #region Kernel Execution

        public Event EnqueueNdRangeKernel(
            Kernel kernel,
            ReadOnlySpan<nuint> globalWorkOffset,
            ReadOnlySpan<nuint> globalWorkSize,
            ReadOnlySpan<nuint> localWorkSize,
            ReadOnlySpan<Event> waitEvents = default)
        {
            var workDim = globalWorkSize.Length;

            // 1. Validate inputs only if they are provided
            if (!globalWorkOffset.IsEmpty && globalWorkOffset.Length != workDim)
                throw new ArgumentException("Global offset dimension must match work dimension", nameof(globalWorkOffset));

            if (!localWorkSize.IsEmpty && localWorkSize.Length != workDim)
                throw new ArgumentException("Local work size dimension must match work dimension", nameof(localWorkSize));

            // 2. Prepare events
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            // Pin the work size arrays
            fixed (nuint* globalOffsetPtr = globalWorkOffset)
            fixed (nuint* globalSizePtr = globalWorkSize)
            fixed (nuint* localSizePtr = localWorkSize)
            {
                OpenCLNative.EnqueueNdRangeKernel(
                    Handle,
                    kernel.Handle,
                    workDim,
                    // Explicitly pass NULL if the span is empty to prevent OpenCL 
                    // from reading 'workDim' items from a valid-but-empty pointer.
                    NullIfEmpty(globalWorkOffset, globalOffsetPtr),
                    globalSizePtr,
                    NullIfEmpty(localWorkSize, localSizePtr),
                    waitEvents.Length,
                    NullIfEmpty(waitEvents, eventHandles),
                    out var eventHandle).ThrowIfUnsuccessful();

                return Event.Reify(eventHandle);
            }
        }

        #endregion

        #region Native Kernel Execution

        public Context GetContext()
            => Context.Reify(GetContextHandle());

        public Handle<Context> GetContextHandle()
            => GetInfo<Handle<Context>>(CommandQueueInfo.Context);

        public Event EnqueueNativeKernel<TMemObject>(
            Action<Span<IntPtr>> hostKernel,
            ReadOnlySpan<TMemObject> memObjects,
            ReadOnlySpan<Event> waitEvents = default)
            where TMemObject : BaseMemoryObject<TMemObject>, IReify<TMemObject, MemoryObject>
        {
            var numMemObjects = memObjects.Length;

            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            var memHandles = stackalloc Handle<MemoryObject>[memObjects.Length];
            var argsBuffer = stackalloc Handle<MemoryObject>[memObjects.Length];
            var memLocations = stackalloc void*[memObjects.Length];

            for (var i = 0; i < memObjects.Length; i++)
            {
                memHandles[i] = memObjects[i].Handle;
                argsBuffer[i] = memObjects[i].Handle;
                memLocations[i] = &argsBuffer[i];
            }

            var callback = (void** pointers) =>
            {
                try
                {
                    hostKernel(new Span<IntPtr>(pointers, numMemObjects));
                } catch { }
            };

            var funcPtr = (void*)Marshal.GetFunctionPointerForDelegate(callback);

            OpenCLNative.EnqueueNativeKernel(
                    Handle, funcPtr, argsBuffer, (nuint) (memObjects.Length*sizeof(nint)),
                    memObjects.Length, memHandles, memLocations,
                    waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), out var eventHandle)
                .ThrowIfUnsuccessful();

            var completionEvent = Event.Reify(eventHandle);

            var managed = GCHandle.Alloc(callback, GCHandleType.Pinned);
            completionEvent.OnDispose += () => managed.Free();

            return completionEvent;
        }

        #endregion

        #region Marker & Barrier Operations

        public Event EnqueueMarkerWithWaitList(ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueMarkerWithWaitList(
                Handle, waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), out var eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        public Event EnqueueBarrierWithWaitList(ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueBarrierWithWaitList(
                Handle, waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), out var eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        #endregion

        #region SVM Operations (Complete Implementation)

        public Event EnqueueSvmFree(
            ReadOnlySpan<IntPtr> svmPointers,
            Action? freeCallback = null,
            ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            var pointers = stackalloc void*[svmPointers.Length];
            for (var i = 0; i < svmPointers.Length; i++) pointers[i] = (void*)svmPointers[i];

            void* callbackPtr = null;
            if (freeCallback != null)
            {
                void Callback(Handle<CommandQueue> queue, int numPointers, void** pointers, void* userData)
                {
                    try { freeCallback(); } catch { }
                }
                callbackPtr = (void*)Marshal.GetFunctionPointerForDelegate(Callback);
            }

            OpenCLNative.EnqueueSvmFree(
                Handle, svmPointers.Length, pointers, callbackPtr, null,
                waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), out var eventHandle)
                .ThrowIfUnsuccessful();

            var completionEvent = Event.Reify(eventHandle);

            if (freeCallback != null)
            {
                var managed = GCHandle.Alloc(freeCallback);
                completionEvent.OnDispose += () => managed.Free();
            }

            return completionEvent;
        }

        public Event EnqueueSvmMemcpy(
            bool blocking,
            void* dstPtr,
            void* srcPtr,
            nuint size,
            ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueSvmMemcpy(
                Handle, blocking, dstPtr, srcPtr, size,
                waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), out var eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        public Event EnqueueSvmMemFill<T>(
            void* svmPtr,
            T pattern,
            nuint size,
            ReadOnlySpan<Event> waitEvents = default)
            where T : unmanaged
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueSvmMemFill(
                    Handle, svmPtr, &pattern, (nuint)sizeof(T), size,
                    waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), out var eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        public Event EnqueueSvmMemFill<T>(
            void* svmPtr,
            Span<T> pattern,
            nuint size,
            ReadOnlySpan<Event> waitEvents = default) where T : unmanaged
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            fixed(T * ptr = pattern)
            {
                OpenCLNative.EnqueueSvmMemFill(
                    Handle, svmPtr, ptr, (nuint)(sizeof(T) * pattern.Length), size,
                    waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), out var eventHandle)
                .ThrowIfUnsuccessful();

                return Event.Reify(eventHandle);
            }
        }

        public Event EnqueueSvmMap(
            MapFlags flags,
            void* svmPtr,
            nuint size,
            ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            Handle<Event> eventHandle;
            OpenCLNative.EnqueueSvmMap(
                    Handle, false, flags, svmPtr, size,
                    waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), &eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        public void EnqueueSvmMapBlocking(
            MapFlags flags,
            void* svmPtr,
            nuint size,
            ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueSvmMap(
                    Handle, true, flags, svmPtr, size,
                    waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), null)
                .ThrowIfUnsuccessful();
        }

        public Event EnqueueSvmUnmap(
            void* svmPtr,
            ReadOnlySpan<Event> waitEvents = default)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueSvmUnmap(
                Handle, svmPtr, waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), out var eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        public Event EnqueueSvmMigrateMem(
            ReadOnlySpan<IntPtr> svmPointers,
            ReadOnlySpan<nuint> sizes,
            MemMigrationFlags flags,
            ReadOnlySpan<Event> waitEvents = default)
        {
            if (svmPointers.Length != sizes.Length)
                throw new ArgumentException("Pointers and sizes arrays must have same length");

            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            var pointers = stackalloc void*[svmPointers.Length];
            for (var i = 0; i < svmPointers.Length; i++) pointers[i] = (void*)svmPointers[i];

            fixed (nuint* sizesPtr = sizes)
            {
                OpenCLNative.EnqueueSvmMigrateMem(
                    Handle, svmPointers.Length, pointers, sizesPtr, flags,
                    waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), out var eventHandle)
                    .ThrowIfUnsuccessful();

                return Event.Reify(eventHandle);
            }
        }

        #endregion

        protected override void GetInfo(CommandQueueInfo paramName, nuint paramValueSize, void* paramValue, out nuint paramValueSizeRet)
            => OpenCLNative.GetCommandQueueInfo(Handle, paramName, paramValueSize, paramValue, out paramValueSizeRet).ThrowIfUnsuccessful();

        protected override void RetainHook() => OpenCLNative.RetainCommandQueue(Handle).ThrowIfUnsuccessful();
        protected override void ReleaseHook(Handle<CommandQueue> tmpHandle) => OpenCLNative.ReleaseCommandQueue(tmpHandle).ThrowIfUnsuccessful();

        public static CommandQueue Reify(Handle<CommandQueue> handle) => new(handle);
    }

    public static class QueueExtensions
    {
        public static int GetReferenceCount(this CommandQueue queue)
            => queue.GetInfo<int>(CommandQueueInfo.ReferenceCount);

        public static Context GetContext(this CommandQueue queue)
            => Context.Reify(queue.GetInfo<Handle<Context>>(CommandQueueInfo.Context));

        public static Device GetDevice(this CommandQueue queue)
            => Device.Reify(queue.GetInfo<Handle<Device>>(CommandQueueInfo.Device));

        public static CommandQueueProperty GetProperties(this CommandQueue queue)
            => queue.GetInfo<CommandQueueProperty>(CommandQueueInfo.Properties);
    }

    public readonly ref struct SubTensorLayout<T> where T : unmanaged
    {
        public readonly ReadOnlySpan<nuint> Origin;
        public readonly ReadOnlySpan<nuint> Dimension;
        public readonly ReadOnlySpan<nuint> Stride;

        public SubTensorLayout(
            ReadOnlySpan<nuint> origin,
            ReadOnlySpan<nuint> dimension,
            ReadOnlySpan<nuint> stride)
        {
            Origin = origin;
            Dimension = dimension;
            Stride = stride;

            Validate();
        }

        public int Rank => Dimension.Length;

        private void Validate()
        {
            if (Origin.Length != Rank) throw new ArgumentException("Size mismatch Origin.Length != Dimension.Length"); 
            if (Stride.Length != Rank) throw new ArgumentException("Size mismatch Stride.Length != Dimension.Length");

            if (Stride[0] != 1) throw new ArgumentException("Stride[0] != 1");

            for (int i = 1; i < Rank; i++)
            {
                if (Dimension[i] == 0) throw new ArgumentException($"Dimension[{i}] == 1");
            }

            for (int i = 1; i < Rank; i++)
            {
                if (Dimension[i - 1] * Stride[i - 1] > Stride[i]) throw new ArgumentException($"Inconsistent striding : Dimension[{i-1}] * Stride[{i - 1}] > Stride[{i}]");
            }
        }
    }

    public static unsafe class BufferRectangularExtensions
    {
        public static Event EnqueueReadBufferRect<T>(
            this CommandQueue queue,
            Buffer memObject,
            SubTensorLayout<T> targetLayout,
            SubTensorLayout<T> sourceLayout,
            T* targetHostPtr,
            Span<Event> waitEvents = default)
            where T : unmanaged
        {
            #region Initialize

            Span<nuint> targetOrigin = stackalloc nuint[3];
            Span<nuint> sourceOrigin = stackalloc nuint[3];
            Span<nuint> targetPitch = stackalloc nuint[3];
            Span<nuint> sourcePitch = stackalloc nuint[3];
            Span<nuint> region = stackalloc nuint[3];

            ConvertArguments(targetLayout, sourceLayout, targetOrigin, sourceOrigin, region, targetPitch, sourcePitch);

            #endregion

            return queue.EnqueueReadBufferRect(
                memObject,
                targetOrigin,
                sourceOrigin,
                region,
                targetPitch,
                sourcePitch,
                targetHostPtr,
                waitEvents);
        }

        public static void EnqueueReadBufferRectBlocking<T>(
            this CommandQueue queue,
            Buffer memObject,
            SubTensorLayout<T> targetLayout,
            SubTensorLayout<T> sourceLayout,
            T* targetHostPtr,
            ReadOnlySpan<Event> waitEvents = default)
            where T : unmanaged
        {
            #region Initialize

            Span<nuint> targetOrigin = stackalloc nuint[3];
            Span<nuint> sourceOrigin = stackalloc nuint[3];
            Span<nuint> targetPitch = stackalloc nuint[3];
            Span<nuint> sourcePitch = stackalloc nuint[3];
            Span<nuint> region = stackalloc nuint[3];

            ConvertArguments(targetLayout, sourceLayout, targetOrigin, sourceOrigin, region, targetPitch, sourcePitch);

            #endregion

            queue.EnqueueReadBufferRectBlocking(
                memObject,
                targetOrigin,
                sourceOrigin,
                region,
                targetPitch,
                sourcePitch,
                targetHostPtr,
                waitEvents);
        }

        public static Event EnqueueWriteBufferRect<T>(
            this CommandQueue queue,
            Buffer memObject,
            SubTensorLayout<T> targetLayout,
            SubTensorLayout<T> sourceLayout,
            T* sourceHostPtr,
            ReadOnlySpan<Event> waitEvents = default)
            where T : unmanaged
        {
            #region Initialize

            Span<nuint> targetOrigin = stackalloc nuint[3];
            Span<nuint> sourceOrigin = stackalloc nuint[3];
            Span<nuint> targetPitch = stackalloc nuint[3];
            Span<nuint> sourcePitch = stackalloc nuint[3];
            Span<nuint> region = stackalloc nuint[3];

            ConvertArguments(targetLayout, sourceLayout, targetOrigin, sourceOrigin, region, targetPitch, sourcePitch);

            #endregion

            return queue.EnqueueWriteBufferRect(
                memObject,
                targetOrigin,
                sourceOrigin,
                region,
                targetPitch,
                sourcePitch,
                sourceHostPtr,
                waitEvents);
        }

        public static void EnqueueWriteBufferRectBlocking<T>(
            this CommandQueue queue,
            Buffer memObject,
            SubTensorLayout<T> targetLayout,
            SubTensorLayout<T> sourceLayout,
            T* sourceHostPtr,
            ReadOnlySpan<Event> waitEvents = default)
            where T : unmanaged
        {
            #region Initialize

            Span<nuint> targetOrigin = stackalloc nuint[3];
            Span<nuint> sourceOrigin = stackalloc nuint[3];
            Span<nuint> targetPitch = stackalloc nuint[3];
            Span<nuint> sourcePitch = stackalloc nuint[3];
            Span<nuint> region = stackalloc nuint[3];

            ConvertArguments(targetLayout, sourceLayout, targetOrigin, sourceOrigin, region, targetPitch, sourcePitch);

            #endregion

            queue.EnqueueWriteBufferRectBlocking(
                memObject,
                targetOrigin,
                sourceOrigin,
                region,
                targetPitch,
                sourcePitch,
                sourceHostPtr,
                waitEvents);
        }

        public static Event EnqueueCopyBufferRect<T>(
            this CommandQueue queue,
            Buffer targetBuffer,
            Buffer sourceBuffer,
            SubTensorLayout<T> targetLayout,
            SubTensorLayout<T> sourceLayout,
            ReadOnlySpan<Event> waitEvents = default)
            where T : unmanaged
        {
            #region Initialize

            Span<nuint> targetOrigin = stackalloc nuint[3];
            Span<nuint> sourceOrigin = stackalloc nuint[3];
            Span<nuint> targetPitch = stackalloc nuint[3];
            Span<nuint> sourcePitch = stackalloc nuint[3];
            Span<nuint> region = stackalloc nuint[3];

            ConvertArguments(targetLayout, sourceLayout, targetOrigin, sourceOrigin, region, targetPitch, sourcePitch);

            #endregion

            return queue.EnqueueCopyBufferRect(
                targetBuffer,
                sourceBuffer,
                targetOrigin,
                sourceOrigin,
                region,
                targetPitch,
                sourcePitch,
                waitEvents);
        }

        public static void ConvertArguments<T>(
            SubTensorLayout<T> targetLayout,
            SubTensorLayout<T> sourceLayout,
            Span<nuint> targetOrigin,
            Span<nuint> sourceOrigin,
            Span<nuint> region,
            Span<nuint> targetPitch,
            Span<nuint> sourcePitch)
            where T : unmanaged
        {
            #region Assumptions

            // dimensions are the same between source and target
            // all ranks are the same (length of individual spans)
            // at least one dimension is > 0

            // stride[0] = 1
            // for all i = 0..rank-1, we have stride[i] * dimension[i] <= stride[i+1]
            // ie consistent increasing strides (column major)

            if (sourceLayout.Rank != targetLayout.Rank) throw new InvalidOperationException("mismatch between source and target ranks");

            for (var i = 0; i < sourceLayout.Rank; i++)
            {
                if (sourceLayout.Dimension[i] != targetLayout.Dimension[i])
                    throw new InvalidOperationException("mismatch between source and target dimensions");
            }

            if (targetLayout.Stride[0] != 1)
                throw new InvalidOperationException("target stride[0] should be 1");

            if (sourceLayout.Stride[0] != 1)
                throw new InvalidOperationException("source stride[0] should be 1");

            #endregion

            #region Initialize

            for (var i = 0; i < 3; i++)
            {
                targetOrigin[i] = 0;
                sourceOrigin[i] = 0;
                targetPitch[i] = 0;
                sourcePitch[i] = 0;
                region[i] = 1;
            }

            var effectiveRank = 0;

            nuint currentDimension = 1;

            nuint currentTargetOrigin = 0;
            nuint currentTargetStride = 1;

            nuint currentSourceOrigin = 0;
            nuint currentSourceStride = 1;

            #endregion

            for (var i = 0; i < sourceLayout.Rank; i++)
            {
                if (effectiveRank >= 3) throw new InvalidOperationException("Too jagged");

                currentDimension *= targetLayout.Dimension[i];

                currentTargetOrigin += targetLayout.Origin[i];
                currentTargetStride *= targetLayout.Stride[i];

                currentSourceOrigin += sourceLayout.Origin[i];
                currentSourceStride *= sourceLayout.Stride[i];

                if (i + 1 < sourceLayout.Dimension.Length &&      // not the last
                    targetLayout.Dimension[i] * targetLayout.Stride[i] == targetLayout.Stride[i + 1] &&
                    sourceLayout.Dimension[i] * sourceLayout.Stride[i] == sourceLayout.Stride[i + 1])
                {
                    // absorb
                }
                else
                {
                    // commit

                    region[effectiveRank] = currentDimension;

                    targetOrigin[effectiveRank] = currentTargetOrigin;
                    if (effectiveRank > 0) targetPitch[effectiveRank - 1] = currentTargetStride;

                    sourceOrigin[effectiveRank] = currentSourceOrigin;
                    if (effectiveRank > 0) sourcePitch[effectiveRank - 1] = currentSourceStride;

                    effectiveRank++;

                    currentDimension = 1;

                    currentTargetOrigin = 0;
                    currentTargetStride = 1;

                    currentSourceOrigin = 0;
                    currentSourceStride = 1;
                }
            }

            #region Convert to bytes

            region[0] *= (nuint)sizeof(T);
            targetOrigin[0] *= (nuint)sizeof(T);
            sourceOrigin[0] *= (nuint)sizeof(T);
            for (var i = 0; i < 3; i++)
            {
                targetPitch[i] *= (nuint)sizeof(T);
                sourcePitch[i] *= (nuint)sizeof(T);
            }

            #endregion
        }
    }
}
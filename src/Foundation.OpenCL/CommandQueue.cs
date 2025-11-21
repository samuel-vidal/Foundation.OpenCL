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

        public void EnqueueReadBufferBlocking(Buffer buffer, nuint offset, nuint size, void* hostPtr, params ReadOnlySpan<Event> waitEvents)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueReadBuffer(
                    Handle, buffer.Handle, true, offset, size, hostPtr,
                    waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), null)
                .ThrowIfUnsuccessful();
        }

        public Event EnqueueWriteBuffer(Buffer buffer, nuint offset, nuint size, void* hostPtr, params ReadOnlySpan<Event> waitEvents)
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

        public void EnqueueWriteBufferBlocking(Buffer buffer, nuint offset, nuint size, void* hostPtr, params ReadOnlySpan<Event> waitEvents)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueWriteBuffer(
                    Handle, buffer.Handle, true, offset, size, hostPtr,
                    waitEvents.Length, eventHandles, null)
                .ThrowIfUnsuccessful();
        }

        public Event EnqueueCopyBuffer(Buffer source, Buffer destination, nuint srcOffset, nuint dstOffset, nuint size, params ReadOnlySpan<Event> waitEvents)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueCopyBuffer(
                Handle, source.Handle, destination.Handle, srcOffset, dstOffset, size,
                waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), out var eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        public Event EnqueueFillBuffer<T>(Buffer buffer, T pattern, nuint offset, nuint size, params ReadOnlySpan<Event> waitEvents) where T : unmanaged
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

        //public Event EnqueueReadBufferRect(
        //    Buffer buffer,
        //    bool blocking,
        //    ReadOnlySpan<nuint> bufferOrigin,
        //    ReadOnlySpan<nuint> hostOrigin,
        //    ReadOnlySpan<nuint> region,
        //    nuint bufferRowPitch,
        //    nuint bufferSlicePitch,
        //    nuint hostRowPitch,
        //    nuint hostSlicePitch,
        //    void* hostPtr,
        //    ReadOnlySpan<Event> waitEvents = default)
        //{
        //    var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
        //    for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

        //    fixed (nuint* bufferOriginPtr = bufferOrigin)
        //    fixed (nuint* hostOriginPtr = hostOrigin)
        //    fixed (nuint* regionPtr = region)
        //    {
        //        Handle<Event> eventHandle;
        //        OpenCLNative.EnqueueReadBufferRect(
        //            Handle, buffer.Handle, blocking,
        //            bufferOriginPtr, hostOriginPtr, regionPtr,
        //            bufferRowPitch, bufferSlicePitch,
        //            hostRowPitch, hostSlicePitch,
        //            hostPtr,
        //            waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), &eventHandle)
        //            .ThrowIfUnsuccessful();

        //        return Event.Reify(eventHandle);
        //    }
        //}

        //public Event EnqueueWriteBufferRect(
        //    Buffer buffer,
        //    bool blocking,
        //    ReadOnlySpan<nuint> bufferOrigin,
        //    ReadOnlySpan<nuint> hostOrigin,
        //    ReadOnlySpan<nuint> region,
        //    nuint bufferRowPitch,
        //    nuint bufferSlicePitch,
        //    nuint hostRowPitch,
        //    nuint hostSlicePitch,
        //    void* hostPtr,
        //    ReadOnlySpan<Event> waitEvents = default)
        //{
        //    var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
        //    for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

        //    fixed (nuint* bufferOriginPtr = bufferOrigin)
        //    fixed (nuint* hostOriginPtr = hostOrigin)
        //    fixed (nuint* regionPtr = region)
        //    {
        //        Handle<Event> eventHandle;
        //        OpenCLNative.EnqueueWriteBufferRect(
        //            Handle, buffer.Handle, blocking,
        //            bufferOriginPtr, hostOriginPtr, regionPtr,
        //            bufferRowPitch, bufferSlicePitch,
        //            hostRowPitch, hostSlicePitch,
        //            hostPtr,
        //            waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), &eventHandle)
        //            .ThrowIfUnsuccessful();

        //        return Event.Reify(eventHandle);
        //    }
        //}

        #endregion

        #endregion

        #region Image Operations

        public Event EnqueueReadImage(Image image, nuint* origin, nuint* region, nuint rowPitch, nuint slicePitch, void* ptr, params ReadOnlySpan<Event> waitEvents)
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

        public void EnqueueReadImageBlocking(Image image, nuint* origin, nuint* region, nuint rowPitch, nuint slicePitch, void* ptr, params ReadOnlySpan<Event> waitEvents)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueReadImage(
                    Handle, image.Handle, true, origin, region, rowPitch, slicePitch, ptr,
                    waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), null)
                .ThrowIfUnsuccessful();
        }

        public Event EnqueueWriteImage(Image image, nuint* origin, nuint* region, nuint rowPitch, nuint slicePitch, void* ptr, params ReadOnlySpan<Event> waitEvents)
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

        public void EnqueueWriteImageBlocking(Image image, nuint* origin, nuint* region, nuint rowPitch, nuint slicePitch, void* ptr, params ReadOnlySpan<Event> waitEvents)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueWriteImage(
                    Handle, image.Handle, true, origin, region, rowPitch, slicePitch, ptr,
                    waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), null)
                .ThrowIfUnsuccessful();
        }

        public Event EnqueueCopyImage(Image source, Image destination, nuint* srcOrigin, nuint* dstOrigin, nuint* region, params ReadOnlySpan<Event> waitEvents)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            OpenCLNative.EnqueueCopyImage(
                Handle, source.Handle, destination.Handle, srcOrigin, dstOrigin, region,
                waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), out var eventHandle)
                .ThrowIfUnsuccessful();

            return Event.Reify(eventHandle);
        }

        public Event EnqueueFillImage(Image image, void* fillColor, nuint* origin, nuint* region, params ReadOnlySpan<Event> waitEvents)
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

        public void* EnqueueMapBuffer(Buffer buffer, MapFlags flags, nuint offset, nuint size, out Event mapEvent, params ReadOnlySpan<Event> waitEvents)
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

        public void* EnqueueMapBufferBlocking(Buffer buffer, bool blocking, MapFlags flags, nuint offset, nuint size, params ReadOnlySpan<Event> waitEvents)
        {
            var eventHandles = stackalloc Handle<Event>[waitEvents.Length];
            for (var i = 0; i < waitEvents.Length; i++) eventHandles[i] = waitEvents[i].Handle;

            var ptr = OpenCLNative.EnqueueMapBuffer(
                Handle, buffer.Handle, blocking, flags, offset, size,
                waitEvents.Length, NullIfEmpty(waitEvents, eventHandles), null, out var errorCode);

            errorCode.ThrowIfUnsuccessful();
            return ptr;
        }

        public void* EnqueueMapImage(Image image, MapFlags flags, nuint* origin, nuint* region, out nuint rowPitch, out nuint slicePitch, out Event mapEvent, params ReadOnlySpan<Event> waitEvents)
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

        public void* EnqueueMapImageBlocking(Image image, MapFlags flags, nuint* origin, nuint* region, out nuint rowPitch, out nuint slicePitch, params ReadOnlySpan<Event> waitEvents)
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

        public Event EnqueueMigrateMemObjects<TMemObject>(ReadOnlySpan<TMemObject> memObjects, MemMigrationFlags flags, params ReadOnlySpan<Event> waitEvents)
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
            => EnqueueSvmMemFill(svmPtr, [pattern], size, waitEvents);

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
        protected override void ReleaseHook() => OpenCLNative.ReleaseCommandQueue(Handle).ThrowIfUnsuccessful();

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

}
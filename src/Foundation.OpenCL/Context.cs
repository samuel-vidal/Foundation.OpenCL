using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Foundation.OpenCL
{
    #region Constants

    public enum ContextInfo
    {
        ReferenceCount = 0x1080,                         // CL_CONTEXT_REFERENCE_COUNT
        Devices = 0x1081,                                // CL_CONTEXT_DEVICES
        Properties = 0x1082,                             // CL_CONTEXT_PROPERTIES
        NumDevices = 0x1083,                             // CL_CONTEXT_NUM_DEVICES
        D3D10DeviceKhr = 0x4014,                         // CL_CONTEXT_D3D10_DEVICE_KHR
        D3D10PreferSharedResourcesKhr = 0x402C,          // CL_CONTEXT_D3D10_PREFER_SHARED_RESOURCES_KHR
        D3D11DeviceKhr = 0x401D,                         // CL_CONTEXT_D3D11_DEVICE_KHR
        D3D11PreferSharedResourcesKhr = 0x402D,          // CL_CONTEXT_D3D11_PREFER_SHARED_RESOURCES_KHR
        AdapterD3D9Khr = 0x2025,                         // CL_CONTEXT_ADAPTER_D3D9_KHR
        AdapterD3D9ExKhr = 0x2026,                       // CL_CONTEXT_ADAPTER_D3D9EX_KHR
        AdapterDxvaKhr = 0x2027,                         // CL_CONTEXT_ADAPTER_DXVA_KHR
        VaApiDisplayIntel = 0x4097,                      // CL_CONTEXT_VA_API_DISPLAY_INTEL
        D3D9DeviceIntel = 0x4026,                        // CL_CONTEXT_D3D9_DEVICE_INTEL
        D3D9ExDeviceIntel = 0x4072,                      // CL_CONTEXT_D3D9EX_DEVICE_INTEL
        DxvaDeviceIntel = 0x4073,                        // CL_CONTEXT_DXVA_DEVICE_INTEL
    }

    public enum ContextProperty : long
    {
        Platform = 0x1084,                      // CL_CONTEXT_PLATFORM
        InteropUserSync = 0x1085,               // CL_CONTEXT_INTEROP_USER_SYNC
        MemoryInitializeKhr = 0x2030,           // CL_CONTEXT_MEMORY_INITIALIZE_KHR
        TerminateKhr = 0x2032,                  // CL_CONTEXT_TERMINATE_KHR
        PrintfCallbackArm = 0x40B0,             // CL_PRINTF_CALLBACK_ARM
        PrintfBufferSizeArm = 0x40B1,           // CL_PRINTF_BUFFERSIZE_ARM
        ShowDiagnosticsIntel = 0x4106,          // CL_CONTEXT_SHOW_DIAGNOSTICS_INTEL
    }

    #endregion

    /// <summary>
    /// Configuration pair for creating a context with properties array.
    /// </summary>
    public readonly struct ContextConfig(ContextProperty property, ulong value)
    {
        public ContextProperty Property { get; } = property;
        public ulong Value { get; } = value;
    }

    public sealed unsafe class Context(Handle<Context> handle)
        : InformationNode<Context, ContextInfo>(handle), IReify<Context>
    {
        #region Create Context

        public static Context CreateContextFromType(
            DeviceType deviceType,
            Action<string>? errorCallback = null,
            params ReadOnlySpan<ContextConfig> properties)
        {
            var propArray = stackalloc ulong[properties.Length * 2 + 1];
            for (int i = 0, pos = 0; i < properties.Length; i++)
            {
                propArray[pos++] = (ulong)properties[i].Property;
                propArray[pos++] = properties[i].Value;
                propArray[pos] = 0;       // intentional
            }

            var contextHandle = OpenCLNative.CreateContextFromType(
                propArray,
                deviceType,
                MakeErrorCallbackPtr(errorCallback),
                null, // user_data set to null
                out var errCodeRet);

            errCodeRet.ThrowIfUnsuccessful();
            return Reify(contextHandle);
        }

        public static Context CreateContext(
            ReadOnlySpan<Device> devices,
            Action<string>? errorCallback = null,
            params ReadOnlySpan<ContextConfig> properties)
        {
            var propArray = stackalloc ulong[properties.Length * 2 + 1];
            for (int i = 0, pos = 0; i < properties.Length; i++)
            {
                propArray[pos++] = (ulong)properties[i].Property;
                propArray[pos++] = properties[i].Value;
                propArray[pos] = 0;       // intentional
            }

            var deviceHandles = stackalloc Handle<Device>[devices.Length];
            for (var i = 0; i < devices.Length; i++) deviceHandles[i] = devices[i].Handle;

            // clCreateContext returns the Handle and takes the error code as an out parameter
            var contextHandle = OpenCLNative.CreateContext(
                propArray,
                devices.Length,
                deviceHandles,
                MakeErrorCallbackPtr(errorCallback),
                null, // user_data set to null
                out var errCodeRet);

            errCodeRet.ThrowIfUnsuccessful();
            return Reify(contextHandle);
        }

        private static void* MakeErrorCallbackPtr(Action<string>? callback)
        {
            if (callback == null) return null;

            void ErrorCallback(byte* errorInfo, void* prInfo, nuint cb, void* userData)
            {
                try
                {
                    var length = 0;
                    while (errorInfo[length] != 0) length++;
                    var errorString = Encoding.ASCII.GetString(new ReadOnlySpan<byte>(errorInfo, length));
                    callback(errorString);
                }
                catch
                {

                }
            }

            return (void*)Marshal.GetFunctionPointerForDelegate(ErrorCallback);
        }

        #endregion

        #region Command Queues

        public CommandQueue CreateCommandQueue(
            Device device,
            params ReadOnlySpan<CommandQueueProperty> properties)
        {
            var propArray = stackalloc ulong[properties.Length * 2 + 1];

            for(int i = 0, pos = 0; i < properties.Length; i ++)
            {
                propArray[pos++] = (ulong)CommandQueueInfo.Properties;
                propArray[pos++] = (ulong)properties[i];
                propArray[pos] = 0;
            }

            var handle = OpenCLNative.CreateCommandQueueWithProperties(
                Handle,
                device.Handle,
                NullIfEmpty(properties, propArray),
                out var errCodeRet);

            errCodeRet.ThrowIfUnsuccessful();

            return CommandQueue.Reify(handle);
        }

        public void SetDefaultCommandQueue(Device device, CommandQueue commandQueue)
        {
            OpenCLNative.SetDefaultDeviceCommandQueue(Handle, device.Handle, commandQueue.Handle)
                .ThrowIfUnsuccessful();
        }

        #endregion

        #region User Events

        public Event CreateUserEvent()
        {
            var handle = OpenCLNative.CreateUserEvent(Handle, out var errorCode);
            errorCode.ThrowIfUnsuccessful();
            return Event.Reify(handle);
        }

        #endregion

        #region Memory Buffers

        public Buffer CreateBuffer(MemFlags flags, nuint size, void* hostPtr = null)
        {
            var handle = OpenCLNative.CreateBuffer(Handle, flags, size, hostPtr, out var errorCode);
            errorCode.ThrowIfUnsuccessful();
            return Buffer.Reify(handle);
        }

        #region Memory Buffers with Properties

        public Buffer CreateBufferWithProperties(
            ReadOnlySpan<MemConfig> properties,
            MemFlags flags,
            nuint size,
            void* hostPtr = null)
        {
            var propArray = stackalloc ulong[properties.Length * 2 + 1];
            for (int i = 0, pos = 0; i < properties.Length; i++)
            {
                propArray[pos++] = (ulong)properties[i].Property;
                propArray[pos++] = properties[i].Value;
                propArray[pos] = 0;
            }

            var handle = OpenCLNative.CreateBufferWithProperties(
                Handle, propArray, flags, size, hostPtr, out var errorCode);
            errorCode.ThrowIfUnsuccessful();
            return Buffer.Reify(handle);
        }

        public Image CreateImageWithProperties(
            ReadOnlySpan<MemConfig> properties,
            MemFlags flags,
            ImageFormat imageFormat,
            ImageDesc imageDesc,
            void* hostPtr = null)
        {
            var propArray = stackalloc ulong[properties.Length * 2 + 1];
            for (int i = 0, pos = 0; i < properties.Length; i++)
            {
                propArray[pos++] = (ulong)properties[i].Property;
                propArray[pos++] = properties[i].Value;
                propArray[pos] = 0;
            }

            var handle = OpenCLNative.CreateImageWithProperties(
                Handle, propArray, flags, &imageFormat, &imageDesc, hostPtr, out var errorCode);
            errorCode.ThrowIfUnsuccessful();
            return Image.Reify(handle);
        }

        #endregion

        #endregion

        #region Images

        public Image CreateImage(MemFlags flags, ImageFormat imageFormat, ImageDesc imageDesc, void* hostPtr = null)
        {
            var handle = OpenCLNative.CreateImage(Handle, flags, &imageFormat, &imageDesc, hostPtr, out var errorCode);
            errorCode.ThrowIfUnsuccessful();
            return Image.Reify(handle);
        }

        //      CreateImageWithProperties

        public ImageFormat[] GetSupportedImageFormats(MemFlags flags, MemObjectType imageType)
        {
            OpenCLNative.GetSupportedImageFormats(Handle, flags, imageType, 0, null, out var count).ThrowIfUnsuccessful();
            var formats = new ImageFormat[count];
            fixed (ImageFormat* ptr = formats)
            {
                OpenCLNative.GetSupportedImageFormats(Handle, flags, imageType, 0, ptr, out _).ThrowIfUnsuccessful();
            }

            return formats;
        }

        #endregion

        #region Pipe

        public Pipe CreatePipe(MemFlags flags, int packetSize, int numPackets)
        {
            var handle = OpenCLNative.CreatePipe(Handle, flags, packetSize, numPackets, null, out var errorCode);
            errorCode.ThrowIfUnsuccessful();
            return Pipe.Reify(handle);
        }

        #endregion

        #region SVM

        public void* SvmAlloc(SvmMemFlags flags, nuint size, int alignment)
        {
            return OpenCLNative.SvmAlloc(Handle, flags, size, alignment);
        }

        public void SvmFree(void* svmPointer)
        {
            OpenCLNative.SvmFree(Handle, svmPointer);
        }

        #endregion

        #region Progams

        public Program CreateWithSource(params Span<string> source)
        {
            var lengths = stackalloc nuint[source.Length];
            var stringPtrs = stackalloc byte*[source.Length];
            for (var i = 0; i < source.Length; i++) stringPtrs[i] = null;

            try
            {
                for (var i = 0; i < source.Length; i++)
                {
                    var length = Encoding.UTF8.GetByteCount(source[i]);
                    lengths[i] = (nuint)length;
                    stringPtrs[i] = (byte*)NativeMemory.Alloc(lengths[i]);
                    Encoding.UTF8.GetBytes(source[i], new Span<byte>(stringPtrs[i], length));
                }

                var handle = OpenCLNative.CreateProgramWithSource(
                    Handle, source.Length, stringPtrs, lengths, out var errorCode);
                errorCode.ThrowIfUnsuccessful();

                return Program.Reify(handle);
            }
            finally
            {
                for (var i = 0; i < source.Length; i++) NativeMemory.Free(stringPtrs[i]);
            }
        }

        public Program CreateWithBinary(ReadOnlySpan<Device> devices, ReadOnlySpan<byte[]> binaries)
        {
            var deviceHandles = stackalloc Handle<Device>[devices.Length];
            for (var i = 0; i < devices.Length; i++) deviceHandles[i] = devices[i].Handle;

            var lengths = stackalloc nuint[binaries.Length];
            var binaryPtrs = stackalloc byte*[binaries.Length];
            var binaryStatus = stackalloc ErrorCode[binaries.Length];

            Span<GCHandle> gcHandles = stackalloc GCHandle[binaries.Length];
            try
            {
                // Pin all binary arrays
                for (var i = 0; i < binaries.Length; i++)
                {
                    lengths[i] = (nuint)binaries[i].Length;
                    gcHandles[i] = GCHandle.Alloc(binaries[i], GCHandleType.Pinned);
                    binaryPtrs[i] = (byte*)gcHandles[i].AddrOfPinnedObject();
                }

                var handle = OpenCLNative.CreateProgramWithBinary(
                    Handle, devices.Length, deviceHandles, lengths, binaryPtrs, binaryStatus, out var errorCode);

                errorCode.ThrowIfUnsuccessful(message: string.Join(", ", new Span<ErrorCode>(binaryStatus, binaries.Length).ToArray()));
                return Program.Reify(handle);
            }
            finally
            {
                foreach (var gcHandle in gcHandles)
                    if (gcHandle.IsAllocated) gcHandle.Free();
            }
        }

        public Program CreateProgramWithIl(ReadOnlySpan<byte> il)
        {
            fixed (byte* ilPtr = il)
            {
                var handle = OpenCLNative.CreateProgramWithIL(Handle, ilPtr, (nuint)il.Length, out var errorCode);
                errorCode.ThrowIfUnsuccessful();
                return Program.Reify(handle);
            }
        }

        public Program LinkProgram(
            ReadOnlySpan<Device> devices,
            ReadOnlySpan<Program> inputPrograms,
            string? linkOptions = null,
            Action? notifyCallback = null)
        {
            var deviceHandles = stackalloc Handle<Device>[devices.Length];
            for (var i = 0; i < devices.Length; i++) deviceHandles[i] = devices[i].Handle;

            var programHandles = stackalloc Handle<Program>[inputPrograms.Length];
            for (var i = 0; i < inputPrograms.Length; i++) programHandles[i] = inputPrograms[i].Handle;

            byte* optionsPtr = null;
            if (linkOptions != null)
            {
                var length = Encoding.UTF8.GetByteCount(linkOptions);
                var tmp = stackalloc byte[length + 1];
                optionsPtr = tmp;

                Encoding.UTF8.GetBytes(linkOptions, new Span<byte>(optionsPtr, length));
                optionsPtr[length] = 0;
            }

            void Notify(Handle<Program> programHandle, void* userData)
            {
                try { notifyCallback(); } catch { }
            }

            void* notifyPtr = null;
            if (notifyCallback != null)
            {
                notifyPtr = (void*)Marshal.GetFunctionPointerForDelegate(Notify);
            }

            var programHandle = OpenCLNative.LinkProgram(
                Handle, devices.Length, deviceHandles,
                optionsPtr, inputPrograms.Length, programHandles,
                notifyPtr, null, out var errorCode);
            errorCode.ThrowIfUnsuccessful();

            var result = Program.Reify(programHandle);

            if (notifyCallback != null)
            {
                var notifyHandle = GCHandle.Alloc(Notify);
                result.OnDispose += ()=> notifyHandle.Free();
            }

            return result;
        }

        #endregion

        #region Destructor

        // Canonical SetDestructorCallback implementation
        public void SetDestructorCallback(Action callback)
        {
            GCHandle managed = default;
            void Hook(Handle<Context> handle, void* _)
            {
                try { callback(); } catch { }
                if (managed.IsAllocated) managed.Free();
                GC.SuppressFinalize(this);
            }

            OpenCLNative.SetContextDestructorCallback(Handle, (void*)Marshal.GetFunctionPointerForDelegate(Hook), null)
                .ThrowIfUnsuccessful();

            managed = GCHandle.Alloc(Hook);
        }

        #endregion

        protected override void GetInfo(ContextInfo paramName, nuint paramValueSize, void* paramValue, out nuint paramValueSizeRet)
            => OpenCLNative.GetContextInfo(Handle, paramName, paramValueSize, paramValue, out paramValueSizeRet).ThrowIfUnsuccessful();

        protected override void RetainHook() => OpenCLNative.RetainContext(Handle).ThrowIfUnsuccessful();
        protected override void ReleaseHook(Handle<Context> tmpHandle) => OpenCLNative.ReleaseContext(tmpHandle).ThrowIfUnsuccessful();

        public static Context Reify(Handle<Context> handle) => new(handle);
    }

    public static class ContextExtensions
    {
        public static int GetReferenceCount(this Context queue)
            => queue.GetInfo<int>(ContextInfo.ReferenceCount);
    }
}
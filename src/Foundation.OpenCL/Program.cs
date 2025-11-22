using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Foundation.OpenCL
{
    #region Constants

    public enum ProgramBinaryType
    {
        None = 0x0,                     // CL_PROGRAM_BINARY_TYPE_NONE
        CompiledObject = 0x1,           // CL_PROGRAM_BINARY_TYPE_COMPILED_OBJECT
        Library = 0x2,                  // CL_PROGRAM_BINARY_TYPE_LIBRARY
        Executable = 0x4,               // CL_PROGRAM_BINARY_TYPE_EXECUTABLE
        Intermediate = 0x40E1,          // CL_PROGRAM_BINARY_TYPE_INTERMEDIATE
    }

    public enum ProgramBuildInfo
    {
        BuildStatus = 0x1181,                           // CL_PROGRAM_BUILD_STATUS
        BuildOptions = 0x1182,                          // CL_PROGRAM_BUILD_OPTIONS
        BuildLog = 0x1183,                              // CL_PROGRAM_BUILD_LOG
        BinaryType = 0x1184,                            // CL_PROGRAM_BINARY_TYPE
        BuildGlobalVariableTotalSize = 0x1185,          // CL_PROGRAM_BUILD_GLOBAL_VARIABLE_TOTAL_SIZE
    }

    public enum ProgramInfo
    {
        ReferenceCount = 0x1160,                   // CL_PROGRAM_REFERENCE_COUNT
        Context = 0x1161,                          // CL_PROGRAM_CONTEXT
        NumDevices = 0x1162,                       // CL_PROGRAM_NUM_DEVICES
        Devices = 0x1163,                          // CL_PROGRAM_DEVICES
        Source = 0x1164,                           // CL_PROGRAM_SOURCE
        BinarySizes = 0x1165,                      // CL_PROGRAM_BINARY_SIZES
        Binaries = 0x1166,                         // CL_PROGRAM_BINARIES
        NumKernels = 0x1167,                       // CL_PROGRAM_NUM_KERNELS
        KernelNames = 0x1168,                      // CL_PROGRAM_KERNEL_NAMES
        Il = 0x1169,                               // CL_PROGRAM_IL
        ScopeGlobalCtorsPresent = 0x116A,          // CL_PROGRAM_SCOPE_GLOBAL_CTORS_PRESENT
        ScopeGlobalDtorsPresent = 0x116B,          // CL_PROGRAM_SCOPE_GLOBAL_DTORS_PRESENT
        IlKhr = 0x1169,                            // CL_PROGRAM_IL_KHR
    }

    public enum ProgramBuildStatus
    {
        None = 0,
        Success = 1,
        Error = 2,
        InProgress = 3
    }

    #endregion

    public readonly struct ProgramHeader(Program program, string includeName)
    {
        public Program Program { get; } = program;
        public string IncludeName { get; } = includeName;
    }

    public sealed unsafe class Program(Handle<Program> handle)
        : BaseObject<Program, ProgramInfo>(handle), IReify<Program>
    {
        #region Compilation & Linking

        public void CompileProgram(
            ReadOnlySpan<Device> devices,
            string? compileOptions = null,
            ReadOnlySpan<ProgramHeader> headers = default,
            Action? notifyCallback = null)
        {
            var deviceHandles = stackalloc Handle<Device>[devices.Length];
            for (var i = 0; i < devices.Length; i++) deviceHandles[i] = devices[i].Handle;

            byte* optionsPtr = null;
            if (compileOptions != null)
            {
                var length = Encoding.UTF8.GetByteCount(compileOptions);
                var tmp = stackalloc byte[length + 1];
                optionsPtr = tmp;

                Encoding.UTF8.GetBytes(compileOptions, new Span<byte>(optionsPtr, length));
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

            // Handle headers if provided
            Handle<Program>* headerHandles = null;
            byte** headerNames = null;
            byte* headerNamesBuffer = null;
            if (headers.Length > 0)
            {
                var totalLength = 0;
                foreach (var header in headers) totalLength += Encoding.ASCII.GetByteCount(header.IncludeName);

                headerNamesBuffer = (byte*) NativeMemory.Alloc((nuint)(totalLength + headers.Length));

                var tmp1 = stackalloc Handle<Program>[headers.Length];
                var tmp2 = stackalloc byte*[headers.Length];
                headerHandles = tmp1;
                headerNames = tmp2;

                var pos = 0;
                for (var i = 0; i < headers.Length; i++)
                {
                    headerHandles[i] = headers[i].Program.Handle;
                    headerNames[i] = headerNamesBuffer + pos;
                    pos += Encoding.ASCII.GetBytes(headers[i].IncludeName, new Span<byte>(headerNamesBuffer + pos, totalLength - pos));
                    headerNamesBuffer[pos++] = 0;
                }
            }

            try
            {
                OpenCLNative.CompileProgram(
                    Handle, devices.Length, deviceHandles,
                    optionsPtr, headers.Length, headerHandles, headerNames,
                    notifyPtr, null).ThrowIfUnsuccessful();

                if (notifyCallback != null)
                {
                    var managed = GCHandle.Alloc(Notify);
                    OnDispose += () => managed.Free();
                }
            }
            finally
            {
                if (headers.Length > 0) NativeMemory.Free(headerNamesBuffer);
            }
        }

        public void Build(ReadOnlySpan<Device> devices, string? buildOptions = null, Action? notifyCallback = null)
        {
            var deviceHandles = stackalloc Handle<Device>[devices.Length];
            for (var i = 0; i < devices.Length; i++) deviceHandles[i] = devices[i].Handle;

            byte* optionsPtr = null;
            if (buildOptions != null)
            {
                var optionLength = Encoding.ASCII.GetByteCount(buildOptions);
                var ptr = stackalloc byte[optionLength + 1];
                optionsPtr = ptr;
                Encoding.ASCII.GetBytes(buildOptions, new Span<byte>(optionsPtr, optionLength));
                optionsPtr[optionLength] = 0;
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

            OpenCLNative.BuildProgram(Handle, devices.Length, deviceHandles, optionsPtr, notifyPtr, null)
                .ThrowIfUnsuccessful();

            if (notifyCallback != null)
            {
                var managed = GCHandle.Alloc(Notify);
                OnDispose += () => managed.Free();
            }
        }

        public string GetBuildLog(Device device) => GetStringBuildInfo(device, ProgramBuildInfo.BuildLog);
        public ProgramBuildStatus GetBuildStatus(Device device) => GetInfo<ProgramBuildStatus>(device, ProgramBuildInfo.BuildStatus);

        private string GetStringBuildInfo(Device device, ProgramBuildInfo paramName)
        {
            var length = GetBuildInfoByteSize(device, paramName);
            if (length == 0) return string.Empty;

            var buffer = stackalloc byte[length];
            GetBuildInfo(device, paramName, (nuint)length, buffer, out _);
            return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buffer, length - 1));
        }

        private int GetBuildInfoByteSize(Device device, ProgramBuildInfo paramName)
        {
            GetBuildInfo(device, paramName, 0, null, out var size);
            return (int)size;
        }

        private void GetBuildInfo(Device device, ProgramBuildInfo paramName, nuint paramValueSize, void* paramValue, out nuint paramValueSizeRet)
            => OpenCLNative.GetProgramBuildInfo(Handle, device.Handle, paramName, paramValueSize, paramValue, out paramValueSizeRet).ThrowIfUnsuccessful();

        private T GetInfo<T>(Device device, ProgramBuildInfo paramName) where T : unmanaged
        {
            if (TryGetBuildInfo(device, paramName, out T value)) return value;
            throw new InvalidOperationException();
        }

        private bool TryGetBuildInfo<T>(Device device, ProgramBuildInfo paramName, out T value) where T : unmanaged
        {
            var val = value = default;
            GetBuildInfo(device, paramName, (nuint)sizeof(T), &val, out var size);
            if ((int)size != sizeof(T)) return false;
            value = val;
            return true;
        }

        #endregion

        #region Kernels

        public Kernel[] CreateAllKernels()
        {
            OpenCLNative.CreateKernelsInProgram(Handle, 0, null, out var count).ThrowIfUnsuccessful();
            if (count == 0) return [];

            var handles = stackalloc Handle<Kernel>[count];
            OpenCLNative.CreateKernelsInProgram(Handle, count, handles, out count).ThrowIfUnsuccessful();

            var result = new Kernel[count];
            for (var i = 0; i < result.Length; i++) result[i] = Kernel.Reify(handles[i]);
            return result;
        }

        public Kernel CreateKernel(string name)
        {
            var nameLength = Encoding.UTF8.GetByteCount(name);
            var namePtr = stackalloc byte[nameLength + 1];
            Encoding.UTF8.GetBytes(name, new Span<byte>(namePtr, nameLength));
            namePtr[nameLength] = 0;

            var handle = OpenCLNative.CreateKernel(Handle, namePtr, out var errorCode);
                errorCode.ThrowIfUnsuccessful();
                return Kernel.Reify(handle);
        }

        #endregion

        protected override void RetainHook() => OpenCLNative.RetainProgram(Handle).ThrowIfUnsuccessful();
        protected override void ReleaseHook(Handle<Program> tmpHandle) => OpenCLNative.ReleaseProgram(tmpHandle).ThrowIfUnsuccessful();

        protected override void GetInfo(ProgramInfo paramName, nuint paramValueSize, void* paramValue, out nuint paramValueSizeRet)
            => OpenCLNative.GetProgramInfo(Handle, paramName, paramValueSize, paramValue, out paramValueSizeRet).ThrowIfUnsuccessful();

        public static Program Reify(Handle<Program> handle) => new(handle);
    }
}

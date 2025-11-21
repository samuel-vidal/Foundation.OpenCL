using System.Runtime.InteropServices;

namespace Foundation.OpenCL.Extensions.Intel
{
    public static unsafe class UnifiedSharedMemoryNative
    {
        [DllImport(OpenCLNative.OpenClLibrary, EntryPoint = "clHostMemAllocINTEL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void* HostMemAlloc(
            Handle<Context> context,
            ulong* properties,
            nuint size,
            uint alignment,
            out ErrorCode errorCode);

        [DllImport(OpenCLNative.OpenClLibrary, EntryPoint = "clDeviceMemAllocINTEL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void* DeviceMemAlloc(
            Handle<Context> context,
            Handle<Device> device,
            ulong* properties,
            nuint size,
            uint alignment,
            out ErrorCode errorCode);

        [DllImport(OpenCLNative.OpenClLibrary, EntryPoint = "clSharedMemAllocINTEL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void* SharedMemAlloc(
            Handle<Context> context,
            Handle<Device> device,
            ulong* properties,
            nuint size,
            uint alignment,
            out ErrorCode errorCode);

        [DllImport(OpenCLNative.OpenClLibrary, EntryPoint = "clMemFreeINTEL", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode MemFree(
            Handle<Context> context,
            void* ptr);

        [DllImport(OpenCLNative.OpenClLibrary, EntryPoint = "clMemBlockingFreeINTEL", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode MemBlockingFree(
            Handle<Context> context,
            void* ptr);

        [DllImport(OpenCLNative.OpenClLibrary, EntryPoint = "clGetMemAllocInfoINTEL", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetMemAllocInfo(
            Handle<Context> context,
            void* ptr,
            MemAllocInfo paramName,
            nuint paramValueSize,
            void* paramValue,
            out nuint paramValueSizeRet);

        [DllImport(OpenCLNative.OpenClLibrary, EntryPoint = "clSetKernelArgMemPointerINTEL", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode SetKernelArgMemPointer(
            Handle<Kernel> kernel,
            uint argIndex,
            void* argValue);

        [DllImport(OpenCLNative.OpenClLibrary, EntryPoint = "clEnqueueMemFillINTEL", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueMemFill(
            Handle<CommandQueue> commandQueue,
            void* dstPtr,
            void* pattern,
            nuint patternSize,
            nuint size,
            uint numEventsInWaitList,
            Handle<Event>* eventWaitList,
            out Handle<Event> userEvent);

        [DllImport(OpenCLNative.OpenClLibrary, EntryPoint = "clEnqueueMemcpyINTEL", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueMemcpy(
            Handle<CommandQueue> commandQueue,
            bool blocking,
            void* dstPtr,
            void* srcPtr,
            nuint size,
            uint numEventsInWaitList,
            Handle<Event>* eventWaitList,
            out Handle<Event> userEvent);

        [DllImport(OpenCLNative.OpenClLibrary, EntryPoint = "clEnqueueMigrateMemINTEL", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueMigrateMem(
            Handle<CommandQueue> commandQueue,
            void* ptr,
            nuint size,
            MemMigrationFlags flags,
            uint numEventsInWaitList,
            Handle<Event>* eventWaitList,
            out Handle<Event> userEvent);

        [DllImport(OpenCLNative.OpenClLibrary, EntryPoint = "clEnqueueMemAdviseINTEL", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueMemAdvise(
            Handle<CommandQueue> commandQueue,
            void* ptr,
            nuint size,
            MemAdvice advice,
            uint numEventsInWaitList,
            Handle<Event>* eventWaitList,
            out Handle<Event> userEvent);
    }
}

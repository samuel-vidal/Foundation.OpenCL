using System;
using System.Runtime.InteropServices;

namespace Foundation.OpenCL
{
    #region Constants

    public enum MemInfo
    {
        Type = 0x1100,                            // CL_MEM_TYPE
        Flags = 0x1101,                           // CL_MEM_FLAGS
        Size = 0x1102,                            // CL_MEM_SIZE
        HostPtr = 0x1103,                         // CL_MEM_HOST_PTR
        MapCount = 0x1104,                        // CL_MEM_MAP_COUNT
        ReferenceCount = 0x1105,                  // CL_MEM_REFERENCE_COUNT
        Context = 0x1106,                         // CL_MEM_CONTEXT
        AssociatedMemobject = 0x1107,             // CL_MEM_ASSOCIATED_MEMOBJECT
        Offset = 0x1108,                          // CL_MEM_OFFSET
        UsesSvmPointer = 0x1109,                  // CL_MEM_USES_SVM_POINTER
        Properties = 0x110A,                      // CL_MEM_PROPERTIES
        D3D10ResourceKhr = 0x4015,                // CL_MEM_D3D10_RESOURCE_KHR
        D3D11ResourceKhr = 0x401E,                // CL_MEM_D3D11_RESOURCE_KHR
        DX9MediaAdapterTypeKhr = 0x2028,          // CL_MEM_DX9_MEDIA_ADAPTER_TYPE_KHR
        DX9MediaSurfaceInfoKhr = 0x2029,          // CL_MEM_DX9_MEDIA_SURFACE_INFO_KHR
        UsesSvmPointerArm = 0x40B7,               // CL_MEM_USES_SVM_POINTER_ARM
        VAApiMediaSurfaceIntel = 0x4098,          // CL_MEM_VA_API_MEDIA_SURFACE_INTEL
        DX9ResourceIntel = 0x4027,                // CL_MEM_DX9_RESOURCE_INTEL
        DX9SharedHandleIntel = 0x4074,            // CL_MEM_DX9_SHARED_HANDLE_INTEL
    }

    public enum MemProperties : ulong
    {
        AllocFlagsImg = 0x40D7,                         // CL_MEM_ALLOC_FLAGS_IMG
        DeviceHandleListKhr = 0x2051,                   // CL_MEM_DEVICE_HANDLE_LIST_KHR
        LocallyUncachedResourceIntel = 0x4218,          // CL_MEM_LOCALLY_UNCACHED_RESOURCE_INTEL
        DeviceIDIntel = 0x4219,                         // CL_MEM_DEVICE_ID_INTEL
    }

    public enum MemObjectType
    {
        Buffer = 0x10F0,                 // CL_MEM_OBJECT_BUFFER
        Image2D = 0x10F1,                // CL_MEM_OBJECT_IMAGE2D
        Image3D = 0x10F2,                // CL_MEM_OBJECT_IMAGE3D
        Image2DArray = 0x10F3,           // CL_MEM_OBJECT_IMAGE2D_ARRAY
        Image1D = 0x10F4,                // CL_MEM_OBJECT_IMAGE1D
        Image1DArray = 0x10F5,           // CL_MEM_OBJECT_IMAGE1D_ARRAY
        Image1DBuffer = 0x10F6,          // CL_MEM_OBJECT_IMAGE1D_BUFFER
        Pipe = 0x10F7,                   // CL_MEM_OBJECT_PIPE
    }

    [Flags]
    public enum MemFlags : ulong
    {
        None = 0x0,
        ReadWrite = 0x1,                                   // CL_MEM_READ_WRITE
        WriteOnly = 0x2,                                   // CL_MEM_WRITE_ONLY
        ReadOnly = 0x4,                                    // CL_MEM_READ_ONLY
        UseHostPtr = 0x8,                                  // CL_MEM_USE_HOST_PTR
        AllocHostPtr = 0x10,                               // CL_MEM_ALLOC_HOST_PTR
        CopyHostPtr = 0x20,                                // CL_MEM_COPY_HOST_PTR
        HostWriteOnly = 0x80,                              // CL_MEM_HOST_WRITE_ONLY
        HostReadOnly = 0x100,                              // CL_MEM_HOST_READ_ONLY
        HostNoAccess = 0x200,                              // CL_MEM_HOST_NO_ACCESS
        SvmFineGrainBuffer = 0x400,                        // CL_MEM_SVM_FINE_GRAIN_BUFFER
        SvmAtomics = 0x800,                                // CL_MEM_SVM_ATOMICS
        KernelReadAndWrite = 0x1000,                       // CL_MEM_KERNEL_READ_AND_WRITE
        ExtHostPtrQCom = 0x20000000,                       // CL_MEM_EXT_HOST_PTR_QCOM
        UseUncachedCpuMemoryImg = 0x4000000,               // CL_MEM_USE_UNCACHED_CPU_MEMORY_IMG
        UseCachedCpuMemoryImg = 0x8000000,                 // CL_MEM_USE_CACHED_CPU_MEMORY_IMG
        UseGrallocPtrImg = 0x10000000,                     // CL_MEM_USE_GRALLOC_PTR_IMG
        NoAccessIntel = 0x1000000,                         // CL_MEM_NO_ACCESS_INTEL
        AccessFlagsUnrestrictedIntel = 0x2000000,          // CL_MEM_ACCESS_FLAGS_UNRESTRICTED_INTEL
        ForceHostMemoryIntel = 0x100000,                   // CL_MEM_FORCE_HOST_MEMORY_INTEL
        ProtectedAllocArm = 0x1000000000,                  // CL_MEM_PROTECTED_ALLOC_ARM
    }

    public enum BufferCreateType : ulong
    {
        Region = 0x1220,          // CL_BUFFER_CREATE_TYPE_REGION
    }

    public enum SvmMemFlags : ulong
    {
        None = 0x0,
    }

    #endregion

    public struct BufferRegion
    {
        public nuint Origin;
        public nuint Size;
    }

    public sealed class MemoryObject
    {
        private MemoryObject() { }
    }
    
    // Configuration structure for memory properties
    public readonly struct MemConfig(MemProperties property, ulong value)
    {
        public MemProperties Property { get; } = property;
        public ulong Value { get; } = value;
    }

    public abstract unsafe class BaseMemoryObject<TSelf>(Handle<MemoryObject> handle)
        : BaseObject<TSelf, MemoryObject, MemInfo>(handle)
        where TSelf : BaseObject<TSelf, MemoryObject, MemInfo>, IReify<TSelf, MemoryObject>
    {

        public void SetDestructorCallback(Action callback)
        {
            GCHandle managed = default;

            void Hook(Handle<MemoryObject> handle, void* _)
            {
                try { callback(); } catch { }
                if (managed.IsAllocated) managed.Free();
            }

            OpenCLNative.SetMemObjectDestructorCallback(Handle,
                    (void*)Marshal.GetFunctionPointerForDelegate(Hook), null)
                .ThrowIfUnsuccessful();

            managed = GCHandle.Alloc(Hook);
            OnDispose += () => managed.Free();
        }

        protected override void RetainHook() => OpenCLNative.RetainMemObject(Handle).ThrowIfUnsuccessful();
        protected override void ReleaseHook(Handle<MemoryObject> tmpHandle) => OpenCLNative.ReleaseMemObject(tmpHandle).ThrowIfUnsuccessful();

        protected override void GetInfo(MemInfo paramName, nuint paramValueSize, void* paramValue, out nuint paramValueSizeRet)
            => OpenCLNative.GetMemObjectInfo(Handle, paramName, paramValueSize, paramValue, out paramValueSizeRet).ThrowIfUnsuccessful();

        //  Todo
        //      SetMemObjectDestructorCallback
    }

    public sealed unsafe class Buffer(Handle<MemoryObject> handle)
        : BaseMemoryObject<Buffer>(handle), IReify<Buffer, MemoryObject>
    {
        /// <summary>
        /// Creates a sub-buffer using the CL_BUFFER_CREATE_TYPE_REGION type.
        /// </summary>
        /// <param name="flags">A bit-field of options for the sub-buffer.</param>
        /// <param name="origin">The byte offset in the parent buffer.</param>
        /// <param name="size">The size in bytes of the sub-buffer.</param>
        /// <returns>A new Buffer object representing the sub-region.</returns>
        public Buffer CreateSubBuffer(MemFlags flags, nuint origin, nuint size)
        {
            var region = new BufferRegion { Origin = origin, Size = size };
            var handle = OpenCLNative.CreateSubBuffer(Handle, flags, (ulong)BufferCreateType.Region, &region, out var errorCode);
            errorCode.ThrowIfUnsuccessful();
            return Reify(handle);
        }


        /// <summary>
        /// Creates a sub-buffer using an arbitrary, unmanaged parameter structure.
        /// </summary>
        /// <typeparam name="T">The type of the unmanaged parameter structure (e.g., BufferRegion).</typeparam>
        public Buffer UnsafeCreateSubBuffer<T>(MemFlags flags, BufferCreateType type, in T parameter)
            where T : unmanaged
        {
            fixed (T* ptr = &parameter)
            {
                var handle = OpenCLNative.CreateSubBuffer(Handle, flags, (ulong)type, ptr, out var errorCode);
                errorCode.ThrowIfUnsuccessful();
                return Reify(handle);
            }
        }

        public static Buffer Reify(Handle<MemoryObject> handle) => new(handle);
    }
}

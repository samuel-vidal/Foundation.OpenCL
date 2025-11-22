using System;
using System.Text;

namespace Foundation.OpenCL
{
    #region Constants

    public enum KernelArgAccessQualifier
    {
        ReadOnly = 0x11A0,           // CL_KERNEL_ARG_ACCESS_READ_ONLY
        WriteOnly = 0x11A1,          // CL_KERNEL_ARG_ACCESS_WRITE_ONLY
        ReadWrite = 0x11A2,          // CL_KERNEL_ARG_ACCESS_READ_WRITE
        None = 0x11A3,               // CL_KERNEL_ARG_ACCESS_NONE
    }

    public enum KernelArgAddressQualifier
    {
        Global = 0x119B,            // CL_KERNEL_ARG_ADDRESS_GLOBAL
        Local = 0x119C,             // CL_KERNEL_ARG_ADDRESS_LOCAL
        Constant = 0x119D,          // CL_KERNEL_ARG_ADDRESS_CONSTANT
        Private = 0x119E,           // CL_KERNEL_ARG_ADDRESS_PRIVATE
    }

    public enum KernelArgInfo
    {
        AddressQualifier = 0x1196,          // CL_KERNEL_ARG_ADDRESS_QUALIFIER
        AccessQualifier = 0x1197,           // CL_KERNEL_ARG_ACCESS_QUALIFIER
        TypeName = 0x1198,                  // CL_KERNEL_ARG_TYPE_NAME
        TypeQualifier = 0x1199,             // CL_KERNEL_ARG_TYPE_QUALIFIER
        Name = 0x119A,                      // CL_KERNEL_ARG_NAME
    }

    public enum KernelArgTypeQualifier : ulong
    {
        None = 0x0,              // CL_KERNEL_ARG_TYPE_NONE
        Const = 0x1,             // CL_KERNEL_ARG_TYPE_CONST
        Restrict = 0x2,          // CL_KERNEL_ARG_TYPE_RESTRICT
        Volatile = 0x4,          // CL_KERNEL_ARG_TYPE_VOLATILE
        Pipe = 0x8,              // CL_KERNEL_ARG_TYPE_PIPE
    }

    public enum KernelExecInfo
    {
        SvmPtrs = 0x11B6,                                 // CL_KERNEL_EXEC_INFO_SVM_PTRS
        SvmFineGrainSystem = 0x11B7,                      // CL_KERNEL_EXEC_INFO_SVM_FINE_GRAIN_SYSTEM
        IndirectHostAccessIntel = 0x4200,                 // CL_KERNEL_EXEC_INFO_INDIRECT_HOST_ACCESS_INTEL
        IndirectDeviceAccessIntel = 0x4201,               // CL_KERNEL_EXEC_INFO_INDIRECT_DEVICE_ACCESS_INTEL
        IndirectSharedAccessIntel = 0x4202,               // CL_KERNEL_EXEC_INFO_INDIRECT_SHARED_ACCESS_INTEL
        UsmPtrsIntel = 0x4203,                            // CL_KERNEL_EXEC_INFO_USM_PTRS_INTEL
        WorkgroupBatchSizeArm = 0x41E5,                   // CL_KERNEL_EXEC_INFO_WORKGROUP_BATCH_SIZE_ARM
        WorkgroupBatchSizeModifierArm = 0x41E6,           // CL_KERNEL_EXEC_INFO_WORKGROUP_BATCH_SIZE_MODIFIER_ARM
        WarpCountLimitArm = 0x41E8,                       // CL_KERNEL_EXEC_INFO_WARP_COUNT_LIMIT_ARM
        ComputeUnitMaxQueuedBatchesArm = 0x41F1,          // CL_KERNEL_EXEC_INFO_COMPUTE_UNIT_MAX_QUEUED_BATCHES_ARM
    }

    public enum KernelInfo
    {
        FunctionName = 0x1190,             // CL_KERNEL_FUNCTION_NAME
        NumArgs = 0x1191,                  // CL_KERNEL_NUM_ARGS
        ReferenceCount = 0x1192,           // CL_KERNEL_REFERENCE_COUNT
        Context = 0x1193,                  // CL_KERNEL_CONTEXT
        Program = 0x1194,                  // CL_KERNEL_PROGRAM
        Attributes = 0x1195,               // CL_KERNEL_ATTRIBUTES
        MaxWarpCountArm = 0x41E9,          // CL_KERNEL_MAX_WARP_COUNT_ARM
    }

    public enum KernelSubGroupInfo
    {
        MaxNumSubGroups = 0x11B9,                       // CL_KERNEL_MAX_NUM_SUB_GROUPS
        CompileNumSubGroups = 0x11BA,                   // CL_KERNEL_COMPILE_NUM_SUB_GROUPS
        MaxSubGroupSizeForNdrange = 0x2033,             // CL_KERNEL_MAX_SUB_GROUP_SIZE_FOR_NDRANGE
        SubGroupCountForNdrange = 0x2034,               // CL_KERNEL_SUB_GROUP_COUNT_FOR_NDRANGE
        LocalSizeForSubGroupCount = 0x11B8,             // CL_KERNEL_LOCAL_SIZE_FOR_SUB_GROUP_COUNT
        MaxSubGroupSizeForNdrangeKhr = 0x2033,          // CL_KERNEL_MAX_SUB_GROUP_SIZE_FOR_NDRANGE_KHR
        SubGroupCountForNdrangeKhr = 0x2034,            // CL_KERNEL_SUB_GROUP_COUNT_FOR_NDRANGE_KHR
        CompileSubGroupSizeIntel = 0x410A,              // CL_KERNEL_COMPILE_SUB_GROUP_SIZE_INTEL
    }

    public enum KernelWorkGroupInfo
    {
        WorkGroupSize = 0x11B0,                           // CL_KERNEL_WORK_GROUP_SIZE
        CompileWorkGroupSize = 0x11B1,                    // CL_KERNEL_COMPILE_WORK_GROUP_SIZE
        LocalMemSize = 0x11B2,                            // CL_KERNEL_LOCAL_MEM_SIZE
        PreferredWorkGroupSizeMultiple = 0x11B3,          // CL_KERNEL_PREFERRED_WORK_GROUP_SIZE_MULTIPLE
        PrivateMemSize = 0x11B4,                          // CL_KERNEL_PRIVATE_MEM_SIZE
        GlobalWorkSize = 0x11B5,                          // CL_KERNEL_GLOBAL_WORK_SIZE
        SpillMemSizeIntel = 0x4109,                       // CL_KERNEL_SPILL_MEM_SIZE_INTEL
    }

    #endregion

    public sealed unsafe class Kernel(Handle<Kernel> handle)
        : BaseObject<Kernel, KernelInfo>(handle), IReify<Kernel>
    {
        #region Kernel Arguments

        public void SetArg(int index, nuint size, void* value)
        {
            OpenCLNative.SetKernelArg(Handle, index, size, value).ThrowIfUnsuccessful();
        }

        public void SetArg<T>(int index, ref T value) where T : unmanaged
        {
            fixed (T* ptr = &value)
            {
                SetArg(index, (nuint)sizeof(T), ptr);
            }
        }

        public void SetArg<T>(int index, T value) where T : unmanaged
        {
            SetArg(index, ref value);
        }

        public void SetArgBuffer(int index, Buffer buffer)
        {
            var handleValue = buffer.Handle.Value;
            SetArg(index, (nuint)sizeof(nint), &handleValue);
        }

        public void SetArgLocal(int index, nuint size)      // todo read doc
        {
            SetArg(index, size, null);
        }

        #endregion

        #region Kernel Information

        public string GetArgName(int argIndex) => GetStringArgInfo(argIndex, KernelArgInfo.Name);

        public KernelArgAddressQualifier GetArgAddressQualifier(int argIndex) => GetArgInfo<KernelArgAddressQualifier>(argIndex, KernelArgInfo.AddressQualifier);

        public KernelArgAccessQualifier GetArgAccessQualifier(int argIndex) => GetArgInfo<KernelArgAccessQualifier>(argIndex, KernelArgInfo.AccessQualifier);

        private string GetStringArgInfo(int argIndex, KernelArgInfo paramName)
        {
            var length = GetArgInfoByteSize(argIndex, paramName);
            if (length == 0) return string.Empty;

            var buffer = stackalloc byte[length];
            GetArgInfo(argIndex, paramName, (nuint)length, buffer, out _);
            return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buffer, length - 1));
        }

        private int GetArgInfoByteSize(int argIndex, KernelArgInfo paramName)
        {
            GetArgInfo(argIndex, paramName, 0, null, out var size);
            return (int)size;
        }

        private void GetArgInfo(int argIndex, KernelArgInfo paramName, nuint paramValueSize, void* paramValue, out nuint paramValueSizeRet)
        {
            OpenCLNative.GetKernelArgInfo(Handle, argIndex, paramName, paramValueSize, paramValue, out paramValueSizeRet)
                .ThrowIfUnsuccessful();
        }

        private T GetArgInfo<T>(int argIndex, KernelArgInfo paramName) where T : unmanaged
        {
            if (TryGetArgInfo(argIndex, paramName, out T value)) return value;
            throw new InvalidOperationException();
        }

        private bool TryGetArgInfo<T>(int argIndex, KernelArgInfo paramName, out T value) where T : unmanaged
        {
            var val = value = default;
            GetArgInfo(argIndex, paramName, (nuint)sizeof(T), &val, out var size);
            if ((int)size != sizeof(T)) return false;
            value = val;
            return true;
        }

        #endregion

        public Kernel Clone()
        {
            var handle = OpenCLNative.CloneKernel(Handle, out var errorCode);
            errorCode.ThrowIfUnsuccessful();
            return Reify(handle);
        }

        protected override void RetainHook() => OpenCLNative.RetainKernel(Handle).ThrowIfUnsuccessful();
        protected override void ReleaseHook(Handle<Kernel> tmpHandle) => OpenCLNative.ReleaseKernel(tmpHandle).ThrowIfUnsuccessful();

        protected override void GetInfo(KernelInfo paramName, nuint paramValueSize, void* paramValue, out nuint paramValueSizeRet)
            => OpenCLNative.GetKernelInfo(Handle, paramName, paramValueSize, paramValue, out paramValueSizeRet).ThrowIfUnsuccessful();

        public static Kernel Reify(Handle<Kernel> handle) => new(handle);
    }
}

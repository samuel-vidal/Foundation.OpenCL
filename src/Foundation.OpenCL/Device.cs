using System;

namespace Foundation.OpenCL
{
    #region Constants

    public enum DeviceInfo
    {
        Type = 0x1000,                                                            // CL_DEVICE_TYPE
        VendorID = 0x1001,                                                        // CL_DEVICE_VENDOR_ID
        MaxComputeUnits = 0x1002,                                                 // CL_DEVICE_MAX_COMPUTE_UNITS
        MaxWorkItemDimensions = 0x1003,                                           // CL_DEVICE_MAX_WORK_ITEM_DIMENSIONS
        MaxWorkGroupSize = 0x1004,                                                // CL_DEVICE_MAX_WORK_GROUP_SIZE
        MaxWorkItemSizes = 0x1005,                                                // CL_DEVICE_MAX_WORK_ITEM_SIZES
        PreferredVectorWidthChar = 0x1006,                                        // CL_DEVICE_PREFERRED_VECTOR_WIDTH_CHAR
        PreferredVectorWidthShort = 0x1007,                                       // CL_DEVICE_PREFERRED_VECTOR_WIDTH_SHORT
        PreferredVectorWidthInt = 0x1008,                                         // CL_DEVICE_PREFERRED_VECTOR_WIDTH_INT
        PreferredVectorWidthLong = 0x1009,                                        // CL_DEVICE_PREFERRED_VECTOR_WIDTH_LONG
        PreferredVectorWidthFloat = 0x100A,                                       // CL_DEVICE_PREFERRED_VECTOR_WIDTH_FLOAT
        PreferredVectorWidthDouble = 0x100B,                                      // CL_DEVICE_PREFERRED_VECTOR_WIDTH_DOUBLE
        MaxClockFrequency = 0x100C,                                               // CL_DEVICE_MAX_CLOCK_FREQUENCY
        AddressBits = 0x100D,                                                     // CL_DEVICE_ADDRESS_BITS
        MaxReadImageArgs = 0x100E,                                                // CL_DEVICE_MAX_READ_IMAGE_ARGS
        MaxWriteImageArgs = 0x100F,                                               // CL_DEVICE_MAX_WRITE_IMAGE_ARGS
        MaxMemAllocSize = 0x1010,                                                 // CL_DEVICE_MAX_MEM_ALLOC_SIZE
        Image2DMaxWidth = 0x1011,                                                 // CL_DEVICE_IMAGE2D_MAX_WIDTH
        Image2DMaxHeight = 0x1012,                                                // CL_DEVICE_IMAGE2D_MAX_HEIGHT
        Image3DMaxWidth = 0x1013,                                                 // CL_DEVICE_IMAGE3D_MAX_WIDTH
        Image3DMaxHeight = 0x1014,                                                // CL_DEVICE_IMAGE3D_MAX_HEIGHT
        Image3DMaxDepth = 0x1015,                                                 // CL_DEVICE_IMAGE3D_MAX_DEPTH
        ImageSupport = 0x1016,                                                    // CL_DEVICE_IMAGE_SUPPORT
        MaxParameterSize = 0x1017,                                                // CL_DEVICE_MAX_PARAMETER_SIZE
        MaxSamplers = 0x1018,                                                     // CL_DEVICE_MAX_SAMPLERS
        MemBaseAddrAlign = 0x1019,                                                // CL_DEVICE_MEM_BASE_ADDR_ALIGN
        SingleFPConfig = 0x101B,                                                  // CL_DEVICE_SINGLE_FP_CONFIG
        GlobalMemCacheType = 0x101C,                                              // CL_DEVICE_GLOBAL_MEM_CACHE_TYPE
        GlobalMemCachelineSize = 0x101D,                                          // CL_DEVICE_GLOBAL_MEM_CACHELINE_SIZE
        GlobalMemCacheSize = 0x101E,                                              // CL_DEVICE_GLOBAL_MEM_CACHE_SIZE
        GlobalMemSize = 0x101F,                                                   // CL_DEVICE_GLOBAL_MEM_SIZE
        MaxConstantBufferSize = 0x1020,                                           // CL_DEVICE_MAX_CONSTANT_BUFFER_SIZE
        MaxConstantArgs = 0x1021,                                                 // CL_DEVICE_MAX_CONSTANT_ARGS
        LocalMemType = 0x1022,                                                    // CL_DEVICE_LOCAL_MEM_TYPE
        LocalMemSize = 0x1023,                                                    // CL_DEVICE_LOCAL_MEM_SIZE
        ErrorCorrectionSupport = 0x1024,                                          // CL_DEVICE_ERROR_CORRECTION_SUPPORT
        ProfilingTimerResolution = 0x1025,                                        // CL_DEVICE_PROFILING_TIMER_RESOLUTION
        EndianLittle = 0x1026,                                                    // CL_DEVICE_ENDIAN_LITTLE
        Available = 0x1027,                                                       // CL_DEVICE_AVAILABLE
        CompilerAvailable = 0x1028,                                               // CL_DEVICE_COMPILER_AVAILABLE
        ExecutionCapabilities = 0x1029,                                           // CL_DEVICE_EXECUTION_CAPABILITIES
        Name = 0x102B,                                                            // CL_DEVICE_NAME
        Vendor = 0x102C,                                                          // CL_DEVICE_VENDOR
        DriverVersion = 0x102D,                                                   // CL_DRIVER_VERSION
        Profile = 0x102E,                                                         // CL_DEVICE_PROFILE
        Version = 0x102F,                                                         // CL_DEVICE_VERSION
        Extensions = 0x1030,                                                      // CL_DEVICE_EXTENSIONS
        Platform = 0x1031,                                                        // CL_DEVICE_PLATFORM
        MinDataTypeAlignSize = 0x101A,                                            // CL_DEVICE_MIN_DATA_TYPE_ALIGN_SIZE
        QueueProperties = 0x102A,                                                 // CL_DEVICE_QUEUE_PROPERTIES
        PreferredVectorWidthHalf = 0x1034,                                        // CL_DEVICE_PREFERRED_VECTOR_WIDTH_HALF
        NativeVectorWidthChar = 0x1036,                                           // CL_DEVICE_NATIVE_VECTOR_WIDTH_CHAR
        NativeVectorWidthShort = 0x1037,                                          // CL_DEVICE_NATIVE_VECTOR_WIDTH_SHORT
        NativeVectorWidthInt = 0x1038,                                            // CL_DEVICE_NATIVE_VECTOR_WIDTH_INT
        NativeVectorWidthLong = 0x1039,                                           // CL_DEVICE_NATIVE_VECTOR_WIDTH_LONG
        NativeVectorWidthFloat = 0x103A,                                          // CL_DEVICE_NATIVE_VECTOR_WIDTH_FLOAT
        NativeVectorWidthDouble = 0x103B,                                         // CL_DEVICE_NATIVE_VECTOR_WIDTH_DOUBLE
        NativeVectorWidthHalf = 0x103C,                                           // CL_DEVICE_NATIVE_VECTOR_WIDTH_HALF
        HostUnifiedMemory = 0x1035,                                               // CL_DEVICE_HOST_UNIFIED_MEMORY
        OpenclCVersion = 0x103D,                                                  // CL_DEVICE_OPENCL_C_VERSION
        DoubleFPConfig = 0x1032,                                                  // CL_DEVICE_DOUBLE_FP_CONFIG
        LinkerAvailable = 0x103E,                                                 // CL_DEVICE_LINKER_AVAILABLE
        BuiltInKernels = 0x103F,                                                  // CL_DEVICE_BUILT_IN_KERNELS
        ImageMaxBufferSize = 0x1040,                                              // CL_DEVICE_IMAGE_MAX_BUFFER_SIZE
        ImageMaxArraySize = 0x1041,                                               // CL_DEVICE_IMAGE_MAX_ARRAY_SIZE
        ParentDevice = 0x1042,                                                    // CL_DEVICE_PARENT_DEVICE
        PartitionMaxSubDevices = 0x1043,                                          // CL_DEVICE_PARTITION_MAX_SUB_DEVICES
        PartitionProperties = 0x1044,                                             // CL_DEVICE_PARTITION_PROPERTIES
        PartitionAffinityDomain = 0x1045,                                         // CL_DEVICE_PARTITION_AFFINITY_DOMAIN
        PartitionType = 0x1046,                                                   // CL_DEVICE_PARTITION_TYPE
        ReferenceCount = 0x1047,                                                  // CL_DEVICE_REFERENCE_COUNT
        PreferredInteropUserSync = 0x1048,                                        // CL_DEVICE_PREFERRED_INTEROP_USER_SYNC
        PrintfBufferSize = 0x1049,                                                // CL_DEVICE_PRINTF_BUFFER_SIZE
        QueueOnHostProperties = 0x102A,                                           // CL_DEVICE_QUEUE_ON_HOST_PROPERTIES
        ImagePitchAlignment = 0x104A,                                             // CL_DEVICE_IMAGE_PITCH_ALIGNMENT
        ImageBaseAddressAlignment = 0x104B,                                       // CL_DEVICE_IMAGE_BASE_ADDRESS_ALIGNMENT
        MaxReadWriteImageArgs = 0x104C,                                           // CL_DEVICE_MAX_READ_WRITE_IMAGE_ARGS
        MaxGlobalVariableSize = 0x104D,                                           // CL_DEVICE_MAX_GLOBAL_VARIABLE_SIZE
        QueueOnDeviceProperties = 0x104E,                                         // CL_DEVICE_QUEUE_ON_DEVICE_PROPERTIES
        QueueOnDevicePreferredSize = 0x104F,                                      // CL_DEVICE_QUEUE_ON_DEVICE_PREFERRED_SIZE
        QueueOnDeviceMaxSize = 0x1050,                                            // CL_DEVICE_QUEUE_ON_DEVICE_MAX_SIZE
        MaxOnDeviceQueues = 0x1051,                                               // CL_DEVICE_MAX_ON_DEVICE_QUEUES
        MaxOnDeviceEvents = 0x1052,                                               // CL_DEVICE_MAX_ON_DEVICE_EVENTS
        SvmCapabilities = 0x1053,                                                 // CL_DEVICE_SVM_CAPABILITIES
        GlobalVariablePreferredTotalSize = 0x1054,                                // CL_DEVICE_GLOBAL_VARIABLE_PREFERRED_TOTAL_SIZE
        MaxPipeArgs = 0x1055,                                                     // CL_DEVICE_MAX_PIPE_ARGS
        PipeMaxActiveReservations = 0x1056,                                       // CL_DEVICE_PIPE_MAX_ACTIVE_RESERVATIONS
        PipeMaxPacketSize = 0x1057,                                               // CL_DEVICE_PIPE_MAX_PACKET_SIZE
        PreferredPlatformAtomicAlignment = 0x1058,                                // CL_DEVICE_PREFERRED_PLATFORM_ATOMIC_ALIGNMENT
        PreferredGlobalAtomicAlignment = 0x1059,                                  // CL_DEVICE_PREFERRED_GLOBAL_ATOMIC_ALIGNMENT
        PreferredLocalAtomicAlignment = 0x105A,                                   // CL_DEVICE_PREFERRED_LOCAL_ATOMIC_ALIGNMENT
        ILVersion = 0x105B,                                                       // CL_DEVICE_IL_VERSION
        MaxNumSubGroups = 0x105C,                                                 // CL_DEVICE_MAX_NUM_SUB_GROUPS
        SubGroupIndependentForwardProgress = 0x105D,                              // CL_DEVICE_SUB_GROUP_INDEPENDENT_FORWARD_PROGRESS
        AtomicMemoryCapabilities = 0x1063,                                        // CL_DEVICE_ATOMIC_MEMORY_CAPABILITIES
        AtomicFenceCapabilities = 0x1064,                                         // CL_DEVICE_ATOMIC_FENCE_CAPABILITIES
        NonUniformWorkGroupSupport = 0x1065,                                      // CL_DEVICE_NON_UNIFORM_WORK_GROUP_SUPPORT
        OpenclCAllVersions = 0x1066,                                              // CL_DEVICE_OPENCL_C_ALL_VERSIONS
        WorkGroupCollectiveFunctionsSupport = 0x1068,                             // CL_DEVICE_WORK_GROUP_COLLECTIVE_FUNCTIONS_SUPPORT
        GenericAddressSpaceSupport = 0x1069,                                      // CL_DEVICE_GENERIC_ADDRESS_SPACE_SUPPORT
        OpenclCFeatures = 0x106F,                                                 // CL_DEVICE_OPENCL_C_FEATURES
        DeviceEnqueueCapabilities = 0x1070,                                       // CL_DEVICE_DEVICE_ENQUEUE_CAPABILITIES
        PipeSupport = 0x1071,                                                     // CL_DEVICE_PIPE_SUPPORT
        NumericVersion = 0x105E,                                                  // CL_DEVICE_NUMERIC_VERSION
        ExtensionsWithVersion = 0x1060,                                           // CL_DEVICE_EXTENSIONS_WITH_VERSION
        IlsWithVersion = 0x1061,                                                  // CL_DEVICE_ILS_WITH_VERSION
        BuiltInKernelsWithVersion = 0x1062,                                       // CL_DEVICE_BUILT_IN_KERNELS_WITH_VERSION
        PreferredWorkGroupSizeMultiple = 0x1067,                                  // CL_DEVICE_PREFERRED_WORK_GROUP_SIZE_MULTIPLE
        LatestConformanceVersionPassed = 0x1072,                                  // CL_DEVICE_LATEST_CONFORMANCE_VERSION_PASSED
        HalfFPConfig = 0x1033,                                                    // CL_DEVICE_HALF_FP_CONFIG
        ILVersionKhr = 0x105B,                                                    // CL_DEVICE_IL_VERSION_KHR
        ImagePitchAlignmentKhr = 0x104A,                                          // CL_DEVICE_IMAGE_PITCH_ALIGNMENT_KHR
        ImageBaseAddressAlignmentKhr = 0x104B,                                    // CL_DEVICE_IMAGE_BASE_ADDRESS_ALIGNMENT_KHR
        TerminateCapabilityKhr = 0x2031,                                          // CL_DEVICE_TERMINATE_CAPABILITY_KHR
        SpirVersions = 0x40E0,                                                    // CL_DEVICE_SPIR_VERSIONS
        ComputeCapabilityMajorNV = 0x4000,                                        // CL_DEVICE_COMPUTE_CAPABILITY_MAJOR_NV
        ComputeCapabilityMinorNV = 0x4001,                                        // CL_DEVICE_COMPUTE_CAPABILITY_MINOR_NV
        RegistersPerBlockNV = 0x4002,                                             // CL_DEVICE_REGISTERS_PER_BLOCK_NV
        WarpSizeNV = 0x4003,                                                      // CL_DEVICE_WARP_SIZE_NV
        GpuOverlapNV = 0x4004,                                                    // CL_DEVICE_GPU_OVERLAP_NV
        KernelExecTimeoutNV = 0x4005,                                             // CL_DEVICE_KERNEL_EXEC_TIMEOUT_NV
        IntegratedMemoryNV = 0x4006,                                              // CL_DEVICE_INTEGRATED_MEMORY_NV
        ProfilingTimerOffsetAmd = 0x4036,                                         // CL_DEVICE_PROFILING_TIMER_OFFSET_AMD
        TopologyAmd = 0x4037,                                                     // CL_DEVICE_TOPOLOGY_AMD
        BoardNameAmd = 0x4038,                                                    // CL_DEVICE_BOARD_NAME_AMD
        GlobalFreeMemoryAmd = 0x4039,                                             // CL_DEVICE_GLOBAL_FREE_MEMORY_AMD
        SimdPerComputeUnitAmd = 0x4040,                                           // CL_DEVICE_SIMD_PER_COMPUTE_UNIT_AMD
        SimdWidthAmd = 0x4041,                                                    // CL_DEVICE_SIMD_WIDTH_AMD
        SimdInstructionWidthAmd = 0x4042,                                         // CL_DEVICE_SIMD_INSTRUCTION_WIDTH_AMD
        WavefrontWidthAmd = 0x4043,                                               // CL_DEVICE_WAVEFRONT_WIDTH_AMD
        GlobalMemChannelsAmd = 0x4044,                                            // CL_DEVICE_GLOBAL_MEM_CHANNELS_AMD
        GlobalMemChannelBanksAmd = 0x4045,                                        // CL_DEVICE_GLOBAL_MEM_CHANNEL_BANKS_AMD
        GlobalMemChannelBankWidthAmd = 0x4046,                                    // CL_DEVICE_GLOBAL_MEM_CHANNEL_BANK_WIDTH_AMD
        LocalMemSizePerComputeUnitAmd = 0x4047,                                   // CL_DEVICE_LOCAL_MEM_SIZE_PER_COMPUTE_UNIT_AMD
        LocalMemBanksAmd = 0x4048,                                                // CL_DEVICE_LOCAL_MEM_BANKS_AMD
        ThreadTraceSupportedAmd = 0x4049,                                         // CL_DEVICE_THREAD_TRACE_SUPPORTED_AMD
        GfxipMajorAmd = 0x404A,                                                   // CL_DEVICE_GFXIP_MAJOR_AMD
        GfxipMinorAmd = 0x404B,                                                   // CL_DEVICE_GFXIP_MINOR_AMD
        AvailableAsyncQueuesAmd = 0x404C,                                         // CL_DEVICE_AVAILABLE_ASYNC_QUEUES_AMD
        PreferredWorkGroupSizeAmd = 0x4030,                                       // CL_DEVICE_PREFERRED_WORK_GROUP_SIZE_AMD
        MaxWorkGroupSizeAmd = 0x4031,                                             // CL_DEVICE_MAX_WORK_GROUP_SIZE_AMD
        PreferredConstantBufferSizeAmd = 0x4033,                                  // CL_DEVICE_PREFERRED_CONSTANT_BUFFER_SIZE_AMD
        PcieIDAmd = 0x4034,                                                       // CL_DEVICE_PCIE_ID_AMD
        ParentDeviceExt = 0x4054,                                                 // CL_DEVICE_PARENT_DEVICE_EXT
        PartitionTypesExt = 0x4055,                                               // CL_DEVICE_PARTITION_TYPES_EXT
        AffinityDomainsExt = 0x4056,                                              // CL_DEVICE_AFFINITY_DOMAINS_EXT
        ReferenceCountExt = 0x4057,                                               // CL_DEVICE_REFERENCE_COUNT_EXT
        PartitionStyleExt = 0x4058,                                               // CL_DEVICE_PARTITION_STYLE_EXT
        ExtMemPaddingInBytesQCom = 0x40A0,                                        // CL_DEVICE_EXT_MEM_PADDING_IN_BYTES_QCOM
        PageSizeQCom = 0x40A1,                                                    // CL_DEVICE_PAGE_SIZE_QCOM
        MaxNamedBarrierCountKhr = 0x2035,                                         // CL_DEVICE_MAX_NAMED_BARRIER_COUNT_KHR
        SvmCapabilitiesArm = 0x40B6,                                              // CL_DEVICE_SVM_CAPABILITIES_ARM
        ComputeUnitsBitfieldArm = 0x40BF,                                         // CL_DEVICE_COMPUTE_UNITS_BITFIELD_ARM
        MEVersionIntel = 0x407E,                                                  // CL_DEVICE_ME_VERSION_INTEL
        SimultaneousInteropsIntel = 0x4104,                                       // CL_DEVICE_SIMULTANEOUS_INTEROPS_INTEL
        NumSimultaneousInteropsIntel = 0x4105,                                    // CL_DEVICE_NUM_SIMULTANEOUS_INTEROPS_INTEL
        SubGroupSizesIntel = 0x4108,                                              // CL_DEVICE_SUB_GROUP_SIZES_INTEL
        PlanarYuvMaxWidthIntel = 0x417E,                                          // CL_DEVICE_PLANAR_YUV_MAX_WIDTH_INTEL
        PlanarYuvMaxHeightIntel = 0x417F,                                         // CL_DEVICE_PLANAR_YUV_MAX_HEIGHT_INTEL
        AvcMEVersionIntel = 0x410B,                                               // CL_DEVICE_AVC_ME_VERSION_INTEL
        AvcMESupportsTextureSamplerUseIntel = 0x410C,                             // CL_DEVICE_AVC_ME_SUPPORTS_TEXTURE_SAMPLER_USE_INTEL
        AvcMESupportsPreemptionIntel = 0x410D,                                    // CL_DEVICE_AVC_ME_SUPPORTS_PREEMPTION_INTEL
        HostMemCapabilitiesIntel = 0x4190,                                        // CL_DEVICE_HOST_MEM_CAPABILITIES_INTEL
        DeviceMemCapabilitiesIntel = 0x4191,                                      // CL_DEVICE_DEVICE_MEM_CAPABILITIES_INTEL
        SingleDeviceSharedMemCapabilitiesIntel = 0x4192,                          // CL_DEVICE_SINGLE_DEVICE_SHARED_MEM_CAPABILITIES_INTEL
        CrossDeviceSharedMemCapabilitiesIntel = 0x4193,                           // CL_DEVICE_CROSS_DEVICE_SHARED_MEM_CAPABILITIES_INTEL
        SharedSystemMemCapabilitiesIntel = 0x4194,                                // CL_DEVICE_SHARED_SYSTEM_MEM_CAPABILITIES_INTEL
        UuidKhr = 0x106A,                                                         // CL_DEVICE_UUID_KHR
        DriverUuidKhr = 0x106B,                                                   // CL_DRIVER_UUID_KHR
        LuidValidKhr = 0x106C,                                                    // CL_DEVICE_LUID_VALID_KHR
        LuidKhr = 0x106D,                                                         // CL_DEVICE_LUID_KHR
        NodeMaskKhr = 0x106E,                                                     // CL_DEVICE_NODE_MASK_KHR
        SchedulingControlsCapabilitiesArm = 0x41E4,                               // CL_DEVICE_SCHEDULING_CONTROLS_CAPABILITIES_ARM
        SupportedRegisterAllocationsArm = 0x41EB,                                 // CL_DEVICE_SUPPORTED_REGISTER_ALLOCATIONS_ARM
        MaxWarpCountArm = 0x41EA,                                                 // CL_DEVICE_MAX_WARP_COUNT_ARM
        CxxForOpenclNumericVersionExt = 0x4230,                                   // CL_DEVICE_CXX_FOR_OPENCL_NUMERIC_VERSION_EXT
        NumericVersionKhr = 0x105E,                                               // CL_DEVICE_NUMERIC_VERSION_KHR
        OpenclCNumericVersionKhr = 0x105F,                                        // CL_DEVICE_OPENCL_C_NUMERIC_VERSION_KHR
        ExtensionsWithVersionKhr = 0x1060,                                        // CL_DEVICE_EXTENSIONS_WITH_VERSION_KHR
        IlsWithVersionKhr = 0x1061,                                               // CL_DEVICE_ILS_WITH_VERSION_KHR
        BuiltInKernelsWithVersionKhr = 0x1062,                                    // CL_DEVICE_BUILT_IN_KERNELS_WITH_VERSION_KHR
        MemoryCapabilitiesImg = 0x40D8,                                           // CL_DEVICE_MEMORY_CAPABILITIES_IMG
        ControlledTerminationCapabilitiesArm = 0x41EE,                            // CL_DEVICE_CONTROLLED_TERMINATION_CAPABILITIES_ARM
        QueueFamilyPropertiesIntel = 0x418B,                                      // CL_DEVICE_QUEUE_FAMILY_PROPERTIES_INTEL
        PciBusInfoKhr = 0x410F,                                                   // CL_DEVICE_PCI_BUS_INFO_KHR
        IPVersionIntel = 0x4250,                                                  // CL_DEVICE_IP_VERSION_INTEL
        IDIntel = 0x4251,                                                         // CL_DEVICE_ID_INTEL
        NumSlicesIntel = 0x4252,                                                  // CL_DEVICE_NUM_SLICES_INTEL
        NumSubSlicesPerSliceIntel = 0x4253,                                       // CL_DEVICE_NUM_SUB_SLICES_PER_SLICE_INTEL
        NumEusPerSubSliceIntel = 0x4254,                                          // CL_DEVICE_NUM_EUS_PER_SUB_SLICE_INTEL
        NumThreadsPerEUIntel = 0x4255,                                            // CL_DEVICE_NUM_THREADS_PER_EU_INTEL
        FeatureCapabilitiesIntel = 0x4256,                                        // CL_DEVICE_FEATURE_CAPABILITIES_INTEL
        IntegerDotProductCapabilitiesKhr = 0x1073,                                // CL_DEVICE_INTEGER_DOT_PRODUCT_CAPABILITIES_KHR
        IntegerDotProductAccelerationProperties8BitKhr = 0x1074,                  // CL_DEVICE_INTEGER_DOT_PRODUCT_ACCELERATION_PROPERTIES_8BIT_KHR
        IntegerDotProductAccelerationProperties4x8BitPackedKhr = 0x1075,          // CL_DEVICE_INTEGER_DOT_PRODUCT_ACCELERATION_PROPERTIES_4x8BIT_PACKED_KHR
        SemaphoreTypesKhr = 0x204C,                                               // CL_DEVICE_SEMAPHORE_TYPES_KHR
        SemaphoreImportHandleTypesKhr = 0x204D,                                   // CL_DEVICE_SEMAPHORE_IMPORT_HANDLE_TYPES_KHR
        SemaphoreExportHandleTypesKhr = 0x204E,                                   // CL_DEVICE_SEMAPHORE_EXPORT_HANDLE_TYPES_KHR
        ExternalMemoryImportHandleTypesKhr = 0x204F,                              // CL_DEVICE_EXTERNAL_MEMORY_IMPORT_HANDLE_TYPES_KHR
        ExternalMemoryImportAssumeLinearImagesHandleTypesKhr = 0x2052,            // CL_DEVICE_EXTERNAL_MEMORY_IMPORT_ASSUME_LINEAR_IMAGES_HANDLE_TYPES_KHR
        CommandBufferCapabilitiesKhr = 0x12A9,                                    // CL_DEVICE_COMMAND_BUFFER_CAPABILITIES_KHR
        CommandBufferRequiredQueuePropertiesKhr = 0x12AA,                         // CL_DEVICE_COMMAND_BUFFER_REQUIRED_QUEUE_PROPERTIES_KHR
        SingleFPAtomicCapabilitiesExt = 0x4231,                                   // CL_DEVICE_SINGLE_FP_ATOMIC_CAPABILITIES_EXT
        DoubleFPAtomicCapabilitiesExt = 0x4232,                                   // CL_DEVICE_DOUBLE_FP_ATOMIC_CAPABILITIES_EXT
        HalfFPAtomicCapabilitiesExt = 0x4233,                                     // CL_DEVICE_HALF_FP_ATOMIC_CAPABILITIES_EXT
        JobSlotsArm = 0x41E0,                                                     // CL_DEVICE_JOB_SLOTS_ARM
        MutableDispatchCapabilitiesKhr = 0x12B0,                                  // CL_DEVICE_MUTABLE_DISPATCH_CAPABILITIES_KHR
        CommandBufferNumSyncDevicesKhr = 0x12AB,                                  // CL_DEVICE_COMMAND_BUFFER_NUM_SYNC_DEVICES_KHR
        CommandBufferSyncDevicesKhr = 0x12AC,                                     // CL_DEVICE_COMMAND_BUFFER_SYNC_DEVICES_KHR
        KernelClockCapabilitiesKhr = 0x1076,                                      // CL_DEVICE_KERNEL_CLOCK_CAPABILITIES_KHR
    }

    [Flags]
    public enum DeviceType : ulong
    {
        None = 0,
        Default = 0x1,              // CL_DEVICE_TYPE_DEFAULT
        Cpu = 0x2,                  // CL_DEVICE_TYPE_CPU
        Gpu = 0x4,                  // CL_DEVICE_TYPE_GPU
        Accelerator = 0x8,          // CL_DEVICE_TYPE_ACCELERATOR
        All = 0xFFFFFFFF,           // CL_DEVICE_TYPE_ALL
        Custom = 0x10,              // CL_DEVICE_TYPE_CUSTOM
    }

    #endregion

    public enum PartitionProperty : ulong
    {
        // TBA
    }

    public readonly struct PartitionConfig(PartitionProperty property, ulong value)
    {
        public PartitionProperty Property { get; } = property;
        public ulong Value { get; } = value;
    }

    public sealed unsafe class Device(Handle<Device> handle)
        : BaseObject<Device, DeviceInfo>(handle), IReify<Device>
    {
        public Device[] CreateSubDevices(params ReadOnlySpan<PartitionConfig> properties)
        {
            var partitionProperties = stackalloc ulong[properties.Length * 2 + 1];
            for (int i = 0, pos = 0; i < properties.Length; i++)
            {
                partitionProperties[pos++] = (ulong)properties[i].Property;
                partitionProperties[pos++] = properties[i].Value;
                partitionProperties[pos] = 0;       // intentional
            }

            OpenCLNative.CreateSubDevices(Handle, null, 0, null, out var count).ThrowIfUnsuccessful();
            if (count == 0) return [];

            var handles = stackalloc Handle<Device>[count];
            OpenCLNative.CreateSubDevices(Handle, partitionProperties, count, handles, out _).ThrowIfUnsuccessful();

            var result = new Device[count];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = Reify(handles[i]);
            }
            return result;
        }

        public void GetDeviceAndHostTimer(out ulong deviceTimestamp, out ulong hostTimestamp)
        {
            OpenCLNative.GetDeviceAndHostTimer(Handle, out deviceTimestamp, out hostTimestamp)
                .ThrowIfUnsuccessful();
        }

        public ulong GetHostTimer()
        {
            OpenCLNative.GetHostTimer(Handle, out var hostTimestamp)
                .ThrowIfUnsuccessful();
            return hostTimestamp;
        }

        protected override void GetInfo(DeviceInfo paramName, nuint paramValueSize, void* paramValue, out nuint paramValueSizeRet)
            => OpenCLNative.GetDeviceInfo(Handle, paramName, paramValueSize, paramValue, out paramValueSizeRet).ThrowIfUnsuccessful();

        protected override void RetainHook() => OpenCLNative.RetainDevice(Handle).ThrowIfUnsuccessful();
        protected override void ReleaseHook(Handle<Device> tmpHandle) => OpenCLNative.ReleaseDevice(tmpHandle).ThrowIfUnsuccessful();

        public static Device Reify(Handle<Device> handle) => new (handle);
    }



    public static class DeviceExtensions
    {
        public static int GetReferenceCount(this Device queue)
            => queue.GetInfo<int>(DeviceInfo.ReferenceCount);
    }
}
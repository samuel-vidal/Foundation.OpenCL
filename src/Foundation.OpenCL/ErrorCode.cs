using System;
using System.Runtime.CompilerServices;

namespace Foundation.OpenCL
{
    #region Constants

    public enum ErrorCode
    {
        Success = 0,                                            // CL_SUCCESS
        DeviceNotFound = -1,                                    // CL_DEVICE_NOT_FOUND
        DeviceNotAvailable = -2,                                // CL_DEVICE_NOT_AVAILABLE
        CompilerNotAvailable = -3,                              // CL_COMPILER_NOT_AVAILABLE
        MemObjectAllocationFailure = -4,                        // CL_MEM_OBJECT_ALLOCATION_FAILURE
        OutOfResources = -5,                                    // CL_OUT_OF_RESOURCES
        OutOfHostMemory = -6,                                   // CL_OUT_OF_HOST_MEMORY
        ProfilingInfoNotAvailable = -7,                         // CL_PROFILING_INFO_NOT_AVAILABLE
        MemCopyOverlap = -8,                                    // CL_MEM_COPY_OVERLAP
        ImageFormatMismatch = -9,                               // CL_IMAGE_FORMAT_MISMATCH
        ImageFormatNotSupported = -10,                          // CL_IMAGE_FORMAT_NOT_SUPPORTED
        BuildProgramFailure = -11,                              // CL_BUILD_PROGRAM_FAILURE
        MapFailure = -12,                                       // CL_MAP_FAILURE
        InvalidValue = -30,                                     // CL_INVALID_VALUE
        InvalidDeviceType = -31,                                // CL_INVALID_DEVICE_TYPE
        InvalidPlatform = -32,                                  // CL_INVALID_PLATFORM
        InvalidDevice = -33,                                    // CL_INVALID_DEVICE
        InvalidContext = -34,                                   // CL_INVALID_CONTEXT
        InvalidQueueProperties = -35,                           // CL_INVALID_QUEUE_PROPERTIES
        InvalidCommandQueue = -36,                              // CL_INVALID_COMMAND_QUEUE
        InvalidHostPtr = -37,                                   // CL_INVALID_HOST_PTR
        InvalidMemObject = -38,                                 // CL_INVALID_MEM_OBJECT
        InvalidImageFormatDescriptor = -39,                     // CL_INVALID_IMAGE_FORMAT_DESCRIPTOR
        InvalidImageSize = -40,                                 // CL_INVALID_IMAGE_SIZE
        InvalidSampler = -41,                                   // CL_INVALID_SAMPLER
        InvalidBinary = -42,                                    // CL_INVALID_BINARY
        InvalidBuildOptions = -43,                              // CL_INVALID_BUILD_OPTIONS
        InvalidProgram = -44,                                   // CL_INVALID_PROGRAM
        InvalidProgramExecutable = -45,                         // CL_INVALID_PROGRAM_EXECUTABLE
        InvalidKernelName = -46,                                // CL_INVALID_KERNEL_NAME
        InvalidKernelDefinition = -47,                          // CL_INVALID_KERNEL_DEFINITION
        InvalidKernel = -48,                                    // CL_INVALID_KERNEL
        InvalidArgIndex = -49,                                  // CL_INVALID_ARG_INDEX
        InvalidArgValue = -50,                                  // CL_INVALID_ARG_VALUE
        InvalidArgSize = -51,                                   // CL_INVALID_ARG_SIZE
        InvalidKernelArgs = -52,                                // CL_INVALID_KERNEL_ARGS
        InvalidWorkDimension = -53,                             // CL_INVALID_WORK_DIMENSION
        InvalidWorkGroupSize = -54,                             // CL_INVALID_WORK_GROUP_SIZE
        InvalidWorkItemSize = -55,                              // CL_INVALID_WORK_ITEM_SIZE
        InvalidGlobalOffset = -56,                              // CL_INVALID_GLOBAL_OFFSET
        InvalidEventWaitList = -57,                             // CL_INVALID_EVENT_WAIT_LIST
        InvalidEvent = -58,                                     // CL_INVALID_EVENT
        InvalidOperation = -59,                                 // CL_INVALID_OPERATION
        InvalidGlObject = -60,                                  // CL_INVALID_GL_OBJECT
        InvalidBufferSize = -61,                                // CL_INVALID_BUFFER_SIZE
        InvalidMipLevel = -62,                                  // CL_INVALID_MIP_LEVEL
        InvalidGlobalWorkSize = -63,                            // CL_INVALID_GLOBAL_WORK_SIZE
        MisalignedSubBufferOffset = -13,                        // CL_MISALIGNED_SUB_BUFFER_OFFSET
        ExecStatusErrorForEventsInWaitList = -14,               // CL_EXEC_STATUS_ERROR_FOR_EVENTS_IN_WAIT_LIST
        InvalidProperty = -64,                                  // CL_INVALID_PROPERTY
        CompileProgramFailure = -15,                            // CL_COMPILE_PROGRAM_FAILURE
        LinkerNotAvailable = -16,                               // CL_LINKER_NOT_AVAILABLE
        LinkProgramFailure = -17,                               // CL_LINK_PROGRAM_FAILURE
        DevicePartitionFailed = -18,                            // CL_DEVICE_PARTITION_FAILED
        KernelArgInfoNotAvailable = -19,                        // CL_KERNEL_ARG_INFO_NOT_AVAILABLE
        InvalidImageDescriptor = -65,                           // CL_INVALID_IMAGE_DESCRIPTOR
        InvalidCompilerOptions = -66,                           // CL_INVALID_COMPILER_OPTIONS
        InvalidLinkerOptions = -67,                             // CL_INVALID_LINKER_OPTIONS
        InvalidDevicePartitionCount = -68,                      // CL_INVALID_DEVICE_PARTITION_COUNT
        InvalidPipeSize = -69,                                  // CL_INVALID_PIPE_SIZE
        InvalidDeviceQueue = -70,                               // CL_INVALID_DEVICE_QUEUE
        InvalidSpecId = -71,                                    // CL_INVALID_SPEC_ID
        MaxSizeRestrictionExceeded = -72,                       // CL_MAX_SIZE_RESTRICTION_EXCEEDED
        InvalidD3D10DeviceKhr = -1002,                          // CL_INVALID_D3D10_DEVICE_KHR
        InvalidD3D10ResourceKhr = -1003,                        // CL_INVALID_D3D10_RESOURCE_KHR
        D3D10ResourceAlreadyAcquiredKhr = -1004,                // CL_D3D10_RESOURCE_ALREADY_ACQUIRED_KHR
        D3D10ResourceNotAcquiredKhr = -1005,                    // CL_D3D10_RESOURCE_NOT_ACQUIRED_KHR
        InvalidD3D11DeviceKhr = -1006,                          // CL_INVALID_D3D11_DEVICE_KHR
        InvalidD3D11ResourceKhr = -1007,                        // CL_INVALID_D3D11_RESOURCE_KHR
        D3D11ResourceAlreadyAcquiredKhr = -1008,                // CL_D3D11_RESOURCE_ALREADY_ACQUIRED_KHR
        D3D11ResourceNotAcquiredKhr = -1009,                    // CL_D3D11_RESOURCE_NOT_ACQUIRED_KHR
        InvalidDx9MediaAdapterKhr = -1010,                      // CL_INVALID_DX9_MEDIA_ADAPTER_KHR
        InvalidDx9MediaSurfaceKhr = -1011,                      // CL_INVALID_DX9_MEDIA_SURFACE_KHR
        Dx9MediaSurfaceAlreadyAcquiredKhr = -1012,              // CL_DX9_MEDIA_SURFACE_ALREADY_ACQUIRED_KHR
        Dx9MediaSurfaceNotAcquiredKhr = -1013,                  // CL_DX9_MEDIA_SURFACE_NOT_ACQUIRED_KHR
        InvalidEglObjectKhr = -1093,                            // CL_INVALID_EGL_OBJECT_KHR
        EglResourceNotAcquiredKhr = -1092,                      // CL_EGL_RESOURCE_NOT_ACQUIRED_KHR
        PlatformNotFoundKhr = -1001,                            // CL_PLATFORM_NOT_FOUND_KHR
        ContextTerminatedKhr = -1121,                           // CL_CONTEXT_TERMINATED_KHR
        DevicePartitionFailedExt = -1057,                       // CL_DEVICE_PARTITION_FAILED_EXT
        InvalidPartitionCountExt = -1058,                       // CL_INVALID_PARTITION_COUNT_EXT
        InvalidPartitionNameExt = -1059,                        // CL_INVALID_PARTITION_NAME_EXT
        GrallocResourceNotAcquiredImg = 16596,                  // CL_GRALLOC_RESOURCE_NOT_ACQUIRED_IMG
        InvalidGrallocObjectImg = 16597,                        // CL_INVALID_GRALLOC_OBJECT_IMG
        InvalidAcceleratorIntel = -1094,                        // CL_INVALID_ACCELERATOR_INTEL
        InvalidAcceleratorTypeIntel = -1095,                    // CL_INVALID_ACCELERATOR_TYPE_INTEL
        InvalidAcceleratorDescriptorIntel = -1096,              // CL_INVALID_ACCELERATOR_DESCRIPTOR_INTEL
        AcceleratorTypeNotSupportedIntel = -1097,               // CL_ACCELERATOR_TYPE_NOT_SUPPORTED_INTEL
        InvalidVaApiMediaAdapterIntel = -1098,                  // CL_INVALID_VA_API_MEDIA_ADAPTER_INTEL
        InvalidVaApiMediaSurfaceIntel = -1099,                  // CL_INVALID_VA_API_MEDIA_SURFACE_INTEL
        VaApiMediaSurfaceAlreadyAcquiredIntel = -1100,          // CL_VA_API_MEDIA_SURFACE_ALREADY_ACQUIRED_INTEL
        VaApiMediaSurfaceNotAcquiredIntel = -1101,              // CL_VA_API_MEDIA_SURFACE_NOT_ACQUIRED_INTEL
        InvalidDx9DeviceIntel = -1010,                          // CL_INVALID_DX9_DEVICE_INTEL
        InvalidDx9ResourceIntel = -1011,                        // CL_INVALID_DX9_RESOURCE_INTEL
        Dx9ResourceAlreadyAcquiredIntel = -1012,                // CL_DX9_RESOURCE_ALREADY_ACQUIRED_INTEL
        Dx9ResourceNotAcquiredIntel = -1013,                    // CL_DX9_RESOURCE_NOT_ACQUIRED_INTEL
        InvalidGlSharegroupReferenceKhr = -1000,                // CL_INVALID_GL_SHAREGROUP_REFERENCE_KHR
        CommandTerminatedItselfWithFailureArm = -1108,          // CL_COMMAND_TERMINATED_ITSELF_WITH_FAILURE_ARM
        InvalidSemaphoreKhr = -1142,                            // CL_INVALID_SEMAPHORE_KHR
        InvalidCommandBufferKhr = -1138,                        // CL_INVALID_COMMAND_BUFFER_KHR
        InvalidSyncPointWaitListKhr = -1139,                    // CL_INVALID_SYNC_POINT_WAIT_LIST_KHR
        IncompatibleCommandQueueKhr = -1140,                    // CL_INCOMPATIBLE_COMMAND_QUEUE_KHR
        InvalidMutableCommandKhr = -1141,                       // CL_INVALID_MUTABLE_COMMAND_KHR
        CancelledImg = -1126,                                   // CL_CANCELLED_IMG
    }

    #endregion

    public static class ErrorCodeExtensions
    {
        public static void ThrowIfUnsuccessful(this ErrorCode errorCode, string? message = null, [CallerMemberName] string methodName = "")
        {
            if (errorCode != ErrorCode.Success)
                throw new InvalidOperationException($"OpenCl {methodName} failed with error {errorCode} : 0x{(int)errorCode:X8}"
                    + (message != null ? $", message : [{message}]" : ""));
        }
    }
}
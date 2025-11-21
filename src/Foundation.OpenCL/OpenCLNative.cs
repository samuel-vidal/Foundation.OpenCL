using System.Runtime.InteropServices;

namespace Foundation.OpenCL
{
    public static unsafe class OpenCLNative
    {
        // Generic "OpenCL" to allow the .NET runtime to automatically
        // resolve the correct library file (.dll, .so, .dylib) across platforms.
        public const string OpenClLibrary = "OpenCL";

        #region Platform API

        // cl_int clGetPlatformIDs(cl_uint num_entries, cl_platform_id *platforms, cl_uint *num_platforms)
        [DllImport(OpenClLibrary, EntryPoint = "clGetPlatformIDs", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetPlatformIds(
            int numEntries,
            Handle<Platform>* platforms,
            out int numPlatforms);

        // cl_int clGetPlatformInfo(cl_platform_id platform, cl_platform_info param_name, size_t param_value_size, void *param_value, size_t *param_value_size_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clGetPlatformInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetPlatformInfo(
            Handle<Platform> platform,
            PlatformInfo paramName,
            nuint paramValueSize,
            void* paramValue,
            out nuint paramValueSizeRet);

        [DllImport(OpenClLibrary, EntryPoint = "clUnloadPlatformCompiler", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode UnloadPlatformCompiler(Handle<Platform> platform);

        #endregion

        #region Device API

        // cl_int clGetDeviceIDs(cl_platform_id platform, cl_device_type device_type, cl_uint num_entries, cl_device_id *devices, cl_uint *num_devices)
        [DllImport(OpenClLibrary, EntryPoint = "clGetDeviceIDs", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetDeviceIds(
            Handle<Platform> platform,
            DeviceType deviceType,
            int numEntries,
            Handle<Device>* devices,
            out int numDevices);

        // cl_int clGetDeviceInfo(cl_device_id device, cl_device_info param_name, size_t param_value_size, void *param_value, size_t *param_value_size_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clGetDeviceInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetDeviceInfo(
            Handle<Device> device,
            DeviceInfo paramName,
            nuint paramValueSize,
            void* paramValue,
            out nuint paramValueSizeRet);

        // cl_int clRetainDevice(cl_context context)
        [DllImport(OpenClLibrary, EntryPoint = "clRetainDevice", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode RetainDevice(Handle<Device> context);

        // cl_int clReleaseDevice(cl_context context)
        [DllImport(OpenClLibrary, EntryPoint = "clReleaseDevice", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode ReleaseDevice(Handle<Device> context);

        // cl_int clCreateSubDevices(cl_device_id in_device,
        //                           const cl_device_partition_property * properties,
        //                           cl_uint num_devices,
        //                           cl_device_id * out_devices,
        //                           cl_uint * num_devices_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clCreateSubDevices", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode CreateSubDevices(
            Handle<Device> inDevice,
            ulong* properties,           // array of cl_device_partition_property, null-terminated
            int numDevices,
            Handle<Device>* outDevices,
            out int numDevicesRet);

        // cl_int clGetDeviceAndHostTimer(cl_device_id device,
        //                                cl_ulong* device_timestamp,
        //                                cl_ulong* host_timestamp)
        [DllImport(OpenClLibrary, EntryPoint = "clGetDeviceAndHostTimer", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetDeviceAndHostTimer(
            Handle<Device> device,
            out ulong deviceTimestamp,
            out ulong hostTimestamp);

        // cl_int clGetHostTimer(cl_device_id device,
        //                       cl_ulong* host_timestamp)
        [DllImport(OpenClLibrary, EntryPoint = "clGetHostTimer", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetHostTimer(
            Handle<Device> device,
            out ulong hostTimestamp);

        #endregion

        #region Context API

        // cl_context clCreateContext(const cl_context_properties * properties,
        //                            cl_uint num_devices, const cl_device_id * devices,
        //                            void (CL_CALLBACK * pfn_notify)(...), void * user_data, cl_int * errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clCreateContext", CallingConvention = CallingConvention.Cdecl)]
        public static extern Handle<Context> CreateContext(
            ulong* properties,           // cl_context_properties[] (array of nint), null-terminated
            int numDevices,
            Handle<Device>* devices,
            void* pfnNotify,            // void (CL_CALLBACK*)(const char*, const void*, size_t, void*)
            void* userData,
            out ErrorCode errCodeRet);

        // cl_context clCreateContextFromType(const cl_context_properties * properties,
        //                                    cl_device_type device_type,
        //                                    void (CL_CALLBACK * pfn_notify)(...), void * user_data, cl_int * errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clCreateContextFromType", CallingConvention = CallingConvention.Cdecl)]
        public static extern Handle<Context> CreateContextFromType(
            ulong* properties,           // cl_context_properties[] (array of nint), null-terminated
            DeviceType deviceType,
            void* pfnNotify,            // void (CL_CALLBACK*)(const char*, const void*, size_t, void*)
            void* userData,
            out ErrorCode errCodeRet);


        // cl_int clSetDefaultDeviceCommandQueue(cl_context context,
        //                                       cl_device_id device,
        //                                       cl_command_queue command_queue)
        [DllImport(OpenClLibrary, EntryPoint = "clSetDefaultDeviceCommandQueue", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode SetDefaultDeviceCommandQueue(
            Handle<Context> context,
            Handle<Device> device,
            Handle<CommandQueue> commandQueue);

        // cl_int clRetainContext(cl_context context)
        [DllImport(OpenClLibrary, EntryPoint = "clRetainContext", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode RetainContext(Handle<Context> context);

        // cl_int clReleaseContext(cl_context context)
        [DllImport(OpenClLibrary, EntryPoint = "clReleaseContext", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode ReleaseContext(Handle<Context> context);

        // cl_int clGetContextInfo(cl_context context, cl_context_info param_name,
        //                         size_t param_value_size, void *param_value, size_t *param_value_size_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clGetContextInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetContextInfo(
            Handle<Context> context,
            ContextInfo paramName,
            nuint paramValueSize,
            void* paramValue,
            out nuint paramValueSizeRet);

        // cl_int clSetContextDestructorCallback(cl_context context,
        //                                       void (CL_CALLBACK* pfn_notify)(cl_context context, void* user_data),
        //                                       void* user_data)
        [DllImport(OpenClLibrary, EntryPoint = "clSetContextDestructorCallback", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode SetContextDestructorCallback(
            Handle<Context> context,
            void* pfnNotify,            // void (CL_CALLBACK*)(cl_context, void*)
            void* userData);

        #endregion

        #region Command Queue API

        // cl_command_queue clCreateCommandQueueWithProperties(cl_context context,
        //                                                     cl_device_id device,
        //                                                     const cl_queue_properties * properties,
        //                                                     cl_int * errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clCreateCommandQueueWithProperties", CallingConvention = CallingConvention.Cdecl)]
        public static extern Handle<CommandQueue> CreateCommandQueueWithProperties(
            Handle<Context> context,
            Handle<Device> device,
            ulong* properties,   // array of cl_queue_properties (nint), null-terminated
            out ErrorCode errCodeRet);

        // cl_int clRetainCommandQueue(cl_command_queue command_queue)
        [DllImport(OpenClLibrary, EntryPoint = "clRetainCommandQueue", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode RetainCommandQueue(Handle<CommandQueue> commandQueue);

        // cl_int clReleaseCommandQueue(cl_command_queue command_queue)
        [DllImport(OpenClLibrary, EntryPoint = "clReleaseCommandQueue", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode ReleaseCommandQueue(Handle<CommandQueue> commandQueue);

        // cl_int clGetCommandQueueInfo(cl_command_queue command_queue,
        //                              cl_command_queue_info param_name,
        //                              size_t param_value_size,
        //                              void * param_value,
        //                              size_t * param_value_size_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clGetCommandQueueInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetCommandQueueInfo(
            Handle<CommandQueue> commandQueue,
            CommandQueueInfo paramName,
            nuint paramValueSize,
            void* paramValue,
            out nuint paramValueSizeRet);


        // cl_int clFlush(cl_command_queue command_queue)
        [DllImport(OpenClLibrary, EntryPoint = "clFlush", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode Flush(Handle<CommandQueue> commandQueue);

        // cl_int clFinish(cl_command_queue command_queue)
        [DllImport(OpenClLibrary, EntryPoint = "clFinish", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode Finish(Handle<CommandQueue> commandQueue);


        #endregion

        #region User Event API

        // cl_int clWaitForEvents(cl_uint num_events, const cl_event * event_list)
        [DllImport(OpenClLibrary, EntryPoint = "clWaitForEvents", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode WaitForEvents(
            int numEvents,
            Handle<Event>* eventList);

        // cl_int clGetEventInfo(cl_event event, cl_event_info param_name,
        //                       size_t param_value_size, void *param_value, size_t *param_value_size_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clGetEventInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetEventInfo(
            Handle<Event> userEvent,
            EventInfo paramName,
            nuint paramValueSize,
            void* paramValue,
            out nuint paramValueSizeRet);

        // cl_event clCreateUserEvent(cl_context context, cl_int * errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clCreateUserEvent", CallingConvention = CallingConvention.Cdecl)]
        public static extern Handle<Event> CreateUserEvent(
            Handle<Context> context,
            out ErrorCode errCodeRet);

        // cl_int clRetainEvent(cl_event event)
        [DllImport(OpenClLibrary, EntryPoint = "clRetainEvent", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode RetainEvent(Handle<Event> userEvent);

        // cl_int clReleaseEvent(cl_event event)
        [DllImport(OpenClLibrary, EntryPoint = "clReleaseEvent", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode ReleaseEvent(Handle<Event> userEvent);

        // cl_int clSetUserEventStatus(cl_event event, cl_int execution_status)
        [DllImport(OpenClLibrary, EntryPoint = "clSetUserEventStatus", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode SetUserEventStatus(
            Handle<Event> userEvent,
            CommandExecutionStatus executionStatus);

        // cl_int clSetEventCallback(cl_event event, cl_int command_exec_callback_type,
        //                           void (CL_CALLBACK * pfn_notify)(cl_event, cl_int, void*), void * user_data)
        [DllImport(OpenClLibrary, EntryPoint = "clSetEventCallback", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode SetEventCallback(
            Handle<Event> userEvent,
            CommandExecutionStatus status,
            void* pfnNotify,      // void (CL_CALLBACK*)(cl_event, cl_int, void*)
            void* userData);

        #endregion

        #region Memory Objects API

        // cl_int clRetainMemObject(cl_mem memobj)
        [DllImport(OpenClLibrary, EntryPoint = "clRetainMemObject", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode RetainMemObject(Handle<MemoryObject> memObject);

        // cl_int clReleaseMemObject(cl_mem memobj)
        [DllImport(OpenClLibrary, EntryPoint = "clReleaseMemObject", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode ReleaseMemObject(Handle<MemoryObject> memObject);

        // cl_int clGetMemObjectInfo(cl_mem memobj, cl_mem_info param_name, size_t param_value_size, void * param_value, size_t * param_value_size_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clGetMemObjectInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetMemObjectInfo(
            Handle<MemoryObject> memObject,
            MemInfo paramName,
            nuint paramValueSize,
            void* paramValue,
            out nuint paramValueSizeRet);

        // cl_int clSetMemObjectDestructorCallback(cl_mem memobj, void (CL_CALLBACK * pfn_notify)(cl_mem memobj, void * user_data), void * user_data)
        [DllImport(OpenClLibrary, EntryPoint = "clSetMemObjectDestructorCallback", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode SetMemObjectDestructorCallback(
            Handle<MemoryObject> memObject,
            void* pfnNotify,
            void* userData);

        // cl_mem clCreateBuffer(cl_context context, cl_mem_flags flags, size_t size, void * host_ptr, cl_int * errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clCreateBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern Handle<MemoryObject> CreateBuffer(
            Handle<Context> context,
            MemFlags flags,
            nuint size,
            void* hostPtr,
            out ErrorCode errCodeRet);

        // cl_mem clCreateBufferWithProperties(cl_context context, const cl_mem_properties * properties, cl_mem_flags flags, size_t size, void * host_ptr, cl_int * errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clCreateBufferWithProperties", CallingConvention = CallingConvention.Cdecl)]
        public static extern Handle<MemoryObject> CreateBufferWithProperties(
            Handle<Context> context,
            ulong* properties,
            MemFlags flags,
            nuint size,
            void* hostPtr,
            out ErrorCode errCodeRet);

        // cl_mem clCreateSubBuffer(cl_mem buffer, cl_mem_flags flags, cl_buffer_create_type buffer_create_type, const void * buffer_create_info, cl_int * errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clCreateSubBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern Handle<MemoryObject> CreateSubBuffer(
            Handle<MemoryObject> buffer,
            MemFlags flags,
            ulong bufferCreateType,
            void* bufferCreateInfo,
            out ErrorCode errCodeRet);

        // cl_mem clCreateImage(cl_context context, cl_mem_flags flags, const cl_image_format * image_format, const cl_image_desc * image_desc, void * host_ptr, cl_int * errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clCreateImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern Handle<MemoryObject> CreateImage(
            Handle<Context> context,
            MemFlags flags,
            ImageFormat* imageFormat,
            ImageDesc* imageDesc,
            void* hostPtr,
            out ErrorCode errCodeRet);

        // cl_mem clCreateImageWithProperties(cl_context context, const cl_mem_properties * properties, cl_mem_flags flags, const cl_image_format * image_format, const cl_image_desc * image_desc, void * host_ptr, cl_int * errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clCreateImageWithProperties", CallingConvention = CallingConvention.Cdecl)]
        public static extern Handle<MemoryObject> CreateImageWithProperties(
            Handle<Context> context,
            ulong* properties,
            MemFlags flags,
            ImageFormat* imageFormat,
            ImageDesc* imageDesc,
            void* hostPtr,
            out ErrorCode errCodeRet);

        // cl_int clGetSupportedImageFormats(cl_context context, cl_mem_flags flags, cl_mem_object_type image_type, cl_uint num_entries, cl_image_format * image_formats, cl_uint * num_image_formats)
        [DllImport(OpenClLibrary, EntryPoint = "clGetSupportedImageFormats", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetSupportedImageFormats(
            Handle<Context> context,
            MemFlags flags,
            MemObjectType imageType,
            int numEntries,
            ImageFormat* imageFormats,
            out int numImageFormats);

        // cl_int clGetImageInfo(cl_mem image, cl_image_info param_name, size_t param_value_size, void * param_value, size_t * param_value_size_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clGetImageInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetImageInfo(
            Handle<MemoryObject> image,
            ImageInfo paramName,
            nuint paramValueSize,
            void* paramValue,
            out nuint paramValueSizeRet);

        // cl_mem clCreatePipe(cl_context context, cl_mem_flags flags, cl_uint pipe_packet_size, cl_uint pipe_max_packets, const cl_pipe_properties * properties, cl_int * errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clCreatePipe", CallingConvention = CallingConvention.Cdecl)]
        public static extern Handle<MemoryObject> CreatePipe(
            Handle<Context> context,
            MemFlags flags,
            int packetSize,
            int maxPackets,
            ulong* properties,
            out ErrorCode errCodeRet);

        // cl_int clGetPipeInfo(cl_mem pipe, cl_pipe_info param_name, size_t param_value_size, void * param_value, size_t * param_value_size_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clGetPipeInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetPipeInfo(
            Handle<MemoryObject> pipe,
            PipeInfo paramName,
            nuint paramValueSize,
            void* paramValue,
            out nuint paramValueSizeRet);

        #endregion

        #region Buffer Operations

        // cl_int clEnqueueReadBuffer(cl_command_queue command_queue,
        //                            cl_mem buffer, cl_bool blocking_read,
        //                            size_t offset, size_t size, void * ptr,
        //                            cl_uint num_events_in_wait_list,
        //                            const cl_event * event_wait_list,
        //                            cl_event * event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueReadBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueReadBuffer(
            Handle<CommandQueue> commandQueue,
            Handle<MemoryObject> buffer,
            bool blockingRead,
            nuint offset,
            nuint size,
            void* ptr,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
            Handle<Event>* userEvent);

        // cl_int clEnqueueReadBufferRect(cl_command_queue command_queue,
        //                                cl_mem buffer, cl_bool blocking_read,
        //                                const size_t * buffer_origin,
        //                                const size_t * host_origin,
        //                                const size_t * region,
        //                                size_t buffer_row_pitch,
        //                                size_t buffer_slice_pitch,
        //                                size_t host_row_pitch,
        //                                size_t host_slice_pitch,
        //                                void * ptr,
        //                                cl_uint num_events_in_wait_list,
        //                                const cl_event * event_wait_list,
        //                                cl_event * event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueReadBufferRect", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueReadBufferRect(
            Handle<CommandQueue> commandQueue,
            Handle<MemoryObject> buffer,
            bool blockingRead,
            nuint* bufferOrigin,
            nuint* hostOrigin,
            nuint* region,
            nuint bufferRowPitch,
            nuint bufferSlicePitch,
            nuint hostRowPitch,
            nuint hostSlicePitch,
            void* ptr,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
            Handle<Event>* userEvent);

        // cl_int clEnqueueWriteBuffer(cl_command_queue command_queue,
        //                             cl_mem buffer, cl_bool blocking_write,
        //                             size_t offset, size_t size, const void * ptr,
        //                             cl_uint num_events_in_wait_list,
        //                             const cl_event * event_wait_list,
        //                             cl_event * event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueWriteBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueWriteBuffer(
            Handle<CommandQueue> commandQueue,
            Handle<MemoryObject> buffer,
            bool blockingWrite,
            nuint offset,
            nuint size,
            void* ptr,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
            Handle<Event>* userEvent);

        // cl_int clEnqueueWriteBufferRect(cl_command_queue command_queue,
        //                                 cl_mem buffer, cl_bool blocking_write,
        //                                 const size_t * buffer_origin,
        //                                 const size_t * host_origin,
        //                                 const size_t * region,
        //                                 size_t buffer_row_pitch,
        //                                 size_t buffer_slice_pitch,
        //                                 size_t host_row_pitch,
        //                                 size_t host_slice_pitch,
        //                                 const void * ptr,
        //                                 cl_uint num_events_in_wait_list,
        //                                 const cl_event * event_wait_list,
        //                                 cl_event * event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueWriteBufferRect", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueWriteBufferRect(
            Handle<CommandQueue> commandQueue,
            Handle<MemoryObject> buffer,
            bool blockingWrite,
            nuint* bufferOrigin,
            nuint* hostOrigin,
            nuint* region,
            nuint bufferRowPitch,
            nuint bufferSlicePitch,
            nuint hostRowPitch,
            nuint hostSlicePitch,
            void* ptr,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
            Handle<Event>* userEvent);

        // cl_int clEnqueueFillBuffer(cl_command_queue command_queue,
        //                            cl_mem buffer, const void * pattern,
        //                            size_t pattern_size, size_t offset,
        //                            size_t size,
        //                            cl_uint num_events_in_wait_list,
        //                            const cl_event * event_wait_list,
        //                            cl_event * event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueFillBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueFillBuffer(
            Handle<CommandQueue> commandQueue,
            Handle<MemoryObject> buffer,
            void* pattern,
            nuint patternSize,
            nuint offset,
            nuint size,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
        out Handle<Event> userEvent);

        // cl_int clEnqueueCopyBuffer(cl_command_queue command_queue,
        //                            cl_mem src_buffer, cl_mem dst_buffer,
        //                            size_t src_offset, size_t dst_offset,
        //                            size_t size,
        //                            cl_uint num_events_in_wait_list,
        //                            const cl_event * event_wait_list,
        //                            cl_event * event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueCopyBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueCopyBuffer(
            Handle<CommandQueue> commandQueue,
            Handle<MemoryObject> srcBuffer,
            Handle<MemoryObject> dstBuffer,
            nuint srcOffset,
            nuint dstOffset,
            nuint size,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
        out Handle<Event> userEvent);

        // cl_int clEnqueueCopyBufferRect(cl_command_queue command_queue,
        //                                cl_mem src_buffer, cl_mem dst_buffer,
        //                                const size_t * src_origin,
        //                                const size_t * dst_origin,
        //                                const size_t * region,
        //                                size_t src_row_pitch,
        //                                size_t src_slice_pitch,
        //                                size_t dst_row_pitch,
        //                                size_t dst_slice_pitch,
        //                                cl_uint num_events_in_wait_list,
        //                                const cl_event * event_wait_list,
        //                                cl_event * event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueCopyBufferRect", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueCopyBufferRect(
            Handle<CommandQueue> commandQueue,
            Handle<MemoryObject> srcBuffer,
            Handle<MemoryObject> dstBuffer,
            nuint* srcOrigin,
            nuint* dstOrigin,
            nuint* region,
            nuint srcRowPitch,
            nuint srcSlicePitch,
            nuint dstRowPitch,
            nuint dstSlicePitch,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
        out Handle<Event> userEvent);

        #endregion

        #region Image Operations

        // cl_int clEnqueueReadImage(cl_command_queue command_queue,
        //                           cl_mem image, cl_bool blocking_read,
        //                           const size_t * origin,
        //                           const size_t * region,
        //                           size_t row_pitch,
        //                           size_t slice_pitch,
        //                           void * ptr,
        //                           cl_uint num_events_in_wait_list,
        //                           const cl_event * event_wait_list,
        //                           cl_event * event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueReadImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueReadImage(
            Handle<CommandQueue> commandQueue,
            Handle<MemoryObject> image,
            bool blockingRead,
            nuint* origin,
            nuint* region,
            nuint rowPitch,
            nuint slicePitch,
            void* ptr,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
            Handle<Event>* userEvent);

        // cl_int clEnqueueWriteImage(cl_command_queue command_queue,
        //                            cl_mem image, cl_bool blocking_write,
        //                            const size_t * origin,
        //                            const size_t * region,
        //                            size_t input_row_pitch,
        //                            size_t input_slice_pitch,
        //                            const void * ptr,
        //                            cl_uint num_events_in_wait_list,
        //                            const cl_event * event_wait_list,
        //                            cl_event * event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueWriteImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueWriteImage(
            Handle<CommandQueue> commandQueue,
            Handle<MemoryObject> image,
            bool blockingWrite,
            nuint* origin,
            nuint* region,
            nuint inputRowPitch,
            nuint inputSlicePitch,
            void* ptr,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
            Handle<Event>* userEvent);

        // cl_int clEnqueueFillImage(cl_command_queue command_queue,
        //                           cl_mem image, const void * fill_color,
        //                           const size_t * origin,
        //                           const size_t * region,
        //                           cl_uint num_events_in_wait_list,
        //                           const cl_event * event_wait_list,
        //                           cl_event * event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueFillImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueFillImage(
            Handle<CommandQueue> commandQueue,
            Handle<MemoryObject> image,
            void* fillColor,
            nuint* origin,
            nuint* region,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
        out Handle<Event> userEvent);

        // cl_int clEnqueueCopyImage(cl_command_queue command_queue,
        //                           cl_mem src_image, cl_mem dst_image,
        //                           const size_t * src_origin,
        //                           const size_t * dst_origin,
        //                           const size_t * region,
        //                           cl_uint num_events_in_wait_list,
        //                           const cl_event * event_wait_list,
        //                           cl_event * event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueCopyImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueCopyImage(
            Handle<CommandQueue> commandQueue,
            Handle<MemoryObject> srcImage,
            Handle<MemoryObject> dstImage,
            nuint* srcOrigin,
            nuint* dstOrigin,
            nuint* region,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
        out Handle<Event> userEvent);

        // cl_int clEnqueueCopyImageToBuffer(cl_command_queue command_queue,
        //                                   cl_mem src_image, cl_mem dst_buffer,
        //                                   const size_t * src_origin,
        //                                   const size_t * region,
        //                                   size_t dst_offset,
        //                                   cl_uint num_events_in_wait_list,
        //                                   const cl_event * event_wait_list,
        //                                   cl_event * event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueCopyImageToBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueCopyImageToBuffer(
            Handle<CommandQueue> commandQueue,
            Handle<MemoryObject> srcImage,
            Handle<MemoryObject> dstBuffer,
            nuint* srcOrigin,
            nuint* region,
            nuint dstOffset,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
        out Handle<Event> userEvent);

        // cl_int clEnqueueCopyBufferToImage(cl_command_queue command_queue,
        //                                   cl_mem src_buffer, cl_mem dst_image,
        //                                   size_t src_offset,
        //                                   const size_t * dst_origin,
        //                                   const size_t * region,
        //                                   cl_uint num_events_in_wait_list,
        //                                   const cl_event * event_wait_list,
        //                                   cl_event * event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueCopyBufferToImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueCopyBufferToImage(
            Handle<CommandQueue> commandQueue,
            Handle<MemoryObject> srcBuffer,
            Handle<MemoryObject> dstImage,
            nuint srcOffset,
            nuint* dstOrigin,
            nuint* region,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
            out Handle<Event> userEvent);

        #endregion

        #region Mapping

        // void * clEnqueueMapBuffer(cl_command_queue command_queue,
        //                           cl_mem buffer, cl_bool blocking_map,
        //                           cl_map_flags map_flags, size_t offset,
        //                           size_t size, cl_uint num_events_in_wait_list,
        //                           const cl_event * event_wait_list,
        //                           cl_event * event, cl_int * errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueMapBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void* EnqueueMapBuffer(
            Handle<CommandQueue> commandQueue,
            Handle<MemoryObject> buffer,
            bool blockingMap,
            MapFlags mapFlags,
            nuint offset,
            nuint size,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
            Handle<Event>* userEvent,
            out ErrorCode errCodeRet);

        // void * clEnqueueMapImage(cl_command_queue command_queue,
        //                          cl_mem image, cl_bool blocking_map,
        //                          cl_map_flags map_flags,
        //                          const size_t * origin,
        //                          const size_t * region,
        //                          size_t * image_row_pitch,
        //                          size_t * image_slice_pitch,
        //                          cl_uint num_events_in_wait_list,
        //                          const cl_event * event_wait_list,
        //                          cl_event * event, cl_int * errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueMapImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern void* EnqueueMapImage(
            Handle<CommandQueue> commandQueue,
            Handle<MemoryObject> image,
            bool blockingMap,
            MapFlags mapFlags,
            nuint* origin,
            nuint* region,
            out nuint imageRowPitch,
            out nuint imageSlicePitch,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
            Handle<Event>* userEvent,
            out ErrorCode errCodeRet);

        // cl_int clEnqueueUnmapMemObject(cl_command_queue command_queue,
        //                                cl_mem memobj, void * mapped_ptr,
        //                                cl_uint num_events_in_wait_list,
        //                                const cl_event * event_wait_list,
        //                                cl_event * event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueUnmapMemObject", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueUnmapMemObject(
            Handle<CommandQueue> commandQueue,
            Handle<MemoryObject> memObject,
            void* mappedPtr,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
        out Handle<Event> userEvent);

        #endregion

        #region Migration

        // cl_int clEnqueueMigrateMemObjects(cl_command_queue command_queue,
        //                                   cl_uint num_mem_objects,
        //                                   const cl_mem * mem_objects,
        //                                   cl_mem_migration_flags flags,
        //                                   cl_uint num_events_in_wait_list,
        //                                   const cl_event * event_wait_list,
        //                                   cl_event * event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueMigrateMemObjects", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueMigrateMemObjects(
            Handle<CommandQueue> commandQueue,
            int numMemObjects,
            Handle<MemoryObject>* memObjects,
            MemMigrationFlags flags,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
        out Handle<Event> userEvent);

        #endregion

        #region SVM APIs

        // void * clSVMAlloc(cl_context context, cl_svm_mem_flags flags, size_t size, cl_uint alignment)
        [DllImport(OpenClLibrary, EntryPoint = "clSVMAlloc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void* SvmAlloc(
            Handle<Context> context,
            SvmMemFlags flags,
            nuint size,
            int alignment);

        // void clSVMFree(cl_context context, void * svm_pointer)
        [DllImport(OpenClLibrary, EntryPoint = "clSVMFree", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SvmFree(
            Handle<Context> context,
            void* svmPointer);

        #endregion

        #region Sampler API

        // cl_sampler clCreateSamplerWithProperties(cl_context context,
        //                                          const cl_sampler_properties * sampler_properties,
        //                                          cl_int * errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clCreateSamplerWithProperties", CallingConvention = CallingConvention.Cdecl)]
        public static extern Handle<Sampler> CreateSamplerWithProperties(
            Handle<Context> context,
            ulong* samplerProperties,   // array of (SamplerProperty, ulong) pairs, null-terminated
            out ErrorCode errCodeRet);

        // cl_int clRetainSampler(cl_sampler sampler)
        [DllImport(OpenClLibrary, EntryPoint = "clRetainSampler", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode RetainSampler(Handle<Sampler> sampler);

        // cl_int clReleaseSampler(cl_sampler sampler)
        [DllImport(OpenClLibrary, EntryPoint = "clReleaseSampler", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode ReleaseSampler(Handle<Sampler> sampler);

        // cl_int clGetSamplerInfo(cl_sampler sampler, cl_sampler_info param_name,
        //                         size_t param_value_size, void *param_value, size_t *param_value_size_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clGetSamplerInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetSamplerInfo(
            Handle<Sampler> sampler,
            SamplerInfo paramName,
            nuint paramValueSize,
            void* paramValue,
            out nuint paramValueSizeRet);

        #endregion

        #region Program API

        // cl_program clCreateProgramWithSource(cl_context context, cl_uint count, const char ** strings, const size_t * lengths, cl_int * errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clCreateProgramWithSource", CallingConvention = CallingConvention.Cdecl)]
        public static extern Handle<Program> CreateProgramWithSource(
            Handle<Context> context,
            int count,
            byte** strings,
            nuint* lengths,
            out ErrorCode errCodeRet);

        // cl_program clCreateProgramWithBinary(cl_context context, cl_uint num_devices, const cl_device_id * device_list, const size_t * lengths, const unsigned char ** binaries, cl_int * binary_status, cl_int * errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clCreateProgramWithBinary", CallingConvention = CallingConvention.Cdecl)]
        public static extern Handle<Program> CreateProgramWithBinary(
            Handle<Context> context,
            int numDevices,
            Handle<Device>* deviceList,
            nuint* lengths,
            byte** binaries,
            ErrorCode* binaryStatus,
            out ErrorCode errCodeRet);

        // cl_program clCreateProgramWithBuiltInKernels(cl_context context, cl_uint num_devices, const cl_device_id * device_list, const char * kernel_names, cl_int * errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clCreateProgramWithBuiltInKernels", CallingConvention = CallingConvention.Cdecl)]
        public static extern Handle<Program> CreateProgramWithBuiltInKernels(
            Handle<Context> context,
            int numDevices,
            Handle<Device>* deviceList,
            byte* kernelNames,
            out ErrorCode errCodeRet);

        // cl_program clCreateProgramWithIL(cl_context context, const void* il, size_t length, cl_int* errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clCreateProgramWithIL", CallingConvention = CallingConvention.Cdecl)]
        public static extern Handle<Program> CreateProgramWithIL(
            Handle<Context> context,
            void* il,
            nuint length,
            out ErrorCode errCodeRet);

        // cl_int clRetainProgram(cl_program program)
        [DllImport(OpenClLibrary, EntryPoint = "clRetainProgram", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode RetainProgram(Handle<Program> program);

        // cl_int clReleaseProgram(cl_program program)
        [DllImport(OpenClLibrary, EntryPoint = "clReleaseProgram", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode ReleaseProgram(Handle<Program> program);

        // cl_int clBuildProgram(cl_program program, cl_uint num_devices, const cl_device_id * device_list, const char * options, void (CL_CALLBACK * pfn_notify)(cl_program program, void * user_data), void * user_data)
        [DllImport(OpenClLibrary, EntryPoint = "clBuildProgram", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode BuildProgram(
            Handle<Program> program,
            int numDevices,
            Handle<Device>* deviceList,
            byte* options,
            void* pfnNotify,
            void* userData);

        // cl_int clCompileProgram(cl_program program, cl_uint num_devices, const cl_device_id * device_list, const char * options, cl_uint num_input_headers, const cl_program * input_headers, const char ** header_include_names, void (CL_CALLBACK * pfn_notify)(cl_program program, void * user_data), void * user_data)
        [DllImport(OpenClLibrary, EntryPoint = "clCompileProgram", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode CompileProgram(
            Handle<Program> program,
            int numDevices,
            Handle<Device>* deviceList,
            byte* options,
            int numInputHeaders,
            Handle<Program>* inputHeaders,
            byte** headerIncludeNames,
            void* pfnNotify,
            void* userData);

        // cl_program clLinkProgram(cl_context context, cl_uint num_devices, const cl_device_id * device_list, const char * options, cl_uint num_input_programs, const cl_program * input_programs, void (CL_CALLBACK * pfn_notify)(cl_program program, void * user_data), void * user_data, cl_int * errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clLinkProgram", CallingConvention = CallingConvention.Cdecl)]
        public static extern Handle<Program> LinkProgram(
            Handle<Context> context,
            int numDevices,
            Handle<Device>* deviceList,
            byte* options,
            int numInputPrograms,
            Handle<Program>* inputPrograms,
            void* pfnNotify,
            void* userData,
            out ErrorCode errCodeRet);

        // cl_int clGetProgramInfo(cl_program program, cl_program_info param_name, size_t param_value_size, void * param_value, size_t * param_value_size_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clGetProgramInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetProgramInfo(
            Handle<Program> program,
            ProgramInfo paramName,
            nuint paramValueSize,
            void* paramValue,
            out nuint paramValueSizeRet);

        // cl_int clGetProgramBuildInfo(cl_program program, cl_device_id device, cl_program_build_info param_name, size_t param_value_size, void * param_value, size_t * param_value_size_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clGetProgramBuildInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetProgramBuildInfo(
            Handle<Program> program,
            Handle<Device> device,
            ProgramBuildInfo paramName,
            nuint paramValueSize,
            void* paramValue,
            out nuint paramValueSizeRet);

        #endregion

        #region Kernel API

        // cl_kernel clCreateKernel(cl_program program, const char * kernel_name, cl_int * errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clCreateKernel", CallingConvention = CallingConvention.Cdecl)]
        public static extern Handle<Kernel> CreateKernel(
            Handle<Program> program,
            byte* kernelName,
            out ErrorCode errCodeRet);

        // cl_int clCreateKernelsInProgram(cl_program program, cl_uint num_kernels, cl_kernel * kernels, cl_uint * num_kernels_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clCreateKernelsInProgram", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode CreateKernelsInProgram(
            Handle<Program> program,
            int numKernels,
            Handle<Kernel>* kernels,
            out int numKernelsRet);

        // cl_kernel clCloneKernel(cl_kernel source_kernel, cl_int* errcode_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clCloneKernel", CallingConvention = CallingConvention.Cdecl)]
        public static extern Handle<Kernel> CloneKernel(
            Handle<Kernel> sourceKernel,
            out ErrorCode errCodeRet);

        // cl_int clRetainKernel(cl_kernel kernel)
        [DllImport(OpenClLibrary, EntryPoint = "clRetainKernel", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode RetainKernel(Handle<Kernel> kernel);

        // cl_int clReleaseKernel(cl_kernel kernel)
        [DllImport(OpenClLibrary, EntryPoint = "clReleaseKernel", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode ReleaseKernel(Handle<Kernel> kernel);

        // cl_int clSetKernelArg(cl_kernel kernel, cl_uint arg_index, size_t arg_size, const void * arg_value)
        [DllImport(OpenClLibrary, EntryPoint = "clSetKernelArg", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode SetKernelArg(
            Handle<Kernel> kernel,
            int argIndex,
            nuint argSize,
            void* argValue);

        // cl_int clSetKernelArgSVMPointer(cl_kernel kernel, cl_uint arg_index, const void * arg_value)
        [DllImport(OpenClLibrary, EntryPoint = "clSetKernelArgSVMPointer", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode SetKernelArgSvmPointer(
            Handle<Kernel> kernel,
            int argIndex,
            void* argValue);

        // cl_int clSetKernelExecInfo(cl_kernel kernel, cl_kernel_exec_info param_name, size_t param_value_size, const void * param_value)
        [DllImport(OpenClLibrary, EntryPoint = "clSetKernelExecInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode SetKernelExecInfo(
            Handle<Kernel> kernel,
            KernelExecInfo paramName,
            nuint paramValueSize,
            void* paramValue);

        // cl_int clGetKernelInfo(cl_kernel kernel, cl_kernel_info param_name, size_t param_value_size, void * param_value, size_t * param_value_size_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clGetKernelInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetKernelInfo(
            Handle<Kernel> kernel,
            KernelInfo paramName,
            nuint paramValueSize,
            void* paramValue,
            out nuint paramValueSizeRet);

        // cl_int clGetKernelArgInfo(cl_kernel kernel, cl_uint arg_indx, cl_kernel_arg_info param_name, size_t param_value_size, void * param_value, size_t * param_value_size_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clGetKernelArgInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetKernelArgInfo(
            Handle<Kernel> kernel,
            int argIndex,
            KernelArgInfo paramName,
            nuint paramValueSize,
            void* paramValue,
            out nuint paramValueSizeRet);

        // cl_int clGetKernelWorkGroupInfo(cl_kernel kernel, cl_device_id device, cl_kernel_work_group_info param_name, size_t param_value_size, void * param_value, size_t * param_value_size_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clGetKernelWorkGroupInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetKernelWorkGroupInfo(
            Handle<Kernel> kernel,
            Handle<Device> device,
            KernelWorkGroupInfo paramName,
            nuint paramValueSize,
            void* paramValue,
            out nuint paramValueSizeRet);

        // cl_int clGetKernelSubGroupInfo(cl_kernel kernel, cl_device_id device, cl_kernel_sub_group_info param_name, size_t input_value_size, const void* input_value, size_t param_value_size, void* param_value, size_t* param_value_size_ret)
        [DllImport(OpenClLibrary, EntryPoint = "clGetKernelSubGroupInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode GetKernelSubGroupInfo(
            Handle<Kernel> kernel,
            Handle<Device> device,
            KernelSubGroupInfo paramName,
            nuint inputValueSize,
            void* inputValue,
            nuint paramValueSize,
            void* paramValue,
            out nuint paramValueSizeRet);

        #endregion

        #region Kernel Execution API

        // cl_int clEnqueueNDRangeKernel(cl_command_queue command_queue, cl_kernel kernel, cl_uint work_dim, const size_t * global_work_offset, const size_t * global_work_size, const size_t * local_work_size, cl_uint num_events_in_wait_list, const cl_event * event_wait_list, cl_event * event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueNDRangeKernel", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueNdRangeKernel(
            Handle<CommandQueue> commandQueue,
            Handle<Kernel> kernel,
            int workDim,
            nuint* globalWorkOffset,
            nuint* globalWorkSize,
            nuint* localWorkSize,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
            out Handle<Event> userEvent);

        // cl_int clEnqueueNativeKernel(cl_command_queue command_queue, void (CL_CALLBACK *user_func)(void *), void *args, size_t cb_args, cl_uint num_mem_objects, const cl_mem *mem_list, const void **args_mem_loc, cl_uint num_events_in_wait_list, const cl_event *event_wait_list, cl_event *event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueNativeKernel", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueNativeKernel(
            Handle<CommandQueue> commandQueue,
            void* userFunc,
            void* args,
            nuint cbArgs,
            int numMemObjects,
            Handle<MemoryObject>* memList,
            void** argsMemLoc,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
            out Handle<Event> userEvent);

        #endregion

        #region Marker & Barrier API (CL 1.2+)

        // cl_int clEnqueueMarkerWithWaitList(cl_command_queue command_queue, cl_uint num_events_in_wait_list, const cl_event *event_wait_list, cl_event *event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueMarkerWithWaitList", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueMarkerWithWaitList(
            Handle<CommandQueue> commandQueue,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
            out Handle<Event> userEvent);

        // cl_int clEnqueueBarrierWithWaitList(cl_command_queue command_queue, cl_uint num_events_in_wait_list, const cl_event *event_wait_list, cl_event *event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueBarrierWithWaitList", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueBarrierWithWaitList(
            Handle<CommandQueue> commandQueue,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
            out Handle<Event> userEvent);

        #endregion

        #region SVM Operations (CL 2.0+)

        // cl_int clEnqueueSVMFree(cl_command_queue command_queue, cl_uint num_svm_pointers, void *svm_pointers[], void (CL_CALLBACK *pfn_free_func)(cl_command_queue queue, cl_uint num_svm_pointers, void *svm_pointers[], void *user_data), void *user_data, cl_uint num_events_in_wait_list, const cl_event *event_wait_list, cl_event *event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueSVMFree", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueSvmFree(
            Handle<CommandQueue> commandQueue,
            int numSvmPointers,
            void** svmPointers,
            void* pfnFreeFunc,
            void* userData,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
            out Handle<Event> userEvent);

        // cl_int clEnqueueSVMMemcpy(cl_command_queue command_queue, cl_bool blocking_copy, void *dst_ptr, const void *src_ptr, size_t size, cl_uint num_events_in_wait_list, const cl_event *event_wait_list, cl_event *event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueSVMMemcpy", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueSvmMemcpy(
            Handle<CommandQueue> commandQueue,
            bool blockingCopy,
            void* dstPtr,
            void* srcPtr,
            nuint size,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
            out Handle<Event> userEvent);

        // cl_int clEnqueueSVMMemFill(cl_command_queue command_queue, void *svm_ptr, const void *pattern, size_t pattern_size, size_t size, cl_uint num_events_in_wait_list, const cl_event *event_wait_list, cl_event *event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueSVMMemFill", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueSvmMemFill(
            Handle<CommandQueue> commandQueue,
            void* svmPtr,
            void* pattern,
            nuint patternSize,
            nuint size,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
            out Handle<Event> userEvent);

        // cl_int clEnqueueSVMMap(cl_command_queue command_queue, cl_bool blocking_map, cl_map_flags flags, void *svm_ptr, size_t size, cl_uint num_events_in_wait_list, const cl_event *event_wait_list, cl_event *event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueSVMMap", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueSvmMap(
            Handle<CommandQueue> commandQueue,
            bool blockingMap,
            MapFlags flags,
            void* svmPtr,
            nuint size,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
            Handle<Event>* userEvent);

        // cl_int clEnqueueSVMUnmap(cl_command_queue command_queue, void *svm_ptr, cl_uint num_events_in_wait_list, const cl_event *event_wait_list, cl_event *event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueSVMUnmap", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueSvmUnmap(
            Handle<CommandQueue> commandQueue,
            void* svmPtr,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
            out Handle<Event> userEvent);

        #endregion

        #region SVM Migration (CL 2.1+)

        // cl_int clEnqueueSVMMigrateMem(cl_command_queue command_queue, cl_uint num_svm_pointers, const void **svm_pointers, const size_t *sizes, cl_mem_migration_flags flags, cl_uint num_events_in_wait_list, const cl_event *event_wait_list, cl_event *event)
        [DllImport(OpenClLibrary, EntryPoint = "clEnqueueSVMMigrateMem", CallingConvention = CallingConvention.Cdecl)]
        public static extern ErrorCode EnqueueSvmMigrateMem(
            Handle<CommandQueue> commandQueue,
            int numSvmPointers,
            void** svmPointers,
            nuint* sizes,
            MemMigrationFlags flags,
            int numEventsInWaitList,
            Handle<Event>* eventWaitList,
            out Handle<Event> userEvent);

        #endregion

        #region Extension Function Access

        // void * clGetExtensionFunctionAddressForPlatform(cl_platform_id platform, const char *func_name)
        [DllImport(OpenClLibrary, EntryPoint = "clGetExtensionFunctionAddressForPlatform", CallingConvention = CallingConvention.Cdecl)]
        public static extern void* GetExtensionFunctionAddressForPlatform(
            Handle<Platform> platform,
            byte* funcName);

        #endregion
    }
}
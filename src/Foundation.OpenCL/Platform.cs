using System;

namespace Foundation.OpenCL
{
    #region Constants

    public enum PlatformInfo
    {
        Profile = 0x900,                                      // CL_PLATFORM_PROFILE
        Version = 0x901,                                      // CL_PLATFORM_VERSION
        Name = 0x902,                                         // CL_PLATFORM_NAME
        Vendor = 0x903,                                       // CL_PLATFORM_VENDOR
        Extensions = 0x904,                                   // CL_PLATFORM_EXTENSIONS
        HostTimerResolution = 0x905,                          // CL_PLATFORM_HOST_TIMER_RESOLUTION
        NumericVersion = 0x906,                               // CL_PLATFORM_NUMERIC_VERSION
        ExtensionsWithVersion = 0x907,                        // CL_PLATFORM_EXTENSIONS_WITH_VERSION
        IcdSuffixKhr = 0x920,                                 // CL_PLATFORM_ICD_SUFFIX_KHR
        NumericVersionKhr = 0x906,                            // CL_PLATFORM_NUMERIC_VERSION_KHR
        ExtensionsWithVersionKhr = 0x907,                     // CL_PLATFORM_EXTENSIONS_WITH_VERSION_KHR
        SemaphoreTypesKhr = 0x2036,                           // CL_PLATFORM_SEMAPHORE_TYPES_KHR
        SemaphoreImportHandleTypesKhr = 0x2037,               // CL_PLATFORM_SEMAPHORE_IMPORT_HANDLE_TYPES_KHR
        SemaphoreExportHandleTypesKhr = 0x2038,               // CL_PLATFORM_SEMAPHORE_EXPORT_HANDLE_TYPES_KHR
        ExternalMemoryImportHandleTypesKhr = 0x2044,          // CL_PLATFORM_EXTERNAL_MEMORY_IMPORT_HANDLE_TYPES_KHR
        CommandBufferCapabilitiesKhr = 0x908,                 // CL_PLATFORM_COMMAND_BUFFER_CAPABILITIES_KHR
    }

    #endregion

    public sealed unsafe class Platform(Handle<Platform> handle)
        : BaseObject<Platform, PlatformInfo>(handle), IReify<Platform>
    {
        public static Platform[] GetPlatforms()
        {
            OpenCLNative.GetPlatformIds(0, null, out var count).ThrowIfUnsuccessful();
            var handles = stackalloc Handle<Platform>[count];
            OpenCLNative.GetPlatformIds(count, handles, out count).ThrowIfUnsuccessful();

            var result = new Platform[count];
            for (var i = 0; i < result.Length; i++) result[i] = Platform.Reify(handles[i]);
            return result;
        }

        public Device[] GetDevices(DeviceType deviceType = DeviceType.All)
        {
            OpenCLNative.GetDeviceIds(Handle, deviceType, 0, null, out var count).ThrowIfUnsuccessful();
            var handles = stackalloc Handle<Device>[count];
            OpenCLNative.GetDeviceIds(Handle, deviceType, count, handles, out count).ThrowIfUnsuccessful();

            var result = new Device[count];
            for (var i = 0; i < result.Length; i++) result[i] = Device.Reify(handles[i]);
            return result;
        }

        public void UnloadCompiler() => OpenCLNative.UnloadPlatformCompiler(Handle).ThrowIfUnsuccessful();

        public Context CreateContext(ReadOnlySpan<Device> devices, Action<string>? errorCallback = null)
            => Context.CreateContext(devices, errorCallback, new ContextConfig(ContextProperty.Platform, (ulong)Handle.Value));

        protected override void GetInfo(PlatformInfo paramName, nuint paramValueSize, void* paramValue, out nuint paramValueSizeRet)
            => OpenCLNative.GetPlatformInfo(Handle, paramName, paramValueSize, paramValue, out paramValueSizeRet).ThrowIfUnsuccessful();

        protected override void RetainHook() { }
        protected override void ReleaseHook() { }
        public static Platform Reify(Handle<Platform> handle) => new(handle);
    }
}
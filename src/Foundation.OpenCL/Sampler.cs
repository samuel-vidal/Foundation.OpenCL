using System;

namespace Foundation.OpenCL
{
    #region Constants

    public enum SamplerInfo
    {
        ReferenceCount = 0x1150,            // CL_SAMPLER_REFERENCE_COUNT
        Context = 0x1151,                   // CL_SAMPLER_CONTEXT
        NormalizedCoords = 0x1152,          // CL_SAMPLER_NORMALIZED_COORDS
        AddressingMode = 0x1153,            // CL_SAMPLER_ADDRESSING_MODE
        FilterMode = 0x1154,                // CL_SAMPLER_FILTER_MODE
        MipFilterMode = 0x1155,             // CL_SAMPLER_MIP_FILTER_MODE
        LodMin = 0x1156,                    // CL_SAMPLER_LOD_MIN
        LodMax = 0x1157,                    // CL_SAMPLER_LOD_MAX
        Properties = 0x1158,                // CL_SAMPLER_PROPERTIES
    }

    public enum SamplerProperty : ulong
    {
        MipFilterModeKhr = 0x1155,          // CL_SAMPLER_MIP_FILTER_MODE_KHR
        LodMinKhr = 0x1156,                 // CL_SAMPLER_LOD_MIN_KHR
        LodMaxKhr = 0x1157,                 // CL_SAMPLER_LOD_MAX_KHR
    }

    public enum AddressingMode
    {
        None = 0x1130,                    // CL_ADDRESS_NONE
        ClampToEdge = 0x1131,             // CL_ADDRESS_CLAMP_TO_EDGE
        Clamp = 0x1132,                   // CL_ADDRESS_CLAMP
        Repeat = 0x1133,                  // CL_ADDRESS_REPEAT
        MirroredRepeat = 0x1134,          // CL_ADDRESS_MIRRORED_REPEAT
    }

    public enum FilterMode
    {
        Nearest = 0x1140,          // CL_FILTER_NEAREST
        Linear = 0x1141,           // CL_FILTER_LINEAR
    }

    #endregion

    public readonly struct SamplerConfig(SamplerInfo property, ulong value)
    {
        public SamplerInfo Property { get; } = property;
        public ulong Value { get; } = value;
    }

    public sealed unsafe class Sampler(Handle<Sampler> handle)
        : InformationNode<Sampler, SamplerInfo>(handle), IReify<Sampler>
    {
        protected override void GetInfo(SamplerInfo paramName, nuint paramValueSize, void* paramValue, out nuint paramValueSizeRet)
            => OpenCLNative.GetSamplerInfo(Handle, paramName, paramValueSize, paramValue, out paramValueSizeRet).ThrowIfUnsuccessful();

        protected override void RetainHook() => OpenCLNative.RetainSampler(Handle).ThrowIfUnsuccessful();
        protected override void ReleaseHook(Handle<Sampler> tmpHandle) => OpenCLNative.ReleaseSampler(tmpHandle).ThrowIfUnsuccessful();

        public static Sampler Reify(Handle<Sampler> handle) => new(handle);

        public static Sampler Create(
            Context context,
            bool normalizedCoords = true,
            AddressingMode addressingMode = AddressingMode.ClampToEdge,
            FilterMode filterMode = FilterMode.Linear)
        {
            return Create(context,
                new SamplerConfig(SamplerInfo.NormalizedCoords, normalizedCoords ? 1UL : 0UL),
                new SamplerConfig(SamplerInfo.AddressingMode, (ulong)addressingMode),
                new SamplerConfig(SamplerInfo.FilterMode, (ulong)filterMode));
        }

        public static Sampler Create(
            Context context,
            params ReadOnlySpan<SamplerConfig> properties)
        {
            var propArray = stackalloc ulong[properties.Length * 2 + 1];
            for (int i = 0, pos = 0; i < properties.Length; i++)
            {
                propArray[pos++] = (ulong)properties[i].Property;
                propArray[pos++] = properties[i].Value;
                propArray[pos] = 0;       // intentional
            }

            var handle = OpenCLNative.CreateSamplerWithProperties(
                context.Handle,
                propArray,
                out var errCodeRet);

            errCodeRet.ThrowIfUnsuccessful();
            return Reify(handle);
        }

        public static Sampler CreateNearestClamp(Context context)
            => Create(context, true, AddressingMode.Clamp, FilterMode.Nearest);

        public static Sampler CreateLinearRepeat(Context context)
            => Create(context, true, AddressingMode.Repeat, FilterMode.Linear);

        public static Sampler CreateNearestMirror(Context context)
            => Create(context, true, AddressingMode.MirroredRepeat, FilterMode.Nearest);

        public bool GetNormalizedCoords()
            => GetInfo<ulong>(SamplerInfo.NormalizedCoords) != 0;

        public AddressingMode GetAddressingMode()
            => (AddressingMode)GetInfo<ulong>(SamplerInfo.AddressingMode);

        public FilterMode GetFilterMode()
            => (FilterMode)GetInfo<ulong>(SamplerInfo.FilterMode);
    }
}

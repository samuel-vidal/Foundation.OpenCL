using System;
using System.Text;

namespace Foundation.OpenCL
{
    #region Constants

    public enum ChannelType
    {
        SNormInt8 = 0x10D0,                    // CL_SNORM_INT8
        SNormInt16 = 0x10D1,                   // CL_SNORM_INT16
        UnormInt8 = 0x10D2,                    // CL_UNORM_INT8
        UnormInt16 = 0x10D3,                   // CL_UNORM_INT16
        UnormShort565 = 0x10D4,                // CL_UNORM_SHORT_565
        UnormShort555 = 0x10D5,                // CL_UNORM_SHORT_555
        UnormInt101010 = 0x10D6,               // CL_UNORM_INT_101010
        SignedInt8 = 0x10D7,                   // CL_SIGNED_INT8
        SignedInt16 = 0x10D8,                  // CL_SIGNED_INT16
        SignedInt32 = 0x10D9,                  // CL_SIGNED_INT32
        UnsignedInt8 = 0x10DA,                 // CL_UNSIGNED_INT8
        UnsignedInt16 = 0x10DB,                // CL_UNSIGNED_INT16
        UnsignedInt32 = 0x10DC,                // CL_UNSIGNED_INT32
        HalfFloat = 0x10DD,                    // CL_HALF_FLOAT
        Float = 0x10DE,                        // CL_FLOAT
        UnormInt1010102 = 0x10E0,              // CL_UNORM_INT_101010_2
        UnormInt24 = 0x10DF,                   // CL_UNORM_INT24
        UnsignedIntRaw10Ext = 0x10E3,          // CL_UNSIGNED_INT_RAW10_EXT
        UnsignedIntRaw12Ext = 0x10E4,          // CL_UNSIGNED_INT_RAW12_EXT
        UnormInt2101010Ext = 0x10E5,           // CL_UNORM_INT_2_101010_EXT
    }

    public enum ChannelOrder
    {
        R = 0x10B0, // CL_R
        A = 0x10B1, // CL_A
        RG = 0x10B2, // CL_RG
        RA = 0x10B3, // CL_RA
        Rgb = 0x10B4, // CL_RGB
        Rgba = 0x10B5, // CL_RGBA
        Bgra = 0x10B6, // CL_BGRA
        Argb = 0x10B7, // CL_ARGB
        Intensity = 0x10B8, // CL_INTENSITY
        Luminance = 0x10B9, // CL_LUMINANCE
        Rx = 0x10BA, // CL_Rx
        RGx = 0x10BB, // CL_RGx
        Rgbx = 0x10BC, // CL_RGBx
        Depth = 0x10BD, // CL_DEPTH
        SRgb = 0x10BF, // CL_sRGB
        SRgbx = 0x10C0, // CL_sRGBx
        SRgba = 0x10C1, // CL_sRGBA
        SBgra = 0x10C2, // CL_sBGRA
        Abgr = 0x10C3, // CL_ABGR
        NV21Img = 0x40D0, // CL_NV21_IMG
        YV12Img = 0x40D1, // CL_YV12_IMG
        YuyvIntel = 0x4076, // CL_YUYV_INTEL
        UyvyIntel = 0x4077, // CL_UYVY_INTEL
        YvyuIntel = 0x4078, // CL_YVYU_INTEL
        VyuyIntel = 0x4079, // CL_VYUY_INTEL
        NV12Intel = 0x410E, // CL_NV12_INTEL
        DepthStencil = 0x10BE, // CL_DEPTH_STENCIL
    }

    public enum ImageInfo
    {
        Format = 0x1110,                       // CL_IMAGE_FORMAT
        ElementSize = 0x1111,                  // CL_IMAGE_ELEMENT_SIZE
        RowPitch = 0x1112,                     // CL_IMAGE_ROW_PITCH
        SlicePitch = 0x1113,                   // CL_IMAGE_SLICE_PITCH
        Width = 0x1114,                        // CL_IMAGE_WIDTH
        Height = 0x1115,                       // CL_IMAGE_HEIGHT
        Depth = 0x1116,                        // CL_IMAGE_DEPTH
        ArraySize = 0x1117,                    // CL_IMAGE_ARRAY_SIZE
        NumMipLevels = 0x1119,                 // CL_IMAGE_NUM_MIP_LEVELS
        NumSamples = 0x111A,                   // CL_IMAGE_NUM_SAMPLES
        Buffer = 0x1118,                       // CL_IMAGE_BUFFER
        D3D10SubresourceKhr = 0x4016,          // CL_IMAGE_D3D10_SUBRESOURCE_KHR
        D3D11SubresourceKhr = 0x401F,          // CL_IMAGE_D3D11_SUBRESOURCE_KHR
        DX9MediaPlaneKhr = 0x202A,             // CL_IMAGE_DX9_MEDIA_PLANE_KHR
        VAApiPlaneIntel = 0x4099,              // CL_IMAGE_VA_API_PLANE_INTEL
        DX9PlaneIntel = 0x4075,                // CL_IMAGE_DX9_PLANE_INTEL
    }

    #endregion

    public struct ImageFormat
    {
        public ChannelOrder ChannelOrder;
        public ChannelType ChannelType;
    }

    public struct ImageDesc
    {
        public MemObjectType ImageType;
        public ulong ImageWidth;
        public ulong ImageHeight;
        public ulong ImageDepth;
        public ulong ImageArraySize;
        public ulong ImageRowPitch;
        public ulong ImageSlicePitch;
        public uint NumMipLevels;
        public uint NumSamples;
        public Handle<MemoryObject> MemObject;
    }

    public sealed unsafe class Image(Handle<MemoryObject> handle)
        : BaseMemoryObject<Image>(handle), IReify<Image, MemoryObject>
    {
        private void GetInfo(ImageInfo paramName, nuint paramValueSize, void* paramValue, out nuint paramValueSizeRet)
            => OpenCLNative.GetImageInfo(Handle, paramName, paramValueSize, paramValue, out paramValueSizeRet).ThrowIfUnsuccessful();

        public bool TryGetInfo<T>(ImageInfo paramName, out T value)
            where T : unmanaged
        {
            var val = value = default;
            GetInfo(paramName, (nuint)sizeof(T), &val, out var size);
            if ((int)size != sizeof(T)) return false;
            value = val;
            return true;
        }

        public T GetInfo<T>(ImageInfo paramName)
            where T : unmanaged
        {
            if (TryGetInfo(paramName, out T value)) return value;
            throw new InvalidOperationException();
        }

        public string GetStringInfo(ImageInfo paramName)
        {
            var length = GetInfoByteSize(paramName);
            var buffer = stackalloc byte[length];

            GetInfo(paramName, (nuint)length, buffer, out _);
            return Encoding.ASCII.GetString(new ReadOnlySpan<byte>(buffer, length - 1));        // C-strings have an extra zero in the end.
        }

        public int GetInfoByteSize(ImageInfo paramName)
        {
            GetInfo(paramName, 0, null, out var size);
            return (int)size;
        }

        public bool TryGetInfo<T>(ImageInfo paramName, Span<T> values)
            where T : unmanaged
        {
            fixed (T* ptr = values)
            {
                var bufferSize = (nuint)(sizeof(T) * values.Length);
                GetInfo(paramName, bufferSize, ptr, out var size);
                return size == bufferSize;
            }
        }

        public static Image Reify(Handle<MemoryObject> handle) => new(handle);
    }
}

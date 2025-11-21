using System;

namespace Foundation.OpenCL.Tests.Tensors
{
    public static partial class Check
    {
        [System.Diagnostics.Conditional("DEBUG")]
        public static void AreEqual(int x, int y)
        {
            if (x != y) throw new Exception();
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void That(bool c)
        {
            if (!c) throw new Exception();
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void That(Func<bool> condition)
        {
            if (!condition()) throw new Exception();
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static unsafe void Aligned<T>(T* ptr, int byteAlign)
            where T : unmanaged
        {
            Check.That((ulong)ptr % (ulong)byteAlign == 0);
        }
    }
}
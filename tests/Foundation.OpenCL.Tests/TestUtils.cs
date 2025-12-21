using Foundation.OpenCL.Tests.Tensors;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Reflection;

namespace Foundation.OpenCL.Tests
{
    public static class TestUtils
    {
        public record Params(
            nuint[] GlobalSize,
            nuint[] LocalSize,
            Action<Kernel>? SetArguments = null
        );

        public static string GetResource(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(path)!;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static void FillRandom<TCoeff>(Random rand, Tensor<TCoeff> x)
            where TCoeff : unmanaged, INumber<TCoeff>
        {
            var span = x.AsSpan();

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = TCoeff.CreateTruncating(rand.NextSingle() - 0.5f);
            }
        }

        public static void ReferenceImplementation<TCoeff>(
            Tensor<TCoeff> c,
            Tensor<TCoeff> a,
            Tensor<TCoeff> b)
            where TCoeff : unmanaged, INumber<TCoeff>
        {
            ReferenceImplementation<TCoeff, TCoeff>(c, a, b);
        }

        public static void ReferenceImplementation<TCoeff, TAcc>(
            Tensor<TCoeff> c,
            Tensor<TCoeff> a,
            Tensor<TCoeff> b)
            where TCoeff : unmanaged, INumber<TCoeff>
            where TAcc : unmanaged, INumber<TAcc>
        {
            #region Checks

            Check.That(a.Rank == 2);
            Check.That(b.Rank == 2);
            Check.That(c.Rank == 2);

            Check.That(c.Dimensions[0] == a.Dimensions[0]);
            Check.That(c.Dimensions[1] == b.Dimensions[1]);
            Check.That(a.Dimensions[1] == b.Dimensions[0]);

            #endregion

            var n1 = c.Dimensions[0];
            var n2 = c.Dimensions[1];
            var n3 = a.Dimensions[1];

            for (var i = 0; i < n1; i++)
            {
                for (var j = 0; j < n2; j++)
                {
                    var sum = TAcc.Zero;
                    for (var k = 0; k < n3; k++)
                    {
                        sum += TAcc.CreateTruncating(a[i, k]) * TAcc.CreateTruncating(b[k, j]);
                    }
                    c[i, j] = TCoeff.CreateTruncating(sum + TAcc.CreateTruncating(c[i, j]));
                }
            }
        }

        public static void ReferenceImplementationLayerNorm<TCoeff, TAcc>(
            Tensor<TCoeff> c,
            Tensor<TCoeff> a,
            Tensor<TCoeff> b)
            where TCoeff : unmanaged, INumber<TCoeff>
            where TAcc : unmanaged, IRootFunctions<TAcc>
        {
            #region Checks

            Check.That(a.Rank == 2);
            Check.That(b.Rank == 2);
            Check.That(c.Rank == 2);

            Check.That(c.Dimensions[0] == a.Dimensions[0]);
            Check.That(c.Dimensions[1] == b.Dimensions[1]);
            Check.That(a.Dimensions[1] == b.Dimensions[0]);

            #endregion

            var n1 = c.Dimensions[0];
            var n2 = c.Dimensions[1];
            var n3 = a.Dimensions[1];

            Span<TAcc> scale = new TAcc[n1];
            var epsilon = TAcc.CreateTruncating(1e-6);

            for (int i = 0; i < n1; i++)
            {
                var sq_norm = TAcc.Zero;
                for (int j = 0; j < n3; j++)
                {
                    var val = TAcc.CreateTruncating(a[i, j]);
                    sq_norm += val * val;
                }

                scale[i] = TAcc.One / TAcc.Sqrt(sq_norm / TAcc.CreateTruncating(n3) + epsilon);
            }

            for (var i = 0; i < n1; i++)
            {
                for (var j = 0; j < n2; j++)
                {
                    var sum = TAcc.Zero;
                    for (var k = 0; k < n3; k++)
                    {
                        sum += TAcc.CreateTruncating(a[i, k]) * TAcc.CreateTruncating(b[k, j]);
                    }
                    c[i, j] = TCoeff.CreateTruncating(sum * scale[i] + TAcc.CreateTruncating(c[i, j]));
                }
            }
        }

        public static void RunTest<T, TAcc>(
            int n1,
            int n2,
            int n3,
            TensorLayout layoutC,
            TensorLayout layoutA,
            TensorLayout layoutB,
            string kernelName,
            T epsilon,
            Params? parameters = null,
            Action<Tensor<T>, Tensor<T>, Tensor<T>>? reference = null)
            where T : unmanaged, INumber<T>
            where TAcc : unmanaged, INumber<TAcc>
        {
            #region Prepare

            var rand = new Random(42);

            var manager = new TensorMemoryManager();
            var a = manager.Create<T>([n1, n2], layoutA);
            var b = manager.Create<T>([n2, n3], layoutB);
            var c_cpu = manager.Create<T>([n1, n3], layoutC, clear: true);
            var c_gpu = manager.Create<T>([n1, n3], layoutC, clear: true);

            FillRandom(rand, a);
            FillRandom(rand, b);

            #endregion

            #region Actual

            parameters ??= new Params([16], [16]);
            GpuImplementation(c_gpu, a, b, kernelName, parameters);

            #endregion

            #region Expected

            reference ??= ReferenceImplementation<T, TAcc>;
            reference(c_cpu, a, b);

            #endregion

            #region Condition

            for (var i = 0; i < n1; i++)
            {
                for (var j = 0; j < n3; j++)
                {
                    Assert.That(T.Abs(c_gpu[i, j] - c_cpu[i, j]) <= epsilon, $"Mismatch at i={i}, j={j}, expected={c_cpu[i, j]}, actual={c_gpu[i, j]}");
                }
            }

            #endregion

            manager.Dispose();
        }

        private static unsafe void GpuImplementation<T>(
            Tensor<T> y,
            Tensor<T> x,
            Tensor<T> w,
            string kernelName,
            Params parameters)
            where T : unmanaged, INumber<T>
        {
            // 1. Platform/Device discovery with our clean API
            var platforms = Platform.GetPlatforms();
            var platform = platforms[0];
            var devices = platform.GetDevices(DeviceType.Gpu);
            var device = devices[0];

            // 2. Context creation with proper configuration
            var context = platform.CreateContext([device]);

            // 3. Command queue with profiling for performance tuning
            var queue = context.CreateCommandQueue(device);

            // 4. Load and build kernel from embedded resource
            var kernelSource = GetResource($"Foundation.OpenCL.Tests.Kernels.{kernelName}.cl");
            using var program = context.CreateWithSource(kernelSource);

            try
            {
                program.Build([device], "-cl-std=CL3.0", () =>
                {
                    Console.WriteLine($"Program built successfully for device: {device.GetStringInfo(DeviceInfo.Name)}");
                });
            }
            catch
            {
                var log = program.GetBuildLog(device);
                Console.WriteLine(log);
                throw;
            }

            // 5. Create buffers with proper memory flags
            var yBuffer = context.CreateBuffer(MemFlags.ReadWrite | MemFlags.CopyHostPtr, y.ByteSize, y.Rep);
            var xBuffer = context.CreateBuffer(MemFlags.ReadOnly | MemFlags.CopyHostPtr, x.ByteSize, x.Rep);
            var wBuffer = context.CreateBuffer(MemFlags.ReadOnly | MemFlags.CopyHostPtr, w.ByteSize, w.Rep);

            // 6. Create kernel with type-safe argument setting
            var kernel = program.CreateKernel(kernelName);

            // 7. Set kernel arguments with compile-time type safety
            kernel.SetArgBuffer(0, yBuffer);
            kernel.SetArgBuffer(1, xBuffer);
            kernel.SetArgBuffer(2, wBuffer);

            if (parameters.SetArguments is not null) parameters.SetArguments(kernel);

            // 8. Configure execution dimensions
            var globalOffset = new nuint[parameters.GlobalSize.Length];
            var globalSize = parameters.GlobalSize;
            var localSize = parameters.LocalSize;

            // 9. Execute kernel with proper event synchronization
            var sw = Stopwatch.StartNew();

            using var kernelEvent = queue.EnqueueNdRangeKernel(
                kernel, globalOffset, globalSize, localSize);

            kernelEvent.Wait(); // Blocks until kernel completion

            Console.WriteLine($"Kernel execution took: {sw.ElapsedMilliseconds} ms");

            // 10. Read back results with blocking call
            queue.EnqueueReadBufferBlocking(yBuffer, 0, y.ByteSize, y.Rep);

            queue.Finish();
            queue.Dispose();

            yBuffer.Dispose();
            xBuffer.Dispose();
            wBuffer.Dispose();

            // 11. Implicit cleanup via IDisposable pattern
            kernel.Dispose();
            context.Dispose();
        }
    }
}

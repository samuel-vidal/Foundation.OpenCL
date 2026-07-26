using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Foundation.OpenCL.Tests.Tensors;

namespace Foundation.OpenCL.Tests
{
    [Category("Hardware")]
    public class RunKernelTest
    {
        private const int inputSize = 2048;
        private const int outputSize = 2048;

        private const string kernelName = "gemm_bf16_layout5";

        [TestCase(32)]
        [TestCase(4)]
        public void TestKernel(int batchSize)
        {
            Console.WriteLine($"Test {kernelName} : inputSize = {inputSize}, outputSize = {outputSize}, batchSize = {batchSize},");

            #region Prepare

            var rand = new Random(42);

            var manager = new TensorMemoryManager();
            var x = manager.Create<Half>([batchSize, inputSize], [inputSize, 1]);
            var w = manager.Create<Half>([outputSize, inputSize], [inputSize, 1]);
            var yExpected = manager.Create<Half>([batchSize, outputSize], [outputSize, 1], clear: true);
            var yActual = manager.Create<Half>([batchSize, outputSize], [outputSize, 1], clear: true);

            FillRandom(rand, x);
            FillRandom(rand, w);

            #endregion

            #region Actual

            GpuImplementation(yActual, x, w);

            #endregion

            #region Expected

            ReferenceImplementation(yExpected, x, w);

            #endregion

            #region Condition

            //var sb = new StringBuilder();

            //sb.AppendLine("y_actual :");
            //for (int i = 0; i < batchSize; i++)
            //{
            //    for (int j = 0; j < outputSize; j++)
            //    {
            //        sb.Append($"{y_actual[i, j],8}");
            //    }
            //    sb.AppendLine();
            //}

            //sb.AppendLine();
            //sb.AppendLine("y_expected :");
            //for (int i = 0; i < batchSize; i++)
            //{
            //    for (int j = 0; j < outputSize; j++)
            //    {
            //        sb.Append($"{y_expected[i, j],8}");
            //    }
            //    sb.AppendLine();
            //}

            //Console.WriteLine(sb);

            var epsilon = (Half)2.5e-2;

            for (var i = 0; i < batchSize; i++)
            {
                for (var j = 0; j < outputSize; j++)
                {
                    Assert.That(Half.Abs(yActual[i, j] - yExpected[i, j]) < epsilon, $"Mismatch at i={i}, j={j}, expected={yExpected[i, j]}, actual={yActual[i, j]}");
                }
            }

            #endregion

            manager.Dispose();
        }

        private static void FillRandom(Random rand, Tensor<Half> x)
        {
            for (var i = 0; i < x.Dimensions[0]; i++)
            {
                for (var j = 0; j < x.Dimensions[1]; j++)
                {
                    x[i, j] = (Half)(rand.NextSingle() - 0.5f);
                }
            }
        }

        private static void ReferenceImplementation(
            Tensor<Half> y,
            Tensor<Half> x,
            Tensor<Half> w)
        {
            // Y[ i, j ] = Y[ i, j ] +  Sum_k X[ i, k ] W[ j, k ]

            for (var i = 0; i < y.Dimensions[0]; i++)
            {
                for (var j = 0; j < y.Dimensions[1]; j++)
                {
                    var sum = 0.0;
                    for (var k = 0; k < x.Dimensions[1]; k++)
                    {
                        sum += (double)(x[i, k] * w[j, k]);
                    }

                    y[i, j] = (Half)(sum + (double)y[i, j]);
                }
            }
        }

        public static string GetResource(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(path)!;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private static unsafe void GpuImplementation(
            Tensor<Half> y,
            Tensor<Half> x,
            Tensor<Half> w)
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
            var kernelSource = GetResource("Foundation.OpenCL.Tests.Kernels.gemm_bf16_layout5.cl");
            using var program = context.CreateWithSource(kernelSource);

            program.Build([device], "-cl-std=CL3.0", () =>
            {
                Console.WriteLine($"Program built successfully for device: {device.GetStringInfo(DeviceInfo.Name)}");
            });

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
            kernel.SetArg(3, y.Strides[0]);     // strideY
            kernel.SetArg(4, x.Strides[0]);     // strideX  
            kernel.SetArg(5, w.Strides[0]);     // strideW
            kernel.SetArg(6, y.Dimensions[0]);  // batch size

            // 8. Configure execution dimensions
            var warpCount = (nuint)(outputSize / 128);
            var globalOffset = new nuint[] { 0, 0 };
            var globalSize = new nuint[] { warpCount, 32 };
            var localSize = new nuint[] { warpCount, 32 };

            // 9. Execute kernel with proper event synchronization
            var sw = Stopwatch.StartNew();

            using var kernelEvent = queue.EnqueueNdRangeKernelEvent(
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

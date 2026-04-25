using Foundation.OpenCL;
using Foundation.OpenCL.Tests;
using Foundation.OpenCL.Tests.Tensors;
using System.Diagnostics;
using System.Numerics;
using static Foundation.OpenCL.Tests.TestUtils;
using Buffer = Foundation.OpenCL.Buffer;

namespace Shell
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var test = new IntelMxmIntrinsicsTests();

            for (int i = 1; i <= 32; i *= 2)
            {
                for (int j = 0; j < 10; j++)
                {
                    test.Test_gemm_kiss(32);
                }
            }


            // This small exe can be used with VTune profiler



            //Benchmark(
            //    2048,
            //    1,
            //    200);
        }

        public static void MicroBenchmark()
        {
            var test = new IntelMxmIntrinsicsTests();

            test.Test_1_2048_2048_fused_layer_norm_shuffled();

        }

        public static void Benchmark(
            int hiddenDim,
            int batchSize,
            int chainLength = 10,
            bool prefetch = false,
            TensorLayout layoutA = TensorLayout.LastIndexContiguous,
            TensorLayout layoutB = TensorLayout.FirstIndexContiguous)
        {
            #region Prepare

            var rand = new Random(42);

            var manager = new TensorMemoryManager();

            var activations = new Tensor<Half>[chainLength + 1];
            for (var i = 0; i <= chainLength; i++)
            {
                activations[i] = manager.Create<Half>([batchSize, hiddenDim], layoutA);
            }
            
            FillRandom(rand, activations[0]);

            var weights = new Tensor<Half>[chainLength];
            for (var i = 0; i < chainLength; i++)
            {
                weights[i] = manager.Create<Half>([hiddenDim, hiddenDim], layoutB);
                FillRandom(rand, weights[i]);
            }

            #endregion

            const string kernelName = "gemm_1_2048_16n_fused_layer_norm";

            GpuImplementation(activations, weights, kernelName, new Params([128 * 256], [256]));
        }

        private static unsafe void GpuImplementation<T>(
            Tensor<T>[] activations,
            Tensor<T>[] weights,
            string kernelName,
            Params parameters)
            where T : unmanaged, INumber<T>
        {
            // 1. Platform/Device discovery with our clean API
            var platforms = Platform.GetPlatforms();
            var platform = platforms[0];
            var devices = platform.GetDevices(DeviceType.Gpu);
            var device = devices[0];

            var deviceName = device.GetStringInfo(DeviceInfo.Name);
            var deviceType = device.GetInfo<DeviceType>(DeviceInfo.Type);
            Console.WriteLine($"    - {deviceName} ({deviceType})");

            // 2. Context creation with proper configuration
            var context = platform.CreateContext([device]);

            // 3. Command queue with profiling for performance tuning
            //var queue = context.CreateCommandQueue(
            //    device,
            //    CommandQueueProperty.OutOfOrderExecModeEnable,
            //    CommandQueueProperty.OnDevice);
            var queue = context.CreateCommandQueue(
                device);

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

            var activationBuffers = new Buffer[activations.Length];
            var weightBuffers = new Buffer[weights.Length];

            activationBuffers[0] = context.CreateBuffer(MemFlags.ReadOnly | MemFlags.CopyHostPtr | MemFlags.HostNoAccess, activations[0].ByteSize, activations[0].Rep);
            activationBuffers[^1] = context.CreateBuffer(MemFlags.ReadOnly | MemFlags.HostReadOnly, activations[^1].ByteSize);

            for (var i = 1; i < weights.Length; i++)
            {
                activationBuffers[i] = context.CreateBuffer(MemFlags.ReadWrite | MemFlags.HostNoAccess, activations[i].ByteSize);
            }

            for (var i = 0; i < weights.Length; i++)
            {
                weightBuffers[i] = context.CreateBuffer(MemFlags.ReadOnly | MemFlags.CopyHostPtr | MemFlags.HostNoAccess, weights[i].ByteSize, weights[i].Rep);
            }

            // 6. Create kernel with type-safe argument setting

            // 7. Set kernel arguments with compile-time type safety

            // 8. Configure execution dimensions
            var globalOffset = new nuint[parameters.GlobalSize.Length];
            var globalSize = parameters.GlobalSize;
            var localSize = parameters.LocalSize;

            // 9. Execute kernel with proper event synchronization

            var eventList = new List<Event>();
            var startEvent = context.CreateUserEvent();
            eventList.Add(startEvent);
            var lastEvent = startEvent;
            
            var kernel = program.CreateKernel(kernelName);
            if (parameters.SetArguments is not null) parameters.SetArguments(kernel);

            for (var i = 0; i < weights.Length; i++)
            {
                kernel.SetArgBuffer(0, activationBuffers[i + 1]);
                kernel.SetArgBuffer(1, activationBuffers[i]);
                kernel.SetArgBuffer(2, weightBuffers[i]);

                lastEvent = queue.EnqueueNdRangeKernel(
                    kernel, globalOffset, globalSize, localSize, [lastEvent]);

                eventList.Add(lastEvent);
            }

            queue.Flush();

            var sw = Stopwatch.StartNew();

            startEvent.SetEventStatus(CommandExecutionStatus.Complete);

            lastEvent.Wait(); // Blocks until kernel completion

            Console.WriteLine($"Kernel execution took: {sw.ElapsedMilliseconds} ms");

            foreach (var evt in eventList) evt.Dispose();
            //foreach (var kernel in kernelList) kernel.Dispose();
            kernel.Dispose();

            // 10. Read back results with blocking call
            queue.EnqueueReadBufferBlocking(activationBuffers[^1], 0, activations[^1].ByteSize, activations[^1].Rep);

            queue.Finish();
            queue.Dispose();

            foreach (var buffer in weightBuffers) buffer.Dispose();
            foreach (var buffer in activationBuffers) buffer.Dispose();

            // 11. Implicit cleanup via IDisposable pattern
            context.Dispose();
        }
    }
}

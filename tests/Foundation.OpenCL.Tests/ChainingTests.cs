using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Foundation.OpenCL.Tests.Tensors;
using static System.Single;

namespace Foundation.OpenCL.Tests
{
    [Category("Hardware")]
    public unsafe class ChainingTests
    {
        [TestCase(2, 5e-4f)]
        [TestCase(8, 1e-3f)]
        [TestCase(100, 1e-2f)]
        public void Strategy1(int iterations, float tolerance)
        {
            var manager = new TensorMemoryManager();
            var rand = new Random(42);

            var a = manager.Create<Half>([32, 32]);
            var b = manager.Create<Half>([32, 32]);
            var p = manager.Create<Half>([32, 32]);
            var q = manager.Create<Half>([32, 32]);
            var actual = manager.Create<Half>([32, 32]);

            RandomOrthogonal(rand, a);
            RandomOrthogonal(rand, p);
            RandomOrthogonal(rand, q);

            var platforms = Platform.GetPlatforms();
            var devices = platforms[0].GetDevices(DeviceType.Gpu);
            var device = devices[0];
            var context = Context.CreateContext([device]);
            var queue = context.CreateCommandQueue(
                device,
                CommandQueueProperty.OutOfOrderExecModeEnable,
                CommandQueueProperty.OnDevice);

            var a_buffer = context.CreateBuffer(MemFlags.ReadWrite | MemFlags.CopyHostPtr, a.ByteSize, a.Rep);
            var b_buffer = context.CreateBuffer(MemFlags.ReadWrite, b.ByteSize);
            var p_buffer = context.CreateBuffer(MemFlags.ReadOnly | MemFlags.CopyHostPtr, p.ByteSize, p.Rep);
            var q_buffer = context.CreateBuffer(MemFlags.ReadOnly | MemFlags.CopyHostPtr, q.ByteSize, q.Rep);

            var kernelSource = GetResource("Foundation.OpenCL.Tests.Kernels.basic_gemm_square.cl");
            using var program = context.CreateWithSource(kernelSource);

            try
            {
                program.Build(
                    [device],
                    "-cl-std=CL3.0",
                    () =>
                    {
                        Console.WriteLine(
                            $"Program built successfully for device: {device.GetStringInfo(DeviceInfo.Name)}");
                    });
            }
            catch
            {
                var log = program.GetBuildLog(device);
                Console.WriteLine(log);
                throw;
            }

            var kernel0 = program.CreateKernel("basic_gemm_square");
            kernel0.SetArgBuffer(0, b_buffer);
            kernel0.SetArgBuffer(1, a_buffer);
            kernel0.SetArgBuffer(2, q_buffer);

            var kernel1 = program.CreateKernel("basic_gemm_square");
            kernel1.SetArgBuffer(0, a_buffer);
            kernel1.SetArgBuffer(1, b_buffer);
            kernel1.SetArgBuffer(2, p_buffer);

            var eventList = new List<Event>();

            var completion = queue.EnqueueNdRangeKernel(kernel0, [0], [32], [32]);
            eventList.Add(completion);
            completion = queue.EnqueueNdRangeKernel(kernel1, [0], [32], [32], [completion]);
            eventList.Add(completion);

            Event.Wait(completion);
            var sw = Stopwatch.StartNew();

            for (var i = 2; i < iterations; i+=2)
            {
                completion = queue.EnqueueNdRangeKernel(kernel0, [0], [32], [32], [completion]);
                eventList.Add(completion);
                completion = queue.EnqueueNdRangeKernel(kernel1, [0], [32], [32], [completion]);
                eventList.Add(completion);
            }

            queue.EnqueueReadBufferBlocking(a_buffer, 0, actual.ByteSize, actual.Rep, [completion]);

            Console.WriteLine($"took {sw.Elapsed}");

            queue.Flush();
            queue.Finish();

            foreach (var ev in eventList) ev.Dispose();

            kernel0.Dispose();
            kernel1.Dispose();

            a_buffer.Dispose();
            b_buffer.Dispose();
            p_buffer.Dispose();
            q_buffer.Dispose();
            queue.Dispose();
            context.Dispose();

            Reference(a, b, p, q, iterations);

            for (var i = 0; i < 32; i++)
            {
                for (var j = 0; j < 32; j++)
                {
                    Assert.That(Abs((float)actual[i, j] - (float)a[i, j]) < tolerance, $"expected[{i},{j}] = {a[i, j]}, actual[{i},{j}] = {actual[i, j]}");
                }
            }

            manager.Dispose();
        }

        [TestCase(2, 5e-4f)]
        [TestCase(8, 1e-3f)]
        [TestCase(100, 1e-2f)]
        public void Strategy2(int iterations, float tolerance)
        {
            var manager = new TensorMemoryManager();
            var rand = new Random(42);

            var a = manager.Create<Half>([32, 32]);
            var b = manager.Create<Half>([32, 32]);
            var p = manager.Create<Half>([32, 32]);
            var q = manager.Create<Half>([32, 32]);
            var result_actual = manager.Create<Half>([32, 32]);

            RandomOrthogonal(rand, a);
            RandomOrthogonal(rand, p);
            RandomOrthogonal(rand, q);

            var platforms = Platform.GetPlatforms();
            var devices = platforms[0].GetDevices(DeviceType.Gpu);
            var device = devices[0];
            var context = Context.CreateContext([device]);
            var queue = context.CreateCommandQueue(
                device,
                CommandQueueProperty.OutOfOrderExecModeEnable);

            var a_buffer = context.CreateBuffer(MemFlags.ReadWrite | MemFlags.CopyHostPtr, a.ByteSize, a.Rep);
            var b_buffer = context.CreateBuffer(MemFlags.ReadWrite, b.ByteSize);
            var p_buffer = context.CreateBuffer(MemFlags.ReadOnly | MemFlags.CopyHostPtr, p.ByteSize, p.Rep);
            var q_buffer = context.CreateBuffer(MemFlags.ReadOnly | MemFlags.CopyHostPtr, q.ByteSize, q.Rep);

            var kernelSource = GetResource("Foundation.OpenCL.Tests.Kernels.basic_gemm_square.cl");
            using var program = context.CreateWithSource(kernelSource);

            try
            {
                program.Build(
                    [device],
                    "-cl-std=CL3.0",
                    () =>
                    {
                        Console.WriteLine(
                            $"Program built successfully for device: {device.GetStringInfo(DeviceInfo.Name)}");
                    });
            }
            catch
            {
                var log = program.GetBuildLog(device);
                Console.WriteLine(log);
                throw;
            }

            var kernel0 = program.CreateKernel("basic_gemm_square");
            kernel0.SetArgBuffer(0, b_buffer);
            kernel0.SetArgBuffer(1, a_buffer);
            kernel0.SetArgBuffer(2, q_buffer);

            var kernel1 = program.CreateKernel("basic_gemm_square");
            kernel1.SetArgBuffer(0, a_buffer);
            kernel1.SetArgBuffer(1, b_buffer);
            kernel1.SetArgBuffer(2, p_buffer);

            var eventList = new List<Event>();


            var start = context.CreateEvent();
            eventList.Add(start);

            var completion = queue.EnqueueNdRangeKernel(kernel0, [0], [32], [32], [start]);
            eventList.Add(completion);
            completion = queue.EnqueueNdRangeKernel(kernel1, [0], [32], [32], [completion]);
            eventList.Add(completion);

            for (var i = 2; i < iterations; i+=2)
            {
                completion = queue.EnqueueNdRangeKernel(kernel0, [0], [32], [32], [completion]);
                eventList.Add(completion);
                completion = queue.EnqueueNdRangeKernel(kernel1, [0], [32], [32], [completion]);
                eventList.Add(completion);
            }


            queue.Flush();

            var sw = Stopwatch.StartNew();
            start.SetEvenStatus(CommandExecutionStatus.Complete);

            queue.EnqueueReadBufferBlocking(a_buffer, 0, result_actual.ByteSize, result_actual.Rep, [completion]);
            Console.WriteLine($"took {sw.Elapsed}");

            queue.Finish();

            foreach (var ev in eventList) ev.Dispose();

            kernel0.Dispose();
            kernel1.Dispose();

            a_buffer.Dispose();
            b_buffer.Dispose();
            p_buffer.Dispose();
            q_buffer.Dispose();
            queue.Dispose();
            context.Dispose();

            Reference(a, b, p, q, iterations);

            for (var i = 0; i < 32; i++)
            {
                for (var j = 0; j < 32; j++)
                {
                    Assert.That(Abs((float)result_actual[i, j] - (float)a[i, j]) < tolerance, $"expected[{i},{j}] = {a[i, j]}, actual[{i},{j}] = {result_actual[i, j]}");
                }
            }

            manager.Dispose();
        }

        [TestCase(2, 5e-4f)]
        [TestCase(8, 1e-3f)]
        [TestCase(100, 1e-2f)]
        public void Strategy3(int iterations, float tolerance)
        {
            var manager = new TensorMemoryManager();
            var rand = new Random(42);

            var a = manager.Create<Half>([32, 32]);
            var b = manager.Create<Half>([32, 32]);
            var p = manager.Create<Half>([32, 32]);
            var q = manager.Create<Half>([32, 32]);
            var result_actual = manager.Create<Half>([32, 32]);

            RandomOrthogonal(rand, a);
            RandomOrthogonal(rand, p);
            RandomOrthogonal(rand, q);

            var platforms = Platform.GetPlatforms();
            var devices = platforms[0].GetDevices(DeviceType.Gpu);
            var device = devices[0];
            var context = Context.CreateContext([device]);
            var queue = context.CreateCommandQueue(device);

            var a_buffer = context.CreateBuffer(MemFlags.ReadWrite | MemFlags.CopyHostPtr, a.ByteSize, a.Rep);
            var b_buffer = context.CreateBuffer(MemFlags.ReadWrite, b.ByteSize);
            var p_buffer = context.CreateBuffer(MemFlags.ReadOnly | MemFlags.CopyHostPtr, p.ByteSize, p.Rep);
            var q_buffer = context.CreateBuffer(MemFlags.ReadOnly | MemFlags.CopyHostPtr, q.ByteSize, q.Rep);

            var kernelSource = GetResource("Foundation.OpenCL.Tests.Kernels.basic_gemm_square.cl");
            using var program = context.CreateWithSource(kernelSource);

            try
            {
                program.Build(
                    [device],
                    "-cl-std=CL3.0",
                    () =>
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

            var kernel0 = program.CreateKernel("basic_gemm_square");
            kernel0.SetArgBuffer(0, b_buffer);
            kernel0.SetArgBuffer(1, a_buffer);
            kernel0.SetArgBuffer(2, q_buffer);

            var kernel1 = program.CreateKernel("basic_gemm_square");
            kernel1.SetArgBuffer(0, a_buffer);
            kernel1.SetArgBuffer(1, b_buffer);
            kernel1.SetArgBuffer(2, p_buffer);

            var eventList = new List<Event>();


            eventList.Add(queue.EnqueueNdRangeKernel(kernel0, [0], [32], [32]));
            eventList.Add(queue.EnqueueNdRangeKernel(kernel1, [0], [32], [32]));

            Event.Wait(eventList[^1]);
            var sw = Stopwatch.StartNew();

            for (var i = 2; i < iterations; i += 2)
            {
                eventList.Add(queue.EnqueueNdRangeKernel(kernel0, [0], [32], [32]));
                eventList.Add(queue.EnqueueNdRangeKernel(kernel1, [0], [32], [32]));
            }

            queue.EnqueueReadBufferBlocking(a_buffer, 0, result_actual.ByteSize, result_actual.Rep);

            Console.WriteLine($"took {sw.Elapsed}");

            queue.Flush();
            queue.Finish();

            foreach (var ev in eventList) ev.Dispose();

            kernel0.Dispose();
            kernel1.Dispose();

            a_buffer.Dispose();
            b_buffer.Dispose();
            p_buffer.Dispose();
            q_buffer.Dispose();
            queue.Dispose();
            context.Dispose();

            Reference(a, b, p, q, iterations);

            for (var i = 0; i < 32; i++)
            {
                for (var j = 0; j < 32; j++)
                {
                    Assert.That(Abs((float)result_actual[i, j] - (float)a[i, j]) < tolerance, $"expected[{i},{j}] = {a[i, j]}, actual[{i},{j}] = {result_actual[i, j]}");
                }
            }

            manager.Dispose();
        }

        private static void Reference<T>(
            Tensor<T> a,
            Tensor<T> b,
            Tensor<T> p,
            Tensor<T> q,
            int iterations)
            where T : unmanaged, INumber<T>
        {
            for (var i = 0; i < iterations; i+=2)
            {
                Mult(b, a, q);
                Mult(a, b, p);
            }
        }

        private static string GetResource(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(path)!;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private static void RandomOrthogonal<T>(Random rand, Tensor<T> q)
            where T : unmanaged, INumber<T>
        {
            Assert.That(q.Rank, Is.EqualTo(2));
            Assert.That(q.Dimensions[1], Is.EqualTo(q.Dimensions[0]));

            var span = q.AsSpan();
            for (var i = 0; i < span.Length; i++) span[i] = T.CreateTruncating(rand.NextSingle() - 0.5f);

            OrthogonalizeRows(q);

            for (var i = 0; i < q.Dimensions[0]; i++)
            {
                Assert.That(Abs(Dot(Row(q, i), Row(q, i))- 1f) < 1e-3f);
                for (var j = i+1; j < q.Dimensions[0]; j++)
                {
                    Assert.That(Abs(Dot(Row(q, i), Row(q, j))) < 1e-1f,$"dot({i},{j}) = {Dot(Row(q, i), Row(q, j))}");
                }
            }
        }

        private static void OrthogonalizeRows<T>(Tensor<T> q)
            where T : unmanaged, INumber<T>
        {
            Normalize(Row(q, 0));
            for (var i = 1; i < q.Dimensions[0]; i++)
            {
                for (var j = 0; j < i; j++)
                {
                    Ortho(Row(q, j), Row(q, i));
                }
            }
        }

        // Computes v = unit(v - < u, v > u)
        // u is assumed to be a unit vector
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Ortho<T>(Span<T> u, Span<T> v)
            where T : unmanaged, INumber<T>
        {
            var coeff = Dot(u, v);

            Span<float> fv = stackalloc float[v.Length];

            for (var i = 0; i < v.Length; i++) fv[i] = CreateTruncating(v[i]);

            var sum = 0f;
            for (var i = 0; i < v.Length; i++)
            {
                var val = fv[i] - coeff * CreateTruncating(u[i]);
                sum += val * val;
                fv[i] = val;
            }

            var norm = Sqrt(sum);
            var rnorm = 1f / norm;

            Mult(fv, rnorm);

            for (var i = 0; i < v.Length; i++) v[i] = T.CreateTruncating(fv[i]);
            return norm;
        }

        private static float Normalize<T>(Span<T> row)
            where T : unmanaged, INumber<T>
        {
            var sqNorm = Dot(row, row);

            var norm = Sqrt(sqNorm);
            var rnorm = 1f / norm;

            Mult(row, rnorm);

            return norm;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Mult<T>(Span<T> row, float a)
            where T : unmanaged, INumber<T>
        {
            for (var i = 0; i < row.Length; i++) row[i] = T.CreateTruncating(CreateTruncating(row[i]) * a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Dot<T>(Span<T> x, Span<T> y)
            where T : unmanaged, INumber<T>
        {
            var dot = 0f;
            for (var i = 0; i < x.Length; i++)
            {
                dot += CreateTruncating(x[i]) * CreateTruncating(y[i]);
            }
            return dot;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> Row<T>(Tensor<T> t, int i)
            where T : unmanaged
            => new(t.Rep + i * t.Strides[0], t.Dimensions[1]);

        // computes C = A B^T (accumulation in float)
        private static void Mult<T>(Tensor<T> c, Tensor<T> a, Tensor<T> b)
            where T : unmanaged, INumber<T>
        {
            var m = c.Dimensions[0];
            var n = c.Dimensions[1];
            var o = a.Dimensions[1];

            Assert.That(a.Dimensions[0], Is.EqualTo(m));
            Assert.That(b.Dimensions[0], Is.EqualTo(n));
            Assert.That(b.Dimensions[1], Is.EqualTo(o));

            for (var i = 0; i < m; i++)
            {
                var ai = Row(a, i);
                for (var j = 0; j < n; j++)
                {
                    var bj = Row(b, j);
                    c[i, j] = T.CreateTruncating(Dot(ai, bj));
                }
            }
        }
    }
}

using Foundation.OpenCL.Tests.Tensors;
using System;

namespace Foundation.OpenCL.Tests
{
    [Category("Hardware")]
    public class IntelMxmIntrinsicsTests
    {
        [Test]
        public void Test_8_16_16_row_maj()
        {
            const int inputSize = 16;
            const int outputSize = 16;
            const int batchSize = 8;

            const string kernelName = "gemm_8_16_16";

            Console.WriteLine(
                $"Test {kernelName} : inputSize = {inputSize}, outputSize = {outputSize}, batchSize = {batchSize},");

            TestUtils.RunTest<Half, float>(
                batchSize, inputSize, outputSize,
                TensorLayout.LastIndexContiguous,
                TensorLayout.LastIndexContiguous,
                TensorLayout.FirstIndexContiguous,
                kernelName, (Half)0);
        }

        [Test]
        public void Test_8_32_32_row_maj()
        {
            const int inputSize = 32;
            const int outputSize = 32;
            const int batchSize = 8;

            const string kernelName = "gemm_8_32_32";

            Console.WriteLine(
                $"Test {kernelName} : inputSize = {inputSize}, outputSize = {outputSize}, batchSize = {batchSize},");

            TestUtils.RunTest<Half, float>(
                batchSize, inputSize, outputSize,
                TensorLayout.LastIndexContiguous,
                TensorLayout.LastIndexContiguous,
                TensorLayout.FirstIndexContiguous,
                kernelName, (Half)0);
        }

        [Test]
        public void Test_1_2048_16_row_maj()
        {
            const int batchSize = 1;
            const int inputSize = 2048;
            const int outputSize = 16;

            const string kernelName = "gemm_1_2048_16";

            Console.WriteLine(
                $"Test {kernelName} : inputSize = {inputSize}, outputSize = {outputSize}, batchSize = {batchSize},");

            TestUtils.RunTest<Half, float>(
                batchSize, inputSize, outputSize,
                TensorLayout.LastIndexContiguous,
                TensorLayout.LastIndexContiguous,
                TensorLayout.FirstIndexContiguous,
                kernelName, (Half)0,
                new TestUtils.Params([16, 16], [16, 16]));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(128)]
        [TestCase(1024)]
        public void Test_1_2048_16n_row_maj(int n)
        {
            const int batchSize = 1;
            const int inputSize = 2048;
            int outputSize = 16*n;

            const string kernelName = "gemm_1_2048_16n";

            Console.WriteLine(
                $"Test {kernelName} : inputSize = {inputSize}, outputSize = {outputSize}, batchSize = {batchSize},");

            TestUtils.RunTest<Half, float>(
                batchSize, inputSize, outputSize,
                TensorLayout.LastIndexContiguous,
                TensorLayout.LastIndexContiguous,
                TensorLayout.FirstIndexContiguous,
                kernelName, (Half)1e-2,
                new TestUtils.Params([(nuint)(n * 256)], [256]));
        }

        [Test]
        public void Test_8_32_16_row_maj()
        {
            const int inputSize = 32;
            const int outputSize = 16;
            const int batchSize = 8;

            const string kernelName = "gemm_8_32_16";

            Console.WriteLine(
                $"Test {kernelName} : inputSize = {inputSize}, outputSize = {outputSize}, batchSize = {batchSize},");

            TestUtils.RunTest<Half, float>(
                batchSize, inputSize, outputSize,
                TensorLayout.LastIndexContiguous,
                TensorLayout.LastIndexContiguous,
                TensorLayout.FirstIndexContiguous,
                kernelName, (Half)0);
        }

        [Test]
        public void Test_8_2048_16_row_maj()
        {
            const int inputSize = 2048;
            const int outputSize = 16;
            const int batchSize = 8;

            const string kernelName = "gemm_8_2048_16";

            Console.WriteLine(
                $"Test {kernelName} : inputSize = {inputSize}, outputSize = {outputSize}, batchSize = {batchSize},");

            TestUtils.RunTest<Half, float>(
                batchSize, inputSize, outputSize,
                TensorLayout.LastIndexContiguous,
                TensorLayout.LastIndexContiguous,
                TensorLayout.FirstIndexContiguous,
                kernelName, (Half)0);
        }

        [Test]
        public void Test_8_16_16_col_maj()
        {
            const int inputSize = 16;
            const int outputSize = 8;
            const int batchSize = 16;

            const string kernelName = "gemm_8_16_16_col_maj";

            Console.WriteLine(
                $"Test {kernelName} : inputSize = {inputSize}, outputSize = {outputSize}, batchSize = {batchSize},");

            TestUtils.RunTest<Half, float>(
                outputSize, inputSize, batchSize, 
                TensorLayout.FirstIndexContiguous,
                TensorLayout.FirstIndexContiguous,
                TensorLayout.FirstIndexContiguous,
                kernelName, (Half)0);
        }
    }
}
